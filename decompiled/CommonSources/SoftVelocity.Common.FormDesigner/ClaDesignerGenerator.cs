using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Text;
using Clarion;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Refactoring;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.FormDesigner;

public abstract class ClaDesignerGenerator : AbstractDesignerGenerator, IDesignerGenerator
{
	[Flags]
	public enum FormDesignerModeenum
	{
		Standart = 1,
		WindowDesigner = 2,
		ReportDesigner = 4,
		CompactFramework = 8
	}

	protected IClass m_cd_class;

	private IClass m_completeClass;

	protected IMethod m_cd_initializeComponents;

	private TextEditorControl m_textEditorControl;

	private FormDesignerModeenum m_formDesignerMode;

	private string m_FileName = string.Empty;

	private FormsDesignerViewContent m_FormsDesignerViewContent;

	private IClass m_initial_cd_class;

	private ClaDesignerLoader m_loader;

	public IClass CompleteClass
	{
		get
		{
			return m_completeClass;
		}
		set
		{
			m_completeClass = value;
		}
	}

	public FormDesignerModeenum FormDesignerMode => m_formDesignerMode;

	public string FileName => m_FileName;

	public FormsDesignerViewContent FormsDesignerViewContent
	{
		get
		{
			return m_FormsDesignerViewContent;
		}
		set
		{
			m_FormsDesignerViewContent = value;
		}
	}

	public IClass CodeDOMClass
	{
		get
		{
			return m_cd_class;
		}
		set
		{
			m_cd_class = value;
		}
	}

	public IClass InitialCodeDOMClass
	{
		get
		{
			return m_initial_cd_class;
		}
		set
		{
			m_initial_cd_class = value;
		}
	}

	public IMethod CodeInitComponents
	{
		get
		{
			return m_cd_initializeComponents;
		}
		set
		{
			m_cd_initializeComponents = value;
		}
	}

	public TextEditorControl TextEditorControl
	{
		get
		{
			return m_textEditorControl;
		}
		set
		{
			m_textEditorControl = value;
		}
	}

	public TextArea TextArea => ((TextEditorControlBase)TextEditorControl).ActiveTextAreaControl.TextArea;

	public ClaDesignerLoader DesignerLoader
	{
		get
		{
			return m_loader;
		}
		set
		{
			m_loader = value;
		}
	}

	protected abstract CommonIDEParser GetParser();

	public ClaDesignerGenerator(bool isWindow)
	{
		m_formDesignerMode = ((!isWindow) ? FormDesignerModeenum.Standart : FormDesignerModeenum.WindowDesigner);
	}

	public ClaDesignerGenerator(FormDesignerModeenum mode, string curFileName)
	{
		m_FileName = ((curFileName == null) ? string.Empty : curFileName);
		m_formDesignerMode = mode;
	}

	public bool ReleaseAll()
	{
		m_FormsDesignerViewContent = null;
		DesignerLoader = null;
		return true;
	}

	public override ICollection GetCompatibleMethods(EventDescriptor edesc)
	{
		if (FormDesignerMode == FormDesignerModeenum.WindowDesigner || FormDesignerMode == FormDesignerModeenum.ReportDesigner)
		{
			return null;
		}
		ArrayList arrayList = new ArrayList();
		MethodInfo method = edesc.EventType.GetMethod("Invoke");
		foreach (IMethod method2 in m_completeClass.Methods)
		{
			if (((IMethodOrProperty)method2).Parameters.Count == method.GetParameters().Length)
			{
				bool flag = true;
				for (int i = 0; i < method.GetParameters().Length; i++)
				{
					_ = method.GetParameters()[i];
					_ = ((IMethodOrProperty)method2).Parameters[i];
				}
				if (flag)
				{
					arrayList.Add(((IMember)method2).Name);
				}
			}
		}
		return arrayList;
	}

	public override ICollection GetCompatibleMethods(EventInfo edesc)
	{
		return new ArrayList();
	}

	public void MergeFormChanges(ControlContainer rcd, IDesignerHost host)
	{
	}

