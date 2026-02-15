using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui.OptionPanels;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class CompilerMessageView : AbstractPadContent, IClipboardHandler
{
	private const int WM_SETREDRAW = 11;

	private static CompilerMessageView instance;

	private RichTextBox textEditorControl = new RichTextBox();

	private Panel myPanel = new Panel();

	private ToolStrip toolStrip;

	private List<MessageViewCategory> messageCategories = new List<MessageViewCategory>();

	private int selectedCategory;

	private Properties properties;

	private object appendCallLock = new object();

	private volatile int pendingAppendCalls;

	public static CompilerMessageView Instance
	{
		get
		{
			if (instance == null)
			{
				WorkbenchSingleton.SafeThreadCall(InitializeInstance);
			}
			return instance;
		}
	}

	public int SelectedCategoryIndex
	{
		get
		{
			return selectedCategory;
		}
		set
		{
			if (selectedCategory != value)
			{
				selectedCategory = value;
				textEditorControl.Text = ((value < 0) ? "" : StringParser.Parse(messageCategories[value].Text));
				textEditorControl.Refresh();
				OnSelectedCategoryIndexChanged(EventArgs.Empty);
				ToolbarService.UpdateToolbar(toolStrip);
			}
		}
	}

	public bool WordWrap
	{
		get
		{
			return properties.Get("WordWrap", defaultValue: true);
		}
		set
		{
			properties.Set("WordWrap", value);
		}
	}

	public MessageViewCategory SelectedMessageViewCategory
	{
		get
		{
			if (selectedCategory >= 0)
			{
				return messageCategories[selectedCategory];
			}
			return null;
		}
	}

	public List<MessageViewCategory> MessageCategories => messageCategories;

	public override Control Control => myPanel;

	public bool EnableCut => false;

	public bool EnableCopy => textEditorControl.SelectionLength > 0;

	public bool EnablePaste => false;

	public bool EnableDelete => false;

	public bool EnableSelectAll => textEditorControl.TextLength > 0;

	public event EventHandler MessageCategoryAdded;

	public event EventHandler SelectedCategoryIndexChanged;

	private static void InitializeInstance()
	{
		if (WorkbenchSingleton.Workbench != null)
		{
			WorkbenchSingleton.Workbench.GetPad(typeof(CompilerMessageView)).CreatePad();
		}
	}

	public CompilerMessageView()
	{
		instance = this;
		AddCategory(TaskService.BuildMessageViewCategory);
		myPanel.SuspendLayout();
		textEditorControl.Dock = DockStyle.Fill;
		textEditorControl.BorderStyle = BorderStyle.FixedSingle;
		textEditorControl.BackColor = SystemColors.Window;
		textEditorControl.LinkClicked += delegate(object sender, LinkClickedEventArgs e)
		{
			FileService.OpenFile("browser://" + e.LinkText);
		};
		textEditorControl.HideSelection = false;
		textEditorControl.ReadOnly = true;
		textEditorControl.ContextMenuStrip = MenuService.CreateContextMenu(this, "/SharpDevelop/Pads/CompilerMessageView/ContextMenu");
		properties = PropertyService.Get(OutputWindowOptionsPanel.OutputWindowsProperty, new Properties());
		textEditorControl.Font = FontService.GetFont(FontService.FontType.TextEditor);
		properties.PropertyChanged += PropertyChanged;
		textEditorControl.DoubleClick += TextEditorControlDoubleClick;
		toolStrip = ToolbarService.CreateToolStrip(this, "/SharpDevelop/Pads/CompilerMessageView/Toolbar");
		toolStrip.Stretch = true;
		toolStrip.GripStyle = ToolStripGripStyle.Hidden;
		myPanel.Controls.AddRange(new Control[2] { textEditorControl, toolStrip });
		SetWordWrap();
		myPanel.ResumeLayout(performLayout: false);
		SetText(messageCategories[selectedCategory], messageCategories[selectedCategory].Text);
		ProjectService.SolutionLoaded += SolutionLoaded;
	}

	private void SolutionLoaded(object sender, SolutionEventArgs e)
	{
		foreach (MessageViewCategory messageCategory in messageCategories)
		{
			ClearText(messageCategory);
		}
	}

	private void SetWordWrap()
	{
		bool wordWrap = WordWrap;
		textEditorControl.WordWrap = wordWrap;
		if (wordWrap)
		{
			textEditorControl.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
		}
		else
		{
			textEditorControl.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
		}
	}

	public override void RedrawContent()
	{
		base.RedrawContent();
		textEditorControl.Update();
		ToolbarService.UpdateToolbar(toolStrip);
	}

	public void AddCategory(MessageViewCategory category)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadAsyncCall(AddCategory, category);
			return;
		}
		messageCategories.Add(category);
		category.Cleared += CategoryTextCleared;
		category.TextSet += CategoryTextSet;
		category.TextAppended += CategoryTextAppended;
		OnMessageCategoryAdded(EventArgs.Empty);
	}

	private void CategoryTextCleared(object sender, EventArgs e)
	{
		WorkbenchSingleton.SafeThreadAsyncCall(ClearText, (MessageViewCategory)sender);
	}

	private void ClearText(MessageViewCategory category)
	{
		if (messageCategories[SelectedCategoryIndex] == category)
		{
			textEditorControl.Text = string.Empty;
		}
	}

	private void CategoryTextSet(object sender, TextEventArgs e)
	{
		WorkbenchSingleton.SafeThreadAsyncCall(SetText, (MessageViewCategory)sender, e.Text);
	}

	private void CategoryTextAppended(object sender, TextEventArgs e)
	{
		lock (appendCallLock)
		{
			pendingAppendCalls++;
			MessageViewCategory messageViewCategory = (MessageViewCategory)sender;
			if (pendingAppendCalls < 5)
			{
				WorkbenchSingleton.SafeThreadAsyncCall(AppendText, messageViewCategory, messageViewCategory.Text, e.Text);
			}
			else if (pendingAppendCalls == 5)
			{
				WorkbenchSingleton.SafeThreadAsyncCall(AppendTextCombined, messageViewCategory);
			}
		}
	}

	[DllImport("user32.dll")]
	[SuppressUnmanagedCodeSecurity]
	private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	private void SetUpdate(bool update)
	{
		SendMessage(textEditorControl.Handle, 11, update ? new IntPtr(1) : IntPtr.Zero, IntPtr.Zero);
	}

	private void AppendTextCombined(MessageViewCategory category)
	{
		Application.DoEvents();
		Thread.Sleep(50);
		Application.DoEvents();
		lock (appendCallLock)
		{
			SetUpdate(update: false);
			SetText(category, category.Text);
			SetUpdate(update: true);
			textEditorControl.SelectionStart = textEditorControl.TextLength;
			if (LoggingService.IsDebugEnabled)
			{
				LoggingService.Debug("Replaced " + pendingAppendCalls + " appends with one set call");
			}
			pendingAppendCalls = 0;
		}
		textEditorControl.Refresh();
	}

	private void AppendText(MessageViewCategory category, string fullText, string text)
	{
		lock (appendCallLock)
		{
			if (pendingAppendCalls >= 5)
			{
				return;
			}
			pendingAppendCalls--;
		}
		if (messageCategories[SelectedCategoryIndex] != category)
		{
			SelectCategory(category.Category, fullText);
			return;
		}
		if (text != null)
		{
			text = StringParser.Parse(text);
			textEditorControl.AppendText(text);
			textEditorControl.SelectionStart = textEditorControl.TextLength;
		}
		ToolbarService.UpdateToolbar(toolStrip);
	}

	private void SetText(MessageViewCategory category, string text)
	{
		if (messageCategories[SelectedCategoryIndex] != category)
		{
			SelectCategory(category.Category);
			return;
		}
		text = ((text != null) ? StringParser.Parse(text) : string.Empty);
		textEditorControl.Text = text;
		ToolbarService.UpdateToolbar(toolStrip);
	}

	public void SelectCategory(string categoryName)
	{
		for (int i = 0; i < messageCategories.Count; i++)
		{
			MessageViewCategory messageViewCategory = messageCategories[i];
			if (messageViewCategory.Category == categoryName)
			{
				SelectedCategoryIndex = i;
				break;
			}
		}
		if (!base.IsVisible)
		{
			ActivateThisPad();
		}
	}

	private void SelectCategory(string categoryName, string text)
	{
		for (int i = 0; i < messageCategories.Count; i++)
		{
			MessageViewCategory messageViewCategory = messageCategories[i];
			if (messageViewCategory.Category == categoryName)
			{
				selectedCategory = i;
				textEditorControl.Text = StringParser.Parse(text);
				OnSelectedCategoryIndexChanged(EventArgs.Empty);
				break;
			}
		}
	}

	public MessageViewCategory GetCategory(string categoryName)
	{
		foreach (MessageViewCategory messageCategory in messageCategories)
		{
			if (messageCategory.Category == categoryName)
			{
				return messageCategory;
			}
		}
		return null;
	}

	private void ActivateThisPad()
	{
		WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(GetType().FullName);
	}

	private void TextEditorControlDoubleClick(object sender, EventArgs e)
	{
		string text = textEditorControl.Text;
		if (text.Length > 0)
		{
			Point pt = textEditorControl.PointToClient(Control.MousePosition);
			int num = textEditorControl.GetCharIndexFromPosition(pt);
			int num2 = num;
			while (--num2 > 0 && text[num2 - 1] != '\n')
			{
			}
			if (num2 == -1)
			{
				num2 = 0;
			}
			while (++num < text.Length && text[num] != '\n')
			{
			}
			string text2 = text.Substring(num2, num - num2);
			if (!string.IsNullOrEmpty(text2))
			{
				SelectedMessageViewCategory?.JumpToPosition(text2);
			}
		}
	}

	private void PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.Key == "WordWrap")
		{
			SetWordWrap();
			ToolbarService.UpdateToolbar(toolStrip);
		}
		if (e.Key == "DefaultFont")
		{
			FontService.SetFont(FontService.FontType.TextEditor, FontSelectionPanel.ParseFont(properties.Get("DefaultFont", ResourceService.DefaultMonospacedFont.ToString()).ToString()));
			textEditorControl.Font = FontSelectionPanel.ParseFont(properties.Get("DefaultFont", ResourceService.DefaultMonospacedFont.ToString()).ToString());
		}
	}

	protected virtual void OnMessageCategoryAdded(EventArgs e)
	{
		if (this.MessageCategoryAdded != null)
		{
			this.MessageCategoryAdded(this, e);
		}
	}

	protected virtual void OnSelectedCategoryIndexChanged(EventArgs e)
	{
		if (this.SelectedCategoryIndexChanged != null)
		{
			this.SelectedCategoryIndexChanged(this, e);
		}
	}

	public void Cut()
	{
	}

	public void Copy()
	{
		textEditorControl.Copy();
	}

	public void Paste()
	{
	}

	public void Delete()
	{
	}

	public void SelectAll()
	{
		textEditorControl.SelectAll();
		ToolbarService.UpdateToolbar(toolStrip);
	}
}
