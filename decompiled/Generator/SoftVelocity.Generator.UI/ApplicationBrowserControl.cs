using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using Clarion.GEN;
using CommonSources.Commands.TabStrip;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.SharpDevelop.Project.Commands;
using SoftVelocity.Common.DependencyEditor.Commands;
using SoftVelocity.Generator.Commands;
using SoftVelocity.Generator.Properties;
using SoftVelocity.Ide.Core;

namespace SoftVelocity.Generator.UI;

internal class ApplicationBrowserControl : UserControl, IHasPropertyContainer
{
	private PropertyContainer propertyContainer;

	private readonly bool OnlyEnterprise = (int)VersionService.Version == 1;

	private GenMakeSelection _generateSelection;

	private GenerationMode _generationModeConditional;

	private GenerationMode _generateTrace;

	private GenerationMode _runWithDebugger;

	private GenerationMode _generateBeforeBuild;

	private Properties prop = PropertyService.Get<Properties>("SoftVelocity.Generator.ApplicationService", new Properties());

	private string _fullLabelText = "";

	private IContainer components;

	internal TreeViewAdvBase applicationBrowserTree;

	private NodeIcon nodeIcon;

	private NodeTextBox nodeTextBox;

	private ToolStrip toolStrip1;

	private ToolStripButton editButton;

	private ToolStripButton exportToTextButton;

	private ToolStripButton createFromTxaButton;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton sortingSelectionButton;

	private ToolStripButton refreshGenerationSortButton;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripSplitButton generateButton;

	private ToolStripMenuItem generateAllCheck;

	private ToolStripMenuItem generateSelectedCheck;

	private ToolStripMenuItem generateEditedCheck;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripMenuItem generationModeDefaultCheck;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem generateTraceNoCheck;

	private ToolStripMenuItem generateTraceYesCheck;

	private ToolStripMenuItem generateTraceDefaultCheck;

	private ToolStripMenuItem generationModeConditionalCheck;

	private ToolStripMenuItem generationModeUnconditionalCheck;

	private TextBox generateModeString;

	private SplitContainer splitContainer1;

	private ToolStripButton viewDCTButton;

	private ToolStripSplitButton runButton;

	private ToolStripSeparator toolStripSeparator6;

	private ToolStripMenuItem runToolStripMenuItem;

	private ToolStripMenuItem runWithDebuggerToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator7;

	private ToolStripMenuItem MRgenerateAllCheck;

	private ToolStripMenuItem MRgenerateSelectedCheck;

	private ToolStripMenuItem MRgenerateEditedCheck;

	private ToolStripSeparator MRtoolStripSeparator4;

	private ToolStripMenuItem MRgenerationModeDefaultCheck;

	private ToolStripSeparator MRtoolStripSeparator5;

	private ToolStripMenuItem MRgenerateTraceNoCheck;

	private ToolStripMenuItem MRgenerateTraceYesCheck;

	private ToolStripMenuItem MRgenerateTraceDefaultCheck;

	private ToolStripMenuItem MRgenerationModeConditionalCheck;

	private ToolStripMenuItem MRgenerationModeUnconditionalCheck;

	private ToolStripSplitButton buildButton;

	private ToolStripMenuItem MBgenerateAllCheck;

	private ToolStripMenuItem MBgenerateSelectedCheck;

	private ToolStripMenuItem MBgenerateEditedCheck;

	private ToolStripSeparator MBtoolStripSeparator4;

	private ToolStripMenuItem MBgenerationModeDefaultCheck;

	private ToolStripSeparator MBtoolStripSeparator5;

	private ToolStripMenuItem MBgenerateTraceNoCheck;

	private ToolStripMenuItem MBgenerateTraceYesCheck;

	private ToolStripMenuItem MBgenerateTraceDefaultCheck;

	private ToolStripMenuItem MBgenerationModeConditionalCheck;

	private ToolStripMenuItem MBgenerationModeUnconditionalCheck;

	private ToolStripMenuItem generateAndBuildToolStripMenuItem;

	private ToolStripMenuItem buildToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem2;

	private ToolStripMenuItem generateOnlyToolStripMenuItem;

	private ToolStripSeparator toolStripSeparator8;

	private ToolStripButton openContainerFolder;

	private ToolStripButton runSelectedAppProject;

	private ToolStripButton dependencyEditorButton;

	private ToolStripButton cancelGenAndBuildButton;

	private Locator Locator;

	private ToolStripButton viewProjectButton;

	public PropertyContainer PropertyContainer => propertyContainer;

