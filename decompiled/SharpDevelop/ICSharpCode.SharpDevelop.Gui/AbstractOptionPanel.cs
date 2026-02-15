using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class AbstractOptionPanel : BaseSharpDevelopUserControl, IDialogPanel
{
	protected class BrowseButtonEvent
	{
		private AbstractOptionPanel panel;

		private string target;

		private string filter;

		private TextBoxEditMode textBoxEditMode;

		[Obsolete("specify textBoxEditMode")]
		public BrowseButtonEvent(AbstractOptionPanel panel, string target, string filter)
			: this(panel, target, filter, TextBoxEditMode.EditEvaluatedProperty)
		{
		}

		public BrowseButtonEvent(AbstractOptionPanel panel, string target, string filter, TextBoxEditMode textBoxEditMode)
		{
			this.panel = panel;
			this.filter = filter;
			this.target = target;
			this.textBoxEditMode = textBoxEditMode;
		}

		public void Event(object sender, EventArgs e)
		{
			using SoftVelocity.Ide.Core.OpenFileDialog openFileDialog = FileDialogService.OpenFileDialog();
			openFileDialog.Filter = StringParser.Parse(filter);
			openFileDialog.Multiselect = false;
			openFileDialog.InitialDirectory = FileService.CurrentDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string text = openFileDialog.FileName;
				if (panel.baseDirectory != null)
				{
					text = FileUtility.GetRelativePath(panel.baseDirectory, text);
				}
				if (textBoxEditMode == TextBoxEditMode.EditEvaluatedProperty)
				{
					panel.ControlDictionary[target].Text = text;
				}
				else
				{
					panel.ControlDictionary[target].Text = MSBuildInternals.Escape(text);
				}
			}
		}
	}

	private class BrowseFolderEvent
	{
		private AbstractOptionPanel panel;

		private string target;

		private string description;

		private TextBoxEditMode textBoxEditMode;

		[Obsolete("Do not use BrowseFolderEvent directly")]
		public BrowseFolderEvent(AbstractOptionPanel panel, string target, string description)
			: this(panel, target, description, TextBoxEditMode.EditEvaluatedProperty)
		{
		}

		internal BrowseFolderEvent(AbstractOptionPanel panel, string target, string description, TextBoxEditMode textBoxEditMode)
		{
			this.panel = panel;
			this.description = description;
			this.target = target;
			this.textBoxEditMode = textBoxEditMode;
		}

		public void Event(object sender, EventArgs e)
		{
			string baseDirectory = panel.baseDirectory;
			string text = panel.ControlDictionary[target].Text;
			if (textBoxEditMode == TextBoxEditMode.EditRawProperty)
			{
				text = MSBuildInternals.Unescape(text);
			}
			using FolderBrowserDialog folderBrowserDialog = FileService.CreateFolderBrowserDialog(selectedPath: (baseDirectory == null) ? text : FileUtility.GetAbsolutePath(baseDirectory, text), description: description);
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				string text2 = folderBrowserDialog.SelectedPath;
				if (panel.baseDirectory != null)
				{
					text2 = FileUtility.GetRelativePath(panel.baseDirectory, text2);
				}
				if (!text2.EndsWith("\\") && !text2.EndsWith("/"))
				{
					text2 += "\\";
				}
				if (textBoxEditMode == TextBoxEditMode.EditEvaluatedProperty)
				{
					panel.ControlDictionary[target].Text = text2;
				}
				else
				{
					panel.ControlDictionary[target].Text = MSBuildInternals.Escape(text2);
				}
			}
		}
	}

	private bool wasActivated;

	private bool isFinished = true;

	private object customizationObject;

	private Dictionary<string, Control> _ControlDictionary;

	protected string baseDirectory;

	public Control Control => this;

	public bool WasActivated => wasActivated;

	public virtual object CustomizationObject
	{
		get
		{
			return customizationObject;
		}
		set
		{
			customizationObject = value;
			OnCustomizationObjectChanged();
		}
	}

	public virtual bool EnableFinish
	{
		get
		{
			return isFinished;
		}
		set
		{
			if (isFinished != value)
			{
				isFinished = value;
				OnEnableFinishChanged();
			}
		}
	}

	public override Dictionary<string, Control> ControlDictionary
	{
		get
		{
			if (xmlLoader == null)
			{
				if (_ControlDictionary == null)
				{
					_ControlDictionary = new Dictionary<string, Control>();
				}
				return _ControlDictionary;
			}
			return base.ControlDictionary;
		}
	}

	public event EventHandler CustomizationObjectChanged;

	public event EventHandler EnableFinishChanged;

	public void LoadControlDictionary(ControlCollection controls)
	{
		if (xmlLoader != null)
		{
			return;
		}
		try
		{
			foreach (Control control in controls)
			{
				ControlDictionary.Add(control.Name, control);
			}
		}
		catch
		{
		}
	}

	public virtual bool ReceiveDialogMessage(DialogMessage message)
	{
		switch (message)
		{
		case DialogMessage.Activated:
			if (!wasActivated)
			{
				LoadPanelContents();
				Dock = DockStyle.Fill;
				base.AutoScaleMode = AutoScaleMode.Font;
				AutoSize = true;
				Font = FontService.GetFont(FontService.FontType.Dialogs);
				ResumeLayout(performLayout: true);
				PerformLayout();
				wasActivated = true;
			}
			break;
		case DialogMessage.OK:
			if (wasActivated)
			{
				return StorePanelContents();
			}
			break;
		}
		return true;
	}

	public virtual void LoadPanelContents()
	{
	}

	public virtual bool StorePanelContents()
	{
		return true;
	}

	public bool ExistControlWithText(string textToSearch, TreeViewLocator.SearchFoundEventArgs controlsToSelect)
	{
		string textToSearch2 = textToSearch.ToUpper();
		if (ControlDictionary != null)
		{
			foreach (Control value in ControlDictionary.Values)
			{
				if (ExistControlWithText(value, textToSearch2, controlsToSelect))
				{
					return true;
				}
			}
		}
		if (base.Controls.Count > 0)
		{
			foreach (Control control in base.Controls)
			{
				if (ExistControlWithText(control, textToSearch, controlsToSelect))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ExistControlWithText(Control ctrl, string textToSearch, TreeViewLocator.SearchFoundEventArgs controlsToSelect)
	{
		if (ctrl != null)
		{
			if (!string.IsNullOrEmpty(ctrl.Text) && ctrl.Text.ToUpper().Contains(textToSearch))
			{
				controlsToSelect.AddControl(ctrl);
				return true;
			}
			if (ctrl.Controls.Count > 0)
			{
				foreach (Control control in ctrl.Controls)
				{
					if (ExistControlWithText(control, textToSearch, controlsToSelect))
					{
						controlsToSelect.AddControl(ctrl);
						return true;
					}
				}
			}
		}
		return false;
	}

	[Obsolete("Please specify fileFilter and targetNeedsMSBuildEncoding")]
	protected void ConnectBrowseButton(string browseButton, string target)
	{
		ConnectBrowseButton(browseButton, target, "${res:SharpDevelop.FileFilter.AllFiles}|*.*");
	}

	[Obsolete("Please specify targetNeedsMSBuildEncoding")]
	protected void ConnectBrowseButton(string browseButton, string target, string fileFilter)
	{
		ConnectBrowseButton(browseButton, target, fileFilter, TextBoxEditMode.EditEvaluatedProperty);
	}

	protected void ConnectBrowseButton(string browseButton, string target, string fileFilter, TextBoxEditMode textBoxEditMode)
	{
		if (ControlDictionary[browseButton] == null)
		{
			MessageService.ShowError(browseButton + " not found!");
		}
		else if (ControlDictionary[target] == null)
		{
			MessageService.ShowError(target + " not found!");
		}
		else
		{
			ControlDictionary[browseButton].Click += new BrowseButtonEvent(this, target, fileFilter, textBoxEditMode).Event;
		}
	}

	[Obsolete("Please specify textBoxEditMode")]
	protected void ConnectBrowseFolder(string browseButton, string target)
	{
		ConnectBrowseFolder(browseButton, target, TextBoxEditMode.EditEvaluatedProperty);
	}

	[Obsolete("Please specify textBoxEditMode")]
	protected void ConnectBrowseFolder(string browseButton, string target, string description)
	{
		ConnectBrowseFolder(browseButton, target, description, TextBoxEditMode.EditEvaluatedProperty);
	}

	protected void ConnectBrowseFolder(string browseButton, string target, TextBoxEditMode textBoxEditMode)
	{
		ConnectBrowseFolder(browseButton, target, "${res:Dialog.ProjectOptions.SelectFolderTitle}", textBoxEditMode);
	}

	protected void ConnectBrowseFolder(string browseButton, string target, string description, TextBoxEditMode textBoxEditMode)
	{
		if (ControlDictionary[browseButton] == null)
		{
			MessageService.ShowError(browseButton + " not found!");
		}
		else if (ControlDictionary[target] == null)
		{
			MessageService.ShowError(target + " not found!");
		}
		else
		{
			ControlDictionary[browseButton].Click += new BrowseFolderEvent(this, target, description, textBoxEditMode).Event;
		}
	}

	protected virtual void OnEnableFinishChanged()
	{
		if (this.EnableFinishChanged != null)
		{
			this.EnableFinishChanged(this, null);
		}
	}

	protected virtual void OnCustomizationObjectChanged()
	{
		if (this.CustomizationObjectChanged != null)
		{
			this.CustomizationObjectChanged(this, null);
		}
	}
}
