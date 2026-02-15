using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace SoftVelocity.Common.CodeCompletion;

internal class ClaCodeCompletionListView : CodeCompletionListView
{
	private int declDrawPos = -1;

	public int DeclDrawPosition
	{
		get
		{
			if (declDrawPos < 0)
			{
				return ((CodeCompletionListView)this).ItemHeight * 10;
			}
			return declDrawPos;
		}
		set
		{
			declDrawPos = value;
		}
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		float num = 1f;
		float num2 = ((CodeCompletionListView)this).ItemHeight;
		int num3 = (int)(num2 * (float)((CodeCompletionListView)this).ImageList.ImageSize.Width / (float)((CodeCompletionListView)this).ImageList.ImageSize.Height);
		int i = ((CodeCompletionListView)this).FirstItem;
		Graphics graphics = pe.Graphics;
		if (base.completionData != null && base.completionDataLength > 0)
		{
			for (; i < base.completionDataLength; i++)
			{
				if (!(num < (float)((Control)this).Height))
				{
					break;
				}
				RectangleF rect = new RectangleF(1f, num, ((Control)this).Width - 2, num2);
				if (rect.IntersectsWith(pe.ClipRectangle))
				{
					if (i == base.selectedItem)
					{
						graphics.FillRectangle(SystemBrushes.Highlight, rect);
					}
					else
					{
						graphics.FillRectangle(SystemBrushes.Window, rect);
					}
					int num4 = 0;
					if (((CodeCompletionListView)this).ImageList != null && base.completionData[i].ImageIndex < ((CodeCompletionListView)this).ImageList.Images.Count)
					{
						graphics.DrawImage(((CodeCompletionListView)this).ImageList.Images[base.completionData[i].ImageIndex], new RectangleF(1f, num, num3, num2));
						num4 = num3;
					}
					StringFormat genericDefault = StringFormat.GenericDefault;
					genericDefault.FormatFlags |= StringFormatFlags.NoWrap;
					if (i == base.selectedItem)
					{
						if (base.completionData[i] is ClaCodeCompletionData)
						{
							graphics.DrawString(base.completionData[i].Text, ((Control)(object)this).Font, SystemBrushes.HighlightText, new RectangleF(num4, num, DeclDrawPosition - 2, num2), genericDefault);
							graphics.DrawString(((ClaCodeCompletionData)(object)base.completionData[i]).DeclText, ((Control)(object)this).Font, SystemBrushes.HighlightText, DeclDrawPosition, num);
						}
						else
						{
							graphics.DrawString(base.completionData[i].Text, ((Control)(object)this).Font, SystemBrushes.HighlightText, num4, num);
						}
					}
					else if (base.completionData[i] is ClaCodeCompletionData)
					{
						graphics.DrawString(base.completionData[i].Text, ((Control)(object)this).Font, SystemBrushes.WindowText, new RectangleF(num4, num, DeclDrawPosition - 2, num2), genericDefault);
						graphics.DrawString(((ClaCodeCompletionData)(object)base.completionData[i]).DeclText, ((Control)(object)this).Font, SystemBrushes.WindowText, DeclDrawPosition, num);
					}
					else
					{
						graphics.DrawString(base.completionData[i].Text, ((Control)(object)this).Font, SystemBrushes.WindowText, num4, num);
					}
				}
				num += num2;
			}
			graphics.DrawRectangle(SystemPens.ControlDark, new Rectangle(0, 0, ((Control)this).Width - 1, ((Control)this).Height - 1));
		}
		else
		{
			RectangleF rect2 = new RectangleF(1f, num, ((Control)this).Width - 2, num2);
			if (rect2.IntersectsWith(pe.ClipRectangle))
			{
				graphics.FillRectangle(SystemBrushes.Window, rect2);
				graphics.DrawString("No suggestions", ((Control)(object)this).Font, SystemBrushes.WindowText, 1f, num);
			}
			graphics.DrawRectangle(SystemPens.ControlDark, new Rectangle(0, 0, ((Control)this).Width - 1, ((Control)this).Height - 1));
		}
	}
}
