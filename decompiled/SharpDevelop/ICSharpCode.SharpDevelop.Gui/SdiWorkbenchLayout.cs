using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DockPanelSkin;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Widgets.AutoHide;
using WeifenLuo.WinFormsUI;

namespace ICSharpCode.SharpDevelop.Gui;

public class SdiWorkbenchLayout : IWorkbenchLayout
{
	private class PadContentWrapper : DockContent
	{
		private PadDescriptor padDescriptor;

		private bool isInitialized;

		internal bool allowInitialize;

		public IPadContent PadContent => padDescriptor.PadContent;

		public PadContentWrapper(PadDescriptor padDescriptor)
		{
			if (padDescriptor == null)
			{
				throw new ArgumentNullException("padDescriptor");
			}
			this.padDescriptor = padDescriptor;
			base.DockableAreas = DockAreas.Float | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom;
			base.HideOnClose = true;
		}

		public void DetachContent()
		{
			base.Controls.Clear();
			padDescriptor = null;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (base.Visible && base.Width > 0)
			{
				ActivateContent();
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (base.Visible && base.Width > 0)
			{
				ActivateContent();
			}
		}

		public void AllowInitialize()
		{
			allowInitialize = true;
			if (base.Visible && base.Width > 0)
			{
				ActivateContent();
			}
		}

		public void ActivateContent()
		{
			ActivateContent(force: false);
		}

		public void ActivateContent(bool force)
		{
			if ((allowInitialize || force) && !isInitialized)
			{
				isInitialized = true;
				IPadContent padContent = padDescriptor.PadContent;
				if (padContent != null)
				{
					Control control = padContent.Control;
					control.Dock = DockStyle.Fill;
					base.Controls.Add(control);
				}
			}
		}

		protected override string GetPersistString()
		{
			return padDescriptor.Class;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && padDescriptor != null)
			{
				padDescriptor.Dispose();
				padDescriptor = null;
			}
		}
	}

	private DefaultWorkbench wbForm;

	private DockPanel dockPanel;

	private Dictionary<string, PadContentWrapper> contentHash = new Dictionary<string, PadContentWrapper>();

	private ToolStripContainer toolStripContainer;

	private AutoHideMenuStripContainer mainMenuContainer;

	private AutoHideStatusStripContainer statusStripContainer;

	private IDockContent lastActiveContent;

	private IWorkbenchWindow oldSelectedWindow;

	public IWorkbenchWindow ActiveWorkbenchwindow
	{
		get
		{
			if (dockPanel == null)
			{
				return null;
			}
			if (!(dockPanel.ActiveDocument is IWorkbenchWindow { IsDisposed: false } workbenchWindow))
			{
				return null;
			}
			return workbenchWindow;
		}
	}

	public object ActiveContent
	{
		get
		{
			IDockContent dockContent = ((dockPanel != null) ? (dockPanel.ActiveContent ?? lastActiveContent) : lastActiveContent);
			if (dockContent != null && dockContent.IsDisposed)
			{
				dockContent = null;
			}
			lastActiveContent = dockContent;
			if (dockContent is IWorkbenchWindow)
			{
				return ((IWorkbenchWindow)dockContent).ActiveViewContent;
			}
			if (dockContent is PadContentWrapper)
			{
				return ((PadContentWrapper)dockContent).PadContent;
			}
			return dockContent;
		}
	}

	public event EventHandler ActiveWorkbenchWindowChanged;

	public void Attach(IWorkbench workbench)
	{
		wbForm = (DefaultWorkbench)workbench;
		wbForm.SuspendLayout();
		wbForm.Controls.Clear();
		toolStripContainer = new ToolStripContainer();
		toolStripContainer.SuspendLayout();
		toolStripContainer.Dock = DockStyle.Fill;
		mainMenuContainer = new AutoHideMenuStripContainer(wbForm.TopMenu);
		mainMenuContainer.Dock = DockStyle.Top;
		statusStripContainer = new AutoHideStatusStripContainer((StatusStrip)StatusBarService.Control);
		statusStripContainer.Dock = DockStyle.Bottom;
		string value = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.DockPanelStyle", Extender.Style.VS2013.ToString());
		if (!Enum.TryParse<Extender.Style>(value, ignoreCase: false, out var result))
		{
			result = Extender.Style.VS2013;
		}
		dockPanel = new DockPanel(result);
		dockPanel.DocumentStyle = DocumentStyles.DockingWindow;
		dockPanel.Dock = DockStyle.Fill;
		Panel panel = new Panel();
		panel.Dock = DockStyle.Fill;
		panel.Controls.Add(dockPanel);
		toolStripContainer.ContentPanel.Controls.Add(panel);
		toolStripContainer.ContentPanel.Controls.Add(mainMenuContainer);
		toolStripContainer.ContentPanel.Controls.Add(statusStripContainer);
		wbForm.Controls.Add(toolStripContainer);
		LoadLayoutConfiguration();
		ShowPads();
		ShowViewContents();
		RedrawAllComponents();
		dockPanel.ActiveDocumentChanged += ActiveMdiChanged;
		dockPanel.ActiveContentChanged += ActiveContentChanged;
		ActiveMdiChanged(this, EventArgs.Empty);
		toolStripContainer.ResumeLayout(performLayout: false);
		wbForm.ResumeLayout(performLayout: false);
		((SdStatusBar)StatusBarService.Control).Init();
		Properties properties = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.FullscreenOptions", new Properties());
		properties.PropertyChanged += TrackFullscreenPropertyChanges;
	}

