using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project.Converter;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public class MSBuildBasedProject : AbstractProject, IProjectItemListProvider, IProjectAllowChangeConfigurations
{
	public const string ProjectGuidPropertyName = "ProjectGuid";

	private Microsoft.Build.BuildEngine.Project project;

	protected readonly Set<string> saveAfterImportsProperties = new Set<string>("PostBuildEvent", "PreBuildEvent");

	private Microsoft.Build.BuildEngine.Project evaluatingTempProject;

	protected List<ProjectItem> items = new List<ProjectItem>();

	protected volatile ReadOnlyCollection<ProjectItem> itemsReadOnly;

	private volatile ICollection<ItemType> availableFileItemTypes = ItemType.DefaultFileItems;

	protected bool isLoading;

	private ICollection<string> configurationNames;

	private ICollection<string> platformNames;

	[Browsable(false)]
	public Microsoft.Build.BuildEngine.Project MSBuildProject => project;

	public override ReadOnlyCollection<ProjectItem> Items
	{
		get
		{
			ReadOnlyCollection<ProjectItem> readOnlyCollection = itemsReadOnly;
			if (readOnlyCollection == null)
			{
				lock (base.SyncRoot)
				{
					readOnlyCollection = Array.AsReadOnly(items.ToArray());
				}
				itemsReadOnly = readOnlyCollection;
			}
			return readOnlyCollection;
		}
	}

	public override ICollection<ItemType> AvailableFileItemTypes => availableFileItemTypes;

	public override string AppDesignerFolder
	{
		get
		{
			return GetEvaluatedProperty("AppDesignerFolder");
		}
		set
		{
			SetProperty("AppDesignerFolder", value);
		}
	}

	[Browsable(false)]
	public override string IdGuid
	{
		get
		{
			return base.IdGuid;
		}
		set
		{
			if (base.IdGuid == null)
			{
				SetPropertyInternal(null, null, "ProjectGuid", value, PropertyStorageLocations.Base, treatPropertyValueAsLiteral: true);
				try
				{
					project.Save(base.FileName);
				}
				catch
				{
				}
			}
			base.IdGuid = value;
		}
	}

	public override ICollection<string> ConfigurationNames
	{
		get
		{
			if (configurationNames == null)
			{
				LoadConfigurationPlatformNamesFromMSBuild();
			}
			return configurationNames;
		}
	}

	public override ICollection<string> PlatformNames
	{
		get
		{
			if (platformNames == null)
			{
				LoadConfigurationPlatformNamesFromMSBuild();
			}
			return platformNames;
		}
	}

	public event EventHandler<ProjectPropertyChangedEventArgs> PropertyChanged;

	public MSBuildBasedProject(Engine engine)
	{
		if (engine == null)
		{
			throw new ArgumentNullException("engine");
		}
		project = engine.CreateNewProject();
	}

	public override void Dispose()
	{
		base.Dispose();
		MSBuildInternals.EnsureCorrectTempProject(project, null, null, ref evaluatingTempProject);
	}

	private ProjectItem CreateProjectItem(BuildItem item, BuildItemGroup refs)
	{
		if (item.Name == "Reference")
		{
			ReferenceProjectItem referenceProjectItem = new ReferenceProjectItem(this, item);
			if (string.IsNullOrEmpty(referenceProjectItem.HintPath) && refs != null)
			{
				try
				{
					int num = item.Include.IndexOf(',');
					string text = ((num == -1) ? item.Include : item.Include.Substring(0, num));
					string value = text + ".dll";
					string value2 = text + ".exe";
					foreach (BuildItem @ref in refs)
					{
						if (Path.IsPathRooted(@ref.Include))
						{
							if (@ref.Include.IndexOf(value) != -1)
							{
								referenceProjectItem.EvaluatedReferencePath = @ref.Include;
								break;
							}
							if (@ref.Include.IndexOf(value2) != -1)
							{
								referenceProjectItem.EvaluatedReferencePath = @ref.Include;
								break;
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
			return referenceProjectItem;
		}
		return CreateProjectItem(item);
	}

	public override ProjectItem CreateProjectItem(BuildItem item)
	{
		switch (item.Name)
		{
		case "Reference":
			return new ReferenceProjectItem(this, item);
		case "ProjectReference":
			return new ProjectReferenceProjectItem(this, item);
		case "COMReference":
			return new ComReferenceProjectItem(this, item);
		case "Import":
			return new ImportProjectItem(this, item);
		case "None":
		case "Compile":
		case "EmbeddedResource":
		case "Resource":
		case "Content":
		case "Folder":
			return new FileProjectItem(this, item);
		case "WebReferenceUrl":
			return new WebReferenceUrl(this, item);
		case "WebReferences":
			return new WebReferencesProjectItem(this, item);
		default:
			if (AvailableFileItemTypes.Contains(new ItemType(item.Name)) || SafeFileExists(base.Directory, item.FinalItemSpec))
			{
				return new FileProjectItem(this, item);
			}
			return base.CreateProjectItem(item);
		}
	}

	private static bool SafeFileExists(string directory, string fileName)
	{
		try
		{
			return File.Exists(Path.Combine(directory, fileName));
		}
		catch (Exception)
		{
			return false;
		}
	}

	protected virtual void Create(ProjectCreateInformation information)
	{
		InitializeMSBuildProject(project);
		base.Name = information.ProjectName;
		base.FileName = information.OutputProjectFileName;
		base.IdGuid = "{" + Guid.NewGuid().ToString().ToUpperInvariant() + "}";
		BuildPropertyGroup buildPropertyGroup = project.AddNewPropertyGroup(insertAtEndOfProject: false);
		buildPropertyGroup.AddNewProperty("ProjectGuid", IdGuid, treatPropertyValueAsLiteral: true);
		buildPropertyGroup.AddNewProperty("Configuration", "Debug", treatPropertyValueAsLiteral: true).Condition = " '$(Configuration)' == '' ";
		buildPropertyGroup.AddNewProperty("Platform", "AnyCPU", treatPropertyValueAsLiteral: true).Condition = " '$(Platform)' == '' ";
		base.ActiveConfiguration = "Debug";
		ActivePlatform = "AnyCPU";
	}

	protected void AddGuardedProperty(string name, string value, bool treatValueAsLiteral)
	{
		foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
		{
			if (!propertyGroup.IsImported && string.IsNullOrEmpty(propertyGroup.Condition))
			{
				propertyGroup.AddNewProperty(name, value, treatValueAsLiteral).Condition = " '$(" + name + ")' == '' ";
				return;
			}
		}
		BuildPropertyGroup buildPropertyGroup2 = project.AddNewPropertyGroup(insertAtEndOfProject: false);
		buildPropertyGroup2.AddNewProperty(name, value, treatValueAsLiteral).Condition = " '$(" + name + ")' == '' ";
	}

	protected void AddImport(string projectFile, string condition)
	{
		project.AddNewImport(projectFile, condition);
		CreateItemsListFromMSBuild();
	}

	public string GetEvaluatedProperty(string propertyName)
	{
		lock (base.SyncRoot)
		{
			return project.GetEvaluatedProperty(propertyName);
		}
	}

	public string GetProperty(string configuration, string platform, string propertyName)
	{
		PropertyStorageLocations propertyStorageLocations;
		return GetProperty(configuration, platform, propertyName, out propertyStorageLocations);
	}

	public string GetProperty(string configuration, string platform, string propertyName, out PropertyStorageLocations location)
	{
		lock (base.SyncRoot)
		{
			BuildPropertyGroup group;
			return FindPropertyObject(configuration, platform, propertyName, out group, out location)?.FinalValue;
		}
	}

	public string GetUnevalatedProperty(string propertyName)
	{
		return GetUnevalatedProperty(base.ActiveConfiguration, ActivePlatform, propertyName);
	}

	public string GetUnevalatedProperty(string configuration, string platform, string propertyName)
	{
		PropertyStorageLocations propertyStorageLocations;
		return GetUnevalatedProperty(configuration, platform, propertyName, out propertyStorageLocations);
	}

	public string GetUnevalatedProperty(string configuration, string platform, string propertyName, out PropertyStorageLocations location)
	{
		lock (base.SyncRoot)
		{
			BuildPropertyGroup group;
			return FindPropertyObject(configuration, platform, propertyName, out group, out location)?.Value;
		}
	}

	protected bool EvaluateMSBuildCondition(string configuration, string platform, string condition)
	{
		return MSBuildInternals.EvaluateCondition(project, configuration, platform, condition, ref evaluatingTempProject);
	}

	protected BuildProperty FindPropertyObject(string configuration, string platform, string propertyName, out BuildPropertyGroup group, out PropertyStorageLocations location)
	{
		if (string.IsNullOrEmpty(configuration))
		{
			configuration = base.ActiveConfiguration;
		}
		if (string.IsNullOrEmpty(platform))
		{
			platform = ActivePlatform;
		}
		foreach (BuildPropertyGroup item in Linq.ToList(Linq.CastTo<BuildPropertyGroup>(project.PropertyGroups)))
		{
			if (!item.IsImported)
			{
				BuildProperty property = MSBuildInternals.GetProperty(item, propertyName);
				if (property != null && EvaluateMSBuildCondition(configuration, platform, item.Condition))
				{
					location = MSBuildInternals.GetLocationFromCondition(item.Condition);
					group = item;
					return property;
				}
			}
		}
		location = PropertyStorageLocations.Unchanged;
		group = null;
		return null;
	}

	private string GetAnyUnevaluatedPropertyValue(string configuration, string platform, string propertyName)
	{
		foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
		{
			if (propertyGroup.IsImported)
			{
				continue;
			}
			BuildProperty property = MSBuildInternals.GetProperty(propertyGroup, propertyName);
			if (property != null)
			{
				MSBuildInternals.GetConfigurationAndPlatformFromCondition(propertyGroup.Condition, out var configuration2, out var platform2);
				if ((configuration == null || configuration == configuration2 || configuration2 == null) && (platform == null || platform == platform2 || platform2 == null))
				{
					return property.Value;
				}
			}
		}
		return null;
	}

	public IList<BuildProperty> GetAllProperties(string propertyName)
	{
		List<BuildProperty> list = new List<BuildProperty>();
		foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
		{
			if (!propertyGroup.IsImported)
			{
				BuildProperty property = MSBuildInternals.GetProperty(propertyGroup, propertyName);
				if (property != null)
				{
					list.Add(property);
				}
			}
		}
		return list;
	}

	protected virtual void OnPropertyChanged(ProjectPropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}

	private PropertyStorageLocations FindExistingPropertyInAllConfigurations(string propertyName)
	{
		foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
		{
			if (!propertyGroup.IsImported && MSBuildInternals.GetProperty(propertyGroup, propertyName) != null)
			{
				return MSBuildInternals.GetLocationFromCondition(propertyGroup.Condition);
			}
		}
		return PropertyStorageLocations.Unchanged;
	}

	public void SetProperty(string propertyName, string newValue)
	{
		SetProperty(propertyName, newValue, treatPropertyValueAsLiteral: true);
	}

	public void SetProperty(string propertyName, string newValue, bool treatPropertyValueAsLiteral)
	{
		SetProperty(base.ActiveConfiguration, ActivePlatform, propertyName, newValue, PropertyStorageLocations.Unchanged, treatPropertyValueAsLiteral);
	}

	public void SetProperty(string configuration, string platform, string propertyName, string newValue, PropertyStorageLocations location, bool treatPropertyValueAsLiteral)
	{
		ProjectPropertyChangedEventArgs e;
		lock (base.SyncRoot)
		{
			e = SetPropertyInternal(configuration, platform, propertyName, newValue, location, treatPropertyValueAsLiteral);
		}
		if (e.NewValue != e.OldValue || e.NewLocation != e.OldLocation)
		{
			OnPropertyChanged(e);
		}
	}

	private ProjectPropertyChangedEventArgs SetPropertyInternal(string configuration, string platform, string propertyName, string newValue, PropertyStorageLocations location, bool treatPropertyValueAsLiteral)
	{
		BuildPropertyGroup group;
		PropertyStorageLocations propertyStorageLocations;
		BuildProperty buildProperty = FindPropertyObject(configuration, platform, propertyName, out group, out propertyStorageLocations);
		if (propertyStorageLocations == PropertyStorageLocations.Unchanged)
		{
			propertyStorageLocations = FindExistingPropertyInAllConfigurations(propertyName);
			if (propertyStorageLocations == PropertyStorageLocations.Unchanged)
			{
				propertyStorageLocations = PropertyStorageLocations.Base;
			}
		}
		if (location == PropertyStorageLocations.Unchanged)
		{
			location = propertyStorageLocations;
		}
		PropertyPosition position = (saveAfterImportsProperties.Contains(propertyName) ? PropertyPosition.UseExistingOrCreateAfterLastImport : PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup);
		if (propertyStorageLocations != location)
		{
			switch (location & PropertyStorageLocations.ConfigurationAndPlatformSpecific)
			{
			case PropertyStorageLocations.Unchanged:
				RemovePropertyCompletely(propertyName);
				break;
			case PropertyStorageLocations.ConfigurationSpecific:
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
				foreach (string configurationName in ConfigurationNames)
				{
					dictionary2[configurationName] = GetAnyUnevaluatedPropertyValue(configurationName, null, propertyName);
				}
				RemovePropertyCompletely(propertyName);
				foreach (KeyValuePair<string, string> item in dictionary2)
				{
					if (item.Value != null)
					{
						project.SetProperty(propertyName, item.Value, CreateCondition(item.Key, null, location), position, treatPropertyValueAsLiteral: false);
					}
				}
				break;
			}
			case PropertyStorageLocations.PlatformSpecific:
			{
				Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
				foreach (string platformName in PlatformNames)
				{
					dictionary3[platformName] = GetAnyUnevaluatedPropertyValue(null, platformName, propertyName);
				}
				RemovePropertyCompletely(propertyName);
				foreach (KeyValuePair<string, string> item2 in dictionary3)
				{
					if (item2.Value != null)
					{
						project.SetProperty(propertyName, item2.Value, CreateCondition(null, item2.Key, location), position, treatPropertyValueAsLiteral: false);
					}
				}
				break;
			}
			case PropertyStorageLocations.ConfigurationAndPlatformSpecific:
			{
				Dictionary<Pair<string, string>, string> dictionary = new Dictionary<Pair<string, string>, string>();
				foreach (string configurationName2 in ConfigurationNames)
				{
					foreach (string platformName2 in PlatformNames)
					{
						dictionary[new Pair<string, string>(configurationName2, platformName2)] = GetAnyUnevaluatedPropertyValue(configurationName2, platformName2, propertyName);
					}
				}
				RemovePropertyCompletely(propertyName);
				foreach (KeyValuePair<Pair<string, string>, string> item3 in dictionary)
				{
					if (item3.Value != null)
					{
						project.SetProperty(propertyName, item3.Value, CreateCondition(item3.Key.First, item3.Key.Second, location), position, treatPropertyValueAsLiteral: false);
					}
				}
				break;
			}
			default:
				throw new NotSupportedException();
			}
			buildProperty = FindPropertyObject(configuration, platform, propertyName, out group, out propertyStorageLocations);
		}
		ProjectPropertyChangedEventArgs e = new ProjectPropertyChangedEventArgs(propertyName);
		e.Configuration = configuration;
		e.Platform = platform;
		e.NewLocation = location;
		e.OldLocation = propertyStorageLocations;
		if (newValue != null)
		{
			e.NewValue = (treatPropertyValueAsLiteral ? MSBuildInternals.Escape(newValue) : newValue);
		}
		if (newValue == null)
		{
			if (group != null && buildProperty != null)
			{
				e.OldValue = buildProperty.Value;
				group.RemoveProperty(buildProperty);
				if (group.Count == 0)
				{
					project.RemovePropertyGroup(group);
				}
			}
		}
		else if (group != null && buildProperty != null)
		{
			e.OldValue = buildProperty.Value;
			project.SetProperty(propertyName, newValue, group.Condition, position, treatPropertyValueAsLiteral);
		}
		else
		{
			project.SetProperty(propertyName, newValue, CreateCondition(configuration, platform, location), position, treatPropertyValueAsLiteral);
		}
		return e;
	}

	private void RemovePropertyCompletely(string propertyName)
	{
		List<BuildPropertyGroup> list = new List<BuildPropertyGroup>();
		foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
		{
			if (!propertyGroup.IsImported)
			{
				propertyGroup.RemoveProperty(propertyName);
				if (propertyGroup.Count == 0)
				{
					list.Add(propertyGroup);
				}
			}
		}
		list.ForEach(project.RemovePropertyGroup);
	}

	private static string CreateCondition(string configuration, string platform)
	{
		if (configuration == null)
		{
			return CreateCondition(configuration, platform, PropertyStorageLocations.PlatformSpecific);
		}
		if (platform == null)
		{
			return CreateCondition(configuration, platform, PropertyStorageLocations.ConfigurationSpecific);
		}
		return CreateCondition(configuration, platform, PropertyStorageLocations.ConfigurationAndPlatformSpecific);
	}

	private static string CreateCondition(string configuration, string platform, PropertyStorageLocations location)
	{
		switch (location & PropertyStorageLocations.ConfigurationAndPlatformSpecific)
		{
		case PropertyStorageLocations.ConfigurationSpecific:
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			return " '$(Configuration)' == '" + configuration + "' ";
		case PropertyStorageLocations.PlatformSpecific:
			if (platform == null)
			{
				throw new ArgumentNullException("platform");
			}
			return " '$(Platform)' == '" + platform + "' ";
		case PropertyStorageLocations.ConfigurationAndPlatformSpecific:
			if (platform == null)
			{
				throw new ArgumentNullException("platform");
			}
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			return " '$(Configuration)|$(Platform)' == '" + configuration + "|" + platform + "' ";
		default:
			return null;
		}
	}

	internal void CreateItemsListFromMSBuild()
	{
		WorkbenchSingleton.AssertMainThread();
		lock (base.SyncRoot)
		{
			foreach (ProjectItem item in items)
			{
				item.Dispose();
			}
			items.Clear();
			itemsReadOnly = null;
			Set<ItemType> set = new Set<ItemType>();
			set.AddRange(ItemType.DefaultFileItems);
			foreach (BuildItem item2 in this.project.GetEvaluatedItemsByName("AvailableItemName"))
			{
				set.Add(new ItemType(item2.Include));
			}
			availableFileItemTypes = set.AsReadOnly();
			BuildItemGroup refs = null;
			try
			{
				Engine engine = new Engine(RuntimeEnvironment.GetRuntimeDirectory());
				Microsoft.Build.BuildEngine.Project project = new Microsoft.Build.BuildEngine.Project(engine);
				InitializeMSBuildProjectProperties(project.GlobalProperties);
				project.Load(base.FileName);
				if (project.Build("ResolveAssemblyReferences"))
				{
					refs = project.GetEvaluatedItemsByName("ReferencePath");
				}
			}
			catch (Exception)
			{
			}
			foreach (BuildItem evaluatedItem in this.project.EvaluatedItems)
			{
				if (!evaluatedItem.IsImported)
				{
					items.Add(CreateProjectItem(evaluatedItem, refs));
				}
			}
		}
		ClearFindFileCache();
	}

	void IProjectItemListProvider.AddProjectItem(ProjectItem item)
	{
		AddProjectItem(item);
	}

	protected virtual void AddProjectItem(ProjectItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (item.Project != this)
		{
			throw new ArgumentException("item does not belong to this project", "item");
		}
		if (item.IsAddedToProject)
		{
			if (items.Contains(item))
			{
				throw new ArgumentException("item is already added to project", "item");
			}
			item.BuildItem = null;
		}
		WorkbenchSingleton.AssertMainThread();
		lock (base.SyncRoot)
		{
			items.Add(item);
			itemsReadOnly = null;
			foreach (BuildItemGroup itemGroup in project.ItemGroups)
			{
				if (itemGroup.IsImported || !string.IsNullOrEmpty(itemGroup.Condition) || itemGroup.Count == 0)
				{
					continue;
				}
				if (itemGroup[0].Name == item.ItemType.ItemName)
				{
					MSBuildInternals.AddItemToGroup(itemGroup, item);
					return;
				}
				if (!(itemGroup[0].Name == "Reference"))
				{
					if (!ItemType.DefaultFileItems.Contains(new ItemType(itemGroup[0].Name)))
					{
						MSBuildInternals.AddItemToGroup(itemGroup, item);
						return;
					}
					if (ItemType.DefaultFileItems.Contains(item.ItemType))
					{
						MSBuildInternals.AddItemToGroup(itemGroup, item);
						return;
					}
				}
			}
			BuildItemGroup buildItemGroup2 = project.AddNewItemGroup();
			MSBuildInternals.AddItemToGroup(buildItemGroup2, item);
		}
	}

	bool IProjectItemListProvider.RemoveProjectItem(ProjectItem item)
	{
		return RemoveProjectItem(item);
	}

	protected virtual bool RemoveProjectItem(ProjectItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (item.Project != this)
		{
			throw new ArgumentException("item does not belong to this project", "item");
		}
		if (!item.IsAddedToProject)
		{
			return false;
		}
		WorkbenchSingleton.AssertMainThread();
		lock (base.SyncRoot)
		{
			if (items.Remove(item))
			{
				itemsReadOnly = null;
				project.RemoveItem(item.BuildItem);
				base.IsDirty = true;
				item.BuildItem = null;
				return true;
			}
			throw new InvalidOperationException("Expected that the item is added to this project!");
		}
	}

	public override void StartBuild(BuildOptions options)
	{
		RunMSBuild(ParentSolution, this, base.ActiveConfiguration, ActivePlatform, options);
	}

	internal static void RunMSBuild(Solution solution, IProject project, string configuration, string platform, BuildOptions options)
	{
		WorkbenchSingleton.Workbench.GetPad(typeof(CompilerMessageView)).BringPadToFront();
		MSBuildEngine mSBuildEngine = new MSBuildEngine();
		mSBuildEngine.Configuration = configuration;
		mSBuildEngine.Platform = platform;
		mSBuildEngine.MessageView = TaskService.BuildMessageViewCategory;
		mSBuildEngine.Run(solution, project, options);
	}

	internal static void InitializeMSBuildProject(Microsoft.Build.BuildEngine.Project project)
	{
		InitializeMSBuildProjectProperties(project.GlobalProperties);
	}

	internal static void InitializeMSBuildProjectProperties(BuildPropertyGroup propertyGroup)
	{
		foreach (KeyValuePair<string, string> mSBuildProperty in MSBuildEngine.MSBuildProperties)
		{
			propertyGroup.SetProperty(mSBuildProperty.Key, mSBuildProperty.Value);
		}
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/MSBuildEngine/AdditionalProperties", throwOnNotFound: false);
		if (treeNode == null)
		{
			return;
		}
		foreach (Codon codon in treeNode.Codons)
		{
			object obj = codon.BuildItem(null, new ArrayList());
			if (obj != null)
			{
				bool treatPropertyValueAsLiteral = !codon.Properties.Get("text", "").Contains("$(");
				propertyGroup.SetProperty(codon.Id, obj.ToString(), treatPropertyValueAsLiteral);
			}
		}
	}

	protected virtual void LoadProject(string fileName)
	{
		isLoading = true;
		try
		{
			base.FileName = fileName;
			InitializeMSBuildProject(project);
			try
			{
				project.Load(fileName);
			}
			catch (InvalidProjectFileException ex)
			{
				LoggingService.Warn(ex);
				if (!(ex.ErrorCode == "MSB4075"))
				{
					throw;
				}
				PrjxToSolutionProject.ConvertVSNetProject(fileName);
				project.Load(fileName);
			}
			base.ActiveConfiguration = GetEvaluatedProperty("Configuration") ?? base.ActiveConfiguration;
			ActivePlatform = GetEvaluatedProperty("Platform") ?? ActivePlatform;
			project.GlobalProperties.SetProperty("Configuration", base.ActiveConfiguration, treatPropertyValueAsLiteral: true);
			project.GlobalProperties.SetProperty("Platform", ActivePlatform, treatPropertyValueAsLiteral: true);
			CreateItemsListFromMSBuild();
			LoadConfigurationPlatformNamesFromMSBuild();
			base.IdGuid = GetEvaluatedProperty("ProjectGuid");
		}
		finally
		{
			isLoading = false;
		}
	}

	public override void Save(string fileName)
	{
		lock (base.SyncRoot)
		{
			if (project.IsDirty)
			{
				project.Save(fileName);
			}
		}
	}

	private static XmlElement GetProjectNode(XmlElement parent)
	{
		if (parent.Name == "Project")
		{
			return parent;
		}
		foreach (XmlElement childNode in parent.ChildNodes)
		{
			XmlElement projectNode = GetProjectNode(childNode);
			if (projectNode != null)
			{
				return projectNode;
			}
		}
		return null;
	}

	public override void AddToolsVersionAttribute()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(base.FileName);
		XmlElement projectNode = GetProjectNode(xmlDocument.DocumentElement);
		projectNode.SetAttribute("ToolsVersion", "4.0");
		xmlDocument.Save(base.FileName);
	}

	protected override void OnActiveConfigurationChanged(EventArgs e)
	{
		if (!isLoading)
		{
			lock (base.SyncRoot)
			{
				project.GlobalProperties.SetProperty("Configuration", base.ActiveConfiguration, treatPropertyValueAsLiteral: true);
				CreateItemsListFromMSBuild();
			}
		}
		base.OnActiveConfigurationChanged(e);
	}

	protected override void OnActivePlatformChanged(EventArgs e)
	{
		if (!isLoading)
		{
			lock (base.SyncRoot)
			{
				project.GlobalProperties.SetProperty("Platform", ActivePlatform, treatPropertyValueAsLiteral: true);
				CreateItemsListFromMSBuild();
			}
		}
		base.OnActivePlatformChanged(e);
	}

	private void LoadConfigurationPlatformNamesFromMSBuild()
	{
		Set<string> set = new Set<string>();
		Set<string> set2 = new Set<string>();
		foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
		{
			if (!propertyGroup.IsImported)
			{
				BuildProperty property = MSBuildInternals.GetProperty(propertyGroup, "Configuration");
				if (property != null && !string.IsNullOrEmpty(property.FinalValue))
				{
					set.Add(property.FinalValue);
				}
				property = MSBuildInternals.GetProperty(propertyGroup, "Platform");
				if (property != null && !string.IsNullOrEmpty(property.FinalValue))
				{
					set2.Add(property.FinalValue);
				}
				MSBuildInternals.GetConfigurationAndPlatformFromCondition(propertyGroup.Condition, out var configuration, out var platform);
				if (configuration != null)
				{
					set.Add(configuration);
				}
				if (platform != null)
				{
					set2.Add(platform);
				}
			}
		}
		if (set.Count == 0)
		{
			set.Add("Debug");
			set.Add("Release");
		}
		if (set2.Count == 0)
		{
			set2.Add("AnyCPU");
		}
		configurationNames = set.AsReadOnly();
		platformNames = set2.AsReadOnly();
	}

	bool IProjectAllowChangeConfigurations.RenameProjectConfiguration(string oldName, string newName)
	{
		lock (base.SyncRoot)
		{
			foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
			{
				if (!propertyGroup.IsImported)
				{
					BuildProperty property = MSBuildInternals.GetProperty(propertyGroup, "Configuration");
					if (property != null && property.Value == oldName)
					{
						property.Value = newName;
					}
					MSBuildInternals.GetConfigurationAndPlatformFromCondition(propertyGroup.Condition, out var configuration, out var platform);
					if (configuration == oldName)
					{
						propertyGroup.Condition = CreateCondition(newName, platform);
					}
				}
			}
			LoadConfigurationPlatformNamesFromMSBuild();
			return true;
		}
	}

	bool IProjectAllowChangeConfigurations.RenameProjectPlatform(string oldName, string newName)
	{
		lock (base.SyncRoot)
		{
			foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
			{
				if (!propertyGroup.IsImported)
				{
					BuildProperty property = MSBuildInternals.GetProperty(propertyGroup, "Platform");
					if (property != null && property.Value == oldName)
					{
						property.Value = newName;
					}
					MSBuildInternals.GetConfigurationAndPlatformFromCondition(propertyGroup.Condition, out var configuration, out var platform);
					if (platform == oldName)
					{
						propertyGroup.Condition = CreateCondition(configuration, newName);
					}
				}
			}
			LoadConfigurationPlatformNamesFromMSBuild();
			return true;
		}
	}

	bool IProjectAllowChangeConfigurations.AddProjectConfiguration(string newName, string copyFrom)
	{
		lock (base.SyncRoot)
		{
			bool flag = false;
			if (copyFrom != null)
			{
				foreach (BuildPropertyGroup item in Linq.ToList(Linq.CastTo<BuildPropertyGroup>(project.PropertyGroups)))
				{
					if (!item.IsImported)
					{
						MSBuildInternals.GetConfigurationAndPlatformFromCondition(item.Condition, out var configuration, out var platform);
						if (configuration == copyFrom)
						{
							CopyProperties(item, newName, platform);
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				project.AddNewPropertyGroup(insertAtEndOfProject: false).Condition = CreateCondition(newName, null);
			}
			LoadConfigurationPlatformNamesFromMSBuild();
			return true;
		}
	}

	bool IProjectAllowChangeConfigurations.AddProjectPlatform(string newName, string copyFrom)
	{
		lock (base.SyncRoot)
		{
			bool flag = false;
			if (copyFrom != null)
			{
				foreach (BuildPropertyGroup item in Linq.ToList(Linq.CastTo<BuildPropertyGroup>(project.PropertyGroups)))
				{
					if (!item.IsImported)
					{
						MSBuildInternals.GetConfigurationAndPlatformFromCondition(item.Condition, out var configuration, out var platform);
						if (platform == copyFrom)
						{
							CopyProperties(item, configuration, newName);
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				project.AddNewPropertyGroup(insertAtEndOfProject: false).Condition = CreateCondition(null, newName);
			}
			LoadConfigurationPlatformNamesFromMSBuild();
			return true;
		}
	}

	private void CopyProperties(BuildPropertyGroup g, string newConfiguration, string newPlatform)
	{
		BuildPropertyGroup buildPropertyGroup = project.AddNewPropertyGroup(insertAtEndOfProject: false);
		buildPropertyGroup.Condition = CreateCondition(newConfiguration, newPlatform);
		foreach (BuildProperty item in g)
		{
			buildPropertyGroup.AddNewProperty(item.Name, item.Value);
		}
	}

	bool IProjectAllowChangeConfigurations.RemoveProjectConfiguration(string name)
	{
		lock (base.SyncRoot)
		{
			string text = null;
			foreach (string configurationName in ConfigurationNames)
			{
				if (configurationName != name)
				{
					text = name;
					break;
				}
			}
			if (text == null)
			{
				throw new InvalidOperationException("cannot remove the last configuration");
			}
			foreach (BuildPropertyGroup item in Linq.ToList(Linq.CastTo<BuildPropertyGroup>(project.PropertyGroups)))
			{
				if (!item.IsImported)
				{
					BuildProperty property = MSBuildInternals.GetProperty(item, "Configuration");
					if (property != null && property.Value == name)
					{
						property.Value = text;
					}
					MSBuildInternals.GetConfigurationAndPlatformFromCondition(item.Condition, out var configuration, out var _);
					if (configuration == name)
					{
						project.RemovePropertyGroup(item);
					}
				}
			}
			LoadConfigurationPlatformNamesFromMSBuild();
			return true;
		}
	}

	bool IProjectAllowChangeConfigurations.RemoveProjectPlatform(string name)
	{
		lock (base.SyncRoot)
		{
			string text = null;
			foreach (string platformName in PlatformNames)
			{
				if (platformName != name)
				{
					text = name;
					break;
				}
			}
			if (text == null)
			{
				throw new InvalidOperationException("cannot remove the last platform");
			}
			foreach (BuildPropertyGroup item in Linq.ToList(Linq.CastTo<BuildPropertyGroup>(project.PropertyGroups)))
			{
				if (!item.IsImported)
				{
					BuildProperty property = MSBuildInternals.GetProperty(item, "Platform");
					if (property != null && property.Value == name)
					{
						property.Value = text;
					}
					MSBuildInternals.GetConfigurationAndPlatformFromCondition(item.Condition, out var _, out var platform);
					if (platform == name)
					{
						project.RemovePropertyGroup(item);
					}
				}
			}
			LoadConfigurationPlatformNamesFromMSBuild();
			return true;
		}
	}
}
