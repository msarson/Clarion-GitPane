using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop.Project;

public interface ILanguageBinding
{
	string Language { get; }

	IProject LoadProject(IMSBuildEngineProvider engineProvider, string fileName, string projectName);

	IProject CreateProject(ProjectCreateInformation info);
}
