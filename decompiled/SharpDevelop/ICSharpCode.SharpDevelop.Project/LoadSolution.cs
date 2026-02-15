namespace ICSharpCode.SharpDevelop.Project;

public class LoadSolution : IProjectLoader
{
	public void Load(string fileName)
	{
		ProjectService.LoadSolution(fileName);
	}
}
