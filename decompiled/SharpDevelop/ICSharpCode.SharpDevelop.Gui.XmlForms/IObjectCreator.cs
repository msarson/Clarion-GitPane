using System;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public interface IObjectCreator
{
	object CreateObject(string name, XmlElement el);

	Type GetType(string name);
}
