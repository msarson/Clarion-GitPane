using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop.Project.Dialogs;

public class NewProjectDialog : BaseSharpDevelopForm
{
	public class Category : TreeNode, ICategory
	{
		private List<Category> categories = new List<Category>();

		private List<TemplateItem> templates = new List<TemplateItem>();

		private int sortOrder = -1;

		private bool templatesInitialized;

		public int SortOrder
		{
			get
			{
				return sortOrder;
			}
			set
			{
				sortOrder = value;
			}
		}

		public List<Category> Categories => categories;

		public List<TemplateItem> Templates => templates;

		public Category(string name)
			: this(name, -1)
		{
		}

		public Category(string name, int sortOrder)
			: base(StringParser.Parse(name))
		{
			base.Name = StringParser.Parse(name);
			base.ImageIndex = 1;
			this.sortOrder = sortOrder;
		}

		public void InitTemplates()
		{
			if (!templatesInitialized)
			{
				templatesInitialized = true;
				templates.Sort(new TemplateSorter());
				if (templates.Count > 0)
				{
					templates[0].Selected = true;
				}
			}
		}

		[SpecialName]
		string ICategory.get_Name()
		{
			return base.Name;
		}

		[SpecialName]
		void ICategory.set_Name(string P_0)
		{
			base.Name = P_0;
		}
	}

	private class TemplateSorter : IComparer<TemplateItem>
	{
		public int Compare(TemplateItem x, TemplateItem y)
		{
			return string.Compare(x.Text, y.Text);
		}
	}

	public class TemplateItem : ListViewItem
	{
		private ProjectTemplate template;

		public ProjectTemplate Template => template;

		public TemplateItem(ProjectTemplate template)
			: base(StringParser.Parse(template.Name))
		{
			this.template = template;
			base.ImageIndex = 0;
		}
	}

	protected List<TemplateItem> alltemplates = new List<TemplateItem>();

	protected List<Category> categories = new List<Category>();

	protected Dictionary<string, int> icons = new Dictionary<string, int>();

	protected bool createNewSolution;

	public string NewProjectLocation;

	public string NewCombineLocation;

	public string DefaultProjectPath
	{
		set
		{
			if (value != null)
			{
				((TextBox)base.ControlDictionary["locationTextBox"]).Text = value;
			}
		}
	}

	private string ProjectSolution
	{
		get
		{
			string text = string.Empty;
			if (((CheckBox)base.ControlDictionary["createSeparateDirCheckBox"]).Checked)
			{
				text = text + Path.DirectorySeparatorChar + ((TextBox)base.ControlDictionary["solutionNameTextBox"]).Text.Trim();
			}
			return ProjectLocation + text;
		}
	}

	private string ProjectLocation
	{
		get
		{
			string text = ((TextBox)base.ControlDictionary["locationTextBox"]).Text.TrimEnd('\\', '/', Path.DirectorySeparatorChar);
			string text2 = ((TextBox)base.ControlDictionary["nameTextBox"]).Text.Trim();
			return text.Trim() + (((CheckBox)base.ControlDictionary["autoCreateSubDirCheckBox"]).Checked ? (Path.DirectorySeparatorChar + text2) : string.Empty);
		}
	}

	public NewProjectDialog(bool createNewSolution)
	{
		StandardHeader.SetHeaders();
		this.createNewSolution = createNewSolution;
		InitializeComponents();
		FormLocationHelper.Apply(this, "ICSharpCode.SharpDevelop.Gui.NewProjectDialog.Location", isResizable: true);
		InitializeTemplates();
		InitializeView();
		((TreeView)base.ControlDictionary["categoryTreeView"]).Select();
		((TextBox)base.ControlDictionary["locationTextBox"]).Text = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.DefaultPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Clarion Projects"));
		base.StartPosition = FormStartPosition.CenterParent;
		base.Icon = null;
	}

