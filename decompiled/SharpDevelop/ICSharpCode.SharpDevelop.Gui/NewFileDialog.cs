using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class NewFileDialog : BaseSharpDevelopForm
{
	public class Category : TreeNode, ICategory
	{
		private ArrayList categories = new ArrayList();

		private ArrayList templates = new ArrayList();

		private int sortOrder = -1;

		public bool Selected;

		public bool HasSelectedTemplate;

		public ArrayList Categories => categories;

		public ArrayList Templates => templates;

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

		public Category(string name, int sortOrder)
			: base(StringParser.Parse(name))
		{
			base.Name = StringParser.Parse(name);
			base.ImageIndex = 1;
			this.sortOrder = sortOrder;
		}

		public Category(string name)
			: this(name, -1)
		{
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

	public class TemplateItem : ListViewItem
	{
		private FileTemplate template;

		public FileTemplate Template => template;

		public TemplateItem(FileTemplate template)
			: base(StringParser.Parse(template.Name))
		{
			this.template = template;
			base.ImageIndex = 0;
		}
	}

	private const int GridWidth = 256;

	private const int GridMargin = 8;

	private ArrayList alltemplates = new ArrayList();

	private ArrayList categories = new ArrayList();

	private Hashtable icons = new Hashtable();

	private bool allowUntitledFiles;

	private string basePath;

	private List<KeyValuePair<string, FileDescriptionTemplate>> createdFiles = new List<KeyValuePair<string, FileDescriptionTemplate>>();

	private PropertyGrid propertyGrid = new PropertyGrid();

	private LocalizedTypeDescriptor localizedTypeDescriptor;

	private bool isNameModified;

	public List<KeyValuePair<string, FileDescriptionTemplate>> CreatedFiles => createdFiles;

	private bool AllPropertiesHaveAValue
	{
		get
		{
			foreach (TemplateProperty property in SelectedTemplate.Properties)
			{
				string text = StringParser.Properties["Properties." + property.Name];
				if (text == null || text.Length == 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	protected FileTemplate SelectedTemplate
	{
		get
		{
			if (((ListView)base.ControlDictionary["templateListView"]).SelectedItems.Count == 1)
			{
				return ((TemplateItem)((ListView)base.ControlDictionary["templateListView"]).SelectedItems[0]).Template;
			}
			return null;
		}
	}

	public NewFileDialog(string basePath)
	{
		StandardHeader.SetHeaders();
		this.basePath = basePath;
		allowUntitledFiles = basePath == null;
		try
		{
			InitializeComponents();
			FormLocationHelper.Apply(this, "ICSharpCode.SharpDevelop.Gui.NewFileDialog.Location", isResizable: true);
			InitializeTemplates();
			InitializeView();
			((TreeView)base.ControlDictionary["categoryTreeView"]).Select();
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	private void InitializeView()
	{
		ImageList imageList = new ImageList();
		ImageList imageList2 = new ImageList();
		imageList.ColorDepth = ColorDepth.Depth32Bit;
		imageList2.ColorDepth = ColorDepth.Depth32Bit;
		imageList2.ImageSize = new Size(32, 32);
		imageList.ImageSize = new Size(16, 16);
		imageList.Images.Add(IconService.GetBitmap("Icons.32x32.EmptyFileIcon"));
		imageList2.Images.Add(IconService.GetBitmap("Icons.32x32.EmptyFileIcon"));
		int num = 0;
		Hashtable hashtable = new Hashtable(icons);
		foreach (DictionaryEntry icon in icons)
		{
			Bitmap bitmap = IconService.GetBitmap(icon.Key.ToString());
			if (bitmap != null)
			{
				imageList.Images.Add(bitmap);
				imageList2.Images.Add(bitmap);
				hashtable[icon.Key] = ++num;
			}
			else
			{
				LoggingService.Warn("NewFileDialog: can't load bitmap " + icon.Key.ToString() + " using default");
			}
		}
		icons = hashtable;
		foreach (TemplateItem alltemplate in alltemplates)
		{
			if (alltemplate.Template.Icon == null)
			{
				alltemplate.ImageIndex = 0;
			}
			else
			{
				alltemplate.ImageIndex = (int)icons[alltemplate.Template.Icon];
			}
		}
		((ListView)base.ControlDictionary["templateListView"]).LargeImageList = imageList2;
		((ListView)base.ControlDictionary["templateListView"]).SmallImageList = imageList;
		InsertCategories(null, categories);
		((TreeView)base.ControlDictionary["categoryTreeView"]).TreeViewNodeSorter = new TemplateCategoryComparer();
		((TreeView)base.ControlDictionary["categoryTreeView"]).Sort();
		SelectLastSelectedCategoryNode(((TreeView)base.ControlDictionary["categoryTreeView"]).Nodes, PropertyService.Get("Dialogs.NewFileDialog.LastSelectedCategory", "C#"));
	}

	private void InsertCategories(TreeNode node, ArrayList catarray)
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

	private Category GetCategory(string categoryname, string subcategoryname)
	{
		foreach (Category category3 in categories)
		{
			if (category3.Name == categoryname)
			{
				if (subcategoryname == null)
				{
					return category3;
				}
				return GetSubcategory(category3, subcategoryname);
			}
		}
		Category category2 = new Category(categoryname, TemplateCategorySortOrderFile.GetFileCategorySortOrder(categoryname));
		categories.Add(category2);
		if (subcategoryname != null)
		{
			return GetSubcategory(category2, subcategoryname);
		}
		return category2;
	}

	private Category GetSubcategory(Category parentCategory, string name)
	{
		foreach (Category category3 in parentCategory.Categories)
		{
			if (category3.Name == name)
			{
				return category3;
			}
		}
		Category category2 = new Category(name, TemplateCategorySortOrderFile.GetFileCategorySortOrder(parentCategory.Name, name));
		parentCategory.Categories.Add(category2);
		return category2;
	}

	private void InitializeTemplates()
	{
		foreach (FileTemplate fileTemplate in FileTemplate.FileTemplates)
		{
			TemplateItem templateItem = new TemplateItem(fileTemplate);
			if (templateItem.Template.Icon != null)
			{
				icons[templateItem.Template.Icon] = 0;
			}
			if (fileTemplate.NewFileDialogVisible)
			{
				Category category = GetCategory(StringParser.Parse(templateItem.Template.Category), StringParser.Parse(templateItem.Template.Subcategory));
				category.Templates.Add(templateItem);
				if (!category.Selected && fileTemplate.WizardPath == null)
				{
					category.Selected = true;
				}
				if (!category.HasSelectedTemplate && templateItem.Template.FileDescriptionTemplates.Count == 1 && templateItem.Template.FileDescriptionTemplates[0].Name.StartsWith("Empty"))
				{
					templateItem.Selected = true;
					category.HasSelectedTemplate = true;
				}
			}
			alltemplates.Add(templateItem);
		}
	}

	private void CategoryChange(object sender, TreeViewEventArgs e)
	{
		((ListView)base.ControlDictionary["templateListView"]).Items.Clear();
		HidePropertyGrid();
		if (((TreeView)base.ControlDictionary["categoryTreeView"]).SelectedNode == null)
		{
			return;
		}
		foreach (TemplateItem template in ((Category)((TreeView)base.ControlDictionary["categoryTreeView"]).SelectedNode).Templates)
		{
			((ListView)base.ControlDictionary["templateListView"]).Items.Add(template);
		}
	}

	private void OnBeforeExpand(object sender, TreeViewCancelEventArgs e)
	{
		e.Node.ImageIndex = 1;
	}

	private void OnBeforeCollapse(object sender, TreeViewCancelEventArgs e)
	{
		e.Node.ImageIndex = 0;
	}

	private void ShowPropertyGrid()
	{
		if (localizedTypeDescriptor == null)
		{
			localizedTypeDescriptor = new LocalizedTypeDescriptor();
		}
		if (base.Controls.Contains(propertyGrid))
		{
			return;
		}
		SuspendLayout();
		propertyGrid.Location = new Point(base.Width - 8, 8);
		localizedTypeDescriptor.Properties.Clear();
		foreach (TemplateProperty property in SelectedTemplate.Properties)
		{
			LocalizedProperty localizedProperty;
			if (property.Type.StartsWith("Types:"))
			{
				localizedProperty = new LocalizedProperty(property.Name, "System.Enum", property.Category, property.Description);
				TemplateType templateType = null;
				foreach (TemplateType customType in SelectedTemplate.CustomTypes)
				{
					if (customType.Name == property.Type.Substring("Types:".Length))
					{
						templateType = customType;
						break;
					}
				}
				if (templateType == null)
				{
					throw new Exception("type : " + property.Type + " not found.");
				}
				localizedProperty.TypeConverterObject = new CustomTypeConverter(templateType);
				StringParser.Properties["Properties." + localizedProperty.Name] = property.DefaultValue;
				localizedProperty.DefaultValue = property.DefaultValue;
			}
			else
			{
				localizedProperty = new LocalizedProperty(property.Name, property.Type, property.Category, property.Description);
				if (property.Type == "System.Boolean")
				{
					localizedProperty.TypeConverterObject = new BooleanTypeConverter();
					string text = ((property.DefaultValue == null) ? null : property.DefaultValue.ToString());
					if (text == null || text.Length == 0)
					{
						text = "True";
					}
					StringParser.Properties["Properties." + localizedProperty.Name] = text;
					localizedProperty.DefaultValue = bool.Parse(text);
				}
			}
			localizedProperty.LocalizedName = property.LocalizedName;
			localizedTypeDescriptor.Properties.Add(localizedProperty);
		}
		propertyGrid.ToolbarVisible = false;
		propertyGrid.SelectedObject = localizedTypeDescriptor;
		propertyGrid.Size = new Size(256, base.Height - 32);
		base.Width += 256;
		base.Controls.Add(propertyGrid);
		ResumeLayout(performLayout: false);
	}

	private void HidePropertyGrid()
	{
		if (base.Controls.Contains(propertyGrid))
		{
			SuspendLayout();
			base.Controls.Remove(propertyGrid);
			base.Width -= 256;
			ResumeLayout(performLayout: false);
		}
	}

	public string GenerateCurrentFileName(FileTemplate fileTemplate)
	{
		if (fileTemplate.DefaultName.IndexOf("${Number}") >= 0)
		{
			try
			{
				int num = 1;
				while (true)
				{
					StringParser.Properties["Number"] = num.ToString();
					string text = StringParser.Parse(fileTemplate.DefaultName);
					if (allowUntitledFiles)
					{
						bool flag = false;
						foreach (string openFile in FileService.GetOpenFiles())
						{
							if (Path.GetFileName(openFile) == text)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							break;
						}
					}
					else if (!File.Exists(Path.Combine(basePath, text)))
					{
						break;
					}
					num++;
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
		return StringParser.Parse(fileTemplate.DefaultName);
	}

	public virtual string GenerateCurrentFileName()
	{
		return GenerateCurrentFileName(SelectedTemplate);
	}

	private void SelectedIndexChange(object sender, EventArgs e)
	{
		if (((ListView)base.ControlDictionary["templateListView"]).SelectedItems.Count == 1)
		{
			base.ControlDictionary["descriptionLabel"].Text = StringParser.Parse(SelectedTemplate.Description);
			base.ControlDictionary["openButton"].Enabled = true;
			if (SelectedTemplate.HasProperties)
			{
				ShowPropertyGrid();
			}
			if (!allowUntitledFiles && !isNameModified)
			{
				base.ControlDictionary["fileNameTextBox"].Text = GenerateCurrentFileName();
				isNameModified = false;
			}
		}
		else
		{
			base.ControlDictionary["descriptionLabel"].Text = string.Empty;
			base.ControlDictionary["openButton"].Enabled = false;
			HidePropertyGrid();
		}
	}

	private void FileNameChanged(object sender, EventArgs e)
	{
		isNameModified = true;
	}

	private void CheckedChange(object sender, EventArgs e)
	{
		((ListView)base.ControlDictionary["templateListView"]).View = (((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).Checked ? View.List : View.LargeIcon);
	}

	public virtual bool IsFilenameAvailable(string fileName)
	{
		if (Path.IsPathRooted(fileName))
		{
			return !File.Exists(fileName);
		}
		return true;
	}

	private void SaveFile(FileDescriptionTemplate newfile, string content, byte[] binaryContent, bool openFile)
	{
		string text = StringParser.Parse(newfile.Name);
		string text2 = StringParser.Parse(StringParser.Parse(content));
		if ((text.StartsWith("/") && !text.StartsWith("//")) || (text.StartsWith("\\") && !text.StartsWith("\\\\")))
		{
			text = text.Substring(1);
		}
		if (newfile.IsDependentFile && Path.IsPathRooted(text))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(text));
			if (binaryContent != null)
			{
				File.WriteAllBytes(text, binaryContent);
			}
			else
			{
				File.WriteAllText(text, text2, ParserService.DefaultFileEncoding);
			}
			ParserService.ParseFile(text, text2);
		}
		else
		{
			if (binaryContent != null)
			{
				LoggingService.Warn("binary file was skipped");
				return;
			}
			IWorkbenchWindow workbenchWindow = FileService.NewFile(Path.GetFileName(text), StringParser.Parse(newfile.Language), text2, openFile);
			if (workbenchWindow == null)
			{
				return;
			}
			if (Path.IsPathRooted(text))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(text));
				workbenchWindow.ViewContent.Save(text);
			}
		}
		createdFiles.Add(new KeyValuePair<string, FileDescriptionTemplate>(text, newfile));
	}

	public virtual void SaveFile(FileDescriptionTemplate newfile, string content, byte[] binaryContent)
	{
		SaveFile(newfile, content, binaryContent, openFile: false);
	}

	internal static string GenerateValidClassName(string className)
	{
		int i;
		for (i = 0; i < className.Length && className[i] != '_' && !char.IsLetter(className[i]); i++)
		{
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (; i < className.Length; i++)
		{
			if (char.IsLetterOrDigit(className[i]) || className[i] == '_')
			{
				stringBuilder.Append(className[i]);
			}
			if (className[i] == ' ' || className[i] == '-')
			{
				stringBuilder.Append('_');
			}
		}
		return stringBuilder.ToString();
	}

	public virtual void ProcessFile(ref string fileName, TemplateItem item)
	{
		if (!FileUtility.IsValidFileName(fileName) || fileName.IndexOf(Path.AltDirectorySeparatorChar) >= 0 || fileName.IndexOf(Path.DirectorySeparatorChar) >= 0)
		{
			MessageService.ShowError(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.SaveFile.InvalidFileNameError}", new string[1, 2] { { "FileName", fileName } }));
			return;
		}
		if (Path.GetExtension(fileName).Length == 0)
		{
			fileName += Path.GetExtension(item.Template.DefaultName);
		}
		fileName = Path.Combine(basePath, fileName);
		fileName = Path.GetFullPath(fileName);
		IProject currentProject = ProjectService.CurrentProject;
		if (currentProject != null)
		{
			StringParser.Properties["StandardNamespace"] = CustomToolsService.GetDefaultNamespace(currentProject, fileName);
		}
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
			PropertyService.Set("Dialogs.NewProjectDialog.LargeImages", ((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).Checked);
			PropertyService.Set("Dialogs.NewFileDialog.LastSelectedCategory", MakeNodePath(((TreeView)base.ControlDictionary["categoryTreeView"]).SelectedNode));
		}
		createdFiles.Clear();
		if (((ListView)base.ControlDictionary["templateListView"]).SelectedItems.Count != 1)
		{
			return;
		}
		if (!AllPropertiesHaveAValue)
		{
			MessageService.ShowMessage("${res:Dialog.NewFile.FillOutFirstMessage}", "${res:Dialog.NewFile.FillOutFirstCaption}");
			return;
		}
		TemplateItem templateItem = (TemplateItem)((ListView)base.ControlDictionary["templateListView"]).SelectedItems[0];
		OpenTemplate(templateItem, allowUntitledFiles);
		base.DialogResult = DialogResult.OK;
		foreach (KeyValuePair<string, FileDescriptionTemplate> createdFile in createdFiles)
		{
			FileService.FireFileCreated(createdFile.Key);
		}
	}

	public string[] OpenTemplate(TemplateItem templateItem)
	{
		OpenTemplate(templateItem, allowUntitledFiles: true, processFile: true, openFile: false);
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, FileDescriptionTemplate> createdFile in createdFiles)
		{
			list.Add(createdFile.Key);
		}
		return list.ToArray();
	}

	private void OpenTemplate(TemplateItem templateItem, bool allowUntitledFiles)
	{
		OpenTemplate(templateItem, allowUntitledFiles, processFile: false, openFile: true);
	}

	private void OpenTemplate(TemplateItem templateItem, bool allowUntitledFiles, bool processFile, bool openFile)
	{
		StringParser.Properties["StandardNamespace"] = "DefaultNamespace";
		string fileName;
		if (allowUntitledFiles)
		{
			fileName = GenerateCurrentFileName(templateItem.Template);
			if (processFile)
			{
				ProcessFile(ref fileName, templateItem);
			}
		}
		else
		{
			fileName = base.ControlDictionary["fileNameTextBox"].Text.Trim();
			ProcessFile(ref fileName, templateItem);
		}
		StringParser.Properties["FullName"] = fileName;
		StringParser.Properties["FileName"] = Path.GetFileName(fileName);
		StringParser.Properties["FileNameWithoutExtension"] = Path.GetFileNameWithoutExtension(fileName);
		StringParser.Properties["Extension"] = Path.GetExtension(fileName);
		StringParser.Properties["Path"] = Path.GetDirectoryName(fileName);
		StringParser.Properties["ClassName"] = GenerateValidClassName(Path.GetFileNameWithoutExtension(fileName));
		foreach (FileDescriptionTemplate fileDescriptionTemplate in templateItem.Template.FileDescriptionTemplates)
		{
			if (!IsFilenameAvailable(StringParser.Parse(fileDescriptionTemplate.Name)))
			{
				MessageService.ShowError("Filename " + StringParser.Parse(fileDescriptionTemplate.Name) + " is in use.\nChoose another one");
				return;
			}
		}
		if (templateItem.Template.WizardPath != null)
		{
			Properties properties = new Properties();
			properties.Set("Template", templateItem.Template);
			properties.Set("Creator", this);
			using WizardDialog wizardDialog = new WizardDialog("File Wizard", properties, templateItem.Template.WizardPath);
			if (wizardDialog.ShowDialog(WorkbenchSingleton.MainForm) != DialogResult.OK)
			{
				return;
			}
		}
		if (templateItem.Template.BinaryFileGeneratorPath != null && !BinaryFileGeneratorLoader.Run(templateItem.Template.BinaryFileGeneratorPath, templateItem.Template))
		{
			return;
		}
		ScriptRunner scriptRunner = new ScriptRunner();
		foreach (FileDescriptionTemplate fileDescriptionTemplate2 in templateItem.Template.FileDescriptionTemplates)
		{
			if (!fileDescriptionTemplate2.Skip)
			{
				if (fileDescriptionTemplate2.ContentData != null)
				{
					SaveFile(fileDescriptionTemplate2, null, fileDescriptionTemplate2.ContentData);
				}
				else if (fileDescriptionTemplate2.ProcessScripts)
				{
					SaveFile(fileDescriptionTemplate2, scriptRunner.CompileScript(templateItem.Template, fileDescriptionTemplate2), null, openFile);
				}
				else
				{
					SaveFile(fileDescriptionTemplate2, fileDescriptionTemplate2.Content, null);
				}
			}
		}
	}

	private void InitializeComponents()
	{
		if (allowUntitledFiles)
		{
			SetupFromXmlStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Resources.NewFileDialog.xfrm"));
		}
		else
		{
			SetupFromXmlStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Resources.NewFileWithNameDialog.xfrm"));
			base.ControlDictionary["fileNameTextBox"].TextChanged += FileNameChanged;
		}
		ImageList imageList = new ImageList();
		imageList.ColorDepth = ColorDepth.Depth32Bit;
		imageList.Images.Add(IconService.GetBitmap("Icons.16x16.OpenFolderBitmap"));
		imageList.Images.Add(IconService.GetBitmap("Icons.16x16.ClosedFolderBitmap"));
		((TreeView)base.ControlDictionary["categoryTreeView"]).ImageList = imageList;
		((TreeView)base.ControlDictionary["categoryTreeView"]).AfterSelect += CategoryChange;
		((TreeView)base.ControlDictionary["categoryTreeView"]).BeforeSelect += OnBeforeExpand;
		((TreeView)base.ControlDictionary["categoryTreeView"]).BeforeExpand += OnBeforeExpand;
		((TreeView)base.ControlDictionary["categoryTreeView"]).BeforeCollapse += OnBeforeCollapse;
		((ListView)base.ControlDictionary["templateListView"]).SelectedIndexChanged += SelectedIndexChange;
		((ListView)base.ControlDictionary["templateListView"]).DoubleClick += OpenEvent;
		base.ControlDictionary["openButton"].Click += OpenEvent;
		((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).Checked = PropertyService.Get("Dialogs.NewProjectDialog.LargeImages", defaultValue: true);
		((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).CheckedChanged += CheckedChange;
		((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).FlatStyle = FlatStyle.Standard;
		((RadioButton)base.ControlDictionary["largeIconsRadioButton"]).Image = IconService.GetBitmap("Icons.16x16.LargeIconsIcon");
		((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).Checked = !PropertyService.Get("Dialogs.NewProjectDialog.LargeImages", defaultValue: true);
		((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).CheckedChanged += CheckedChange;
		((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).FlatStyle = FlatStyle.Standard;
		((RadioButton)base.ControlDictionary["smallIconsRadioButton"]).Image = IconService.GetBitmap("Icons.16x16.SmallIconsIcon");
		ToolTip toolTip = new ToolTip();
		toolTip.SetToolTip(base.ControlDictionary["largeIconsRadioButton"], StringParser.Parse("${res:Global.LargeIconToolTip}"));
		toolTip.SetToolTip(base.ControlDictionary["smallIconsRadioButton"], StringParser.Parse("${res:Global.SmallIconToolTip}"));
		toolTip.Active = true;
		base.Owner = (Form)WorkbenchSingleton.Workbench;
		base.StartPosition = FormStartPosition.CenterParent;
		base.Icon = null;
		CheckedChange(this, EventArgs.Empty);
	}
}
