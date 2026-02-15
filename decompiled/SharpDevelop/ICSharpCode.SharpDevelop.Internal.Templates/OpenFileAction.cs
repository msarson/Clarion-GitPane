using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Internal.Templates;

public class OpenFileAction
{
	private string fileName;

	public OpenFileAction(string fileName)
	{
		this.fileName = fileName;
	}

	public void Run(ProjectCreateInformation projectCreateInformation)
	{
		string text = StringParser.Parse(fileName, new string[1, 2] { { "ProjectName", projectCreateInformation.ProjectName } });
		string text2 = FileUtility.Combine(projectCreateInformation.ProjectBasePath, text);
		FileService.OpenFile(text2);
	}
}
