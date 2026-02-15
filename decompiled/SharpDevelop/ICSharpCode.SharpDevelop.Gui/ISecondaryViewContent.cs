using System;

namespace ICSharpCode.SharpDevelop.Gui;

public interface ISecondaryViewContent : IBaseViewContent, IDisposable
{
	bool Visible { get; }

	void NotifyBeforeSave();

	void NotifyAfterSave(bool successful);

	void NotifyFileNameChanged();
}
