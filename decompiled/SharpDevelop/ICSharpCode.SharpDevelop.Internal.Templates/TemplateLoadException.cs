using System;
using System.Runtime.Serialization;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

[Serializable]
public class TemplateLoadException : Exception
{
	public TemplateLoadException()
	{
	}

	public TemplateLoadException(string message)
		: base(message)
	{
	}

	public TemplateLoadException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected TemplateLoadException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal static void AssertAttributeExists(XmlElement element, string attributeName)
	{
		if (string.IsNullOrEmpty(element.GetAttribute(attributeName)))
		{
			throw new TemplateLoadException("Error in template on node '" + element.Name + "':\nThe attribute '" + attributeName + "' is required.");
		}
	}
}
