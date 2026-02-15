using System;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop.Gui;

public interface IViewContent : IBaseViewContent, IDisposable, ICanBeDirty
{
	string UntitledName { get; set; }

	string TitleName { get; set; }

	string FileName { get; set; }

	bool IsUntitled { get; }

	bool IsReadOnly { get; }

	bool IsViewOnly { get; }

	List<ISecondaryViewContent> SecondaryViewContents { get; }

	event EventHandler TitleNameChanged;

	event EventHandler FileNameChanged;

	event EventHandler Saving;

	event SaveEventHandler Saved;

	void Save();

	void Save(string fileName);

	void Load(string fileName);

	INavigationPoint BuildNavPoint();
}
