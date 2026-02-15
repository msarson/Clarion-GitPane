using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ICSharpCode.Core;

public class AddInReference : ICloneable
{
	private string name;

	private Version minimumVersion;

	private Version maximumVersion;

	private static Version entryVersion;

	public Version MinimumVersion => minimumVersion;

	public Version MaximumVersion => maximumVersion;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("name");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("name cannot be an empty string", "name");
			}
			name = value;
		}
	}

	public bool Check(Dictionary<string, Version> addIns, out Version versionFound)
	{
		if (addIns.TryGetValue(name, out versionFound))
		{
			if (CompareVersion(versionFound, minimumVersion) >= 0)
			{
				return CompareVersion(versionFound, maximumVersion) <= 0;
			}
			return false;
		}
		return false;
	}

	private int CompareVersion(Version a, Version b)
	{
		if (a.Major != b.Major)
		{
			if (a.Major <= b.Major)
			{
				return -1;
			}
			return 1;
		}
		if (a.Minor != b.Minor)
		{
			if (a.Minor <= b.Minor)
			{
				return -1;
			}
			return 1;
		}
		if (a.Build < 0 || b.Build < 0)
		{
			return 0;
		}
		if (a.Build != b.Build)
		{
			if (a.Build <= b.Build)
			{
				return -1;
			}
			return 1;
		}
		if (a.Revision < 0 || b.Revision < 0)
		{
			return 0;
		}
		if (a.Revision != b.Revision)
		{
			if (a.Revision <= b.Revision)
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}

	public static AddInReference Create(Properties properties, string hintPath)
	{
		AddInReference addInReference = new AddInReference(properties["addin"]);
		string text = properties["version"];
		if (text != null && text.Length > 0)
		{
			int num = text.IndexOf('-');
			if (num > 0)
			{
				addInReference.minimumVersion = ParseVersion(text.Substring(0, num), hintPath);
				addInReference.maximumVersion = ParseVersion(text.Substring(num + 1), hintPath);
			}
			else
			{
				addInReference.maximumVersion = (addInReference.minimumVersion = ParseVersion(text, hintPath));
			}
		}
		return addInReference;
	}

	internal static Version ParseVersion(string version, string hintPath)
	{
		if (version == null || version.Length == 0)
		{
			return new Version(0, 0, 0, 0);
		}
		if (version.StartsWith("@"))
		{
			if (version == "@SharpDevelopCoreVersion")
			{
				if (entryVersion == null)
				{
					entryVersion = new Version("10.0.12463");
				}
				return entryVersion;
			}
			if (hintPath != null)
			{
				string fileName = Path.Combine(hintPath, version.Substring(1));
				try
				{
					FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileName);
					return new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
				}
				catch (FileNotFoundException ex)
				{
					throw new AddInLoadException("Cannot get version '" + version + "': " + ex.Message);
				}
			}
			return new Version(0, 0, 0, 0);
		}
		return new Version(version);
	}

	public AddInReference(string name)
		: this(name, new Version(0, 0, 0, 0), new Version(int.MaxValue, int.MaxValue))
	{
	}

	public AddInReference(string name, Version specificVersion)
		: this(name, specificVersion, specificVersion)
	{
	}

	public AddInReference(string name, Version minimumVersion, Version maximumVersion)
	{
		Name = name;
		if (minimumVersion == null)
		{
			throw new ArgumentNullException("minimumVersion");
		}
		if (maximumVersion == null)
		{
			throw new ArgumentNullException("maximumVersion");
		}
		this.minimumVersion = minimumVersion;
		this.maximumVersion = maximumVersion;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AddInReference))
		{
			return false;
		}
		AddInReference addInReference = (AddInReference)obj;
		if (name == addInReference.name && minimumVersion == addInReference.minimumVersion)
		{
			return maximumVersion == addInReference.maximumVersion;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return name.GetHashCode() ^ minimumVersion.GetHashCode() ^ maximumVersion.GetHashCode();
	}

	public override string ToString()
	{
		if (minimumVersion.ToString() == "0.0.0.0")
		{
			if (maximumVersion.Major == int.MaxValue)
			{
				return name;
			}
			return name + ", version <" + maximumVersion.ToString();
		}
		if (maximumVersion.Major == int.MaxValue)
		{
			return name + ", version >" + minimumVersion.ToString();
		}
		if (minimumVersion == maximumVersion)
		{
			return name + ", version " + minimumVersion.ToString();
		}
		return name + ", version " + minimumVersion.ToString() + "-" + maximumVersion.ToString();
	}

	public AddInReference Clone()
	{
		return new AddInReference(name, minimumVersion, maximumVersion);
	}

	object ICloneable.Clone()
	{
		return Clone();
	}
}
