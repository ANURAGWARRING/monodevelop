﻿//
// MicrosoftTemplateEngineProjectTemplatingProvider.cs
//
// Author:
//       David Karlaš <david.karlas@xamarin.com>
//
// Copyright (c) 2017 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.Utils;
using MonoDevelop.Ide.Projects;
using MonoDevelop.Projects;
using System.Linq;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects;
using MonoDevelop.Core;
using Microsoft.TemplateEngine.Edge;
using System.IO;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Config;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Macros;
using System.Threading.Tasks;
using Mono.Addins;
using MonoDevelop.Ide.Codons;

namespace MonoDevelop.Ide.Templates
{
	public class MicrosoftTemplateEngineProjectTemplatingProvider : IProjectTemplatingProvider
	{
		public bool CanProcessTemplate (SolutionTemplate template)
		{
			return template is MicrosoftTemplateEngineSolutionTemplate;
		}

		static EngineEnvironmentSettings environmentSettings = new EngineEnvironmentSettings (new MyTemplateEngineHost (), (env) => new SettingsLoader (env));
		static TemplateCreator templateCreator = new TemplateCreator (environmentSettings);

		static bool dontUpdateCache = true;

		static MicrosoftTemplateEngineProjectTemplatingProvider ()
		{
			AddinManager.AddExtensionNodeHandler ("/MonoDevelop/Ide/TemplatesPaths", OnExtensionChanged);
			dontUpdateCache = false;
			UpdateCache ();
		}

		static List<TemplatesPathCodon> ScanPaths = new List<TemplatesPathCodon> ();

		static void UpdateCache ()
		{
			if (dontUpdateCache)//Avoid updating cache while scan paths are added during registration 
				return;
			var paths = new Paths (environmentSettings);

			//TODO: Uncomment this IF, but also add logic to invalidate/check if new templates were added from newly installed AddOns...
			//if (!paths.Exists (paths.User.BaseDir) || !paths.Exists (paths.User.FirstRunCookie)) {
				var _templateCache = new TemplateCache (environmentSettings);
				foreach (var template in ScanPaths) {
					_templateCache.Scan (template.ScanPath);
				}
				_templateCache.WriteTemplateCaches ();
				paths.WriteAllText (paths.User.FirstRunCookie, "");
			//}
		}

		
		static void OnExtensionChanged (object s, ExtensionNodeEventArgs args)
		{
			if (args.Change == ExtensionChange.Add) {
				var codon = (TemplatesPathCodon)args.ExtensionNode;
				try {
					ScanPaths.Add (codon);
				} catch (Exception e) {
					string extId = null, addinId = null;
					if (codon != null) {
						if (codon.HasId)
							extId = codon.Id;
						if (codon.Addin != null)
							addinId = codon.Addin.Id;
					}
					LoggingService.LogError ("Error loading template id {0} in addin {1}:\n{2}",
											 extId ?? "(null)", addinId ?? "(null)", e.ToString ());
				}
			} else {
				foreach (var pt in ScanPaths) {
					var codon = (TemplatesPathCodon)args.ExtensionNode;
					if (pt.Id == codon.Id) {
						ScanPaths.Remove (pt);
						break;
					}
				}
			}
			UpdateCache ();
		}

		public IEnumerable<SolutionTemplate> GetTemplates ()
		{
			return templateCreator.List (false, (t, s) => new MatchInfo ()).Select ((match) => new MicrosoftTemplateEngineSolutionTemplate (match.Info));
		}

		static MonoDevelop.Core.Instrumentation.Counter TemplateCounter = MonoDevelop.Core.Instrumentation.InstrumentationService.CreateCounter ("Template Instantiated", "Project Model", id: "Core.Template.Instantiated");

