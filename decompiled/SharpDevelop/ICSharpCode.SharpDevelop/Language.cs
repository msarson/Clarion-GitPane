namespace ICSharpCode.SharpDevelop;

public class Language
{
	private string name;

	private string code;

	private int imageIndex;

	public string Name => name;

	public string Code => code;

	public int ImageIndex => imageIndex;

	public Language(string name, string code, int imageIndex)
	{
		this.name = name;
		this.code = code;
		this.imageIndex = imageIndex;
	}
}
