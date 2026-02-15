using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using SoftVelocity.ClarionNet.WindowDesigner;

namespace SoftVelocity.Common.ClarionEditor;

[ToolboxItem(false)]
[DesignTimeVisible(false)]
internal class TabOrder : Control
{
	private MenuCommand[] commands;

	private Control ctlHover;

	private string decimalSep;

	private StringBuilder drawString;

	private Pen highlightPen;

	private Brush highlightTextBrush;

	private IDesignerHost host;

	private MenuCommand[] newCommands;

	private Region region;

	private int selSize;

	private ArrayList tabComplete;

	private ArrayList tabControls;

	private Font tabFont;

	private Rectangle[] tabGlyphs;

	private Hashtable tabNext;

	private Hashtable tabProperties;

	private Dictionary<Control, int> controlTabIndexes = new Dictionary<Control, int>();

	private TabOrderBehaviour behaviour;

	private bool isWindow;

	public TabOrderBehaviour Behaviour => behaviour;

	private bool IsWindow => isWindow;

	public TabOrder(IDesignerHost host)
		: this(host, isWindow: false)
	{
	}

	public TabOrder(IDesignerHost host, bool isWindow)
	{
		try
		{
			this.host = host;
			this.isWindow = isWindow;
			IUIService iUIService = (IUIService)host.GetService(typeof(IUIService));
			if (iUIService != null)
			{
				tabFont = (Font)iUIService.Styles["DialogFont"];
			}
			else
			{
				tabFont = Control.DefaultFont;
			}
			tabFont = new Font(tabFont, FontStyle.Bold);
			selSize = 8;
			drawString = new StringBuilder(12);
			highlightTextBrush = new SolidBrush(SystemColors.HighlightText);
			highlightPen = new Pen(SystemColors.Highlight);
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.GetFormat(typeof(NumberFormatInfo));
			if (numberFormatInfo != null)
			{
				decimalSep = numberFormatInfo.NumberDecimalSeparator;
			}
			else
			{
				decimalSep = ".";
			}
			tabProperties = new Hashtable();
			SetStyle(ControlStyles.Opaque, value: true);
			CallPushOverlay();
			((IHelpService)host.GetService(typeof(IHelpService)))?.AddContextAttribute("Keyword", "TabOrderView", HelpKeywordType.FilterKeyword);
			commands = new MenuCommand[8]
			{
				new MenuCommand(OnKeyCancel, MenuCommands.KeyCancel),
				new MenuCommand(OnKeyDefault, MenuCommands.KeyDefaultAction),
				new MenuCommand(OnKeyPrevious, MenuCommands.KeyMoveUp),
				new MenuCommand(OnKeyNext, MenuCommands.KeyMoveDown),
				new MenuCommand(OnKeyPrevious, MenuCommands.KeyMoveLeft),
				new MenuCommand(OnKeyNext, MenuCommands.KeyMoveRight),
				new MenuCommand(OnKeyNext, MenuCommands.KeySelectNext),
				new MenuCommand(OnKeyPrevious, MenuCommands.KeySelectPrevious)
			};
			newCommands = new MenuCommand[1]
			{
				new MenuCommand(OnKeyDefault, MenuCommands.KeyTabOrderSelect)
			};
			IMenuCommandService menuCommandService = (IMenuCommandService)host.GetService(typeof(IMenuCommandService));
			if (menuCommandService != null)
			{
				MenuCommand[] array = newCommands;
				foreach (MenuCommand command in array)
				{
					menuCommandService.AddCommand(command);
				}
			}
			CallPushHandler();
			IComponentChangeService componentChangeService = (IComponentChangeService)host.GetService(typeof(IComponentChangeService));
			if (componentChangeService != null)
			{
				componentChangeService.ComponentAdded += OnComponentAddRemove;
				componentChangeService.ComponentRemoved += OnComponentAddRemove;
				componentChangeService.ComponentChanged += OnComponentChanged;
			}
		}
		finally
		{
			InitBehaviour();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (region != null)
			{
				region.Dispose();
				region = null;
			}
			if (host != null)
			{
				CallRemoveOverlay();
				CallPopHandler();
				IMenuCommandService menuCommandService = (IMenuCommandService)host.GetService(typeof(IMenuCommandService));
				if (menuCommandService != null)
				{
					MenuCommand[] array = newCommands;
					foreach (MenuCommand command in array)
					{
						menuCommandService.RemoveCommand(command);
					}
				}
				IComponentChangeService componentChangeService = (IComponentChangeService)host.GetService(typeof(IComponentChangeService));
				if (componentChangeService != null)
				{
					componentChangeService.ComponentAdded -= OnComponentAddRemove;
					componentChangeService.ComponentRemoved -= OnComponentAddRemove;
					componentChangeService.ComponentChanged -= OnComponentChanged;
				}
				((IHelpService)host.GetService(typeof(IHelpService)))?.RemoveContextAttribute("Keyword", "TabOrderView");
				host = null;
			}
		}
		base.Dispose(disposing);
	}

