using System;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.Generator.Editor;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public abstract class AbstractClarionGeneratorCommand : AbstractClarionDesignerCommand
{
	private CommonClarionGenDesignerView view;

	public override bool IsEnabled
	{
		get
		{
			if (view != null)
			{
				return true;
			}
			return false;
		}
		set
		{
		}
	}

	public CommonClarionGenDesignerView View => GetView();

	public AbstractClarionGeneratorCommand()
	{
		if ((view = GetView()) == null)
		{
			throw new NullReferenceException("AbstractSharpCommand : No view available");
		}
	}

	public CommonClarionGenDesignerView GetView()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
		{
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as CommonClarionGenDesignerView;
		}
		return null;
	}
}
