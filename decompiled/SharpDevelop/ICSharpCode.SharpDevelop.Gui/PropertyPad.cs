using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;
using VisualHint.SmartPropertyGrid;

namespace ICSharpCode.SharpDevelop.Gui;

public class PropertyPad : AbstractPadContent, IContextHelpProvider
{
	private static PropertyPad instance;

	private PropertyContainer activeContainer;

	private Panel panel;

	private ComboBox comboBox;

	private PropertyGridSVBase grid;

	private IDesignerHost host;

	private bool inUpdate;

	public static PropertyPad Instance => instance;

	public static PropertyGridSVBase Grid
	{
		get
		{
			if (instance == null)
			{
				return null;
			}
			return instance.grid;
		}
	}

	public override Control Control => panel;

	public override bool WantsEscape => grid.WantsEscape;

	public static event VisualHint.SmartPropertyGrid.PropertyGrid.PropertyChangedEventHandler PropertyValueChanged;

	public static event EventHandler SelectedObjectChanged;

	public static event VisualHint.SmartPropertyGrid.PropertyGrid.PropertySelectedEventHandler SelectedGridItemChanged;

	public static event DrawPadItemEventHandler DrawItem;

	public static event MeasurePadItemEventHandler MeasureItem;

	private void SetActiveContainer(PropertyContainer pc)
	{
		if (activeContainer == pc)
		{
			return;
		}
		if (pc == null)
		{
			activeContainer = null;
			SetSelectableObjects(new object[0]);
			grid.SelectedObjects = null;
			return;
		}
		bool flag = false;
		if (activeContainer != pc)
		{
			flag = true;
		}
		activeContainer = pc;
		UpdateHostIfActive(pc);
		UpdateSelectedObjectIfActive(pc);
		UpdateSelectableIfActive(pc);
		if (flag)
		{
			comboBox.Refresh();
			comboBox.Invalidate();
			if (comboBox.Items.Count > 0)
			{
				comboBox.SelectedIndex = 0;
			}
		}
	}

	internal static void UpdateSelectedObjectIfActive(PropertyContainer container)
	{
		if (instance != null && instance.activeContainer == container)
		{
			LoggingService.Debug("UpdateSelectedObjectIfActive");
			if (container.SelectedObjects != null)
			{
				instance.SetDesignableObjects(container.SelectedObjects);
			}
			else
			{
				instance.SetDesignableObject(container.SelectedObject);
			}
		}
	}

	internal static void UpdateHostIfActive(PropertyContainer container)
	{
		if (instance == null || instance.activeContainer != container)
		{
			return;
		}
		LoggingService.Debug("UpdateHostIfActive");
		if (instance.host != container.Host)
		{
			if (instance.host != null)
			{
				instance.RemoveHost(instance.host);
			}
			if (container.Host != null)
			{
				instance.SetDesignerHost(container.Host);
			}
		}
	}

	internal static void UpdateSelectableIfActive(PropertyContainer container)
	{
		if (instance != null && instance.activeContainer == container)
		{
			LoggingService.Debug("UpdateSelectableIfActive");
			instance.SetSelectableObjects(container.SelectableObjects);
		}
	}

	private void WorkbenchWindowChanged(object sender, EventArgs e)
	{
		if (WorkbenchSingleton.Workbench.ActiveContent is IHasPropertyContainer hasPropertyContainer)
		{
			SetActiveContainer(hasPropertyContainer.PropertyContainer);
		}
	}

