using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Reflection;
using System.Text;
using Clarion;
using ICSharpCode.Core;
using ICSharpCode.FormsDesigner;
using ICSharpCode.FormsDesigner.Services;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.ClarionNet;
using SoftVelocity.ClarionNet.CommonProperties;
using SoftVelocity.ClarionNet.ReportDesigner;
using SoftVelocity.ClarionNet.WindowDesigner;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Parser.Ast;
using SoftVelocity.Common.Parser.IDE;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.FormDesigner;

public class ClaDesignerLoader : CodeDomDesignerLoader
{
	private static string m_designableClassNotFound = "No class derived from Form or UserControl was found.";

	internal static string DesignableTypeNotFound = "The type of '{0}' could not be found (are you missing a USING directive or an assembly reference?)";

	private static string m_missingReferenceMessage = "Your project is missing a reference to '${Name}' - please add it using 'Project > Add Reference'.";

	private bool loading = true;

	private IDesignerLoaderHost designerLoaderHost;

	protected ClaDesignerGenerator generator;

	private ITypeResolutionService typeResolutionService;

	private TextEditorControl textEditorControl;

	protected string compilationErrors;

	private ITypeDescriptorFilterService m_old_ITypeDescriptorFilterService;

	private ITypeDescriptorFilterService m_new_ITypeDescriptorFilterService;

	private IMenuCommandService m_old_IMenuCommandService;

	private WindowMenuCommandService m_new_IMenuCommandService;

	private DesignerOptionService m_old_DesignerOptionService;

	private DesignerOptionService m_new_DesignerOptionService;

	private ClaDesignerGenerator.FormDesignerModeenum m_formDesignerMode;

	private ControlContainer m_rcd;

	private ArrayList m_arr;

	private bool m_isWindowWindow;

	private string lastTextContent;

	public string CompilationErrors
	{
		get
		{
			return compilationErrors;
		}
		set
		{
			compilationErrors = value;
		}
	}

	private IDocument Document => ((TextEditorControlBase)textEditorControl).Document;

	public string TextContent => ((TextEditorControlBase)textEditorControl).Document.TextContent;

	public override bool Loading => loading;

	public IDesignerLoaderHost DesignerLoaderHost => designerLoaderHost;

	protected override CodeDomProvider CodeDomProvider => null;

	protected override ITypeResolutionService TypeResolutionService => typeResolutionService;

	public ClaDesignerGenerator.FormDesignerModeenum FormDesignerMode => m_formDesignerMode;

	public ControlContainer ControlContainerDecl => m_rcd;

