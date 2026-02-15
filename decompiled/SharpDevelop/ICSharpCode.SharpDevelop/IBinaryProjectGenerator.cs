using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop;

public interface IBinaryProjectGenerator
{
	bool ProjectCreated { get; }

	string ProjectCreatedName { get; }

	bool GenerateFiles(ProjectTemplate projectTemplate, ProjectCreateInformation projectCreateInformation);
}
