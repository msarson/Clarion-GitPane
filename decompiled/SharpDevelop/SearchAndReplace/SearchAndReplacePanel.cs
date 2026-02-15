using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace SearchAndReplace;

public class SearchAndReplacePanel : BaseSharpDevelopUserControl
{
	private SearchAndReplaceMode searchAndReplaceMode;

	private ISearcher currentSearcher;

	private int customDirectoryIndex = 5;

	private Font textFieldsFont;

	public SearchAndReplaceMode SearchAndReplaceMode
	{
		get
		{
			return searchAndReplaceMode;
		}
		set
		{
			searchAndReplaceMode = value;
			SuspendLayout();
			base.Controls.Clear();
			switch (searchAndReplaceMode)
			{
			case SearchAndReplaceMode.Search:
				SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.FindPanel.xfrm"));
				Get<Button>("bookmarkAll").Click += BookmarkAllButtonClicked;
				Get<Button>("findAll").Click += FindAllButtonClicked;
				base.ParentForm.AcceptButton = Get<Button>("findNext");
				Get<ComboBox>("find").Font = textFieldsFont;
				break;
			case SearchAndReplaceMode.Replace:
				SetupFromXmlStream(GetType().Assembly.GetManifestResourceStream("Resources.ReplacePanel.xfrm"));
				Get<Button>("replace").Click += ReplaceButtonClicked;
				Get<Button>("replaceAll").Click += ReplaceAllButtonClicked;
				base.ParentForm.AcceptButton = Get<Button>("replace");
				Get<ComboBox>("find").Font = textFieldsFont;
				Get<ComboBox>("replace").Font = textFieldsFont;
				break;
			}
			Dock = DockStyle.None;
			Get<ComboBox>("find").TextChanged += FindPatternChanged;
			ControlDictionary["findNextButton"].Click += FindNextButtonClicked;
			ControlDictionary["lookInBrowseButton"].Click += LookInBrowseButtonClicked;
			((Form)base.Parent).AcceptButton = (Button)ControlDictionary["findNextButton"];
			SetOptions();
			EnableButtons(HasFindPattern);
			RightToLeftConverter.ReConvertRecursive(this);
			base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			base.AutoScaleMode = AutoScaleMode.Font;
			ResumeLayout(performLayout: true);
			PerformAutoScale();
			SearchReplaceManager.ResetSearch();
		}
	}

	private bool IsSelectionSearch => currentSearcher != null;

	private bool HasFindPattern => Get<ComboBox>("find").Text.Length != 0;

