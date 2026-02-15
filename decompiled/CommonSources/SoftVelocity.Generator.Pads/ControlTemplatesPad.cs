using System;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace SoftVelocity.Generator.Pads;

internal sealed class ControlTemplatesPad : AbstractPadContent
{
	private ControlTemplatesControl _Control;

	private static ControlTemplatesPad _Instance;

	public override Control Control => _Control;

	public static ControlTemplatesPad Instance
	{
		get
		{
			if (_Instance == null)
			{
				PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(ControlTemplatesPad));
				if (pad != null)
				{
					pad.CreatePad();
				}
				else
				{
					_Instance = new ControlTemplatesPad();
				}
			}
			return _Instance;
		}
	}

	public ControlTemplatesPad()
	{
		_Control = new ControlTemplatesControl();
		_Control.Dock = DockStyle.Fill;
		_Instance = this;
	}

	public override void RedrawContent()
	{
		if (_Control != null)
		{
			_Control.Refresh();
		}
	}

	public bool RefreshTemplates(IFormatter iformatter)
	{
		if (_Instance != null)
		{
			return _Control.RefreshTemplates(iformatter);
		}
		return true;
	}

	public bool RemoveSelection()
	{
		if (_Instance != null)
		{
			return _Control.RemoveSelection();
		}
		return true;
	}

	public bool IsTemplateSelected()
	{
		if (_Instance != null)
		{
			return _Control.IsTemplateSelected;
		}
		return false;
	}

	public static void SelectedTabItemChanged(object sender, EventArgs e)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		if (_Instance == null || sender == null)
		{
			return;
		}
		SideTab val = (SideTab)sender;
		if (val != null)
		{
			SideTabItem choosedItem = val.ChoosedItem;
			if (choosedItem != null && choosedItem.Tag != null && choosedItem.Tag.ToString().ToUpperInvariant() != "POINTER")
			{
				_Instance.RemoveSelection();
			}
		}
	}

	public static SpecialDataObject GetDragDropDataObject()
	{
		if (_Instance != null)
		{
			return _Instance._Control.GetDragDropDataObject();
		}
		return null;
	}

	public static bool TemplatePopulated()
	{
		if (_Instance != null)
		{
			return _Instance._Control.TemplatePopulated(null, isRefresh: true);
		}
		return false;
	}
}
