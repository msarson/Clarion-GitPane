using System;
using System.Reflection;
using System.Windows.Forms;
using Clarion.ASL;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace SoftVelocity.Common.Dialogs.FileDrivers;

public class RegisterFileDrivers : AbstractMenuCommand
{
	private class FileDriverListDialog : FileDriverDialog
	{
		private ListViewItem CurrentItem
		{
			get
			{
				ListView listView = (ListView)((XmlForm)this).ControlDictionary["driverList"];
				if (listView.SelectedItems.Count > 0)
				{
					return listView.Items[listView.SelectedIndices[0]];
				}
				return null;
			}
		}

		private void RemovePressed(object sender, EventArgs args)
		{
			ListViewItem currentItem = CurrentItem;
			if (currentItem != null)
			{
				object tag = currentItem.Tag;
				IASLFileDriver val = (IASLFileDriver)((tag is IASLFileDriver) ? tag : null);
				if (val != null)
				{
					FileDriverRegistry.UnRegisterDriver(val.Name);
				}
			}
		}

		private void GetProperties(object sender, EventArgs args)
		{
			ListView listView = (ListView)((XmlForm)this).ControlDictionary["driverList"];
			if (listView.SelectedItems.Count > 0)
			{
				ListViewItem listViewItem = listView.Items[listView.SelectedIndices[0]];
				MessageBox.Show("To Do:  Show properties for " + listViewItem.Text);
				listView.Select();
			}
		}

		protected override void SetupForm()
		{
			((XmlForm)this).SetupFromXmlStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CommonSources.Resources.RegisterFileDriversDialog.xfrm"));
			((XmlForm)this).Get<Button>("properties").Click += GetProperties;
			((XmlForm)this).Get<Button>("add").Click += base.AddPressed;
			((XmlForm)this).Get<Button>("remove").Click += RemovePressed;
			selectButtonName = "removeButton";
			Control control = ((XmlForm)this).ControlDictionary["propertiesButton"];
			Control control2 = ((XmlForm)this).ControlDictionary["removeButton"];
			control2.Location = control.Location;
			control.Visible = false;
		}
	}

	public override void Run()
	{
		FileDriverListDialog fileDriverListDialog = new FileDriverListDialog();
		try
		{
			((Form)(object)fileDriverListDialog).Owner = (Form)(object)WorkbenchSingleton.Workbench;
			((Form)(object)fileDriverListDialog).ShowDialog();
		}
		finally
		{
			((IDisposable)fileDriverListDialog)?.Dispose();
		}
	}
}
