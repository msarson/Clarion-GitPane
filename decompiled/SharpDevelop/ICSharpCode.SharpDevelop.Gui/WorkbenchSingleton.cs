using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Bookmarks;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public static class WorkbenchSingleton
{
	private class STAThreadCaller
	{
		private Control ctl;

		public STAThreadCaller(Control ctl)
		{
			this.ctl = ctl;
		}

		public object Call(Delegate method)
		{
			if ((object)method == null)
			{
				throw new ArgumentNullException("method");
			}
			return ctl.Invoke(method);
		}

		public object Call(Delegate method, params object[] arguments)
		{
			if ((object)method == null)
			{
				throw new ArgumentNullException("method");
			}
			return ctl.Invoke(method, arguments);
		}

		public void BeginCall(Delegate method)
		{
			if ((object)method == null)
			{
				throw new ArgumentNullException("method");
			}
			ctl.BeginInvoke(method);
		}

		public void BeginCall(Delegate method, params object[] arguments)
		{
			if ((object)method == null)
			{
				throw new ArgumentNullException("method");
			}
			ctl.BeginInvoke(method, arguments);
		}
	}

	private struct MessageParameters
	{
		public string message;

		public string caption;

		public MessageBoxButtons buttons;

		public MessageBoxIcon icon;
	}

	private const string uiIconStyle = "IconMenuItem.IconMenuStyle";

	private const string uiLanguageProperty = "CoreProperties.UILanguage";

	private const string workbenchMemento = "WorkbenchMemento";

	public static Form helpHost;

	private static STAThreadCaller caller;

	private static bool workbenchClosed;

	private static DefaultWorkbench workbench = null;

	private static readonly object[] emptyObjectArray = new object[0];

	public static Form MainForm
	{
		get
		{
			if (workbench != null)
			{
				return workbench.MainForm;
			}
			return null;
		}
	}

	public static IWorkbench Workbench => workbench;

	public static Control ActiveControl
	{
		get
		{
			ContainerControl containerControl = MainForm;
			Control activeControl;
			do
			{
				activeControl = containerControl.ActiveControl;
				if (activeControl == null)
				{
					return containerControl;
				}
				containerControl = activeControl as ContainerControl;
			}
			while (containerControl != null);
			return activeControl;
		}
	}

	public static bool InvokeRequired
	{
		get
		{
			if (workbench == null)
			{
				return false;
			}
			return workbench.MainForm.InvokeRequired;
		}
	}

	public static bool SupportMultipleInstances
	{
		get
		{
			return PropertyService.Get("ICSharpCode.SharpDevelop.Gui.SupportMultipleInstances", defaultValue: true);
		}
		set
		{
			PropertyService.Set("ICSharpCode.SharpDevelop.Gui.SupportMultipleInstances", value);
		}
	}

	public static event EventHandler WorkbenchCreated;

	public static event EventHandler<BoolEventArg> LayoutChanging;

	private static void TrackPropertyChanges(object sender, PropertyChangedEventArgs e)
	{
		if (e.OldValue != e.NewValue && workbench != null)
		{
			switch (e.Key)
			{
			case "ICSharpCode.SharpDevelop.Gui.StatusBarVisible":
			case "ICSharpCode.SharpDevelop.Gui.VisualStyle":
			case "ICSharpCode.SharpDevelop.Gui.ToolBarVisible":
			case "ICSharpCode.SharpDevelop.Gui.UseSmallIconsInToolbar":
				workbench.RedrawAllComponents();
				break;
			case "ICSharpCode.SharpDevelop.Gui.UseProfessionalRenderer":
				workbench.UpdateRenderer();
				break;
			case "ICSharpCode.SharpDevelop.Gui.ProfessionalRendererColorTableStyles":
				workbench.UpdateRenderer();
				break;
			}
		}
	}

	public static void InitializeWorkbench()
	{
		LayoutConfiguration.LoadLayoutConfiguration();
		StatusBarService.Initialize();
		DomHostCallback.Register();
		ParserService.InitializeParserService();
		BookmarkManager.Initialize();
		CustomToolsService.Initialize();
		workbench = new DefaultWorkbench();
		MessageService.MainForm = workbench;
		PropertyService.PropertyChanged += TrackPropertyChanges;
		ResourceService.LanguageChanged += delegate
		{
			workbench.RedrawAllComponents();
		};
		caller = new STAThreadCaller(workbench);
		workbench.InitializeWorkspace();
		workbench.Closed += Workbench_Closed;
		Properties properties = new Properties();
		properties.Set("bounds", "0,0,800,600");
		properties.Set("windowstate", "Normal");
		properties.Set("defaultstate", "Normal");
		workbench.SetMemento(PropertyService.Get("WorkbenchMemento", properties));
		workbench.WorkbenchLayout = new SdiWorkbenchLayout();
		OnWorkbenchCreated();
		helpHost = new Form();
		helpHost.CreateControl();
	}

	private static void Workbench_Closed(object sender, EventArgs e)
	{
		workbenchClosed = true;
		caller = null;
	}

	public static void AssertMainThread()
	{
		if (InvokeRequired)
		{
			throw new InvalidOperationException("This operation can be called on the main thread only.");
		}
	}

	public static R SafeThreadFunction<R>(Func<R> method)
	{
		if (caller == null)
		{
			return method();
		}
		return (R)caller.Call(method);
	}

	public static R SafeThreadFunction<A, R>(Func<A, R> method, A arg1)
	{
		if (caller == null)
		{
			return method(arg1);
		}
		return (R)caller.Call(method, arg1);
	}

	public static R SafeThreadFunction<A, B, R>(Func<A, B, R> method, A arg1, B arg2)
	{
		if (caller == null)
		{
			return method(arg1, arg2);
		}
		return (R)caller.Call(method, arg1, arg2);
	}

	public static R SafeThreadFunction<A, B, C, R>(Func<A, B, C, R> method, A arg1, B arg2, C arg3)
	{
		if (caller == null)
		{
			return method(arg1, arg2, arg3);
		}
		return (R)caller.Call(method, arg1, arg2, arg3);
	}

	public static void SafeThreadCall(Action method)
	{
		if (caller == null)
		{
			method();
		}
		else
		{
			caller.Call(method);
		}
	}

	public static void SafeThreadCall<A>(Action<A> method, A arg1)
	{
		if (caller == null)
		{
			method(arg1);
			return;
		}
		caller.Call(method, arg1);
	}

	public static void SafeThreadCall<A, B>(Action<A, B> method, A arg1, B arg2)
	{
		if (caller == null)
		{
			method(arg1, arg2);
			return;
		}
		caller.Call(method, arg1, arg2);
	}

	public static void SafeThreadCall<A, B, C>(Action<A, B, C> method, A arg1, B arg2, C arg3)
	{
		if (caller == null)
		{
			method(arg1, arg2, arg3);
			return;
		}
		caller.Call(method, arg1, arg2, arg3);
	}

	public static void SafeThreadCall<A, B, C, D>(Action<A, B, C, D> method, A arg1, B arg2, C arg3, D arg4)
	{
		if (caller == null)
		{
			method(arg1, arg2, arg3, arg4);
			return;
		}
		caller.Call(method, arg1, arg2, arg3, arg4);
	}

	public static void SafeThreadAsyncCall(Action method)
	{
		if (caller == null)
		{
			method();
		}
		else
		{
			caller.BeginCall(method);
		}
	}

	public static void SafeThreadAsyncCall<A>(Action<A> method, A arg1)
	{
		if (caller == null)
		{
			method(arg1);
			return;
		}
		caller.BeginCall(method, arg1);
	}

	public static void SafeThreadAsyncCall<A, B>(Action<A, B> method, A arg1, B arg2)
	{
		if (caller == null)
		{
			method(arg1, arg2);
			return;
		}
		caller.BeginCall(method, arg1, arg2);
	}

	public static void SafeThreadAsyncCall<A, B, C>(Action<A, B, C> method, A arg1, B arg2, C arg3)
	{
		if (caller == null)
		{
			method(arg1, arg2, arg3);
			return;
		}
		caller.BeginCall(method, arg1, arg2, arg3);
	}

	public static void SafeThreadAsyncCall<A, B, C, D>(Action<A, B, C, D> method, A arg1, B arg2, C arg3, D arg4)
	{
		if (caller == null)
		{
			method(arg1, arg2, arg3, arg4);
			return;
		}
		caller.BeginCall(method, arg1, arg2, arg3, arg4);
	}

	public static void SafeThreadAsyncCall<A, B, C, D, E>(Action<A, B, C, D, E> method, A arg1, B arg2, C arg3, D arg4, E arg5)
	{
		if (caller == null)
		{
			method(arg1, arg2, arg3, arg4, arg5);
			return;
		}
		caller.BeginCall(method, arg1, arg2, arg3, arg4, arg5);
	}

	public static void CallLater(int delayMilliseconds, Action method)
	{
		if (delayMilliseconds <= 0)
		{
			throw new ArgumentOutOfRangeException("delayMilliseconds", delayMilliseconds, "Value must be positive");
		}
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		SafeThreadAsyncCall(delegate
		{
			Timer t = new Timer();
			t.Interval = delayMilliseconds;
			t.Tick += delegate
			{
				t.Stop();
				t.Dispose();
				method();
			};
			t.Start();
		});
	}

	public static void ThreadSafeShowMessage(string message, string caption)
	{
		SafeThreadCall(MessageService.ShowMessage, message, caption);
	}

	private static DialogResult MessageHandler(MessageParameters parameters)
	{
		return MessageBox.Show(workbenchClosed ? null : MainForm, parameters.message, parameters.caption, parameters.buttons, parameters.icon);
	}

	public static DialogResult ThreadSafeShowMessage(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		MessageParameters arg = default(MessageParameters);
		arg.message = message;
		arg.caption = caption;
		arg.buttons = buttons;
		arg.icon = icon;
		return SafeThreadFunction(MessageHandler, arg);
	}

	private static void OnWorkbenchCreated()
	{
		if (WorkbenchSingleton.WorkbenchCreated != null)
		{
			WorkbenchSingleton.WorkbenchCreated(null, EventArgs.Empty);
		}
	}

	public static void NotifyLayoutChange(bool on)
	{
		if (WorkbenchSingleton.LayoutChanging != null)
		{
			WorkbenchSingleton.LayoutChanging(null, new BoolEventArg(on));
		}
	}

	public static void DoEvents()
	{
		if (InvokeRequired)
		{
			SafeThreadCall(DoEvents);
		}
		else
		{
			Application.DoEvents();
		}
	}

	public static bool DoWin32Event(ref Message msg)
	{
		if (workbench == null)
		{
			return false;
		}
		return workbench.DoWin32Event(ref msg);
	}
}
