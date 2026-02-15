using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace SoftVelocity.Generator.UI.Controls;

public class TabStrip : ToolStrip
{
	private TabStripRenderer myRenderer = new TabStripRenderer();

	protected TabStripButton mySelTab;

	private DesignerVerb insPage;

	public override ISite Site
	{
		get
		{
			ISite site = base.Site;
			if (site != null && site.DesignMode)
			{
				IContainer container = site.Container;
				if (container != null && container is IDesignerHost designerHost)
				{
					IDesigner designer = designerHost.GetDesigner(site.Component);
					if (designer != null && !designer.Verbs.Contains(insPage))
					{
						designer.Verbs.Add(insPage);
					}
				}
			}
			return site;
		}
		set
		{
			base.Site = value;
		}
	}

	public new ToolStripRenderer Renderer
	{
		get
		{
			return myRenderer;
		}
		set
		{
			base.Renderer = myRenderer;
		}
	}

	public new ToolStripLayoutStyle LayoutStyle
	{
		get
		{
			return base.LayoutStyle;
		}
		set
		{
			switch (value)
			{
			case ToolStripLayoutStyle.StackWithOverflow:
			case ToolStripLayoutStyle.HorizontalStackWithOverflow:
			case ToolStripLayoutStyle.VerticalStackWithOverflow:
				base.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;
				break;
			case ToolStripLayoutStyle.Table:
				base.LayoutStyle = ToolStripLayoutStyle.Table;
				break;
			case ToolStripLayoutStyle.Flow:
				base.LayoutStyle = ToolStripLayoutStyle.Flow;
				break;
			default:
				base.LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;
				break;
			}
		}
	}

	[Obsolete("Use RenderStyle instead")]
	[Browsable(false)]
	public new ToolStripRenderMode RenderMode
	{
		get
		{
			return base.RenderMode;
		}
		set
		{
			RenderStyle = value;
		}
	}

	[Category("Appearance")]
	[Description("Gets or sets render style for TabStrip. You should use this property instead of RenderMode.")]
	public ToolStripRenderMode RenderStyle
	{
		get
		{
			return myRenderer.RenderMode;
		}
		set
		{
			myRenderer.RenderMode = value;
			Invalidate();
		}
	}

	protected override Padding DefaultPadding => Padding.Empty;

	[Browsable(false)]
	public new Padding Padding
	{
		get
		{
			return DefaultPadding;
		}
		set
		{
		}
	}

	[Category("Appearance")]
	[Description("Specifies if TabStrip should use system visual styles for painting items")]
	public bool UseVisualStyles
	{
		get
		{
			return myRenderer.UseVS;
		}
		set
		{
			myRenderer.UseVS = value;
			Invalidate();
		}
	}

	[Category("Appearance")]
	[Description("Specifies if TabButtons should be drawn flipped (for right- and bottom-aligned TabStrips)")]
	public bool FlipButtons
	{
		get
		{
			return myRenderer.Mirrored;
		}
		set
		{
			myRenderer.Mirrored = value;
			Invalidate();
		}
	}

	public TabStripButton SelectedTab
	{
		get
		{
			return mySelTab;
		}
		set
		{
			if (value != null && mySelTab != value)
			{
				if (value.Owner != this)
				{
					throw new ArgumentException("Cannot select TabButtons that do not belong to this TabStrip");
				}
				OnItemClicked(new ToolStripItemClickedEventArgs(value));
			}
		}
	}

	public event EventHandler<SelectedTabChangedEventArgs> SelectedTabChanged;

	public TabStrip()
	{
		InitControl();
	}

	public TabStrip(params TabStripButton[] buttons)
		: base(buttons)
	{
		InitControl();
	}

	protected void InitControl()
	{
		base.RenderMode = ToolStripRenderMode.ManagerRenderMode;
		base.Renderer = myRenderer;
		myRenderer.RenderMode = RenderStyle;
		insPage = new DesignerVerb("Insert tab page", OnInsertPageClicked);
	}

	protected void OnInsertPageClicked(object sender, EventArgs e)
	{
		ISite site = base.Site;
		if (site != null && site.DesignMode)
		{
			IContainer container = site.Container;
			if (container != null)
			{
				TabStripButton tabStripButton = new TabStripButton();
				container.Add(tabStripButton);
				tabStripButton.Text = tabStripButton.Name;
			}
		}
	}

	protected void OnTabSelected(TabStripButton tab)
	{
		Invalidate();
		if (this.SelectedTabChanged != null)
		{
			this.SelectedTabChanged(this, new SelectedTabChangedEventArgs(tab));
		}
	}

	protected override void OnItemAdded(ToolStripItemEventArgs e)
	{
		base.OnItemAdded(e);
		if (e.Item is TabStripButton)
		{
			SelectedTab = (TabStripButton)e.Item;
		}
	}

	protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
	{
		if (e.ClickedItem is TabStripButton tab)
		{
			SuspendLayout();
			mySelTab = tab;
			ResumeLayout();
			OnTabSelected(tab);
		}
		base.OnItemClicked(e);
	}
}
