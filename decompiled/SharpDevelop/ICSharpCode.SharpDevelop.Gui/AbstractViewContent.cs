using System;
using System.Collections.Generic;
using System.IO;

namespace ICSharpCode.SharpDevelop.Gui;

public abstract class AbstractViewContent : AbstractBaseViewContent, IViewContent, IBaseViewContent, IDisposable, ICanBeDirty
{
	private string untitledName = string.Empty;

	private string titleName;

	private string fileName;

	private bool isViewOnly;

	private List<ISecondaryViewContent> secondaryViewContents = new List<ISecondaryViewContent>();

	private bool isDirty;

	public virtual string UntitledName
	{
		get
		{
			return untitledName;
		}
		set
		{
			untitledName = value;
		}
	}

	public virtual string TitleName
	{
		get
		{
			return titleName ?? Path.GetFileName(untitledName);
		}
		set
		{
			titleName = value;
			OnTitleNameChanged(EventArgs.Empty);
		}
	}

	public virtual string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
			OnFileNameChanged(EventArgs.Empty);
		}
	}

	public virtual bool IsUntitled => titleName == null;

	public virtual bool IsReadOnly => false;

	public virtual bool IsViewOnly
	{
		get
		{
			return isViewOnly;
		}
		set
		{
			isViewOnly = value;
		}
	}

	public List<ISecondaryViewContent> SecondaryViewContents => secondaryViewContents;

	public virtual bool IsDirty
	{
		get
		{
			return isDirty;
		}
		set
		{
			if (isDirty != value)
			{
				isDirty = value;
				OnDirtyChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler FileNameChanged;

	public event EventHandler TitleNameChanged;

	public event EventHandler Saving;

	public event SaveEventHandler Saved;

	public event EventHandler DirtyChanged;

	public AbstractViewContent()
	{
	}

	public AbstractViewContent(string titleName)
	{
		this.titleName = titleName;
	}

	public AbstractViewContent(string titleName, string fileName)
	{
		this.titleName = titleName;
		this.fileName = fileName;
	}

	protected void SetTitleAndFileName(string fileName)
	{
		TitleName = Path.GetFileName(fileName);
		FileName = fileName;
		IsDirty = false;
	}

	protected virtual void OnFileNameChanged(EventArgs e)
	{
		foreach (ISecondaryViewContent secondaryViewContent in SecondaryViewContents)
		{
			secondaryViewContent.NotifyFileNameChanged();
		}
		if (this.FileNameChanged != null)
		{
			this.FileNameChanged(this, e);
		}
	}

	public virtual void Save()
	{
		if (IsDirty)
		{
			Save(fileName);
		}
	}

	public virtual void Save(string fileName)
	{
		throw new NotImplementedException();
	}

	public virtual void Load(string fileName)
	{
		throw new NotImplementedException();
	}

	public virtual INavigationPoint BuildNavPoint()
	{
		return new DefaultNavigationPoint(FileName);
	}

	protected virtual void OnTitleNameChanged(EventArgs e)
	{
		if (this.TitleNameChanged != null)
		{
			this.TitleNameChanged(this, e);
		}
	}

	protected virtual void OnSaving(EventArgs e)
	{
		OnSecondaryContentsSaving();
		if (this.Saving != null)
		{
			this.Saving(this, e);
		}
	}

	protected virtual void OnSecondaryContentsSaving()
	{
		foreach (ISecondaryViewContent secondaryViewContent in SecondaryViewContents)
		{
			secondaryViewContent.NotifyBeforeSave();
		}
	}

	protected virtual void OnSaved(SaveEventArgs e)
	{
		OnSecondaryContentsSaved(e);
		if (this.Saved != null)
		{
			this.Saved(this, e);
		}
	}

	protected virtual void OnSecondaryContentsSaved(SaveEventArgs e)
	{
		foreach (ISecondaryViewContent secondaryViewContent in SecondaryViewContents)
		{
			secondaryViewContent.NotifyAfterSave(e.Successful);
		}
	}

	public override void Dispose()
	{
		foreach (ISecondaryViewContent secondaryViewContent in secondaryViewContents)
		{
			secondaryViewContent.Dispose();
		}
		base.Dispose();
	}

	protected virtual void OnDirtyChanged(EventArgs e)
	{
		if (this.DirtyChanged != null)
		{
			this.DirtyChanged(this, e);
		}
	}
}
