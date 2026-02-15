using System;

namespace ICSharpCode.SharpDevelop.Project;

[Flags]
public enum PropertyStorageLocations
{
	Unchanged = 0,
	Unknown = 0,
	Base = 1,
	ConfigurationSpecific = 2,
	PlatformSpecific = 4,
	ConfigurationAndPlatformSpecific = 6,
	UserFile = 8
}
