using System;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Internal.ExternalTool;

public class ExternalTool
{
	private string menuCommand = "New Tool";

	private string command = string.Empty;

	private string arguments = string.Empty;

	private string initialDirectory = string.Empty;

	private bool promptForArguments;

	private bool useOutputPad;

	private bool addtopseparator;

	private string _shortcutKeys = string.Empty;

	public string MenuCommand
	{
		get
		{
			return menuCommand;
		}
		set
		{
			menuCommand = value;
		}
	}

	public string Command
	{
		get
		{
			return command;
		}
		set
		{
			command = value;
		}
	}

	public string Arguments
	{
		get
		{
			return arguments;
		}
		set
		{
			arguments = value;
		}
	}

	public string InitialDirectory
	{
		get
		{
			return initialDirectory;
		}
		set
		{
			initialDirectory = value;
		}
	}

	public bool PromptForArguments
	{
		get
		{
			return promptForArguments;
		}
		set
		{
			promptForArguments = value;
		}
	}

	public bool UseOutputPad
	{
		get
		{
			return useOutputPad;
		}
		set
		{
			useOutputPad = value;
		}
	}

	public bool AddTopSeparator
	{
		get
		{
			return addtopseparator;
		}
		set
		{
			addtopseparator = value;
		}
	}

	public string ShortcutKeys
	{
		get
		{
			return _shortcutKeys;
		}
		set
		{
			_shortcutKeys = value;
		}
	}

	public ExternalTool()
	{
	}

	public ExternalTool(XmlElement el)
	{
		if (el == null)
		{
			throw new ArgumentNullException("ExternalTool(XmlElement el) : el can't be null");
		}
		if (el["INITIALDIRECTORY"] == null || el["ARGUMENTS"] == null || el["COMMAND"] == null || el["MENUCOMMAND"] == null || el["PROMPTFORARGUMENTS"] == null)
		{
			throw new Exception("ExternalTool(XmlElement el) : INITIALDIRECTORY and ARGUMENTS and COMMAND and MENUCOMMAND and PROMPTFORARGUMENTS attributes must exist.(check the ExternalTool XML)");
		}
		InitialDirectory = el["INITIALDIRECTORY"].InnerText;
		Arguments = el["ARGUMENTS"].InnerText;
		Command = el["COMMAND"].InnerText;
		MenuCommand = el["MENUCOMMAND"].InnerText;
		PromptForArguments = bool.Parse(el["PROMPTFORARGUMENTS"].InnerText);
		if (el["USEOUTPUTPAD"] != null)
		{
			UseOutputPad = bool.Parse(el["USEOUTPUTPAD"].InnerText);
		}
		if (el["ADDSEPARATOR"] != null)
		{
			AddTopSeparator = bool.Parse(el["ADDSEPARATOR"].InnerText);
		}
		if (el["SHORTCUTKEY"] != null)
		{
			ShortcutKeys = el["SHORTCUTKEY"].InnerText;
		}
	}

	public override string ToString()
	{
		return menuCommand;
	}

	public XmlElement ToXmlElement(XmlDocument doc)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("ExternalTool.ToXmlElement(XmlDocument doc) : doc can't be null");
		}
		XmlElement xmlElement = doc.CreateElement("TOOL");
		XmlElement xmlElement2 = doc.CreateElement("INITIALDIRECTORY");
		xmlElement2.InnerText = InitialDirectory;
		xmlElement.AppendChild(xmlElement2);
		xmlElement2 = doc.CreateElement("ARGUMENTS");
		xmlElement2.InnerText = Arguments;
		xmlElement.AppendChild(xmlElement2);
		xmlElement2 = doc.CreateElement("COMMAND");
		xmlElement2.InnerText = command;
		xmlElement.AppendChild(xmlElement2);
		xmlElement2 = doc.CreateElement("MENUCOMMAND");
		xmlElement2.InnerText = MenuCommand;
		xmlElement.AppendChild(xmlElement2);
		xmlElement2 = doc.CreateElement("PROMPTFORARGUMENTS");
		xmlElement2.InnerText = PromptForArguments.ToString();
		xmlElement.AppendChild(xmlElement2);
		xmlElement2 = doc.CreateElement("USEOUTPUTPAD");
		xmlElement2.InnerText = UseOutputPad.ToString();
		xmlElement.AppendChild(xmlElement2);
		xmlElement2 = doc.CreateElement("ADDSEPARATOR");
		xmlElement2.InnerText = AddTopSeparator.ToString();
		xmlElement.AppendChild(xmlElement2);
		xmlElement2 = doc.CreateElement("SHORTCUTKEY");
		xmlElement2.InnerText = ShortcutKeys;
		xmlElement.AppendChild(xmlElement2);
		return xmlElement;
	}
}
