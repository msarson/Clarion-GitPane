using System;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.Editor;

public class GenTextNavigationPoint : TextNavigationPoint
{
	public GenTextNavigationPoint()
	{
	}

	public GenTextNavigationPoint(string fileName)
		: base(fileName)
	{
	}

	public GenTextNavigationPoint(string fileName, int lineNumber, int column)
		: base(fileName, lineNumber, column)
	{
	}

	public GenTextNavigationPoint(string fileName, int lineNumber, int column, string content)
		: base(fileName, lineNumber, column, content)
	{
	}

	public override void JumpTo()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(((DefaultNavigationPoint)this).FileName))
		{
			return;
		}
		IWorkbenchWindow openFile = FileService.GetOpenFile(((DefaultNavigationPoint)this).FileName);
		if (openFile != null)
		{
			openFile.SelectWindow();
			IBaseViewContent activeViewContent = openFile.ActiveViewContent;
			if (activeViewContent is CommonGenEditor)
			{
				((IPositionable)activeViewContent).JumpTo(Math.Max(0, ((TextNavigationPoint)this).LineNumber), Math.Max(0, ((TextNavigationPoint)this).Column));
			}
		}
	}
}