		public async Task<ProcessedTemplateResult> ProcessTemplate (SolutionTemplate template, NewProjectConfiguration config, SolutionFolder parentFolder)
		{
			var templateInfo = ((MicrosoftTemplateEngineSolutionTemplate)template).template;
			var workspaceItems = new List<IWorkspaceFileObject> ();
			var result = await templateCreator.InstantiateAsync (
				templateInfo,
				config.ProjectName,
				config.GetValidProjectName (),
				config.ProjectLocation,
				new Dictionary<string, string> (),
				true);
			if (result.ResultInfo.PrimaryOutputs.Any ()) {
				foreach (var res in result.ResultInfo.PrimaryOutputs)
					workspaceItems.Add (await MonoDevelop.Projects.Services.ProjectService.ReadSolutionItem (new Core.ProgressMonitor (), res.Path));
			} else {
				foreach (var path in Directory.GetFiles (config.SolutionLocation, "*.*proj", SearchOption.AllDirectories)) {
					if (path.EndsWith (".csproj", StringComparison.OrdinalIgnoreCase) || path.EndsWith (".fsproj", StringComparison.OrdinalIgnoreCase) || path.EndsWith (".vbproj", StringComparison.OrdinalIgnoreCase))
						workspaceItems.Add (await MonoDevelop.Projects.Services.ProjectService.ReadSolutionItem (new Core.ProgressMonitor (), path));
				}
			}

			var metadata = new Dictionary<string, string> ();
			metadata ["Id"] = templateInfo.Identity;
			metadata ["Name"] = templateInfo.Name;
			metadata ["Language"] = template.Language;
			metadata ["Platform"] = "Unknown";//TODO
			TemplateCounter.Inc (1, null, metadata);

			if (parentFolder == null) {
				var solution = new Solution ();
				solution.SetLocation (config.SolutionLocation, config.SolutionName);
				solution.AddConfiguration ("Debug", false);
				solution.AddConfiguration ("Release", false);
				foreach (var item in workspaceItems.Cast<SolutionFolderItem> ()) {
					solution.RootFolder.AddItem (item);
				}
				return new MicrosoftTemplateEngineProcessedTemplateResult (new [] { solution }, solution.FileName, config.ProjectLocation);
			} else {
				return new MicrosoftTemplateEngineProcessedTemplateResult (workspaceItems.ToArray(), parentFolder.ParentSolution.FileName, config.ProjectLocation);
			}
		}

		class MyTemplateEngineHost : DefaultTemplateEngineHost
		{
			public MyTemplateEngineHost () : base (BrandingService.ApplicationName, BuildInfo.CompatVersion, "en-US", new Dictionary<string, string> { { "dotnet-cli-version", "0" } }, new Dictionary<Guid, Func<Type>>
			   {
				{ new Guid("0C434DF7-E2CB-4DEE-B216-D7C58C8EB4B3"), () => typeof(RunnableProjectGenerator) },
				{ new Guid("3147965A-08E5-4523-B869-02C8E9A8AAA1"), () => typeof(BalancedNestingConfig) },
				{ new Guid("3E8BCBF0-D631-45BA-A12D-FBF1DE03AA38"), () => typeof(ConditionalConfig) },
				{ new Guid("A1E27A4B-9608-47F1-B3B8-F70DF62DC521"), () => typeof(FlagsConfig) },
				{ new Guid("3FAE1942-7257-4247-B44D-2DDE07CB4A4A"), () => typeof(IncludeConfig) },
				{ new Guid("3D33B3BF-F40E-43EB-A14D-F40516F880CD"), () => typeof(RegionConfig) },
				{ new Guid("62DB7F1F-A10E-46F0-953F-A28A03A81CD1"), () => typeof(ReplacementConfig) },
				{ new Guid("370996FE-2943-4AED-B2F6-EC03F0B75B4A"), () => typeof(ConstantMacro) },
				{ new Guid("BB625F71-6404-4550-98AF-B2E546F46C5F"), () => typeof(EvaluateMacro) },
				{ new Guid("10919008-4E13-4FA8-825C-3B4DA855578E"), () => typeof(GuidMacro) },
				{ new Guid("F2B423D7-3C23-4489-816A-41D8D2A98596"), () => typeof(NowMacro) },
				{ new Guid("011E8DC1-8544-4360-9B40-65FD916049B7"), () => typeof(RandomMacro) },
				{ new Guid("8A4D4937-E23F-426D-8398-3BDBD1873ADB"), () => typeof(RegexMacro) },
				{ new Guid("B57D64E0-9B4F-4ABE-9366-711170FD5294"), () => typeof(SwitchMacro) },
				{ new Guid("10919118-4E13-4FA9-825C-3B4DA855578E"), () => typeof(CaseChangeMacro) }
			}.ToList ())
			{
			}

			public override bool TryGetHostParamDefault (string paramName, out string value)
			{
				if (paramName == "HostIdentifier") {
					value = this.HostIdentifier;
					return true;
				}
				value = null;
				return false;
			}
		}
	}
}