	protected virtual void InitializeView()
	{
		ImageList imageList = new ImageList();
		ImageList imageList2 = new ImageList();
		imageList.ColorDepth = ColorDepth.Depth32Bit;
		imageList2.ColorDepth = ColorDepth.Depth32Bit;
		imageList2.ImageSize = new Size(32, 32);
		imageList.ImageSize = new Size(16, 16);
		imageList.Images.Add(ResourceService.GetBitmap("Icons.32x32.EmptyProjectIcon"));
		imageList2.Images.Add(ResourceService.GetBitmap("Icons.32x32.EmptyProjectIcon"));
		int num = 0;
		Dictionary<string, int> dictionary = new Dictionary<string, int>(icons);
		foreach (KeyValuePair<string, int> icon in icons)
		{
			Bitmap bitmap = IconService.GetBitmap(icon.Key);
			if (bitmap != null)
			{
				imageList.Images.Add(bitmap);
				imageList2.Images.Add(bitmap);
				num = (dictionary[icon.Key] = num + 1);
			}
			else
			{
				LoggingService.Warn("NewProjectDialog: can't load bitmap " + icon.Key.ToString() + " using default");
			}
		}
		icons = dictionary;
		foreach (TemplateItem alltemplate in alltemplates)
		{
			if (alltemplate.Template.Icon == null)
			{
				alltemplate.ImageIndex = 0;
			}
			else
			{
				alltemplate.ImageIndex = icons[alltemplate.Template.Icon];
			}
		}
		((ListView)base.ControlDictionary["templateListView"]).LargeImageList = imageList2;
		((ListView)base.ControlDictionary["templateListView"]).SmallImageList = imageList;
		InsertCategories(null, categories);
		((TreeView)base.ControlDictionary["categoryTreeView"]).TreeViewNodeSorter = new TemplateCategoryComparer();
		((TreeView)base.ControlDictionary["categoryTreeView"]).Sort();
		SelectLastSelectedCategoryNode(((TreeView)base.ControlDictionary["categoryTreeView"]).Nodes, PropertyService.Get("Dialogs.NewProjectDialog.LastSelectedCategory", "Clarion"));
	}

	private void InsertCategories(TreeNode node, IEnumerable<Category> catarray)
	{
		foreach (Category item in catarray)
		{
			if (node == null)
			{
				((TreeView)base.ControlDictionary["categoryTreeView"]).Nodes.Add(item);
			}
			else
			{
				node.Nodes.Add(item);
			}
			InsertCategories(item, item.Categories);
		}
	}

	protected Category GetCategory(string categoryname, string subcategoryname)
	{
		foreach (Category category2 in categories)
		{
			if (category2.Text == categoryname)
			{
				if (subcategoryname == null)
				{
					return category2;
				}
				return GetSubcategory(category2, subcategoryname);
			}
		}
		Category category = new Category(categoryname, TemplateCategorySortOrderFile.GetProjectCategorySortOrder(categoryname));
		categories.Add(category);
		if (subcategoryname != null)
		{
			return GetSubcategory(category, subcategoryname);
		}
		return category;
	}

	private Category GetSubcategory(Category parentCategory, string name)
	{
		foreach (Category category2 in parentCategory.Categories)
		{
			if (category2.Text == name)
			{
				return category2;
			}
		}
		Category category = new Category(name, TemplateCategorySortOrderFile.GetProjectCategorySortOrder(parentCategory.Name, name));
		parentCategory.Categories.Add(category);
		return category;
	}

	protected virtual void InitializeTemplates()
	{
		foreach (ProjectTemplate projectTemplate in ProjectTemplate.ProjectTemplates)
		{
			if (projectTemplate.ProjectDescriptor != null || createNewSolution)
			{
				TemplateItem templateItem = new TemplateItem(projectTemplate);
				if (templateItem.Template.Icon != null)
				{
					icons[templateItem.Template.Icon] = 0;
				}
				if (projectTemplate.NewProjectDialogVisible)
				{
					Category category = GetCategory(StringParser.Parse(templateItem.Template.Category), StringParser.Parse(templateItem.Template.Subcategory));
					category.Templates.Add(templateItem);
				}
				alltemplates.Add(templateItem);
			}
		}
	}

