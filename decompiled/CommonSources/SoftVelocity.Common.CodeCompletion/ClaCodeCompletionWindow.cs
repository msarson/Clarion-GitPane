using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaCodeCompletionWindow : AbstractCompletionWindow
{
	public class ClaCompletionOptions
	{
		public CompletionOptions Flags;

		private int maxItemsNum = 10;

		private int preferredWidth = -1;

		private bool useEnhancedList;

		public int MaxVisibleItems
		{
			get
			{
				return maxItemsNum;
			}
			set
			{
				if (value >= 1)
				{
					maxItemsNum = value;
				}
				else
				{
					maxItemsNum = 1;
				}
			}
		}

		public int PreferredWidth
		{
			get
			{
				return preferredWidth;
			}
			set
			{
				preferredWidth = value;
			}
		}

		public bool UseEnhancedList
		{
			get
			{
				return useEnhancedList;
			}
			set
			{
				useEnhancedList = value;
			}
		}
	}

	private const int WHEEL_DELTA = 120;

	protected ICompletionData[] completionData;

	protected ICompletionData[] filteredCompletionData;

	protected int filteredCompletionDataLength;

	protected CodeCompletionListView codeCompletionListView;

	private VScrollBar vScrollBar = new VScrollBar();

	protected ICompletionDataProvider dataProvider;

	protected IDocument document;

	private int filteredDefaultIndex = -1;

	private ClaCompletionOptions options;

	protected int startOffset;

	protected int endOffset;

	private DeclarationViewWindow declarationViewWindow;

	private Rectangle workingScreen;

	private bool scrollbarVisible;

	private bool inScrollUpdate;

	private int mouseWheelDelta;

	public ClaCompletionOptions Options => options;

	public static ClaCodeCompletionWindow ShowCompletionWindow(Form parent, TextEditorControl control, string fileName, ICompletionDataProvider completionDataProvider, char firstChar, ClaCompletionOptions opt)
	{
		ICompletionData[] array = completionDataProvider.GenerateCompletionData(fileName, ((TextEditorControlBase)control).ActiveTextAreaControl.TextArea, firstChar);
		if (array == null || array.Length == 0)
		{
			return null;
		}
		Array.Sort(array);
		ClaCodeCompletionWindow claCodeCompletionWindow = new ClaCodeCompletionWindow(completionDataProvider, array, parent, control, opt);
		((AbstractCompletionWindow)claCodeCompletionWindow).ShowCompletionWindow();
		return claCodeCompletionWindow;
	}

	protected ClaCodeCompletionWindow(ICompletionDataProvider completionDataProvider, ICompletionData[] completionData, Form parent, TextEditorControl control, ClaCompletionOptions opt)
		: base(parent, control)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Expected O, but got Unknown
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Expected O, but got Unknown
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Expected O, but got Unknown
		dataProvider = completionDataProvider;
		this.completionData = completionData;
		document = ((TextEditorControlBase)control).Document;
		options = opt;
		workingScreen = Screen.GetWorkingArea(((Form)this).Location);
		startOffset = ((TextEditorControlBase)control).ActiveTextAreaControl.Caret.Offset + 1;
		endOffset = startOffset;
		if (completionDataProvider.PreSelection != null)
		{
			startOffset -= completionDataProvider.PreSelection.Length + 1;
			endOffset--;
		}
		((Form)this).ControlBox = false;
		((Control)this).SetStyle(ControlStyles.Selectable, value: false);
		((Control)this).SetStyle(ControlStyles.ContainerControl, value: false);
		((Form)this).FormBorderStyle = FormBorderStyle.Sizable;
		if (options.UseEnhancedList)
		{
			codeCompletionListView = (CodeCompletionListView)(object)new ClaCodeCompletionListView();
		}
		else
		{
			codeCompletionListView = new CodeCompletionListView();
		}
		codeCompletionListView.ImageList = completionDataProvider.ImageList;
		((Control)(object)codeCompletionListView).Dock = DockStyle.Fill;
		codeCompletionListView.SelectedItemChanged += CodeCompletionListViewSelectedItemChanged;
		((Control)(object)codeCompletionListView).DoubleClick += CodeCompletionListViewDoubleClick;
		((Control)(object)codeCompletionListView).Click += CodeCompletionListViewClick;
		codeCompletionListView.FirstItemChanged += CodeCompletionListViewFirstItemChanged;
		((Control)this).Controls.Add((Control)(object)codeCompletionListView);
		vScrollBar.Dock = DockStyle.Right;
		vScrollBar.Minimum = 0;
		vScrollBar.SmallChange = 1;
		vScrollBar.LargeChange = options.MaxVisibleItems;
		vScrollBar.Visible = false;
		((Control)this).Controls.Add(vScrollBar);
		if (options.UseEnhancedList)
		{
			((Control)(object)this).MinimumSize = new Size(codeCompletionListView.ItemHeight * 20, codeCompletionListView.ItemHeight * options.MaxVisibleItems + 2 * (SystemInformation.Border3DSize.Height + SystemInformation.BorderSize.Height + SystemInformation.BorderMultiplierFactor));
		}
		else
		{
			((Control)(object)this).MinimumSize = new Size(codeCompletionListView.ItemHeight * 12, codeCompletionListView.ItemHeight * options.MaxVisibleItems + 2 * (SystemInformation.Border3DSize.Height + SystemInformation.BorderSize.Height + SystemInformation.BorderMultiplierFactor));
		}
		((Control)(object)this).MaximumSize = new Size(int.MaxValue, codeCompletionListView.ItemHeight * options.MaxVisibleItems + 2 * (SystemInformation.Border3DSize.Height + SystemInformation.BorderSize.Height + SystemInformation.BorderMultiplierFactor));
		if (declarationViewWindow == null)
		{
			declarationViewWindow = new DeclarationViewWindow(parent);
			((Control)(object)declarationViewWindow).MouseMove += base.ControlMouseMove;
			((Control)(object)declarationViewWindow).Font = new Font("Tahoma", 8f);
		}
		((Control)(object)control).Focus();
		if ((options.Flags & 1) != 0)
		{
			filteredCompletionData = (ICompletionData[])(object)new ICompletionData[completionData.Length];
			FilterData();
			document.DocumentChanged += new DocumentEventHandler(DocumentChanged);
		}
		else
		{
			filteredCompletionData = completionData;
			filteredCompletionDataLength = completionData.Length;
			filteredDefaultIndex = dataProvider.DefaultIndex;
			codeCompletionListView.SetCompletionData(filteredCompletionData, filteredCompletionDataLength);
			UpdateSize(updateLocation: true);
		}
		if (filteredDefaultIndex >= 0)
		{
			codeCompletionListView.SelectIndex(filteredDefaultIndex);
		}
		if (completionDataProvider.PreSelection != null)
		{
			((AbstractCompletionWindow)this).CaretOffsetChanged((object)this, EventArgs.Empty);
		}
		document.DocumentAboutToBeChanged += new DocumentEventHandler(DocumentAboutToBeChanged);
	}

	private void FilterData()
	{
		Array.Clear(filteredCompletionData, 0, filteredCompletionData.Length);
		filteredCompletionDataLength = 0;
		int num = endOffset - startOffset;
		string text = ((document.TextLength < startOffset + num) ? string.Empty : document.GetText(startOffset, num));
		for (int i = 0; i < completionData.Length; i++)
		{
			if (text == string.Empty || completionData[i].Text.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
			{
				filteredCompletionData[filteredCompletionDataLength] = completionData[i];
				if (i == dataProvider.DefaultIndex)
				{
					filteredDefaultIndex = filteredCompletionDataLength;
				}
				filteredCompletionDataLength++;
			}
			else if (i == dataProvider.DefaultIndex)
			{
				filteredDefaultIndex = -1;
			}
		}
		codeCompletionListView.SetCompletionData(filteredCompletionData, filteredCompletionDataLength);
		UpdateSize(updateLocation: true);
	}

	private void UpdateSize(bool updateLocation)
	{
		if (filteredCompletionDataLength > options.MaxVisibleItems)
		{
			if (!scrollbarVisible)
			{
				vScrollBar.ValueChanged += VScrollBarValueChanged;
				vScrollBar.Scroll += vScrollBar_Scroll;
				vScrollBar.Visible = true;
				scrollbarVisible = true;
			}
		}
		else if (scrollbarVisible)
		{
			vScrollBar.ValueChanged -= VScrollBarValueChanged;
			vScrollBar.Scroll -= vScrollBar_Scroll;
			vScrollBar.Visible = false;
			scrollbarVisible = false;
		}
		base.drawingSize = GetRealSize();
		((Control)(object)this).MinimumSize = new Size(((Control)(object)this).MinimumSize.Width, base.drawingSize.Height);
		((Control)(object)this).MaximumSize = new Size(((Control)(object)this).MaximumSize.Width, base.drawingSize.Height);
		if (updateLocation)
		{
			((AbstractCompletionWindow)this).SetLocation();
		}
		if (scrollbarVisible && vScrollBar.Maximum != filteredCompletionDataLength - 1)
		{
			vScrollBar.Maximum = filteredCompletionDataLength - 1;
			CodeCompletionListViewFirstItemChanged(this, EventArgs.Empty);
		}
		SetDeclarationViewLocation();
		declarationViewWindow.ShowDeclarationViewWindow();
		CodeCompletionListViewSelectedItemChanged(this, EventArgs.Empty);
		((Control)(object)((TextEditorControlBase)base.control).ActiveTextAreaControl.TextArea).Focus();
	}

	protected override void WndProc(ref Message m)
	{
		int msg = m.Msg;
		if (msg == 33)
		{
			m.Result = (IntPtr)3;
		}
		else
		{
			((Form)this).WndProc(ref m);
		}
	}

	protected override void OnResizeEnd(EventArgs e)
	{
		((Form)this).OnResizeEnd(e);
		options.PreferredWidth = ((Form)this).Size.Width;
		UpdateSize(updateLocation: false);
		((Control)(object)codeCompletionListView).Refresh();
	}

	private void CodeCompletionListViewFirstItemChanged(object sender, EventArgs e)
	{
		if (!inScrollUpdate)
		{
			inScrollUpdate = true;
			vScrollBar.Value = Math.Min(vScrollBar.Maximum, codeCompletionListView.FirstItem);
			inScrollUpdate = false;
		}
	}

	private void VScrollBarValueChanged(object sender, EventArgs e)
	{
		if (!inScrollUpdate)
		{
			inScrollUpdate = true;
			codeCompletionListView.FirstItem = vScrollBar.Value;
			((Control)(object)codeCompletionListView).Refresh();
			inScrollUpdate = false;
		}
	}

	private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		((Control)(object)((TextEditorControlBase)base.control).ActiveTextAreaControl.TextArea).Focus();
	}

	private void SetDeclarationViewLocation()
	{
		int num = ((Control)this).Bounds.Left - workingScreen.Left;
		int num2 = workingScreen.Right - ((Control)this).Bounds.Right;
		Point point;
		if (num2 * 2 > num)
		{
			declarationViewWindow.FixedWidth = false;
			point = new Point(((Control)this).Bounds.Right, ((Control)this).Bounds.Top);
			if (((Form)(object)declarationViewWindow).Location != point)
			{
				((Form)(object)declarationViewWindow).Location = point;
			}
			return;
		}
		((Control)(object)declarationViewWindow).Width = declarationViewWindow.GetRequiredLeftHandSideWidth(new Point(((Control)this).Bounds.Left, ((Control)this).Bounds.Top));
		declarationViewWindow.FixedWidth = true;
		point = ((((Control)this).Bounds.Left >= ((Control)(object)declarationViewWindow).Width) ? new Point(((Control)this).Bounds.Left - ((Control)(object)declarationViewWindow).Width, ((Control)this).Bounds.Top) : new Point(0, ((Control)this).Bounds.Top));
		if (((Form)(object)declarationViewWindow).Location != point)
		{
			((Form)(object)declarationViewWindow).Location = point;
		}
		((Control)(object)declarationViewWindow).Refresh();
	}

	protected override void SetLocation()
	{
		((AbstractCompletionWindow)this).SetLocation();
		if (declarationViewWindow != null)
		{
			SetDeclarationViewLocation();
		}
	}

	public override void HandleMouseWheel(MouseEventArgs e)
	{
		int num = GetScrollAmount(e);
		if (num != 0)
		{
			if (((TextEditorControlBase)base.control).TextEditorProperties.MouseWheelScrollDown)
			{
				num = -num;
			}
			int val = vScrollBar.Value + vScrollBar.SmallChange * num;
			vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum - vScrollBar.LargeChange + 1, val));
		}
	}

	public int GetScrollAmount(MouseEventArgs e)
	{
		mouseWheelDelta += e.Delta;
		int num = Math.Max(SystemInformation.MouseWheelScrollLines, 1);
		int result = mouseWheelDelta * num / 120;
		mouseWheelDelta %= Math.Max(1, 120 / num);
		return result;
	}

	private void CodeCompletionListViewSelectedItemChanged(object sender, EventArgs e)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		ICompletionData selectedCompletionData = codeCompletionListView.SelectedCompletionData;
		if ((options.Flags & 8) != 0 && selectedCompletionData != null && selectedCompletionData.Description != null && selectedCompletionData.Description.Length > 0)
		{
			declarationViewWindow.Description = selectedCompletionData.Description;
			SetDeclarationViewLocation();
		}
		else
		{
			declarationViewWindow.Description = null;
		}
	}

	public override bool ProcessKeyEvent(char ch)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected I4, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if (ch == ':')
		{
			string value = string.Empty;
			if (endOffset - startOffset > 0)
			{
				value = ((document.TextLength < startOffset) ? string.Empty : (document.GetText(startOffset, endOffset - startOffset) + ":"));
			}
			if (!string.IsNullOrEmpty(value))
			{
				ICompletionData[] array = filteredCompletionData;
				foreach (ICompletionData val in array)
				{
					if (val == null)
					{
						break;
					}
					bool fullWordTyped;
					if (val.Text.Equals(value, StringComparison.InvariantCultureIgnoreCase) && val is ClaCodeCompletionData && ((ClaCodeCompletionData)(object)val).IsPre)
					{
						return InsertSelectedItem(val, ch, out fullWordTyped);
					}
				}
			}
		}
		CompletionDataProviderKeyResult val2 = dataProvider.ProcessKey(ch);
		switch ((int)val2)
		{
		case 2:
			startOffset++;
			endOffset++;
			return ((AbstractCompletionWindow)this).ProcessKeyEvent(ch);
		case 0:
			return ((AbstractCompletionWindow)this).ProcessKeyEvent(ch);
		case 1:
			if ((options.Flags & 4) != 0)
			{
				return InsertSelectedItem(ch);
			}
			((Form)this).Close();
			return false;
		default:
			throw new InvalidOperationException("Invalid return value of dataProvider.ProcessKey");
		}
	}

	private void DocumentAboutToBeChanged(object sender, DocumentEventArgs e)
	{
		if (e.Offset < startOffset || e.Offset > endOffset)
		{
			return;
		}
		if (e.Length > 0)
		{
			endOffset -= e.Length;
			if (endOffset < startOffset)
			{
				endOffset = startOffset;
			}
		}
		if (!string.IsNullOrEmpty(e.Text))
		{
			endOffset += e.Text.Length;
		}
	}

	private void DocumentChanged(object sender, DocumentEventArgs e)
	{
		FilterData();
	}

	protected override void CaretOffsetChanged(object sender, EventArgs e)
	{
		int offset = ((TextEditorControlBase)base.control).ActiveTextAreaControl.Caret.Offset;
		if (offset != startOffset)
		{
			if (offset < startOffset || offset > endOffset)
			{
				((Form)this).Close();
			}
			else
			{
				codeCompletionListView.SelectItemWithStart((document.TextLength < startOffset) ? string.Empty : document.GetText(startOffset, offset - startOffset));
			}
		}
	}

	protected override bool ProcessTextAreaKey(Keys keyData)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (!((Control)this).Visible)
		{
			return false;
		}
		switch (keyData)
		{
		case Keys.Home:
			codeCompletionListView.SelectIndex(0);
			return true;
		case Keys.End:
			codeCompletionListView.SelectIndex(filteredCompletionDataLength - 1);
			return true;
		case Keys.Next:
			codeCompletionListView.PageDown();
			return true;
		case Keys.Prior:
			codeCompletionListView.PageUp();
			return true;
		case Keys.Down:
			codeCompletionListView.SelectNextItem();
			return true;
		case Keys.Up:
			codeCompletionListView.SelectPrevItem();
			return true;
		case Keys.Tab:
			InsertSelectedItem('\t');
			return true;
		case Keys.Return:
		{
			InsertSelectedItem('\n', out var fullWordTyped);
			if ((options.Flags & 2) == 0)
			{
				return true;
			}
			return !fullWordTyped;
		}
		default:
			return ((AbstractCompletionWindow)this).ProcessTextAreaKey(keyData);
		}
	}

	private void CodeCompletionListViewDoubleClick(object sender, EventArgs e)
	{
		InsertSelectedItem('\0');
	}

	private void CodeCompletionListViewClick(object sender, EventArgs e)
	{
		((Control)(object)((TextEditorControlBase)base.control).ActiveTextAreaControl.TextArea).Focus();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			RemoveDocumentEvents();
			if (codeCompletionListView != null)
			{
				((Component)(object)codeCompletionListView).Dispose();
				codeCompletionListView = null;
			}
			if (declarationViewWindow != null)
			{
				((Component)(object)declarationViewWindow).Dispose();
				declarationViewWindow = null;
			}
			Array.Clear(completionData, 0, completionData.Length);
			Array.Clear(filteredCompletionData, 0, completionData.Length);
		}
		((Form)this).Dispose(disposing);
	}

	protected bool InsertSelectedItem(char ch)
	{
		bool fullWordTyped;
		return InsertSelectedItem(ch, out fullWordTyped);
	}

	protected bool InsertSelectedItem(char ch, out bool fullWordTyped)
	{
		ICompletionData selectedCompletionData = codeCompletionListView.SelectedCompletionData;
		return InsertSelectedItem(selectedCompletionData, ch, out fullWordTyped);
	}

	protected virtual bool InsertSelectedItem(ICompletionData data, char ch, out bool fullWordTyped)
	{
		RemoveDocumentEvents();
		bool result = false;
		fullWordTyped = true;
		if (data != null)
		{
			((TextEditorControlBase)base.control).BeginUpdate();
			string text = string.Empty;
			string value = string.Empty;
			try
			{
				if (endOffset - startOffset > 0)
				{
					text = ((document.TextLength < startOffset) ? string.Empty : document.GetText(startOffset, endOffset - startOffset));
					((TextEditorControlBase)base.control).Document.Remove(startOffset, endOffset - startOffset);
				}
				result = dataProvider.InsertAction(data, ((TextEditorControlBase)base.control).ActiveTextAreaControl.TextArea, startOffset, ch);
				int offset = ((TextEditorControlBase)base.control).ActiveTextAreaControl.Caret.Offset;
				if (offset - startOffset > 0)
				{
					value = ((TextEditorControlBase)base.control).Document.GetText(startOffset, offset - startOffset);
				}
			}
			finally
			{
				fullWordTyped = text.Equals(value, StringComparison.InvariantCultureIgnoreCase);
				((TextEditorControlBase)base.control).EndUpdate();
			}
		}
		((Form)this).Close();
		return result;
	}

	protected virtual Size GetRealSize()
	{
		int num = options.PreferredWidth;
		if (num < ((Control)(object)this).MinimumSize.Width)
		{
			num = ((Control)(object)this).MinimumSize.Width;
		}
		Size clientSize = new Size(num, codeCompletionListView.ItemHeight * Math.Min(options.MaxVisibleItems, (filteredCompletionDataLength < 1) ? 1 : filteredCompletionDataLength));
		return ((Control)(object)this).SizeFromClientSize(clientSize);
	}

	protected void RemoveDocumentEvents()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		document.DocumentAboutToBeChanged -= new DocumentEventHandler(DocumentAboutToBeChanged);
		document.DocumentChanged -= new DocumentEventHandler(DocumentChanged);
	}
}
