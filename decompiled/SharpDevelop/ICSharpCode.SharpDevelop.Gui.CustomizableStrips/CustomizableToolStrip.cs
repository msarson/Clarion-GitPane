using System;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public class CustomizableToolStrip : ToolStrip
{
	private AppearanceControl _Appearance;

	private bool _RoundedEdges;

	public AppearanceControl Appearance
	{
		get
		{
			return _Appearance;
		}
		set
		{
			_Appearance = value;
			if (value != null)
			{
				base.Renderer = value.Renderer;
			}
			Invalidate();
			OnAppearanceControlChanged(EventArgs.Empty);
		}
	}

	public bool RoundedEdges
	{
		get
		{
			return _RoundedEdges;
		}
		set
		{
			_RoundedEdges = value;
		}
	}

	public event EventHandler AppearanceControlChanged;

	public CustomizableToolStrip()
	{
		RoundedEdges = true;
	}

	private void AppearanceControl_AppearanceChanged(object sender, EventArgs e)
	{
		base.Renderer = Appearance.Renderer;
		((ToolStripProfessionalRenderer)base.Renderer).RoundedEdges = _RoundedEdges;
		Invalidate();
	}

	private void AppearanceControl_Disposed(object sender, EventArgs e)
	{
		Appearance = null;
		OnAppearanceControlChanged(EventArgs.Empty);
	}

	protected virtual void OnAppearanceControlChanged(EventArgs e)
	{
		if (Appearance != null)
		{
			Appearance.AppearanceChanged += AppearanceControl_AppearanceChanged;
			Appearance.Disposed += AppearanceControl_Disposed;
			base.Renderer = Appearance.Renderer;
		}
		else
		{
			base.Renderer = new ToolStripProfessionalRenderer();
		}
		Invalidate();
		if (this.AppearanceControlChanged != null)
		{
			this.AppearanceControlChanged(this, e);
		}
	}
}
