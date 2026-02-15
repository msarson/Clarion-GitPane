using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SoftVelocity.Controls;

namespace SoftVelocity.Generator.UI;

internal class AppHeaderLabel : GradientLabel
{
	private Stack<string> _titleStack = new Stack<string>();

	private string _HeaderTitleSeparation = " - ";

	public string HeaderTitle
	{
		get
		{
			if (_titleStack.Count > 0)
			{
				return _titleStack.Peek();
			}
			return "";
		}
	}

	public string HeaderTitleSeparation
	{
		get
		{
			return _HeaderTitleSeparation;
		}
		set
		{
			_HeaderTitleSeparation = value;
		}
	}

	internal AppHeaderLabel()
	{
		base.BackColorGradientBegin = Color.Azure;
		base.BackColorGradientEnd = Color.LightBlue;
		base.GradientMode = LinearGradientMode.Vertical;
		Dock = DockStyle.Top;
		Font = new Font("Verdana", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
		ForeColor = Color.Black;
		base.Location = new Point(0, 0);
		base.Name = "_AppHeaderLabel";
		base.Padding = new Padding(5, 2, 0, 0);
		base.Size = new Size(717, 22);
		base.TabIndex = 3;
		Text = "Application Editor";
		base.UseProfessionalColorTable = true;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			base.Parent.Controls.Remove(this);
			base.Parent = null;
		}
		base.Dispose(disposing);
	}

	private void RefreshHeaderTitle()
	{
		Text = HeaderTitle;
	}

	public void RemoveCurrentHeaderTitle()
	{
		if (_titleStack.Count > 0)
		{
			_titleStack.Pop();
		}
		RefreshHeaderTitle();
	}

	public void SetHeaderTitle(string title)
	{
		_titleStack.Push(title);
		RefreshHeaderTitle();
	}

	public void ReplaceHeaderTitle(string title)
	{
		if (_titleStack.Count > 0)
		{
			_titleStack.Pop();
		}
		SetHeaderTitle(title);
	}

	public void AppendHeaderTitle(string title)
	{
		if (_titleStack.Count > 0)
		{
			_titleStack.Push(_titleStack.Peek() + HeaderTitleSeparation + title);
		}
		else
		{
			_titleStack.Push(title);
		}
		RefreshHeaderTitle();
	}
}
