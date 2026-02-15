namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class StringWrapper
{
	private string text;

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public override string ToString()
	{
		return text;
	}
}