	public PropertyPad()
	{
		instance = this;
		panel = new Panel();
		grid = new PropertyGridSVBase();
		grid.Name = "PropertyPadGrid";
		grid.AutoResizeLabelColumn = false;
		grid.AutoPreserveLabelSize = true;
		grid.LocatorVisibility = true;
		grid.DisplayMode = (PropertyService.Get("FormsDesigner.DesignerOptions.PropertyGridSortAlphabetical", defaultValue: false) ? VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.FlatSorted : VisualHint.SmartPropertyGrid.PropertyGrid.DisplayModes.Categorized);
		grid.Dock = DockStyle.Fill;
		grid.SelectedObjectChanged += delegate(object sender, SelectedObjectChangedEventArgs e)
		{
			if (PropertyPad.SelectedObjectChanged != null)
			{
				PropertyPad.SelectedObjectChanged(sender, e);
			}
		};
		grid.PropertySelected += delegate(object sender, PropertySelectedEventArgs e)
		{
			if (PropertyPad.SelectedGridItemChanged != null)
			{
				PropertyPad.SelectedGridItemChanged(sender, e);
			}
		};
		comboBox = new ComboBox();
		comboBox.Font = FontService.GetFont(FontService.FontType.ListControls);
		comboBox.Dock = DockStyle.Top;
		comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
		comboBox.DrawMode = DrawMode.OwnerDrawFixed;
		comboBox.Sorted = true;
		comboBox.DrawItem += ComboBoxDrawItem;
		comboBox.MeasureItem += ComboBoxMeasureItem;
		comboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
		panel.Controls.Add(grid);
		panel.Controls.Add(comboBox);
		ProjectService.SolutionClosed += CombineClosedEvent;
		grid.PropertyChanged += PropertyChanged;
		grid.ContextMenuStrip = MenuService.CreateContextMenu(this, "/SharpDevelop/Views/PropertyPad/ContextMenu");
		LoggingService.Debug("PropertyPad created");
		WorkbenchSingleton.Workbench.ActiveWorkbenchWindowChanged += WorkbenchWindowChanged;
		WorkbenchWindowChanged(null, null);
		grid.RestoreLabelSize();
	}

	private void CombineClosedEvent(object sender, EventArgs e)
	{
		SetDesignableObjects(null);
		grid.StoreLabelSize();
	}

	private void ComboBoxMeasureItem(object sender, MeasureItemEventArgs mea)
	{
		if (mea.Index < 0 || mea.Index >= comboBox.Items.Count)
		{
			mea.ItemHeight = comboBox.Font.Height;
			return;
		}
		object obj = comboBox.Items[mea.Index];
		bool handled = false;
		if (PropertyPad.MeasureItem != null)
		{
			PropertyPad.MeasureItem(sender, mea, obj, ref handled);
		}
		if (handled)
		{
			return;
		}
		string text;
		string text2;
		if (obj == null)
		{
			text = string.Empty;
			text2 = string.Empty;
		}
		else
		{
			text = TypeDescriptor.GetClassName(obj, noCustomTypeDesc: false);
			text2 = TypeDescriptor.GetFullComponentName(obj);
		}
		if (obj is INamedComponent)
		{
			text = ((INamedComponent)obj).TypeName();
		}
		Size size = TextRenderer.MeasureText(mea.Graphics, text, comboBox.Font);
		mea.ItemHeight = size.Height;
		mea.ItemWidth = size.Width;
		if (string.IsNullOrEmpty(text2))
		{
			return;
		}
		using Font font = new Font(comboBox.Font, FontStyle.Bold);
		mea.ItemWidth += TextRenderer.MeasureText(mea.Graphics, text2 + "-", font).Width;
	}

	private void ComboBoxDrawItem(object sender, DrawItemEventArgs dea)
	{
		if (dea.Index < 0 || dea.Index >= comboBox.Items.Count)
		{
			return;
		}
		object obj = comboBox.Items[dea.Index];
		int num = dea.Bounds.X;
		bool handled = false;
		if (PropertyPad.DrawItem != null)
		{
			PropertyPad.DrawItem(sender, dea, obj, ref handled);
		}
		if (handled)
		{
			return;
		}
		Graphics graphics = dea.Graphics;
		Color foreColor = SystemColors.ControlText;
		if ((dea.State & DrawItemState.Selected) == DrawItemState.Selected)
		{
			if ((dea.State & DrawItemState.Focus) == DrawItemState.Focus)
			{
				graphics.FillRectangle(SystemBrushes.Highlight, dea.Bounds);
				foreColor = SystemColors.HighlightText;
			}
			else
			{
				graphics.FillRectangle(SystemBrushes.Window, dea.Bounds);
			}
		}
		else
		{
			graphics.FillRectangle(SystemBrushes.Window, dea.Bounds);
		}
		string text;
		string text2;
		if (obj == null)
		{
			text = string.Empty;
			text2 = string.Empty;
		}
		else
		{
			text = TypeDescriptor.GetFullComponentName(obj);
			text2 = TypeDescriptor.GetClassName(obj, noCustomTypeDesc: false);
		}
		if (!string.IsNullOrEmpty(text))
		{
			using Font font = new Font(comboBox.Font, FontStyle.Bold);
			TextRenderer.DrawText(graphics, text, font, new Point(num, dea.Bounds.Y), foreColor);
			num += TextRenderer.MeasureText(graphics, text + "-", font).Width;
		}
		if (obj is INamedComponent)
		{
			text2 = ((INamedComponent)obj).TypeName();
		}
		TextRenderer.DrawText(graphics, text2, comboBox.Font, new Point(num, dea.Bounds.Y), foreColor);
	}

