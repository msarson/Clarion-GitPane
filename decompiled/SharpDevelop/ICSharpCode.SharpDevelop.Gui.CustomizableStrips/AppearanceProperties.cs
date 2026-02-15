using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using ZetaColorEditor.PropertyGridEditors;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

[Serializable]
public class AppearanceProperties
{
	[Serializable]
	public class ButtonAppearanceProperties
	{
		private CheckedButtonAppearanceProperties _CheckedAppearance;

		private PressedButtonAppearanceProperties _PressedAppearance;

		private SelectedButtonAppearanceProperties _SelectedAppearance;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CheckedButtonAppearanceProperties CheckedAppearance
		{
			get
			{
				return _CheckedAppearance;
			}
			set
			{
				_CheckedAppearance = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public PressedButtonAppearanceProperties PressedAppearance
		{
			get
			{
				return _PressedAppearance;
			}
			set
			{
				_PressedAppearance = value;
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SelectedButtonAppearanceProperties SelectedAppearance
		{
			get
			{
				return _SelectedAppearance;
			}
			set
			{
				_SelectedAppearance = value;
			}
		}

		public ButtonAppearanceProperties()
		{
		}

		public ButtonAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_SelectedAppearance = new SelectedButtonAppearanceProperties(appearanceControl);
			_CheckedAppearance = new CheckedButtonAppearanceProperties(appearanceControl);
			_PressedAppearance = new PressedButtonAppearanceProperties(appearanceControl);
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class CheckedButtonAppearanceProperties
	{
		private Color _Background;

		private Color _BorderHighlight;

		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _GradientMiddle;

		private Color _Highlight;

		private Color _PressedBackrgound;

		private Color _SelectedBackground;

		private IAppearanceControl ap;

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "255, 192, 111")]
		public Color Background
		{
			get
			{
				return _Background;
			}
			set
			{
				_Background = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "51, 94, 168")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color BorderHighlight
		{
			get
			{
				return _BorderHighlight;
			}
			set
			{
				_BorderHighlight = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "255, 223, 154")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "255, 166, 76")]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "255, 195, 116")]
		[XmlIgnore]
		public Color GradientMiddle
		{
			get
			{
				return _GradientMiddle;
			}
			set
			{
				_GradientMiddle = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 208, 229")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		public Color Highlight
		{
			get
			{
				return _Highlight;
			}
			set
			{
				_Highlight = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBackground
		{
			get
			{
				return ColorToString(Background);
			}
			set
			{
				Background = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xBorderHighlight
		{
			get
			{
				return ColorToString(BorderHighlight);
			}
			set
			{
				BorderHighlight = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientMiddle
		{
			get
			{
				return ColorToString(GradientMiddle);
			}
			set
			{
				GradientMiddle = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xHighlight
		{
			get
			{
				return ColorToString(Highlight);
			}
			set
			{
				Highlight = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xPressedBackground
		{
			get
			{
				return ColorToString(PressedBackground);
			}
			set
			{
				PressedBackground = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xSelectedBackground
		{
			get
			{
				return ColorToString(SelectedBackground);
			}
			set
			{
				SelectedBackground = ColorFromString(value);
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "254, 128, 62")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color PressedBackground
		{
			get
			{
				return _PressedBackrgound;
			}
			set
			{
				_PressedBackrgound = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "254, 128, 62")]
		public Color SelectedBackground
		{
			get
			{
				return _SelectedBackground;
			}
			set
			{
				_SelectedBackground = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		public CheckedButtonAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(255, 223, 154);
			_GradientMiddle = Color.FromArgb(255, 195, 116);
			_GradientEnd = Color.FromArgb(255, 166, 76);
			_Highlight = Color.FromArgb(196, 208, 229);
			_BorderHighlight = Color.FromArgb(51, 94, 168);
			_Background = Color.FromArgb(255, 192, 111);
			_SelectedBackground = Color.FromArgb(254, 128, 62);
			_PressedBackrgound = Color.FromArgb(254, 128, 62);
		}

		public CheckedButtonAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(255, 223, 154);
			_GradientMiddle = Color.FromArgb(255, 195, 116);
			_GradientEnd = Color.FromArgb(255, 166, 76);
			_Highlight = Color.FromArgb(196, 208, 229);
			_BorderHighlight = Color.FromArgb(51, 94, 168);
			_Background = Color.FromArgb(255, 192, 111);
			_SelectedBackground = Color.FromArgb(254, 128, 62);
			_PressedBackrgound = Color.FromArgb(254, 128, 62);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class GripAppearanceProperties
	{
		private Color _Dark;

		private Color _Light;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "39, 65, 118")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color Dark
		{
			get
			{
				return _Dark;
			}
			set
			{
				_Dark = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xDark
		{
			get
			{
				return ColorToString(Dark);
			}
			set
			{
				Dark = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xLight
		{
			get
			{
				return ColorToString(Light);
			}
			set
			{
				Light = ColorFromString(value);
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "255, 255, 255")]
		public Color Light
		{
			get
			{
				return _Light;
			}
			set
			{
				_Light = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		public GripAppearanceProperties()
		{
			_Dark = Color.FromArgb(39, 65, 118);
			_Light = Color.FromArgb(255, 255, 255);
		}

		public GripAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_Dark = Color.FromArgb(39, 65, 118);
			_Light = Color.FromArgb(255, 255, 255);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class ImageMarginAppearanceProperties
	{
		private ImageMarginNormalAppearanceProperties _Normal;

		private ImageMarginRevealedAppearanceProperties _Revealed;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public ImageMarginNormalAppearanceProperties Normal
		{
			get
			{
				return _Normal;
			}
			set
			{
				_Normal = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public ImageMarginRevealedAppearanceProperties Revealed
		{
			get
			{
				return _Revealed;
			}
			set
			{
				_Revealed = value;
			}
		}

		public ImageMarginAppearanceProperties()
		{
		}

		public ImageMarginAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_Normal = new ImageMarginNormalAppearanceProperties(appearanceControl);
			_Revealed = new ImageMarginRevealedAppearanceProperties(appearanceControl);
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class ImageMarginNormalAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _GradientMiddle;

		private IAppearanceControl ap;

		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "227, 239, 255")]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "123, 164, 224")]
		[XmlIgnore]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "203, 225, 252")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientMiddle
		{
			get
			{
				return _GradientMiddle;
			}
			set
			{
				_GradientMiddle = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientMiddle
		{
			get
			{
				return ColorToString(GradientMiddle);
			}
			set
			{
				GradientMiddle = ColorFromString(value);
			}
		}

		public ImageMarginNormalAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(227, 239, 255);
			_GradientMiddle = Color.FromArgb(203, 225, 252);
			_GradientEnd = Color.FromArgb(123, 164, 224);
		}

		public ImageMarginNormalAppearanceProperties(IAppearanceControl appearanceControl)
			: this()
		{
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class ImageMarginRevealedAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _GradientMiddle;

		private IAppearanceControl ap;

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "203, 221, 246")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "114, 155, 215")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[DefaultValue(typeof(Color), "161, 197, 249")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientMiddle
		{
			get
			{
				return _GradientMiddle;
			}
			set
			{
				_GradientMiddle = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientMiddle
		{
			get
			{
				return ColorToString(GradientMiddle);
			}
			set
			{
				GradientMiddle = ColorFromString(value);
			}
		}

		public ImageMarginRevealedAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(203, 221, 246);
			_GradientMiddle = Color.FromArgb(161, 197, 249);
			_GradientEnd = Color.FromArgb(114, 155, 215);
		}

		public ImageMarginRevealedAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(203, 221, 246);
			_GradientMiddle = Color.FromArgb(161, 197, 249);
			_GradientEnd = Color.FromArgb(114, 155, 215);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class MenuItemAppearanceProperties
	{
		private Color _Border;

		private Color _PressedGradientBegin;

		private Color _PressedGradientEnd;

		private Color _PressedGradientMiddle;

		private Color _Selected;

		private Color _SelectedGradientBegin;

		private Color _SelectedGradientEnd;

		private IAppearanceControl ap;

		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "0, 0, 128")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color Border
		{
			get
			{
				return _Border;
			}
			set
			{
				_Border = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBorder
		{
			get
			{
				return ColorToString(Border);
			}
			set
			{
				Border = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xPressedGradientBegin
		{
			get
			{
				return ColorToString(PressedGradientBegin);
			}
			set
			{
				PressedGradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xPressedGradientEnd
		{
			get
			{
				return ColorToString(PressedGradientEnd);
			}
			set
			{
				PressedGradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xPressedGradientMiddle
		{
			get
			{
				return ColorToString(PressedGradientMiddle);
			}
			set
			{
				PressedGradientMiddle = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xSelected
		{
			get
			{
				return ColorToString(Selected);
			}
			set
			{
				Selected = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xSelectedGradientBegin
		{
			get
			{
				return ColorToString(SelectedGradientBegin);
			}
			set
			{
				SelectedGradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xSelectedGradientEnd
		{
			get
			{
				return ColorToString(SelectedGradientEnd);
			}
			set
			{
				SelectedGradientEnd = ColorFromString(value);
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "227, 239, 255")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color PressedGradientBegin
		{
			get
			{
				return _PressedGradientBegin;
			}
			set
			{
				_PressedGradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "123, 164, 224")]
		public Color PressedGradientEnd
		{
			get
			{
				return _PressedGradientEnd;
			}
			set
			{
				_PressedGradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "161, 197, 249")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		public Color PressedGradientMiddle
		{
			get
			{
				return _PressedGradientMiddle;
			}
			set
			{
				_PressedGradientMiddle = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "255, 238, 194")]
		public Color Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "255, 255, 222")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		public Color SelectedGradientBegin
		{
			get
			{
				return _SelectedGradientBegin;
			}
			set
			{
				_SelectedGradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "255, 203, 136")]
		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color SelectedGradientEnd
		{
			get
			{
				return _SelectedGradientEnd;
			}
			set
			{
				_SelectedGradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		public MenuItemAppearanceProperties()
		{
			_Selected = Color.FromArgb(255, 238, 194);
			_Border = Color.FromArgb(0, 0, 128);
			_SelectedGradientBegin = Color.FromArgb(255, 255, 222);
			_SelectedGradientEnd = Color.FromArgb(255, 203, 136);
			_PressedGradientBegin = Color.FromArgb(227, 239, 255);
			_PressedGradientMiddle = Color.FromArgb(161, 197, 249);
			_PressedGradientEnd = Color.FromArgb(123, 164, 224);
		}

		public MenuItemAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_Selected = Color.FromArgb(255, 238, 194);
			_Border = Color.FromArgb(0, 0, 128);
			_SelectedGradientBegin = Color.FromArgb(255, 255, 222);
			_SelectedGradientEnd = Color.FromArgb(255, 203, 136);
			_PressedGradientBegin = Color.FromArgb(227, 239, 255);
			_PressedGradientMiddle = Color.FromArgb(161, 197, 249);
			_PressedGradientEnd = Color.FromArgb(123, 164, 224);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class MenustripAppearanceProperties
	{
		private Color _Border;

		private Color _GradientBegin;

		private Color _GradientEnd;

		private IAppearanceControl ap;

		[DefaultValue(typeof(Color), "0, 45, 150")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		public Color Border
		{
			get
			{
				return _Border;
			}
			set
			{
				_Border = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBorder
		{
			get
			{
				return ColorToString(Border);
			}
			set
			{
				Border = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		public MenustripAppearanceProperties()
		{
			_Border = Color.FromArgb(0, 45, 150);
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
		}

		public MenustripAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_Border = Color.FromArgb(0, 45, 150);
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class OverflowButtonAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _GradientMiddle;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "127, 177, 250")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "0, 53, 145")]
		[XmlIgnore]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[DefaultValue(typeof(Color), "82, 127, 208")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientMiddle
		{
			get
			{
				return _GradientMiddle;
			}
			set
			{
				_GradientMiddle = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientMiddle
		{
			get
			{
				return ColorToString(GradientMiddle);
			}
			set
			{
				GradientMiddle = ColorFromString(value);
			}
		}

		public OverflowButtonAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(127, 177, 250);
			_GradientMiddle = Color.FromArgb(82, 127, 208);
			_GradientEnd = Color.FromArgb(0, 53, 145);
		}

		public OverflowButtonAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(127, 177, 250);
			_GradientMiddle = Color.FromArgb(82, 127, 208);
			_GradientEnd = Color.FromArgb(0, 53, 145);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class PressedButtonAppearanceProperties
	{
		private Color _Border;

		private Color _BorderHighlight;

		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _GradientMiddle;

		private Color _Highlight;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "0, 0, 128")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color Border
		{
			get
			{
				return _Border;
			}
			set
			{
				_Border = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "51, 94, 168")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color BorderHighlight
		{
			get
			{
				return _BorderHighlight;
			}
			set
			{
				_BorderHighlight = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "254, 128, 62")]
		[XmlIgnore]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[DefaultValue(typeof(Color), "255, 223, 154")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "255, 177, 109")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientMiddle
		{
			get
			{
				return _GradientMiddle;
			}
			set
			{
				_GradientMiddle = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "152, 173, 210")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color Highlight
		{
			get
			{
				return _Highlight;
			}
			set
			{
				_Highlight = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBorder
		{
			get
			{
				return ColorToString(Border);
			}
			set
			{
				Border = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xBorderHighlight
		{
			get
			{
				return ColorToString(BorderHighlight);
			}
			set
			{
				BorderHighlight = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientMiddle
		{
			get
			{
				return ColorToString(GradientMiddle);
			}
			set
			{
				GradientMiddle = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xHighlight
		{
			get
			{
				return ColorToString(Highlight);
			}
			set
			{
				Highlight = ColorFromString(value);
			}
		}

		public PressedButtonAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(254, 128, 62);
			_GradientMiddle = Color.FromArgb(255, 177, 109);
			_GradientEnd = Color.FromArgb(255, 223, 154);
			_Highlight = Color.FromArgb(152, 173, 210);
			_BorderHighlight = Color.FromArgb(51, 94, 168);
			_Border = Color.FromArgb(0, 0, 128);
		}

		public PressedButtonAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(254, 128, 62);
			_GradientMiddle = Color.FromArgb(255, 177, 109);
			_GradientEnd = Color.FromArgb(255, 223, 154);
			_Highlight = Color.FromArgb(152, 173, 210);
			_BorderHighlight = Color.FromArgb(51, 94, 168);
			_Border = Color.FromArgb(0, 0, 128);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class RaftingContainerAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private IAppearanceControl ap;

		[DefaultValue(typeof(Color), "158, 190, 245")]
		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		public RaftingContainerAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
		}

		public RaftingContainerAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class SelectedButtonAppearanceProperties
	{
		private Color _Border;

		private Color _BorderHighlight;

		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _GradientMiddle;

		private Color _Highlight;

		private IAppearanceControl ap;

		[DefaultValue(typeof(Color), "0, 0, 128")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color Border
		{
			get
			{
				return _Border;
			}
			set
			{
				_Border = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "0, 0, 128")]
		public Color BorderHighlight
		{
			get
			{
				return _BorderHighlight;
			}
			set
			{
				_BorderHighlight = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "255, 255, 222")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "255, 203, 136")]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "255, 225, 172")]
		public Color GradientMiddle
		{
			get
			{
				return _GradientMiddle;
			}
			set
			{
				_GradientMiddle = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 208, 229")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color Highlight
		{
			get
			{
				return _Highlight;
			}
			set
			{
				_Highlight = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBorder
		{
			get
			{
				return ColorToString(Border);
			}
			set
			{
				Border = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xBorderHighlight
		{
			get
			{
				return ColorToString(BorderHighlight);
			}
			set
			{
				BorderHighlight = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientMiddle
		{
			get
			{
				return ColorToString(GradientMiddle);
			}
			set
			{
				GradientMiddle = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xHighlight
		{
			get
			{
				return ColorToString(Highlight);
			}
			set
			{
				Highlight = ColorFromString(value);
			}
		}

		public SelectedButtonAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(255, 255, 222);
			_GradientMiddle = Color.FromArgb(255, 225, 172);
			_GradientEnd = Color.FromArgb(255, 203, 136);
			_Highlight = Color.FromArgb(196, 208, 229);
			_BorderHighlight = Color.FromArgb(0, 0, 128);
			_Border = Color.FromArgb(0, 0, 128);
		}

		public SelectedButtonAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(255, 255, 222);
			_GradientMiddle = Color.FromArgb(255, 225, 172);
			_GradientEnd = Color.FromArgb(255, 203, 136);
			_Highlight = Color.FromArgb(196, 208, 229);
			_BorderHighlight = Color.FromArgb(0, 0, 128);
			_Border = Color.FromArgb(0, 0, 128);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class SeparatorAppearanceProperties
	{
		private Color _Dark;

		private Color _Light;

		private IAppearanceControl ap;

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "106, 140, 203")]
		public Color Dark
		{
			get
			{
				return _Dark;
			}
			set
			{
				_Dark = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xDark
		{
			get
			{
				return ColorToString(Dark);
			}
			set
			{
				Dark = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xLight
		{
			get
			{
				return ColorToString(Light);
			}
			set
			{
				Light = ColorFromString(value);
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "241, 249, 255")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color Light
		{
			get
			{
				return _Light;
			}
			set
			{
				_Light = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		public SeparatorAppearanceProperties()
		{
			_Dark = Color.FromArgb(106, 140, 203);
			_Light = Color.FromArgb(241, 249, 255);
		}

		public SeparatorAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_Dark = Color.FromArgb(106, 140, 203);
			_Light = Color.FromArgb(241, 249, 255);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class StatusStripAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		public StatusStripAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
		}

		public StatusStripAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class ToolstripAppearanceProperties
	{
		private Color _Border;

		private Color _ContentPanelGradientBegin;

		private Color _ContentPanelGradientEnd;

		private Color _DropDownBackground;

		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _GradientMiddle;

		private Color _PanelGradientBegin;

		private Color _PanelGradientEnd;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "59, 97, 156")]
		public Color Border
		{
			get
			{
				return _Border;
			}
			set
			{
				_Border = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		public Color ContentPanelGradientBegin
		{
			get
			{
				return _ContentPanelGradientBegin;
			}
			set
			{
				_ContentPanelGradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		public Color ContentPanelGradientEnd
		{
			get
			{
				return _ContentPanelGradientEnd;
			}
			set
			{
				_ContentPanelGradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "246, 246, 246")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color DropDownBackground
		{
			get
			{
				return _DropDownBackground;
			}
			set
			{
				_DropDownBackground = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "227, 239, 255")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "123, 164, 224")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "203, 225, 252")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientMiddle
		{
			get
			{
				return _GradientMiddle;
			}
			set
			{
				_GradientMiddle = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBorder
		{
			get
			{
				return ColorToString(Border);
			}
			set
			{
				Border = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xContentPanelGradientBegin
		{
			get
			{
				return ColorToString(ContentPanelGradientBegin);
			}
			set
			{
				ContentPanelGradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xContentPanelGradientEnd
		{
			get
			{
				return ColorToString(ContentPanelGradientEnd);
			}
			set
			{
				ContentPanelGradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xDropDownBackground
		{
			get
			{
				return ColorToString(DropDownBackground);
			}
			set
			{
				DropDownBackground = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientMiddle
		{
			get
			{
				return ColorToString(GradientMiddle);
			}
			set
			{
				GradientMiddle = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xPanelGradientBegin
		{
			get
			{
				return ColorToString(PanelGradientBegin);
			}
			set
			{
				PanelGradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xPanelGradientEnd
		{
			get
			{
				return ColorToString(PanelGradientEnd);
			}
			set
			{
				PanelGradientEnd = ColorFromString(value);
			}
		}

		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		public Color PanelGradientBegin
		{
			get
			{
				return _PanelGradientBegin;
			}
			set
			{
				_PanelGradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		public Color PanelGradientEnd
		{
			get
			{
				return _PanelGradientEnd;
			}
			set
			{
				_PanelGradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		public ToolstripAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(227, 239, 255);
			_GradientMiddle = Color.FromArgb(203, 225, 252);
			_GradientEnd = Color.FromArgb(123, 164, 224);
			_Border = Color.FromArgb(59, 97, 156);
			_DropDownBackground = Color.FromArgb(246, 246, 246);
			_ContentPanelGradientBegin = Color.FromArgb(158, 190, 245);
			_ContentPanelGradientEnd = Color.FromArgb(196, 218, 250);
			_PanelGradientBegin = Color.FromArgb(158, 190, 245);
			_PanelGradientEnd = Color.FromArgb(196, 218, 250);
		}

		public ToolstripAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(227, 239, 255);
			_GradientMiddle = Color.FromArgb(203, 225, 252);
			_GradientEnd = Color.FromArgb(123, 164, 224);
			_Border = Color.FromArgb(59, 97, 156);
			_DropDownBackground = Color.FromArgb(246, 246, 246);
			_ContentPanelGradientBegin = Color.FromArgb(158, 190, 245);
			_ContentPanelGradientEnd = Color.FromArgb(196, 218, 250);
			_PanelGradientBegin = Color.FromArgb(158, 190, 245);
			_PanelGradientEnd = Color.FromArgb(196, 218, 250);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class DockTabAppearanceProperties
	{
		private DockTabActiveAppearanceProperties _Active;

		private DockTabInactiveAppearanceProperties _Inactive;

		private DockPadTabActiveAppearanceProperties _PadActive;

		private DockPadTabHideAppearanceProperties _PadHide;

		private DockPadTabHideOverAppearanceProperties _PadHideOver;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public DockTabActiveAppearanceProperties Active
		{
			get
			{
				return _Active;
			}
			set
			{
				_Active = value;
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public DockTabInactiveAppearanceProperties Inactive
		{
			get
			{
				return _Inactive;
			}
			set
			{
				_Inactive = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public DockPadTabActiveAppearanceProperties PadActive
		{
			get
			{
				return _PadActive;
			}
			set
			{
				_PadActive = value;
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public DockPadTabHideAppearanceProperties PadHide
		{
			get
			{
				return _PadHide;
			}
			set
			{
				_PadHide = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public DockPadTabHideOverAppearanceProperties PadHideOver
		{
			get
			{
				return _PadHideOver;
			}
			set
			{
				_PadHideOver = value;
			}
		}

		public DockTabAppearanceProperties()
		{
			_Active = new DockTabActiveAppearanceProperties();
			_Inactive = new DockTabInactiveAppearanceProperties();
			_PadActive = new DockPadTabActiveAppearanceProperties();
			_PadHide = new DockPadTabHideAppearanceProperties();
			_PadHideOver = new DockPadTabHideOverAppearanceProperties();
		}

		public DockTabAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_Active = new DockTabActiveAppearanceProperties(appearanceControl);
			_Inactive = new DockTabInactiveAppearanceProperties(appearanceControl);
			_PadActive = new DockPadTabActiveAppearanceProperties(appearanceControl);
			_PadHide = new DockPadTabHideAppearanceProperties(appearanceControl);
			_PadHideOver = new DockPadTabHideOverAppearanceProperties(appearanceControl);
		}

		public override string ToString()
		{
			return string.Empty;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			Active.SetAppearanceControl(appearanceControl);
			Inactive.SetAppearanceControl(appearanceControl);
			PadActive.SetAppearanceControl(appearanceControl);
			PadHide.SetAppearanceControl(appearanceControl);
			PadHideOver.SetAppearanceControl(appearanceControl);
		}
	}

	[Serializable]
	public class DockTabActiveAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _EdgeColor;

		private Color _TextColor;

		private IAppearanceControl ap;

		[DefaultValue(typeof(Color), "158, 190, 245")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color EdgeColor
		{
			get
			{
				return _EdgeColor;
			}
			set
			{
				_EdgeColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "102, 102, 102")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color TextColor
		{
			get
			{
				return _TextColor;
			}
			set
			{
				_TextColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				_GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				_GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xEdgeColor
		{
			get
			{
				return ColorToString(EdgeColor);
			}
			set
			{
				_EdgeColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xTextColor
		{
			get
			{
				return ColorToString(TextColor);
			}
			set
			{
				_TextColor = ColorFromString(value);
			}
		}

		public DockTabActiveAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(102, 102, 102);
		}

		public DockTabActiveAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(102, 102, 102);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class DockTabInactiveAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _EdgeColor;

		private Color _TextColor;

		private IAppearanceControl ap;

		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "109, 109, 109")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color TextColor
		{
			get
			{
				return _TextColor;
			}
			set
			{
				_TextColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		public Color EdgeColor
		{
			get
			{
				return _EdgeColor;
			}
			set
			{
				_EdgeColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xTextColor
		{
			get
			{
				return ColorToString(TextColor);
			}
			set
			{
				TextColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xEdgeColor
		{
			get
			{
				return ColorToString(EdgeColor);
			}
			set
			{
				EdgeColor = ColorFromString(value);
			}
		}

		public DockTabInactiveAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(109, 109, 109);
		}

		public DockTabInactiveAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(109, 109, 109);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class DockPadTabActiveAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _EdgeColor;

		private Color _TextColor;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		public Color EdgeColor
		{
			get
			{
				return _EdgeColor;
			}
			set
			{
				_EdgeColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "102, 102, 102")]
		public Color TextColor
		{
			get
			{
				return _TextColor;
			}
			set
			{
				_TextColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xEdgeColor
		{
			get
			{
				return ColorToString(EdgeColor);
			}
			set
			{
				EdgeColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xTextColor
		{
			get
			{
				return ColorToString(TextColor);
			}
			set
			{
				TextColor = ColorFromString(value);
			}
		}

		public DockPadTabActiveAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(102, 102, 102);
		}

		public DockPadTabActiveAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(102, 102, 102);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class DockPadTabHideAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _EdgeColor;

		private Color _TextColor;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color EdgeColor
		{
			get
			{
				return _EdgeColor;
			}
			set
			{
				_EdgeColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[DefaultValue(typeof(Color), "102, 102, 102")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color TextColor
		{
			get
			{
				return _TextColor;
			}
			set
			{
				_TextColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xEdgeColor
		{
			get
			{
				return ColorToString(EdgeColor);
			}
			set
			{
				EdgeColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xTextColor
		{
			get
			{
				return ColorToString(TextColor);
			}
			set
			{
				TextColor = ColorFromString(value);
			}
		}

		public DockPadTabHideAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(102, 102, 102);
		}

		public DockPadTabHideAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(102, 102, 102);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class DockPadTabHideOverAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _EdgeColor;

		private Color _TextColor;

		private IAppearanceControl ap;

		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color EdgeColor
		{
			get
			{
				return _EdgeColor;
			}
			set
			{
				_EdgeColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "102, 102, 102")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color TextColor
		{
			get
			{
				return _TextColor;
			}
			set
			{
				_TextColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xEdgeColor
		{
			get
			{
				return ColorToString(EdgeColor);
			}
			set
			{
				EdgeColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xTextColor
		{
			get
			{
				return ColorToString(TextColor);
			}
			set
			{
				TextColor = ColorFromString(value);
			}
		}

		public DockPadTabHideOverAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(102, 102, 102);
		}

		public DockPadTabHideOverAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_EdgeColor = Color.FromArgb(196, 218, 250);
			_TextColor = Color.FromArgb(102, 102, 102);
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class DockPadTitleAppearanceProperties
	{
		private Color _ActiveBackColorGradientBegin;

		private Color _ActiveBackColorGradientEnd;

		private Color _ActiveTextColor;

		private Color _InactiveBackColor;

		private Color _InactiveTextColor;

		private IAppearanceControl ap;

		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color ActiveBackColorGradientBegin
		{
			get
			{
				return _ActiveBackColorGradientBegin;
			}
			set
			{
				_ActiveBackColorGradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color ActiveBackColorGradientEnd
		{
			get
			{
				return _ActiveBackColorGradientEnd;
			}
			set
			{
				_ActiveBackColorGradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[DefaultValue(typeof(Color), "128, 128, 128")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color ActiveTextColor
		{
			get
			{
				return _ActiveTextColor;
			}
			set
			{
				_ActiveTextColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		public Color InactiveBackColor
		{
			get
			{
				return _InactiveBackColor;
			}
			set
			{
				_InactiveBackColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "128, 128, 128")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		public Color InactiveTextColor
		{
			get
			{
				return _InactiveTextColor;
			}
			set
			{
				_InactiveTextColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xActiveBackColorGradientBegin
		{
			get
			{
				return ColorToString(_ActiveBackColorGradientBegin);
			}
			set
			{
				_ActiveBackColorGradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xActiveBackColorGradientEnd
		{
			get
			{
				return ColorToString(_ActiveBackColorGradientEnd);
			}
			set
			{
				_ActiveBackColorGradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xActiveTextColor
		{
			get
			{
				return ColorToString(_ActiveTextColor);
			}
			set
			{
				_ActiveTextColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xInactiveBackColor
		{
			get
			{
				return ColorToString(_InactiveBackColor);
			}
			set
			{
				_InactiveBackColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xInactiveTextColor
		{
			get
			{
				return ColorToString(_InactiveTextColor);
			}
			set
			{
				_InactiveTextColor = ColorFromString(value);
			}
		}

		public DockPadTitleAppearanceProperties()
		{
			_ActiveBackColorGradientBegin = Color.FromArgb(158, 190, 245);
			_ActiveBackColorGradientEnd = Color.FromArgb(196, 218, 250);
			_InactiveBackColor = Color.FromArgb(196, 218, 250);
			_ActiveTextColor = Color.Gray;
			_InactiveTextColor = Color.Gray;
		}

		public DockPadTitleAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_ActiveBackColorGradientBegin = Color.FromArgb(158, 190, 245);
			_ActiveBackColorGradientEnd = Color.FromArgb(196, 218, 250);
			_InactiveBackColor = Color.FromArgb(196, 218, 250);
			_ActiveTextColor = Color.Gray;
			_InactiveTextColor = Color.Gray;
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class DockTabStripAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _TextColor;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "109, 109, 109")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color TextColor
		{
			get
			{
				return _TextColor;
			}
			set
			{
				_TextColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				_GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				_GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xTextColor
		{
			get
			{
				return ColorToString(TextColor);
			}
			set
			{
				_TextColor = ColorFromString(value);
			}
		}

		public DockTabStripAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_TextColor = SystemColors.ControlDark;
		}

		public DockTabStripAppearanceProperties(IAppearanceControl appearanceControl)
		{
			_GradientBegin = Color.FromArgb(158, 190, 245);
			_GradientEnd = Color.FromArgb(196, 218, 250);
			_TextColor = SystemColors.ControlDark;
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class StartPageAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private Color _SecondaryColor;

		private Color _PrimaryColor;

		private Color _ButtonImageColor;

		private Color _GridHeaderColor;

		private Color _GridBodyColor;

		private Color _GridAltBodyColor;

		private Color _GridLineColor;

		private Color _GridHoverColor;

		private IAppearanceControl ap;

		[XmlIgnore]
		[DefaultValue(typeof(Color), "158, 190, 245")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color BackgroundGradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		public Color BackgroundGradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		public Color SecondaryColor
		{
			get
			{
				return _SecondaryColor;
			}
			set
			{
				_SecondaryColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 218, 250")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		public Color PrimaryColor
		{
			get
			{
				return _PrimaryColor;
			}
			set
			{
				_PrimaryColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 218, 250")]
		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color ButtonImageColor
		{
			get
			{
				return _ButtonImageColor;
			}
			set
			{
				_ButtonImageColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		public Color GridHeaderColor
		{
			get
			{
				return _GridHeaderColor;
			}
			set
			{
				_GridHeaderColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		public Color GridBodyColor
		{
			get
			{
				return _GridBodyColor;
			}
			set
			{
				_GridBodyColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		public Color GridAltBodyColor
		{
			get
			{
				return _GridAltBodyColor;
			}
			set
			{
				_GridAltBodyColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(typeof(Color), "196, 218, 250")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GridLineColor
		{
			get
			{
				return _GridLineColor;
			}
			set
			{
				_GridLineColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "196, 218, 250")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GridHoverColor
		{
			get
			{
				return _GridHoverColor;
			}
			set
			{
				_GridHoverColor = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(BackgroundGradientBegin);
			}
			set
			{
				_GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(BackgroundGradientEnd);
			}
			set
			{
				_GradientEnd = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xSecondaryColor
		{
			get
			{
				return ColorToString(SecondaryColor);
			}
			set
			{
				_SecondaryColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xPrimaryColor
		{
			get
			{
				return ColorToString(PrimaryColor);
			}
			set
			{
				_PrimaryColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xButtonImageColor
		{
			get
			{
				return ColorToString(ButtonImageColor);
			}
			set
			{
				_ButtonImageColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGridHeaderColor
		{
			get
			{
				return ColorToString(GridHeaderColor);
			}
			set
			{
				_GridHeaderColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGridBodyColor
		{
			get
			{
				return ColorToString(GridBodyColor);
			}
			set
			{
				_GridBodyColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGridAltBodyColor
		{
			get
			{
				return ColorToString(GridAltBodyColor);
			}
			set
			{
				_GridAltBodyColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGridLineColorr
		{
			get
			{
				return ColorToString(GridLineColor);
			}
			set
			{
				_GridLineColor = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGridHoverColorr
		{
			get
			{
				return ColorToString(GridHoverColor);
			}
			set
			{
				_GridHoverColor = ColorFromString(value);
			}
		}

		public StartPageAppearanceProperties()
		{
			_ButtonImageColor = SystemColors.ActiveCaption;
			_PrimaryColor = SystemColors.ActiveCaption;
			_SecondaryColor = SystemColors.GradientInactiveCaption;
			_GradientBegin = SystemColors.ActiveCaption;
			_GradientEnd = SystemColors.GradientInactiveCaption;
			_GridHeaderColor = SystemColors.ControlLight;
			_GridBodyColor = SystemColors.ControlLightLight;
			_GridAltBodyColor = SystemColors.ControlLightLight;
			_GridLineColor = SystemColors.ActiveBorder;
			_GridHoverColor = SystemColors.GradientActiveCaption;
		}

		public StartPageAppearanceProperties(IAppearanceControl appearanceControl)
			: this()
		{
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class ApplicationHeaderAppearanceProperties
	{
		private Color _GradientBegin;

		private Color _GradientEnd;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[DefaultValue(typeof(Color), "255, 255, 222")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color GradientBegin
		{
			get
			{
				return _GradientBegin;
			}
			set
			{
				_GradientBegin = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "255, 203, 136")]
		public Color GradientEnd
		{
			get
			{
				return _GradientEnd;
			}
			set
			{
				_GradientEnd = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xGradientBegin
		{
			get
			{
				return ColorToString(GradientBegin);
			}
			set
			{
				_GradientBegin = ColorFromString(value);
			}
		}

		[Browsable(false)]
		public string xGradientEnd
		{
			get
			{
				return ColorToString(GradientEnd);
			}
			set
			{
				_GradientEnd = ColorFromString(value);
			}
		}

		public ApplicationHeaderAppearanceProperties()
		{
			_GradientBegin = Color.FromArgb(255, 255, 222);
			_GradientEnd = Color.FromArgb(255, 203, 136);
		}

		public ApplicationHeaderAppearanceProperties(IAppearanceControl appearanceControl)
			: this()
		{
			SetAppearanceControl(appearanceControl);
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
			if (ap != null && ap.AppearanceProperties != null && _GradientBegin == Color.FromArgb(255, 255, 222) && _GradientEnd == Color.FromArgb(255, 203, 136))
			{
				_GradientBegin = ap.AppearanceProperties.MenuItemAppearance.PressedGradientBegin;
				_GradientEnd = ap.AppearanceProperties.MenuItemAppearance.PressedGradientEnd;
			}
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	[Serializable]
	public class ListAppearanceProperties
	{
		private Color _Background;

		private Color _Text;

		private Color _BarActiveBackground;

		private Color _BarActiveText;

		private Color _BarInactiveBackground;

		private Color _BarInactiveText;

		private IAppearanceControl ap;

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "39, 65, 118")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		public Color Background
		{
			get
			{
				return _Background;
			}
			set
			{
				_Background = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBackground
		{
			get
			{
				return ColorToString(Background);
			}
			set
			{
				Background = ColorFromString(value);
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "39, 65, 118")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		public Color Text
		{
			get
			{
				return _Text;
			}
			set
			{
				_Text = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xText
		{
			get
			{
				return ColorToString(Text);
			}
			set
			{
				Text = ColorFromString(value);
			}
		}

		[XmlIgnore]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "39, 65, 118")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color BarActiveBackground
		{
			get
			{
				return _BarActiveBackground;
			}
			set
			{
				_BarActiveBackground = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBarActiveBackground
		{
			get
			{
				return ColorToString(BarActiveBackground);
			}
			set
			{
				BarActiveBackground = ColorFromString(value);
			}
		}

		[DefaultValue(typeof(Color), "39, 65, 118")]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		public Color BarActiveText
		{
			get
			{
				return _BarActiveText;
			}
			set
			{
				_BarActiveText = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBarActiveText
		{
			get
			{
				return ColorToString(BarActiveText);
			}
			set
			{
				BarActiveText = ColorFromString(value);
			}
		}

		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		[DefaultValue(typeof(Color), "39, 65, 118")]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[XmlIgnore]
		public Color BarInactiveBackground
		{
			get
			{
				return _BarInactiveBackground;
			}
			set
			{
				_BarInactiveBackground = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBarInactiveBackground
		{
			get
			{
				return ColorToString(BarInactiveBackground);
			}
			set
			{
				BarInactiveBackground = ColorFromString(value);
			}
		}

		[DefaultValue(typeof(Color), "39, 65, 118")]
		[XmlIgnore]
		[TypeConverter(typeof(ColorTypeDropDownConverter))]
		[Editor(typeof(ColorTypeEditorDropDown), typeof(UITypeEditor))]
		public Color BarInactiveText
		{
			get
			{
				return _BarInactiveText;
			}
			set
			{
				_BarInactiveText = value;
				if (ap != null)
				{
					ap.OnAppearanceChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public string xBarInactiveText
		{
			get
			{
				return ColorToString(BarInactiveText);
			}
			set
			{
				BarInactiveText = ColorFromString(value);
			}
		}

		public ListAppearanceProperties()
		{
			_Background = SystemColors.Window;
			_Text = SystemColors.WindowText;
			_BarActiveBackground = SystemColors.Highlight;
			_BarActiveText = SystemColors.HighlightText;
			_BarInactiveBackground = SystemColors.InactiveCaption;
			_BarInactiveText = SystemColors.InactiveCaptionText;
		}

		public ListAppearanceProperties(IAppearanceControl appearanceControl)
			: this()
		{
			ap = appearanceControl;
		}

		public void SetAppearanceControl(IAppearanceControl appearanceControl)
		{
			ap = appearanceControl;
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	private ApplicationHeaderAppearanceProperties _ApplicationHeaderAppearance;

	private StartPageAppearanceProperties _StartPageAppearance;

	private DockTabAppearanceProperties _DockTabAppearance;

	private DockPadTitleAppearanceProperties _DockPadTitleAppearance;

	private DockTabStripAppearanceProperties _DockTabStripAppearance;

	private ButtonAppearanceProperties _ButtonAppearance;

	private GripAppearanceProperties _GripAppearance;

	private ImageMarginAppearanceProperties _ImageMarginAppearance;

	private MenuItemAppearanceProperties _MenuItemAppearance;

	private MenustripAppearanceProperties _MenuStripAppearance;

	private OverflowButtonAppearanceProperties _OverflowButtonAppearance;

	private RaftingContainerAppearanceProperties _RaftingContainerAppearance;

	private SeparatorAppearanceProperties _SeparatorAppearance;

	private StatusStripAppearanceProperties _StatusStripAppearance;

	private ToolstripAppearanceProperties _ToolStripAppearance;

	private ListAppearanceProperties _ListAppearance;

	[Category("DockPanel Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public DockTabStripAppearanceProperties DockTabStripAppearance
	{
		get
		{
			return _DockTabStripAppearance;
		}
		set
		{
			_DockTabStripAppearance = value;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("DockPanel Appearance")]
	public DockPadTitleAppearanceProperties DockPadTitleAppearance
	{
		get
		{
			return _DockPadTitleAppearance;
		}
		set
		{
			_DockPadTitleAppearance = value;
		}
	}

	[Category("DockPanel Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public DockTabAppearanceProperties DockTabAppearance
	{
		get
		{
			return _DockTabAppearance;
		}
		set
		{
			_DockTabAppearance = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Category("Application Header Appearance")]
	public ApplicationHeaderAppearanceProperties ApplicationHeaderAppearance
	{
		get
		{
			return _ApplicationHeaderAppearance;
		}
		set
		{
			_ApplicationHeaderAppearance = value;
		}
	}

	[Category("Start Page Appearance")]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public StartPageAppearanceProperties StartPageAppearance
	{
		get
		{
			return _StartPageAppearance;
		}
		set
		{
			_StartPageAppearance = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Category("Appearance")]
	public ButtonAppearanceProperties ButtonAppearance
	{
		get
		{
			return _ButtonAppearance;
		}
		set
		{
			_ButtonAppearance = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Appearance")]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public GripAppearanceProperties GripAppearance
	{
		get
		{
			return _GripAppearance;
		}
		set
		{
			_GripAppearance = value;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ImageMarginAppearanceProperties ImageMarginAppearance
	{
		get
		{
			return _ImageMarginAppearance;
		}
		set
		{
			_ImageMarginAppearance = value;
		}
	}

	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public MenuItemAppearanceProperties MenuItemAppearance
	{
		get
		{
			return _MenuItemAppearance;
		}
		set
		{
			_MenuItemAppearance = value;
		}
	}

	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public MenustripAppearanceProperties MenuStripAppearance
	{
		get
		{
			return _MenuStripAppearance;
		}
		set
		{
			_MenuStripAppearance = value;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public OverflowButtonAppearanceProperties OverflowButtonAppearance
	{
		get
		{
			return _OverflowButtonAppearance;
		}
		set
		{
			_OverflowButtonAppearance = value;
		}
	}

	[Category("Appearance")]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public RaftingContainerAppearanceProperties RaftingContainerAppearance
	{
		get
		{
			return _RaftingContainerAppearance;
		}
		set
		{
			_RaftingContainerAppearance = value;
		}
	}

	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public SeparatorAppearanceProperties SeparatorAppearance
	{
		get
		{
			return _SeparatorAppearance;
		}
		set
		{
			_SeparatorAppearance = value;
		}
	}

	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public StatusStripAppearanceProperties StatusStripAppearance
	{
		get
		{
			return _StatusStripAppearance;
		}
		set
		{
			_StatusStripAppearance = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Category("Appearance")]
	public ToolstripAppearanceProperties ToolStripAppearance
	{
		get
		{
			return _ToolStripAppearance;
		}
		set
		{
			_ToolStripAppearance = value;
		}
	}

	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ListAppearanceProperties ListAppearance
	{
		get
		{
			return _ListAppearance;
		}
		set
		{
			_ListAppearance = value;
		}
	}

	public static string ColorToString(Color value)
	{
		if (value.IsNamedColor || value.IsSystemColor || value.IsKnownColor)
		{
			string text = value.ToString();
			return text.Substring(7, text.Length - 8);
		}
		return value.ToArgb().ToString();
	}

	public static Color ColorFromString(string value)
	{
		value = value.Trim();
		if (char.IsDigit(value, 0) || value[0] == '-')
		{
			return Color.FromArgb(int.Parse(value));
		}
		return Color.FromName(value);
	}

	public AppearanceProperties()
	{
		_DockTabAppearance = new DockTabAppearanceProperties();
		_DockPadTitleAppearance = new DockPadTitleAppearanceProperties();
		_DockTabStripAppearance = new DockTabStripAppearanceProperties();
		_StartPageAppearance = new StartPageAppearanceProperties();
		_ApplicationHeaderAppearance = new ApplicationHeaderAppearanceProperties();
		_ButtonAppearance = new ButtonAppearanceProperties();
		_GripAppearance = new GripAppearanceProperties();
		_ImageMarginAppearance = new ImageMarginAppearanceProperties();
		_MenuStripAppearance = new MenustripAppearanceProperties();
		_MenuItemAppearance = new MenuItemAppearanceProperties();
		_RaftingContainerAppearance = new RaftingContainerAppearanceProperties();
		_SeparatorAppearance = new SeparatorAppearanceProperties();
		_StatusStripAppearance = new StatusStripAppearanceProperties();
		_ToolStripAppearance = new ToolstripAppearanceProperties();
		_OverflowButtonAppearance = new OverflowButtonAppearanceProperties();
		_ListAppearance = new ListAppearanceProperties();
	}

	public AppearanceProperties(IAppearanceControl appearanceControl)
	{
		_DockTabAppearance = new DockTabAppearanceProperties(appearanceControl);
		_DockPadTitleAppearance = new DockPadTitleAppearanceProperties(appearanceControl);
		_DockTabStripAppearance = new DockTabStripAppearanceProperties(appearanceControl);
		_StartPageAppearance = new StartPageAppearanceProperties(appearanceControl);
		_ApplicationHeaderAppearance = new ApplicationHeaderAppearanceProperties(appearanceControl);
		_ButtonAppearance = new ButtonAppearanceProperties(appearanceControl);
		_GripAppearance = new GripAppearanceProperties(appearanceControl);
		_ImageMarginAppearance = new ImageMarginAppearanceProperties(appearanceControl);
		_MenuStripAppearance = new MenustripAppearanceProperties(appearanceControl);
		_MenuItemAppearance = new MenuItemAppearanceProperties(appearanceControl);
		_RaftingContainerAppearance = new RaftingContainerAppearanceProperties(appearanceControl);
		_SeparatorAppearance = new SeparatorAppearanceProperties(appearanceControl);
		_StatusStripAppearance = new StatusStripAppearanceProperties(appearanceControl);
		_ToolStripAppearance = new ToolstripAppearanceProperties(appearanceControl);
		_OverflowButtonAppearance = new OverflowButtonAppearanceProperties(appearanceControl);
		_ListAppearance = new ListAppearanceProperties(appearanceControl);
	}

	public void SetAppearanceControl(IAppearanceControl ap)
	{
		_ButtonAppearance.SelectedAppearance.SetAppearanceControl(ap);
		_ButtonAppearance.PressedAppearance.SetAppearanceControl(ap);
		_ButtonAppearance.CheckedAppearance.SetAppearanceControl(ap);
		_GripAppearance.SetAppearanceControl(ap);
		_ImageMarginAppearance.Normal.SetAppearanceControl(ap);
		_ImageMarginAppearance.Revealed.SetAppearanceControl(ap);
		_MenuStripAppearance.SetAppearanceControl(ap);
		_MenuItemAppearance.SetAppearanceControl(ap);
		_RaftingContainerAppearance.SetAppearanceControl(ap);
		_SeparatorAppearance.SetAppearanceControl(ap);
		_StatusStripAppearance.SetAppearanceControl(ap);
		_ToolStripAppearance.SetAppearanceControl(ap);
		_OverflowButtonAppearance.SetAppearanceControl(ap);
		_DockTabAppearance.SetAppearanceControl(ap);
		_DockPadTitleAppearance.SetAppearanceControl(ap);
		_DockTabStripAppearance.SetAppearanceControl(ap);
		_StartPageAppearance.SetAppearanceControl(ap);
		_ApplicationHeaderAppearance.SetAppearanceControl(ap);
		_ListAppearance.SetAppearanceControl(ap);
	}

	public static bool Save(string xmlFile, AppearanceProperties appearanceProperties)
	{
		try
		{
			using (FileStream fileStream = new FileStream(xmlFile, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				new XmlSerializer(typeof(AppearanceProperties)).Serialize(fileStream, appearanceProperties);
				fileStream.Close();
			}
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString(), "Error saving the xml file for the theme");
			return false;
		}
	}

	public static bool Load(string xmlFile, out AppearanceProperties appearanceProperties)
	{
		appearanceProperties = null;
		try
		{
			using (FileStream stream = new FileStream(xmlFile, FileMode.Open, FileAccess.Read, FileShare.None))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(AppearanceProperties));
				AppearanceProperties appearanceProperties2 = (AppearanceProperties)xmlSerializer.Deserialize(stream);
				appearanceProperties = appearanceProperties2;
			}
			return true;
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message, "Error loading the xml file for the theme");
			return false;
		}
	}

	public static bool LoadCustomColorTable(string appearanceXmlFile, out CustomColorTable customColorTable)
	{
		if (Load(appearanceXmlFile, out var appearanceProperties))
		{
			customColorTable = appearanceProperties.GetCustomColorTable();
			return true;
		}
		customColorTable = null;
		return false;
	}

	public CustomColorTable GetCustomColorTable()
	{
		return new CustomColorTable(this);
	}

	public override string ToString()
	{
		return string.Empty;
	}
}
