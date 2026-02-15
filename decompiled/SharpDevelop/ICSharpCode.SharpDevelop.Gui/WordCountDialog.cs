using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.XmlForms;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class WordCountDialog : BaseSharpDevelopForm
{
	internal enum SearchOptions
	{
		CurrentFile,
		AllFiles,
		AllProjects
	}

	internal class Report
	{
		public string name;

		public long chars;

		public long words;

		public long lines;

		public Report(string name, long chars, long words, long lines)
		{
			this.name = name;
			this.chars = chars;
			this.words = words;
			this.lines = lines;
		}

		public ListViewItem ToListItem()
		{
			return new ListViewItem(new string[4]
			{
				Path.GetFileName(name),
				chars.ToString(),
				words.ToString(),
				lines.ToString()
			});
		}

		public static Report operator +(Report r, Report s)
		{
			Report report = new Report(ResourceService.GetString("Dialog.WordCountDialog.TotalText"), s.chars, s.words, s.lines);
			report.chars += r.chars;
			report.words += r.words;
			report.lines += r.lines;
			return report;
		}
	}

	internal class ReportComparer : IComparer<Report>
	{
		private int sortKey;

		public ReportComparer(int SortKey)
		{
			sortKey = SortKey;
		}

		public int Compare(Report x, Report y)
		{
			if (x == null || y == null)
			{
				return 1;
			}
			return sortKey switch
			{
				0 => string.Compare(x.name, y.name), 
				1 => x.chars.CompareTo(y.chars), 
				2 => x.words.CompareTo(y.words), 
				3 => x.lines.CompareTo(y.lines), 
				_ => 1, 
			};
		}
	}

	private List<Report> items;

	private Report total;

	private List<SearchOptions> searchOptionsIndex = new List<SearchOptions>();

	private Report GetReport(string filename)
	{
		if (!File.Exists(filename))
		{
			return null;
		}
		using StreamReader reader = new StreamReader(filename);
		return GetReport(filename, reader);
	}

	private Report GetReport(string filename, TextReader reader)
	{
		long num = 0L;
		long num2 = 0L;
		long num3 = 0L;
		for (string text = reader.ReadLine(); text != null; text = reader.ReadLine())
		{
			num++;
			num3 += text.Length;
			string[] array = text.Split(null);
			num2 += array.Length;
		}
		return new Report(filename, num3, num2, num);
	}

	private void startEvent(object sender, EventArgs e)
	{
		items = new List<Report>();
		total = null;
		switch (searchOptionsIndex[((ComboBox)base.ControlDictionary["locationComboBox"]).SelectedIndex])
		{
		case SearchOptions.CurrentFile:
		{
			IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
			if (activeWorkbenchWindow == null)
			{
				break;
			}
			if (!(activeWorkbenchWindow.ViewContent is IEditable editable2))
			{
				MessageService.ShowWarning("${res:Dialog.WordCountDialog.IsNotTextFile}");
				break;
			}
			Report report2 = GetReport(activeWorkbenchWindow.ViewContent.IsUntitled ? activeWorkbenchWindow.ViewContent.UntitledName : activeWorkbenchWindow.ViewContent.FileName, new StringReader(editable2.Text));
			if (report2 != null)
			{
				items.Add(report2);
			}
			break;
		}
		case SearchOptions.AllFiles:
			if (WorkbenchSingleton.Workbench.ViewContentCollection.Count <= 0)
			{
				break;
			}
			total = new Report(StringParser.Parse("${res:Dialog.WordCountDialog.TotalText}"), 0L, 0L, 0L);
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				if (item is IEditable editable)
				{
					Report report = GetReport(item.IsUntitled ? item.UntitledName : item.FileName, new StringReader(editable.Text));
					if (report != null)
					{
						total += report;
						items.Add(report);
					}
				}
			}
			break;
		case SearchOptions.AllProjects:
			if (ProjectService.OpenSolution == null)
			{
				MessageService.ShowError("${res:Dialog.WordCountDialog.MustBeInProtectedModeWarning}");
				break;
			}
			total = new Report(StringParser.Parse("${res:Dialog.WordCountDialog.TotalText}"), 0L, 0L, 0L);
			CountCombine(ProjectService.OpenSolution, ref total);
			break;
		}
		UpdateList(0);
	}

	private void CountCombine(Solution combine, ref Report all)
	{
		foreach (IProject project in combine.Projects)
		{
			foreach (ProjectItem item in project.Items)
			{
				if (item.ItemType == ItemType.Compile)
				{
					Report report = GetReport(item.FileName);
					if (report != null)
					{
						all += report;
						items.Add(report);
					}
				}
			}
		}
	}

	private void UpdateList(int SortKey)
	{
		if (items == null)
		{
			return;
		}
		((ListView)base.ControlDictionary["resultListView"]).BeginUpdate();
		((ListView)base.ControlDictionary["resultListView"]).Items.Clear();
		if (items.Count != 0)
		{
			ReportComparer comparer = new ReportComparer(SortKey);
			items.Sort(comparer);
			for (int i = 0; i < items.Count; i++)
			{
				((ListView)base.ControlDictionary["resultListView"]).Items.Add(items[i].ToListItem());
			}
			if (total != null)
			{
				((ListView)base.ControlDictionary["resultListView"]).Items.Add(new ListViewItem(""));
				((ListView)base.ControlDictionary["resultListView"]).Items.Add(total.ToListItem());
			}
		}
		((ListView)base.ControlDictionary["resultListView"]).EndUpdate();
	}

	private void SortEvt(object sender, ColumnClickEventArgs e)
	{
		UpdateList(e.Column);
	}

	public WordCountDialog()
	{
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.WordCountDialog.xfrm"));
		((Button)base.ControlDictionary["startButton"]).Click += startEvent;
		((ListView)base.ControlDictionary["resultListView"]).ColumnClick += SortEvt;
		base.Icon = IconService.GetIcon("Icons.16x16.FindIcon");
		base.Owner = (Form)WorkbenchSingleton.Workbench;
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow != null)
		{
			if (activeWorkbenchWindow.ViewContent is IEditable)
			{
				((ComboBox)base.ControlDictionary["locationComboBox"]).Items.Add(StringParser.Parse("${res:Global.Location.currentfile}"));
				searchOptionsIndex.Add(SearchOptions.CurrentFile);
			}
			bool flag = false;
			foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
			{
				if (item is IEditable)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				((ComboBox)base.ControlDictionary["locationComboBox"]).Items.Add(StringParser.Parse("${res:Global.Location.allopenfiles}"));
				searchOptionsIndex.Add(SearchOptions.AllFiles);
			}
		}
		if (ProjectService.OpenSolution != null)
		{
			((ComboBox)base.ControlDictionary["locationComboBox"]).Items.Add(StringParser.Parse("${res:Global.Location.wholeproject}"));
			searchOptionsIndex.Add(SearchOptions.AllProjects);
		}
		((ComboBox)base.ControlDictionary["locationComboBox"]).SelectedIndex = 0;
	}
}
