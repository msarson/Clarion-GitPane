using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Commands;
using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.TextEditor.Util;
using WeifenLuo.WinFormsUI;

namespace ICSharpCode.SharpDevelop.Gui;

public class SdiWorkspaceWindow : DockContent, IWorkbenchWindow, IOwnerState
{
	[Flags]
	public enum OpenFileTabState
	{
		Nothing = 0,
		FileDirty = 1,
		FileReadOnly = 2,
		FileUntitled = 4
	}

	private static readonly string contextMenuPath = "/SharpDevelop/Workbench/OpenFileTab/ContextMenu";

	private TabControl viewTabControl;

	private int activeViewContentIndex;

	private bool skipTabEvents;

	private IViewContent content;

	public Enum InternalState
	{
		get
		{
			OpenFileTabState openFileTabState = OpenFileTabState.Nothing;
			if (content != null)
			{
				if (content.IsDirty)
				{
					openFileTabState |= OpenFileTabState.FileDirty;
				}
				if (content.IsReadOnly)
				{
					openFileTabState |= OpenFileTabState.FileReadOnly;
				}
				if (content.IsUntitled)
				{
					openFileTabState |= OpenFileTabState.FileUntitled;
				}
			}
			return openFileTabState;
		}
	}

	public string Title
	{
		get
		{
			return Text;
		}
		set
		{
			Text = value;
			OnTitleChanged(EventArgs.Empty);
		}
	}

	public IBaseViewContent ActiveViewContent => GetSubViewContent(activeViewContentIndex);

	public IViewContent ViewContent => content;

	public event EventHandler WindowSelected;

	public event EventHandler WindowDeselected;

	public event EventHandler TitleChanged;

	public event CancelEventHandler ClosingEvent;

	public event EventHandler CloseEvent;

	public event EventHandler SecondaryViewsUpdated;

	public void SwitchView(int viewNumber)
	{
		if (content.SecondaryViewContents.Count == 0 || viewNumber - 1 >= content.SecondaryViewContents.Count)
		{
			return;
		}
		skipTabEvents = true;
		if (viewTabControl != null)
		{
			for (int i = 0; i < viewTabControl.TabPages.Count; i++)
			{
				if ((int)viewTabControl.TabPages[i].Tag == viewNumber)
				{
					if (viewTabControl.SelectedIndex != i)
					{
						viewTabControl.SelectedIndex = i;
					}
					break;
				}
			}
		}
		ViewDeselecting(activeViewContentIndex);
		ViewDeselected(activeViewContentIndex);
		ViewSelected(viewNumber);
		skipTabEvents = false;
	}