	protected void CategoryChange(object sender, TreeViewEventArgs e)
	{
		((ListView)base.ControlDictionary["templateListView"]).Items.Clear();
		Category category = (Category)((TreeView)base.ControlDictionary["categoryTreeView"]).SelectedNode;
		if (category != null)
		{
			category.InitTemplates();
			foreach (TemplateItem template in category.Templates)
			{
				((ListView)base.ControlDictionary["templateListView"]).Items.Add(template);
			}
		}
		SelectedIndexChange(sender, e);
	}

	private void OnBeforeExpand(object sender, TreeViewCancelEventArgs e)
	{
		e.Node.ImageIndex = 1;
	}

	private void OnBeforeCollapse(object sender, TreeViewCancelEventArgs e)
	{
		e.Node.ImageIndex = 0;
	}

	private void CheckedChange(object sender, EventArgs e)
	{
		((TextBox)base.ControlDictionary["solutionNameTextBox"]).ReadOnly = !((CheckBox)base.ControlDictionary["createSeparateDirCheckBox"]).Checked;
		if (((TextBox)base.ControlDictionary["solutionNameTextBox"]).ReadOnly)
		{
			NameTextChanged(null, null);
		}
	}

	private void NameTextChanged(object sender, EventArgs e)
	{
		if (!((CheckBox)base.ControlDictionary["createSeparateDirCheckBox"]).Checked)
		{
			((TextBox)base.ControlDictionary["solutionNameTextBox"]).Text = ((TextBox)base.ControlDictionary["nameTextBox"]).Text.Trim();
		}
	}

	private void PathChanged(object sender, EventArgs e)
	{
		string text = ProjectSolution;
		try
		{
			if (text.Length > 3 && Path.IsPathRooted(text))
			{
				text = text.Substring(3);
				bool flag = false;
				while (text.Length > 62 && text.Length > 1)
				{
					int num = text.IndexOf(Path.DirectorySeparatorChar, 1);
					if (num < 0)
					{
						break;
					}
					text = text.Substring(num);
					flag = true;
				}
				text = ProjectSolution.Substring(0, 3) + (flag ? "..." : "") + text;
				if (text.Length > 68)
				{
					text = text.Substring(0, 65) + "...";
				}
			}
		}
		catch (ArgumentException)
		{
			base.ControlDictionary["createInLabel"].Text = ResourceService.GetString("ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.IllegalProjectNameError").Replace("\n", " ").Replace("\r", "");
			return;
		}
		base.ControlDictionary["createInLabel"].Text = ResourceService.GetString("Dialog.NewProject.ProjectAtDescription") + " " + text;
	}

