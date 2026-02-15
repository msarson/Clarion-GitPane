namespace ICSharpCode.SharpDevelop.Project;

public class LoadProject : IProjectLoader
{
	public void Load(string fileName)
	{
		ProjectService.LoadProject(fileName);
	}
}
