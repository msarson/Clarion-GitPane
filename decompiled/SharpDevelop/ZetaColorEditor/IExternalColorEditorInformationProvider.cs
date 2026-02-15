using System.Collections.Generic;
using System.Drawing;
using ZetaColorEditor.Runtime.Colors;

namespace ZetaColorEditor;

public interface IExternalColorEditorInformationProvider
{
	IColorScheme[] ColorSchemes { get; }

	bool AllowNoColorSelectable { get; }

	void FormatDisplayText(Color color, ref string displayText);

	void AdjustColorSettingLookupOrder(IList<ColorLookupElement> lookupOrder);

	void SavePerUserPerWorkstationValue(string name, string value);

	string RestorePerUserPerWorkstationValue(string name, string fallBackTo);
}
