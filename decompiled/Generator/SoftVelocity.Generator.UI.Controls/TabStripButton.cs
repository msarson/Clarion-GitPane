using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SoftVelocity.Generator.UI.Controls;

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
public class TabStripButton : ToolStripButton
{
	private Color m_HotTextColor = Control.DefaultForeColor;

	private Color m_SelectedTextColor = Control.DefaultForeColor;

	private Font m_SelectedFont;

	protected override Padding DefaultMargin => new Padding(0);

	[Browsable(false)]
	public new Padding Margin
	{
		get
		{
			return base.Margin;
		}
		set
		{
		}
	}

	[Browsable(false)]
	public new Padding Padding
	{
		get
		{
			return base.Padding;
		}
		set
		{
		}
	}

	[Description("Text color when TabButton is highlighted")]
	[Category("Appearance")]
	public Color HotTextColor
	{
		get
		{
			return m_HotTextColor;
		}
		set
		{
			m_HotTextColor = value;
		}
	}

	[Description("Text color when TabButton is selected")]
	[Category("Appearance")]
	public Color SelectedTextColor
	{
		get
		{
			return m_SelectedTextColor;
		}
		set
		{
			m_SelectedTextColor = value;
		}
	}

	[Description("Font when TabButton is selected")]
	[Category("Appearance")]
	public Font SelectedFont
	{
		get
		{
			if (m_SelectedFont != null)
			{
				return m_SelectedFont;
			}
			return Font;
		}
		set
		{
			m_SelectedFont = value;
		}
	}

	[Browsable(false)]
	[DefaultValue(false)]
	public new bool Checked
	{
		get
		{
			return IsSelected;
		}
		set
		{
		}
	}

	[Browsable(false)]
	public bool IsSelected
	{
		get
		{
			if (base.Owner is TabStrip tabStrip)
			{
				return this == tabStrip.SelectedTab;
			}
			return false;
		}
		set
		{
			if (value && base.Owner is TabStrip tabStrip)
			{
				tabStrip.SelectedTab = this;
			}
		}
	}

	public TabStripButton()
	{
		InitButton();
	}

	public TabStripButton(Image image)
		: base(image)
	{
		InitButton();
	}

	public TabStripButton(string text)
		: base(text)
	{
		InitButton();
	}

	public TabStripButton(string text, Image image)
		: base(text, image)
	{
		InitButton();
	}

	public TabStripButton(string Text, Image Image, EventHandler Handler)
		: base(Text, Image, Handler)
	{
		InitButton();
	}

	public TabStripButton(string Text, Image Image, EventHandler Handler, string name)
		: base(Text, Image, Handler, name)
	{
		InitButton();
	}

	private void InitButton()
	{
		m_SelectedFont = Font;
	}

	public override Size GetPreferredSize(Size constrainingSize)
	{
		Size preferredSize = base.GetPreferredSize(constrainingSize);
		if (base.Owner != null && base.Owner.Orientation == Orientation.Vertical)
		{
			preferredSize.Width += 3;
			preferredSize.Height += 10;
		}
		return preferredSize;
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		if (base.Owner != null && !(base.Owner is TabStrip))
		{
			throw new Exception("Cannot add TabStripButton to " + base.Owner.GetType().Name);
		}
		base.OnOwnerChanged(e);
	}
}
