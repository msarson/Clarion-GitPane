using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common.ClarionEditor.WindowDesigner.Commands;

public class Duplicate : ViewCode
{
	public override bool IsEnabled
	{
		get
		{
			return true;
		}
		set
		{
			((AbstractMenuCommand)this).IsEnabled = value;
		}
	}

	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null)
		{
			return;
		}
		FormsDesignerViewContent formDesigner = base.FormDesigner;
		if (formDesigner == null || base.View == null || !base.View.IsDuplicateAllowed())
		{
			return;
		}
		((IClipboardHandler)formDesigner).Copy();
		if (formDesigner.Host != null)
		{
			ISelectionService selectionService = (ISelectionService)formDesigner.Host.GetService(typeof(ISelectionService));
			if (selectionService != null && selectionService.PrimarySelection is Control && selectionService.PrimarySelection is Control control)
			{
				IDesigner designer = formDesigner.Host.GetDesigner(control);
				if (designer is ControlDesigner && control != formDesigner.Host.RootComponent)
				{
					Control parentForDuplicate = base.View.GetParentForDuplicate(control);
					if (parentForDuplicate != null)
					{
						selectionService.SetSelectedComponents(new IComponent[1] { parentForDuplicate }, SelectionTypes.Click);
					}
				}
			}
		}
		((IClipboardHandler)formDesigner).Paste();
	}
}