	private void DrawTabs(IList tabs, Graphics gr, bool fRegion)
	{
		IEnumerator enumerator = tabs.GetEnumerator();
		int num = 0;
		Rectangle empty = Rectangle.Empty;
		Size empty2 = Size.Empty;
		Font font = tabFont;
		if (fRegion)
		{
			region = new Region(new Rectangle(0, 0, 0, 0));
		}
		if (ctlHover != null)
		{
			Rectangle convertedBounds = GetConvertedBounds(ctlHover);
			Rectangle rectangle = convertedBounds;
			rectangle.Inflate(selSize, selSize);
			if (fRegion)
			{
				region = new Region(rectangle);
				region.Exclude(convertedBounds);
			}
			else
			{
				Color backColor = ctlHover.Parent.BackColor;
				Region clip = gr.Clip;
				gr.ExcludeClip(convertedBounds);
				gr.FillRectangle(new SolidBrush(backColor), rectangle);
				ControlPaint.DrawSelectionFrame(gr, active: false, rectangle, convertedBounds, backColor);
				gr.Clip = clip;
			}
		}
		while (enumerator.MoveNext())
		{
			Control control = (Control)enumerator.Current;
			empty = GetConvertedBounds(control);
			drawString.Length = 0;
			Control sitedParent = GetSitedParent(control);
			Control control2 = (Control)host.RootComponent;
			int num2;
			while (sitedParent != control2 && sitedParent != null)
			{
				drawString.Insert(0, decimalSep);
				num2 = controlTabIndexes[sitedParent];
				drawString.Insert(0, num2.ToString(CultureInfo.CurrentCulture));
				sitedParent = GetSitedParent(sitedParent);
			}
			drawString.Insert(0, ' ');
			num2 = controlTabIndexes[control];
			drawString.Append(num2.ToString(CultureInfo.CurrentCulture));
			drawString.Append(' ');
			if (((PropertyDescriptor)tabProperties[control]).IsReadOnly)
			{
				drawString.Append("WindowsFormsTabOrderReadOnly");
				drawString.Append(' ');
			}
			string s = drawString.ToString();
			empty2 = Size.Ceiling(gr.MeasureString(s, font));
			empty.Width = empty2.Width + 2;
			empty.Height = empty2.Height + 2;
			tabGlyphs[num++] = empty;
			if (fRegion)
			{
				region.Union(empty);
				continue;
			}
			Brush highlight;
			Pen highlightText;
			Color color;
			if (tabComplete.IndexOf(control) != -1)
			{
				highlight = highlightTextBrush;
				highlightText = highlightPen;
				color = SystemColors.Highlight;
			}
			else
			{
				highlight = SystemBrushes.Highlight;
				highlightText = SystemPens.HighlightText;
				color = SystemColors.HighlightText;
			}
			gr.FillRectangle(highlight, empty);
			gr.DrawRectangle(highlightText, empty.X, empty.Y, empty.Width - 1, empty.Height - 1);
			using Brush brush = new SolidBrush(color);
			gr.DrawString(s, font, brush, empty.X + 1, empty.Y + 1);
		}
		if (fRegion)
		{
			Control control = (Control)host.RootComponent;
			empty = GetConvertedBounds(control);
			region.Intersect(empty);
			base.Region = region;
		}
	}

	private Control GetControlAtPoint(IList tabs, int x, int y)
	{
		IEnumerator enumerator = tabs.GetEnumerator();
		Control result = null;
		while (enumerator.MoveNext())
		{
			Control control = (Control)enumerator.Current;
			Control sitedParent = GetSitedParent(control);
			Rectangle bounds = control.Bounds;
			if (sitedParent.RectangleToScreen(bounds).Contains(x, y))
			{
				result = control;
			}
		}
		return result;
	}

	private Rectangle GetConvertedBounds(Control ctl)
	{
		Control control = ctl.Parent;
		Rectangle bounds = ctl.Bounds;
		bounds = control.RectangleToScreen(bounds);
		return RectangleToClient(bounds);
	}

	private int GetMaxControlCount(Control ctl)
	{
		int num = 0;
		Control[] children = GetChildren(ctl);
		for (int i = 0; i < children.Length; i++)
		{
			if (GetTabbable(children[i]))
			{
				num++;
			}
		}
		return num;
	}

