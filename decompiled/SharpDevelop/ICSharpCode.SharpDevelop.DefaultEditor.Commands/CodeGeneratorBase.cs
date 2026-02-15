using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.SharpDevelop.Refactoring;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public abstract class CodeGeneratorBase
{
	private ArrayList content = new ArrayList();

	protected CodeGenerator codeGen;

	protected ClassFinder classFinderContext;

	protected IClass currentClass;

	private IClass selectedClass;

	public abstract string CategoryName { get; }

	public virtual string Hint => "no hint";

	public abstract int ImageIndex { get; }

	public virtual bool IsActive => content.Count > 0;

	public ArrayList Content => content;

	public void Initialize(IClass currentClass)
	{
		selectedClass = currentClass;
		this.currentClass = currentClass.GetCompoundClass();
		codeGen = currentClass.ProjectContent.Language.CodeGenerator;
		classFinderContext = new ClassFinder(currentClass, currentClass.Region.BeginLine + 1, 0);
		InitContent();
	}

	protected virtual void InitContent()
	{
	}

	protected TypeReference ConvertType(IReturnType type)
	{
		return CodeGenerator.ConvertType(type, classFinderContext);
	}

	public virtual void GenerateCode(TextArea textArea, IList items)
	{
		List<AbstractNode> list = new List<AbstractNode>();
		GenerateCode(list, items);
		codeGen.InsertCodeInClass(selectedClass, new TextEditorDocument(textArea.Document), textArea.Caret.Line, list.ToArray());
		ParserService.ParseCurrentViewContent();
	}

	public abstract void GenerateCode(List<AbstractNode> nodes, IList items);
}
