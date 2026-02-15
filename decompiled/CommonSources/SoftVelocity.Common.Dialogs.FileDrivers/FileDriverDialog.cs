using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Clarion.ASL;
using Clarion.Core.Options;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using SoftVelocity.Ide.Core;

namespace SoftVelocity.Common.Dialogs.FileDrivers;

public abstract class FileDriverDialog : PositionedSharpDevelopForm
{
	protected string selectButtonName;

	private bool allowMultiSelect;

	private bool noADO;

	public FileDriverDialog(bool allowMultiSelect, bool browsableDriversOnly)
	{
		this.allowMultiSelect = allowMultiSelect;
		noADO = browsableDriversOnly;
		InitializeComponents();
	}

	public FileDriverDialog(bool allowMultiSelect)
		: this(allowMultiSelect, browsableDriversOnly: false)
	{
	}

	public FileDriverDialog()
		: this(allowMultiSelect: true)
	{
	}

	protected void AddPressed(object sender, EventArgs args)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		RedirectionFile redirectionFile = RedirectionFile.GetRedirectionFile(true, Versions.GetActiveVersion(true));
		OpenFileDialog val = new OpenFileDialog(redirectionFile);
		try
		{
			((SoftVelocity.Ide.Core.FileDialog)(object)val).Filter = StringParser.Parse("${res:Clarion.FileFilter.FileDrivers} (" + Version.Prefix + "*.dll)|" + Version.Prefix + "*.dll|${res:SharpDevelop.FileFilter.AllFiles}|*.*");
			((SoftVelocity.Ide.Core.OpenFileDialog)(object)val).Multiselect = true;
			((SoftVelocity.Ide.Core.FileDialog)(object)val).CheckFileExists = true;
			((SoftVelocity.Ide.Core.FileDialog)(object)val).AddExtension = true;
			val.ExpandName = false;
			((SoftVelocity.Ide.Core.FileDialog)(object)val).InitialDirectory = Path.Combine(Path.GetPathRoot(Assembly.GetEntryAssembly().Location), Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
			((SoftVelocity.Ide.Core.FileDialog)(object)val).Title = ResourceService.GetString("Dialog.RegisterFileDriversDialog.Select");
			if (((SoftVelocity.Ide.Core.FileDialog)(object)val).ShowDialog() == DialogResult.OK)
			{
				string[] fileNames = ((SoftVelocity.Ide.Core.FileDialog)(object)val).FileNames;
				foreach (string text in fileNames)
				{
					string empty = string.Empty;
					if (FileDriverRegistry.RegisterFileDriver(text, ref empty) == null)
					{
						string arg = ResourceService.GetString("Dialog.RegisterFileDriversDialog.LoadFailed.Title");
						string format = ResourceService.GetString("Dialog.RegisterFileDriversDialog.LoadFailed");
						MessageBox.Show(string.Format(format, text, empty, arg));
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		((XmlForm)this).ControlDictionary["driverList"].Select();
	}

	private void DriverAdded(IASLFileDriver driver)
	{
		if (!noADO || !driver.Name.Equals("ADO"))
		{
			ListView listView = (ListView)((XmlForm)this).ControlDictionary["driverList"];
			ListView.ListViewItemCollection items = listView.Items;
			ListViewItem listViewItem = items.Add(driver.Description);
			listViewItem.Tag = driver;
			((XmlForm)this).ControlDictionary[selectButtonName].Enabled = true;
			listView.Sort();
		}
	}

	private void DriverRemoved(IASLFileDriver driver)
	{
		ListView listView = (ListView)((XmlForm)this).ControlDictionary["driverList"];
		ListView.ListViewItemCollection items = listView.Items;
		foreach (ListViewItem item in items)
		{
			if (item.Text == driver.Description)
			{
				items.Remove(item);
				if (items.Count == 0)
				{
					((XmlForm)this).ControlDictionary[selectButtonName].Enabled = false;
				}
				break;
			}
		}
		listView.Select();
	}

	private void ListDoubleClicked(object sender, EventArgs args)
	{
		if (((Form)this).AcceptButton != null)
		{
			((Form)this).AcceptButton.PerformClick();
		}
	}

	protected abstract void SetupForm();

	private void InitializeComponents()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		SetupForm();
		((Form)this).Icon = IconService.GetIcon("Icons.16x16.FindIcon");
		((Form)this).Owner = (Form)(object)WorkbenchSingleton.Workbench;
		ListView listView = (ListView)((XmlForm)this).ControlDictionary["driverList"];
		listView.DoubleClick += ListDoubleClicked;
		listView.Sorting = SortOrder.Ascending;
		ListView.ListViewItemCollection items = listView.Items;
		listView.MultiSelect = allowMultiSelect;
		FileDriverRegistry.FileDriverList.FileDriverAdded += new DriverEvent(DriverAdded);
		FileDriverRegistry.FileDriverList.FileDriverRemoved += new DriverEvent(DriverRemoved);
		FileDriverEnumerator enumerator = FileDriverRegistry.FileDriverList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				IASLFileDriver val = (IASLFileDriver)enumerator.Current;
				if (!noADO || !val.Name.Equals("ADO"))
				{
					ListViewItem listViewItem = items.Add(val.Description);
					listViewItem.Tag = val;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator)?.Dispose();
		}
		if (items.Count > 0)
		{
			((XmlForm)this).ControlDictionary[selectButtonName].Enabled = true;
			listView.Select();
			listView.Focus();
			ListViewItem itemAt = listView.GetItemAt(listView.Left, listView.Top);
			itemAt.Selected = true;
		}
	}

	private void InitializeComponent()
	{
		((Control)this).SuspendLayout();
		((Form)this).ClientSize = new Size(284, 262);
		((Control)this).Name = "FileDriverDialog";
		((Form)this).ShowInTaskbar = false;
		((Control)this).ResumeLayout(performLayout: false);
	}
}
