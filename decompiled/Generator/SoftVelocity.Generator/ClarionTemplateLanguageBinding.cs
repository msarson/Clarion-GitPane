using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator;

internal class ClarionTemplateLanguageBinding : ILanguageBinding
{
	public string Language => "ClarionTemplates";

	public IProject CreateProject(ProjectCreateInformation info)
	{
		return null;
	}

	public IProject LoadProject(IMSBuildEngineProvider engineProvider, string fileName, string projectName)
	{
		return null;
	}
}