	public override void MergeFormChanges(CodeCompileUnit unit)
	{
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		InitDocument(null);
		CodeTypeDeclaration codeTypeDeclaration = null;
		CodeMemberMethod codeMemberMethod = null;
		foreach (CodeNamespace @namespace in unit.Namespaces)
		{
			foreach (CodeTypeDeclaration type in @namespace.Types)
			{
				foreach (CodeTypeMember member in type.Members)
				{
					if (member is CodeMemberMethod && member.Name == "InitializeComponent")
					{
						codeTypeDeclaration = type;
						codeMemberMethod = (CodeMemberMethod)member;
						break;
					}
				}
			}
		}
		if (codeTypeDeclaration == null || codeMemberMethod == null)
		{
			throw new InvalidOperationException("InitializeComponent method not found in framework-generated CodeDom.");
		}
		if (codeTypeDeclaration.Name != CodeDOMClass.Name)
		{
			FindReferencesAndRenameHelper.RenameClass(CodeDOMClass, codeTypeDeclaration.Name);
		}
		StringWriter stringWriter = new StringWriter();
		ClaCodeDOMGenerator claCodeDOMGenerator = new ClaCodeDOMGenerator(base.CodeDomProvider, "\t");
		claCodeDOMGenerator.ConvertContentDefinition(codeMemberMethod, stringWriter);
		string text = stringWriter.ToString().TrimEnd();
		if (CodeInitComponents is ClaMethod && ((ClaMethod)(object)CodeInitComponents).IsInline)
		{
			text = " INLINE\r\n" + text + "\r\n END";
		}
		DomRegion region = CodeDOMClass.Region;
		int beginLine = ((DomRegion)(ref region)).BeginLine;
		DomRegion replaceRegion = GetReplaceRegion(base.Document, CodeInitComponents);
		if (((DomRegion)(ref replaceRegion)).BeginColumn <= 0 || ((DomRegion)(ref replaceRegion)).EndColumn <= 0)
		{
			throw new InvalidOperationException("Column must be > 0");
		}
		int length = GetInitializeComponentsString(base.Document, CodeInitComponents).Length;
		int totalNumberOfLines = base.Document.TotalNumberOfLines;
		ReplaceFormFields(codeTypeDeclaration, base.Document, claCodeDOMGenerator);
		int totalNumberOfLines2 = base.Document.TotalNumberOfLines;
		IDocument obj = base.Document;
		DomRegion bodyRegion = ((IMember)CodeInitComponents).BodyRegion;
		LineSegment lineSegment = obj.GetLineSegment(((DomRegion)(ref bodyRegion)).BeginLine - totalNumberOfLines + totalNumberOfLines2);
		int offset = lineSegment.Offset;
		base.Document.Replace(offset, length, text);
		_ = base.Document.TotalNumberOfLines;
		DomRegion bodyRegion2 = CodeDOMClass.BodyRegion;
		int beginLine2 = ((DomRegion)(ref bodyRegion2)).BeginLine;
		lineSegment = base.Document.GetLineSegment(beginLine2 + 1);
		int offset2 = lineSegment.Offset;
		ReplaceExternalFormFields(codeTypeDeclaration);
		foreach (CodeTypeMember member2 in codeTypeDeclaration.Members)
		{
			if (member2 is CodeMemberField)
			{
				CodeMemberField field = (CodeMemberField)member2;
				base.Document.Insert(offset2, claCodeDOMGenerator.GenerateFieldDeclaration(field) + Environment.NewLine);
			}
		}
		int totalNumberOfLines3 = base.Document.TotalNumberOfLines;
		IndentDocument(base.Document, beginLine, totalNumberOfLines3 - 1);
		SaveDocument();
		ParserService.EnqueueForParsing(base.DesignerFile, base.Document.TextContent);
	}