	private Control GetSitedParent(Control child)
	{
		Control control;
		for (control = GetParent(child); control != null; control = GetParent(control))
		{
			ISite site = control.Site;
			IContainer container = null;
			if (site != null)
			{
				container = site.Container;
			}
			if (site != null && container == host)
			{
				return control;
			}
		}
		return control;
	}

	private bool GetTabbable(Control control)
	{
		for (Control control2 = control; control2 != null; control2 = GetParent(control2))
		{
			if (!control2.Visible)
			{
				return false;
			}
		}
		ISite site = control.Site;
		if (site == null || site.Container != host)
		{
			return false;
		}
		PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(control)["TabIndex"];
		if (propertyDescriptor == null || !propertyDescriptor.IsBrowsable)
		{
			return false;
		}
		tabProperties[control] = propertyDescriptor;
		return true;
	}

	private void GetTabbing(Control ctl, IList tabs)
	{
		Control[] children = GetChildren(ctl);
		ControlTabOrderComparer comparer = new ControlTabOrderComparer();
		Array.Sort(children, comparer);
		for (int num = children.Length - 1; num >= 0; num--)
		{
			Control control = children[num];
			controlTabIndexes.Add(control, num);
			if (GetSitedParent(control) != null && GetTabbable(control))
			{
				tabs.Add(control);
			}
			if (GetChildren(control).Length > 0)
			{
				GetTabbing(control, tabs);
			}
		}
	}

	private void OnComponentAddRemove(object sender, ComponentEventArgs ce)
	{
		ctlHover = null;
		tabControls = null;
		tabGlyphs = null;
		if (tabComplete != null)
		{
			tabComplete.Clear();
		}
		if (tabNext != null)
		{
			tabNext.Clear();
		}
		if (region != null)
		{
			region.Dispose();
			region = null;
		}
		Invalidate();
	}

	private void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
	{
		tabControls = null;
		tabGlyphs = null;
		if (region != null)
		{
			region.Dispose();
			region = null;
		}
		Invalidate();
	}

	private void OnKeyCancel(object sender, EventArgs e)
	{
		((IMenuCommandService)host.GetService(typeof(IMenuCommandService)))?.FindCommand(StandardCommands.TabOrder)?.Invoke();
	}

	private void OnKeyDefault(object sender, EventArgs e)
	{
		if (ctlHover != null)
		{
			SetNextTabIndex(ctlHover);
			RotateControls(forward: true);
		}
	}

	private void OnKeyNext(object sender, EventArgs e)
	{
		RotateControls(forward: true);
	}

	private void OnKeyPrevious(object sender, EventArgs e)
	{
		RotateControls(forward: false);
	}

	public virtual void OnMouseDoubleClick(IComponent component)
	{
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (ctlHover != null)
		{
			SetNextTabIndex(ctlHover);
		}
	}

	public virtual void OnMouseDown(IComponent component, MouseButtons button, int x, int y)
	{
		if (ctlHover != null)
		{
			SetNextTabIndex(ctlHover);
		}
	}

	public virtual void OnMouseHover(IComponent component)
	{
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (tabGlyphs != null)
		{
			Control newHover = null;
			for (int i = 0; i < tabGlyphs.Length; i++)
			{
				if (tabGlyphs[i].Contains(e.X, e.Y))
				{
					newHover = (Control)tabControls[i];
				}
			}
			SetNewHover(newHover);
		}
		SetAppropriateCursor();
	}

	public virtual void OnMouseMove(IComponent component, int x, int y)
	{
		if (tabControls != null)
		{
			Control controlAtPoint = GetControlAtPoint(tabControls, x, y);
			SetNewHover(controlAtPoint);
		}
	}

