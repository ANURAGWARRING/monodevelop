//
// MicrosoftTemplateEngineSolutionTemplate.cs
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
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Edge.Settings;
using System.Linq;

namespace MonoDevelop.Ide.Templates
{
	public class MicrosoftTemplateEngineSolutionTemplate : SolutionTemplate
	{
		internal readonly ITemplateInfo template;

		static string GetIconId (ITemplateInfo template)
		{
			string iconId;
			if (template.Tags.TryGetValue ("IconId", out iconId))
			   return iconId;
			return string.Empty;
		}

		internal MicrosoftTemplateEngineSolutionTemplate (ITemplateInfo template)
			: base (template.Identity, template.Name, GetIconId(template))
		{
			this.template = template;

			Description = template.Description;
			string category;
			if (template.Tags.TryGetValue ("Category", out category))
				Category = category;
			else
				Category = string.Empty;
			if (template.Tags.TryGetValue ("Language", out category))
				Language = category;
			else
				Language = string.Empty;
			GroupId = template.GroupIdentity;
			//TODO: Support all this params
			//Condition = template.Condition;
			//ProjectFileExtension = template.FileExtension;
			//Wizard = template.WizardPath;
			//SupportedParameters = template.SupportedParameters;
			//DefaultParameters = template.DefaultParameters;
			//ImageId = template.ImageId;
			//ImageFile = template.ImageFile;
			//Visibility = GetVisibility (template.Visibility);

			//HasProjects = (template.SolutionDescriptor.EntryDescriptors.Length > 0);
		}
	}
}
