using System;
using ICSharpCode.Core;
using VisualHint.SmartPropertyGrid;

namespace ICSharpCode.SharpDevelop.Gui;

public class PropertyPadResetCommand : AbstractMenuCommand
{
	public override void Run()
	{
		try
		{
			PropertyVisibleDeepEnumerator selectedPropertyEnumerator = PropertyPad.Grid.SelectedPropertyEnumerator;
			if (selectedPropertyEnumerator != null)
			{
				selectedPropertyEnumerator.Property.Value.ResetToDefaultValue();
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Gui.Pads.PropertyPadResetCommand}" + Environment.NewLine + ex.Message);
		}
	}
}
