using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Aga.Controls.Tree;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.UI;

internal sealed class ApplicationBrowserPad : AbstractPadContent, IHasPropertyContainer
{
	private ApplicationBrowserControl _Control;

	private static ApplicationBrowserPad _Instance;

	internal ApplicationBrowserControl Applications => _Control;

	public override Control Control => _Control;

	public static ApplicationBrowserPad Instance
	{
		get
		{
			if (_Instance == null)
			{
				PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(ApplicationBrowserPad));
				if (pad != null)
				{
					pad.CreatePad();
				}
				else
				{
					_Instance = new ApplicationBrowserPad();
				}
			}
			return _Instance;
		}
	}

	public Application SelectedApplication => ((TreeViewAdv)(object)_Control.applicationBrowserTree).SelectedNode.Tag as Application;

	public List<Application> SelectedApplications
	{
		get
		{
			List<Application> list = new List<Application>();
			Application application = null;
			foreach (TreeNodeAdv selectedNode in ((TreeViewAdv)(object)_Control.applicationBrowserTree).SelectedNodes)
			{
				if (selectedNode.Tag is Application item)
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public PropertyContainer PropertyContainer => _Control.PropertyContainer;

	public ApplicationBrowserPad()
	{
		_Control = new ApplicationBrowserControl(withProperStrings: true);
		_Control.Dock = DockStyle.Fill;
		_Instance = this;
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += ActiveWindowChanged;
		ActiveWindowChanged(null, null);
	}

	private void ActiveWindowChanged(object sender, EventArgs e)
	{
	}

	public override void RedrawContent()
	{
		if (_Control != null)
		{
			_Control.RedrawContent();
		}
	}
}
