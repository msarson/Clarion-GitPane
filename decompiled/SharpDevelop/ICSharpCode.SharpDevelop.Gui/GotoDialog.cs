using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.Gui;

public class GotoDialog : BaseSharpDevelopForm
{
	private static GotoDialog Instance;

	private ListView listView;

	private TextBox textBox;

	private ICompletionData[] ctrlSpaceCompletionData;

	private Dictionary<string, object> visibleEntries = new Dictionary<string, object>();

	private double bestPriority;

	private ListViewItem bestItem;

	public static void ShowSingleInstance()
	{
		if (Instance == null)
		{
			Instance = new GotoDialog();
			Instance.Show(WorkbenchSingleton.MainForm);
		}
		else
		{
			Instance.Focus();
		}
	}

	public GotoDialog()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.GotoDialog.xfrm"));
		base.ControlDictionary["okButton"].Click += OKButtonClick;
		base.ControlDictionary["cancelButton"].Click += CancelButtonClick;
		listView = (ListView)base.ControlDictionary["listView"];
		textBox = (TextBox)base.ControlDictionary["textBox"];
		textBox.TextChanged += TextBoxTextChanged;
		textBox.KeyDown += TextBoxKeyDown;
		listView.SmallImageList = ClassBrowserIconService.ImageList;
		listView.ItemActivate += OKButtonClick;
		listView.Sorting = SortOrder.Ascending;
		listView.SizeChanged += ListViewSizeChanged;
		listView.HideSelection = false;
		ListViewSizeChanged(null, null);
		base.Owner = WorkbenchSingleton.MainForm;
		base.Icon = null;
		FormPositionService.Instance.Apply(this, "GotoDialog");
	}

	private void ListViewSizeChanged(object sender, EventArgs e)
	{
		listView.Columns[0].Width = listView.Width - 24;
	}

	private void TextBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (listView.SelectedItems.Count != 0)
		{
			if (e.KeyData == Keys.Up)
			{
				e.Handled = true;
				ChangeIndex(-1);
			}
			else if (e.KeyData == Keys.Down)
			{
				e.Handled = true;
				ChangeIndex(1);
			}
			else if (e.KeyData == Keys.Prior)
			{
				e.Handled = true;
				ChangeIndex(-listView.ClientSize.Height / listView.Items[0].Bounds.Height);
			}
			else if (e.KeyData == Keys.Next)
			{
				e.Handled = true;
				ChangeIndex(listView.ClientSize.Height / listView.Items[0].Bounds.Height);
			}
		}
	}

	private void ChangeIndex(int increment)
	{
		int num = listView.SelectedIndices[0];
		num = Math.Max(0, Math.Min(listView.Items.Count - 1, num + increment));
		listView.Items[num].Selected = true;
		listView.EnsureVisible(num);
	}

	private ICompletionData[] GetCompletionData()
	{
		if (ctrlSpaceCompletionData != null)
		{
			return ctrlSpaceCompletionData;
		}
		TextEditorControl editor = GetEditor();
		if (editor != null)
		{
			CtrlSpaceCompletionDataProvider ctrlSpaceCompletionDataProvider = new CtrlSpaceCompletionDataProvider(ExpressionContext.Default);
			ctrlSpaceCompletionData = ctrlSpaceCompletionDataProvider.GenerateCompletionData(editor.FileName, editor.ActiveTextAreaControl.TextArea, '\0');
			return ctrlSpaceCompletionData;
		}
		return new ICompletionData[0];
	}

	private ICompletionData[] Resolve(string expression)
	{
		TextEditorControl editor = GetEditor();
		if (editor != null)
		{
			CodeCompletionDataProvider codeCompletionDataProvider = new CodeCompletionDataProvider(new ExpressionResult(expression));
			return codeCompletionDataProvider.GenerateCompletionData(editor.FileName, editor.ActiveTextAreaControl.TextArea, '.');
		}
		return new ICompletionData[0];
	}

	protected override void OnClosed(EventArgs e)
	{
		Instance = null;
		base.OnClosed(e);
	}

	private void TextBoxTextChanged(object sender, EventArgs e)
	{
		string text = textBox.Text.Trim();
		listView.BeginUpdate();
		listView.Items.Clear();
		visibleEntries.Clear();
		bestItem = null;
		if (text.Length == 0)
		{
			listView.EndUpdate();
			return;
		}
		if (text.Length == 1 && !char.IsDigit(text, 0))
		{
			listView.EndUpdate();
			return;
		}
		int num = text.IndexOf('.');
		int num2 = text.IndexOf(',');
		if (num2 < 0)
		{
			num2 = text.IndexOf(':');
		}
		if (char.IsDigit(text, 0))
		{
			ShowLineNumberItem(text);
		}
		else if (num2 > 0)
		{
			string text2 = text.Substring(0, num2).Trim();
			string text3 = text.Substring(num2 + 1).Trim();
			if (text3.StartsWith("line"))
			{
				text3 = text3.Substring(4).Trim();
			}
			if (!int.TryParse(text3, out var result))
			{
				result = 0;
			}
			AddSourceFiles(text2, result);
		}
		else if (num > 0)
		{
			AddSourceFiles(text, 0);
			string text4 = text.Substring(0, num).Trim();
			text = text.Substring(num + 1).Trim();
			ShowCompletionData(Resolve(text4), text);
			foreach (IClass item in SearchClasses(text4))
			{
				if (!item.Name.Equals(text4, StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}
				foreach (IMethod method in item.DefaultReturnType.GetMethods())
				{
					if (!method.IsConstructor)
					{
						AddItemIfMatchText(text, method, ClassBrowserIconService.GetIcon(method));
					}
				}
				foreach (IField field in item.DefaultReturnType.GetFields())
				{
					AddItemIfMatchText(text, field, ClassBrowserIconService.GetIcon(field));
				}
				foreach (IProperty property in item.DefaultReturnType.GetProperties())
				{
					AddItemIfMatchText(text, property, ClassBrowserIconService.GetIcon(property));
				}
				foreach (IEvent @event in item.DefaultReturnType.GetEvents())
				{
					AddItemIfMatchText(text, @event, ClassBrowserIconService.GetIcon(@event));
				}
			}
		}
		else
		{
			AddSourceFiles(text, 0);
			ShowCtrlSpaceCompletion(text);
		}
		if (bestItem != null)
		{
			bestItem.Selected = true;
			listView.EnsureVisible(bestItem.Index);
		}
		listView.EndUpdate();
	}

	private void AddSourceFiles(string text, int lineNumber)
	{
		if (ProjectService.OpenSolution == null)
		{
			return;
		}
		foreach (IProject project in ProjectService.OpenSolution.Projects)
		{
			foreach (ProjectItem item in project.Items)
			{
				if (item is FileProjectItem)
				{
					AddSourceFile(text, lineNumber, item);
				}
			}
		}
	}

	private void AddSourceFile(string text, int lineNumber, ProjectItem item)
	{
		string value = text.ToLowerInvariant();
		string fileName = item.FileName;
		string text2 = Path.GetFileName(fileName);
		if (text2.Length >= text.Length && text2.ToLowerInvariant().IndexOf(value) >= 0)
		{
			if (lineNumber > 0)
			{
				text2 = text2 + ", line " + lineNumber;
			}
			if (item.Project != null)
			{
				text2 = text2 + StringParser.Parse(" ${res:MainWindow.Windows.SearchResultPanel.In} ") + item.Project.Name;
			}
			AddItem(text2, 13, new FileLineReference(fileName, lineNumber), 0.5);
		}
	}

	private void ShowLineNumberItem(string text)
	{
		if (int.TryParse(text, out var result))
		{
			TextEditorControl editor = GetEditor();
			if (editor != null)
			{
				result = Math.Min(editor.Document.TotalNumberOfLines, Math.Max(1, result));
				AddItem(StringParser.Parse("${res:Dialog.Goto.GotoLine} ") + result, 13, result, 0.0);
			}
		}
	}

	private void ShowCompletionData(ICompletionData[] dataList, string text)
	{
		string value = text.ToLowerInvariant();
		foreach (ICompletionData completionData in dataList)
		{
			if (!(completionData is CodeCompletionData { Text: var text2 } codeCompletionData))
			{
				break;
			}
			if (text2.Length >= text.Length && text2.ToLowerInvariant().IndexOf(value) >= 0)
			{
				if (codeCompletionData.Class != null)
				{
					AddItem(codeCompletionData.Class, completionData.ImageIndex, completionData.Priority);
				}
				else if (codeCompletionData.Member != null)
				{
					AddItem(codeCompletionData.Member, completionData.ImageIndex, completionData.Priority);
				}
			}
		}
	}

	private void ShowCtrlSpaceCompletion(string text)
	{
		ShowCompletionData(GetCompletionData(), text);
		foreach (IClass item in SearchClasses(text))
		{
			AddItem(item);
		}
	}

	private ArrayList SearchClasses(string text)
	{
		string lowerText = text.ToLowerInvariant();
		ArrayList arrayList = new ArrayList();
		if (ProjectService.OpenSolution != null)
		{
			foreach (IProject project in ProjectService.OpenSolution.Projects)
			{
				IProjectContent projectContent = ParserService.GetProjectContent(project);
				if (projectContent != null)
				{
					AddClasses(lowerText, arrayList, projectContent.Classes);
				}
			}
		}
		return arrayList;
	}

	private void AddClasses(string lowerText, ArrayList list, IEnumerable<IClass> classes)
	{
		foreach (IClass @class in classes)
		{
			string name = @class.Name;
			if (name.Length >= lowerText.Length && name.ToLowerInvariant().IndexOf(lowerText) >= 0)
			{
				list.Add(@class);
			}
			AddClasses(lowerText, list, @class.InnerClasses);
		}
	}

	private void AddItem(string text, int imageIndex, object tag, double priority)
	{
		if (!visibleEntries.ContainsKey(text))
		{
			visibleEntries.Add(text, null);
			ListViewItem listViewItem = new ListViewItem(text, imageIndex);
			listViewItem.Tag = tag;
			if (bestItem == null || (priority > bestPriority && (!(tag is IClass) || !(bestItem.Tag is IMember))) || (tag is IMember && bestItem.Tag is IClass))
			{
				bestItem = listViewItem;
				bestPriority = priority;
			}
			listView.Items.Add(listViewItem);
		}
	}

	private void AddItem(IClass c)
	{
		AddItem(c, ClassBrowserIconService.GetIcon(c), CodeCompletionDataUsageCache.GetPriority(c.DotNetName, incrementShowCount: true));
	}

	private void AddItemIfMatchText(string text, IMember member, int imageIndex)
	{
		string name = member.Name;
		if (name.Length >= text.Length && text.Equals(name.Substring(0, text.Length), StringComparison.OrdinalIgnoreCase))
		{
			AddItem(member, imageIndex, CodeCompletionDataUsageCache.GetPriority(member.DotNetName, incrementShowCount: true));
		}
	}

	private void AddItem(IClass c, int imageIndex, double priority)
	{
		AddItem(c.Name + " (" + c.FullyQualifiedName + ")", imageIndex, c, priority);
	}

	private void AddItem(IMember m, int imageIndex, double priority)
	{
		AddItem(m.Name + " (" + m.FullyQualifiedName + ")", imageIndex, m, priority);
	}

	private void CancelButtonClick(object sender, EventArgs e)
	{
		Close();
	}

	private void GotoRegion(DomRegion region, string fileName)
	{
		if (fileName != null && !region.IsEmpty)
		{
			FileService.JumpToFilePosition(fileName, region.BeginLine - 1, region.BeginColumn - 1);
		}
	}

	private TextEditorControl GetEditor()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null && activeWorkbenchWindow.ViewContent is ITextEditorControlProvider)
		{
			return ((ITextEditorControlProvider)activeWorkbenchWindow.ViewContent).TextEditorControl;
		}
		return null;
	}

	private void OKButtonClick(object sender, EventArgs e)
	{
		try
		{
			if (listView.SelectedItems.Count == 0)
			{
				return;
			}
			object tag = listView.SelectedItems[0].Tag;
			if (tag is int)
			{
				TextEditorControl editor = GetEditor();
				if (editor != null)
				{
					int num = Math.Min(editor.Document.TotalNumberOfLines, Math.Max(1, (int)tag));
					editor.ActiveTextAreaControl.JumpTo(num - 1, int.MaxValue);
				}
				return;
			}
			if (tag is IClass)
			{
				IClass obj = tag as IClass;
				CodeCompletionDataUsageCache.IncrementUsage(obj.DotNetName);
				GotoRegion(obj.Region, obj.CompilationUnit.FileName);
				return;
			}
			if (tag is IMember)
			{
				IMember member = tag as IMember;
				CodeCompletionDataUsageCache.IncrementUsage(member.DotNetName);
				GotoRegion(member.Region, member.DeclaringType.CompilationUnit.FileName);
				return;
			}
			if (tag is FileLineReference)
			{
				FileLineReference fileLineReference = (FileLineReference)tag;
				if (fileLineReference.Line <= 0)
				{
					FileService.OpenFile(fileLineReference.FileName);
				}
				else
				{
					FileService.JumpToFilePosition(fileLineReference.FileName, fileLineReference.Line - 1, fileLineReference.Column);
				}
				return;
			}
			throw new NotImplementedException("Unknown tag: " + tag);
		}
		finally
		{
			Close();
		}
	}
}
