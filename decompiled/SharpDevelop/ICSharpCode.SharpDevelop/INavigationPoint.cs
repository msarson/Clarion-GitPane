using System;

namespace ICSharpCode.SharpDevelop;

public interface INavigationPoint : IComparable
{
	string FileName { get; }

	string Description { get; }

	string FullDescription { get; }

	string ToolTip { get; }

	object NavigationData { get; }

	int Index { get; }

	void JumpTo();

	void FileNameChanged(string newName);

	void ContentChanging(object sender, EventArgs e);
}
