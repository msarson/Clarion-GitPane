using System;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.Commands;

public abstract class AbstractClarionReportCommand : AbstractClarionDesignerCommand
{
	private CommonClarionDesignerView view;

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

	public CommonClarionDesignerView View => GetView();

	public AbstractClarionReportCommand()
	{
		if ((view = GetView()) == null)
		{
			throw new NullReferenceException("AbstractSharpCommand : No view available");
		}
	}

	public CommonClarionDesignerView GetView()
	{
		if (WorkbenchSingleton.Workbench.ActiveWorkbenchWindow != null)
		{
			return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow.ActiveViewContent as CommonClarionDesignerView;
		}
		return null;
	}
}
