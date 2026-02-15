using System.Drawing;

namespace ZetaColorEditor.Runtime.Colors;

public interface IColorScheme
{
	Color[] Colors { get; }

	string Name { get; }
}
