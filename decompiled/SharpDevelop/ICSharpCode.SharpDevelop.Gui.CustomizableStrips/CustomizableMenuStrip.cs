using System;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public class CustomizableMenuStrip : MenuStrip
{
	private AppearanceControl _Appearance;

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

	public event EventHandler AppearanceControlChanged;

	private void AppearanceControl_AppearanceChanged(object sender, EventArgs e)
	{
		base.Renderer = Appearance.Renderer;
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