	public ApplicationBrowserControl()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Invalid comparison between Unknown and I4
		propertyContainer = new PropertyContainer();
		InitializeComponent();
		((Control)(object)applicationBrowserTree).Font = FontService.GetFont((FontType)1);
		splitContainer1.SplitterDistance = prop.Get<int>("ApplicationPadSplitter1", splitContainer1.SplitterDistance);
		((TreeViewAdv)(object)applicationBrowserTree).Model = ApplicationService.ApplicationBrowserTreeModel();
		nodeIcon.ValueNeeded += nodeIcon_ValueNeeded;
		nodeTextBox.ValueNeeded += nodeTextBox_ValueNeeded;
		((TreeViewAdv)(object)applicationBrowserTree).Model.StructureChanged += Model_StructureChanged;
		if ((int)VersionService.Version == 1)
		{
			generateAndBuildToolStripMenuItem.Text += " in Batch";
			buildToolStripMenuItem.Text += " in Batch";
		}
		resetLabelText();
		ProjectService.StartBuild += OnBuildStart;
		ProjectService.EndBuild += OnBuildEnd;
		ApplicationService.GenerationStarting += OnGenerationStarting;
		ApplicationService.GenerationEnded += OnGenerationEnded;
		base.Load += OnApplicationBrowserControl_Load;
	}

	private void OnApplicationBrowserControl_Load(object sender, EventArgs e)
	{
		ToolbarService.SetToolStripSize((object)base.ParentForm, toolStrip1);
	}

	internal void RedrawContent()
	{
		ToolbarService.SetToolStripSize((object)base.ParentForm, toolStrip1);
	}

	private void OnGenerationEnded(object sender, GenerationEndEventArgs e)
	{
		EnableButtons();
	}

	private void OnGenerationStarting(object sender, GenerationStartEventArgs e)
	{
		EnableButtons();
	}

	private void OnBuildEnd(object sender, EventArgs e)
	{
		EnableButtons();
	}

	private void OnBuildStart(object sender, EventArgs e)
	{
		EnableButtons();
	}

	internal ApplicationBrowserControl(bool withProperStrings)
		: this()
	{
		editButton.Text = ResourceService.GetString("Clarion.Generator.Pad.Buttons.Edit");
		exportToTextButton.Text = ResourceService.GetString("Clarion.Generator.Pad.Buttons.ExportToText");
		createFromTxaButton.Text = ResourceService.GetString("Clarion.Generator.Pad.Buttons.CreateFromTxa");
		refreshGenerationSortButton.Text = ResourceService.GetString("Clarion.Generator.Pad.Buttons.Refresh");
		sortingSelectionButton.Text = ResourceService.GetString("Clarion.Generator.Pad.Buttons.sortByPosition");
		dependencyEditorButton.Text = ResourceService.GetString("Clarion.Generator.Pad.Buttons.dependencyEditor");
		generateAllCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateAllCheckText");
		generateSelectedCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateSelectedCheckText");
		generateEditedCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateEditedCheckText");
		generateTraceNoCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateTraceNoCheckText");
		generateTraceYesCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateTraceYesCheckText");
		generateTraceDefaultCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateTraceDefaultCheckText");
		generationModeDefaultCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generationModeDefaultCheckText");
		generationModeConditionalCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generationModeConditionalCheckText");
		generationModeUnconditionalCheck.Text = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generationModeUnconditionalCheckText");
		EnableButtons();
	}

	private void Model_StructureChanged(object sender, TreePathEventArgs e)
	{
		if (ApplicationService.ApplicationsList.Count == 0)
		{
			propertyContainer.Clear();
		}
		sortingButtonImageRefresh();
		EnableButtons();
		if (((TreeViewAdv)(object)applicationBrowserTree).ItemCount > 0 && ((TreeViewAdv)(object)applicationBrowserTree).SelectedNode == null)
		{
			using IEnumerator<TreeNodeAdv> enumerator = ((TreeViewAdv)(object)applicationBrowserTree).AllNodes.GetEnumerator();
			if (enumerator.MoveNext())
			{
				TreeNodeAdv current = enumerator.Current;
				((TreeViewAdv)(object)applicationBrowserTree).SelectedNode = current;
			}
		}
		SetBottomText();
	}

	private void nodeIcon_ValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node != null && e.Node.Tag != null)
		{
			Application application = (Application)e.Node.Tag;
			if (application != null)
			{
				if (application.IsOnSolution)
				{
					if (application.IsLoaded)
					{
						e.Value = Resources.ApplicationProject_FileOpened;
					}
					else
					{
						e.Value = Resources.ApplicationProject_File;
					}
				}
				else
				{
					e.Value = Resources.Application_File;
				}
				return;
			}
		}
		e.Value = Resources.GenTDis;
	}

	private void nodeTextBox_ValueNeeded(object sender, NodeControlValueEventArgs e)
	{
		if (e.Node != null && e.Node.Tag != null)
		{
			Application application = (Application)e.Node.Tag;
			if (application != null)
			{
				if (!string.IsNullOrEmpty(application.TargetType))
				{
					if (ApplicationServiceSettings.AlwaysShowChangedDateTime)
					{
						e.Value = $"{application.Name} ({application.TargetType.ToUpperInvariant()}) - ({application.ModificationDate:G})";
					}
					else if (ApplicationService.ApplicationListCurrentSort == ApplicationService.ApplicationsSort.ByModificationDate)
					{
						e.Value = $"{application.Name} ({application.TargetType.ToUpperInvariant()}) - ({application.ModificationDate:G})";
					}
					else
					{
						e.Value = $"{application.Name} ({application.TargetType.ToUpperInvariant()})";
					}
				}
				else if (ApplicationServiceSettings.AlwaysShowChangedDateTime)
				{
					e.Value = $"{application.Name} - ({application.ModificationDate:G})";
				}
				else
				{
					e.Value = application.Name;
				}
			}
			else
			{
				e.Value = string.Empty;
			}
		}
		else
		{
			e.Value = string.Empty;
		}
	}

	private void EditButton_Click(object sender, EventArgs e)
	{
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNode != null && ((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag != null)
		{
			Application application = (Application)((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag;
			if (!application.IsBusy)
			{
				ApplicationService.EditApplication(application);
			}
		}
	}

	private void EnableButtons()
	{
		bool flag = false;
		bool flag2 = false;
		bool enabled = true;
		bool enabled2 = true;
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNode != null)
		{
			flag2 = true;
		}
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNodes != null && ((TreeViewAdv)(object)applicationBrowserTree).SelectedNodes.Count > 1)
		{
			flag = true;
		}
		if (((TreeViewAdv)(object)applicationBrowserTree).ItemCount == 0)
		{
			enabled2 = false;
			flag2 = false;
			enabled = false;
		}
		dependencyEditorButton.Enabled = ProjectService.OpenSolution != null && OnlyEnterprise;
		generateButton.Enabled = enabled2;
		runButton.Enabled = enabled2;
		sortingSelectionButton.Enabled = enabled;
		editButton.Enabled = flag2;
		exportToTextButton.Enabled = flag2;
		viewDCTButton.Enabled = flag2;
		viewProjectButton.Enabled = flag2;
		buildButton.Enabled = enabled2;
		openContainerFolder.Enabled = flag2 && !flag;
		runSelectedAppProject.Enabled = flag2 && !flag;
		if (ApplicationService.IsGenerating || ProjectService.IsBuilding)
		{
			cancelGenAndBuildButton.Enabled = true;
		}
		else
		{
			cancelGenAndBuildButton.Enabled = false;
		}
	}

	private void applicationBrowserTree_SelectionChanged(object sender, EventArgs e)
	{
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNode != null && ((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag != null)
		{
			Application app = (Application)((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag;
			ApplicationService.SelectApplicationProject(app);
			propertyContainer.SelectedObject = ((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag;
		}
		EnableButtons();
	}

	private void applicationBrowserTree_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && e.Node != null && e.Node.Tag != null)
		{
			Application app = (Application)e.Node.Tag;
			ApplicationService.EditApplication(app);
		}
	}

	private void ExportToTextButton_Click(object sender, EventArgs e)
	{
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNode == null || ((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag == null)
		{
			return;
		}
		Application application = (Application)((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag;
		if (application.IsBusy)
		{
			return;
		}
		using SoftVelocity.Ide.Core.SaveFileDialog saveFileDialog = FileDialogService.SaveFileDialog();
		string text = Path.ChangeExtension(application.FileName, ".txa");
		saveFileDialog.InitialDirectory = Path.GetDirectoryName(text);
		saveFileDialog.FileName = text;
		if (saveFileDialog.ShowDialog(WorkbenchSingleton.MainForm) == DialogResult.OK && application.ExportAll(saveFileDialog.FileName))
		{
			FileService.RecentOpen.AddLastItem(RecentOpen.defaultTypeFiles, FileUtility.NormalizePath(saveFileDialog.FileName), (Properties)null);
		}
	}

	private void CreateFromTxaButton_Click(object sender, EventArgs e)
	{
		string title = ResourceService.GetString("Clarion.Generator.CreateFromTxa.Title");
		string label = ResourceService.GetString("Clarion.Generator.CreateFromTxa.Txa");
		string label2 = ResourceService.GetString("Clarion.Generator.CreateFromTxa.App");
		string filter = StringParser.Parse(string.Join("|", (string[])AddInTree.GetTreeNode("/Clarion/Generator/FileFilter/Txa").BuildChildItems((object)null).ToArray(typeof(string))) + "|${res:SharpDevelop.FileFilter.AllFiles}|*.*");
		string filter2 = StringParser.Parse(string.Join("|", (string[])AddInTree.GetTreeNode("/Clarion/Generator/FileFilter/App").BuildChildItems((object)null).ToArray(typeof(string))) + "|${res:SharpDevelop.FileFilter.AllFiles}|*.*");
		using TwoFileForm twoFileForm = new TwoFileForm(title, label, filter, forOpen1: true, label2, filter2, forOpen2: false);
		twoFileForm.Owner = (Form)(object)WorkbenchSingleton.Workbench;
		if (twoFileForm.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(twoFileForm.File(1)) || string.IsNullOrEmpty(twoFileForm.File(2)))
		{
			return;
		}
		Win32App win32App = ApplicationService.NewAppFromTxa(twoFileForm.File(2), twoFileForm.File(1));
		if (win32App != null)
		{
			if (ProjectService.OpenSolution == null)
			{
				ProjectService.LoadSolutionOrProject(win32App.FileName);
			}
			else
			{
				AddExitingProjectToSolution.AddProject(win32App.FileName);
			}
		}
	}

	private void sortingSelectionButton_Click(object sender, EventArgs e)
	{
		switch (ApplicationService.ApplicationListCurrentSort)
		{
		case ApplicationService.ApplicationsSort.ByName:
			ApplicationService.ApplicationListCurrentSort = ApplicationService.ApplicationsSort.ByDependency;
			break;
		case ApplicationService.ApplicationsSort.ByDependency:
			ApplicationService.ApplicationListCurrentSort = ApplicationService.ApplicationsSort.ByModificationDate;
			break;
		case ApplicationService.ApplicationsSort.ByModificationDate:
			ApplicationService.ApplicationListCurrentSort = ApplicationService.ApplicationsSort.ByName;
			break;
		}
		sortingButtonImageRefresh();
		SetBottomText();
	}

	private void sortingButtonImageRefresh()
	{
		switch (ApplicationService.ApplicationListCurrentSort)
		{
		case ApplicationService.ApplicationsSort.ByName:
			sortingSelectionButton.Image = ApplicationService.GetApplicationsSortImage(ApplicationService.ApplicationsSort.ByDependency);
			sortingSelectionButton.Text = ApplicationService.GetApplicationsSortText(ApplicationService.ApplicationsSort.ByDependency);
			break;
		case ApplicationService.ApplicationsSort.ByDependency:
			sortingSelectionButton.Image = ApplicationService.GetApplicationsSortImage(ApplicationService.ApplicationsSort.ByModificationDate);
			sortingSelectionButton.Text = ApplicationService.GetApplicationsSortText(ApplicationService.ApplicationsSort.ByModificationDate);
			break;
		case ApplicationService.ApplicationsSort.ByModificationDate:
			sortingSelectionButton.Image = ApplicationService.GetApplicationsSortImage(ApplicationService.ApplicationsSort.ByName);
			sortingSelectionButton.Text = ApplicationService.GetApplicationsSortText(ApplicationService.ApplicationsSort.ByName);
			break;
		}
	}

	private void refreshGenerationSortButton_Click(object sender, EventArgs e)
	{
		RefreshSort();
	}

	internal void RefreshSort()
	{
		ApplicationService.ApplicationsSort applicationListCurrentSort = ApplicationService.ApplicationListCurrentSort;
		ApplicationService.ApplicationListCurrentSort = ApplicationService.ApplicationsSort.Unknown;
		if (applicationListCurrentSort != ApplicationService.ApplicationsSort.Unknown)
		{
			ApplicationService.ApplicationListCurrentSort = applicationListCurrentSort;
		}
		else
		{
			ApplicationService.ApplicationListCurrentSort = ApplicationService.ApplicationsSort.ByName;
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F1)
		{
			string parameter = GetType().FullName.Replace('.', '_') + ".htm";
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			FileInfo fileInfo = new FileInfo(entryAssembly.Location);
			string text = Path.Combine(fileInfo.DirectoryName, "ClarionHelp.chm");
			if (File.Exists(text))
			{
				Help.ShowHelp(WorkbenchSingleton.helpHost, text, HelpNavigator.Topic, parameter);
			}
			else
			{
				MessageService.ShowWarning("${res:MainWindow.Windows.HtmlHelp.NotFound} " + text);
			}
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void generateButton_ButtonClick(object sender, EventArgs e)
	{
		PressGenerateButton();
	}

	private void generateAllCheck_Click(object sender, EventArgs e)
	{
		_generateSelection = GenMakeSelection.All;
		changeLabelText();
	}

	private void generateSelectedCheck_Click(object sender, EventArgs e)
	{
		_generateSelection = GenMakeSelection.Selected;
		changeLabelText();
	}

	private void generateEditedCheck_Click(object sender, EventArgs e)
	{
		_generateSelection = GenMakeSelection.Edited;
		changeLabelText();
	}

	private void generationModeDefaultCheck_Click(object sender, EventArgs e)
	{
		_generationModeConditional = GenerationMode.GlobalOption;
		changeLabelText();
	}

	private void generationModeConditionalCheck_Click(object sender, EventArgs e)
	{
		_generationModeConditional = GenerationMode.On;
		changeLabelText();
	}

	private void generationModeUnconditionalCheck_Click(object sender, EventArgs e)
	{
		_generationModeConditional = GenerationMode.Off;
		changeLabelText();
	}

	private void generateTraceDefaultCheck_Click(object sender, EventArgs e)
	{
		_generateTrace = GenerationMode.GlobalOption;
		changeLabelText();
	}

	private void generateTraceNoCheck_Click(object sender, EventArgs e)
	{
		_generateTrace = GenerationMode.Off;
		changeLabelText();
	}

	private void generateTraceYesCheck_Click(object sender, EventArgs e)
	{
		_generateTrace = GenerationMode.On;
		changeLabelText();
	}

	private void resetLabelText()
	{
		_generationModeConditional = prop.Get<GenerationMode>("generationModeConditional", GenerationMode.GlobalOption);
		_generateTrace = prop.Get<GenerationMode>("generateTrace", GenerationMode.GlobalOption);
		_generateSelection = prop.Get<GenMakeSelection>("generateSelection", GenMakeSelection.All);
		_runWithDebugger = prop.Get<GenerationMode>("makeAndRunWihtDebuger", GenerationMode.Off);
		_generateBeforeBuild = prop.Get<GenerationMode>("generateBeforeBuild", GenerationMode.On);
		changeLabelText();
	}

	private void changeLabelText()
	{
		runToolStripMenuItem.Checked = false;
		runWithDebuggerToolStripMenuItem.Checked = false;
		switch (_runWithDebugger)
		{
		case GenerationMode.Off:
			runToolStripMenuItem.Checked = true;
			runButton.Image = Resources.GenerateAndRunExe;
			runButton.Text = "Generate and Run Application";
			break;
		case GenerationMode.On:
			runWithDebuggerToolStripMenuItem.Checked = true;
			runButton.Image = Resources.GenerateAndDebugExe;
			runButton.Text = "Generate and Debug Application";
			break;
		}
		string arg = null;
		string arg2 = null;
		string arg3 = null;
		generateAllCheck.Checked = false;
		generateEditedCheck.Checked = false;
		generateSelectedCheck.Checked = false;
		string text;
		switch (_generateSelection)
		{
		case GenMakeSelection.All:
			generateAllCheck.Checked = true;
			arg = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateAllText");
			text = "All";
			break;
		case GenMakeSelection.Selected:
			generateSelectedCheck.Checked = true;
			arg = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateSelectedText");
			text = "Selected";
			break;
		case GenMakeSelection.Edited:
			generateEditedCheck.Checked = true;
			arg = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateEditedText");
			text = "Edited";
			break;
		case GenMakeSelection.Current:
			generateSelectedCheck.Checked = true;
			arg = null;
			text = "Current";
			break;
		default:
			text = "";
			break;
		}
		generateAndBuildToolStripMenuItem.Checked = false;
		buildToolStripMenuItem.Checked = false;
		generateOnlyToolStripMenuItem.Checked = false;
		switch (_generateBeforeBuild)
		{
		case GenerationMode.GlobalOption:
			generateOnlyToolStripMenuItem.Checked = true;
			buildButton.Image = Resources.GenerateAll;
			buildButton.Text = generateOnlyToolStripMenuItem.Text + "(" + text + ")";
			MBgenerateAllCheck.Text = "Generate All";
			MBgenerateSelectedCheck.Text = "Generate Selected";
			MBgenerateEditedCheck.Text = "Generate Edited";
			break;
		case GenerationMode.Off:
			buildToolStripMenuItem.Checked = true;
			buildButton.Image = Resources.Build;
			buildButton.Text = buildToolStripMenuItem.Text + "(" + text + ")";
			MBgenerateAllCheck.Text = "Build All";
			MBgenerateSelectedCheck.Text = "Build Selected";
			MBgenerateEditedCheck.Text = "Build Edited";
			break;
		case GenerationMode.On:
			generateAndBuildToolStripMenuItem.Checked = true;
			buildButton.Image = Resources.GenerateAndMake;
			buildButton.Text = generateAndBuildToolStripMenuItem.Text + "(" + text + ")";
			MBgenerateAllCheck.Text = "Generate and Build All";
			MBgenerateSelectedCheck.Text = "Generate and Build Selected";
			MBgenerateEditedCheck.Text = "Generate and Build Edited";
			break;
		}
		buildButton.ToolTipText = buildButton.Text;
		generateTraceNoCheck.Checked = false;
		generateTraceYesCheck.Checked = false;
		generateTraceDefaultCheck.Checked = false;
		switch (_generateTrace)
		{
		case GenerationMode.GlobalOption:
			generateTraceDefaultCheck.Checked = true;
			arg3 = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateTraceDefaultText");
			break;
		case GenerationMode.Off:
			generateTraceNoCheck.Checked = true;
			arg3 = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateTraceNoText");
			break;
		case GenerationMode.On:
			generateTraceYesCheck.Checked = true;
			arg3 = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generateTraceYesText");
			break;
		}
		generationModeDefaultCheck.Checked = false;
		generationModeUnconditionalCheck.Checked = false;
		generationModeConditionalCheck.Checked = false;
		switch (_generationModeConditional)
		{
		case GenerationMode.GlobalOption:
			arg2 = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generationModeDefaultText");
			generationModeDefaultCheck.Checked = true;
			break;
		case GenerationMode.Off:
			arg2 = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generationModeUnconditionalText");
			generationModeUnconditionalCheck.Checked = true;
			break;
		case GenerationMode.On:
			arg2 = ResourceService.GetString("Clarion.Generator.Pad.MenuItem.generationModeConditionalText");
			generationModeConditionalCheck.Checked = true;
			break;
		}
		prop.Set<GenerationMode>("makeAndRunWihtDebuger", _runWithDebugger);
		prop.Set<GenerationMode>("generationModeConditional", _generationModeConditional);
		prop.Set<GenerationMode>("generateTrace", _generateTrace);
		prop.Set<GenMakeSelection>("generateSelection", _generateSelection);
		prop.Set<GenerationMode>("generateBeforeBuild", _generateBeforeBuild);
		_fullLabelText = string.Format(ResourceService.GetString("Clarion.Generator.Pad.Buttons.GenerateText"), arg, arg2, arg3);
		generateButton.ToolTipText = _fullLabelText;
		SetBottomText();
		MRgenerateAllCheck.Checked = generateAllCheck.Checked;
		MRgenerateSelectedCheck.Checked = generateSelectedCheck.Checked;
		MRgenerateEditedCheck.Checked = generateEditedCheck.Checked;
		MRgenerationModeDefaultCheck.Checked = generationModeDefaultCheck.Checked;
		MRgenerationModeConditionalCheck.Checked = generationModeConditionalCheck.Checked;
		MRgenerationModeUnconditionalCheck.Checked = generationModeUnconditionalCheck.Checked;
		MRgenerateTraceDefaultCheck.Checked = generateTraceDefaultCheck.Checked;
		MRgenerateTraceNoCheck.Checked = generateTraceNoCheck.Checked;
		MRgenerateTraceYesCheck.Checked = generateTraceYesCheck.Checked;
		MBgenerateAllCheck.Checked = generateAllCheck.Checked;
		MBgenerateSelectedCheck.Checked = generateSelectedCheck.Checked;
		MBgenerateEditedCheck.Checked = generateEditedCheck.Checked;
		MBgenerationModeDefaultCheck.Checked = generationModeDefaultCheck.Checked;
		MBgenerationModeConditionalCheck.Checked = generationModeConditionalCheck.Checked;
		MBgenerationModeUnconditionalCheck.Checked = generationModeUnconditionalCheck.Checked;
		MBgenerateTraceDefaultCheck.Checked = generateTraceDefaultCheck.Checked;
		MBgenerateTraceNoCheck.Checked = generateTraceNoCheck.Checked;
		MBgenerateTraceYesCheck.Checked = generateTraceYesCheck.Checked;
		if (buildToolStripMenuItem.Checked)
		{
			MBgenerationModeDefaultCheck.Enabled = false;
			MBgenerationModeConditionalCheck.Enabled = false;
			MBgenerationModeUnconditionalCheck.Enabled = false;
			MBgenerateTraceDefaultCheck.Enabled = false;
			MBgenerateTraceNoCheck.Enabled = false;
			MBgenerateTraceYesCheck.Enabled = false;
		}
		else
		{
			MBgenerationModeDefaultCheck.Enabled = true;
			MBgenerationModeConditionalCheck.Enabled = true;
			MBgenerationModeUnconditionalCheck.Enabled = true;
			MBgenerateTraceDefaultCheck.Enabled = true;
			MBgenerateTraceNoCheck.Enabled = true;
			MBgenerateTraceYesCheck.Enabled = true;
		}
	}

	private void SetBottomText()
	{
		if (((TreeViewAdv)(object)applicationBrowserTree).ItemCount > 0)
		{
			generateModeString.Text = _fullLabelText + "." + Environment.NewLine + ApplicationService.GetApplicationsSortText(ApplicationService.ApplicationListCurrentSort);
		}
		else
		{
			generateModeString.Text = "";
		}
	}

	private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
	{
		prop.Set<int>("ApplicationPadSplitter1", splitContainer1.SplitterDistance);
	}

	private void OnViewDCTButtonClick(object sender, EventArgs e)
	{
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNode != null)
		{
			Application app = (Application)((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag;
			ApplicationService.OpenDictionary(app);
		}
	}

	private void runButton_ButtonClick(object sender, EventArgs e)
	{
		PressGenerateAndRundButton();
	}

	private void runToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_runWithDebugger = GenerationMode.Off;
		changeLabelText();
	}

	private void runWithDebuggerToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_runWithDebugger = GenerationMode.On;
		changeLabelText();
	}

	private void generateOnlyToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_generateBeforeBuild = GenerationMode.GlobalOption;
		changeLabelText();
	}

	private void generateAndBuildToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_generateBeforeBuild = GenerationMode.On;
		changeLabelText();
	}

	private void buildToolStripMenuItem_Click(object sender, EventArgs e)
	{
		_generateBeforeBuild = GenerationMode.Off;
		changeLabelText();
	}

	private void buildButton_ButtonClick(object sender, EventArgs e)
	{
		PressBuildButton();
	}

	internal void PressGenerateButton()
	{
		ApplicationService.GenMakeApplications(_generateSelection, PosGenerationAction.None, _generationModeConditional, _generateTrace);
	}

	internal void PressBuildButton()
	{
		switch (_generateBeforeBuild)
		{
		case GenerationMode.GlobalOption:
			ApplicationService.GenMakeApplications(generate: true, _generateSelection, PosGenerationAction.None, _generationModeConditional, _generateTrace);
			break;
		case GenerationMode.Off:
			ApplicationService.GenMakeApplications(generate: false, _generateSelection, PosGenerationAction.BatchCompile, _generationModeConditional, _generateTrace);
			break;
		case GenerationMode.On:
			ApplicationService.GenMakeApplications(generate: true, _generateSelection, PosGenerationAction.BatchCompile, _generationModeConditional, _generateTrace);
			break;
		}
	}

	internal void PressGenerateAndRundButton()
	{
		if (_runWithDebugger == GenerationMode.Off)
		{
			ApplicationService.GenMakeApplications(_generateSelection, PosGenerationAction.CompileAndRun, _generationModeConditional, _generateTrace);
		}
		else
		{
			ApplicationService.GenMakeApplications(_generateSelection, PosGenerationAction.CompileAndRunDebug, _generationModeConditional, _generateTrace);
		}
	}

	private void openContainerFolder_Click(object sender, EventArgs e)
	{
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNode != null)
		{
			Application application = (Application)((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag;
			OpenContainingFolder.Run(application.FileName);
		}
	}

	private void runSelectedAppProject_Click(object sender, EventArgs e)
	{
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNode != null)
		{
			Application application = (Application)((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag;
			IProject project = ProjectService.GetProject(application.NameToProjectName());
			if (project != null)
			{
				AbstractRunProjectMenuCommand.RunCurrentProject(project, useDebug: false, fallbackToStartUp: true);
			}
		}
	}

	private void dependencyEditorButton_Click(object sender, EventArgs e)
	{
		OpenProjectDependencyEditorFromSelectedCommand.OpenDependencyEditor();
		if (ApplicationService.ApplicationListCurrentSort == ApplicationService.ApplicationsSort.ByDependency)
		{
			ApplicationService.ApplicationListCurrentSort = ApplicationService.ApplicationsSort.Unknown;
			ApplicationService.ApplicationListCurrentSort = ApplicationService.ApplicationsSort.ByDependency;
		}
	}

	private void cancelGenAndBuildButton_Click(object sender, EventArgs e)
	{
		CancelGenerationAndBuild.DoRun();
	}

	private void OnViewProjectButtonClick(object sender, EventArgs e)
	{
		if (((TreeViewAdv)(object)applicationBrowserTree).SelectedNode != null)
		{
			Application app = (Application)((TreeViewAdv)(object)applicationBrowserTree).SelectedNode.Tag;
			OpenCurrentApplicationProjectMenuCommand.OpenApplicationProject(app);
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		this.applicationBrowserTree = new TreeViewAdvBase();
		this.nodeIcon = new Aga.Controls.Tree.NodeControls.NodeIcon();
		this.nodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
		this.Locator = new Aga.Controls.Tree.Locator();
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.editButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.generateButton = new System.Windows.Forms.ToolStripSplitButton();
		this.generateAllCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.generateSelectedCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.generateEditedCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.generationModeDefaultCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.generationModeConditionalCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.generationModeUnconditionalCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.generateTraceDefaultCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.generateTraceNoCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.generateTraceYesCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.buildButton = new System.Windows.Forms.ToolStripSplitButton();
		this.generateOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.generateAndBuildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
		this.MBgenerateAllCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MBgenerateSelectedCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MBgenerateEditedCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MBtoolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.MBgenerationModeDefaultCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MBgenerationModeConditionalCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MBgenerationModeUnconditionalCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MBtoolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.MBgenerateTraceDefaultCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MBgenerateTraceNoCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MBgenerateTraceYesCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.runButton = new System.Windows.Forms.ToolStripSplitButton();
		this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.runWithDebuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
		this.MRgenerateAllCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MRgenerateSelectedCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MRgenerateEditedCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MRtoolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.MRgenerationModeDefaultCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MRgenerationModeConditionalCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MRgenerationModeUnconditionalCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MRtoolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.MRgenerateTraceDefaultCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MRgenerateTraceNoCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.MRgenerateTraceYesCheck = new System.Windows.Forms.ToolStripMenuItem();
		this.cancelGenAndBuildButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
		this.runSelectedAppProject = new System.Windows.Forms.ToolStripButton();
		this.openContainerFolder = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.viewDCTButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
		this.sortingSelectionButton = new System.Windows.Forms.ToolStripButton();
		this.dependencyEditorButton = new System.Windows.Forms.ToolStripButton();
		this.refreshGenerationSortButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.createFromTxaButton = new System.Windows.Forms.ToolStripButton();
		this.exportToTextButton = new System.Windows.Forms.ToolStripButton();
		this.generateModeString = new System.Windows.Forms.TextBox();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.viewProjectButton = new System.Windows.Forms.ToolStripButton();
		this.toolStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		base.SuspendLayout();
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).BackColor = System.Drawing.SystemColors.Window;
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).Cursor = System.Windows.Forms.Cursors.Default;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).DefaultToolTipProvider = null;
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).Dock = System.Windows.Forms.DockStyle.Fill;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).DragDropMarkColor = System.Drawing.Color.Black;
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).Font = new System.Drawing.Font("Segoe UI", 15f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).FullRowSelect = true;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).GoToLastWhenClickBelowLast = false;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).InactiveRowColor = System.Drawing.SystemColors.ControlLight;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).Indent = 0;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).LineColor = System.Drawing.SystemColors.ControlDark;
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).Location = new System.Drawing.Point(0, 0);
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).Margin = new System.Windows.Forms.Padding(4);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).Model = null;
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).Name = "applicationBrowserTree";
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).NodeControls.Add(this.nodeIcon);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).NodeControls.Add(this.nodeTextBox);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).SelectedNode = null;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).SelectedRowColor = System.Drawing.SystemColors.Highlight;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).SelectionMode = Aga.Controls.Tree.TreeSelectionMode.Multi;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).ShowLines = false;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).ShowPlusMinus = false;
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).Size = new System.Drawing.Size(432, 182);
		((System.Windows.Forms.Control)(object)this.applicationBrowserTree).TabIndex = 4;
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(applicationBrowserTree_NodeMouseDoubleClick);
		((Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree).SelectionChanged += new System.EventHandler(applicationBrowserTree_SelectionChanged);
		this.nodeIcon.LeftMargin = 1;
		this.nodeIcon.ParentColumn = null;
		this.nodeIcon.VirtualMode = true;
		this.nodeTextBox.DataPropertyName = "Name";
		this.nodeTextBox.EditEnabled = false;
		this.nodeTextBox.IncrementalSearchEnabled = true;
		this.nodeTextBox.LeftMargin = 3;
		this.nodeTextBox.ParentColumn = null;
		this.nodeTextBox.VirtualMode = true;
		this.Locator.Dock = System.Windows.Forms.DockStyle.Top;
		this.Locator.InString = true;
		this.Locator.Location = new System.Drawing.Point(0, 31);
		this.Locator.Margin = new System.Windows.Forms.Padding(0);
		this.Locator.Name = "Locator";
		this.Locator.Size = new System.Drawing.Size(432, 28);
		this.Locator.TabIndex = 0;
		this.Locator.TreeToSearch = (Aga.Controls.Tree.TreeViewAdv)(object)this.applicationBrowserTree;
		this.toolStrip1.AutoSize = false;
		this.toolStrip1.GripMargin = new System.Windows.Forms.Padding(0);
		this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[19]
		{
			this.editButton, this.toolStripSeparator2, this.generateButton, this.buildButton, this.runButton, this.cancelGenAndBuildButton, this.toolStripSeparator8, this.runSelectedAppProject, this.openContainerFolder, this.toolStripSeparator1,
			this.viewDCTButton, this.viewProjectButton, this.toolStripSeparator6, this.sortingSelectionButton, this.dependencyEditorButton, this.refreshGenerationSortButton, this.toolStripSeparator3, this.createFromTxaButton, this.exportToTextButton
		});
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Padding = new System.Windows.Forms.Padding(5, 0, 5, 0);
		this.toolStrip1.Size = new System.Drawing.Size(432, 31);
		this.toolStrip1.Stretch = true;
		this.toolStrip1.TabIndex = 2;
		this.toolStrip1.Text = "toolStrip1";
		this.editButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.editButton.Enabled = false;
		this.editButton.Image = SoftVelocity.Generator.Properties.Resources.EditApp;
		this.editButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.editButton.Name = "editButton";
		this.editButton.Size = new System.Drawing.Size(28, 28);
		this.editButton.Text = "Edit";
		this.editButton.Click += new System.EventHandler(EditButton_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
		this.generateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.generateButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[11]
		{
			this.generateAllCheck, this.generateSelectedCheck, this.generateEditedCheck, this.toolStripSeparator4, this.generationModeDefaultCheck, this.generationModeConditionalCheck, this.generationModeUnconditionalCheck, this.toolStripSeparator5, this.generateTraceDefaultCheck, this.generateTraceNoCheck,
			this.generateTraceYesCheck
		});
		this.generateButton.Image = SoftVelocity.Generator.Properties.Resources.Generate;
		this.generateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.generateButton.Name = "generateButton";
		this.generateButton.Size = new System.Drawing.Size(40, 28);
		this.generateButton.Text = "Generate";
		this.generateButton.ButtonClick += new System.EventHandler(generateButton_ButtonClick);
		this.generateAllCheck.Name = "generateAllCheck";
		this.generateAllCheck.Size = new System.Drawing.Size(480, 24);
		this.generateAllCheck.Text = "Generate All";
		this.generateAllCheck.Click += new System.EventHandler(generateAllCheck_Click);
		this.generateSelectedCheck.Name = "generateSelectedCheck";
		this.generateSelectedCheck.Size = new System.Drawing.Size(480, 24);
		this.generateSelectedCheck.Text = "Generate Selected";
		this.generateSelectedCheck.Click += new System.EventHandler(generateSelectedCheck_Click);
		this.generateEditedCheck.Name = "generateEditedCheck";
		this.generateEditedCheck.Size = new System.Drawing.Size(480, 24);
		this.generateEditedCheck.Text = "Generate Edited";
		this.generateEditedCheck.Click += new System.EventHandler(generateEditedCheck_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(477, 6);
		this.generationModeDefaultCheck.Name = "generationModeDefaultCheck";
		this.generationModeDefaultCheck.Size = new System.Drawing.Size(480, 24);
		this.generationModeDefaultCheck.Text = "Conditional Generation Mode - Default (Application Setting)";
		this.generationModeDefaultCheck.Click += new System.EventHandler(generationModeDefaultCheck_Click);
		this.generationModeConditionalCheck.Name = "generationModeConditionalCheck";
		this.generationModeConditionalCheck.Size = new System.Drawing.Size(480, 24);
		this.generationModeConditionalCheck.Text = "Conditional Generation";
		this.generationModeConditionalCheck.Click += new System.EventHandler(generationModeConditionalCheck_Click);
		this.generationModeUnconditionalCheck.Name = "generationModeUnconditionalCheck";
		this.generationModeUnconditionalCheck.Size = new System.Drawing.Size(480, 24);
		this.generationModeUnconditionalCheck.Text = "Unconditional Generation";
		this.generationModeUnconditionalCheck.Click += new System.EventHandler(generationModeUnconditionalCheck_Click);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(477, 6);
		this.generateTraceDefaultCheck.Name = "generateTraceDefaultCheck";
		this.generateTraceDefaultCheck.Size = new System.Drawing.Size(480, 24);
		this.generateTraceDefaultCheck.Text = "Generate Trace File - Default (Application Setting)";
		this.generateTraceDefaultCheck.Click += new System.EventHandler(generateTraceDefaultCheck_Click);
		this.generateTraceNoCheck.Name = "generateTraceNoCheck";
		this.generateTraceNoCheck.Size = new System.Drawing.Size(480, 24);
		this.generateTraceNoCheck.Text = "Don't Generate Trace File";
		this.generateTraceNoCheck.Click += new System.EventHandler(generateTraceNoCheck_Click);
		this.generateTraceYesCheck.Name = "generateTraceYesCheck";
		this.generateTraceYesCheck.Size = new System.Drawing.Size(480, 24);
		this.generateTraceYesCheck.Text = "Generate Trace File";
		this.generateTraceYesCheck.Click += new System.EventHandler(generateTraceYesCheck_Click);
		this.buildButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.buildButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[15]
		{
			this.generateOnlyToolStripMenuItem, this.buildToolStripMenuItem, this.generateAndBuildToolStripMenuItem, this.toolStripMenuItem2, this.MBgenerateAllCheck, this.MBgenerateSelectedCheck, this.MBgenerateEditedCheck, this.MBtoolStripSeparator4, this.MBgenerationModeDefaultCheck, this.MBgenerationModeConditionalCheck,
			this.MBgenerationModeUnconditionalCheck, this.MBtoolStripSeparator5, this.MBgenerateTraceDefaultCheck, this.MBgenerateTraceNoCheck, this.MBgenerateTraceYesCheck
		});
		this.buildButton.Image = SoftVelocity.Generator.Properties.Resources.Build;
		this.buildButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buildButton.Name = "buildButton";
		this.buildButton.Size = new System.Drawing.Size(40, 28);
		this.buildButton.Text = "toolStripSplitButton1";
		this.buildButton.ToolTipText = "Build";
		this.buildButton.ButtonClick += new System.EventHandler(buildButton_ButtonClick);
		this.generateOnlyToolStripMenuItem.Name = "generateOnlyToolStripMenuItem";
		this.generateOnlyToolStripMenuItem.Size = new System.Drawing.Size(480, 24);
		this.generateOnlyToolStripMenuItem.Text = "Generate";
		this.generateOnlyToolStripMenuItem.Click += new System.EventHandler(generateOnlyToolStripMenuItem_Click);
		this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
		this.buildToolStripMenuItem.Size = new System.Drawing.Size(480, 24);
		this.buildToolStripMenuItem.Text = "Build";
		this.buildToolStripMenuItem.Click += new System.EventHandler(buildToolStripMenuItem_Click);
		this.generateAndBuildToolStripMenuItem.Name = "generateAndBuildToolStripMenuItem";
		this.generateAndBuildToolStripMenuItem.Size = new System.Drawing.Size(480, 24);
		this.generateAndBuildToolStripMenuItem.Text = "Generate and Build";
		this.generateAndBuildToolStripMenuItem.Click += new System.EventHandler(generateAndBuildToolStripMenuItem_Click);
		this.toolStripMenuItem2.Name = "toolStripMenuItem2";
		this.toolStripMenuItem2.Size = new System.Drawing.Size(477, 6);
		this.MBgenerateAllCheck.Name = "MBgenerateAllCheck";
		this.MBgenerateAllCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerateAllCheck.Text = "Process All";
		this.MBgenerateAllCheck.Click += new System.EventHandler(generateAllCheck_Click);
		this.MBgenerateSelectedCheck.Name = "MBgenerateSelectedCheck";
		this.MBgenerateSelectedCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerateSelectedCheck.Text = "Process Selected";
		this.MBgenerateSelectedCheck.Click += new System.EventHandler(generateSelectedCheck_Click);
		this.MBgenerateEditedCheck.Name = "MBgenerateEditedCheck";
		this.MBgenerateEditedCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerateEditedCheck.Text = "Process Edited";
		this.MBgenerateEditedCheck.Click += new System.EventHandler(generateEditedCheck_Click);
		this.MBtoolStripSeparator4.Name = "MBtoolStripSeparator4";
		this.MBtoolStripSeparator4.Size = new System.Drawing.Size(477, 6);
		this.MBgenerationModeDefaultCheck.Name = "MBgenerationModeDefaultCheck";
		this.MBgenerationModeDefaultCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerationModeDefaultCheck.Text = "Conditional Generation Mode - Default (Application Setting)";
		this.MBgenerationModeDefaultCheck.Click += new System.EventHandler(generationModeDefaultCheck_Click);
		this.MBgenerationModeConditionalCheck.Name = "MBgenerationModeConditionalCheck";
		this.MBgenerationModeConditionalCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerationModeConditionalCheck.Text = "Conditional Generation";
		this.MBgenerationModeConditionalCheck.Click += new System.EventHandler(generationModeConditionalCheck_Click);
		this.MBgenerationModeUnconditionalCheck.Name = "MBgenerationModeUnconditionalCheck";
		this.MBgenerationModeUnconditionalCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerationModeUnconditionalCheck.Text = "Unconditional Generation";
		this.MBgenerationModeUnconditionalCheck.Click += new System.EventHandler(generationModeUnconditionalCheck_Click);
		this.MBtoolStripSeparator5.Name = "MBtoolStripSeparator5";
		this.MBtoolStripSeparator5.Size = new System.Drawing.Size(477, 6);
		this.MBgenerateTraceDefaultCheck.Name = "MBgenerateTraceDefaultCheck";
		this.MBgenerateTraceDefaultCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerateTraceDefaultCheck.Text = "Generate Trace File - Default (Application Setting)";
		this.MBgenerateTraceDefaultCheck.Click += new System.EventHandler(generateTraceDefaultCheck_Click);
		this.MBgenerateTraceNoCheck.Name = "MBgenerateTraceNoCheck";
		this.MBgenerateTraceNoCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerateTraceNoCheck.Text = "Don't Generate Trace File";
		this.MBgenerateTraceNoCheck.Click += new System.EventHandler(generateTraceNoCheck_Click);
		this.MBgenerateTraceYesCheck.Name = "MBgenerateTraceYesCheck";
		this.MBgenerateTraceYesCheck.Size = new System.Drawing.Size(480, 24);
		this.MBgenerateTraceYesCheck.Text = "Generate Trace File";
		this.MBgenerateTraceYesCheck.Click += new System.EventHandler(generateTraceYesCheck_Click);
		this.runButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.runButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[14]
		{
			this.runToolStripMenuItem, this.runWithDebuggerToolStripMenuItem, this.toolStripSeparator7, this.MRgenerateAllCheck, this.MRgenerateSelectedCheck, this.MRgenerateEditedCheck, this.MRtoolStripSeparator4, this.MRgenerationModeDefaultCheck, this.MRgenerationModeConditionalCheck, this.MRgenerationModeUnconditionalCheck,
			this.MRtoolStripSeparator5, this.MRgenerateTraceDefaultCheck, this.MRgenerateTraceNoCheck, this.MRgenerateTraceYesCheck
		});
		this.runButton.Image = SoftVelocity.Generator.Properties.Resources.GenerateAndDebugExe;
		this.runButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.runButton.Name = "runButton";
		this.runButton.Size = new System.Drawing.Size(40, 28);
		this.runButton.Text = "Run Application";
		this.runButton.ButtonClick += new System.EventHandler(runButton_ButtonClick);
		this.runToolStripMenuItem.Name = "runToolStripMenuItem";
		this.runToolStripMenuItem.Size = new System.Drawing.Size(480, 24);
		this.runToolStripMenuItem.Text = "Run";
		this.runToolStripMenuItem.Click += new System.EventHandler(runToolStripMenuItem_Click);
		this.runWithDebuggerToolStripMenuItem.Name = "runWithDebuggerToolStripMenuItem";
		this.runWithDebuggerToolStripMenuItem.Size = new System.Drawing.Size(480, 24);
		this.runWithDebuggerToolStripMenuItem.Text = "Run with Debugger";
		this.runWithDebuggerToolStripMenuItem.Click += new System.EventHandler(runWithDebuggerToolStripMenuItem_Click);
		this.toolStripSeparator7.Name = "toolStripSeparator7";
		this.toolStripSeparator7.Size = new System.Drawing.Size(477, 6);
		this.MRgenerateAllCheck.Name = "MRgenerateAllCheck";
		this.MRgenerateAllCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerateAllCheck.Text = "Generate and Build All";
		this.MRgenerateAllCheck.Click += new System.EventHandler(generateAllCheck_Click);
		this.MRgenerateSelectedCheck.Name = "MRgenerateSelectedCheck";
		this.MRgenerateSelectedCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerateSelectedCheck.Text = "Generate and Build Selected";
		this.MRgenerateSelectedCheck.Click += new System.EventHandler(generateSelectedCheck_Click);
		this.MRgenerateEditedCheck.Name = "MRgenerateEditedCheck";
		this.MRgenerateEditedCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerateEditedCheck.Text = "Generate and Build Edited";
		this.MRgenerateEditedCheck.Click += new System.EventHandler(generateEditedCheck_Click);
		this.MRtoolStripSeparator4.Name = "MRtoolStripSeparator4";
		this.MRtoolStripSeparator4.Size = new System.Drawing.Size(477, 6);
		this.MRgenerationModeDefaultCheck.Name = "MRgenerationModeDefaultCheck";
		this.MRgenerationModeDefaultCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerationModeDefaultCheck.Text = "Conditional Generation Mode - Default (Application Setting)";
		this.MRgenerationModeDefaultCheck.Click += new System.EventHandler(generationModeDefaultCheck_Click);
		this.MRgenerationModeConditionalCheck.Name = "MRgenerationModeConditionalCheck";
		this.MRgenerationModeConditionalCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerationModeConditionalCheck.Text = "Conditional Generation";
		this.MRgenerationModeConditionalCheck.Click += new System.EventHandler(generationModeConditionalCheck_Click);
		this.MRgenerationModeUnconditionalCheck.Name = "MRgenerationModeUnconditionalCheck";
		this.MRgenerationModeUnconditionalCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerationModeUnconditionalCheck.Text = "Unconditional Generation";
		this.MRgenerationModeUnconditionalCheck.Click += new System.EventHandler(generationModeUnconditionalCheck_Click);
		this.MRtoolStripSeparator5.Name = "MRtoolStripSeparator5";
		this.MRtoolStripSeparator5.Size = new System.Drawing.Size(477, 6);
		this.MRgenerateTraceDefaultCheck.Name = "MRgenerateTraceDefaultCheck";
		this.MRgenerateTraceDefaultCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerateTraceDefaultCheck.Text = "Generate Trace File - Default (Application Setting)";
		this.MRgenerateTraceDefaultCheck.Click += new System.EventHandler(generateTraceDefaultCheck_Click);
		this.MRgenerateTraceNoCheck.Name = "MRgenerateTraceNoCheck";
		this.MRgenerateTraceNoCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerateTraceNoCheck.Text = "Don't Generate Trace File";
		this.MRgenerateTraceNoCheck.Click += new System.EventHandler(generateTraceNoCheck_Click);
		this.MRgenerateTraceYesCheck.Name = "MRgenerateTraceYesCheck";
		this.MRgenerateTraceYesCheck.Size = new System.Drawing.Size(480, 24);
		this.MRgenerateTraceYesCheck.Text = "Generate Trace File";
		this.MRgenerateTraceYesCheck.Click += new System.EventHandler(generateTraceYesCheck_Click);
		this.cancelGenAndBuildButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.cancelGenAndBuildButton.Enabled = false;
		this.cancelGenAndBuildButton.Image = SoftVelocity.Generator.Properties.Resources.CancelGenerateAndBuild;
		this.cancelGenAndBuildButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.cancelGenAndBuildButton.Name = "cancelGenAndBuildButton";
		this.cancelGenAndBuildButton.Size = new System.Drawing.Size(28, 28);
		this.cancelGenAndBuildButton.Text = "Cancel Generation and Make Process";
		this.cancelGenAndBuildButton.Click += new System.EventHandler(cancelGenAndBuildButton_Click);
		this.toolStripSeparator8.Name = "toolStripSeparator8";
		this.toolStripSeparator8.Size = new System.Drawing.Size(6, 31);
		this.runSelectedAppProject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.runSelectedAppProject.Image = SoftVelocity.Generator.Properties.Resources.RunExe;
		this.runSelectedAppProject.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.runSelectedAppProject.Name = "runSelectedAppProject";
		this.runSelectedAppProject.Size = new System.Drawing.Size(28, 28);
		this.runSelectedAppProject.Text = "Run Selected Application's Project";
		this.runSelectedAppProject.Click += new System.EventHandler(runSelectedAppProject_Click);
		this.openContainerFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.openContainerFolder.Image = SoftVelocity.Generator.Properties.Resources.OpenFileIcon;
		this.openContainerFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.openContainerFolder.Name = "openContainerFolder";
		this.openContainerFolder.Size = new System.Drawing.Size(28, 28);
		this.openContainerFolder.Text = "Open Container Folder";
		this.openContainerFolder.Click += new System.EventHandler(openContainerFolder_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
		this.viewDCTButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.viewDCTButton.Image = SoftVelocity.Generator.Properties.Resources.Dictionary;
		this.viewDCTButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.viewDCTButton.Name = "viewDCTButton";
		this.viewDCTButton.Size = new System.Drawing.Size(28, 28);
		this.viewDCTButton.Text = "Open Dictionary";
		this.viewDCTButton.Click += new System.EventHandler(OnViewDCTButtonClick);
		this.toolStripSeparator6.Name = "toolStripSeparator6";
		this.toolStripSeparator6.Size = new System.Drawing.Size(6, 31);
		this.sortingSelectionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.sortingSelectionButton.Image = SoftVelocity.Generator.Properties.Resources.SortNumDes;
		this.sortingSelectionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.sortingSelectionButton.Name = "sortingSelectionButton";
		this.sortingSelectionButton.Size = new System.Drawing.Size(28, 28);
		this.sortingSelectionButton.Text = "Sort by Generation Order";
		this.sortingSelectionButton.Click += new System.EventHandler(sortingSelectionButton_Click);
		this.dependencyEditorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.dependencyEditorButton.Image = SoftVelocity.Generator.Properties.Resources.DependencyEditor;
		this.dependencyEditorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.dependencyEditorButton.Name = "dependencyEditorButton";
		this.dependencyEditorButton.Size = new System.Drawing.Size(28, 28);
		this.dependencyEditorButton.Text = "dependencyEditorButton";
		this.dependencyEditorButton.Click += new System.EventHandler(dependencyEditorButton_Click);
		this.refreshGenerationSortButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.refreshGenerationSortButton.Image = SoftVelocity.Generator.Properties.Resources.RefreshList;
		this.refreshGenerationSortButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.refreshGenerationSortButton.Name = "refreshGenerationSortButton";
		this.refreshGenerationSortButton.Size = new System.Drawing.Size(28, 28);
		this.refreshGenerationSortButton.Text = "Refresh generation sort";
		this.refreshGenerationSortButton.Click += new System.EventHandler(refreshGenerationSortButton_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
		this.createFromTxaButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.createFromTxaButton.Image = SoftVelocity.Generator.Properties.Resources.CreateApplicationFromTxa;
		this.createFromTxaButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.createFromTxaButton.Name = "createFromTxaButton";
		this.createFromTxaButton.Size = new System.Drawing.Size(28, 28);
		this.createFromTxaButton.Text = "toolStripButton1";
		this.createFromTxaButton.Click += new System.EventHandler(CreateFromTxaButton_Click);
		this.exportToTextButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.exportToTextButton.Enabled = false;
		this.exportToTextButton.Image = SoftVelocity.Generator.Properties.Resources.AppToText;
		this.exportToTextButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.exportToTextButton.Name = "exportToTextButton";
		this.exportToTextButton.Size = new System.Drawing.Size(28, 28);
		this.exportToTextButton.Text = "toolStripButton1";
		this.exportToTextButton.Click += new System.EventHandler(ExportToTextButton_Click);
		this.generateModeString.AcceptsReturn = true;
		this.generateModeString.Dock = System.Windows.Forms.DockStyle.Fill;
		this.generateModeString.Location = new System.Drawing.Point(0, 0);
		this.generateModeString.Margin = new System.Windows.Forms.Padding(4);
		this.generateModeString.Multiline = true;
		this.generateModeString.Name = "generateModeString";
		this.generateModeString.ReadOnly = true;
		this.generateModeString.Size = new System.Drawing.Size(432, 67);
		this.generateModeString.TabIndex = 0;
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.Location = new System.Drawing.Point(0, 59);
		this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer1.Panel1.Controls.Add((System.Windows.Forms.Control)(object)this.applicationBrowserTree);
		this.splitContainer1.Panel2.Controls.Add(this.generateModeString);
		this.splitContainer1.Size = new System.Drawing.Size(432, 256);
		this.splitContainer1.SplitterDistance = 182;
		this.splitContainer1.SplitterWidth = 7;
		this.splitContainer1.TabIndex = 4;
		this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(splitContainer1_SplitterMoved);
		this.viewProjectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.viewProjectButton.Image = SoftVelocity.Generator.Properties.Resources.ProcedureTree;
		this.viewProjectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.viewProjectButton.Name = "viewProjectButton";
		this.viewProjectButton.Size = new System.Drawing.Size(28, 28);
		this.viewProjectButton.Text = "Open Project Properties";
		this.viewProjectButton.Click += new System.EventHandler(OnViewProjectButtonClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.Locator);
		base.Controls.Add(this.toolStrip1);
		base.Margin = new System.Windows.Forms.Padding(0);
		base.Name = "ApplicationBrowserControl";
		base.Size = new System.Drawing.Size(432, 315);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		this.splitContainer1.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
