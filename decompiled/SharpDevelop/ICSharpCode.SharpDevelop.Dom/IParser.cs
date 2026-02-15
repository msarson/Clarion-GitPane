using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Dom;

public interface IParser
{
	string[] LexerTags { get; set; }

	LanguageProperties Language { get; }

	IExpressionFinder CreateExpressionFinder(string fileName);

	bool CanParse(string fileName);

	bool CanParse(IProject project);

	ICompilationUnit Parse(IProjectContent projectContent, string fileName, string fileContent);

	IResolver CreateResolver();
}