	private void TrackFullscreenPropertyChanges(object sender, PropertyChangedEventArgs e)
	{
		if (!object.Equals(e.OldValue, e.NewValue) && wbForm.FullScreen)
		{
			switch (e.Key)
			{
			case "HideMainMenu":
			case "ShowMainMenuOnMouseMove":
				RedrawMainMenu();
				break;
			case "HideToolbars":
				RedrawToolbars();
				break;
			case "HideStatusBar":
			case "ShowStatusBarOnMouseMove":
				RedrawStatusBar();
				break;
			}
		}
	}

	private void ShowPads()
	{
		foreach (PadDescriptor item in WorkbenchSingleton.Workbench.PadContentCollection)
		{
			if (!contentHash.ContainsKey(item.Class))
			{
				ShowPad(item);
			}
		}
		foreach (PadContentWrapper value in contentHash.Values)
		{
			value.AllowInitialize();
		}
	}

	private void ShowViewContents()
	{
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			ShowView(item);
		}
	}

	private void LoadLayoutConfiguration()
	{
		try
		{
			LoadDockPanelLayout(LayoutConfiguration.CurrentLayoutFileName);
		}
		catch
		{
			try
			{
				LoadDefaultLayoutConfiguration();
			}
			catch
			{
			}
		}
	}

	private void LoadDefaultLayoutConfiguration()
	{
		if (File.Exists(LayoutConfiguration.CurrentLayoutTemplateFileName))
		{
			LoadDockPanelLayout(LayoutConfiguration.CurrentLayoutTemplateFileName);
		}
	}

	private void LoadDockPanelLayout(string fileName)
	{
		using FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		dockPanel.LoadFromXml(stream, GetContent);
	}

	private void ShowToolBars()
	{
		if (wbForm.ToolBars == null)
		{
			return;
		}
		ArrayList arrayList = new ArrayList();
		foreach (Control control in toolStripContainer.ContentPanel.Controls)
		{
			arrayList.Add(control);
		}
		toolStripContainer.ContentPanel.Controls.Clear();
		toolStripContainer.ContentPanel.Controls.Add(arrayList[0] as Control);
		ToolStrip[] toolBars = wbForm.ToolBars;
		foreach (ToolStrip toolStrip in toolBars)
		{
			if (!toolStripContainer.ContentPanel.Controls.Contains(toolStrip))
			{
				toolStripContainer.ContentPanel.Controls.Add(toolStrip);
			}
		}
		for (int j = 1; j < arrayList.Count; j++)
		{
			toolStripContainer.ContentPanel.Controls.Add(arrayList[j] as Control);
		}
	}

	private void HideToolBars()
	{
		if (wbForm.ToolBars == null)
		{
			return;
		}
		ToolStrip[] toolBars = wbForm.ToolBars;
		foreach (ToolStrip toolStrip in toolBars)
		{
			if (toolStripContainer.ContentPanel.Controls.Contains(toolStrip))
			{
				toolStripContainer.ContentPanel.Controls.Remove(toolStrip);
			}
		}
	}

	private DockContent GetContent(string padTypeName)
	{
		foreach (PadDescriptor item in WorkbenchSingleton.Workbench.PadContentCollection)
		{
			if (item.Class == padTypeName)
			{
				return CreateContent(item);
			}
		}
		return null;
	}

	public void LoadConfiguration()
	{
		if (dockPanel == null)
		{
			return;
		}
		NativeMethods.SetWindowRedraw(wbForm.Handle, allowRedraw: false);
		try
		{
			WorkbenchSingleton.NotifyLayoutChange(on: true);
			IViewContent activeView = GetActiveView();
			dockPanel.ActiveDocumentChanged -= ActiveMdiChanged;
			DetachPadContents(dispose: false);
			DetachViewContents(dispose: false);
			dockPanel.ActiveDocumentChanged += ActiveMdiChanged;
			LoadLayoutConfiguration();
			ShowPads();
			ShowViewContents();
			if (activeView != null && activeView.WorkbenchWindow != null)
			{
				activeView.WorkbenchWindow.SelectWindow();
			}
		}
		finally
		{
			WorkbenchSingleton.NotifyLayoutChange(on: false);
			NativeMethods.SetWindowRedraw(wbForm.Handle, allowRedraw: true);
			wbForm.Refresh();
		}
	}

	public void StoreConfiguration()
	{
		try
		{
			if (dockPanel == null)
			{
				return;
			}
			LayoutConfiguration currentLayout = LayoutConfiguration.CurrentLayout;
			if (currentLayout != null && !currentLayout.ReadOnly)
			{
				string text = Path.Combine(PropertyService.ConfigDirectory, "layouts");
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				dockPanel.SaveAsXml(Path.Combine(text, currentLayout.FileName), Encoding.UTF8);
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	private void DetachPadContents(bool dispose)
	{
		foreach (PadContentWrapper value2 in contentHash.Values)
		{
			value2.allowInitialize = false;
		}
		foreach (PadDescriptor item in wbForm.PadContentCollection)
		{
			try
			{
				PadContentWrapper value = null;
				if (!string.IsNullOrEmpty(item.Class) && !string.IsNullOrEmpty(item.Class) && contentHash.TryGetValue(item.Class, out value) && value != null)
				{
					value.DockPanel = null;
					if (dispose)
					{
						value.DetachContent();
						value.Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
		if (dispose)
		{
			contentHash.Clear();
		}
	}

	private void DetachViewContents(bool dispose)
	{
		foreach (IViewContent item in WorkbenchSingleton.Workbench.ViewContentCollection)
		{
			try
			{
				SdiWorkspaceWindow sdiWorkspaceWindow = (SdiWorkspaceWindow)item.WorkbenchWindow;
				sdiWorkspaceWindow.DockPanel = null;
				if (dispose)
				{
					item.WorkbenchWindow = null;
					sdiWorkspaceWindow.CloseEvent -= CloseWindowEvent;
					sdiWorkspaceWindow.DetachContent();
					sdiWorkspaceWindow.Dispose();
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
			}
		}
	}

	public void Detach()
	{
		StoreConfiguration();
		dockPanel.ActiveDocumentChanged -= ActiveMdiChanged;
		DetachPadContents(dispose: true);
		DetachViewContents(dispose: true);
		try
		{
			if (dockPanel != null)
			{
				dockPanel.Dispose();
				dockPanel = null;
			}
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
		if (contentHash != null)
		{
			contentHash.Clear();
		}
		wbForm.Controls.Clear();
	}

	private PadContentWrapper CreateContent(PadDescriptor content)
	{
		PadContentWrapper value = null;
		if (content != null && !string.IsNullOrEmpty(content.Class) && contentHash.TryGetValue(content.Class, out value) && value != null)
		{
			return value;
		}
		PropertyService.Get("Workspace.ViewMementos", new Properties());
		PadContentWrapper padContentWrapper = new PadContentWrapper(content);
		if (!string.IsNullOrEmpty(content.Icon))
		{
			padContentWrapper.Icon = IconService.GetIcon(content.Icon);
		}
		padContentWrapper.Text = StringParser.Parse(content.Title);
		contentHash[content.Class] = padContentWrapper;
		return padContentWrapper;
	}

	public void ShowPad(PadDescriptor content)
	{
		if (content != null)
		{
			PadContentWrapper value = null;
			if (!string.IsNullOrEmpty(content.Class) && contentHash.TryGetValue(content.Class, out value) && value != null)
			{
				value.Show();
				return;
			}
			DockContent dockContent = CreateContent(content);
			dockContent.Show(dockPanel);
			dockContent.Hide();
		}
	}

	public void ShowAndDockPad(PadDescriptor content)
	{
		if (content == null)
		{
			return;
		}
		PadContentWrapper value = null;
		if (!string.IsNullOrEmpty(content.Class) && contentHash.TryGetValue(content.Class, out value) && value != null)
		{
			DockContent dockContent = value;
			DockState visibleState = dockContent.VisibleState;
			DockState dockState = visibleState;
			switch (visibleState)
			{
			case DockState.DockBottomAutoHide:
				dockState = DockState.DockBottom;
				break;
			case DockState.DockLeftAutoHide:
				dockState = DockState.DockLeft;
				break;
			case DockState.DockRightAutoHide:
				dockState = DockState.DockRight;
				break;
			case DockState.DockTopAutoHide:
				dockState = DockState.DockTop;
				break;
			}
			dockContent.Show(dockPanel, dockState);
		}
		else
		{
			DockContent dockContent2 = CreateContent(content);
			dockContent2.Show(dockPanel);
			dockContent2.Hide();
		}
	}

	public bool IsVisible(PadDescriptor padContent)
	{
		PadContentWrapper value = null;
		if (padContent != null && !string.IsNullOrEmpty(padContent.Class) && contentHash.TryGetValue(padContent.Class, out value) && value != null)
		{
			return !value.IsHidden;
		}
		return false;
	}

	public void HidePad(PadDescriptor padContent)
	{
		PadContentWrapper value = null;
		if (padContent != null && !string.IsNullOrEmpty(padContent.Class) && contentHash.TryGetValue(padContent.Class, out value))
		{
			value?.Hide();
		}
	}

	public void UnloadPad(PadDescriptor padContent)
	{
		PadContentWrapper value = null;
		if (padContent != null && !string.IsNullOrEmpty(padContent.Class) && contentHash.TryGetValue(padContent.Class, out value) && value != null)
		{
			value.Close();
			value.Dispose();
			contentHash.Remove(padContent.Class);
		}
	}

	public void ActivatePadContent(PadDescriptor padContent)
	{
		if (padContent == null)
		{
			return;
		}
		PadContentWrapper value = null;
		if (padContent != null && !string.IsNullOrEmpty(padContent.Class) && contentHash.TryGetValue(padContent.Class, out value) && value != null)
		{
			value.ActivateContent(force: true);
			if (value.IsHidden)
			{
				value.Show();
				value.Hide();
			}
		}
	}

	public void ActivateAndDockPad(string fullyQualifiedTypeName)
	{
		PadContentWrapper value = null;
		if (!string.IsNullOrEmpty(fullyQualifiedTypeName) && contentHash.TryGetValue(fullyQualifiedTypeName, out value) && value != null)
		{
			DockContent dockContent = value;
			DockState visibleState = dockContent.VisibleState;
			DockState dockState = visibleState;
			switch (visibleState)
			{
			case DockState.DockBottomAutoHide:
				dockState = DockState.DockBottom;
				break;
			case DockState.DockLeftAutoHide:
				dockState = DockState.DockLeft;
				break;
			case DockState.DockRightAutoHide:
				dockState = DockState.DockRight;
				break;
			case DockState.DockTopAutoHide:
				dockState = DockState.DockTop;
				break;
			}
			dockContent.Show(dockPanel, dockState);
		}
	}

	public void ActivateAndDockPad(PadDescriptor padContent)
	{
		if (padContent != null)
		{
			ActivateAndDockPad(padContent.Class);
		}
	}

	public void ActivatePad(PadDescriptor padContent)
	{
		if (padContent != null)
		{
			ActivatePad(padContent.Class);
		}
	}

	public void ActivatePad(string fullyQualifiedTypeName)
	{
		PadContentWrapper value = null;
		if (!string.IsNullOrEmpty(fullyQualifiedTypeName) && contentHash.TryGetValue(fullyQualifiedTypeName, out value))
		{
			value?.Show();
		}
	}

	public void RedrawAllComponents()
	{
		foreach (PadDescriptor item in ((IWorkbench)wbForm).PadContentCollection)
		{
			PadContentWrapper value = null;
			if (!string.IsNullOrEmpty(item.Class) && !string.IsNullOrEmpty(item.Class) && contentHash.TryGetValue(item.Class, out value) && value != null)
			{
				value.Text = StringParser.Parse(item.Title);
			}
		}
		RedrawMainMenu();
		RedrawToolbars();
		RedrawStatusBar();
	}

	private void RedrawMainMenu()
	{
		Properties properties = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.FullscreenOptions", new Properties());
		bool flag = properties.Get("HideMainMenu", defaultValue: false);
		bool showOnMouseMove = properties.Get("ShowMainMenuOnMouseMove", defaultValue: true);
		mainMenuContainer.AutoHide = wbForm.FullScreen && flag;
		mainMenuContainer.ShowOnMouseDown = true;
		mainMenuContainer.ShowOnMouseMove = showOnMouseMove;
	}

	private void RedrawToolbars()
	{
		Properties properties = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.FullscreenOptions", new Properties());
		bool flag = properties.Get("HideToolbars", defaultValue: true);
		if (PropertyService.Get("ICSharpCode.SharpDevelop.Gui.ToolBarVisible", defaultValue: true))
		{
			if (wbForm.FullScreen && flag)
			{
				HideToolBars();
			}
			else
			{
				ShowToolBars();
			}
		}
		else
		{
			HideToolBars();
		}
	}

	private void RedrawStatusBar()
	{
		Properties properties = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.FullscreenOptions", new Properties());
		bool flag = properties.Get("HideStatusBar", defaultValue: true);
		bool showOnMouseMove = properties.Get("ShowStatusBarOnMouseMove", defaultValue: true);
		bool visible = PropertyService.Get("ICSharpCode.SharpDevelop.Gui.StatusBarVisible", defaultValue: true);
		statusStripContainer.AutoHide = wbForm.FullScreen && flag;
		statusStripContainer.ShowOnMouseDown = true;
		statusStripContainer.ShowOnMouseMove = showOnMouseMove;
		statusStripContainer.Visible = visible;
	}

	public void CloseWindowEvent(object sender, EventArgs e)
	{
		SdiWorkspaceWindow sdiWorkspaceWindow = (SdiWorkspaceWindow)sender;
		sdiWorkspaceWindow.CloseEvent -= CloseWindowEvent;
		if (sdiWorkspaceWindow.ViewContent != null)
		{
			((IWorkbench)wbForm).CloseContent(sdiWorkspaceWindow.ViewContent);
			if (sdiWorkspaceWindow == oldSelectedWindow)
			{
				oldSelectedWindow = null;
			}
			ActiveMdiChanged(this, null);
		}
	}

	public IWorkbenchWindow CreateWorkbenchWindow(IViewContent content)
	{
		SdiWorkspaceWindow sdiWorkspaceWindow = new SdiWorkspaceWindow(content);
		sdiWorkspaceWindow.CloseEvent += CloseWindowEvent;
		_ = dockPanel;
		return sdiWorkspaceWindow;
	}

	public IWorkbenchWindow ShowView(IViewContent content)
	{
		if (content.WorkbenchWindow is SdiWorkspaceWindow)
		{
			SdiWorkspaceWindow sdiWorkspaceWindow = (SdiWorkspaceWindow)content.WorkbenchWindow;
			if (!sdiWorkspaceWindow.IsDisposed)
			{
				sdiWorkspaceWindow.Show(dockPanel);
				return sdiWorkspaceWindow;
			}
		}
		if (!content.Control.Visible)
		{
			content.Control.Visible = true;
		}
		content.Control.Dock = DockStyle.Fill;
		SdiWorkspaceWindow sdiWorkspaceWindow2 = new SdiWorkspaceWindow(content);
		sdiWorkspaceWindow2.CloseEvent += CloseWindowEvent;
		if (dockPanel != null)
		{
			sdiWorkspaceWindow2.Show(dockPanel);
		}
		return sdiWorkspaceWindow2;
	}

	private void ActiveMdiChanged(object sender, EventArgs e)
	{
		OnActiveWorkbenchWindowChanged(e);
	}

	private void ActiveContentChanged(object sender, EventArgs e)
	{
		OnActiveWorkbenchWindowChanged(e);
	}

	private static IViewContent GetActiveView()
	{
		return WorkbenchSingleton.Workbench.ActiveWorkbenchWindow?.ViewContent;
	}

	public virtual void OnActiveWorkbenchWindowChanged(EventArgs e)
	{
		IWorkbenchWindow activeWorkbenchwindow = ActiveWorkbenchwindow;
		if ((activeWorkbenchwindow == null || activeWorkbenchwindow.ViewContent != null) && this.ActiveWorkbenchWindowChanged != null)
		{
			this.ActiveWorkbenchWindowChanged(this, e);
		}
		if (oldSelectedWindow != null)
		{
			oldSelectedWindow.OnWindowDeselected(EventArgs.Empty);
		}
		oldSelectedWindow = activeWorkbenchwindow;
		if (oldSelectedWindow != null && oldSelectedWindow.ActiveViewContent != null && oldSelectedWindow.ActiveViewContent.Control != null)
		{
			oldSelectedWindow.OnWindowSelected(EventArgs.Empty);
			oldSelectedWindow.ActiveViewContent.SwitchedTo();
			if (!(oldSelectedWindow.ActiveViewContent is ITextEditorControlProvider))
			{
				StatusBarService.ClearCaretancursorText();
			}
		}
	}
}
