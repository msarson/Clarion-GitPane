using System;

namespace SoftVelocity.Common.CodeCompletion;

public class SelectedItemEventArgs : EventArgs
{
	private readonly object selectedItem;

	public object SelectedItem => selectedItem;

	public SelectedItemEventArgs(object selectedItem)
	{
		this.selectedItem = selectedItem;
	}
}
