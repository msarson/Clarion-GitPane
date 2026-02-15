using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public abstract class ProjectItem : LocalizedObject, ICloneable, IComponent, IDisposable, INamedComponent
{
	public delegate bool ValidationDelegate(ProjectItem item);

	private IProject project;

	private volatile string fileNameCache;

	private BuildItem buildItem;

	private string virtualInclude;

	private ItemType virtualItemType;

	private Dictionary<string, string> virtualMetadata = new Dictionary<string, string>();

	private bool disposed;

	private ISite m_site;

	[Browsable(false)]
	public IProject Project => project;

	private object SyncRoot
	{
		get
		{
			if (project != null)
			{
				return project.SyncRoot;
			}
			return virtualMetadata;
		}
	}

	[Browsable(false)]
	public bool IsAddedToProject => buildItem != null;

	[Browsable(false)]
	internal BuildItem BuildItem
	{
		get
		{
			return buildItem;
		}
		set
		{
			if (project is AbstractProject)
			{
				((AbstractProject)project).ClearFindFileCache();
			}
			if (value != null)
			{
				virtualMetadata = null;
				virtualItemType = default(ItemType);
				virtualInclude = null;
			}
			else
			{
				virtualItemType = ItemType;
				virtualInclude = Include;
				virtualMetadata = new Dictionary<string, string>();
				foreach (string metadataName in MetadataNames)
				{
					virtualMetadata[metadataName] = GetMetadata(metadataName);
				}
			}
			buildItem = value;
		}
	}

	[Browsable(false)]
	public ItemType ItemType
	{
		get
		{
			lock (SyncRoot)
			{
				if (buildItem != null)
				{
					return new ItemType(buildItem.Name);
				}
				return virtualItemType;
			}
		}
		set
		{
			lock (SyncRoot)
			{
				if (buildItem != null)
				{
					buildItem.Name = value.ToString();
				}
				else
				{
					virtualItemType = value;
				}
			}
		}
	}

	[Browsable(false)]
	public string Include
	{
		get
		{
			lock (SyncRoot)
			{
				if (buildItem != null)
				{
					return buildItem.FinalItemSpec;
				}
				return virtualInclude;
			}
		}
		set
		{
			lock (SyncRoot)
			{
				if (project is AbstractProject)
				{
					((AbstractProject)project).ClearFindFileCache();
				}
				if (buildItem != null)
				{
					buildItem.Include = MSBuildInternals.Escape(value);
				}
				else
				{
					virtualInclude = value ?? "";
				}
				fileNameCache = null;
			}
		}
	}

	[Browsable(false)]
	public IEnumerable<string> MetadataNames
	{
		get
		{
			lock (SyncRoot)
			{
				if (buildItem != null)
				{
					return MSBuildInternals.GetCustomMetadataNames(buildItem);
				}
				return Linq.ToArray(virtualMetadata.Keys);
			}
		}
	}

	[Browsable(false)]
	public virtual string FileName
	{
		get
		{
			if (project == null)
			{
				return Include;
			}
			string text = fileNameCache;
			if (text == null)
			{
				lock (SyncRoot)
				{
					text = Path.Combine(project.Directory, Include);
					try
					{
						if (Path.IsPathRooted(text))
						{
							text = Path.GetFullPath(text);
						}
					}
					catch
					{
					}
					fileNameCache = text;
				}
			}
			return text;
		}
		set
		{
			if (project == null)
			{
				throw new NotSupportedException("Not supported for items without project.");
			}
			Include = FileUtility.GetRelativePath(project.Directory, value);
		}
	}

	[Browsable(false)]
	public bool IsDisposed => disposed;

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

	protected ProjectItem(IProject project, BuildItem buildItem)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		this.project = project;
		this.buildItem = buildItem;
	}

	protected ProjectItem(IProject project, ItemType itemType)
		: this(project, itemType, null)
	{
	}

	protected ProjectItem(IProject project, ItemType itemType, string include)
	{
		this.project = project;
		virtualItemType = itemType;
		virtualInclude = include ?? "";
		virtualMetadata = new Dictionary<string, string>();
	}

	public bool HasMetadata(string metadataName)
	{
		lock (SyncRoot)
		{
			if (buildItem != null)
			{
				return buildItem.HasMetadata(metadataName);
			}
			return virtualMetadata.ContainsKey(metadataName);
		}
	}

	public string GetEvaluatedMetadata(string metadataName)
	{
		lock (SyncRoot)
		{
			if (buildItem != null)
			{
				return buildItem.GetEvaluatedMetadata(metadataName) ?? "";
			}
			virtualMetadata.TryGetValue(metadataName, out var value);
			if (value == null)
			{
				return "";
			}
			return MSBuildInternals.Unescape(value);
		}
	}

	public T GetEvaluatedMetadata<T>(string metadataName, T defaultValue)
	{
		return GenericConverter.FromString(GetEvaluatedMetadata(metadataName), defaultValue);
	}

	public string GetMetadata(string metadataName)
	{
		lock (SyncRoot)
		{
			if (buildItem != null)
			{
				return buildItem.GetMetadata(metadataName) ?? "";
			}
			virtualMetadata.TryGetValue(metadataName, out var value);
			return value ?? "";
		}
	}

	public void SetEvaluatedMetadata(string metadataName, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			RemoveMetadata(metadataName);
			return;
		}
		lock (SyncRoot)
		{
			if (buildItem != null)
			{
				buildItem.SetMetadata(metadataName, value, treatMetadataValueAsLiteral: true);
			}
			else
			{
				virtualMetadata[metadataName] = MSBuildInternals.Escape(value);
			}
		}
	}

	public void SetEvaluatedMetadata<T>(string metadataName, T value)
	{
		SetEvaluatedMetadata(metadataName, GenericConverter.ToString(value));
	}

	public void SetMetadata(string metadataName, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			RemoveMetadata(metadataName);
			return;
		}
		lock (SyncRoot)
		{
			if (buildItem != null)
			{
				buildItem.SetMetadata(metadataName, value);
			}
			else
			{
				virtualMetadata[metadataName] = value;
			}
		}
	}

	public void RemoveMetadata(string metadataName)
	{
		lock (SyncRoot)
		{
			if (buildItem != null)
			{
				buildItem.RemoveMetadata(metadataName);
			}
			else
			{
				virtualMetadata.Remove(metadataName);
			}
		}
	}

	public virtual void CopyMetadataTo(ProjectItem targetItem)
	{
		lock (SyncRoot)
		{
			lock (targetItem.SyncRoot)
			{
				if (buildItem != null && targetItem.buildItem != null)
				{
					buildItem.CopyCustomMetadataTo(targetItem.buildItem);
					return;
				}
				foreach (string metadataName in MetadataNames)
				{
					targetItem.SetMetadata(metadataName, GetMetadata(metadataName));
				}
			}
		}
	}

	public virtual ProjectItem Clone()
	{
		if (Project != null)
		{
			return CloneFor(Project);
		}
		throw new NotSupportedException();
	}

	public ProjectItem CloneFor(IProject targetProject)
	{
		if (targetProject == null)
		{
			throw new ArgumentNullException("project");
		}
		ProjectItem projectItem = targetProject.CreateProjectItem(CloneBuildItem());
		projectItem.BuildItem = null;
		return projectItem;
	}

	private BuildItem CloneBuildItem()
	{
		lock (SyncRoot)
		{
			if (this.buildItem != null)
			{
				return this.buildItem.Clone();
			}
			BuildItem buildItem = new BuildItem(ItemType.ToString(), Include);
			foreach (string metadataName in MetadataNames)
			{
				buildItem.SetMetadata(metadataName, GetMetadata(metadataName));
			}
			return buildItem;
		}
	}

	object ICloneable.Clone()
	{
		return Clone();
	}

	public virtual void Dispose()
	{
		if (this.Disposed != null)
		{
			this.Disposed(null, null);
		}
		disposed = true;
	}

	public override string ToString()
	{
		return $"[{GetType().Name}: <{ItemType.ItemName} Include='{Include}'>]";
	}

	public override void InformSetValue(LocalizedPropertyDescriptor localizedPropertyDescriptor, object component, object value)
	{
		base.InformSetValue(localizedPropertyDescriptor, component, value);
		if (project != null)
		{
			project.Save();
		}
	}

	public virtual string TypeName()
	{
		return "File Properties";
	}
}
