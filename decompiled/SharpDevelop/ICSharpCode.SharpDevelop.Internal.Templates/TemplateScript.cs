using System.Xml;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class TemplateScript
{
	private string languageName;

	private string runAt;

	private string scriptSourceCode;

	public string LanguageName => languageName;

	public string RunAt => runAt;

	private string SourceText => "public class ScriptObject : System.MarshalByRefObject { " + scriptSourceCode + "}";

	public TemplateScript(XmlElement scriptConfig)
	{
		languageName = scriptConfig.GetAttribute("language");
		runAt = scriptConfig.GetAttribute("runAt");
		scriptSourceCode = scriptConfig.InnerText;
	}
}
