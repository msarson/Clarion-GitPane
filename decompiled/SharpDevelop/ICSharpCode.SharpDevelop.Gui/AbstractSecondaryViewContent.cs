using System;

namespace ICSharpCode.SharpDevelop.Gui;

public abstract class AbstractSecondaryViewContent : AbstractBaseViewContent, ISecondaryViewContent, IBaseViewContent, IDisposable
{
	public virtual bool Visible => true;

	public virtual void NotifyBeforeSave()
	{
	}

	public virtual void NotifyAfterSave(bool successful)
	{
	}

	public virtual void NotifyFileNameChanged()
	{
	}
}
