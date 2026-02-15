using System;
using System.IO;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator;

public class ApplicationLanguageBinding : ILanguageBinding
{
	public string Language => "ClarionGenerator";

	public IProject LoadProject(IMSBuildEngineProvider engineProvider, string fileName, string projectName)
	{
		if (Path.GetExtension(fileName).ToLowerInvariant() == ApplicationService.ApplicationInportFileExtension)
		{
			fileName = Path.ChangeExtension(fileName, ".app");
		}
		return ApplicationService.LoadProject(engineProvider, fileName, projectName);
	}

	public IProject CreateProject(ProjectCreateInformation info)
	{
		throw new Exception("The method or operation is not implemented.");
	}
}
