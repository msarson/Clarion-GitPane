using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Commands;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace SoftVelocity.Common.Redirection;

internal class OpenViaRedirectionFile : RedMenuCommand
{
	internal class LoadViaRedDialog : PositionedSharpDevelopForm
	{
		private RedirectionFile redFile;

		private TextBox textFileName;

		private AutoCompleteStringCollection redList;

		private int traceHeight = -1;

		private int buttonsHeight = -1;

		private int originalHeight;

		private string title;

		private bool autoCompleteEnable;

		private int lastFound = -1;

		private string lastText = "";

		public string File => ((XmlForm)this).ControlDictionary["fileName"].Text;

		protected override string DialogName => "LoadViaRedirectionFileDialog";

		public LoadViaRedDialog(string defaultFileName, RedirectionFile red)
		{
			autoCompleteEnable = PropertyService.Get<bool>("OpenFileViaRedDialogAutoComplete", true, "FileDialog", new string[0]);
			redList = new AutoCompleteStringCollection();
			redFile = red;
			lock (this)
			{
				((XmlForm)this).SetupFromXmlStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CommonSources.Resources.LoadViaRed.xfrm"));
				if (!string.IsNullOrEmpty(defaultFileName))
				{
					((XmlForm)this).ControlDictionary["fileName"].Text = defaultFileName;
				}
				title = ((Control)(object)this).Text;
				((Button)((XmlForm)this).ControlDictionary["traceButton"]).Click += OnTrace_Clicked;
				((Button)((XmlForm)this).ControlDictionary["copyButton"]).Click += OnCopy_Clicked;
				((XmlForm)this).ControlDictionary["traceButton"].Enabled = false;
				((XmlForm)this).ControlDictionary["copyButton"].Enabled = false;
				((XmlForm)this).ControlDictionary["copyButton"].Visible = false;
				textFileName = (TextBox)((XmlForm)this).ControlDictionary["fileName"];
				textFileName.TextChanged += OnFileName_TextChanged;
				if (autoCompleteEnable)
				{
					textFileName.AutoCompleteMode = AutoCompleteMode.None;
					textFileName.AutoCompleteSource = AutoCompleteSource.None;
					textFileName.AutoCompleteCustomSource = redList;
				}
				buttonsHeight = ((Button)((XmlForm)this).ControlDictionary["traceButton"]).Height;
				ButtonsRefresh();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			int height = ((Control)this).Height;
			((PositionedSharpDevelopForm)this).OnLoad(e);
			((Control)this).Height = height;
			originalHeight = ((Control)this).Height;
		}

		private void OnCopy_Clicked(object sender, EventArgs e)
		{
			StringBuilder stringBuilder = new StringBuilder();
			ListBox listBox = (ListBox)((XmlForm)this).ControlDictionary["traceListBox"];
			foreach (object item in listBox.Items)
			{
				stringBuilder.AppendLine(item.ToString());
			}
			ClipboardWrapper.SetDataObject((object)stringBuilder.ToString());
		}

		private void OnTrace_Clicked(object sender, EventArgs e)
		{
			List<string> list = redFile.Trace(File, RedirectionFile.CurrentDirectory);
			ListShow(doShow: true, list.ToArray());
		}

		private void ListShow(bool doShow, string[] listContent)
		{
			ListBox listBox = (ListBox)((XmlForm)this).ControlDictionary["traceListBox"];
			if (doShow)
			{
				if (!listBox.Visible)
				{
					listBox.Items.Clear();
					listBox.Items.AddRange(listContent);
					listBox.Visible = true;
					((XmlForm)this).ControlDictionary["copyButton"].Visible = true;
					traceHeight = ((Control)this).Height;
					if (((Control)this).Height < 300)
					{
						((Control)this).Height = 300;
					}
					listBox.Height = ((Control)this).Height - 130 - buttonsHeight - 10;
					((Control)(object)this).MinimumSize = default(Size);
				}
				else
				{
					listBox.Items.Clear();
					listBox.Items.AddRange(listContent);
				}
			}
			else if (listBox.Visible)
			{
				listBox.Items.Clear();
				listBox.Visible = false;
				((XmlForm)this).ControlDictionary["copyButton"].Visible = false;
				int height = (((Control)this).Height = originalHeight);
				((Control)this).Height = height;
				((Control)(object)this).MinimumSize = default(Size);
			}
		}

		private void ButtonsRefresh()
		{
			bool enabled = !string.IsNullOrEmpty(textFileName.Text);
			((XmlForm)this).ControlDictionary["traceButton"].Enabled = enabled;
			((XmlForm)this).ControlDictionary["okButton"].Enabled = enabled;
			((XmlForm)this).ControlDictionary["copyButton"].Enabled = enabled;
			ListShow(doShow: false, null);
		}

		private void OnFileName_TextChanged(object sender, EventArgs e)
		{
			ButtonsRefresh();
			if (!autoCompleteEnable || string.IsNullOrEmpty(textFileName.Text) || textFileName.Text.Length != 1 || !(textFileName.Text != lastText))
			{
				return;
			}
			lock (this)
			{
				lastText = textFileName.Text;
				string text = null;
				text = (textFileName.Text.Contains(".") ? textFileName.Text.Trim() : (textFileName.Text.Trim() + "*.*"));
				lastFound = 0;
				try
				{
					textFileName.AutoCompleteMode = AutoCompleteMode.None;
					textFileName.AutoCompleteSource = AutoCompleteSource.None;
					textFileName.AutoCompleteCustomSource = null;
					List<string> list = redFile.OpenNames(text, RedirectionFile.CurrentDirectory);
					if (list.Count > 0)
					{
						List<string> list2 = new List<string>();
						foreach (string item in list)
						{
							list2.Add(Path.GetFileName(item));
						}
						if (list2.Count > 0)
						{
							redList.Clear();
						}
						redList.AddRange(list2.ToArray());
						list2.Clear();
						list.Clear();
						lastFound = redList.Count;
					}
				}
				catch
				{
				}
				if (lastFound > 0)
				{
					if (textFileName.AutoCompleteMode == AutoCompleteMode.None)
					{
						textFileName.AutoCompleteCustomSource = redList;
						textFileName.AutoCompleteMode = AutoCompleteMode.Suggest;
						textFileName.AutoCompleteSource = AutoCompleteSource.CustomSource;
						textFileName.AutoCompleteCustomSource = redList;
						textFileName.Text = "";
						SendKeys.SendWait(lastText);
					}
				}
				else
				{
					textFileName.AutoCompleteMode = AutoCompleteMode.None;
					textFileName.AutoCompleteSource = AutoCompleteSource.None;
					textFileName.AutoCompleteCustomSource = null;
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			((Form)this).OnClosing(e);
			if (traceHeight != -1)
			{
				((Control)this).Height = traceHeight;
			}
		}
	}

	private string defaultFileName;

	internal string DefaultFileName
	{
		set
		{
			defaultFileName = value;
		}
	}

	protected override void DoRun()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		LoadViaRedDialog loadViaRedDialog = new LoadViaRedDialog(defaultFileName, redFile);
		try
		{
			bool flag = true;
			do
			{
				try
				{
					if (((Form)(object)loadViaRedDialog).ShowDialog() != DialogResult.OK)
					{
						return;
					}
					flag = !redFile.Exists(loadViaRedDialog.File, RedirectionFile.CurrentDirectory);
					if (!flag)
					{
						continue;
					}
					bool flag2 = Directory.Exists(loadViaRedDialog.File);
					if (!string.IsNullOrEmpty(loadViaRedDialog.File) && (flag2 || loadViaRedDialog.File.Contains("*")))
					{
						OpenFile val = new OpenFile();
						if (flag2)
						{
							val.DefaultFileFilterExtension = "*.*";
							val.DefaultInitialDirectory = loadViaRedDialog.File;
						}
						else
						{
							val.DefaultFileFilterExtension = loadViaRedDialog.File;
							val.DefaultInitialDirectory = RedirectionFile.CurrentDirectory;
						}
						((AbstractCommand)val).Run();
						return;
					}
					MessageBox.Show(string.Format(ResourceService.GetString("Clarion.Gui.OpenViaRedirection.FileNotFound"), loadViaRedDialog.File), ResourceService.GetString("Clarion.Gui.OpenViaRedirection.FileNotFound.Title"));
					continue;
				}
				catch
				{
					return;
				}
			}
			while (flag);
			FileService.OpenFile(redFile.OpenName(loadViaRedDialog.File, RedirectionFile.CurrentDirectory));
		}
		finally
		{
			((IDisposable)loadViaRedDialog)?.Dispose();
		}
	}
}
