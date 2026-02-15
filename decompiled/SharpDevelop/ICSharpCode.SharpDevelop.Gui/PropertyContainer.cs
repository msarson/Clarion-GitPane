using System.Collections;
using System.ComponentModel.Design;

namespace ICSharpCode.SharpDevelop.Gui;

public sealed class PropertyContainer
{
	private object selectedObject;

	private object[] selectedObjects;

	private ICollection selectableObjects;

	private IDesignerHost host;

	public object SelectedObject
	{
		get
		{
			return selectedObject;
		}
		set
		{
			selectedObject = value;
			selectedObjects = null;
			PropertyPad.UpdateSelectedObjectIfActive(this);
		}
	}

	public object[] SelectedObjects
	{
		get
		{
			return selectedObjects;
		}
		set
		{
			selectedObject = null;
			selectedObjects = value;
			PropertyPad.UpdateSelectedObjectIfActive(this);
		}
	}

	public ICollection SelectableObjects
	{
		get
		{
			return selectableObjects;
		}
		set
		{
			selectableObjects = value;
			PropertyPad.UpdateSelectableIfActive(this);
		}
	}

	public IDesignerHost Host
	{
		get
		{
			return host;
		}
		set
		{
			host = value;
			PropertyPad.UpdateHostIfActive(this);
		}
	}

	public void Clear()
	{
		Host = null;
		SelectableObjects = null;
		SelectedObject = null;
	}
}
