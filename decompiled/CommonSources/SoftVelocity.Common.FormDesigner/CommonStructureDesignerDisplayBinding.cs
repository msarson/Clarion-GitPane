using System;
using System.IO;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.FormDesigner;

public abstract class CommonStructureDesignerDisplayBinding : FormsDesignerDisplayBinding
{
	protected abstract string[] FileExtensions { get; }

	public override bool ReattachWhenParserServiceIsReady => false;

	protected override bool _CanAttachTo(IViewContent viewContent)
	{
		if (viewContent is ITextEditorControlProvider)
		{
			string text = (viewContent.IsUntitled ? viewContent.UntitledName : viewContent.FileName);
			if (text == null)
			{
				return false;
			}
			string[] fileExtensions = FileExtensions;
			foreach (string text2 in fileExtensions)
			{
				if (text2.Equals(Path.GetExtension(text), StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}
}
