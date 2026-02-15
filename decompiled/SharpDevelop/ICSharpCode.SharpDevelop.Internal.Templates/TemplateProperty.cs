using System.Xml;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class TemplateProperty
{
	private string name;

	private string localizedName;

	private string type;

	private string category;

	private string description;

	private string defaultValue;

	public string Name => name;

	public string LocalizedName => localizedName;

	public string Type => type;

	public string Category => category;

	public string Description => description;

	public string DefaultValue => defaultValue;

	public TemplateProperty(XmlElement propertyElement)
	{
		name = propertyElement.GetAttribute("name");
		localizedName = propertyElement.GetAttribute("localizedName");
		type = propertyElement.GetAttribute("type");
		category = propertyElement.GetAttribute("category");
		description = propertyElement.GetAttribute("description");
		defaultValue = propertyElement.GetAttribute("defaultValue");
	}
}