	public virtual void OnMouseUp(IComponent component, MouseButtons button)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		tabControls = new ArrayList();
		controlTabIndexes = new Dictionary<Control, int>();
		GetTabbing((Control)host.RootComponent, tabControls);
		tabGlyphs = new Rectangle[tabControls.Count];
		if (tabComplete == null)
		{
			tabComplete = new ArrayList();
		}
		if (tabNext == null)
		{
			tabNext = new Hashtable();
		}
		if (region == null)
		{
			DrawTabs(tabControls, e.Graphics, fRegion: true);
		}
		DrawTabs(tabControls, e.Graphics, fRegion: false);
	}

	public virtual void OnSetCursor(IComponent component)
	{
		SetAppropriateCursor();
	}

	public bool OverrideInvoke(MenuCommand cmd)
	{
		for (int i = 0; i < commands.Length; i++)
		{
			if (commands[i].CommandID.Equals(cmd.CommandID))
			{
				commands[i].Invoke();
				return true;
			}
		}
		return false;
	}

	public bool OverrideStatus(MenuCommand cmd)
	{
		for (int i = 0; i < commands.Length; i++)
		{
			if (commands[i].CommandID.Equals(cmd.CommandID))
			{
				cmd.Enabled = commands[i].Enabled;
				return true;
			}
		}
		if (!cmd.CommandID.Equals(StandardCommands.TabOrder))
		{
			cmd.Enabled = false;
			return true;
		}
		return false;
	}

	private void RotateControls(bool forward)
	{
		Control control = ctlHover;
		Control control2 = (Control)host.RootComponent;
		if (control == null)
		{
			control = control2;
		}
		while ((control = control2.GetNextControl(control, forward)) != null && !GetTabbable(control))
		{
		}
		SetNewHover(control);
	}

	private void SetAppropriateCursor()
	{
		if (ctlHover != null)
		{
			Cursor.Current = Cursors.Cross;
		}
		else
		{
			Cursor.Current = Cursors.Default;
		}
	}

	private void SetNewHover(Control ctl)
	{
		if (ctlHover == ctl)
		{
			return;
		}
		if (ctlHover != null)
		{
			if (region != null)
			{
				region.Dispose();
				region = null;
			}
			GetConvertedBounds(ctlHover).Inflate(selSize, selSize);
			Invalidate();
		}
		ctlHover = ctl;
		if (ctlHover != null)
		{
			if (region != null)
			{
				region.Dispose();
				region = null;
			}
			GetConvertedBounds(ctlHover).Inflate(selSize, selSize);
			Invalidate();
		}
	}

	private void SetNextTabIndex(Control control)
	{
		if (tabControls == null)
		{
			return;
		}
		Control sitedParent = GetSitedParent(control);
		object obj = tabNext[sitedParent];
		if (tabComplete.IndexOf(control) == -1)
		{
			tabComplete.Add(control);
		}
		int num = ((obj != null) ? ((int)obj) : 0);
		try
		{
			PropertyDescriptor propertyDescriptor = (PropertyDescriptor)tabProperties[control];
			if (propertyDescriptor != null)
			{
				int num2 = num + 1;
				if (propertyDescriptor.IsReadOnly)
				{
					num2 = (int)propertyDescriptor.GetValue(control) + 1;
				}
				int maxControlCount = GetMaxControlCount(sitedParent);
				if (num2 >= maxControlCount)
				{
					num2 = 0;
				}
				tabNext[sitedParent] = num2;
				if (tabComplete.Count == tabControls.Count)
				{
					tabComplete.Clear();
				}
				if (!propertyDescriptor.IsReadOnly)
				{
					try
					{
						Behaviour.SetTabIndex(control, propertyDescriptor, num);
					}
					catch (Exception)
					{
					}
				}
				else
				{
					Invalidate();
				}
			}
		}
		catch (Exception ex2)
		{
			if (IsCriticalException(ex2))
			{
				throw;
			}
		}
		Invalidate();
	}

	private static bool IsCriticalException(Exception ex)
	{
		if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is IndexOutOfRangeException))
		{
			return ex is AccessViolationException;
		}
		return true;
	}

	private void GetServiceAndCallMethod(string assembly, string type, string methodName, object[] parameters)
	{
		Type type2 = TypeFinder.FindType(assembly, type);
		object service = host.GetService(type2);
		MethodInfo method = type2.GetMethod(methodName);
		if (method != null && service != null)
		{
			method.Invoke(service, parameters);
		}
	}

	private void CallPushOverlay()
	{
		GetServiceAndCallMethod("System.Design", "System.Windows.Forms.Design.IOverlayService", "PushOverlay", new object[1] { this });
	}

	private void CallRemoveOverlay()
	{
		GetServiceAndCallMethod("System.Design", "System.Windows.Forms.Design.IOverlayService", "RemoveOverlay", new object[1] { this });
	}

	private void CallPushHandler()
	{
		GetServiceAndCallMethod("System.Design", "System.Windows.Forms.Design.IEventHandlerService", "PushHandler", new object[1] { this });
	}

	private void CallPopHandler()
	{
		GetServiceAndCallMethod("System.Design", "System.Windows.Forms.Design.IEventHandlerService", "PopHandler", new object[1] { this });
	}

	private void InitBehaviour()
	{
		if (host != null)
		{
			IComponent rootComponent = host.RootComponent;
			if (IsWindow && rootComponent is GeneralDesiner)
			{
				behaviour = new WindowTabOrderBehaviour(rootComponent as GeneralDesiner);
			}
		}
		if (behaviour == null)
		{
			behaviour = new CommonTabOrderBehaviour();
		}
	}

	private Control GetParent(Control control)
	{
		return behaviour.GetControlParent(control);
	}

	private Control[] GetChildren(Control control)
	{
		try
		{
			return behaviour.GetControlChildren(control);
		}
		catch
		{
			return new Control[0];
		}
	}
}
