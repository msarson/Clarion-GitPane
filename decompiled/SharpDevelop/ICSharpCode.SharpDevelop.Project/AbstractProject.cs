using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public abstract class AbstractProject : AbstractSolutionFolder, IProject, ISolutionFolder, IMementoCapable, ICanBeDirty, INamedComponent, IComponent, IDisposable
{
	private bool isDisposed;

	internal static List<string> filesToOpenAfterSolutionLoad = new List<string>();

	private volatile string fileName;

	private string cachedDirectoryName;

	private List<ProjectSection> projectSections = new List<ProjectSection>();

	private string activeConfiguration = "Debug";

	private string activePlatform = "AnyCPU";

	private Dictionary<string, FileProjectItem> findFileCache;

	private bool isDirty;

	private ISite m_site;

	[Browsable(false)]
	public bool IsDisposed => isDisposed;

	[Browsable(false)]
	public virtual string ProjectType => "MSBuild";

	[ReadOnly(true)]
	public string FileName
	{
		get
		{
			return fileName ?? "";
		}
		set
		{
			WorkbenchSingleton.AssertMainThread();
			lock (base.SyncRoot)
			{
				fileName = value;
				cachedDirectoryName = null;
			}
		}
	}

	[Browsable(false)]
	public string Directory
	{
		get
		{
			lock (base.SyncRoot)
			{
				if (cachedDirectoryName == null)
				{
					try
					{
						cachedDirectoryName = Path.GetDirectoryName(FileName);
					}
					catch (Exception)
					{
						cachedDirectoryName = "";
					}
				}
				return cachedDirectoryName;
			}
		}
	}

	[Browsable(false)]
	public List<ProjectSection> ProjectSections
	{
		get
		{
			WorkbenchSingleton.AssertMainThread();
			return projectSections;
		}
	}

	[Browsable(false)]
	public virtual LanguageProperties LanguageProperties => LanguageProperties.None;

	[Browsable(false)]
	public virtual IAmbience Ambience => null;

	[LocalizedProperty("${res:Dialog.Options.CombineOptions.Configurations.ConfigurationColumnHeader}")]
	[ReadOnly(true)]
	public string ActiveConfiguration
	{
		get
		{
			return activeConfiguration;
		}
		set
		{
			WorkbenchSingleton.AssertMainThread();
			if (activeConfiguration != value)
			{
				activeConfiguration = value;
				OnActiveConfigurationChanged(EventArgs.Empty);
			}
		}
	}

	[LocalizedProperty("${res:Dialog.ProjectOptions.Platform}")]
	[ReadOnly(true)]
	public virtual string ActivePlatform
	{
		get
		{
			return activePlatform;
		}
		set
		{
			WorkbenchSingleton.AssertMainThread();
			if (activePlatform != value)
			{
				activePlatform = value;
				OnActivePlatformChanged(EventArgs.Empty);
			}
		}
	}

	[Browsable(false)]
	public virtual ICollection<string> ConfigurationNames => new string[2] { "Debug", "Release" };

	[Browsable(false)]
	public virtual ICollection<string> PlatformNames => new string[1] { "AnyCPU" };

	[Browsable(false)]
	public virtual ICollection<ItemType> AvailableFileItemTypes => ItemType.DefaultFileItems;

	[Browsable(false)]
	public virtual ReadOnlyCollection<ProjectItem> Items => new ReadOnlyCollection<ProjectItem>(new ProjectItem[0]);

	[ReadOnly(true)]
	public virtual string AssemblyName
	{
		get
		{
			return base.Name;
		}
		set
		{
		}
	}

	[Browsable(false)]
	public virtual string RootNamespace
	{
		get
		{
			return base.Name;
		}
		set
		{
		}
	}

	[Browsable(false)]
	public virtual string TitleName => base.Name;

	[Browsable(false)]
	public virtual string VersionName => string.Empty;

	[Browsable(false)]
	public virtual string OutputAssemblyFullPath => null;

	[Browsable(false)]
	public virtual string AppDesignerFolder
	{
		get
		{
			return "";
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	[ReadOnly(true)]
	public virtual string Language => "";

	[Browsable(false)]
	public virtual bool IsStartable => false;

	[Browsable(false)]
	public bool IsDirty
	{
		get
		{
			return isDirty;
		}
		set
		{
			isDirty = value;
			if (this.DirtyChanged != null)
			{
				this.DirtyChanged(this, EventArgs.Empty);
			}
		}
	}

	[Browsable(false)]
	public ISite Site
	{
		get
		{
			return m_site;
		}
		set
		{
			m_site = value;
		}
	}

	public event EventHandler Disposed;

	public event EventHandler ActiveConfigurationChanged;

	public event EventHandler ActivePlatformChanged;

	public event EventHandler DirtyChanged;

	public static string GetConfigurationNameFromKey(string key)
	{
		int num = key.IndexOf('|');
		if (num < 0)
		{
			return key;
		}
		return key.Substring(0, num);
	}

	public static string GetPlatformNameFromKey(string key)
	{
		return key.Substring(key.IndexOf('|') + 1);
	}

	public virtual void Dispose()
	{
		WorkbenchSingleton.AssertMainThread();
		isDisposed = true;
		if (this.Disposed != null)
		{
			this.Disposed(this, EventArgs.Empty);
		}
	}

	public virtual Properties CreateMemento()
	{
		WorkbenchSingleton.AssertMainThread();
		Properties properties = new Properties();
		properties.Set("bookmarks", BookmarkManager.GetProjectBookmarks(this).ToArray());
		List<string> list = new List<string>();
		if (WorkbenchSingleton.Workbench != null)
		{
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				string text = item.FileName;
				if (text != null && IsFileInProject(text))
				{
					list.Add(text);
				}
			}
		}
		properties.Set("files", list.ToArray());
		return properties;
	}

	public virtual void SetMemento(Properties memento)
	{
		WorkbenchSingleton.AssertMainThread();
		SDBookmark[] array = memento.Get("bookmarks", new SDBookmark[0]);
		foreach (SDBookmark bookmark in array)
		{
			BookmarkManager.AddMark(bookmark);
		}
		string[] array2 = memento.Get("files", new string[0]);
		foreach (string item in array2)
		{
			filesToOpenAfterSolutionLoad.Add(item);
		}
	}

	protected virtual void OnActiveConfigurationChanged(EventArgs e)
	{
		if (this.ActiveConfigurationChanged != null)
		{
			this.ActiveConfigurationChanged(this, e);
		}
	}

	protected virtual void OnActivePlatformChanged(EventArgs e)
	{
		if (this.ActivePlatformChanged != null)
		{
			this.ActivePlatformChanged(this, e);
		}
	}

	public void Save()
	{
		Save(FileName);
		isDirty = false;
	}

	public virtual void Save(string fileName)
	{
		isDirty = false;
	}

	public virtual void AddToolsVersionAttribute()
	{
	}

	public bool ValidateItemsFileName(NamedFileValidationDelegate visitor)
	{
		lock (base.SyncRoot)
		{
			foreach (ProjectItem item in Items)
			{
				if (!visitor(item.FileName))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool VisitItemsFileName(NamedFileValidationDelegate visitor)
	{
		bool flag = true;
		lock (base.SyncRoot)
		{
			foreach (ProjectItem item in Items)
			{
				flag = flag && visitor(item.FileName);
			}
			return flag;
		}
	}

	public virtual IEnumerable<ProjectItem> GetValidItems(ProjectItem.ValidationDelegate visitor)
	{
		foreach (ProjectItem item in Items)
		{
			if (visitor(item))
			{
				yield return item;
			}
		}
	}

	public virtual IEnumerable<ProjectItem> GetItemsOfType(ItemType itemType)
	{
		foreach (ProjectItem item in Items)
		{
			if (item.ItemType == itemType)
			{
				yield return item;
			}
		}
	}

	public virtual void Start(bool withDebugging)
	{
		throw new NotSupportedException();
	}

	public virtual bool IsFileInProject(string fileName)
	{
		return FindFile(fileName) != null;
	}

	protected internal void ClearFindFileCache()
	{
		lock (base.SyncRoot)
		{
			findFileCache = null;
		}
	}

	public FileProjectItem FindFile(string fileName)
	{
		lock (base.SyncRoot)
		{
			if (findFileCache == null)
			{
				findFileCache = new Dictionary<string, FileProjectItem>(StringComparer.InvariantCultureIgnoreCase);
				foreach (ProjectItem item in Items)
				{
					if (item is FileProjectItem value)
					{
						findFileCache[item.FileName] = value;
					}
				}
			}
			try
			{
				if (fileName != null)
				{
					fileName = Path.GetFullPath(fileName);
				}
			}
			catch
			{
			}
			if (fileName != null)
			{
				findFileCache.TryGetValue(fileName, out var value2);
				return value2;
			}
			return null;
		}
	}

	ParseProjectContent IProject.CreateProjectContent()
	{
		return CreateProjectContent();
	}

	protected virtual ParseProjectContent CreateProjectContent()
	{
		return null;
	}

	public virtual void StartBuild(BuildOptions options)
	{
	}

	public virtual ProjectItem CreateProjectItem(BuildItem item)
	{
		return new UnknownProjectItem(this, item);
	}

	public override string ToString()
	{
		return $"[{GetType().Name}: {base.Name}]";
	}

	public virtual ItemType GetDefaultItemType(string fileName)
	{
		return ItemType.None;
	}

	public string TypeName()
	{
		return "Project Properties";
	}
}
