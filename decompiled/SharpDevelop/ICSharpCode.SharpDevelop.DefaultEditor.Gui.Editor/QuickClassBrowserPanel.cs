using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class QuickClassBrowserPanel : UserControl
{
	private class ComboBoxItem : IComparable
	{
		private object item;

		private string text;

		private int iconIndex;

		private bool isInCurrentPart;

		private string cachedString;

		public int IconIndex => iconIndex;

		public object Item => item;

		public bool IsInCurrentPart => isInCurrentPart;

		public DomRegion ItemRegion
		{
			get
			{
				if (item is IClass)
				{
					return ((IClass)item).Region;
				}
				if (item is IMember)
				{
					return ((IMember)item).Region;
				}
				return DomRegion.Empty;
			}
		}

		public int Line
		{
			get
			{
				DomRegion itemRegion = ItemRegion;
				if (itemRegion.IsEmpty)
				{
					return 0;
				}
				return itemRegion.BeginLine - 1;
			}
		}

		public int Column
		{
			get
			{
				DomRegion itemRegion = ItemRegion;
				if (itemRegion.IsEmpty)
				{
					return 0;
				}
				return itemRegion.BeginColumn - 1;
			}
		}

		public int EndLine
		{
			get
			{
				DomRegion itemRegion = ItemRegion;
				if (itemRegion.IsEmpty)
				{
					return 0;
				}
				return itemRegion.EndLine - 1;
			}
		}

		public ComboBoxItem(object item, string text, int iconIndex, bool isInCurrentPart)
		{
			this.item = item;
			this.text = text;
			this.iconIndex = iconIndex;
			this.isInCurrentPart = isInCurrentPart;
		}

		public bool IsInside(int lineNumber)
		{
			if (!isInCurrentPart)
			{
				return false;
			}
			if (item is IClass { Region: var region } obj)
			{
				if (region.IsEmpty)
				{
					return false;
				}
				if (obj.Region.BeginLine - 1 <= lineNumber)
				{
					return obj.Region.EndLine - 1 >= lineNumber;
				}
				return false;
			}
			if (!(item is IMember { Region: { IsEmpty: false }, Region: var region3 } member))
			{
				return false;
			}
			bool flag = region3.BeginLine - 1 <= lineNumber;
			if (member is IMethodOrProperty)
			{
				if (((IMethodOrProperty)member).BodyRegion.EndLine >= 0)
				{
					return flag & (lineNumber <= ((IMethodOrProperty)member).BodyRegion.EndLine - 1);
				}
				return member.Region.BeginLine - 1 == lineNumber;
			}
			return flag & (lineNumber <= member.Region.EndLine - 1);
		}

		public int CompareItemTo(object obj)
		{
			ComboBoxItem comboBoxItem = (ComboBoxItem)obj;
			if (comboBoxItem.Item is IComparable)
			{
				return ((IComparable)comboBoxItem.Item).CompareTo(item);
			}
			if (comboBoxItem.text != text || comboBoxItem.Line != Line || comboBoxItem.EndLine != EndLine || comboBoxItem.iconIndex != iconIndex)
			{
				return 1;
			}
			return 0;
		}

		public override string ToString()
		{
			if (cachedString == null)
			{
				cachedString = ToStringInternal();
			}
			return cachedString;
		}

		private string ToStringInternal()
		{
			IAmbience currentAmbience = AmbienceService.CurrentAmbience;
			currentAmbience.ConversionFlags = ConversionFlags.ShowParameterNames;
			if (item is IMethod)
			{
				return currentAmbience.Convert((IMethod)item);
			}
			if (item is IProperty)
			{
				return currentAmbience.Convert((IProperty)item);
			}
			if (item is IField)
			{
				return currentAmbience.Convert((IField)item);
			}
			if (item is IProperty)
			{
				return currentAmbience.Convert((IProperty)item);
			}
			if (item is IEvent)
			{
				return currentAmbience.Convert((IEvent)item);
			}
			return text;
		}

		public int CompareTo(object obj)
		{
			return ToString().CompareTo(obj.ToString());
		}
	}

	private ComboBox classComboBox;

	private ComboBox membersComboBox;

	private ICompilationUnit currentCompilationUnit;

	private SharpDevelopTextAreaControl textAreaControl;

	private bool autoselect = true;

	private bool membersComboBoxSelectedMember;

	private bool classComboBoxSelectedMember;

	private IClass lastClassInMembersComboBox;

	private static Font font = (font = new Font("Arial", 8.25f));

	private static StringFormat drawStringFormat = new StringFormat(StringFormatFlags.NoWrap);

	public QuickClassBrowserPanel(SharpDevelopTextAreaControl textAreaControl)
	{
		InitializeComponent();
		membersComboBox.MaxDropDownItems = 20;
		base.Dock = DockStyle.Top;
		this.textAreaControl = textAreaControl;
		this.textAreaControl.ActiveTextAreaControl.Caret.PositionChanged += CaretPositionChanged;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			textAreaControl.ActiveTextAreaControl.Caret.PositionChanged -= CaretPositionChanged;
			membersComboBox.Dispose();
			classComboBox.Dispose();
		}
		base.Dispose(disposing);
	}

	private void CaretPositionChanged(object sender, EventArgs e)
	{
		if (e != EventArgs.Empty)
		{
			return;
		}
		try
		{
			ParseInformation parseInformation = ParserService.GetParseInformation(textAreaControl.FileName);
			if (parseInformation == null)
			{
				return;
			}
			if (currentCompilationUnit != parseInformation.MostRecentCompilationUnit)
			{
				currentCompilationUnit = parseInformation.MostRecentCompilationUnit;
				if (currentCompilationUnit != null)
				{
					FillClassComboBox(isUpdateRequired: true);
					FillMembersComboBox();
				}
			}
			UpdateClassComboBox();
			UpdateMembersComboBox();
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	private void UpdateMembersComboBox()
	{
		autoselect = false;
		try
		{
			if (currentCompilationUnit != null)
			{
				for (int i = 0; i < membersComboBox.Items.Count; i++)
				{
					if (((ComboBoxItem)membersComboBox.Items[i]).IsInside(textAreaControl.ActiveTextAreaControl.Caret.Line))
					{
						if (membersComboBox.SelectedIndex != i)
						{
							membersComboBox.SelectedIndex = i;
						}
						if (!membersComboBoxSelectedMember)
						{
							membersComboBox.Refresh();
						}
						membersComboBoxSelectedMember = true;
						return;
					}
				}
			}
			membersComboBox.SelectedIndex = -1;
			if (membersComboBoxSelectedMember)
			{
				membersComboBox.Refresh();
				membersComboBoxSelectedMember = false;
			}
		}
		finally
		{
			autoselect = true;
		}
	}

	private void UpdateClassComboBox()
	{
		if (currentCompilationUnit == null)
		{
			currentCompilationUnit = ParserService.GetParseInformation(Path.GetFullPath(textAreaControl.FileName)).MostRecentCompilationUnit;
		}
		autoselect = false;
		try
		{
			if (currentCompilationUnit != null)
			{
				for (int i = 0; i < classComboBox.Items.Count; i++)
				{
					if (!((ComboBoxItem)classComboBox.Items[i]).IsInside(textAreaControl.ActiveTextAreaControl.Caret.Line))
					{
						continue;
					}
					bool flag = false;
					for (int j = i + 1; j < classComboBox.Items.Count; j++)
					{
						if (((ComboBoxItem)classComboBox.Items[j]).IsInside(textAreaControl.ActiveTextAreaControl.Caret.Line))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						if (classComboBox.SelectedIndex != i)
						{
							classComboBox.SelectedIndex = i;
							FillMembersComboBox();
						}
						if (!classComboBoxSelectedMember)
						{
							classComboBox.Refresh();
						}
						classComboBoxSelectedMember = true;
						return;
					}
				}
			}
			if (classComboBoxSelectedMember)
			{
				classComboBox.Refresh();
				classComboBoxSelectedMember = false;
			}
		}
		finally
		{
			autoselect = true;
		}
	}

	private bool NeedtoUpdate(ArrayList items, ComboBox comboBox)
	{
		if (items.Count != comboBox.Items.Count)
		{
			return true;
		}
		for (int i = 0; i < items.Count; i++)
		{
			ComboBoxItem comboBoxItem = (ComboBoxItem)comboBox.Items[i];
			ComboBoxItem comboBoxItem2 = (ComboBoxItem)items[i];
			if (comboBoxItem.GetType() != comboBoxItem2.GetType())
			{
				return true;
			}
			if (comboBoxItem2.CompareItemTo(comboBoxItem) != 0)
			{
				return true;
			}
		}
		return false;
	}

	private void FillMembersComboBox()
	{
		IClass obj = GetCurrentSelectedClass();
		if (obj == null || lastClassInMembersComboBox == obj)
		{
			return;
		}
		lastClassInMembersComboBox = obj;
		ArrayList arrayList = new ArrayList();
		bool flag = false;
		IClass obj2 = obj;
		if (obj.IsPartial && obj.GetCompoundClass() is CompoundClass compoundClass)
		{
			flag = true;
			obj = compoundClass;
		}
		lock (obj)
		{
			int index = 0;
			IComparer comparer = new Comparer(CultureInfo.InvariantCulture);
			foreach (IMethod method in obj.Methods)
			{
				arrayList.Add(new ComboBoxItem(method, method.Name, ClassBrowserIconService.GetIcon(method), !flag || obj2.Methods.Contains(method)));
			}
			arrayList.Sort(index, obj.Methods.Count, comparer);
			index = arrayList.Count;
			foreach (IProperty property in obj.Properties)
			{
				arrayList.Add(new ComboBoxItem(property, property.Name, ClassBrowserIconService.GetIcon(property), !flag || obj2.Properties.Contains(property)));
			}
			arrayList.Sort(index, obj.Properties.Count, comparer);
			index = arrayList.Count;
			foreach (IField field in obj.Fields)
			{
				arrayList.Add(new ComboBoxItem(field, field.Name, ClassBrowserIconService.GetIcon(field), !flag || obj2.Fields.Contains(field)));
			}
			arrayList.Sort(index, obj.Fields.Count, comparer);
			index = arrayList.Count;
			foreach (IEvent @event in obj.Events)
			{
				arrayList.Add(new ComboBoxItem(@event, @event.Name, ClassBrowserIconService.GetIcon(@event), !flag || obj2.Events.Contains(@event)));
			}
			arrayList.Sort(index, obj.Events.Count, comparer);
			index = arrayList.Count;
		}
		membersComboBox.BeginUpdate();
		membersComboBox.Items.Clear();
		membersComboBox.Items.AddRange(arrayList.ToArray());
		membersComboBox.EndUpdate();
		UpdateMembersComboBox();
	}

	private void AddClasses(ArrayList items, ICollection classes)
	{
		foreach (IClass @class in classes)
		{
			items.Add(new ComboBoxItem(@class, @class.FullyQualifiedName, ClassBrowserIconService.GetIcon(@class), isInCurrentPart: true));
			AddClasses(items, @class.InnerClasses);
		}
	}

	private void FillClassComboBox(bool isUpdateRequired)
	{
		ArrayList arrayList = new ArrayList();
		AddClasses(arrayList, currentCompilationUnit.Classes);
		if (isUpdateRequired)
		{
			classComboBox.BeginUpdate();
		}
		classComboBox.Items.Clear();
		membersComboBox.Items.Clear();
		classComboBox.Items.AddRange(arrayList.ToArray());
		if (arrayList.Count == 1)
		{
			try
			{
				autoselect = false;
				classComboBox.SelectedIndex = 0;
				FillMembersComboBox();
			}
			finally
			{
				autoselect = true;
			}
		}
		if (isUpdateRequired)
		{
			classComboBox.EndUpdate();
		}
		UpdateClassComboBox();
	}

	private void InitializeComponent()
	{
		this.membersComboBox = new System.Windows.Forms.ComboBox();
		this.classComboBox = new System.Windows.Forms.ComboBox();
		base.SuspendLayout();
		this.membersComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.membersComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
		this.membersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.membersComboBox.Location = new System.Drawing.Point(200, 4);
		this.membersComboBox.Name = "membersComboBox";
		this.membersComboBox.Size = new System.Drawing.Size(161, 21);
		this.membersComboBox.TabIndex = 1;
		this.membersComboBox.SelectedIndexChanged += new System.EventHandler(ComboBoxSelectedIndexChanged);
		this.membersComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(MeasureComboBoxItem);
		this.membersComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(ComboBoxDrawItem);
		this.classComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
		this.classComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.classComboBox.Location = new System.Drawing.Point(4, 4);
		this.classComboBox.Name = "classComboBox";
		this.classComboBox.Size = new System.Drawing.Size(189, 21);
		this.classComboBox.TabIndex = 0;
		this.classComboBox.SelectedIndexChanged += new System.EventHandler(ComboBoxSelectedIndexChanged);
		this.classComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(MeasureComboBoxItem);
		this.classComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(ComboBoxDrawItem);
		this.classComboBox.Sorted = true;
		base.Controls.Add(this.membersComboBox);
		base.Controls.Add(this.classComboBox);
		base.Name = "QuickClassBrowserPanel";
		base.Size = new System.Drawing.Size(368, 28);
		base.Resize += new System.EventHandler(QuickClassBrowserPanelResize);
		base.ResumeLayout(false);
	}

	public IClass GetCurrentSelectedClass()
	{
		if (classComboBox.SelectedIndex >= 0)
		{
			return (IClass)((ComboBoxItem)classComboBox.Items[classComboBox.SelectedIndex]).Item;
		}
		return null;
	}

	private void ComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (autoselect)
		{
			ComboBoxItem comboBoxItem = (ComboBoxItem)comboBox.Items[comboBox.SelectedIndex];
			if (comboBoxItem.IsInCurrentPart)
			{
				textAreaControl.ActiveTextAreaControl.Caret.Position = new TextLocation(comboBoxItem.Column, comboBoxItem.Line);
				textAreaControl.ActiveTextAreaControl.TextArea.Focus();
			}
			else if (comboBoxItem.Item is IMember member)
			{
				string fileName = member.DeclaringType.CompilationUnit.FileName;
				FileService.JumpToFilePosition(fileName, comboBoxItem.Line, comboBoxItem.Column);
			}
			if (comboBox == classComboBox)
			{
				FillMembersComboBox();
				UpdateMembersComboBox();
			}
		}
	}

	private void ComboBoxDrawItem(object sender, DrawItemEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		e.DrawBackground();
		if (e.Index >= 0)
		{
			ComboBoxItem comboBoxItem = (ComboBoxItem)comboBox.Items[e.Index];
			e.Graphics.DrawImageUnscaled(ClassBrowserIconService.ImageList.Images[comboBoxItem.IconIndex], new Point(e.Bounds.X, e.Bounds.Y + (e.Bounds.Height - ClassBrowserIconService.ImageList.ImageSize.Height) / 2));
			Rectangle rectangle = new Rectangle(e.Bounds.X + ClassBrowserIconService.ImageList.ImageSize.Width, e.Bounds.Y, e.Bounds.Width - ClassBrowserIconService.ImageList.ImageSize.Width, e.Bounds.Height);
			Brush brush = SystemBrushes.WindowText;
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{
				brush = SystemBrushes.HighlightText;
			}
			if (!comboBoxItem.IsInCurrentPart)
			{
				brush = SystemBrushes.ControlDark;
			}
			else if (e.State == DrawItemState.ComboBoxEdit && !comboBoxItem.IsInside(textAreaControl.ActiveTextAreaControl.Caret.Line))
			{
				brush = SystemBrushes.ControlDark;
			}
			e.Graphics.DrawString(comboBoxItem.ToString(), font, brush, rectangle, drawStringFormat);
		}
		e.DrawFocusRectangle();
	}

	private void QuickClassBrowserPanelResize(object sender, EventArgs e)
	{
		Size size = new Size(base.Width / 2 - 12, 21);
		classComboBox.Size = size;
		membersComboBox.Location = new Point(classComboBox.Bounds.Right + 8, classComboBox.Bounds.Top);
		membersComboBox.Size = size;
	}

	private void MeasureComboBoxItem(object sender, MeasureItemEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (e.Index >= 0)
		{
			ComboBoxItem comboBoxItem = (ComboBoxItem)comboBox.Items[e.Index];
			SizeF sizeF = e.Graphics.MeasureString(comboBoxItem.ToString(), font);
			e.ItemWidth = (int)sizeF.Width;
			e.ItemHeight = (int)Math.Max(sizeF.Height, ClassBrowserIconService.ImageList.ImageSize.Height);
		}
	}
}
