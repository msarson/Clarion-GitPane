using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;

namespace ICSharpCode.SharpDevelop.Project.Converter;

public abstract class NRefactoryLanguageConverter : LanguageConverter
{
	protected abstract void ConvertAst(CompilationUnit compilationUnit, List<ISpecial> specials);

	protected void ConvertFile(FileProjectItem sourceItem, FileProjectItem targetItem, string sourceExtension, string targetExtension, SupportedLanguage sourceLanguage, IOutputAstVisitor outputVisitor)
	{
		FixExtensionOfExtraProperties(targetItem, sourceExtension, targetExtension);
		if (sourceExtension.Equals(Path.GetExtension(sourceItem.FileName), StringComparison.OrdinalIgnoreCase))
		{
			string parseableFileContent = ParserService.GetParseableFileContent(sourceItem.FileName);
			IParser parser = ParserFactory.CreateParser(sourceLanguage, new StringReader(parseableFileContent));
			parser.Parse();
			if (parser.Errors.Count > 0)
			{
				conversionLog.AppendLine();
				conversionLog.AppendLine(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.Convert.IsNotConverted}", new string[1, 2] { { "FileName", sourceItem.FileName } }));
				conversionLog.AppendLine(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.Convert.ParserErrorCount}", new string[1, 2] { 
				{
					"ErrorCount",
					parser.Errors.Count.ToString()
				} }));
				conversionLog.AppendLine(parser.Errors.ErrorOutput);
				base.ConvertFile(sourceItem, targetItem);
				return;
			}
			List<ISpecial> currentSpecials = parser.Lexer.SpecialTracker.CurrentSpecials;
			ConvertAst(parser.CompilationUnit, currentSpecials);
			using (SpecialNodesInserter.Install(currentSpecials, outputVisitor))
			{
				outputVisitor.VisitCompilationUnit(parser.CompilationUnit, null);
			}
			parser.Dispose();
			if (outputVisitor.Errors.Count > 0)
			{
				conversionLog.AppendLine();
				conversionLog.AppendLine(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Commands.Convert.ConverterErrorCount}", new string[2, 2]
				{
					{ "FileName", sourceItem.FileName },
					{
						"ErrorCount",
						outputVisitor.Errors.Count.ToString()
					}
				}));
				conversionLog.AppendLine(outputVisitor.Errors.ErrorOutput);
			}
			targetItem.Include = Path.ChangeExtension(targetItem.Include, targetExtension);
			File.WriteAllText(targetItem.FileName, outputVisitor.Text);
		}
		else
		{
			base.ConvertFile(sourceItem, targetItem);
		}
	}
}
