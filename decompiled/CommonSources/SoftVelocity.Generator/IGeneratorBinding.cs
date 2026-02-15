using Clarion.GEN;
using Clarion.PRJ;
using SoftVelocity.Generator.PWEE;

namespace SoftVelocity.Generator;

public interface IGeneratorBinding
{
	IGeneratorDialog OpenWindowFormatter(string name, IFormatter generator);

	IGeneratorDialog OpenReportFormatter(string name, IFormatter generator);

	IGeneratorEditorDialog OpenWindowReportEditor(string name, IEmbedEditorDetails generator);

	IGeneratorEditorDialog OpenEmbedEditor(string name, IEmbedEditorDetails generator);

	IGeneratorEditorDialog OpenFileEditor(string name, bool readOnly, uint initialLine, IEditorDetails generator);

	PRJFile GetProjectFile(string appName);

	string GetProjectFileName(string appName);

	IGeneratorEditorDialog OpenPwee(IPweeDetails generator);

	IGeneratorDialog OpenFormDesigner(string name, AppgenSymbols appsymbols);
}