	private void IconSizeChange(object sender, EventArgs e)
	{
		((ListView)base.ControlDictionary["templateListView"]).View = (((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).Checked ? View.List : View.LargeIcon);
	}

	private static string MakeNodePath(TreeNode node)
	{
		if (node == null)
		{
			return string.Empty;
		}
		string text = node.Text;
		for (TreeNode treeNode = node.Parent; treeNode != null; treeNode = treeNode.Parent)
		{
			text = treeNode.Text + "/" + text;
		}
		return text;
	}

	private void OpenEvent(object sender, EventArgs e)
	{
		if (((TreeView)base.ControlDictionary["categoryTreeView"]).SelectedNode != null)
		{
			PropertyService.Set("Dialogs.NewProjectDialog.LastSelectedCategory", MakeNodePath(((TreeView)base.ControlDictionary["categoryTreeView"]).SelectedNode));
			PropertyService.Set("Dialogs.NewProjectDialog.LargeImages", ((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).Checked);
		}
		string text = ((TextBox)base.ControlDictionary["solutionNameTextBox"]).Text.Trim();
		string text2 = ((TextBox)base.ControlDictionary["nameTextBox"]).Text.Trim();
		string fileName = ((TextBox)base.ControlDictionary["locationTextBox"]).Text.Trim();
		if (!FileUtility.IsValidFileName(text) || text.IndexOf(Path.DirectorySeparatorChar) >= 0 || text.IndexOf(Path.AltDirectorySeparatorChar) >= 0 || !FileUtility.IsValidFileName(text2) || text2.IndexOf(Path.AltDirectorySeparatorChar) >= 0 || text2.IndexOf(Path.DirectorySeparatorChar) >= 0 || !FileUtility.IsValidFileName(fileName))
		{
			MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.IllegalProjectNameError}");
			return;
		}
		if (!char.IsLetter(text2[0]) && text2[0] != '_')
		{
			MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.ProjectNameMustStartWithLetter}");
			return;
		}
		if (text2.EndsWith("."))
		{
			MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.ProjectNameMustNotEndWithDot}");
			return;
		}
		PropertyService.Set("ICSharpCode.SharpDevelop.Gui.NewProjectDialog.AutoCreateProjectSubdir", ((CheckBox)base.ControlDictionary["autoCreateSubDirCheckBox"]).Checked);
		if (((ListView)base.ControlDictionary["templateListView"]).SelectedItems.Count != 1 || ((TextBox)base.ControlDictionary["locationTextBox"]).Text.Length <= 0 || ((TextBox)base.ControlDictionary["solutionNameTextBox"]).Text.Length <= 0)
		{
			return;
		}
		TemplateItem templateItem = (TemplateItem)((ListView)base.ControlDictionary["templateListView"]).SelectedItems[0];
		try
		{
			Directory.CreateDirectory(ProjectSolution);
		}
		catch (Exception)
		{
			MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.NewProjectDialog.CantCreateDirectoryError}");
			return;
		}
		ProjectCreateInformation projectCreateInformation = new ProjectCreateInformation();
		if (!createNewSolution)
		{
			projectCreateInformation.Solution = ProjectService.OpenSolution;
		}
		projectCreateInformation.SolutionPath = ProjectLocation;
		projectCreateInformation.ProjectBasePath = ProjectSolution;
		projectCreateInformation.ProjectName = text2;
		base.Enabled = false;
		try
		{
			NewCombineLocation = templateItem.Template.CreateProject(projectCreateInformation);
			if (NewCombineLocation == null || NewCombineLocation.Length == 0)
			{
				base.Enabled = true;
				if (((CheckBox)base.ControlDictionary["createSeparateDirCheckBox"]).Checked)
				{
					try
					{
						Directory.Delete(ProjectSolution);
						return;
					}
					catch (Exception)
					{
						return;
					}
				}
			}
			else
			{
				if (createNewSolution)
				{
					ProjectService.LoadSolutionOrProject(NewCombineLocation);
					templateItem.Template.RunOpenActions(projectCreateInformation);
				}
				NewProjectLocation = ((projectCreateInformation.CreatedProjects.Count > 0) ? projectCreateInformation.CreatedProjects[0] : "");
				base.DialogResult = DialogResult.OK;
			}
		}
		finally
		{
			base.Enabled = true;
		}
	}

	private void BrowseDirectories(object sender, EventArgs e)
	{
		TextBox textBox = (TextBox)base.ControlDictionary["locationTextBox"];
		using FolderBrowserDialog folderBrowserDialog = FileService.CreateFolderBrowserDialog("${res:Dialog.NewProject.SelectDirectoryForProject}", textBox.Text);
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			textBox.Text = folderBrowserDialog.SelectedPath;
		}
	}

	private void SelectedIndexChange(object sender, EventArgs e)
	{
		if (((ListView)base.ControlDictionary["templateListView"]).SelectedItems.Count == 1)
		{
			base.ControlDictionary["descriptionLabel"].Text = StringParser.Parse(((TemplateItem)((ListView)base.ControlDictionary["templateListView"]).SelectedItems[0]).Template.Description);
			base.ControlDictionary["openButton"].Enabled = true;
		}
		else
		{
			base.ControlDictionary["descriptionLabel"].Text = string.Empty;
			base.ControlDictionary["openButton"].Enabled = false;
		}
	}

	private TreeNode SelectLastSelectedCategoryNode(TreeNodeCollection nodes, string name)
	{
		string[] array = name.Split('/');
		TreeNodeCollection collection = nodes;
		TreeNode treeNode = null;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.Length != 0)
			{
				TreeNode nodeByName = GetNodeByName(collection, text);
				if (nodeByName == null)
				{
					return null;
				}
				treeNode = nodeByName;
				collection = treeNode.Nodes;
			}
		}
		if (treeNode != null)
		{
			((TreeView)base.ControlDictionary["categoryTreeView"]).SelectedNode = treeNode;
			treeNode.ExpandAll();
		}
		return treeNode;
	}

	private static TreeNode GetNodeByName(TreeNodeCollection collection, string name)
	{
		foreach (TreeNode item in collection)
		{
			if (item.Text == name)
			{
				return item;
			}
		}
		return null;
	}

	protected void InitializeComponents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.NewProjectDialog.xfrm"));
		ImageList imageList = new ImageList();
		imageList.ColorDepth = ColorDepth.Depth32Bit;
		imageList.Images.Add(IconService.GetBitmap("Icons.16x16.OpenFolderBitmap"));
		imageList.Images.Add(IconService.GetBitmap("Icons.16x16.ClosedFolderBitmap"));
		((TreeView)base.ControlDictionary["categoryTreeView"]).ImageList = imageList;
		((ListView)base.ControlDictionary["templateListView"]).DoubleClick += OpenEvent;
		((ListView)base.ControlDictionary["templateListView"]).SelectedIndexChanged += SelectedIndexChange;
		((TreeView)base.ControlDictionary["categoryTreeView"]).AfterSelect += CategoryChange;
		((TreeView)base.ControlDictionary["categoryTreeView"]).BeforeSelect += OnBeforeExpand;
		((TreeView)base.ControlDictionary["categoryTreeView"]).BeforeExpand += OnBeforeExpand;
		((TreeView)base.ControlDictionary["categoryTreeView"]).BeforeCollapse += OnBeforeCollapse;
		((TextBox)base.ControlDictionary["solutionNameTextBox"]).TextChanged += PathChanged;
		((TextBox)base.ControlDictionary["nameTextBox"]).TextChanged += NameTextChanged;
		((TextBox)base.ControlDictionary["nameTextBox"]).TextChanged += PathChanged;
		((TextBox)base.ControlDictionary["locationTextBox"]).TextChanged += PathChanged;
		((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).Checked = PropertyService.Get("Dialogs.NewProjectDialog.LargeImages", defaultValue: true);
		((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).CheckedChanged += IconSizeChange;
		((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).FlatStyle = FlatStyle.Standard;
		((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).Image = IconService.GetBitmap("Icons.16x16.LargeIconsIcon");
		((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).Checked = !PropertyService.Get("Dialogs.NewProjectDialog.LargeImages", defaultValue: true);
		((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).CheckedChanged += IconSizeChange;
		((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).FlatStyle = FlatStyle.Standard;
		((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).Image = IconService.GetBitmap("Icons.16x16.SmallIconsIcon");
		base.ControlDictionary["openButton"].Click += OpenEvent;
		base.ControlDictionary["browseButton"].Click += BrowseDirectories;
		((CheckBox)base.ControlDictionary["createSeparateDirCheckBox"]).CheckedChanged += CheckedChange;
		((CheckBox)base.ControlDictionary["createSeparateDirCheckBox"]).CheckedChanged += PathChanged;
		((CheckBox)base.ControlDictionary["autoCreateSubDirCheckBox"]).CheckedChanged += PathChanged;
		ToolTip toolTip = new ToolTip();
		toolTip.SetToolTip(base.ControlDictionary["largeIconsRadioButton"], StringParser.Parse("${res:Global.LargeIconToolTip}"));
		toolTip.SetToolTip(base.ControlDictionary["smallIconsRadioButton"], StringParser.Parse("${res:Global.SmallIconToolTip}"));
		toolTip.Active = true;
		base.Owner = (Form)WorkbenchSingleton.Workbench;
		base.StartPosition = FormStartPosition.CenterParent;
		base.Icon = null;
		CheckedChange(this, EventArgs.Empty);
		IconSizeChange(this, EventArgs.Empty);
	}
}