	private void ComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (!inUpdate && host != null)
		{
			ISelectionService selectionService = (ISelectionService)host.GetService(typeof(ISelectionService));
			if (comboBox.SelectedIndex >= 0)
			{
				selectionService.SetSelectedComponents(new object[1] { comboBox.Items[comboBox.SelectedIndex] });
			}
			else
			{
				SetDesignableObject(null);
				selectionService.SetSelectedComponents(new object[0]);
			}
		}
	}

	private void SelectedObjectsChanged()
	{
		object[] selectedObjects = grid.SelectedObjects;
		if (selectedObjects != null && selectedObjects.Length == 1)
		{
			bool flag = false;
			for (int i = 0; i < comboBox.Items.Count; i++)
			{
				if (object.Equals(grid.SelectedObject, comboBox.Items[i]))
				{
					flag = true;
					comboBox.SelectedIndex = i;
				}
			}
			if (!flag)
			{
				comboBox.Refresh();
				comboBox.Invalidate();
			}
		}
		else
		{
			comboBox.SelectedIndex = -1;
		}
	}

	public override void RedrawContent()
	{
		grid.Refresh();
	}

	public override void Dispose()
	{
		base.Dispose();
		if (grid != null)
		{
			ProjectService.SolutionClosed -= CombineClosedEvent;
			try
			{
				grid.SelectedObjects = null;
			}
			catch
			{
			}
			grid.Dispose();
			grid = null;
			instance = null;
		}
	}

	private void SetDesignableObject(object obj)
	{
		inUpdate = true;
		if (grid != null)
		{
			grid.SelectedObject = obj;
			SelectedObjectsChanged();
		}
		inUpdate = false;
	}

	private void SetDesignableObjects(object[] obj)
	{
		inUpdate = true;
		if (grid != null)
		{
			grid.SelectedObjects = obj;
			SelectedObjectsChanged();
		}
		inUpdate = false;
	}

	private void RemoveHost(IDesignerHost host)
	{
		this.host = null;
		grid.Site = null;
	}

	private void SetDesignerHost(IDesignerHost host)
	{
		this.host = host;
		if (host != null)
		{
			grid.Site = new IDEContainer(host).CreateSite(grid);
		}
		else
		{
			grid.Site = null;
		}
	}

	private void SetSelectableObjects(ICollection coll)
	{
		inUpdate = true;
		try
		{
			comboBox.Items.Clear();
			if (coll == null)
			{
				return;
			}
			foreach (object item in coll)
			{
				if (item != null)
				{
					comboBox.Items.Add(item);
				}
			}
		}
		finally
		{
			inUpdate = false;
		}
	}

	public string GetSelectedPropertyName()
	{
		PropertyVisibleDeepEnumerator selectedPropertyEnumerator = grid.SelectedPropertyEnumerator;
		if (selectedPropertyEnumerator != null)
		{
			Property property = selectedPropertyEnumerator.Property;
			return property.Name;
		}
		return null;
	}

	public void ShowHelp()
	{
		LoggingService.Info("Show help on property pad");
		PropertyVisibleDeepEnumerator selectedPropertyEnumerator = grid.SelectedPropertyEnumerator;
		if (!(selectedPropertyEnumerator != null))
		{
			return;
		}
		Property property = selectedPropertyEnumerator.Property;
		Type componentType = property.Value.PropertyDescriptor.ComponentType;
		if (!(componentType != null))
		{
			return;
		}
		IClass obj = ParserService.CurrentProjectContent.GetClass(componentType.FullName);
		if (obj == null)
		{
			return;
		}
		foreach (IProperty property2 in obj.DefaultReturnType.GetProperties())
		{
			if (property.Value.PropertyDescriptor.Name == property2.Name)
			{
				HelpProvider.ShowHelp(property2);
				return;
			}
		}
		HelpProvider.ShowHelp(obj);
	}

	private void PropertyChanged(object sender, VisualHint.SmartPropertyGrid.PropertyChangedEventArgs e)
	{
		OnPropertyValueChanged(sender, e);
	}

	private void OnPropertyValueChanged(object sender, VisualHint.SmartPropertyGrid.PropertyChangedEventArgs e)
	{
		if (PropertyPad.PropertyValueChanged != null)
		{
			PropertyPad.PropertyValueChanged(sender, e);
		}
	}
}