	public void SelectWindow()
	{
		Show();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ParserService.LoadSolutionProjectsThreadEnded -= LoadSolutionProjectsThreadEndedEvent;
			ProjectService.ProjectItemAdded -= ProjectService_ProjectItemAdded;
			if (content != null)
			{
				DetachContent();
			}
			if (base.TabPageContextMenu != null)
			{
				base.TabPageContextMenu.Dispose();
				base.TabPageContextMenu = null;
			}
		}
		base.Dispose(disposing);
	}

	public SdiWorkspaceWindow(IViewContent content)
	{
		this.content = content;
		content.WorkbenchWindow = this;
		content.TitleNameChanged += SetTitleEvent;
		content.DirtyChanged += SetTitleEvent;
		base.DockableAreas = DockAreas.Document;
		base.DockPadding.All = 2;
		SetTitleEvent(this, EventArgs.Empty);
		base.TabPageContextMenuStrip = MenuService.CreateContextMenu(this, contextMenuPath);
		InitControls();
		ParserService.LoadSolutionProjectsThreadEnded += LoadSolutionProjectsThreadEndedEvent;
		ProjectService.ProjectItemAdded += ProjectService_ProjectItemAdded;
	}

	private void ProjectService_ProjectItemAdded(object sender, ProjectItemEventArgs e)
	{
		if (content != null)
		{
			object obj = content.FileName ?? content.TitleName;
			if (obj == null)
			{
				obj = content.UntitledName;
			}
			string value = (string)obj;
			if (e.ProjectItem.FileName.Equals(value, StringComparison.InvariantCultureIgnoreCase))
			{
				WorkbenchSingleton.SafeThreadAsyncCall(RefreshSecondaryViewContents);
			}
		}
	}

	private void CreateViewTabControl()
	{
		viewTabControl = new TabControl();
		viewTabControl.GotFocus += delegate
		{
			TabPage selectedTab = viewTabControl.SelectedTab;
			if (selectedTab.Controls.Count == 1 && !selectedTab.ContainsFocus)
			{
				selectedTab.Controls[0].Focus();
			}
		};
		viewTabControl.Alignment = TabAlignment.Bottom;
		viewTabControl.Dock = DockStyle.Fill;
		viewTabControl.Selected += viewTabControlSelected;
		viewTabControl.Deselecting += viewTabControlDeselecting;
		viewTabControl.Deselected += viewTabControlDeselected;
	}

	internal void InitControls()
	{
		if (content.SecondaryViewContents.Count > 0)
		{
			for (int i = 0; i < content.SecondaryViewContents.Count; i++)
			{
				ISecondaryViewContent secondaryViewContent = content.SecondaryViewContents[i];
				if (secondaryViewContent.Visible)
				{
					if (viewTabControl == null)
					{
						CreateViewTabControl();
						AttachSecondaryViewContent(content, 0);
						viewTabControl.SelectedIndex = 0;
					}
					AttachSecondaryViewContent(secondaryViewContent, i + 1);
				}
				else
				{
					secondaryViewContent.WorkbenchWindow = this;
					secondaryViewContent.Control.Dock = DockStyle.Fill;
					secondaryViewContent.Control.Visible = false;
					base.Controls.Add(secondaryViewContent.Control);
				}
			}
			if (viewTabControl != null)
			{
				base.Controls.Add(viewTabControl);
				return;
			}
			content.Control.Dock = DockStyle.Fill;
			base.Controls.Add(content.Control);
		}
		else
		{
			content.Control.Dock = DockStyle.Fill;
			base.Controls.Add(content.Control);
		}
	}

	private void AttachSecondaryViewContent(IBaseViewContent viewContent, int viewIndex)
	{
		viewContent.WorkbenchWindow = this;
		TabPage tabPage = new TabPage(StringParser.Parse(viewContent.TabPageText));
		tabPage.Tag = viewIndex;
		viewContent.Control.Dock = DockStyle.Fill;
		tabPage.Controls.Add(viewContent.Control);
		viewTabControl.TabPages.Add(tabPage);
	}

	private void RefreshSecondaryViewContents()
	{
		if (content == null)
		{
			return;
		}
		int count = content.SecondaryViewContents.Count;
		DisplayBindingService.AttachSubWindows(content, isReattaching: true);
		if (content.SecondaryViewContents.Count <= count)
		{
			return;
		}
		LoggingService.Debug("Attaching new secondary view contents to '" + Title + "'");
		for (int i = count; i < content.SecondaryViewContents.Count; i++)
		{
			ISecondaryViewContent secondaryViewContent = content.SecondaryViewContents[i];
			if (secondaryViewContent.Visible)
			{
				if (viewTabControl == null)
				{
					base.Controls.Remove(content.Control);
					CreateViewTabControl();
					AttachSecondaryViewContent(content, 0);
					viewTabControl.SelectedIndex = 0;
					base.Controls.Add(viewTabControl);
				}
				AttachSecondaryViewContent(secondaryViewContent, i + 1);
			}
			else
			{
				secondaryViewContent.WorkbenchWindow = this;
				secondaryViewContent.Control.Dock = DockStyle.Fill;
				secondaryViewContent.Control.Visible = false;
				base.Controls.Add(secondaryViewContent.Control);
			}
		}
		OnSecondaryViewsUpdated(null);
	}

	private void LoadSolutionProjectsThreadEndedEvent(object sender, EventArgs e)
	{
		WorkbenchSingleton.SafeThreadAsyncCall(RefreshSecondaryViewContents);
	}

	private void SetToolTipText()
	{
		if (content != null)
		{
			try
			{
				if (content.FileName != null && content.FileName.Length > 0)
				{
					base.ToolTipText = Path.GetFullPath(content.FileName);
				}
				else
				{
					base.ToolTipText = null;
				}
				return;
			}
			catch (Exception)
			{
				base.ToolTipText = content.FileName;
				return;
			}
		}
		base.ToolTipText = null;
	}

	public void SetTitleEvent(object sender, EventArgs e)
	{
		if (content == null)
		{
			return;
		}
		SetToolTipText();
		string text = content.TitleName;
		if (content.IsDirty)
		{
			text += "*";
		}
		else if (content.IsReadOnly)
		{
			string text2 = PropertyService.Get("FileReadOnlyText", "+");
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "+";
			}
			text += text2.Trim();
		}
		if (text != Title)
		{
			Title = text;
		}
	}

	public void DetachContent()
	{
		if (viewTabControl != null)
		{
			foreach (TabPage tabPage in viewTabControl.TabPages)
			{
				if (viewTabControl.SelectedTab == tabPage)
				{
					GetSubViewContent((int)tabPage.Tag).Deselecting();
				}
				tabPage.Controls.Clear();
				if (viewTabControl.SelectedTab == tabPage)
				{
					GetSubViewContent((int)tabPage.Tag).Deselected();
				}
			}
			viewTabControl.Dispose();
			viewTabControl = null;
		}
		content.TitleNameChanged -= SetTitleEvent;
		content.DirtyChanged -= SetTitleEvent;
		content = null;
		base.Controls.Clear();
	}

	public bool CloseWindow(bool force)
	{
		if (!force && this.ClosingEvent != null)
		{
			CancelEventArgs e = new CancelEventArgs();
			this.ClosingEvent(this, e);
			if (e.Cancel)
			{
				return false;
			}
		}
		if (!force && ViewContent != null && ViewContent.IsDirty)
		{
			switch (MessageBox.Show(ResourceService.GetString("MainWindow.SaveChangesMessage"), ResourceService.GetString("MainWindow.SaveChangesMessageHeader") + " " + Title + " ?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, RightToLeftConverter.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : ((MessageBoxOptions)0)))
			{
			case DialogResult.Yes:
				if (content.FileName == null)
				{
					do
					{
						new SaveFileAs().Run();
					}
					while (ViewContent.IsDirty && !MessageService.AskQuestion("${res:MainWindow.DiscardChangesMessage}"));
				}
				else
				{
					FileUtility.ObservedSave((FileOperationDelegate)ViewContent.Save, ViewContent.FileName, FileErrorPolicy.ProvideAlternative);
				}
				break;
			case DialogResult.No:
				if (content.FileName != null)
				{
					Encoding defaultFileEncoding = ParserService.DefaultFileEncoding;
					string fileContent = FileReader.ReadFileContent(content.FileName, defaultFileEncoding);
					ParserService.EnqueueForParsing(content.FileName, fileContent);
				}
				break;
			case DialogResult.Cancel:
				return false;
			}
		}
		OnCloseEvent(null);
		Dispose();
		return true;
	}

	private IBaseViewContent GetSubViewContent(int index)
	{
		if (index == 0 || content == null)
		{
			return content;
		}
		return content.SecondaryViewContents[index - 1];
	}

	private void viewTabControlSelected(object sender, TabControlEventArgs e)
	{
		if (!skipTabEvents && e.Action == TabControlAction.Selected && e.TabPageIndex >= 0)
		{
			ViewSelected((int)viewTabControl.TabPages[e.TabPageIndex].Tag);
		}
	}

	private void ViewSelected(int index)
	{
		IBaseViewContent subViewContent = GetSubViewContent(index);
		if (subViewContent != null)
		{
			subViewContent.SwitchedTo();
			subViewContent.Selected();
			IBaseViewContent subViewContent2 = GetSubViewContent(activeViewContentIndex);
			if (subViewContent is ISecondaryViewContent && !((ISecondaryViewContent)subViewContent).Visible)
			{
				Control control = ((subViewContent2 is ISecondaryViewContent && !((ISecondaryViewContent)subViewContent2).Visible) ? subViewContent2.Control : ((viewTabControl == null) ? content.Control : viewTabControl));
				control.Hide();
				subViewContent.Control.Show();
			}
			else if (subViewContent2 is ISecondaryViewContent && !((ISecondaryViewContent)subViewContent2).Visible)
			{
				subViewContent2.Control.Hide();
				if (viewTabControl != null)
				{
					viewTabControl.Show();
				}
				else
				{
					content.Control.Show();
				}
			}
			activeViewContentIndex = index;
		}
		WorkbenchSingleton.Workbench.WorkbenchLayout.OnActiveWorkbenchWindowChanged(EventArgs.Empty);
		Control control2 = ActiveViewContent.Control;
		control2.Focus();
	}

	private void viewTabControlDeselecting(object sender, TabControlCancelEventArgs e)
	{
		if (!skipTabEvents && e.Action == TabControlAction.Deselecting && e.TabPageIndex >= 0)
		{
			ViewDeselecting((int)viewTabControl.TabPages[e.TabPageIndex].Tag);
		}
	}

	private void ViewDeselecting(int index)
	{
		GetSubViewContent(index)?.Deselecting();
	}

	private void viewTabControlDeselected(object sender, TabControlEventArgs e)
	{
		if (!skipTabEvents && e.Action == TabControlAction.Deselected && e.TabPageIndex >= 0)
		{
			ViewDeselected((int)viewTabControl.TabPages[e.TabPageIndex].Tag);
		}
	}

	private void ViewDeselected(int index)
	{
		GetSubViewContent(index)?.Deselected();
	}

	public virtual void RedrawContent()
	{
		if (viewTabControl != null)
		{
			for (int i = 0; i < viewTabControl.TabPages.Count; i++)
			{
				TabPage tabPage = viewTabControl.TabPages[i];
				tabPage.Text = StringParser.Parse(GetSubViewContent((int)tabPage.Tag).TabPageText);
			}
		}
	}

	protected virtual void OnTitleChanged(EventArgs e)
	{
		if (this.TitleChanged != null)
		{
			this.TitleChanged(this, e);
		}
		WorkbenchSingleton.Workbench.WorkbenchLayout.OnActiveWorkbenchWindowChanged(EventArgs.Empty);
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		e.Cancel = !CloseWindow(force: false);
	}

	protected virtual void OnCloseEvent(EventArgs e)
	{
		OnWindowDeselected(e);
		if (this.CloseEvent != null)
		{
			this.CloseEvent(this, e);
		}
	}

	public virtual void OnWindowSelected(EventArgs e)
	{
		if (this.WindowSelected != null)
		{
			this.WindowSelected(this, e);
		}
	}

	public virtual void OnWindowDeselected(EventArgs e)
	{
		if (this.WindowDeselected != null)
		{
			this.WindowDeselected(this, e);
		}
	}

	public virtual void OnSecondaryViewsUpdated(EventArgs e)
	{
		if (this.SecondaryViewsUpdated != null)
		{
			this.SecondaryViewsUpdated(this, e);
		}
	}

	[SpecialName]
	bool IWorkbenchWindow.get_IsDisposed()
	{
		return base.IsDisposed;
	}
}
