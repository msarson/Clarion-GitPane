using System;

namespace ICSharpCode.Core;

public interface ICommand
{
	object Owner { get; set; }

	event EventHandler OwnerChanged;

	void Run();
}
