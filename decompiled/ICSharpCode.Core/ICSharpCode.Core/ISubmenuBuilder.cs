using System.Windows.Forms;

namespace ICSharpCode.Core;

public interface ISubmenuBuilder
{
	ToolStripItem[] BuildSubmenu(Codon codon, object owner);
}
