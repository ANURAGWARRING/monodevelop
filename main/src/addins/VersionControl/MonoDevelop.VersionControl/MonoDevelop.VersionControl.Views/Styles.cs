﻿//
// Styles.cs
//
// Author:
//       Vsevolod Kukol <sevo@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://www.xamarin.com)
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
using MonoDevelop.Ide;
using MonoDevelop.Components;
using Xwt.Drawing;

namespace MonoDevelop.VersionControl
{
	public static class Styles
	{
		public static BlameViewStyle BlameView { get; internal set; }
		public static LogViewStyle LogView { get; internal set; }
		public static DiffViewStyle DiffView { get; internal set; }

		public class BlameViewStyle
		{
			public Color AnnotationBackgroundColor { get; internal set; }
			public Color AnnotationHighlightColor { get; internal set; }
			public Color AnnotationTextColor { get; internal set; }
			public Color AnnotationSummaryTextColor { get; internal set; }
			public Color AnnotationSplitterColor { get; internal set; }
			public Color AnnotationMarkColor { get; internal set; }
			public Color AnnotationMarkModifiedColor { get; internal set; }
			public Color RangeSplitterColor { get; internal set; }
			public Color RangeHazeColor { get; internal set; }
		}

		public class LogViewStyle
		{
			public Color CommitDescBackgroundColor { get; internal set; }
			public Color DiffAddBackgroundColor { get; internal set; }
			public Color DiffRemoveBackgroundColor { get; internal set; }
			public Color DiffHighlightColor { get; internal set; }
			public Color DiffBoxBorderColor { get; internal set; }
			public Color DiffBoxSplitterColor { get; internal set; }
		}

		public class DiffViewStyle
		{
			public Color AddBorderColor { get; internal set; }
			public Color AddBackgroundColor { get; internal set; }
			public Color RemoveBorderColor { get; internal set; }
			public Color RemoveBackgroundColor { get; internal set; }
			public Color MergeBorderColor { get; internal set; }
			public Color MergeBackgroundColor { get; internal set; }
		}

		static Styles ()
		{
			LoadStyles ();
			MonoDevelop.Ide.Gui.Styles.Changed +=  (o, e) => LoadStyles ();
		}

		public static void LoadStyles ()
		{
			if (IdeApp.Preferences.UserInterfaceSkin == Skin.Light) {
				BlameView = new BlameViewStyle {
					AnnotationMarkColor = Color.FromName ("#e5f1ff"),
					AnnotationMarkModifiedColor = Color.FromName ("#ffff00"),
					RangeHazeColor = Color.FromName ("#ababab").WithAlpha (.1),
					RangeSplitterColor = Color.FromName ("#ababab").WithAlpha (.2),
				};

				LogView = new LogViewStyle {
					DiffAddBackgroundColor = Color.FromName ("#7bc87b").AddLight (0.1),
					DiffRemoveBackgroundColor = Color.FromName ("#c88c8c").AddLight (0.1),
					DiffHighlightColor = Color.FromName ("#000000").WithAlpha (0.05),
					DiffBoxBorderColor = Color.FromName ("#eaeaea"),
				};

				DiffView = new DiffViewStyle {
					AddBackgroundColor = Color.FromName ("#85a885"),
					AddBorderColor = Color.FromName ("#85a885"),
					RemoveBackgroundColor = Color.FromName ("#b28c8c"),
					RemoveBorderColor = Color.FromName ("#b28c8c"),
					MergeBackgroundColor = Color.FromName ("#8585a8"),
					MergeBorderColor = Color.FromName ("#8585a8"),
				};
			} else {
				BlameView = new BlameViewStyle {
					AnnotationMarkColor = Color.FromName ("#e5e5ff"), // TODO
					AnnotationMarkModifiedColor = Color.FromName ("#ffff00"), // TODO
					RangeHazeColor = Color.FromName ("#3d3d3d").WithAlpha (.6), // TODO
					RangeSplitterColor = Color.FromName ("#3d3d3d").WithAlpha (.7), // TODO
				};

				LogView = new LogViewStyle () {
					DiffAddBackgroundColor = Color.FromName ("#7bc87b").AddLight (-0.1), // TODO
					DiffRemoveBackgroundColor = Color.FromName ("#c88c8c").AddLight (-0.1), // TODO
					DiffHighlightColor = MonoDevelop.Ide.Gui.Styles.BackgroundColor.AddLight (0.1), // TODO
					DiffBoxBorderColor = Color.FromName ("#4c4c4c"), // TODO
				};

				DiffView = new DiffViewStyle {
					AddBorderColor = Color.FromName ("#bef0be"), // TODO
					AddBackgroundColor = Color.FromName ("#85a885"), // TODO
					RemoveBorderColor = Color.FromName ("#ffffc8"), // TODO
					RemoveBackgroundColor = Color.FromName ("#b28c8c"), // TODO
					MergeBorderColor = Color.FromName ("#bebef0"), // TODO
					MergeBackgroundColor = Color.FromName ("#8585a8"), // TODO
				};
			}

			// Shared

			BlameView.AnnotationTextColor = MonoDevelop.Ide.Gui.Styles.BaseForegroundColor;
			BlameView.AnnotationHighlightColor = MonoDevelop.Ide.Gui.Styles.SecondaryBackgroundLighterColor;
			BlameView.AnnotationBackgroundColor = MonoDevelop.Ide.Gui.Styles.SecondaryBackgroundDarkerColor;
			BlameView.AnnotationSplitterColor = MonoDevelop.Ide.Gui.Styles.SeparatorColor;
			BlameView.AnnotationSummaryTextColor = MonoDevelop.Ide.Gui.Styles.DimTextColor;

			LogView.CommitDescBackgroundColor = MonoDevelop.Ide.Gui.Styles.SecondaryBackgroundDarkerColor;
			LogView.DiffBoxSplitterColor = MonoDevelop.Ide.Gui.Styles.PrimaryBackgroundColor;
		}
	}
}
