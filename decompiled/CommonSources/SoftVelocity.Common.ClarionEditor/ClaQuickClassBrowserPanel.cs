using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using SoftVelocity.Common.ClassBrowser;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.ClarionEditor;

public class ClaQuickClassBrowserPanel : UserControl
{
	private class ComboBoxItem : IComparable
	{
		private IClass classItem;

		private IMember memberItem;

		private string text;

		private int iconIndex;

		private bool isInCurrentPart;

		private bool regionFilled;

		private DomRegion region;

		private string cachedString;

		public int IconIndex => iconIndex;

		public object Item
		{
			get
			{
				if (classItem != null)
				{
					return classItem;
				}
				return memberItem;
			}
		}

		public bool IsInCurrentPart => isInCurrentPart;

		public DomRegion ItemRegion
		{
			get
			{
				//IL_0174: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Unknown result type (might be due to invalid IL or missing references)
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0169: Unknown result type (might be due to invalid IL or missing references)
				//IL_016e: Unknown result type (might be due to invalid IL or missing references)
				//IL_00de: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_0156: Unknown result type (might be due to invalid IL or missing references)
				//IL_015b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0143: Unknown result type (might be due to invalid IL or missing references)
				//IL_0148: Unknown result type (might be due to invalid IL or missing references)
				if (!regionFilled)
				{
					regionFilled = true;
					if (classItem != null)
					{
						DomRegion val = classItem.Region;
						int beginLine = ((DomRegion)(ref val)).BeginLine;
						DomRegion val2 = classItem.Region;
						int beginColumn = ((DomRegion)(ref val2)).BeginColumn;
						DomRegion bodyRegion = classItem.BodyRegion;
						int endLine = ((DomRegion)(ref bodyRegion)).EndLine;
						DomRegion bodyRegion2 = classItem.BodyRegion;
						region = new DomRegion(beginLine, beginColumn, endLine, ((DomRegion)(ref bodyRegion2)).EndColumn);
					}
					else if (memberItem is IMethodOrProperty)
					{
						if (memberItem is ClaMethod)
						{
							ClaMethod claMethod = (ClaMethod)(object)memberItem;
							region = new DomRegion(claMethod.ClaBodyRegion.DeclBeginLine, claMethod.ClaBodyRegion.DeclBeginColumn, claMethod.ClaBodyRegion.EndLine, claMethod.ClaBodyRegion.EndColumn);
						}
						else if (memberItem is ClaProperty)
						{
							ClaProperty claProperty = (ClaProperty)(object)memberItem;
							region = new DomRegion(claProperty.ClaBodyRegion.DeclBeginLine, claProperty.ClaBodyRegion.DeclBeginColumn, claProperty.ClaBodyRegion.EndLine, claProperty.ClaBodyRegion.EndColumn);
						}
						else
						{
							region = memberItem.BodyRegion;
						}
					}
					else
					{
						region = memberItem.Region;
					}
				}
				return region;
			}
		}