	public SearchAndReplacePanel()
	{
		textFieldsFont = FontService.GetFont(FontService.FontType.TextEditor);
		Font font = FontService.GetFont(FontService.FontType.Dialogs);
		textFieldsFont = new Font(textFieldsFont.FontFamily, (int)font.Size);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			foreach (AbstractSearchAndReplaceBinding binding in SearchOptions.Bindings)
			{
				binding.ActiveChanged = (EventHandler)Delegate.Remove(binding.ActiveChanged, new EventHandler(BindingActiveChanged));
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void LookInBrowseButtonClicked(object sender, EventArgs e)
	{
		ComboBox comboBox = Get<ComboBox>("lookIn");
		using FolderBrowserDialog folderBrowserDialog = FileService.CreateFolderBrowserDialog("${res:Dialog.NewProject.SearchReplace.LookIn.SelectDirectory}", comboBox.Text);
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
		{
			comboBox.SelectedIndex = customDirectoryIndex;
			comboBox.Text = folderBrowserDialog.SelectedPath;
		}
	}

	private void FindNextButtonClicked(object sender, EventArgs e)
	{
		bool flag = WritebackOptions();
		if (IsSelectionSearch)
		{
			currentSearcher.FindNext();
		}
		else
		{
			using ProgressNotificationTaskInstance monitor = new ProgressNotificationTaskInstance("Searching...", allowCancel: true);
			if (flag)
			{
				SearchReplaceManager.ResetSearch();
			}
			SearchReplaceManager.FindNext(monitor);
		}
		Focus();
	}

	private void FindAll()
	{
		try
		{
			Cursor.Current = Cursors.WaitCursor;
			if (IsSelectionSearch)
			{
				using ProgressNotificationTaskInstance monitor = new ProgressNotificationTaskInstance("Searching...", 100, allowCancel: true);
				currentSearcher.RunAll(SearchType.Find, monitor);
			}
			else
			{
				using ProgressNotificationTaskInstance monitor2 = new ProgressNotificationTaskInstance("Searching...", allowCancel: true);
				SearchInFilesManager.FindAll(monitor2);
			}
			Cursor.Current = Cursors.Default;
		}
		catch (Exception ex)
		{
			Cursor.Current = Cursors.Default;
			MessageService.ShowError(ex);
		}
	}

	private void FindAllButtonClicked(object sender, EventArgs e)
	{
		WritebackOptions();
		ComboBox comboBox = Get<ComboBox>("lookIn");
		if (comboBox.SelectedIndex != -1)
		{
			AbstractSearchAndReplaceBinding abstractSearchAndReplaceBinding = (AbstractSearchAndReplaceBinding)comboBox.Items[comboBox.SelectedIndex];
			if (StatusBarService.ProgressMonitor.ShowNotifications && abstractSearchAndReplaceBinding.NeedsFilePattern)
			{
				base.ParentForm.Close();
				Thread thread = new Thread(FindAll);
				thread.Start();
			}
			else
			{
				FindAll();
				base.ParentForm.Close();
			}
		}
	}

	private void BookmarkAll()
	{
		try
		{
			Cursor.Current = Cursors.WaitCursor;
			if (IsSelectionSearch)
			{
				using ProgressNotificationTaskInstance monitor = new ProgressNotificationTaskInstance("Searching...", allowCancel: true);
				currentSearcher.RunAll(SearchType.BookMark, monitor);
			}
			else
			{
				using ProgressNotificationTaskInstance monitor2 = new ProgressNotificationTaskInstance("Searching...", allowCancel: true);
				SearchReplaceManager.ResetSearch();
				SearchReplaceManager.MarkAll(monitor2);
			}
			Cursor.Current = Cursors.Default;
		}
		catch (Exception ex)
		{
			Cursor.Current = Cursors.Default;
			MessageService.ShowError(ex);
		}
	}

	private void BookmarkAllButtonClicked(object sender, EventArgs e)
	{
		WritebackOptions();
		BookmarkAll();
	}

	private void ReplaceAll()
	{
		try
		{
			Cursor.Current = Cursors.WaitCursor;
			if (IsSelectionSearch)
			{
				using ProgressNotificationTaskInstance monitor = new ProgressNotificationTaskInstance("Searching and Replacing...", allowCancel: true);
				currentSearcher.RunAll(SearchType.Replace, monitor);
			}
			else
			{
				using ProgressNotificationTaskInstance monitor2 = new ProgressNotificationTaskInstance("Searching and Replacing...", allowCancel: true);
				SearchReplaceManager.ResetSearch();
				SearchReplaceManager.ReplaceAll(monitor2);
			}
			Cursor.Current = Cursors.Default;
		}
		catch (Exception ex)
		{
			Cursor.Current = Cursors.Default;
			MessageService.ShowError(ex);
		}
	}

	private void ReplaceAllButtonClicked(object sender, EventArgs e)
	{
		WritebackOptions();
		ReplaceAll();
		base.ParentForm.Close();
	}

	private void ReplaceButtonClicked(object sender, EventArgs e)
	{
		bool flag = WritebackOptions();
		if (IsSelectionSearch)
		{
			currentSearcher.Replace();
		}
		else
		{
			using ProgressNotificationTaskInstance monitor = new ProgressNotificationTaskInstance("Searching and Replacing...", allowCancel: true);
			if (flag)
			{
				SearchReplaceManager.ResetSearch();
			}
			SearchReplaceManager.Replace(monitor);
		}
		Focus();
	}

	private bool WritebackOptions()
	{
		bool result = false;
		string text = Get<ComboBox>("find").Text;
		if (text != SearchOptions.FindPattern)
		{
			result = true;
			SearchOptions.FindPattern = text;
		}
		if (searchAndReplaceMode == SearchAndReplaceMode.Replace)
		{
			text = Get<ComboBox>("replace").Text;
			if (text != SearchOptions.ReplacePattern)
			{
				result = true;
				SearchOptions.ReplacePattern = text;
			}
		}
		if (SearchOptions.SearchAndReplaceBinding == SearchOptions.DirectoryBinding)
		{
			text = Get<ComboBox>("lookIn").Text;
			if (text != SearchOptions.LookIn)
			{
				result = true;
				SearchOptions.LookIn = text;
			}
		}
		text = Get<ComboBox>("fileTypes").Text;
		if (text != SearchOptions.LookInFiletypes)
		{
			result = true;
			SearchOptions.LookInFiletypes = text;
		}
		bool flag = Get<CheckBox>("matchCase").Checked;
		if (flag != SearchOptions.MatchCase)
		{
			result = true;
			SearchOptions.MatchCase = flag;
		}
		flag = Get<CheckBox>("matchWholeWord").Checked;
		if (flag != SearchOptions.MatchWholeWord)
		{
			result = true;
			SearchOptions.MatchWholeWord = flag;
		}
		flag = Get<CheckBox>("includeSubFolder").Checked;
		if (flag != SearchOptions.IncludeSubdirectories)
		{
			result = true;
			SearchOptions.IncludeSubdirectories = flag;
		}
		flag = Get<CheckBox>("includeReadOnly").Checked;
		if (flag != SearchOptions.IncludeReadOnlyBlocks)
		{
			result = true;
			SearchOptions.IncludeReadOnlyBlocks = flag;
		}
		SearchStrategyType selectedIndex = (SearchStrategyType)Get<ComboBox>("use").SelectedIndex;
		if (selectedIndex != SearchOptions.SearchStrategyType)
		{
			result = true;
			SearchOptions.SearchStrategyType = selectedIndex;
		}
		ComboBox comboBox = Get<ComboBox>("lookIn");
		if (comboBox.SelectedIndex != -1)
		{
			AbstractSearchAndReplaceBinding abstractSearchAndReplaceBinding = (AbstractSearchAndReplaceBinding)comboBox.Items[comboBox.SelectedIndex];
			if (abstractSearchAndReplaceBinding != SearchOptions.SearchAndReplaceBinding)
			{
				result = true;
				SearchOptions.SearchAndReplaceBinding = abstractSearchAndReplaceBinding;
			}
		}
		return result;
	}

	private void SetOptions()
	{
		Get<ComboBox>("find").Text = SearchOptions.FindPattern;
		Get<ComboBox>("find").Items.Clear();
		Get<ComboBox>("find").Text = SearchOptions.FindPattern;
		Get<ComboBox>("find").Items.Clear();
		string[] findPatterns = SearchOptions.FindPatterns;
		foreach (string item in findPatterns)
		{
			Get<ComboBox>("find").Items.Add(item);
		}
		if (searchAndReplaceMode == SearchAndReplaceMode.Replace)
		{
			Get<ComboBox>("replace").Text = SearchOptions.ReplacePattern;
			Get<ComboBox>("replace").Items.Clear();
			string[] replacePatterns = SearchOptions.ReplacePatterns;
			foreach (string item2 in replacePatterns)
			{
				Get<ComboBox>("replace").Items.Add(item2);
			}
		}
		ComboBox comboBox = Get<ComboBox>("lookIn");
		ComboBox.ObjectCollection items = comboBox.Items;
		foreach (AbstractSearchAndReplaceBinding binding in SearchOptions.Bindings)
		{
			binding.ActiveChanged = (EventHandler)Delegate.Combine(binding.ActiveChanged, new EventHandler(BindingActiveChanged));
			if (binding.Active)
			{
				items.Add(binding);
				if (binding.NeedsFileList)
				{
					customDirectoryIndex = items.IndexOf(binding);
				}
			}
		}
		comboBox.SelectedIndexChanged += LookInSelectedIndexChanged;
		int num = items.IndexOf(SearchOptions.SearchAndReplaceBinding);
		if (num == -1)
		{
			num = items.IndexOf(SearchOptions.CurrentDocumentBinding);
		}
		if (num == -1)
		{
			num = items.IndexOf(SearchOptions.DirectoryBinding);
		}
		comboBox.SelectedIndex = num;
		Get<ComboBox>("fileTypes").Text = SearchOptions.LookInFiletypes;
		Get<CheckBox>("matchCase").Checked = SearchOptions.MatchCase;
		Get<CheckBox>("matchWholeWord").Checked = SearchOptions.MatchWholeWord;
		Get<CheckBox>("includeSubFolder").Checked = SearchOptions.IncludeSubdirectories;
		Get<CheckBox>("includeReadOnly").Checked = SearchOptions.IncludeReadOnlyBlocks;
		Get<ComboBox>("use").Items.Clear();
		Get<ComboBox>("use").Items.Add(StringParser.Parse("${res:Dialog.NewProject.SearchReplace.SearchStrategy.Standard}"));
		Get<ComboBox>("use").Items.Add(StringParser.Parse("${res:Dialog.NewProject.SearchReplace.SearchStrategy.RegexSearch}"));
		Get<ComboBox>("use").Items.Add(StringParser.Parse("${res:Dialog.NewProject.SearchReplace.SearchStrategy.WildcardSearch}"));
		switch (SearchOptions.SearchStrategyType)
		{
		case SearchStrategyType.RegEx:
			Get<ComboBox>("use").SelectedIndex = 1;
			break;
		case SearchStrategyType.Wildcard:
			Get<ComboBox>("use").SelectedIndex = 2;
			break;
		default:
			Get<ComboBox>("use").SelectedIndex = 0;
			break;
		}
	}

	private void BindingActiveChanged(object sender, EventArgs e)
	{
		ComboBox comboBox = Get<ComboBox>("lookIn");
		ComboBox.ObjectCollection items = comboBox.Items;
		AbstractSearchAndReplaceBinding abstractSearchAndReplaceBinding = (AbstractSearchAndReplaceBinding)sender;
		if (abstractSearchAndReplaceBinding.Active && !items.Contains(abstractSearchAndReplaceBinding))
		{
			int num = SearchOptions.Bindings.IndexOf(abstractSearchAndReplaceBinding);
			int i;
			for (i = 0; i < items.Count && SearchOptions.Bindings.IndexOf((AbstractSearchAndReplaceBinding)items[i]) < num; i++)
			{
			}
			if (i == items.Count - 1 && SearchOptions.Bindings.IndexOf((AbstractSearchAndReplaceBinding)items[i]) < num)
			{
				items.Add(abstractSearchAndReplaceBinding);
			}
			else
			{
				items.Insert(i, abstractSearchAndReplaceBinding);
			}
		}
		else
		{
			if (abstractSearchAndReplaceBinding.Active || !items.Contains(abstractSearchAndReplaceBinding))
			{
				return;
			}
			if (comboBox.SelectedIndex == items.IndexOf(abstractSearchAndReplaceBinding))
			{
				int num2 = items.IndexOf(SearchOptions.CurrentDocumentBinding);
				if (num2 == -1)
				{
					num2 = items.IndexOf(SearchOptions.DirectoryBinding);
				}
				comboBox.SelectedIndex = num2;
			}
			items.Remove(abstractSearchAndReplaceBinding);
		}
	}

	private void LookInSelectedIndexChanged(object sender, EventArgs e)
	{
		ComboBox comboBox = Get<ComboBox>("lookIn");
		if (comboBox.SelectedIndex == -1)
		{
			Get<ComboBox>("fileTypes").Enabled = false;
			Get<Label>("lookAtTypes").Enabled = false;
			Get<CheckBox>("includeSubFolder").Enabled = false;
			Get<CheckBox>("includeReadOnly").Enabled = true;
			Get<ComboBox>("lookIn").DropDownStyle = ComboBoxStyle.DropDownList;
			currentSearcher = null;
		}
		else
		{
			AbstractSearchAndReplaceBinding abstractSearchAndReplaceBinding = (SearchOptions.SearchAndReplaceBinding = (AbstractSearchAndReplaceBinding)comboBox.Items[comboBox.SelectedIndex]);
			if (abstractSearchAndReplaceBinding.HasFullSearcher)
			{
				currentSearcher = abstractSearchAndReplaceBinding.GetSearcher();
			}
			else
			{
				currentSearcher = null;
			}
			if (abstractSearchAndReplaceBinding.NeedsFilePattern)
			{
				Get<ComboBox>("fileTypes").Enabled = true;
				Get<Label>("lookAtTypes").Enabled = true;
			}
			else
			{
				Get<ComboBox>("fileTypes").Enabled = false;
				Get<Label>("lookAtTypes").Enabled = false;
			}
			Get<CheckBox>("includeSubFolder").Enabled = abstractSearchAndReplaceBinding.NeedsSubFolders;
			Get<CheckBox>("includeReadOnly").Enabled = !abstractSearchAndReplaceBinding.NeedsFileList && !abstractSearchAndReplaceBinding.NeedsFilePattern && !abstractSearchAndReplaceBinding.NeedsSubFolders;
			if (abstractSearchAndReplaceBinding.NeedsFileList)
			{
				Get<ComboBox>("lookIn").DropDownStyle = ComboBoxStyle.DropDown;
			}
			else
			{
				Get<ComboBox>("lookIn").DropDownStyle = ComboBoxStyle.DropDownList;
			}
		}
		if (IsSelectionSearch)
		{
			currentSearcher.Init();
		}
	}

	private void EnableButtons(bool enabled)
	{
		if (searchAndReplaceMode == SearchAndReplaceMode.Replace)
		{
			Get<Button>("replace").Enabled = enabled;
			Get<Button>("replaceAll").Enabled = enabled;
		}
		else
		{
			Get<Button>("bookmarkAll").Enabled = enabled;
			Get<Button>("findAll").Enabled = enabled;
		}
		ControlDictionary["findNextButton"].Enabled = enabled;
	}

	private void FindPatternChanged(object source, EventArgs e)
	{
		EnableButtons(HasFindPattern);
	}
}
