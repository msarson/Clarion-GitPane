using System;

namespace ICSharpCode.SharpDevelop.Gui;

public interface ICanBeDirty
{
	bool IsDirty { get; set; }

	event EventHandler DirtyChanged;
}
