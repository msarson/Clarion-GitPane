using System.Collections.Generic;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class DefaultDialogPanelDescriptor : IDialogPanelDescriptor
{
	private string id = string.Empty;

	private string label = string.Empty;

	private List<IDialogPanelDescriptor> dialogPanelDescriptors;

	private IDialogPanel dialogPanel;

	private AddIn addin;

	private string dialogPanelPath;

	public string ID => id;

	public string Label
	{
		get
		{
			return label;
		}
		set
		{
			label = value;
		}
	}

	public IEnumerable<IDialogPanelDescriptor> ChildDialogPanelDescriptors => dialogPanelDescriptors;

	public IDialogPanel DialogPanel
	{
		get
		{
			if (dialogPanelPath != null)
			{
				if (dialogPanel == null)
				{
					dialogPanel = (IDialogPanel)addin.CreateObject(dialogPanelPath);
				}
				dialogPanelPath = null;
				addin = null;
			}
			return dialogPanel;
		}
		set
		{
			dialogPanel = value;
		}
	}

	public DefaultDialogPanelDescriptor(string id, string label)
	{
		this.id = id;
		this.label = label;
	}

	public DefaultDialogPanelDescriptor(string id, string label, List<IDialogPanelDescriptor> dialogPanelDescriptors)
		: this(id, label)
	{
		this.dialogPanelDescriptors = dialogPanelDescriptors;
	}

	public DefaultDialogPanelDescriptor(string id, string label, AddIn addin, string dialogPanelPath)
		: this(id, label)
	{
		this.addin = addin;
		this.dialogPanelPath = dialogPanelPath;
	}
}
