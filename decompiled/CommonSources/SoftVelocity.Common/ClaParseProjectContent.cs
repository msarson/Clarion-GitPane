using System.Collections;
using System.Collections.Generic;
using System.IO;
using Clarion.Core.Redirection;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.CodeCompletion;
using SoftVelocity.Common.Parser.IDE;
using SoftVelocity.Common.Parser.IDE.Ast;
using SoftVelocity.Common.Parser.SyntaxAnalyzer;
using SoftVelocity.Generator;

namespace SoftVelocity.Common;

public class ClaParseProjectContent : ParseProjectContent, IClarionProjectContent
{
	private bool isParseNewItems = true;

	public bool IsInitializing => base.initializing;

	public bool IsParseNewItems
	{
		get
		{
			return isParseNewItems;
		}
		set
		{
			isParseNewItems = value;
		}
	}

	internal static ParseProjectContent CreateUninitalized(IProject project)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		ClaParseProjectContent claParseProjectContent = new ClaParseProjectContent();
		((ParseProjectContent)claParseProjectContent).project = project;
		((DefaultProjectContent)claParseProjectContent).Language = project.LanguageProperties;
		((ParseProjectContent)claParseProjectContent).initializing = true;
		IProjectContent mscorlib = ParserService.GetRegistryForReference(new ReferenceProjectItem(project, "mscorlib")).Mscorlib;
		((DefaultProjectContent)claParseProjectContent).AddReferencedContent(mscorlib);
		return (ParseProjectContent)(object)claParseProjectContent;
	}

	protected override void OnProjectItemAdded(object sender, ProjectItemEventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (!(e.ProjectItem.ItemType == ItemType.Compile) || IsParseNewItems)
		{
			((ParseProjectContent)this).OnProjectItemAdded(sender, e);
		}
	}

	protected override void Initialize2()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!base.initializing)
		{
			return;
		}
		ParseableFileContentEnumerator val = new ParseableFileContentEnumerator(base.project);
		try
		{
			CommonClarionProject commonClarionProject = base.project as CommonClarionProject;
			if (commonClarionProject != null && !commonClarionProject.ProjectParsingEnabled)
			{
				return;
			}
			IProjectContent[] array;
			lock (((DefaultProjectContent)this).ReferencedContents)
			{
				array = (IProjectContent[])(object)new IProjectContent[((DefaultProjectContent)this).ReferencedContents.Count];
				((DefaultProjectContent)this).ReferencedContents.CopyTo(array, 0);
			}
			IProjectContent[] array2 = array;
			foreach (IProjectContent val2 in array2)
			{
				if (val2 is ReflectionProjectContent)
				{
					((ReflectionProjectContent)val2).InitializeReferences();
				}
			}
			if (!base.initializing)
			{
				return;
			}
			CommonIDEParser.lexerTags = PropertyService.Get<string[]>("SharpDevelop.TaskListTokens", ParserService.DefaultTaskListTokens);
			ArrayList arrayList = new ArrayList();
			ArrayList arrayList2 = new ArrayList();
			while (val.MoveNext())
			{
				if (commonClarionProject == null || commonClarionProject.IsValidFileExtension(Path.GetExtension(val.CurrentFileName)))
				{
					arrayList.Add(new object[3] { val.Index, val.CurrentFileName, val.CurrentFileContent });
					arrayList2.Add(val.CurrentFileName);
				}
			}
			if (arrayList.Count == 0)
			{
				return;
			}
			bool flag = commonClarionProject?.IsWin ?? true;
			CompilerOptions compilerOptions = new CompilerOptions();
			compilerOptions.c7mode = flag;
			compilerOptions.debug = true;
			compilerOptions.outFileName = "Dummy";
			compilerOptions.redFile = CommonClarionProject.CurrentRedirectionFile(base.project, flag);
			compilerOptions.redType = typeof(RedirectionFile);
			CAnalyzer.FindProgram(arrayList2, null, ClarionParser.CreatePU(compilerOptions), null, null, null, null, null, 0);
			int j;
			for (j = 0; j < arrayList.Count && !((string)((object[])arrayList[j])[1] == (string)arrayList2[0]); j++)
			{
			}
			if (j != 0)
			{
				object[] value = (object[])arrayList[j];
				arrayList[j] = arrayList[0];
				arrayList[0] = value;
			}
			string text = (flag ? "test.clw" : "test.cln");
			IParser parser = ParserService.GetParser(text);
			List<ICompilationUnit> list = new List<ICompilationUnit>();
			if (commonClarionProject != null && commonClarionProject.LightweightParsingModeEnabled && arrayList.Count > 1)
			{
				arrayList.RemoveRange(1, arrayList.Count - 1);
			}
			for (j = 0; j < arrayList.Count; j++)
			{
				string text2 = (string)((object[])arrayList[j])[1];
				string text3 = (string)((object[])arrayList[j])[2];
				ICompilationUnit val3 = parser.Parse((IProjectContent)(object)this, text2, text3);
				if (val3 != null)
				{
					list.Add(val3);
				}
				if (!base.initializing)
				{
					return;
				}
			}
			foreach (ICompilationUnit item in list)
			{
				ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(item.FileName);
				if (parseInformationIfExist != null)
				{
					((DefaultProjectContent)this).UpdateCompilationUnit(parseInformationIfExist.MostRecentCompilationUnit, item, item.FileName);
				}
				else
				{
					((DefaultProjectContent)this).UpdateCompilationUnit((ICompilationUnit)null, item, item.FileName);
				}
				if (!base.initializing)
				{
					break;
				}
				TaskService.UpdateCommentTags(item.FileName, item.TagComments);
				if (item.FileName != null)
				{
					ParserService.UpdateParseInformation(item, item.FileName, true);
				}
				if (!base.initializing)
				{
					break;
				}
			}
		}
		finally
		{
			base.initializing = false;
			val.Dispose();
		}
	}

	public Dictionary<string, List<IClass>> GetClarionClassesWithPre(bool lookInReferences)
	{
		Dictionary<string, IClass> dictionary = new Dictionary<string, IClass>();
		Dictionary<string, List<IClass>> dictionary2 = new Dictionary<string, List<IClass>>(((DefaultProjectContent)this).Language.NameComparer);
		lock (((DefaultProjectContent)this).Namespaces)
		{
			for (int i = 0; i < ((DefaultProjectContent)this).ClassLists.Count; i++)
			{
				if (((DefaultProjectContent)this).ClassLists[i].Comparer == ((DefaultProjectContent)this).Language.NameComparer)
				{
					dictionary = ((DefaultProjectContent)this).ClassLists[i];
					break;
				}
			}
			foreach (KeyValuePair<string, IClass> item in dictionary)
			{
				if (item.Value is ClaClass claClass && !string.IsNullOrEmpty(claClass.PreName))
				{
					string key = claClass.Namespace + "." + claClass.PreName;
					if (dictionary2.ContainsKey(key))
					{
						dictionary2[key].Add((IClass)(object)claClass);
						continue;
					}
					List<IClass> list = new List<IClass>();
					list.Add((IClass)(object)claClass);
					dictionary2.Add(key, list);
				}
			}
		}
		if (lookInReferences)
		{
			lock (((DefaultProjectContent)this).ReferencedContents)
			{
				foreach (IProjectContent referencedContent in ((DefaultProjectContent)this).ReferencedContents)
				{
					if (!(referencedContent is IClarionProjectContent))
					{
						continue;
					}
					Dictionary<string, List<IClass>> clarionClassesWithPre = ((IClarionProjectContent)referencedContent).GetClarionClassesWithPre(lookInReferences: false);
					foreach (KeyValuePair<string, List<IClass>> item2 in clarionClassesWithPre)
					{
						if (dictionary2.ContainsKey(item2.Key))
						{
							dictionary2[item2.Key].AddRange(item2.Value);
						}
						else
						{
							dictionary2.Add(item2.Key, item2.Value);
						}
					}
				}
			}
		}
		return dictionary2;
	}

	public SearchTypeResult SearchTypeByPre(SearchTypeRequest request)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		string name = request.Name;
		if (string.IsNullOrEmpty(name))
		{
			return SearchTypeResult.Empty;
		}
		Dictionary<string, List<IClass>> clarionClassesWithPre = GetClarionClassesWithPre(lookInReferences: true);
		List<IClass> list = (clarionClassesWithPre.ContainsKey(name) ? clarionClassesWithPre[name] : new List<IClass>());
		if (request.CurrentType != null)
		{
			string text = request.CurrentType.Namespace;
			while (!string.IsNullOrEmpty(text))
			{
				string key = text + '.' + name;
				if (clarionClassesWithPre.ContainsKey(key))
				{
					list.AddRange(clarionClassesWithPre[key]);
				}
				int num = text.LastIndexOf('.');
				text = ((num < 0) ? null : text.Substring(0, num));
			}
		}
		if (request.CurrentCompilationUnit != null)
		{
			foreach (IUsing @using in request.CurrentCompilationUnit.Usings)
			{
				foreach (string using2 in @using.Usings)
				{
					string key2 = using2 + '.' + name;
					if (clarionClassesWithPre.ContainsKey(key2))
					{
						list.AddRange(clarionClassesWithPre[key2]);
					}
				}
			}
		}
		if (((DefaultProjectContent)this).DefaultImports != null)
		{
			foreach (string using3 in ((DefaultProjectContent)this).DefaultImports.Usings)
			{
				string key3 = using3 + '.' + name;
				if (clarionClassesWithPre.ContainsKey(key3))
				{
					list.AddRange(clarionClassesWithPre[key3]);
				}
			}
		}
		if (request.CurrentCompilationUnit is ClaCompilationUnit)
		{
			object obj = ((ClaCompilationUnit)(object)request.CurrentCompilationUnit).FindNearestObject(request.CaretLine, request.CaretColumn);
			if (obj is ClaMethod)
			{
				SearchLocalTypeByPre(list, request, (ClaMethod)obj);
			}
			if (obj is ClaLocalClass)
			{
				SearchLocalTypeByPre(list, request, ((ClaLocalClass)obj).DeclaringMethod);
			}
		}
		if (list.Count == 0)
		{
			return SearchTypeResult.Empty;
		}
		return new SearchTypeResult((IReturnType)(object)new ClaPreCollectionReturnType(list));
	}

	public override SearchTypeResult SearchType(SearchTypeRequest request)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		SearchTypeResult result = ((DefaultProjectContent)this).SearchType(request);
		if (((SearchTypeResult)(ref result)).Result == null && request.CurrentCompilationUnit is ClaCompilationUnit)
		{
			object obj = ((ClaCompilationUnit)(object)request.CurrentCompilationUnit).FindNearestObject(request.CaretLine, request.CaretColumn);
			if (obj is ClaMethod)
			{
				return SearchLocalType(request, (ClaMethod)obj);
			}
			if (obj is ClaLocalClass)
			{
				return SearchLocalType(request, ((ClaLocalClass)obj).DeclaringMethod);
			}
		}
		return result;
	}

	public SearchTypeResult SearchLocalType(SearchTypeRequest request, ClaMethod method)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (method == null)
		{
			return SearchTypeResult.Empty;
		}
		foreach (IClass localType in method.LocalTypes)
		{
			if (((DefaultProjectContent)this).Language.NameComparer.Equals(request.Name, localType.Name))
			{
				return new SearchTypeResult(localType.DefaultReturnType);
			}
		}
		if (method is ClaLocalMethod)
		{
			return SearchLocalType(request, ((ClaLocalMethod)method).DeclaringMethod);
		}
		if (method.DeclaringType is ClaLocalClass)
		{
			return SearchLocalType(request, ((ClaLocalClass)(object)method.DeclaringType).DeclaringMethod);
		}
		return SearchTypeResult.Empty;
	}

	public void SearchLocalTypeByPre(IList<IClass> results, SearchTypeRequest request, ClaMethod method)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (method == null)
		{
			return;
		}
		foreach (IClass localType in method.LocalTypes)
		{
			if (localType is ClaClass claClass && !string.IsNullOrEmpty(claClass.PreName) && ((DefaultProjectContent)this).Language.NameComparer.Equals(request.Name, claClass.PreName))
			{
				results.Add(localType);
			}
		}
		if (method is ClaLocalMethod)
		{
			SearchLocalTypeByPre(results, request, ((ClaLocalMethod)method).DeclaringMethod);
		}
		if (method.DeclaringType is ClaLocalClass)
		{
			SearchLocalTypeByPre(results, request, ((ClaLocalClass)(object)method.DeclaringType).DeclaringMethod);
		}
	}

	protected override void AddNamespaceContentsClass(ArrayList list, IClass c, LanguageProperties language, bool lookInReferences)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		if (((IDecoration)c).IsInternal && !lookInReferences)
		{
			return;
		}
		if (language.ShowInNamespaceCompletion(c))
		{
			list.Add(c);
		}
		if ((int)c.ClassType != 5)
		{
			return;
		}
		foreach (IMethod method in c.Methods)
		{
			if (((IDecoration)method).IsAccessible((IClass)null, false))
			{
				list.Add(method);
			}
		}
		foreach (IField field in c.Fields)
		{
			if (((IDecoration)field).IsAccessible((IClass)null, false))
			{
				list.Add(field);
			}
		}
	}

	public IReturnType SearchTypeForLike(string name, IClass declaringClass, int line, int column)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		ExpressionResult val = default(ExpressionResult);
		((ExpressionResult)(ref val))._002Ector(name);
		string fileName = declaringClass.CompilationUnit.FileName;
		if (ParserService.GetParseInformationIfExist(fileName) == null)
		{
			return null;
		}
		string text = ((!AppGenEditorsService.IsRegistered(fileName)) ? ParserService.GetParseableFileContent(fileName) : (AppGenEditorsService.GetPweeFileContent(fileName) ?? string.Empty));
		ResolveResult val2 = ParserService.Resolve(val, line, column, fileName, text);
		if (val2 != null)
		{
			return val2.ResolvedType;
		}
		return null;
	}
}
