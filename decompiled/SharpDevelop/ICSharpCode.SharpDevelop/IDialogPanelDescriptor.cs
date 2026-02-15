using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop;

public interface IDialogPanelDescriptor
{
	string ID { get; }

	string Label { get; set; }

	IEnumerable<IDialogPanelDescriptor> ChildDialogPanelDescriptors { get; }

	IDialogPanel DialogPanel { get; }
}
