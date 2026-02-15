using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DockPanelSkin;
using ICSharpCode.Core;
using WeifenLuo.WinFormsUI;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public class AppearanceEditor : Form
{
	private AppearanceProperties _ap;

	private string _fileName;

	private bool dirty;

	private AppearanceControl AppearanceControl1;

	private Button Button1;

	private Button Button2;

	private CustomizableMenuStrip CustomizableMenuStrip1;

	private CustomizableStatusStrip CustomizableStatusStrip1;

	private CustomizableToolStrip CustomizableToolStrip1;

	private Label lblPreview;

	private Button Load_Button;

	private ToolStripMenuItem MenuItem1ToolStripMenuItem;

	private ToolStripMenuItem MenuItem2ToolStripMenuItem;

	private ToolStripMenuItem MenuItem3ToolStripMenuItem;

	private Button OK_Button;

	private Panel Panel1;

	private PropertyGrid PropertyGrid1;

	private Button Save_Button;

	private TableLayoutPanel TableLayoutPanel1;

	private TableLayoutPanel TableLayoutPanel2;

	private TableLayoutPanel TableLayoutPanel3;

	private ToolStripDropDownButton ToolStripDropDownButton1;

	private ToolStripSeparator ToolStripMenuItem1;

	private ToolStripProgressBar ToolStripProgressBar1;

	private ToolStripStatusLabel ToolStripStatusLabel1;

	private ToolStripStatusLabel ToolStripStatusLabel2;

	private ToolStripButton newToolStripButton;

	private ToolStripButton openToolStripButton;

	private ToolStripButton saveToolStripButton;

	private ToolStripButton printToolStripButton;

	private ToolStripSeparator toolStripSeparator6;

	private ToolStripButton cutToolStripButton;

	private ToolStripButton copyToolStripButton;

	private ToolStripButton pasteToolStripButton;

	private ToolStripSeparator toolStripSeparator7;

	private ToolStripButton helpToolStripButton;

	private ToolStripMenuItem fileToolStripMenuItem;

	private ToolStripMenuItem newToolStripMenuItem;

	private ToolStripMenuItem openToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator;

	private ToolStripMenuItem saveToolStripMenuItem;

	private ToolStripMenuItem saveAsToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem printToolStripMenuItem;

	private ToolStripMenuItem printPreviewToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripMenuItem exitToolStripMenuItem;

	private ToolStripMenuItem editToolStripMenuItem;

	private ToolStripMenuItem undoToolStripMenuItem;

	private ToolStripMenuItem redoToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripMenuItem cutToolStripMenuItem;

	private ToolStripMenuItem copyToolStripMenuItem;

	private ToolStripMenuItem pasteToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripMenuItem selectAllToolStripMenuItem;

	private ToolStripMenuItem toolsToolStripMenuItem;

	private ToolStripMenuItem customizeToolStripMenuItem;

	private ToolStripMenuItem optionsToolStripMenuItem;

	private ToolStripMenuItem helpToolStripMenuItem;

	private ToolStripMenuItem contentsToolStripMenuItem;

	private ToolStripMenuItem indexToolStripMenuItem;

	private ToolStripMenuItem searchToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem aboutToolStripMenuItem;

	private Button SaveAs_Button;

	private DockPanel dockPanel;

	private IContainer components;

	private DummyDoc d1;

	private DummyDoc d2;

	private DummyDoc d3;

	private DummyTab t1;

	private DummyTab t1A;

	private DummyTab t1B;

	private DummyTab t2;

	private DummyTab t2A;

	private DummyTab t2B;

	private DummyTab t3;

	private DummyTab t4;

	private Button buttonShowTabs;

	private SplitContainer splitContainer1;

	private Button buttonResetTabsColors;

	private StatusStrip statusStrip1;

	private ToolStripStatusLabel labelFileName;

	private string fileName
	{
		get
		{
			return _fileName;
		}
		set
		{
			_fileName = value;
			labelFileName.Text = $"Theme File Name: {Path.GetFileName(_fileName)}";
		}
	}

	public AppearanceProperties CustomAppearance => _ap;

	public AppearanceEditor()
	{
		DockPanelColorTable.Instance.UseProfessionalColorTable = true;
		_ap = null;
		InitializeComponent();
		string value = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.DockPanelStyle", Extender.Style.VS2013.ToString());
		if (!Enum.TryParse<Extender.Style>(value, ignoreCase: false, out var result))
		{
			result = Extender.Style.VS2013;
		}
		DockPanelColorTable.Instance.SetProfessionalColorTable(AppearanceControl1.Renderer.ColorTable);
		AppearanceControl1.AppearanceChanged += AppearanceControl1_AppearanceChanged;
		SuspendLayout();
		Extender.SetSchema(result, dockPanel);
		dockPanel.DocumentStyle = DocumentStyles.DockingWindow;
		d1 = new DummyDoc("Doc1");
		d2 = new DummyDoc("Doc2");
		d3 = new DummyDoc("Doc3");
		t1 = new DummyTab("Tab1", DockState.DockLeftAutoHide);
		t1A = new DummyTab("Tab1A", DockState.DockLeftAutoHide);
		t1B = new DummyTab("Tab1B", DockState.DockLeftAutoHide);
		t2 = new DummyTab("Tab2", DockState.DockRightAutoHide);
		t2A = new DummyTab("Tab2A", DockState.DockRightAutoHide);
		t2B = new DummyTab("Tab2B", DockState.DockRightAutoHide);
		t3 = new DummyTab("Tab3", DockState.DockBottomAutoHide);
		t4 = new DummyTab("Tab4", DockState.DockBottomAutoHide);
		d1.HideOnClose = true;
		d2.HideOnClose = true;
		d3.HideOnClose = true;
		t1.HideOnClose = true;
		t1A.HideOnClose = true;
		t1B.HideOnClose = true;
		t2.HideOnClose = true;
		t2A.HideOnClose = true;
		t2B.HideOnClose = true;
		t3.HideOnClose = true;
		t4.HideOnClose = true;
		ResumeLayout();
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		AppearanceControl1.AppearanceChanged -= AppearanceControl1_AppearanceChanged;
		d1.HideOnClose = false;
		d1.Close();
		d2.HideOnClose = false;
		d2.Close();
		d3.HideOnClose = false;
		d3.Close();
		t1.HideOnClose = false;
		t1.Close();
		t1A.HideOnClose = false;
		t1A.Close();
		t1B.HideOnClose = false;
		t1B.Close();
		t2.HideOnClose = false;
		t2.Close();
		t2A.HideOnClose = false;
		t2A.Close();
		t2B.HideOnClose = false;
		t2B.Close();
		t3.HideOnClose = false;
		t3.Close();
		t4.HideOnClose = false;
		t4.Close();
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F1)
		{
			string text = GetType().FullName.Replace('.', '_') + ".htm";
			text = text.Replace('+', '_');
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			FileInfo fileInfo = new FileInfo(entryAssembly.Location);
			string text2 = Path.Combine(fileInfo.DirectoryName, "ClarionHelp.chm");
			if (File.Exists(text2))
			{
				Help.ShowHelp(WorkbenchSingleton.helpHost, text2, HelpNavigator.Topic, text);
			}
			else
			{
				MessageService.ShowWarning("${res:MainWindow.Windows.HtmlHelp.NotFound} " + text2);
			}
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void AppearanceControl1_AppearanceChanged(object sender, EventArgs e)
	{
		ShowDockContent();
	}

	private void ShowDockContent()
	{
		DockPanelColorTable.Instance.SetProfessionalColorTable(AppearanceControl1.Renderer.ColorTable);
		dockPanel.SuspendLayout(allWindows: true);
		d1.Hide();
		d2.Hide();
		d3.Hide();
		if (d1.IsHidden)
		{
			d1.Show(dockPanel, DockState.Document);
		}
		if (d2.IsHidden)
		{
			d2.Show(dockPanel, DockState.Document);
		}
		if (d3.IsHidden)
		{
			d3.Show(dockPanel, DockState.Document);
		}
		if (t1.IsHidden)
		{
			t1.Show(dockPanel, DockState.DockLeftAutoHide);
		}
		if (t1A.IsHidden)
		{
			t1A.Show(dockPanel, DockState.DockLeftAutoHide);
		}
		if (t1B.IsHidden)
		{
			t1B.Show(dockPanel, DockState.DockLeftAutoHide);
		}
		if (t2.IsHidden)
		{
			t2.Show(dockPanel, DockState.DockRightAutoHide);
		}
		if (t2A.IsHidden)
		{
			t2A.Show(dockPanel, DockState.DockRightAutoHide);
		}
		if (t2B.IsHidden)
		{
			t2B.Show(dockPanel, DockState.DockRightAutoHide);
		}
		if (t3.IsHidden)
		{
			t3.Show(dockPanel, DockState.DockBottomAutoHide);
		}
		if (t4.IsHidden)
		{
			t4.Show(dockPanel, DockState.DockBottomAutoHide);
		}
		dockPanel.ResumeLayout(performLayout: true, allWindows: true);
	}

	public AppearanceEditor(string fileName)
		: this(new AppearanceControl(fileName).AppearanceProperties)
	{
		this.fileName = fileName;
	}

	public AppearanceEditor(AppearanceProperties ap)
		: this()
	{
		_ap = ap;
		CustomizableMenuStrip1.Appearance.AppearanceProperties = ap;
		CustomizableStatusStrip1.Appearance.AppearanceProperties = ap;
		CustomizableToolStrip1.Appearance.AppearanceProperties = ap;
		PropertyGrid1.SelectedObject = ap;
		PropertyGrid1.PropertyValueChanged += PropertyGrid1_PropertyValueChanged;
		ShowDockContent();
	}

	private void PropertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
	{
		dirty = true;
	}

	public AppearanceEditor(AppearanceProperties ap, string fileNameToEdit)
		: this(ap)
	{
		lblPreview.Text = "Preview: " + fileName;
		fileName = fileNameToEdit;
		Load_Button.Visible = false;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && components != null)
			{
				dockPanel.Dispose();
				components.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
			d1 = null;
			d2 = null;
			d3 = null;
			t1 = null;
			t1A = null;
			t1B = null;
			t2 = null;
			t2A = null;
			t2B = null;
			t3 = null;
			t4 = null;
			_ap = null;
			dockPanel = null;
		}
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ICSharpCode.SharpDevelop.Gui.CustomizableStrips.AppearanceEditor));
		this.TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.OK_Button = new System.Windows.Forms.Button();
		this.TableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
		this.Button1 = new System.Windows.Forms.Button();
		this.Button2 = new System.Windows.Forms.Button();
		this.TableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
		this.Load_Button = new System.Windows.Forms.Button();
		this.Save_Button = new System.Windows.Forms.Button();
		this.SaveAs_Button = new System.Windows.Forms.Button();
		this.Panel1 = new System.Windows.Forms.Panel();
		this.CustomizableToolStrip1 = new ICSharpCode.SharpDevelop.Gui.CustomizableStrips.CustomizableToolStrip();
		this.AppearanceControl1 = new ICSharpCode.SharpDevelop.Gui.CustomizableStrips.AppearanceControl();
		this.newToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.printToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
		this.cutToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.copyToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.pasteToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
		this.helpToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.CustomizableStatusStrip1 = new ICSharpCode.SharpDevelop.Gui.CustomizableStrips.CustomizableStatusStrip();
		this.ToolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
		this.ToolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
		this.ToolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
		this.ToolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
		this.MenuItem3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.ToolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
		this.MenuItem2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.MenuItem1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.CustomizableMenuStrip1 = new ICSharpCode.SharpDevelop.Gui.CustomizableStrips.CustomizableMenuStrip();
		this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
		this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.customizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.indexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.lblPreview = new System.Windows.Forms.Label();
		this.PropertyGrid1 = new System.Windows.Forms.PropertyGrid();
		this.dockPanel = new WeifenLuo.WinFormsUI.DockPanel();
		this.buttonShowTabs = new System.Windows.Forms.Button();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.buttonResetTabsColors = new System.Windows.Forms.Button();
		this.statusStrip1 = new System.Windows.Forms.StatusStrip();
		this.labelFileName = new System.Windows.Forms.ToolStripStatusLabel();
		this.TableLayoutPanel1.SuspendLayout();
		this.TableLayoutPanel2.SuspendLayout();
		this.TableLayoutPanel3.SuspendLayout();
		this.Panel1.SuspendLayout();
		this.CustomizableToolStrip1.SuspendLayout();
		this.CustomizableStatusStrip1.SuspendLayout();
		this.CustomizableMenuStrip1.SuspendLayout();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		this.statusStrip1.SuspendLayout();
		base.SuspendLayout();
		this.TableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.TableLayoutPanel1.ColumnCount = 1;
		this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.TableLayoutPanel1.Controls.Add(this.OK_Button, 0, 0);
		this.TableLayoutPanel1.Location = new System.Drawing.Point(648, 485);
		this.TableLayoutPanel1.Name = "TableLayoutPanel1";
		this.TableLayoutPanel1.RowCount = 1;
		this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.TableLayoutPanel1.Size = new System.Drawing.Size(91, 29);
		this.TableLayoutPanel1.TabIndex = 0;
		this.OK_Button.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.OK_Button.Location = new System.Drawing.Point(6, 3);
		this.OK_Button.Name = "OK_Button";
		this.OK_Button.Size = new System.Drawing.Size(78, 23);
		this.OK_Button.TabIndex = 0;
		this.OK_Button.Text = "OK";
		this.OK_Button.Click += new System.EventHandler(OnOK_Button_Click);
		this.TableLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.TableLayoutPanel2.ColumnCount = 2;
		this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.TableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.TableLayoutPanel2.Controls.Add(this.Button1, 0, 0);
		this.TableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
		this.TableLayoutPanel2.Name = "TableLayoutPanel2";
		this.TableLayoutPanel2.RowCount = 1;
		this.TableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.TableLayoutPanel2.Size = new System.Drawing.Size(200, 100);
		this.TableLayoutPanel2.TabIndex = 0;
		this.Button1.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.Button1.Location = new System.Drawing.Point(16, 38);
		this.Button1.Name = "Button1";
		this.Button1.Size = new System.Drawing.Size(67, 23);
		this.Button1.TabIndex = 0;
		this.Button1.Text = "OK";
		this.Button2.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.Button2.Location = new System.Drawing.Point(36, 3);
		this.Button2.Name = "Button2";
		this.Button2.Size = new System.Drawing.Size(28, 8);
		this.Button2.TabIndex = 1;
		this.Button2.Text = "Cancel";
		this.TableLayoutPanel3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.TableLayoutPanel3.ColumnCount = 3;
		this.TableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.TableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.TableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.TableLayoutPanel3.Controls.Add(this.Load_Button, 0, 0);
		this.TableLayoutPanel3.Controls.Add(this.Save_Button, 1, 0);
		this.TableLayoutPanel3.Controls.Add(this.SaveAs_Button, 2, 0);
		this.TableLayoutPanel3.Location = new System.Drawing.Point(12, 485);
		this.TableLayoutPanel3.Name = "TableLayoutPanel3";
		this.TableLayoutPanel3.RowCount = 1;
		this.TableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.TableLayoutPanel3.Size = new System.Drawing.Size(230, 29);
		this.TableLayoutPanel3.TabIndex = 1;
		this.Load_Button.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.Load_Button.Location = new System.Drawing.Point(3, 3);
		this.Load_Button.Name = "Load_Button";
		this.Load_Button.Size = new System.Drawing.Size(67, 23);
		this.Load_Button.TabIndex = 0;
		this.Load_Button.Text = "Load";
		this.Load_Button.Click += new System.EventHandler(OnLoad_Button_Click);
		this.Save_Button.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.Save_Button.Location = new System.Drawing.Point(76, 3);
		this.Save_Button.Name = "Save_Button";
		this.Save_Button.Size = new System.Drawing.Size(67, 23);
		this.Save_Button.TabIndex = 1;
		this.Save_Button.Text = "Save";
		this.Save_Button.Click += new System.EventHandler(OnSave_Button_Click);
		this.SaveAs_Button.Location = new System.Drawing.Point(149, 3);
		this.SaveAs_Button.Name = "SaveAs_Button";
		this.SaveAs_Button.Size = new System.Drawing.Size(75, 23);
		this.SaveAs_Button.TabIndex = 2;
		this.SaveAs_Button.Text = "Save As";
		this.SaveAs_Button.UseVisualStyleBackColor = true;
		this.SaveAs_Button.Click += new System.EventHandler(SaveAs_Button_Click);
		this.Panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.Panel1.Controls.Add(this.CustomizableToolStrip1);
		this.Panel1.Controls.Add(this.CustomizableStatusStrip1);
		this.Panel1.Controls.Add(this.CustomizableMenuStrip1);
		this.Panel1.Location = new System.Drawing.Point(15, 25);
		this.Panel1.Name = "Panel1";
		this.Panel1.Size = new System.Drawing.Size(724, 88);
		this.Panel1.TabIndex = 2;
		this.CustomizableToolStrip1.Appearance = this.AppearanceControl1;
		this.CustomizableToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[10] { this.newToolStripButton, this.openToolStripButton, this.saveToolStripButton, this.printToolStripButton, this.toolStripSeparator6, this.cutToolStripButton, this.copyToolStripButton, this.pasteToolStripButton, this.toolStripSeparator7, this.helpToolStripButton });
		this.CustomizableToolStrip1.Location = new System.Drawing.Point(0, 28);
		this.CustomizableToolStrip1.Name = "CustomizableToolStrip1";
		this.CustomizableToolStrip1.RoundedEdges = true;
		this.CustomizableToolStrip1.Size = new System.Drawing.Size(722, 25);
		this.CustomizableToolStrip1.TabIndex = 2;
		this.CustomizableToolStrip1.Text = "CustomizableToolStrip1";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.CheckedAppearance.xBackground = "-16273";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.CheckedAppearance.xBorderHighlight = "-13410648";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.CheckedAppearance.xGradientBegin = "-8294";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.CheckedAppearance.xGradientEnd = "-22964";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.CheckedAppearance.xGradientMiddle = "-15500";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.CheckedAppearance.xHighlight = "-3878683";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.CheckedAppearance.xPressedBackground = "-98242";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.CheckedAppearance.xSelectedBackground = "-98242";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.PressedAppearance.Border = System.Drawing.Color.FromArgb(0, 0, 128);
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.PressedAppearance.xBorder = "-16777088";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.PressedAppearance.xBorderHighlight = "-13410648";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.PressedAppearance.xGradientBegin = "-98242";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.PressedAppearance.xGradientEnd = "-8294";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.PressedAppearance.xGradientMiddle = "-20115";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.PressedAppearance.xHighlight = "-6771246";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.SelectedAppearance.Border = System.Drawing.Color.FromArgb(0, 0, 128);
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.SelectedAppearance.BorderHighlight = System.Drawing.Color.FromArgb(0, 0, 128);
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.SelectedAppearance.xBorder = "-16777088";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.SelectedAppearance.xBorderHighlight = "-16777088";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.SelectedAppearance.xGradientBegin = "-34";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.SelectedAppearance.xGradientEnd = "-13432";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.SelectedAppearance.xGradientMiddle = "-7764";
		this.AppearanceControl1.AppearanceProperties.ButtonAppearance.SelectedAppearance.xHighlight = "-3878683";
		this.AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.ActiveBackColorGradientBegin = System.Drawing.Color.FromArgb(227, 239, 255);
		this.AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.ActiveBackColorGradientEnd = System.Drawing.Color.FromArgb(123, 164, 224);
		this.AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.ActiveTextColor = System.Drawing.Color.FromArgb(123, 164, 224);
		this.AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.InactiveBackColor = System.Drawing.Color.FromArgb(158, 190, 245);
		this.AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.InactiveTextColor = System.Drawing.Color.FromArgb(158, 190, 245);
		this.AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.xActiveBackColorGradientBegin = "-1839105";
		this.AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.xActiveBackColorGradientEnd = "-8674080";
		this.AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.xInactiveBackColor = "-6373643";
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.EdgeColor = System.Drawing.Color.FromArgb(0, 45, 150);
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.GradientBegin = System.Drawing.Color.FromArgb(227, 239, 255);
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.GradientEnd = System.Drawing.Color.FromArgb(123, 164, 224);
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.xEdgeColor = "-16765546";
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.xGradientBegin = "-1839105";
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.xGradientEnd = "-8674080";
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Inactive.EdgeColor = System.Drawing.Color.FromArgb(59, 97, 156);
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Inactive.xEdgeColor = "-12885604";
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Inactive.xGradientBegin = "-6373643";
		this.AppearanceControl1.AppearanceProperties.DockTabAppearance.Inactive.xGradientEnd = "-3876102";
		this.AppearanceControl1.AppearanceProperties.DockTabStripAppearance.xGradientBegin = "-6373643";
		this.AppearanceControl1.AppearanceProperties.DockTabStripAppearance.xGradientEnd = "-3876102";
		this.AppearanceControl1.AppearanceProperties.GripAppearance.Light = System.Drawing.Color.FromArgb(255, 255, 255);
		this.AppearanceControl1.AppearanceProperties.GripAppearance.xDark = "-14204554";
		this.AppearanceControl1.AppearanceProperties.GripAppearance.xLight = "-1";
		this.AppearanceControl1.AppearanceProperties.ImageMarginAppearance.Normal.xGradientBegin = "-1839105";
		this.AppearanceControl1.AppearanceProperties.ImageMarginAppearance.Normal.xGradientEnd = "-8674080";
		this.AppearanceControl1.AppearanceProperties.ImageMarginAppearance.Normal.xGradientMiddle = "-3415556";
		this.AppearanceControl1.AppearanceProperties.ImageMarginAppearance.Revealed.xGradientBegin = "-3416586";
		this.AppearanceControl1.AppearanceProperties.ImageMarginAppearance.Revealed.xGradientEnd = "-9266217";
		this.AppearanceControl1.AppearanceProperties.ImageMarginAppearance.Revealed.xGradientMiddle = "-6175239";
		this.AppearanceControl1.AppearanceProperties.MenuItemAppearance.Border = System.Drawing.Color.FromArgb(0, 0, 128);
		this.AppearanceControl1.AppearanceProperties.MenuItemAppearance.xBorder = "-16777088";
		this.AppearanceControl1.AppearanceProperties.MenuItemAppearance.xPressedGradientBegin = "-1839105";
		this.AppearanceControl1.AppearanceProperties.MenuItemAppearance.xPressedGradientEnd = "-8674080";
		this.AppearanceControl1.AppearanceProperties.MenuItemAppearance.xPressedGradientMiddle = "-6175239";
		this.AppearanceControl1.AppearanceProperties.MenuItemAppearance.xSelected = "-4414";
		this.AppearanceControl1.AppearanceProperties.MenuItemAppearance.xSelectedGradientBegin = "-34";
		this.AppearanceControl1.AppearanceProperties.MenuItemAppearance.xSelectedGradientEnd = "-13432";
		this.AppearanceControl1.AppearanceProperties.MenuStripAppearance.xBorder = "-16765546";
		this.AppearanceControl1.AppearanceProperties.MenuStripAppearance.xGradientBegin = "-6373643";
		this.AppearanceControl1.AppearanceProperties.MenuStripAppearance.xGradientEnd = "-3876102";
		this.AppearanceControl1.AppearanceProperties.OverflowButtonAppearance.xGradientBegin = "-8408582";
		this.AppearanceControl1.AppearanceProperties.OverflowButtonAppearance.xGradientEnd = "-16763503";
		this.AppearanceControl1.AppearanceProperties.OverflowButtonAppearance.xGradientMiddle = "-11370544";
		this.AppearanceControl1.AppearanceProperties.RaftingContainerAppearance.xGradientBegin = "-6373643";
		this.AppearanceControl1.AppearanceProperties.RaftingContainerAppearance.xGradientEnd = "-3876102";
		this.AppearanceControl1.AppearanceProperties.SeparatorAppearance.xDark = "-9794357";
		this.AppearanceControl1.AppearanceProperties.SeparatorAppearance.xLight = "-919041";
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.BackgroundGradientBegin = System.Drawing.SystemColors.ActiveCaption;
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.BackgroundGradientEnd = System.Drawing.SystemColors.GradientInactiveCaption;
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.ButtonImageColor = System.Drawing.SystemColors.ActiveCaption;
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.PrimaryColor = System.Drawing.SystemColors.ActiveCaption;
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.SecondaryColor = System.Drawing.SystemColors.GradientInactiveCaption;
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.xButtonImageColor = "ActiveCaption";
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.xGradientBegin = "ActiveCaption";
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.xGradientEnd = "GradientInactiveCaption";
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.xPrimaryColor = "ActiveCaption";
		this.AppearanceControl1.AppearanceProperties.StartPageAppearance.xSecondaryColor = "GradientInactiveCaption";
		this.AppearanceControl1.AppearanceProperties.StatusStripAppearance.xGradientBegin = "-6373643";
		this.AppearanceControl1.AppearanceProperties.StatusStripAppearance.xGradientEnd = "-3876102";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xBorder = "-12885604";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xContentPanelGradientBegin = "-6373643";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xContentPanelGradientEnd = "-3876102";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xDropDownBackground = "-592138";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xGradientBegin = "-1839105";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xGradientEnd = "-8674080";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xGradientMiddle = "-3415556";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xPanelGradientBegin = "-6373643";
		this.AppearanceControl1.AppearanceProperties.ToolStripAppearance.xPanelGradientEnd = "-3876102";
		this.AppearanceControl1.Renderer.RoundedEdges = true;
		this.newToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.newToolStripButton.Image = (System.Drawing.Image)resources.GetObject("newToolStripButton.Image");
		this.newToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.newToolStripButton.Name = "newToolStripButton";
		this.newToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.newToolStripButton.Text = "&New";
		this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.openToolStripButton.Image = (System.Drawing.Image)resources.GetObject("openToolStripButton.Image");
		this.openToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.openToolStripButton.Name = "openToolStripButton";
		this.openToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.openToolStripButton.Text = "&Open";
		this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.saveToolStripButton.Image = (System.Drawing.Image)resources.GetObject("saveToolStripButton.Image");
		this.saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.saveToolStripButton.Name = "saveToolStripButton";
		this.saveToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.saveToolStripButton.Text = "&Save";
		this.printToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.printToolStripButton.Image = (System.Drawing.Image)resources.GetObject("printToolStripButton.Image");
		this.printToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.printToolStripButton.Name = "printToolStripButton";
		this.printToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.printToolStripButton.Text = "&Print";
		this.toolStripSeparator6.Name = "toolStripSeparator6";
		this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
		this.cutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.cutToolStripButton.Image = (System.Drawing.Image)resources.GetObject("cutToolStripButton.Image");
		this.cutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.cutToolStripButton.Name = "cutToolStripButton";
		this.cutToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.cutToolStripButton.Text = "C&ut";
		this.copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.copyToolStripButton.Image = (System.Drawing.Image)resources.GetObject("copyToolStripButton.Image");
		this.copyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.copyToolStripButton.Name = "copyToolStripButton";
		this.copyToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.copyToolStripButton.Text = "&Copy";
		this.pasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.pasteToolStripButton.Image = (System.Drawing.Image)resources.GetObject("pasteToolStripButton.Image");
		this.pasteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.pasteToolStripButton.Name = "pasteToolStripButton";
		this.pasteToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.pasteToolStripButton.Text = "&Paste";
		this.toolStripSeparator7.Name = "toolStripSeparator7";
		this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
		this.helpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.helpToolStripButton.Image = (System.Drawing.Image)resources.GetObject("helpToolStripButton.Image");
		this.helpToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.helpToolStripButton.Name = "helpToolStripButton";
		this.helpToolStripButton.Size = new System.Drawing.Size(23, 22);
		this.helpToolStripButton.Text = "He&lp";
		this.CustomizableStatusStrip1.Appearance = this.AppearanceControl1;
		this.CustomizableStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.ToolStripStatusLabel1, this.ToolStripProgressBar1, this.ToolStripStatusLabel2, this.ToolStripDropDownButton1 });
		this.CustomizableStatusStrip1.Location = new System.Drawing.Point(0, 60);
		this.CustomizableStatusStrip1.Name = "CustomizableStatusStrip1";
		this.CustomizableStatusStrip1.Size = new System.Drawing.Size(722, 26);
		this.CustomizableStatusStrip1.TabIndex = 1;
		this.CustomizableStatusStrip1.Text = "CustomizableStatusStrip1";
		this.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1";
		this.ToolStripStatusLabel1.Size = new System.Drawing.Size(45, 21);
		this.ToolStripStatusLabel1.Text = "Label";
		this.ToolStripProgressBar1.Name = "ToolStripProgressBar1";
		this.ToolStripProgressBar1.Size = new System.Drawing.Size(100, 20);
		this.ToolStripStatusLabel2.Name = "ToolStripStatusLabel2";
		this.ToolStripStatusLabel2.Size = new System.Drawing.Size(494, 21);
		this.ToolStripStatusLabel2.Spring = true;
		this.ToolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.MenuItem3ToolStripMenuItem, this.ToolStripMenuItem1, this.MenuItem2ToolStripMenuItem, this.MenuItem1ToolStripMenuItem });
		this.ToolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.ToolStripDropDownButton1.Name = "ToolStripDropDownButton1";
		this.ToolStripDropDownButton1.Size = new System.Drawing.Size(66, 24);
		this.ToolStripDropDownButton1.Text = "Button";
		this.MenuItem3ToolStripMenuItem.Name = "MenuItem3ToolStripMenuItem";
		this.MenuItem3ToolStripMenuItem.Size = new System.Drawing.Size(161, 24);
		this.MenuItem3ToolStripMenuItem.Text = "Menu item 3";
		this.ToolStripMenuItem1.Name = "ToolStripMenuItem1";
		this.ToolStripMenuItem1.Size = new System.Drawing.Size(158, 6);
		this.MenuItem2ToolStripMenuItem.Name = "MenuItem2ToolStripMenuItem";
		this.MenuItem2ToolStripMenuItem.Size = new System.Drawing.Size(161, 24);
		this.MenuItem2ToolStripMenuItem.Text = "Menu item 2";
		this.MenuItem1ToolStripMenuItem.Name = "MenuItem1ToolStripMenuItem";
		this.MenuItem1ToolStripMenuItem.Size = new System.Drawing.Size(161, 24);
		this.MenuItem1ToolStripMenuItem.Text = "Menu item 1";
		this.CustomizableMenuStrip1.Appearance = this.AppearanceControl1;
		this.CustomizableMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.fileToolStripMenuItem, this.editToolStripMenuItem, this.toolsToolStripMenuItem, this.helpToolStripMenuItem });
		this.CustomizableMenuStrip1.Location = new System.Drawing.Point(0, 0);
		this.CustomizableMenuStrip1.Name = "CustomizableMenuStrip1";
		this.CustomizableMenuStrip1.Size = new System.Drawing.Size(722, 28);
		this.CustomizableMenuStrip1.TabIndex = 0;
		this.CustomizableMenuStrip1.Text = "CustomizableMenuStrip1";
		this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[10] { this.newToolStripMenuItem, this.openToolStripMenuItem, this.toolStripSeparator, this.saveToolStripMenuItem, this.saveAsToolStripMenuItem, this.toolStripSeparator1, this.printToolStripMenuItem, this.printPreviewToolStripMenuItem, this.toolStripSeparator2, this.exitToolStripMenuItem });
		this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
		this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
		this.fileToolStripMenuItem.Text = "&File";
		this.newToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("newToolStripMenuItem.Image");
		this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.newToolStripMenuItem.Name = "newToolStripMenuItem";
		this.newToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.N | System.Windows.Forms.Keys.Control;
		this.newToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
		this.newToolStripMenuItem.Text = "&New";
		this.openToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("openToolStripMenuItem.Image");
		this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.openToolStripMenuItem.Name = "openToolStripMenuItem";
		this.openToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.O | System.Windows.Forms.Keys.Control;
		this.openToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
		this.openToolStripMenuItem.Text = "&Open";
		this.toolStripSeparator.Name = "toolStripSeparator";
		this.toolStripSeparator.Size = new System.Drawing.Size(164, 6);
		this.saveToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("saveToolStripMenuItem.Image");
		this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
		this.saveToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.S | System.Windows.Forms.Keys.Control;
		this.saveToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
		this.saveToolStripMenuItem.Text = "&Save";
		this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
		this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
		this.saveAsToolStripMenuItem.Text = "Save &As";
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(164, 6);
		this.printToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("printToolStripMenuItem.Image");
		this.printToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.printToolStripMenuItem.Name = "printToolStripMenuItem";
		this.printToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.P | System.Windows.Forms.Keys.Control;
		this.printToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
		this.printToolStripMenuItem.Text = "&Print";
		this.printPreviewToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("printPreviewToolStripMenuItem.Image");
		this.printPreviewToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
		this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
		this.printPreviewToolStripMenuItem.Text = "Print Pre&view";
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(164, 6);
		this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
		this.exitToolStripMenuItem.Size = new System.Drawing.Size(167, 24);
		this.exitToolStripMenuItem.Text = "E&xit";
		this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.undoToolStripMenuItem, this.redoToolStripMenuItem, this.toolStripSeparator3, this.cutToolStripMenuItem, this.copyToolStripMenuItem, this.pasteToolStripMenuItem, this.toolStripSeparator4, this.selectAllToolStripMenuItem });
		this.editToolStripMenuItem.Name = "editToolStripMenuItem";
		this.editToolStripMenuItem.Size = new System.Drawing.Size(47, 24);
		this.editToolStripMenuItem.Text = "&Edit";
		this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
		this.undoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Z | System.Windows.Forms.Keys.Control;
		this.undoToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
		this.undoToolStripMenuItem.Text = "&Undo";
		this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
		this.redoToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Y | System.Windows.Forms.Keys.Control;
		this.redoToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
		this.redoToolStripMenuItem.Text = "&Redo";
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(162, 6);
		this.cutToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("cutToolStripMenuItem.Image");
		this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
		this.cutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.X | System.Windows.Forms.Keys.Control;
		this.cutToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
		this.cutToolStripMenuItem.Text = "Cu&t";
		this.copyToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("copyToolStripMenuItem.Image");
		this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
		this.copyToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.Control;
		this.copyToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
		this.copyToolStripMenuItem.Text = "&Copy";
		this.pasteToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("pasteToolStripMenuItem.Image");
		this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
		this.pasteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.V | System.Windows.Forms.Keys.Control;
		this.pasteToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
		this.pasteToolStripMenuItem.Text = "&Paste";
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(162, 6);
		this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
		this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(165, 24);
		this.selectAllToolStripMenuItem.Text = "Select &All";
		this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.customizeToolStripMenuItem, this.optionsToolStripMenuItem });
		this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
		this.toolsToolStripMenuItem.Size = new System.Drawing.Size(57, 24);
		this.toolsToolStripMenuItem.Text = "&Tools";
		this.customizeToolStripMenuItem.Name = "customizeToolStripMenuItem";
		this.customizeToolStripMenuItem.Size = new System.Drawing.Size(147, 24);
		this.customizeToolStripMenuItem.Text = "&Customize";
		this.customizeToolStripMenuItem.Checked = true;
		this.customizeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
		this.optionsToolStripMenuItem.Size = new System.Drawing.Size(147, 24);
		this.optionsToolStripMenuItem.Text = "&Options";
		this.optionsToolStripMenuItem.Checked = true;
		this.optionsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
		this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.contentsToolStripMenuItem, this.indexToolStripMenuItem, this.searchToolStripMenuItem, this.toolStripSeparator5, this.aboutToolStripMenuItem });
		this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
		this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
		this.helpToolStripMenuItem.Text = "&Help";
		this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
		this.contentsToolStripMenuItem.Size = new System.Drawing.Size(136, 24);
		this.contentsToolStripMenuItem.Text = "&Contents";
		this.indexToolStripMenuItem.Name = "indexToolStripMenuItem";
		this.indexToolStripMenuItem.Size = new System.Drawing.Size(136, 24);
		this.indexToolStripMenuItem.Text = "&Index";
		this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
		this.searchToolStripMenuItem.Size = new System.Drawing.Size(136, 24);
		this.searchToolStripMenuItem.Text = "&Search";
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(133, 6);
		this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
		this.aboutToolStripMenuItem.Size = new System.Drawing.Size(136, 24);
		this.aboutToolStripMenuItem.Text = "&About...";
		this.lblPreview.AutoSize = true;
		this.lblPreview.Location = new System.Drawing.Point(12, 6);
		this.lblPreview.Name = "lblPreview";
		this.lblPreview.Size = new System.Drawing.Size(61, 17);
		this.lblPreview.TabIndex = 3;
		this.lblPreview.Text = "Preview:";
		this.PropertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.PropertyGrid1.Location = new System.Drawing.Point(0, 0);
		this.PropertyGrid1.Name = "PropertyGrid1";
		this.PropertyGrid1.Size = new System.Drawing.Size(383, 316);
		this.PropertyGrid1.TabIndex = 4;
		this.PropertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(OnPropertyGrid1_PropertyValueChanged);
		this.dockPanel.ActiveAutoHideContent = null;
		this.dockPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dockPanel.Font = new System.Drawing.Font("Tahoma", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, 0);
		this.dockPanel.Location = new System.Drawing.Point(0, 0);
		this.dockPanel.Name = "dockPanel";
		this.dockPanel.Size = new System.Drawing.Size(335, 316);
		this.dockPanel.TabIndex = 5;
		this.buttonShowTabs.Location = new System.Drawing.Point(16, 122);
		this.buttonShowTabs.Name = "buttonShowTabs";
		this.buttonShowTabs.Size = new System.Drawing.Size(138, 34);
		this.buttonShowTabs.TabIndex = 7;
		this.buttonShowTabs.Text = "Show Tabs";
		this.buttonShowTabs.UseVisualStyleBackColor = true;
		this.buttonShowTabs.Click += new System.EventHandler(buttonShowTabs_Click);
		this.splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
		this.splitContainer1.Location = new System.Drawing.Point(16, 162);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.dockPanel);
		this.splitContainer1.Panel2.Controls.Add(this.PropertyGrid1);
		this.splitContainer1.Size = new System.Drawing.Size(722, 316);
		this.splitContainer1.SplitterDistance = 335;
		this.splitContainer1.TabIndex = 9;
		this.buttonResetTabsColors.Location = new System.Drawing.Point(175, 122);
		this.buttonResetTabsColors.Name = "buttonResetTabsColors";
		this.buttonResetTabsColors.Size = new System.Drawing.Size(143, 34);
		this.buttonResetTabsColors.TabIndex = 10;
		this.buttonResetTabsColors.Text = "Reset Tabs Colors";
		this.buttonResetTabsColors.UseVisualStyleBackColor = true;
		this.buttonResetTabsColors.Click += new System.EventHandler(OnButtonResetTabsColors_Click);
		this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.labelFileName });
		this.statusStrip1.Location = new System.Drawing.Point(0, 518);
		this.statusStrip1.Name = "statusStrip1";
		this.statusStrip1.Size = new System.Drawing.Size(751, 25);
		this.statusStrip1.TabIndex = 12;
		this.statusStrip1.Text = "statusStrip1";
		this.labelFileName.Name = "labelFileName";
		this.labelFileName.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
		this.labelFileName.Size = new System.Drawing.Size(79, 20);
		this.labelFileName.Text = "File Name:";
		base.AcceptButton = this.OK_Button;
		base.ClientSize = new System.Drawing.Size(751, 543);
		base.Controls.Add(this.statusStrip1);
		base.Controls.Add(this.buttonResetTabsColors);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.buttonShowTabs);
		base.Controls.Add(this.lblPreview);
		base.Controls.Add(this.Panel1);
		base.Controls.Add(this.TableLayoutPanel3);
		base.Controls.Add(this.TableLayoutPanel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.MainMenuStrip = this.CustomizableMenuStrip1;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "AppearanceEditor";
		base.ShowIcon = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Appearance Editor";
		base.Load += new System.EventHandler(AppearanceEditor_Load);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(OnFormClosing);
		this.TableLayoutPanel1.ResumeLayout(false);
		this.TableLayoutPanel2.ResumeLayout(false);
		this.TableLayoutPanel3.ResumeLayout(false);
		this.Panel1.ResumeLayout(false);
		this.Panel1.PerformLayout();
		this.CustomizableToolStrip1.ResumeLayout(false);
		this.CustomizableToolStrip1.PerformLayout();
		this.CustomizableStatusStrip1.ResumeLayout(false);
		this.CustomizableStatusStrip1.PerformLayout();
		this.CustomizableMenuStrip1.ResumeLayout(false);
		this.CustomizableMenuStrip1.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		this.splitContainer1.ResumeLayout(false);
		this.statusStrip1.ResumeLayout(false);
		this.statusStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public void SaveAppearance(string xmlFileName, AppearanceControl ac)
	{
		if (ac.SaveAppearanceProperties(xmlFileName))
		{
			dirty = false;
		}
	}

	public void LoadAppearance(string xmlFileName)
	{
		if (AppearanceProperties.Load(xmlFileName, out var appearanceProperties))
		{
			_ap = appearanceProperties;
			CustomizableMenuStrip1.Appearance.AppearanceProperties = _ap;
			CustomizableStatusStrip1.Appearance.AppearanceProperties = _ap;
			CustomizableToolStrip1.Appearance.AppearanceProperties = _ap;
			PropertyGrid1.SelectedObject = _ap;
			ShowDockContent();
		}
		CustomizableMenuStrip1.Appearance.LoadAppearanceProperties(xmlFileName);
	}

	private void OnOK_Button_Click(object sender, EventArgs e)
	{
		if (dirty && !string.IsNullOrEmpty(fileName) && MessageBox.Show("Do you want to save the edited color scheme", "Do you want to save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			SaveAppearance(fileName, AppearanceControl1);
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnPropertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
	{
		CustomizableMenuStrip1.Invalidate();
		CustomizableStatusStrip1.Invalidate();
		CustomizableToolStrip1.Invalidate();
	}

	private void OnSave_Button_Click(object sender, EventArgs e)
	{
		SaveAppearance(fileName, AppearanceControl1);
	}

	private void OnLoad_Button_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Title = "Select XML File.";
		openFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files|*.*";
		openFileDialog.InitialDirectory = ColorThemesListService.ThemeDirectoryPath;
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			LoadAppearance(openFileDialog.FileName);
			CustomizableMenuStrip1.Invalidate();
			CustomizableToolStrip1.Invalidate();
			CustomizableStatusStrip1.Invalidate();
			fileName = openFileDialog.FileName;
		}
	}

	private void SaveAs_Button_Click(object sender, EventArgs e)
	{
		using SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.Title = "Select XML File.";
		saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files|*.*";
		saveFileDialog.InitialDirectory = ColorThemesListService.ThemeDirectoryPath;
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			SaveAppearance(saveFileDialog.FileName, AppearanceControl1);
			fileName = saveFileDialog.FileName;
		}
	}

	private void AppearanceEditor_Load(object sender, EventArgs e)
	{
	}

	private void buttonShowTabs_Click(object sender, EventArgs e)
	{
		ShowDockContent();
	}

	private void OnButtonResetTabsColors_Click(object sender, EventArgs e)
	{
		AppearanceControl1.AppearanceProperties.DockTabStripAppearance.GradientBegin = AppearanceControl1.AppearanceProperties.StatusStripAppearance.GradientBegin;
		AppearanceControl1.AppearanceProperties.DockTabStripAppearance.GradientEnd = AppearanceControl1.AppearanceProperties.StatusStripAppearance.GradientEnd;
		AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.ActiveBackColorGradientBegin = AppearanceControl1.AppearanceProperties.MenuItemAppearance.PressedGradientBegin;
		AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.ActiveBackColorGradientEnd = AppearanceControl1.AppearanceProperties.MenuItemAppearance.PressedGradientEnd;
		AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.ActiveTextColor = AppearanceControl1.AppearanceProperties.MenuItemAppearance.PressedGradientEnd;
		AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.InactiveBackColor = AppearanceControl1.AppearanceProperties.MenuStripAppearance.GradientBegin;
		AppearanceControl1.AppearanceProperties.DockPadTitleAppearance.InactiveTextColor = AppearanceControl1.AppearanceProperties.MenuStripAppearance.GradientBegin;
		AppearanceControl1.AppearanceProperties.DockTabAppearance.Inactive.GradientBegin = AppearanceControl1.AppearanceProperties.RaftingContainerAppearance.GradientBegin;
		AppearanceControl1.AppearanceProperties.DockTabAppearance.Inactive.GradientEnd = AppearanceControl1.AppearanceProperties.RaftingContainerAppearance.GradientEnd;
		AppearanceControl1.AppearanceProperties.DockTabAppearance.Inactive.EdgeColor = AppearanceControl1.AppearanceProperties.ToolStripAppearance.Border;
		AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.GradientBegin = AppearanceControl1.AppearanceProperties.ToolStripAppearance.GradientBegin;
		AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.GradientEnd = AppearanceControl1.AppearanceProperties.ToolStripAppearance.GradientEnd;
		AppearanceControl1.AppearanceProperties.DockTabAppearance.Active.EdgeColor = AppearanceControl1.AppearanceProperties.MenuStripAppearance.Border;
		ShowDockContent();
	}
}
