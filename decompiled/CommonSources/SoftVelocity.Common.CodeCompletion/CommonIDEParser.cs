using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion;

public abstract class CommonIDEParser : IParser
{
	internal static string[] lexerTags;

	public string[] LexerTags
	{
		get
		{
			return lexerTags;
		}
		set
		{
			lexerTags = value;
		}
	}

	public virtual LanguageProperties Language => null;

	protected virtual ClarionParser.ProcessCompileUnitBeforeConversion CUPreprocessor => null;

	protected virtual bool AddMemberKeyword => false;

	protected abstract bool IsWin { get; }

	protected abstract IExpressionFinder _CreateExpressionFinder(string fileName);

	protected abstract bool _CanParse(string fileName);

	protected abstract bool _CanParse(IProject project);

	protected abstract IResolver _CreateResolver();

	public IExpressionFinder CreateExpressionFinder(string fileName)
	{
		return _CreateExpressionFinder(fileName);
	}

	public bool CanParse(string fileName)
	{
		return _CanParse(fileName);
	}

	public bool CanParse(IProject project)
	{
		return _CanParse(project);
	}

	public virtual ICompilationUnit Parse(IProjectContent projectContent, string fileName)
	{
		string fileContent;
		using (StreamReader streamReader = new StreamReader(fileName))
		{
			fileContent = streamReader.ReadToEnd();
		}
		return Parse(projectContent, fileName, fileContent);
	}

	public static CompilerOptions CreateCompilerOptions(IProject project, bool isWin)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		CompilerOptions compilerOptions = new CompilerOptions();
		compilerOptions.c7mode = isWin;
		compilerOptions.debug = true;
		if (project is MSBuildBasedProject)
		{
			MSBuildBasedProject val = (MSBuildBasedProject)project;
			string evaluatedProperty = val.GetEvaluatedProperty("IncludeFolders");
			if (!string.IsNullOrEmpty(evaluatedProperty))
			{
				compilerOptions.includeFolders.AddRange(evaluatedProperty.Split(';'));
			}
			evaluatedProperty = val.GetEvaluatedProperty("DefineConstants");
			if (!string.IsNullOrEmpty(evaluatedProperty))
			{
				CompilerOptions.ParseExternalEquatesString(compilerOptions.equates, evaluatedProperty);
			}
		}
		if (isWin && !compilerOptions.equates.ContainsKey("_WIDTH32_"))
		{
			compilerOptions.equates.Add("_WIDTH32_", 1);
		}
		compilerOptions.noCode = true;
		compilerOptions.outFileName = "Dummy";
		compilerOptions.redFile = CommonClarionProject.CurrentRedirectionFile(project, isWin);
		compilerOptions.redType = typeof(RedirectionFile);
		if (lexerTags != null && lexerTags.Length > 0)
		{
			compilerOptions.taskKeywords = new ArrayList(lexerTags);
		}
		return compilerOptions;
	}

	public ICompilationUnit Parse(IProjectContent projectContent, string fileName, string fileContent)
	{
		object project = projectContent.Project;
		CompilerOptions compilerOptions = CreateCompilerOptions((IProject)((project is IProject) ? project : null), IsWin);
		compilerOptions.noCode = false;
		return Parse(projectContent, fileName, fileContent, compilerOptions);
	}

	public virtual ICompilationUnit Parse(IProjectContent projectContent, string fileName, string fileContent, CompilerOptions compOpt)
	{
		CommonClarionProject commonClarionProject = projectContent.Project as CommonClarionProject;
		Hashtable hashtable = null;
		if (commonClarionProject != null)
		{
			hashtable = commonClarionProject.ProgramEquates;
			if (hashtable != null && fileName.Equals(commonClarionProject.ProgramFileName, StringComparison.InvariantCultureIgnoreCase))
			{
				hashtable = null;
			}
			commonClarionProject.ModifyParserOptions(compOpt);
			fileContent = commonClarionProject.ModifyFileContent(fileName, fileContent);
		}
		ClaCompilationUnit claCompilationUnit;
		try
		{
			claCompilationUnit = ClarionParser.ParseFile(compOpt, projectContent, fileName, fileContent, hashtable, CUPreprocessor, AddMemberKeyword, out var _);
			if (commonClarionProject != null)
			{
				if (claCompilationUnit.IsProgram)
				{
					commonClarionProject.ProgramEquates = claCompilationUnit.ProgramEquates;
					commonClarionProject.ProgramFileName = fileName;
				}
				else if (commonClarionProject.ProgramEquates == null)
				{
					commonClarionProject.ProgramEquates = claCompilationUnit.ProgramEquates;
					commonClarionProject.ProgramFileName = fileName;
				}
				else if (fileName.Equals(commonClarionProject.ProgramFileName, StringComparison.InvariantCultureIgnoreCase))
				{
					commonClarionProject.ProgramEquates = claCompilationUnit.ProgramEquates;
				}
			}
		}
		catch
		{
			claCompilationUnit = new ClaCompilationUnit(projectContent, forWin: true);
		}
		return (ICompilationUnit)(object)claCompilationUnit;
	}

	public static ReportDeclaration ParseStructure(string fileName, string fileContent, int line, int column, bool extract, bool isWin, out ClarionType structType, out CompilerResults compRes)
	{
		IProject val = null;
		if (ProjectService.OpenSolution != null)
		{
			val = ProjectService.OpenSolution.FindProjectContainingFile(Path.GetFileName(fileName));
		}
		string fileName2 = fileName;
		if (val != null && !Path.IsPathRooted(fileName))
		{
			fileName2 = FileUtility.GetAbsolutePath(val.Directory, fileName);
		}
		CompilerOptions compilerOptions = CreateCompilerOptions(val, isWin);
		compilerOptions.noCode = false;
		ReportDeclaration reportDeclaration = ClarionParser.ParseReport(compilerOptions, fileName2, fileContent, line, column, extract, out compRes);
		if (reportDeclaration == null)
		{
			structType = ClarionType.REPORT;
			return null;
		}
		structType = reportDeclaration.Type.ClaType;
		return reportDeclaration;
	}

	public IResolver CreateResolver()
	{
		return _CreateResolver();
	}
}
