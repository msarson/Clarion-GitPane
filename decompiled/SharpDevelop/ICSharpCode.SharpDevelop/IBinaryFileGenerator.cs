using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop;

public interface IBinaryFileGenerator
{
	bool GenerateFiles(FileTemplate projectTemplate);
}
