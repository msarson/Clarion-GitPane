using System;
using System.Collections.Generic;
using System.Xml;

namespace ICSharpCode.Core;

public class AddInManifest
{
	private List<AddInReference> dependencies = new List<AddInReference>();

	private List<AddInReference> conflicts = new List<AddInReference>();

	private Dictionary<string, Version> identities = new Dictionary<string, Version>();

	private Version primaryVersion;

	private string primaryIdentity;

	public string PrimaryIdentity => primaryIdentity;

	public Version PrimaryVersion => primaryVersion;

	public Dictionary<string, Version> Identities => identities;

	public List<AddInReference> Dependencies => dependencies;

	public List<AddInReference> Conflicts => conflicts;

	private void AddIdentity(string name, string version, string hintPath)
	{
		if (name.Length == 0)
		{
			throw new AddInLoadException("Identity needs a name");
		}
		foreach (char c in name)
		{
			if (!char.IsLetterOrDigit(c) && c != '.' && c != '_')
			{
				throw new AddInLoadException("Identity name contains invalid character: '" + c + "'");
			}
		}
		Version value = AddInReference.ParseVersion(version, hintPath);
		if (primaryVersion == null)
		{
			primaryVersion = value;
		}
		if (primaryIdentity == null)
		{
			primaryIdentity = name;
		}
		identities.Add(name, value);
	}

	public void ReadManifestSection(XmlReader reader, string hintPath)
	{
		if (reader.AttributeCount != 0)
		{
			throw new AddInLoadException("Manifest node cannot have attributes.");
		}
		if (reader.IsEmptyElement)
		{
			throw new AddInLoadException("Manifest node cannot be empty.");
		}
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.EndElement:
				if (reader.LocalName == "Manifest")
				{
					return;
				}
				break;
			case XmlNodeType.Element:
			{
				string localName = reader.LocalName;
				Properties properties = Properties.ReadFromAttributes(reader);
				switch (localName)
				{
				case "Identity":
					AddIdentity(properties["name"], properties["version"], hintPath);
					break;
				case "Dependency":
					dependencies.Add(AddInReference.Create(properties, hintPath));
					break;
				case "Conflict":
					conflicts.Add(AddInReference.Create(properties, hintPath));
					break;
				default:
					throw new AddInLoadException("Unknown node in Manifest section:" + localName);
				}
				break;
			}
			}
		}
	}
}
