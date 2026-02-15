using System.Drawing.Printing;

namespace ICSharpCode.SharpDevelop.Gui;

public interface IPrintable
{
	PrintDocument PrintDocument { get; }
}
