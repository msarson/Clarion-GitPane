using System.CodeDom.Compiler;
using SoftVelocity.Common.Parser.Ast;

namespace SoftVelocity.Common.ClarionEditor;

public interface IStructureDesignerCompatible
{
	bool CanShowStructureDesigner { get; }

	bool IsWin { get; }

	ReportDeclaration ParseStructure(string fileName, string fileContent, int line, int column, out ClarionType structureType, out CompilerResults cr);

	string GetTemplatesFileName();

	string GetContentForDesigner();
}
