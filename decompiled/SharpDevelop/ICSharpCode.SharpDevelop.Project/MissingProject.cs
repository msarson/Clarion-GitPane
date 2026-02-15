namespace ICSharpCode.SharpDevelop.Project;

public class MissingProject : AbstractProject
{
	public MissingProject(string fileName, string title)
	{
		base.Name = title;
		base.FileName = fileName;
		TypeGuid = "{00000000-0000-0000-0000-000000000000}";
	}
}
