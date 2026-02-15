using System;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser;

[Flags]
public enum ClassBrowserFilter
{
	None = 0,
	ShowProjectReferences = 1,
	ShowBaseAndDerivedTypes = 0x20,
	ShowPublic = 2,
	ShowProtected = 4,
	ShowPrivate = 8,
	ShowOther = 0x10,
	ShowIncluded = 0x40,
	All = 0x3F
}
