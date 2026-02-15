using System.Collections.Generic;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PrettyPrinter;
using ICSharpCode.NRefactory.Visitors;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public class VBConvertBuffer : AbstractMenuCommand
{
	public override void Run()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null || !(activeWorkbenchWindow.ViewContent is IEditable))
		{
			return;
		}
		IParser parser = ParserFactory.CreateParser(SupportedLanguage.CSharp, new StringReader(((IEditable)activeWorkbenchWindow.ViewContent).Text));
		parser.Parse();
		if (parser.Errors.Count > 0)
		{
			MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Commands.Convert.CorrectSourceCodeErrors}\n" + parser.Errors.ErrorOutput);
			return;
		}
		VBNetOutputVisitor vBNetOutputVisitor = new VBNetOutputVisitor();
		List<ISpecial> currentSpecials = parser.Lexer.SpecialTracker.CurrentSpecials;
		PreprocessingDirective.CSharpToVB(currentSpecials);
		new CSharpToVBNetConvertVisitor().VisitCompilationUnit(parser.CompilationUnit, null);
		using (SpecialNodesInserter.Install(currentSpecials, vBNetOutputVisitor))
		{
			vBNetOutputVisitor.VisitCompilationUnit(parser.CompilationUnit, null);
		}
		FileService.NewFile("Generated.VB", "VBNET", vBNetOutputVisitor.Text);
	}
}
