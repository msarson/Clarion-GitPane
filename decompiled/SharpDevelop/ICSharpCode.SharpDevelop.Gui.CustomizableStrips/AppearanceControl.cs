using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public class AppearanceControl : Component, IAppearanceControl
{
	private AppearanceProperties _AppearanceProperties;

	private ToolStripProfessionalRenderer _Renderer;

	private string _xmlFile;

	[Editor(typeof(CustomAppearancePropertyEditor), typeof(UITypeEditor))]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public AppearanceProperties AppearanceProperties
	{
		get
		{
			return _AppearanceProperties;
		}
		set
		{
			_AppearanceProperties = value;
			_Renderer = new ToolStripProfessionalRenderer(new CustomColorTable(_AppearanceProperties));
			_AppearanceProperties.SetAppearanceControl(this);
			OnAppearanceChanged(EventArgs.Empty);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Browsable(false)]
	public ToolStripProfessionalRenderer Renderer => _Renderer;

	public event EventHandler AppearanceChanged;

	public AppearanceControl()
	{
		_AppearanceProperties = new AppearanceProperties(this);
		_Renderer = new ToolStripProfessionalRenderer(new CustomColorTable(_AppearanceProperties));
	}

	public AppearanceControl(string fileName)
		: this()
	{
		LoadAppearanceProperties(fileName);
	}

	public virtual void OnAppearanceChanged(EventArgs e)
	{
		if (this.AppearanceChanged != null)
		{
			this.AppearanceChanged(this, e);
		}
	}

	public bool SaveAppearanceProperties()
	{
		if (_xmlFile != null)
		{
			return SaveAppearanceProperties(_xmlFile);
		}
		return false;
	}

	public bool SaveAppearanceProperties(string xmlFile)
	{
		_xmlFile = xmlFile;
		return AppearanceProperties.Save(xmlFile, AppearanceProperties);
	}

	public bool LoadAppearanceProperties(string xmlFile)
	{
		_xmlFile = xmlFile;
		if (AppearanceProperties.Load(xmlFile, out var appearanceProperties))
		{
			AppearanceProperties = appearanceProperties;
			return true;
		}
		return false;
	}
}