	protected override void OnBeginUnload()
	{
		if (m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner || m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
		{
			ClaToolBoxProvider.RemoveNewSelectedToolUsedHandler(m_formDesignerMode);
			DesignerLoaderHost.RemoveService(typeof(IMenuCommandService));
			DesignerLoaderHost.AddService(typeof(IMenuCommandService), m_old_IMenuCommandService);
			m_old_IMenuCommandService = null;
			m_new_IMenuCommandService.Dispose();
			m_new_IMenuCommandService = null;
			DesignerLoaderHost.RemoveService(typeof(ITypeDescriptorFilterService));
			DesignerLoaderHost.AddService(typeof(ITypeDescriptorFilterService), m_old_ITypeDescriptorFilterService);
			m_old_ITypeDescriptorFilterService = null;
			m_new_ITypeDescriptorFilterService = null;
			DesignerLoaderHost.RemoveService(typeof(DesignerOptionService));
			DesignerLoaderHost.AddService(typeof(DesignerOptionService), m_old_DesignerOptionService);
			m_old_DesignerOptionService = null;
			m_new_DesignerOptionService = null;
		}
		base.OnBeginUnload();
	}

	public override void Dispose()
	{
		m_rcd = null;
		m_arr = null;
		generator.CodeDOMClass = null;
		generator.CodeInitComponents = null;
		generator.CompleteClass = null;
		generator.InitialCodeDOMClass = null;
		base.Dispose();
	}

	protected override void Initialize()
	{
		base.Initialize();
		if ((m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner || m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner) && DesignerLoaderHost != null)
		{
			DesignerLoaderHost.RemoveService(typeof(IExtenderProviderService));
		}
	}

	protected override bool IsReloadNeeded()
	{
		if (!base.IsReloadNeeded())
		{
			return TextContent != lastTextContent;
		}
		return true;
	}

	public ClaDesignerLoader(TextEditorControl textEditorControl, IDesignerGenerator generator, ClaDesignerGenerator.FormDesignerModeenum mode, ControlContainer rcd, ArrayList arr, bool isWindowWindow)
	{
		m_formDesignerMode = mode;
		if (m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner || m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
		{
			m_arr = arr;
			m_rcd = rcd;
			m_isWindowWindow = isWindowWindow;
		}
		this.textEditorControl = textEditorControl;
		this.generator = (ClaDesignerGenerator)generator;
		this.generator.DesignerLoader = this;
		this.generator.TextEditorControl = textEditorControl;
		if (generator is ClaDesignerGenerator)
		{
			((ClaDesignerGenerator)generator).InitialCodeDOMClass = null;
		}
	}

	public bool ReleaseClaASTree()
	{
		_ = m_rcd;
		m_rcd = null;
		return true;
	}

	public override void BeginLoad(IDesignerLoaderHost host)
	{
		loading = true;
		typeResolutionService = (ITypeResolutionService)host.GetService(typeof(ITypeResolutionService));
		if (m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner || m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
		{
			ClaToolBoxProvider.RemoveSelectedToolUsedHandler(m_formDesignerMode);
			m_old_ITypeDescriptorFilterService = (ITypeDescriptorFilterService)host.GetService(typeof(ITypeDescriptorFilterService));
			host.RemoveService(typeof(ITypeDescriptorFilterService));
			m_new_ITypeDescriptorFilterService = new WindowTypeDescriptorFilterService();
			host.AddService(typeof(ITypeDescriptorFilterService), m_new_ITypeDescriptorFilterService);
			m_old_IMenuCommandService = (IMenuCommandService)host.GetService(typeof(IMenuCommandService));
			host.RemoveService(typeof(IMenuCommandService));
			m_new_IMenuCommandService = new WindowMenuCommandService(((AbstractBaseViewContent)generator.FormsDesignerViewContent).Control, generator.FormsDesignerViewContent.DesignSurface);
			host.AddService(typeof(IMenuCommandService), m_new_IMenuCommandService);
			m_old_DesignerOptionService = (DesignerOptionService)host.GetService(typeof(DesignerOptionService));
			host.RemoveService(typeof(DesignerOptionService));
			if (m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner)
			{
				m_new_DesignerOptionService = new WindowDesignerOptionService();
			}
			else
			{
				m_new_DesignerOptionService = new ReportDesignerOptionService();
			}
			host.AddService(typeof(DesignerOptionService), m_new_DesignerOptionService);
			host.RemoveService(typeof(IEventBindingService));
		}
		else if (m_formDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.Standart)
		{
			ClaToolBoxProvider.RemoveSelectedToolUsedHandler(m_formDesignerMode);
		}
		designerLoaderHost = host;
		base.BeginLoad(host);
		if ((FormDesignerMode != ClaDesignerGenerator.FormDesignerModeenum.Standart && FormDesignerMode != ClaDesignerGenerator.FormDesignerModeenum.CompactFramework) || generator.FormsDesignerViewContent.DesignSurface.LoadErrors.Count == 0)
		{
			return;
		}
		IEnumerator enumerator = generator.FormsDesignerViewContent.DesignSurface.LoadErrors.GetEnumerator();
		if (enumerator.MoveNext() && enumerator.Current is FormsDesignerInitializeCompNotFoundException)
		{
			throw (FormsDesignerInitializeCompNotFoundException)enumerator.Current;
		}
		string text = string.Empty;
		foreach (object loadError in generator.FormsDesignerViewContent.DesignSurface.LoadErrors)
		{
			text = text + loadError.ToString() + "\r\n";
		}
		if (text != string.Empty)
		{
			text += "\r\n";
		}
		if (CError.GetErrorCount() != 0)
		{
			MakeReducedErrorText(CError.GetCompilerResults(), ((TextEditorControlBase)textEditorControl).FileName);
			if (compilationErrors == string.Empty)
			{
				MakeErrorText(CError.GetCompilerResults());
			}
			if (compilationErrors == string.Empty && text == string.Empty)
			{
				compilationErrors = "An unknown error found. Designer cannot be loaded.";
			}
			throw new FormsDesignerLoadException(text + compilationErrors);
		}
		throw new FormsDesignerLoadException(text);
	}

	protected override void OnEndLoad(bool successful, ICollection errors)
	{
		loading = false;
		base.OnEndLoad(successful, errors);
	}

	public static IList<IClass> FindFormClassParts(ICompilationUnit cuBase, out IClass formClass, out bool isFirstClassInFile)
	{
		formClass = null;
		isFirstClassInFile = true;
		foreach (IClass @class in cuBase.Classes)
		{
			if (FormsDesignerSecondaryDisplayBinding.BaseClassIsFormOrControl(@class))
			{
				formClass = @class;
				break;
			}
			if (!(@class is ClaClass) || cuBase.FileName.Equals(((ClaClass)(object)@class).ClaRegion.FileName, StringComparison.InvariantCultureIgnoreCase))
			{
				isFirstClassInFile = false;
			}
		}
		if (formClass == null)
		{
			throw new FormsDesignerLoadException(m_designableClassNotFound);
		}
		formClass = formClass.GetCompoundClass();
		if (formClass is CompoundClass)
		{
			IClass obj = formClass;
			return ((CompoundClass)((obj is CompoundClass) ? obj : null)).GetParts();
		}
		return (IList<IClass>)(object)new IClass[1] { formClass };
	}

	protected override CodeCompileUnit Parse()
	{
		if (FormDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner || FormDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
		{
			return GenerateCodeDOM(Document, ControlContainerDecl);
		}
		lastTextContent = TextContent;
		ParseInformation parseInformation = ParserService.GetParseInformation(((TextEditorControlBase)textEditorControl).FileName);
		IClass formClass;
		bool isFirstClassInFile;
		IList<IClass> list = FindFormClassParts(parseInformation.BestCompilationUnit, out formClass, out isFirstClassInFile);
		if (formClass.ProjectContent.GetClass("System.Drawing.Point", 0) == null)
		{
			throw new FormsDesignerLoadException(StringParser.Parse(m_missingReferenceMessage, new string[1, 2] { { "Name", "System.Drawing" } }));
		}
		if (formClass.ProjectContent.GetClass("System.Windows.Forms.Form", 0) == null)
		{
			throw new FormsDesignerLoadException(StringParser.Parse(m_missingReferenceMessage, new string[1, 2] { { "Name", "System.Windows.Forms" } }));
		}
		List<KeyValuePair<string, CompilationUnit>> list2 = new List<KeyValuePair<string, CompilationUnit>>();
		bool foundInitMethod = false;
		foreach (IClass item in list)
		{
			string fileName = item.CompilationUnit.FileName;
			if (fileName == null)
			{
				continue;
			}
			bool flag = false;
			foreach (KeyValuePair<string, CompilationUnit> item2 in list2)
			{
				if (FileUtility.IsEqualFileName(fileName, item2.Key))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			string parseableFileContent = ParserService.GetParseableFileContent(fileName);
			CompilerResults res;
			CompilationUnit compilationUnit = ParseFile(fileName, parseableFileContent, out res);
			if (res.Errors.HasErrors)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (CompilerError error in res.Errors)
				{
					stringBuilder.AppendLine(error.ToString());
				}
				throw new FormsDesignerLoadException("Syntax errors in " + fileName + ":\r\n" + stringBuilder.ToString());
			}
			FixTypeNames(compilationUnit, item.CompilationUnit, ref foundInitMethod);
			if (foundInitMethod && generator.CodeDOMClass == null)
			{
				generator.CodeDOMClass = item;
				generator.CodeInitComponents = generator.GetInitializeComponents(item);
				generator.CompleteClass = item.DefaultReturnType.GetUnderlyingClass();
			}
			if (((TextEditorControlBase)textEditorControl).FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
			{
				generator.InitialCodeDOMClass = item;
			}
			list2.Add(new KeyValuePair<string, CompilationUnit>(fileName, compilationUnit));
		}
		if (!foundInitMethod)
		{
			throw new FormsDesignerLoadException("The InitializeComponent method was not found. Designer cannot be loaded.");
		}
		CompilationUnit compilationUnit2 = new CompilationUnit();
		NamespaceDeclaration namespaceDeclaration = new NamespaceDeclaration(formClass.Namespace);
		namespaceDeclaration.Children.Clear();
		compilationUnit2.AddChild(namespaceDeclaration);
		TypeDeclaration typeDeclaration = new TypeDeclaration();
		typeDeclaration.Modifier = Modifiers.Public;
		namespaceDeclaration.AddChild(typeDeclaration);
		typeDeclaration.Name = formClass.Name;
		foreach (KeyValuePair<string, CompilationUnit> item3 in list2)
		{
			foreach (INode child in item3.Value.Children)
			{
				if (child is TypeDeclaration typeDeclaration2 && typeDeclaration2.Name == typeDeclaration.Name)
				{
					FillFormInfo(typeDeclaration, typeDeclaration2);
				}
				if (!(child is NamespaceDeclaration))
				{
					continue;
				}
				foreach (INode child2 in ((NamespaceDeclaration)child).Children)
				{
					if (child2 is TypeDeclaration typeDeclaration3 && typeDeclaration3.Name == typeDeclaration.Name)
					{
						FillFormInfo(typeDeclaration, typeDeclaration3);
					}
				}
			}
		}
		bool userControl = false;
		IClass compoundClass = formClass.GetCompoundClass();
		IClass val = formClass.ProjectContent.GetClass("System.Windows.Forms.UserControl");
		if (val != null && compoundClass.IsTypeInInheritanceTree(val))
		{
			userControl = true;
		}
		ChangeBaseClass(typeDeclaration, userControl);
		AddAditionalDesignerAssemblies();
		ClaCodeDomVisitor claCodeDomVisitor = new ClaCodeDomVisitor();
		claCodeDomVisitor.EnvironmentInformationProvider = new ClaRefactoryInformationProvider(formClass.ProjectContent, formClass);
		claCodeDomVisitor.VisitCompilationUnit(compilationUnit2, null);
		if (!isFirstClassInFile)
		{
			MessageService.ShowWarning("The form must be the first class in the file in order for form resources be compiled correctly.\nPlease move other classes below the form class definition or move them to other files.");
		}
		return claCodeDomVisitor.codeCompileUnit;
	}

	protected virtual void ChangeBaseClass(TypeDeclaration decl, bool userControl)
	{
	}

	protected virtual bool AddAditionalDesignerAssemblies()
	{
		return true;
	}

	private static void FillFormInfo(TypeDeclaration formDecl, TypeDeclaration fromType)
	{
		foreach (INode child in fromType.Children)
		{
			if (child is Method)
			{
				FixVariableInitializers((Method)child);
			}
			formDecl.AddChild(child);
		}
		if (fromType.Parent != null)
		{
			formDecl.Parent = fromType.Parent;
		}
		if (fromType.InterfacesExists)
		{
			if (!formDecl.InterfacesExists)
			{
				formDecl.Interfaces = new List<TypeReference>();
			}
			formDecl.Interfaces.AddRange(fromType.Interfaces);
		}
	}

	private static void FixVariableInitializers(Method method)
	{
		if (method.Definition.Children.Count == 0 || method.Definition.Body == null || method.Definition.Body.Children.Count == 0)
		{
			return;
		}
		foreach (INode child in method.Definition.Children)
		{
			if (!(child is VariableDeclaration))
			{
				continue;
			}
			for (int i = 0; i < method.Definition.Body.Children.Count; i++)
			{
				INode node = method.Definition.Body.Children[i];
				if (node is AssignmentExpression && ((AssignmentExpression)node).Left is IdentifierExpression && ((IdentifierExpression)((AssignmentExpression)node).Left).Name.Equals(((VariableDeclaration)child).Name, StringComparison.InvariantCultureIgnoreCase))
				{
					Expression right = ((AssignmentExpression)node).Right;
					((VariableDeclaration)child).Initializer = right;
					right.InitParent = child;
					method.Definition.Body.Children.RemoveAt(i);
					break;
				}
			}
		}
	}

	private static CompilationUnit ParseFile(string fileName, string fileContent, out CompilerResults res)
	{
		IProject val = null;
		if (ProjectService.OpenSolution != null)
		{
			val = ProjectService.OpenSolution.FindProjectContainingFile(fileName);
		}
		bool isWin;
		if (val is CommonClarionProject)
		{
			isWin = ((CommonClarionProject)(object)val).IsWin;
		}
		else
		{
			string extension = Path.GetExtension(fileName);
			isWin = (extension.Equals(".clw", StringComparison.InvariantCultureIgnoreCase) ? true : false);
		}
		CompilerOptions compilerOptions = CommonIDEParser.CreateCompilerOptions(val, isWin);
		compilerOptions.noCode = false;
		return ClarionParser.ParseFile(compilerOptions, fileName, fileContent, addMEMBERKeyword: false, out res);
	}

	private static void FixTypeNames(object o, ICompilationUnit domCu, ref bool foundInitMethod)
	{
		if (domCu == null)
		{
			return;
		}
		if (o is CompilationUnit compilationUnit)
		{
			{
				foreach (INode child in compilationUnit.Children)
				{
					FixTypeNames(child, domCu, ref foundInitMethod);
				}
				return;
			}
		}
		if (o is NamespaceDeclaration namespaceDeclaration)
		{
			{
				foreach (INode child2 in namespaceDeclaration.Children)
				{
					FixTypeNames(child2, domCu, ref foundInitMethod);
				}
				return;
			}
		}
		if (o is TypeDeclaration typeDeclaration)
		{
			if (!typeDeclaration.Reg.File.Equals(domCu.FileName, StringComparison.InvariantCultureIgnoreCase))
			{
				return;
			}
			if (typeDeclaration.Parent != null)
			{
				FixTypeReference(typeDeclaration.Parent, typeDeclaration.Reg, domCu);
			}
			if (typeDeclaration.InterfacesExists)
			{
				foreach (TypeReference @interface in typeDeclaration.Interfaces)
				{
					FixTypeReference(@interface, typeDeclaration.Reg, domCu);
				}
			}
			for (int i = 0; i < typeDeclaration.Children.Count; i++)
			{
				object obj = typeDeclaration.Children[i];
				if (obj.GetType() == typeof(Method))
				{
					Method method = (Method)obj;
					if ((method.Definition.Name == "InitializeComponents" || method.Definition.Name == "InitializeComponent") && method.Definition.Parameters.Count == 0)
					{
						method.Definition.Name = "InitializeComponent";
						method.Declaration.Name = "InitializeComponent";
						if (foundInitMethod)
						{
							throw new FormsDesignerLoadException("There are multiple InitializeComponent methods in the class. Designer cannot be loaded.");
						}
						foundInitMethod = true;
					}
					else
					{
						typeDeclaration.Children.RemoveAt(i--);
					}
				}
				else if (obj is TypeDeclaration || obj is VariableDeclaration)
				{
					FixTypeNames(obj, domCu, ref foundInitMethod);
				}
				else
				{
					typeDeclaration.Children.RemoveAt(i--);
				}
			}
		}
		else if (o is VariableDeclaration variableDeclaration)
		{
			FixTypeReference(variableDeclaration.Type, variableDeclaration.Reg, domCu);
		}
	}

	private static void FixTypeReference(TypeReference type, CASTRegion region, ICompilationUnit domCu)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (type == null || type.SystemType != type.Name)
		{
			return;
		}
		foreach (TypeReference genericType in type.GenericTypes)
		{
			FixTypeReference(genericType, region, domCu);
		}
		IClass nearestType = GetNearestType(region, domCu);
		SearchTypeResult val = domCu.ProjectContent.SearchType(new SearchTypeRequest(type.Name, type.GenericTypes.Count, nearestType, domCu, region.Line, region.Column));
		IReturnType result = ((SearchTypeResult)(ref val)).Result;
		if (result != null)
		{
			type.OriginalName = result.FullyQualifiedName;
		}
	}

	private static IClass GetNearestType(CASTRegion region, ICompilationUnit domCu)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		IClass val = null;
		if (domCu is ClaCompilationUnit)
		{
			object obj = ((ClaCompilationUnit)(object)domCu).FindNearestObject(region.Line, region.Column);
			if (obj is IClass)
			{
				val = (IClass)obj;
			}
			else if (obj is IMember)
			{
				val = ((IDecoration)(IMember)obj).DeclaringType;
			}
			if (val == null)
			{
				val = domCu.GetInnermostClass(region.Line, region.Column);
			}
			if (val == null)
			{
				val = (IClass)(object)((ClaCompilationUnit)(object)domCu).GlobalClass;
			}
		}
		else
		{
			val = domCu.GetInnermostClass(region.Line, region.Column);
		}
		return val;
	}

	private bool MakeErrorText(CompilerResults cr)
	{
		compilationErrors = string.Empty;
		foreach (CompilerError error in cr.Errors)
		{
			string text = error.FileName + "(" + error.Line + "," + error.Column + "): " + error.ErrorText + "\r\n";
			compilationErrors += text;
		}
		return true;
	}

	private bool MakeReducedErrorText(CompilerResults cr, string FileName)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		compilationErrors = string.Empty;
		if (generator.CodeDOMClass == null)
		{
			return false;
		}
		IMethod initializeComponents = generator.GetInitializeComponents(generator.CodeDOMClass);
		DomRegion region = generator.CodeDOMClass.Region;
		if (!((DomRegion)(ref region)).IsEmpty && initializeComponents != null)
		{
			DomRegion region2 = ((IMember)initializeComponents).Region;
			if (!((DomRegion)(ref region2)).IsEmpty)
			{
				DomRegion bodyRegion = ((IMember)initializeComponents).BodyRegion;
				if (!((DomRegion)(ref bodyRegion)).IsEmpty)
				{
					DomRegion region3 = ((IMember)initializeComponents).Region;
					int beginLine = ((DomRegion)(ref region3)).BeginLine;
					DomRegion region4 = ((IMember)initializeComponents).Region;
					int beginColumn = ((DomRegion)(ref region4)).BeginColumn;
					DomRegion bodyRegion2 = ((IMember)initializeComponents).BodyRegion;
					int endLine = ((DomRegion)(ref bodyRegion2)).EndLine;
					DomRegion bodyRegion3 = ((IMember)initializeComponents).BodyRegion;
					DomRegion val = default(DomRegion);
					((DomRegion)(ref val))._002Ector(beginLine, beginColumn, endLine, ((DomRegion)(ref bodyRegion3)).EndColumn);
					foreach (CompilerError error in cr.Errors)
					{
						if (!(error.FileName != string.Empty) || !(Path.GetFullPath(error.FileName).ToUpper() != Path.GetFullPath(FileName).ToUpper()))
						{
							DomRegion region5 = generator.CodeDOMClass.Region;
							if (((DomRegion)(ref region5)).IsInside(error.Line, error.Column) || ((DomRegion)(ref val)).IsInside(error.Line, error.Column))
							{
								string text = error.FileName + "(" + error.Line + "," + error.Column + "): " + error.ErrorText + "\r\n";
								compilationErrors += text;
							}
						}
					}
					return true;
				}
			}
		}
		return false;
	}

	private CodeCompileUnit GenerateCodeDOM(IDocument document, ControlContainer rcd)
	{
		CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
		Assembly assembly = typeof(GeneralDesiner).Assembly;
		if (FormDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
		{
			assembly = typeof(ClaReportManager).Assembly;
		}
		if (!ICSharpCode.FormsDesigner.Services.TypeResolutionService.DesignerAssemblies.Contains(assembly))
		{
			ICSharpCode.FormsDesigner.Services.TypeResolutionService.DesignerAssemblies.Add(assembly);
		}
		string directoryName = Path.GetDirectoryName(typeof(ClaDesignerLoader).Assembly.Location);
		directoryName += "\\..\\Common\\Controls\\CommonControl.dll";
		assembly = Assembly.LoadFrom(directoryName);
		if (!ICSharpCode.FormsDesigner.Services.TypeResolutionService.DesignerAssemblies.Contains(assembly))
		{
			ICSharpCode.FormsDesigner.Services.TypeResolutionService.DesignerAssemblies.Add(assembly);
		}
		if (m_arr != null)
		{
			foreach (object item in m_arr)
			{
				if (item is Assembly && !ICSharpCode.FormsDesigner.Services.TypeResolutionService.DesignerAssemblies.Contains((Assembly)item))
				{
					ICSharpCode.FormsDesigner.Services.TypeResolutionService.DesignerAssemblies.Add((Assembly)item);
				}
			}
		}
		CodeNamespace codeNamespace = new CodeNamespace("GeneratedForm");
		codeCompileUnit.Namespaces.Add(codeNamespace);
		string name = (m_isWindowWindow ? "window1" : "application1");
		if (FormDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
		{
			name = "report1";
		}
		CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(name);
		codeNamespace.Types.Add(codeTypeDeclaration);
		if (FormDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner)
		{
			codeTypeDeclaration.BaseTypes.Add(m_isWindowWindow ? "SoftVelocity.ClarionNet.WindowDesigner.Window" : "SoftVelocity.ClarionNet.WindowDesigner.Application");
		}
		else if (FormDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
		{
			codeTypeDeclaration.BaseTypes.Add("SoftVelocity.ClarionNet.Designer.SectionControls.BaseDesignerControl");
		}
		CodeConstructor codeConstructor = new CodeConstructor();
		codeConstructor.Attributes = (MemberAttributes)24578;
		codeConstructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitializeComponent")));
		CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
		codeMemberMethod.Name = "InitializeComponent";
		codeMemberMethod.ReturnType = new CodeTypeReference("System.Void");
		if (FormDesignerMode == ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner)
		{
			CodeAssignStatement value = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Label"), new CodePrimitiveExpression(rcd.GivenName));
			codeMemberMethod.Statements.Add(value);
			if (rcd.Arguments != null && rcd.Arguments.Count > 0)
			{
				BaseValueSetter baseValueSetter = new BaseValueSetter(typeof(string));
				object val = null;
				baseValueSetter.EvaluateArg(rcd.Arguments[0], ref val, extra: false, exclamation: true);
				value = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Title"), new CodePrimitiveExpression(val));
			}
			codeMemberMethod.Statements.Add(value);
			if (rcd.Attributes != null && rcd.Attributes.Count > 0)
			{
				foreach (SoftVelocity.Common.Parser.Ast.Attribute attribute in rcd.Attributes)
				{
					if (attribute != null && attribute.Name.ToUpperInvariant() == "FONT")
					{
						CodeAssignStatement codeAssignStatement = ClaWindowManager.GetCodeAssignStatement(attribute.Name, attribute.Arguments);
						if (codeAssignStatement != null)
						{
							codeMemberMethod.Statements.Add(codeAssignStatement);
						}
					}
				}
				foreach (SoftVelocity.Common.Parser.Ast.Attribute attribute2 in rcd.Attributes)
				{
					if (attribute2 != null && attribute2.Name.ToUpperInvariant() != "FONT")
					{
						CodeAssignStatement codeAssignStatement2 = ClaWindowManager.GetCodeAssignStatement(attribute2.Name, attribute2.Arguments);
						if (codeAssignStatement2 != null)
						{
							codeMemberMethod.Statements.Add(codeAssignStatement2);
						}
					}
				}
			}
		}
		codeTypeDeclaration.Members.Add(codeConstructor);
		codeTypeDeclaration.Members.Add(codeMemberMethod);
		return codeCompileUnit;
	}

	protected virtual bool AddBaseClass(CodeTypeDeclaration ctd)
	{
		ctd.BaseTypes.Add(generator.CodeDOMClass.BaseTypes[0].FullyQualifiedName);
		return true;
	}

	private bool AddMissingReferencesToProject()
	{
		return true;
	}

	protected override void Write(CodeCompileUnit unit)
	{
		if (FormDesignerMode != ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner && FormDesignerMode != ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner)
		{
			generator.MergeFormChanges(unit);
			generator.CodeDOMClass = null;
			generator.Reparse();
		}
	}

	protected override void ReportFlushErrors(ICollection errors)
	{
	}
}
