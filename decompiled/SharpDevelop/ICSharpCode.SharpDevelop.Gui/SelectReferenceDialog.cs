using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class SelectReferenceDialog : Form, ISelectReferenceDialog
{
	protected ListView referencesListView;

	private Button selectButton;

	private Button removeButton;

	protected TabPage gacTabPage;

	private TabPage projectTabPage;

	protected TabPage browserTabPage;

	protected TabPage comTabPage = new TabPage();

	private Label referencesLabel;

	private ColumnHeader referenceHeader;

	private ColumnHeader typeHeader;

	private ColumnHeader locationHeader;

	protected TabControl referenceTabControl;

	private Button okButton;

	private Button cancelButton;

	private Button helpButton;

	private Container components;

	protected IProject configureProject;

	public ArrayList ReferenceInformations
	{
		get
		{
			ArrayList arrayList = new ArrayList();
			foreach (ListViewItem item in referencesListView.Items)
			{
				arrayList.Add(item.Tag);
			}
			return arrayList;
		}
	}

	public SelectReferenceDialog(IProject configureProject)
	{
		this.configureProject = configureProject;
		InitializeComponent();
		gacTabPage.Controls.Add(new GacReferencePanel(this));
		projectTabPage.Controls.Add(new ProjectReferencePanel(this, configureProject));
		browserTabPage.Controls.Add(new AssemblyReferencePanel(this));
		comTabPage.Controls.Add(new COMReferencePanel(this));
	}

	public void AddReference(ReferenceType referenceType, string referenceName, string referenceLocation, object tag)
	{
		ListViewItem listViewItem = new ListViewItem(new string[3]
		{
			referenceName,
			referenceType.ToString(),
			referenceLocation
		});
		ReferenceProjectItem referenceProjectItem = null;
		switch (referenceType)
		{
		case ReferenceType.Typelib:
			referenceProjectItem = new ComReferenceProjectItem(configureProject, (TypeLibrary)tag);
			break;
		case ReferenceType.Project:
			referenceProjectItem = new ProjectReferenceProjectItem(configureProject, (IProject)tag);
			break;
		case ReferenceType.Gac:
			referenceProjectItem = new ReferenceProjectItem(configureProject, referenceLocation);
			break;
		case ReferenceType.Assembly:
		{
			ReferenceProjectItem referenceProjectItem2 = new ReferenceProjectItem(configureProject);
			referenceProjectItem2.Include = Path.GetFileNameWithoutExtension(referenceLocation);
			referenceProjectItem2.HintPath = FileUtility.GetRelativePath(configureProject.Directory, referenceLocation);
			referenceProjectItem2.SpecificVersion = false;
			referenceProjectItem = referenceProjectItem2;
			break;
		}
		default:
			throw new NotSupportedException("Unknown reference type:" + referenceType);
		}
		listViewItem.Tag = referenceProjectItem;
		string refName = GetRefName(referenceProjectItem);
		foreach (ListViewItem item in referencesListView.Items)
		{
			string refName2 = GetRefName((ReferenceProjectItem)item.Tag);
			if (refName2.Equals(refName, StringComparison.InvariantCultureIgnoreCase))
			{
				return;
			}
		}
		foreach (ProjectItem item2 in configureProject.Items)
		{
			if (item2 is ReferenceProjectItem && item2.IsAddedToProject)
			{
				string refName3 = GetRefName((ReferenceProjectItem)item2);
				if (refName3.Equals(refName, StringComparison.InvariantCultureIgnoreCase))
				{
					MessageService.ShowError(string.Format(StringParser.Parse("${res:Dialog.SelectReferenceDialog.ReferenceExistsError}"), referenceProjectItem.FileName, refName3));
					return;
				}
			}
		}
		referencesListView.Items.Add(listViewItem);
	}

	private static string GetRefName(ReferenceProjectItem i)
	{
		if (i is ProjectReferenceProjectItem)
		{
			return ((ProjectReferenceProjectItem)i).ProjectName;
		}
		return i.Name;
	}

	private void SelectReference(object sender, EventArgs e)
	{
		IReferencePanel referencePanel = (IReferencePanel)referenceTabControl.SelectedTab.Controls[0];
		referencePanel.AddReference();
	}

	private void OkButtonClick(object sender, EventArgs e)
	{
		if (referencesListView.Items.Count == 0)
		{
			SelectReference(sender, e);
		}
	}

	private void RemoveReference(object sender, EventArgs e)
	{
		ArrayList arrayList = new ArrayList();
		foreach (ListViewItem selectedItem in referencesListView.SelectedItems)
		{
			arrayList.Add(selectedItem);
		}
		foreach (ListViewItem item in arrayList)
		{
			referencesListView.Items.Remove(item);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.referenceTabControl = new System.Windows.Forms.TabControl();
		this.referencesListView = new System.Windows.Forms.ListView();
		this.selectButton = new System.Windows.Forms.Button();
		this.removeButton = new System.Windows.Forms.Button();
		this.gacTabPage = new System.Windows.Forms.TabPage();
		this.projectTabPage = new System.Windows.Forms.TabPage();
		this.browserTabPage = new System.Windows.Forms.TabPage();
		this.referencesLabel = new System.Windows.Forms.Label();
		this.referenceHeader = new System.Windows.Forms.ColumnHeader();
		this.typeHeader = new System.Windows.Forms.ColumnHeader();
		this.locationHeader = new System.Windows.Forms.ColumnHeader();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.helpButton = new System.Windows.Forms.Button();
		this.referenceTabControl.SuspendLayout();
		base.SuspendLayout();
		this.referenceTabControl.Controls.AddRange(new System.Windows.Forms.Control[4] { this.gacTabPage, this.projectTabPage, this.browserTabPage, this.comTabPage });
		this.referenceTabControl.Location = new System.Drawing.Point(8, 8);
		this.referenceTabControl.SelectedIndex = 0;
		this.referenceTabControl.Size = new System.Drawing.Size(472, 224);
		this.referenceTabControl.TabIndex = 0;
		this.referencesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.referenceHeader, this.typeHeader, this.locationHeader });
		this.referencesListView.Location = new System.Drawing.Point(8, 256);
		this.referencesListView.Size = new System.Drawing.Size(472, 97);
		this.referencesListView.TabIndex = 3;
		this.referencesListView.View = System.Windows.Forms.View.Details;
		this.referencesListView.FullRowSelect = true;
		this.selectButton.Location = new System.Drawing.Point(488, 32);
		this.selectButton.TabIndex = 1;
		this.selectButton.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.SelectButton");
		this.selectButton.Click += new System.EventHandler(SelectReference);
		this.selectButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.removeButton.Location = new System.Drawing.Point(488, 256);
		this.removeButton.TabIndex = 4;
		this.removeButton.Text = ICSharpCode.Core.ResourceService.GetString("Global.RemoveButtonText");
		this.removeButton.Click += new System.EventHandler(RemoveReference);
		this.removeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.gacTabPage.Location = new System.Drawing.Point(4, 22);
		this.gacTabPage.Size = new System.Drawing.Size(464, 198);
		this.gacTabPage.TabIndex = 0;
		this.gacTabPage.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.GacTabPage");
		this.gacTabPage.UseVisualStyleBackColor = true;
		this.projectTabPage.Location = new System.Drawing.Point(4, 22);
		this.projectTabPage.Size = new System.Drawing.Size(464, 198);
		this.projectTabPage.TabIndex = 1;
		this.projectTabPage.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.ProjectTabPage");
		this.projectTabPage.UseVisualStyleBackColor = true;
		this.browserTabPage.Location = new System.Drawing.Point(4, 22);
		this.browserTabPage.Size = new System.Drawing.Size(464, 198);
		this.browserTabPage.TabIndex = 2;
		this.browserTabPage.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.BrowserTabPage");
		this.browserTabPage.UseVisualStyleBackColor = true;
		this.comTabPage.Location = new System.Drawing.Point(4, 22);
		this.comTabPage.Size = new System.Drawing.Size(464, 198);
		this.comTabPage.TabIndex = 2;
		this.comTabPage.Text = "COM";
		this.comTabPage.UseVisualStyleBackColor = true;
		this.referencesLabel.Location = new System.Drawing.Point(8, 240);
		this.referencesLabel.Size = new System.Drawing.Size(472, 16);
		this.referencesLabel.TabIndex = 2;
		this.referencesLabel.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.ReferencesLabel");
		this.referenceHeader.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.ReferenceHeader");
		this.referenceHeader.Width = 183;
		this.typeHeader.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.TypeHeader");
		this.typeHeader.Width = 57;
		this.locationHeader.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.LocationHeader");
		this.locationHeader.Width = 228;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(312, 368);
		this.okButton.TabIndex = 5;
		this.okButton.Text = ICSharpCode.Core.ResourceService.GetString("Global.OKButtonText");
		this.okButton.Click += new System.EventHandler(OkButtonClick);
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(400, 368);
		this.cancelButton.TabIndex = 6;
		this.cancelButton.Text = ICSharpCode.Core.ResourceService.GetString("Global.CancelButtonText");
		this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.helpButton.Location = new System.Drawing.Point(488, 368);
		this.helpButton.TabIndex = 7;
		this.helpButton.Text = ICSharpCode.Core.ResourceService.GetString("Global.HelpButtonText");
		this.helpButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
		base.AcceptButton = this.okButton;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(570, 399);
		base.Controls.AddRange(new System.Windows.Forms.Control[8] { this.helpButton, this.cancelButton, this.okButton, this.referencesLabel, this.removeButton, this.selectButton, this.referencesListView, this.referenceTabControl });
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.ShowInTaskbar = false;
		this.Text = ICSharpCode.Core.ResourceService.GetString("Dialog.SelectReferenceDialog.DialogName");
		this.referenceTabControl.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
