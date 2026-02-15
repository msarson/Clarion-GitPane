using System;
using System.Collections.Generic;
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

public class RegisterDatabaseEngines : AbstractMenuCommand
{
	private class DatabaseEngineListDialog : PositionedSharpDevelopForm
	{
		private string selectButtonName;

		private Dictionary<string, DatabaseEngine> engineList;

		private ListViewItem CurrentItem
		{
			get
			{
				ListView listView = (ListView)((XmlForm)this).ControlDictionary["engineList"];
				if (listView.SelectedItems.Count > 0)
				{
					return listView.Items[listView.SelectedIndices[0]];
				}
				return null;
			}
		}

		internal DatabaseEngineListDialog()
		{
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected O, but got Unknown
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Expected O, but got Unknown
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Expected O, but got Unknown
			engineList = new Dictionary<string, DatabaseEngine>();
			((XmlForm)this).SetupFromXmlStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CommonSources.Resources.RegisterDatabaseEngineDialog.xfrm"));
			((XmlForm)this).Get<Button>("add").Click += AddPressed;
			((XmlForm)this).Get<Button>("remove").Click += RemovePressed;
			selectButtonName = "removeButton";
			((Form)this).Icon = IconService.GetIcon("Icons.16x16.FindIcon");
			((Form)this).Owner = (Form)(object)WorkbenchSingleton.Workbench;
			ListView listView = (ListView)((XmlForm)this).ControlDictionary["engineList"];
			listView.ItemSelectionChanged += ListClicked;
			listView.Sorting = SortOrder.Ascending;
			ListView.ListViewItemCollection items = listView.Items;
			DatabaseEngineEnumerator enumerator = DatabaseEngineRegistry.DatabaseEngineList.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					DatabaseEngine val = (DatabaseEngine)enumerator.Current;
					if (!val.Hidden)
					{
						engineList.Add(val.Name, val);
						items.Add(val.Name);
					}
				}
			}
			finally
			{
				if (enumerator is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			DatabaseEngineRegistry.DatabaseEngineList.DatabaseEngineAdded += new DatabaseEvent(DatabaseAdded);
			DatabaseEngineRegistry.DatabaseEngineList.DatabaseEngineRemoved += new DatabaseEvent(DatabaseRemoved);
			if (items.Count > 0)
			{
				((XmlForm)this).ControlDictionary[selectButtonName].Enabled = true;
				listView.Select();
				listView.Focus();
				ListViewItem itemAt = listView.GetItemAt(listView.Left, listView.Top);
				itemAt.Selected = true;
			}
		}

		private void RemovePressed(object sender, EventArgs args)
		{
			ListViewItem currentItem = CurrentItem;
			if (currentItem != null)
			{
				DatabaseEngineRegistry.UnRegisterDatabaseEngine(currentItem.Text);
			}
		}

		protected void AddPressed(object sender, EventArgs args)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			RedirectionFile redirectionFile = RedirectionFile.GetRedirectionFile(true, Versions.GetActiveVersion(true));
			OpenFileDialog val = new OpenFileDialog(redirectionFile);
			try
			{
				((SoftVelocity.Ide.Core.FileDialog)(object)val).Filter = StringParser.Parse("${res:Clarion.FileFilter.DatabaseEngines} (" + Version.Prefix + "*s.dll)|" + Version.Prefix + "*s.dll|${res:SharpDevelop.FileFilter.AllFiles}|*.*");
				((SoftVelocity.Ide.Core.OpenFileDialog)(object)val).Multiselect = true;
				((SoftVelocity.Ide.Core.FileDialog)(object)val).CheckFileExists = true;
				((SoftVelocity.Ide.Core.FileDialog)(object)val).AddExtension = true;
				val.ExpandName = false;
				((SoftVelocity.Ide.Core.FileDialog)(object)val).InitialDirectory = Path.Combine(Path.GetPathRoot(Assembly.GetEntryAssembly().Location), Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
				((SoftVelocity.Ide.Core.FileDialog)(object)val).Title = ResourceService.GetString("Dialog.RegisterDatabaseEnginesDialog.Select");
				if (((SoftVelocity.Ide.Core.FileDialog)(object)val).ShowDialog() == DialogResult.OK)
				{
					string[] fileNames = ((SoftVelocity.Ide.Core.FileDialog)(object)val).FileNames;
					foreach (string text in fileNames)
					{
						string empty = string.Empty;
						if (!DatabaseEngineRegistry.RegisterDatabaseEngine(text, ref empty))
						{
							string arg = ResourceService.GetString("Dialog.RegisterDatabaseEnginesDialog.LoadFailed.Title");
							string format = ResourceService.GetString("Dialog.RegisterDatabaseEnginesDialog.LoadFailed");
							MessageBox.Show(string.Format(format, text, empty, arg));
						}
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			((XmlForm)this).ControlDictionary["engineList"].Select();
		}

		private void DatabaseAdded(DatabaseEngine engine)
		{
			ListView listView = (ListView)((XmlForm)this).ControlDictionary["engineList"];
			ListView.ListViewItemCollection items = listView.Items;
			items.Add(engine.Name);
			if (items.Count > 0)
			{
				((XmlForm)this).ControlDictionary[selectButtonName].Enabled = true;
				listView.Sort();
			}
		}

		private void DatabaseRemoved(DatabaseEngine engine)
		{
			ListView listView = (ListView)((XmlForm)this).ControlDictionary["engineList"];
			ListView.ListViewItemCollection items = listView.Items;
			foreach (ListViewItem item in items)
			{
				if (item.Text == engine.Name)
				{
					items.Remove(item);
					if (items.Count == 0)
					{
						((XmlForm)this).ControlDictionary[selectButtonName].Enabled = false;
					}
					else
					{
						listView.Sort();
					}
					break;
				}
			}
			listView.Select();
		}

		private void ListClicked(object sender, ListViewItemSelectionChangedEventArgs args)
		{
			if (args.IsSelected)
			{
				ListViewItem item = args.Item;
				DatabaseEngine val = engineList[item.Text];
				((XmlForm)this).ControlDictionary[selectButtonName].Enabled = val.CanBeUnloaded;
			}
		}
	}

	public override void Run()
	{
		DatabaseEngineListDialog databaseEngineListDialog = new DatabaseEngineListDialog();
		try
		{
			((Form)(object)databaseEngineListDialog).Owner = (Form)(object)WorkbenchSingleton.Workbench;
			((Form)(object)databaseEngineListDialog).ShowDialog();
		}
		finally
		{
			((IDisposable)databaseEngineListDialog)?.Dispose();
		}
	}
}