	private bool IndentDocument(IDocument doc, int start, int end)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)doc.TextEditorProperties.IndentStyle == 2 && doc.FormattingStrategy is ClaCommonFormattingStrategy)
		{
			bool flag = true;
			if (((ClaCommonFormattingStrategy)(object)doc.FormattingStrategy).Disposed)
			{
				flag = false;
			}
			if (flag)
			{
				((ClaCommonFormattingStrategy)(object)doc.FormattingStrategy).IndentLines(doc, start, end);
			}
		}
		return true;
	}

	private bool IsClassLine(LineSegment curLine)
	{
		string text = base.Document.GetText(curLine.Offset, curLine.Length).Trim();
		if (FormDesignerMode == FormDesignerModeenum.CompactFramework)
		{
			string text2 = text.Replace(" ", "");
			text2 = text2.Replace("    ", "");
			if (text2.EndsWith(",TYPE,PUBLIC"))
			{
				return true;
			}
			if (text2.EndsWith(",NETCLASS,PUBLIC"))
			{
				return true;
			}
		}
		else if (text.EndsWith("TYPE") || text.EndsWith("NETCLASS"))
		{
			return true;
		}
		return false;
	}

	public IMethod GetInitializeComponents(IClass c)
	{
		foreach (IMethod method in c.Methods)
		{
			if ((((IMember)method).Name.ToUpperInvariant() == "INITIALIZECOMPONENTS" || ((IMember)method).Name.ToUpperInvariant() == "INITIALIZECOMPONENT") && ((IMethodOrProperty)method).Parameters.Count == 0)
			{
				return method;
			}
		}
		return null;
	}

	public bool HasInitializeComponents(IClass c)
	{
		return GetInitializeComponents(c) != null;
	}

	public bool HasInitializeComponents(ICompilationUnit cu)
	{
		foreach (IClass @class in cu.Classes)
		{
			IMethod val = GetInitializeComponents(@class);
			if (val != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCompilationUnitDesignable(ICompilationUnit cu)
	{
		foreach (IClass @class in cu.Classes)
		{
			IMethod val = GetInitializeComponents(@class);
			if (val != null)
			{
				return FormsDesignerSecondaryDisplayBinding.BaseClassIsFormOrControl(@class);
			}
		}
		return false;
	}

	public bool IsClassDesignable(IClass cl)
	{
		IMethod val = GetInitializeComponents(cl);
		if (val != null)
		{
			return FormsDesignerSecondaryDisplayBinding.BaseClassIsFormOrControl(cl);
		}
		return false;
	}

	private string GetInitializeComponentsString(IDocument doc, IMethod initializeComponents)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		DomRegion bodyRegion = ((IMember)initializeComponents).BodyRegion;
		LineSegment lineSegment = doc.GetLineSegment(((DomRegion)(ref bodyRegion)).BeginLine);
		DomRegion bodyRegion2 = ((IMember)initializeComponents).BodyRegion;
		LineSegment lineSegment2 = doc.GetLineSegment(((DomRegion)(ref bodyRegion2)).EndLine - 1);
		int offset = lineSegment.Offset;
		int offset2 = lineSegment2.Offset;
		DomRegion bodyRegion3 = ((IMember)initializeComponents).BodyRegion;
		int num = offset2 + ((DomRegion)(ref bodyRegion3)).EndColumn - 1;
		return doc.GetText(offset, num - offset);
	}

	private bool ReplaceExternalFormFields(CodeTypeDeclaration formClass)
	{
		ArrayList arrayList = new ArrayList();
		foreach (CodeTypeMember member in formClass.Members)
		{
			if (!(member is CodeMemberField))
			{
				continue;
			}
			CodeMemberField codeMemberField = (CodeMemberField)member;
			foreach (IField field in CompleteClass.Fields)
			{
				if (field is ClaField claField && claField.Name.ToUpperInvariant() == codeMemberField.Name.ToUpperInvariant())
				{
					arrayList.Add(member);
				}
			}
		}
		foreach (CodeMemberField item in arrayList)
		{
			formClass.Members.Remove(item);
		}
		return true;
	}

	private bool ReplaceFormFields(CodeTypeDeclaration formClass, IDocument doc, ClaCodeDOMGenerator domGenerator)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		ArrayList arrayList = new ArrayList();
		ArrayList usedFields = GetUsedFields(doc, CodeDOMClass, CodeInitComponents, isGetAll: false);
		for (int num = usedFields.Count - 1; num >= 0; num--)
		{
			IField val = (IField)usedFields[num];
			DomRegion region = ((IMember)val).Region;
			LineSegment lineSegment = doc.GetLineSegment(((DomRegion)(ref region)).BeginLine - 1);
			bool flag = false;
			foreach (CodeTypeMember member in formClass.Members)
			{
				if (member is CodeMemberField)
				{
					CodeMemberField codeMemberField = (CodeMemberField)member;
					if (((IMember)val).Name == codeMemberField.Name)
					{
						flag = true;
						int offset = lineSegment.Offset;
						DomRegion region2 = ((IMember)val).Region;
						doc.Remove(offset, ((DomRegion)(ref region2)).EndColumn - 1);
						doc.Insert(lineSegment.Offset, domGenerator.GenerateFieldDeclaration(codeMemberField));
						arrayList.Add(member);
						break;
					}
				}
			}
			if (!flag)
			{
				doc.Remove(lineSegment.Offset, lineSegment.TotalLength);
			}
		}
		foreach (CodeMemberField item in arrayList)
		{
			formClass.Members.Remove(item);
		}
		return true;
	}

	private bool DeleteFormFields(IDocument doc)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		ArrayList usedFields = GetUsedFields(doc, CodeDOMClass, CodeInitComponents, isGetAll: false);
		for (int num = usedFields.Count - 1; num >= 0; num--)
		{
			IField val = (IField)usedFields[num];
			DomRegion region = ((IMember)val).Region;
			LineSegment lineSegment = doc.GetLineSegment(((DomRegion)(ref region)).BeginLine - 1);
			doc.Remove(lineSegment.Offset, lineSegment.TotalLength);
		}
		return true;
	}

	public ArrayList GetUsedFields(IDocument doc, IClass c, IMethod initializeComponents, bool isGetAll)
	{
		ArrayList arrayList = new ArrayList();
		if (isGetAll)
		{
			IClass val = (c.IsPartial ? c.GetCompoundClass() : c);
			foreach (IField field in val.Fields)
			{
				if (((IMember)field).ReturnType == null)
				{
					throw new FormsDesignerLoadException(string.Format(ClaDesignerLoader.DesignableTypeNotFound, ((IMember)field).FullyQualifiedName));
				}
				if (((IMember)field).ReturnType.IsDefaultReturnType)
				{
					arrayList.Add(field);
				}
			}
		}
		else
		{
			string initializeComponentsString = GetInitializeComponentsString(doc, initializeComponents);
			foreach (IField field2 in c.Fields)
			{
				string text = "SELF." + ((IMember)field2).Name;
				int num = initializeComponentsString.IndexOf(text, StringComparison.InvariantCultureIgnoreCase);
				int length = text.Length;
				int length2 = initializeComponentsString.Length;
				while (num >= 0)
				{
					int num2 = num + length;
					if (num2 < length2 && (initializeComponentsString[num2] == ' ' || initializeComponentsString[num2] == '\t' || initializeComponentsString[num2] == ':' || initializeComponentsString[num2] == '='))
					{
						arrayList.Add(field2);
						break;
					}
					num = initializeComponentsString.IndexOf(text, num2, StringComparison.InvariantCultureIgnoreCase);
				}
			}
		}
		return arrayList;
	}

	private void SaveDocument()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		NamedFileOperationDelegate val = null;
		if (base.SaveDocumentToFile == null)
		{
			return;
		}
		if (val == null)
		{
			val = (NamedFileOperationDelegate)delegate(string fileName)
			{
				using StreamWriter streamWriter = new StreamWriter(fileName, append: false, Encoding.UTF8);
				streamWriter.Write(base.Document.TextContent);
			};
		}
		NamedFileOperationDelegate val2 = val;
		FileUtility.ObservedSave(val2, base.SaveDocumentToFile, (FileErrorPolicy)0);
	}

	protected bool InitDocument(string forcedFileName)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		base.SaveDocumentToFile = null;
		string textContent = base.ViewContent.Document.TextContent;
		if (!string.IsNullOrEmpty(forcedFileName))
		{
			base.DesignerFile = forcedFileName;
		}
		else
		{
			base.DesignerFile = ((IDecoration)CodeInitComponents).DeclaringType.CompilationUnit.FileName;
		}
		if (FileUtility.IsEqualFileName(((TextEditorControlBase)base.ViewContent.TextEditorControl).FileName, base.DesignerFile))
		{
			string text = textContent;
			base.Document = base.ViewContent.Document;
		}
		else
		{
			IWorkbenchWindow openFile = FileService.GetOpenFile(base.DesignerFile);
			if (openFile == null)
			{
				base.Document = new DocumentFactory().CreateDocument();
				string text = ParserService.GetParseableFileContent(base.DesignerFile);
				base.Document.TextContent = text;
				base.SaveDocumentToFile = base.DesignerFile;
			}
			else
			{
				IViewContent obj = openFile.ViewContent;
				ITextEditorControlProvider val = (ITextEditorControlProvider)(object)((obj is ITextEditorControlProvider) ? obj : null);
				if (val == null)
				{
					throw new ApplicationException("designer file viewcontent must implement ITextEditorControlProvider");
				}
				base.Document = ((TextEditorControlBase)val.TextEditorControl).Document;
				string text = base.Document.TextContent;
			}
		}
		return true;
	}

	public override bool InsertComponentEvent(IComponent component, EventDescriptor edesc, string eventMethodName, string body, out string file, out int position)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		if (FormDesignerMode == FormDesignerModeenum.WindowDesigner || FormDesignerMode == FormDesignerModeenum.ReportDesigner)
		{
			file = string.Empty;
			position = -1;
			return false;
		}
		IClass val = InitialCodeDOMClass;
		if (CodeDOMClass.IsPartial)
		{
			if (!HasInitializeComponents(CodeDOMClass))
			{
				file = CodeDOMClass.CompilationUnit.FileName;
			}
			else if (val != null)
			{
				file = val.CompilationUnit.FileName;
			}
			else
			{
				file = CodeDOMClass.CompilationUnit.FileName;
				val = CodeDOMClass;
			}
		}
		else
		{
			file = CodeDOMClass.CompilationUnit.FileName;
		}
		foreach (IMethod method in val.Methods)
		{
			if (((IMember)method).Name == eventMethodName)
			{
				DomRegion bodyRegion = ((IMember)method).BodyRegion;
				position = ((DomRegion)(ref bodyRegion)).BeginLine + 1;
				return true;
			}
		}
		base.ViewContent.MergeFormChanges();
		Reparse();
		InitDocument(file);
		Reparse(base.Document.TextContent);
		if (!val.IsPartial)
		{
			DomRegion region = CodeDOMClass.Region;
			position = ((DomRegion)(ref region)).EndLine + 1;
		}
		else
		{
			DomRegion region2 = val.Region;
			position = ((DomRegion)(ref region2)).EndLine + 1;
		}
		int num = 0;
		if (!val.IsPartial)
		{
			IDocument obj = base.Document;
			DomRegion bodyRegion2 = CodeDOMClass.BodyRegion;
			num = obj.GetLineSegment(((DomRegion)(ref bodyRegion2)).EndLine - 1).Offset;
		}
		else
		{
			IDocument obj2 = base.Document;
			DomRegion bodyRegion3 = val.BodyRegion;
			num = obj2.GetLineSegment(((DomRegion)(ref bodyRegion3)).EndLine - 1).Offset;
		}
		string text = GenerateParams(edesc);
		_ = base.Document.TextContent;
		string text2 = "";
		text2 = eventMethodName + " PROCEDURE(" + text + "),PUBLIC\r\n";
		base.Document.Insert(num, text2);
		text2 = "\r\n" + CodeDOMClass.Name + "." + eventMethodName + " PROCEDURE(" + text + ")\r\n            CODE\r\n";
		int lineNumberForOffset = base.Document.GetLineNumberForOffset(base.Document.TextContent.Length);
		LineSegment lineSegment = base.Document.GetLineSegment(lineNumberForOffset);
		int num2 = lineSegment.Offset + lineSegment.Length;
		base.Document.Insert(num2, text2);
		position = lineNumberForOffset + 3;
		Reparse(base.Document.TextContent);
		IDocument doc = base.Document;
		DomRegion region3 = val.Region;
		IndentDocument(doc, ((DomRegion)(ref region3)).BeginLine, position);
		return true;
	}

	protected override CodeDomProvider CreateCodeProvider()
	{
		return null;
	}

	public bool Reparse(ICompilationUnit cu)
	{
		if (cu == null)
		{
			return false;
		}
		foreach (IClass @class in cu.Classes)
		{
			CodeInitComponents = GetInitializeComponents(@class);
			if (CodeInitComponents != null)
			{
				CodeDOMClass = @class;
				m_completeClass = @class.DefaultReturnType.GetUnderlyingClass();
				return true;
			}
		}
		return false;
	}

	public bool Reparse(IClass formClass)
	{
		if (formClass == null)
		{
			return false;
		}
		CodeDOMClass = formClass;
		m_completeClass = CodeDOMClass.DefaultReturnType.GetUnderlyingClass();
		CodeInitComponents = GetInitializeComponents(CodeDOMClass);
		if (CodeInitComponents != null)
		{
			return true;
		}
		return false;
	}

	protected void Reparse(string content)
	{
		string fileName = CodeDOMClass.CompilationUnit.FileName;
		ParseInformation val = ParserService.ParseFile(fileName, content, false);
		ICompilationUnit bestCompilationUnit = val.BestCompilationUnit;
		foreach (IClass @class in bestCompilationUnit.Classes)
		{
			CodeInitComponents = GetInitializeComponents(@class);
			if (CodeInitComponents != null)
			{
				CodeDOMClass = @class;
				m_completeClass = @class.DefaultReturnType.GetUnderlyingClass();
				break;
			}
		}
	}

	protected override DomRegion GetReplaceRegion(IDocument document, IMethod method)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		DomRegion bodyRegion = ((IMember)method).BodyRegion;
		return new DomRegion(((DomRegion)(ref bodyRegion)).BeginLine + 1, 1, ((DomRegion)(ref bodyRegion)).EndLine + 1, 1);
	}

	protected override string CreateEventHandler(EventDescriptor edesc, string eventMethodName, string body, string indentation)
	{
		string text = GenerateParams(edesc);
		string text2 = "";
		text2 = eventMethodName + " PROCEDURE(" + text + "),PUBLIC\r\n";
		return CodeDOMClass.Name + "." + eventMethodName + " PROCEDURE(" + text + ")\r\n            CODE\r\n\r\n";
	}

	protected override int GetEventHandlerInsertionLine(IClass c)
	{
		return base.Document.TotalNumberOfLines + 1;
	}

	protected static string GenerateParams(EventDescriptor edesc)
	{
		Type eventType = edesc.EventType;
		MethodInfo method = eventType.GetMethod("Invoke");
		string text = "";
		for (int i = 0; i < method.GetParameters().Length; i++)
		{
			ParameterInfo parameterInfo = method.GetParameters()[i];
			string text2 = parameterInfo.ParameterType.ToString();
			text += text2;
			text = text + " " + parameterInfo.Name;
			if (i + 1 < method.GetParameters().Length)
			{
				text += ", ";
			}
		}
		return text;
	}

	public new void Reparse()
	{
		string textContent = base.ViewContent.Document.TextContent;
		ParseInformation val = ParserService.ParseFile(((TextEditorControlBase)base.ViewContent.TextEditorControl).FileName, textContent, false);
		ICompilationUnit bestCompilationUnit = val.BestCompilationUnit;
		foreach (IClass @class in bestCompilationUnit.Classes)
		{
			if (!FormsDesignerSecondaryDisplayBinding.BaseClassIsFormOrControl(@class))
			{
				continue;
			}
			IClass compoundClass = @class.GetCompoundClass();
			CodeInitComponents = GetInitializeComponents(compoundClass);
			if (CodeInitComponents == null)
			{
				continue;
			}
			string fileName = ((IDecoration)CodeInitComponents).DeclaringType.CompilationUnit.FileName;
			if (FileUtility.IsEqualFileName(((TextEditorControlBase)base.ViewContent.TextEditorControl).FileName, fileName))
			{
				string text = textContent;
			}
			else
			{
				IWorkbenchWindow openFile = FileService.GetOpenFile(fileName);
				string text;
				if (openFile == null)
				{
					text = ParserService.GetParseableFileContent(fileName);
				}
				else
				{
					IViewContent obj = openFile.ViewContent;
					ITextEditorControlProvider val2 = (ITextEditorControlProvider)(object)((obj is ITextEditorControlProvider) ? obj : null);
					if (val2 == null)
					{
						throw new ApplicationException("designer file viewcontent must implement ITextEditorControlProvider");
					}
					text = ((TextEditorControlBase)val2.TextEditorControl).Document.TextContent;
				}
				ParserService.ParseFile(fileName, text, false);
				CodeInitComponents = GetInitializeComponents(compoundClass);
			}
			InitialCodeDOMClass = @class;
			CompleteClass = @class.GetCompoundClass();
			CodeDOMClass = ((IDecoration)CodeInitComponents).DeclaringType;
			break;
		}
	}

	void IDesignerGenerator.Attach(FormsDesignerViewContent viewContent)
	{
		if ((FormDesignerMode == FormDesignerModeenum.Standart || FormDesignerMode == FormDesignerModeenum.CompactFramework) && FormsDesignerViewContent.DesignSurface.LoadErrors.Count != 0)
		{
			IEnumerator enumerator = FormsDesignerViewContent.DesignSurface.LoadErrors.GetEnumerator();
			if (enumerator.MoveNext() && enumerator.Current is FormsDesignerInitializeCompNotFoundException)
			{
				DesignerLoader.CompilationErrors = string.Empty;
				throw (FormsDesignerInitializeCompNotFoundException)enumerator.Current;
			}
			string text = string.Empty;
			foreach (object loadError in FormsDesignerViewContent.DesignSurface.LoadErrors)
			{
				text = text + loadError.ToString() + "\r\n";
			}
			if (text != string.Empty)
			{
				text += "\r\n";
			}
			if (CError.GetErrorCount() != 0)
			{
				string compilationErrors = DesignerLoader.CompilationErrors;
				DesignerLoader.CompilationErrors = string.Empty;
				throw new FormsDesignerLoadException(text + compilationErrors);
			}
			throw new FormsDesignerLoadException(text);
		}
		Attach(viewContent);
	}
}