		public int Line
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				DomRegion itemRegion = ItemRegion;
				if (((DomRegion)(ref itemRegion)).IsEmpty)
				{
					return 0;
				}
				return ((DomRegion)(ref itemRegion)).BeginLine - 1;
			}
		}

		public int Column
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				DomRegion itemRegion = ItemRegion;
				if (((DomRegion)(ref itemRegion)).IsEmpty)
				{
					return 0;
				}
				return ((DomRegion)(ref itemRegion)).BeginColumn - 1;
			}
		}

		public ComboBoxItem(object item, string text, int iconIndex, bool isInCurrentPart)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			if (item is IClass)
			{
				classItem = (IClass)item;
			}
			else if (item is IMember)
			{
				memberItem = (IMember)item;
			}
			this.text = text;
			this.iconIndex = iconIndex;
			this.isInCurrentPart = isInCurrentPart;
		}

		public bool IsInside(int lineNumber)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (!isInCurrentPart)
			{
				return false;
			}
			DomRegion itemRegion = ItemRegion;
			if (((DomRegion)(ref itemRegion)).BeginLine - 1 <= lineNumber)
			{
				DomRegion itemRegion2 = ItemRegion;
				if (((DomRegion)(ref itemRegion2)).EndLine - 1 >= lineNumber)
				{
					return true;
				}
			}
			return false;
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
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Expected O, but got Unknown
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Expected O, but got Unknown
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Expected O, but got Unknown
			IAmbience currentAmbience = (IAmbience)(object)AmbienceService.CurrentAmbience;
			currentAmbience.ConversionFlags = (ConversionFlags)1;
			if (Item is IMethod)
			{
				if (Item is ClaMethod)
				{
					ClaMethod claMethod = (ClaMethod)Item;
					string text;
					if (claMethod.IsAccessor)
					{
						text = currentAmbience.Convert((IProperty)(object)claMethod.DeclaringProperty);
						text = ((!claMethod.IsGetter) ? (text + " (setter)") : (text + " (getter)"));
					}
					else
					{
						text = currentAmbience.Convert((IMethod)Item);
					}
					if (!string.IsNullOrEmpty(claMethod.IfaceImplDisplayName))
					{
						text = claMethod.IfaceImplDisplayName + "." + text;
					}
					return text;
				}
				return currentAmbience.Convert((IMethod)Item);
			}
			if (Item is IProperty)
			{
				return currentAmbience.Convert((IProperty)Item);
			}
			if (Item is IField)
			{
				return currentAmbience.Convert((IField)Item);
			}
			if (Item is IEvent)
			{
				return currentAmbience.Convert((IEvent)Item);
			}
			return this.text;
		}

		public int CompareTo(object obj)
		{
			return ToString().CompareTo(obj.ToString());
		}
	}

	private ComboBox classComboBox;

	private ComboBox membersComboBox;

	private ClaCompilationUnit currentCompilationUnit;

	private SharpDevelopTextAreaControl textAreaControl;

	private bool autoselect = true;

	private bool membersComboBoxSelectedMember;

	private bool classComboBoxSelectedMember;

	private IClass lastClassInMembersComboBox;

	private bool userSelected;

	private static Font font = (font = new Font("Arial", 8.25f));

	private static StringFormat drawStringFormat = new StringFormat(StringFormatFlags.NoWrap);

	public ClaQuickClassBrowserPanel(SharpDevelopTextAreaControl textAreaControl)
	{
		InitializeComponent();
		membersComboBox.MaxDropDownItems = 20;
		base.Dock = DockStyle.Top;
		this.textAreaControl = textAreaControl;
		if (base.Enabled)
		{
			((TextEditorControlBase)this.textAreaControl).ActiveTextAreaControl.Caret.PositionChanged += CaretPositionChanged;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (base.Enabled)
			{
				((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.Caret.PositionChanged -= CaretPositionChanged;
			}
			membersComboBox.Dispose();
			classComboBox.Dispose();
			currentCompilationUnit = null;
			lastClassInMembersComboBox = null;
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
			ParseInformation parseInformationIfExist = ParserService.GetParseInformationIfExist(((TextEditorControlBase)textAreaControl).FileName);
			if (parseInformationIfExist == null)
			{
				return;
			}
			if ((object)currentCompilationUnit != parseInformationIfExist.MostRecentCompilationUnit)
			{
				currentCompilationUnit = parseInformationIfExist.MostRecentCompilationUnit as ClaCompilationUnit;
				if (currentCompilationUnit != null)
				{
					FillClassComboBox(isUpdateRequired: true);
					FillMembersComboBox();
					return;
				}
				membersComboBox.Items.Clear();
				classComboBox.Items.Clear();
				lastClassInMembersComboBox = null;
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
					if (((ComboBoxItem)membersComboBox.Items[i]).IsInside(((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.Caret.Line))
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
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		autoselect = false;
		try
		{
			if (currentCompilationUnit != null)
			{
				object obj = currentCompilationUnit.FindNearestObject(((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.Caret.Line + 1, ((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.Caret.Column + 1);
				IClass val = null;
				if (obj != null)
				{
					if (obj is IClass)
					{
						val = (IClass)obj;
					}
					else if (obj is IMember)
					{
						val = ((IDecoration)(IMember)obj).DeclaringType;
					}
				}
				if (val == null)
				{
					val = (IClass)(object)currentCompilationUnit.GlobalClass;
				}
				if (val != null)
				{
					string text = CreateClassName(val, showPRE: true);
					for (int i = 0; i < classComboBox.Items.Count; i++)
					{
						if (text.Equals(classComboBox.Items[i].ToString(), StringComparison.InvariantCultureIgnoreCase))
						{
							if (classComboBox.SelectedIndex != i)
							{
								classComboBox.SelectedIndex = i;
								FillMembersComboBox();
							}
							else if (userSelected)
							{
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

	private void FillMembersComboBox()
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		IClass val = GetCurrentSelectedClass();
		if (val == null || lastClassInMembersComboBox == val)
		{
			return;
		}
		lastClassInMembersComboBox = val;
		ArrayList arrayList = new ArrayList();
		IClass val2 = val;
		bool flag = false;
		string fileName = val2.CompilationUnit.FileName;
		if (val.IsPartial)
		{
			IClass underlyingClass = val.DefaultReturnType.GetUnderlyingClass();
			CompoundClass val3 = (CompoundClass)(object)((underlyingClass is CompoundClass) ? underlyingClass : null);
			if (val3 != null && val3.GetParts().Count > 0)
			{
				flag = true;
				val = (IClass)(object)val3;
			}
		}
		lock (val)
		{
			int index = 0;
			IComparer comparer = new Comparer(CultureInfo.InvariantCulture);
			int num = 0;
			foreach (IMethod method in val.Methods)
			{
				if (method is ClaMethod)
				{
					ClaMethod claMethod = (ClaMethod)(object)method;
					if (!claMethod.ClaBodyRegion.IsEmpty)
					{
						num++;
						bool isInCurrentPart = claMethod.ClaBodyRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase);
						arrayList.Add(new ComboBoxItem(method, ((IMember)method).Name, ClassBrowserIconService.GetIcon(method), isInCurrentPart));
					}
				}
				else
				{
					DomRegion region = ((IMember)method).Region;
					if (!((DomRegion)(ref region)).IsEmpty)
					{
						num++;
						bool isInCurrentPart2 = !flag || val2.Methods.Contains(method);
						arrayList.Add(new ComboBoxItem(method, ((IMember)method).Name, ClassBrowserIconService.GetIcon(method), isInCurrentPart2));
					}
				}
			}
			arrayList.Sort(index, num, comparer);
			index = arrayList.Count;
			num = 0;
			foreach (IProperty property in val.Properties)
			{
				if (property is ClaProperty)
				{
					ClaProperty claProperty = (ClaProperty)(object)property;
					if (!claProperty.IsUnresolvedDef)
					{
						bool isInCurrentPart3 = claProperty.ClaRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase);
						arrayList.Add(new ComboBoxItem(property, ((IMember)property).Name, ClassBrowserIconService.GetIcon(property), isInCurrentPart3));
						num++;
					}
					if (claProperty.Getter != null && !claProperty.Getter.IsInline)
					{
						bool isInCurrentPart3 = claProperty.ClaGetterRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase);
						arrayList.Add(new ComboBoxItem(claProperty.Getter, claProperty.Getter.Name, ClassBrowserIconService.GetIcon(property), isInCurrentPart3));
						num++;
					}
					if (claProperty.Setter != null && !claProperty.Setter.IsInline)
					{
						bool isInCurrentPart3 = claProperty.ClaSetterRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase);
						arrayList.Add(new ComboBoxItem(claProperty.Setter, claProperty.Setter.Name, ClassBrowserIconService.GetIcon(property), isInCurrentPart3));
						num++;
					}
				}
				else
				{
					bool isInCurrentPart4 = !flag || val2.Properties.Contains(property);
					arrayList.Add(new ComboBoxItem(property, ((IMember)property).Name, ClassBrowserIconService.GetIcon(property), isInCurrentPart4));
					num++;
				}
			}
			arrayList.Sort(index, num, comparer);
			index = arrayList.Count;
			foreach (IField field in val.Fields)
			{
				bool isInCurrentPart5;
				int iconIndex;
				if (field is ClaField)
				{
					isInCurrentPart5 = ((ClaField)(object)field).ClaRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase);
					iconIndex = ((!(field is ClaKeyField)) ? ClassBrowserIconService.GetIcon(field) : ClaClassNode.KeyIcon);
				}
				else
				{
					isInCurrentPart5 = !flag || val2.Fields.Contains(field);
					iconIndex = ClassBrowserIconService.GetIcon(field);
				}
				arrayList.Add(new ComboBoxItem(field, ((IMember)field).Name, iconIndex, isInCurrentPart5));
			}
			arrayList.Sort(index, val.Fields.Count, comparer);
			index = arrayList.Count;
			foreach (IEvent @event in val.Events)
			{
				arrayList.Add(new ComboBoxItem(isInCurrentPart: (!(@event is ClaEvent)) ? (!flag || val2.Events.Contains(@event)) : ((ClaEvent)(object)@event).ClaRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase), item: @event, text: ((IMember)@event).Name, iconIndex: ClassBrowserIconService.GetIcon(@event)));
			}
			arrayList.Sort(index, val.Events.Count, comparer);
		}
		membersComboBox.BeginUpdate();
		membersComboBox.Items.Clear();
		membersComboBox.Items.AddRange(arrayList.ToArray());
		membersComboBox.EndUpdate();
		UpdateMembersComboBox();
	}

	private void AddClasses(ArrayList items, ICompilationUnit cu)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if (cu is ClaCompilationUnit)
		{
			ClaCompilationUnit claCompilationUnit = (ClaCompilationUnit)(object)cu;
			Dictionary<string, IClass> dictionary = new Dictionary<string, IClass>();
			foreach (object @object in claCompilationUnit.Region2ObjectMap.Objects)
			{
				if (@object is IClass)
				{
					string key = CreateClassName((IClass)@object, showPRE: true);
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, (IClass)@object);
					}
				}
				else
				{
					if (!(@object is IDecoration))
					{
						continue;
					}
					IClass declaringType = ((IDecoration)@object).DeclaringType;
					if (declaringType != null)
					{
						string key2 = CreateClassName(declaringType, showPRE: true);
						if (!dictionary.ContainsKey(key2))
						{
							dictionary.Add(key2, declaringType);
						}
					}
				}
			}
			{
				foreach (KeyValuePair<string, IClass> item in dictionary)
				{
					AddClass(items, item.Value, onlyClass: true);
				}
				return;
			}
		}
		AddClasses(items, (ICollection)cu.Classes);
	}

	private void AddClasses(ArrayList items, ICollection classes)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		foreach (IClass @class in classes)
		{
			IClass c = @class;
			AddClass(items, c, onlyClass: false);
		}
	}

	private void AddClass(ArrayList items, IClass c, bool onlyClass)
	{
		int sortOrder = 0;
		int iconIndexForClass = ClaClassNode.GetIconIndexForClass(c, ref sortOrder);
		string text = CreateClassName(c, showPRE: true);
		bool isInCurrentPart = true;
		if (c is ClaClass)
		{
			ClaDomRegion claRegion = ((ClaClass)(object)c).ClaRegion;
			isInCurrentPart = claRegion.IsEmpty || claRegion.FileName.Equals(c.CompilationUnit.FileName, StringComparison.InvariantCultureIgnoreCase);
		}
		items.Add(new ComboBoxItem(c, text, iconIndexForClass, isInCurrentPart));
		if (onlyClass)
		{
			return;
		}
		foreach (IMethod method in c.Methods)
		{
			if (!(method is ClaMethod))
			{
				continue;
			}
			ClaMethod claMethod = (ClaMethod)(object)method;
			if (!claMethod.ClaBodyRegion.FileName.Equals(c.CompilationUnit.FileName, StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			foreach (IClass localType in claMethod.LocalTypes)
			{
				AddClass(items, localType, onlyClass: false);
			}
		}
		foreach (IProperty property in c.Properties)
		{
			if (!(property is ClaProperty))
			{
				continue;
			}
			ClaProperty claProperty = (ClaProperty)(object)property;
			if (claProperty.Getter != null && claProperty.ClaGetterRegion.FileName.Equals(c.CompilationUnit.FileName, StringComparison.InvariantCultureIgnoreCase))
			{
				foreach (IClass localType2 in claProperty.Getter.LocalTypes)
				{
					AddClass(items, localType2, onlyClass: false);
				}
			}
			if (claProperty.Setter == null || !claProperty.ClaSetterRegion.FileName.Equals(c.CompilationUnit.FileName, StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			foreach (IClass localType3 in claProperty.Setter.LocalTypes)
			{
				AddClass(items, localType3, onlyClass: false);
			}
		}
		if (c.InnerClasses.Count > 0)
		{
			AddClasses(items, (ICollection)c.InnerClasses);
		}
	}

	private static string CreateClassName(IClass c, bool showPRE)
	{
		if (c == null)
		{
			return string.Empty;
		}
		if (c is ClaGlobalClass)
		{
			return ClaGlobalClass.globalClassName;
		}
		string text = string.Empty;
		if (!(c is ClaLocalClass))
		{
			text = ((((IDecoration)c).DeclaringType == null) ? c.Name : (CreateClassName(((IDecoration)c).DeclaringType, showPRE: false) + "." + c.Name));
		}
		else
		{
			ClaLocalClass claLocalClass = (ClaLocalClass)(object)c;
			IClass declaringType = claLocalClass.DeclaringMethod.DeclaringType;
			if (declaringType != null && !(declaringType is ClaGlobalClass))
			{
				text = CreateClassName(declaringType, showPRE: false) + ".";
			}
			text = text + claLocalClass.DeclaringMethod.Name + "." + c.Name;
		}
		if (showPRE && c is ClaClass && !string.IsNullOrEmpty(((ClaClass)(object)c).PreName) && !c.Name.Equals(((ClaClass)(object)c).PreName, StringComparison.InvariantCultureIgnoreCase))
		{
			text = text + " (" + ((ClaClass)(object)c).PreName + ")";
		}
		return text;
	}

	private void FillClassComboBox(bool isUpdateRequired)
	{
		if (currentCompilationUnit == null)
		{
			return;
		}
		ArrayList arrayList = new ArrayList();
		AddClasses(arrayList, (ICompilationUnit)(object)currentCompilationUnit);
		if (isUpdateRequired)
		{
			classComboBox.BeginUpdate();
		}
		classComboBox.Items.Clear();
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
		this.membersComboBox.Size = new System.Drawing.Size(161, 23);
		this.membersComboBox.TabIndex = 1;
		this.membersComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(ComboBoxDrawItem);
		this.membersComboBox.SelectedIndexChanged += new System.EventHandler(ComboBoxSelectedIndexChanged);
		this.membersComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(MeasureComboBoxItem);
		this.classComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
		this.classComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.classComboBox.Location = new System.Drawing.Point(4, 4);
		this.classComboBox.MaxDropDownItems = 15;
		this.classComboBox.Name = "classComboBox";
		this.classComboBox.Size = new System.Drawing.Size(189, 23);
		this.classComboBox.Sorted = true;
		this.classComboBox.TabIndex = 0;
		this.classComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(ComboBoxDrawItem);
		this.classComboBox.SelectedIndexChanged += new System.EventHandler(ComboBoxSelectedIndexChanged);
		this.classComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(MeasureComboBoxItem);
		this.BackColor = System.Drawing.SystemColors.Control;
		base.Controls.Add(this.membersComboBox);
		base.Controls.Add(this.classComboBox);
		base.Name = "ClaQuickClassBrowserPanel";
		base.Size = new System.Drawing.Size(368, 28);
		base.Resize += new System.EventHandler(QuickClassBrowserPanelResize);
		base.EnabledChanged += new System.EventHandler(OnEnableChanged);
		base.ResumeLayout(false);
	}

	public IClass GetCurrentSelectedClass()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		if (classComboBox.SelectedIndex >= 0)
		{
			return (IClass)((ComboBoxItem)classComboBox.Items[classComboBox.SelectedIndex]).Item;
		}
		return null;
	}

	private void ComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		ComboBox comboBox = (ComboBox)sender;
		if (!autoselect)
		{
			return;
		}
		userSelected = true;
		ComboBoxItem comboBoxItem = (ComboBoxItem)comboBox.Items[comboBox.SelectedIndex];
		if (comboBoxItem.IsInCurrentPart)
		{
			((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.Caret.Position = new TextLocation(comboBoxItem.Column, comboBoxItem.Line);
			((Control)(object)((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.TextArea).Focus();
		}
		else
		{
			object item = comboBoxItem.Item;
			IMember val = (IMember)((item is IMember) ? item : null);
			if (val != null)
			{
				string text = ((val is ClaAbstractMember && !((ClaAbstractMember)(object)val).ClaBodyRegion.IsEmpty) ? ((ClaAbstractMember)(object)val).ClaBodyRegion.FileName : ((!(val is ClaAbstractMember) || ((ClaAbstractMember)(object)val).ClaRegion.IsEmpty) ? ((IDecoration)val).DeclaringType.CompilationUnit.FileName : ((ClaAbstractMember)(object)val).ClaRegion.FileName));
				FileService.JumpToFilePosition(text, comboBoxItem.Line, comboBoxItem.Column);
			}
			object item2 = comboBoxItem.Item;
			IClass val2 = (IClass)((item2 is IClass) ? item2 : null);
			if (val2 != null)
			{
				FillMembersComboBox();
			}
		}
		userSelected = false;
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
			else if (e.State == DrawItemState.ComboBoxEdit && !comboBoxItem.IsInside(((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.Caret.Line))
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

	private void OnEnableChanged(object sender, EventArgs e)
	{
		if (base.Enabled)
		{
			((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.Caret.PositionChanged += CaretPositionChanged;
		}
		else
		{
			((TextEditorControlBase)textAreaControl).ActiveTextAreaControl.Caret.PositionChanged -= CaretPositionChanged;
		}
	}
}
