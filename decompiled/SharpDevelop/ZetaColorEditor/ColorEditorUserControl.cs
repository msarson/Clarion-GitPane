using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using ZetaColorEditor.Runtime.Colors;
using ZetaColorEditor.Runtime.InternalControls;

namespace ZetaColorEditor;

public class ColorEditorUserControl : UserControl
{
	private bool _tabSet;

	private IExternalColorEditorInformationProvider _ExternalColorEditorInformationProvider;

	private static readonly ColorLookupElement[] _defaultLookupOrder = new ColorLookupElement[6]
	{
		ColorLookupElement.BasicColors,
		ColorLookupElement.BrowserSafeColors,
		ColorLookupElement.WebColors,
		ColorLookupElement.SystemColors,
		ColorLookupElement.SchemeColors,
		ColorLookupElement.CustomColors
	};

	private bool savingColor;

	private IContainer components;

	private TabControl tabControl;

	private TabPage customColorsTabPage;

	private TabPage webColorsTabPage;

	private TabPage systemColorsTabPage;

	private TabPage schemeColorsTabPage;

	private CustomColorEditorUserControl customColorEditorControl;

	private WebColorEditorUserControl webColorEditorControl;

	private SystemColorEditorUserControl systemColorEditorControl;

	private SchemesColorEditorUserControl schemesColorEditorControl;

	private BasicColorsEditorUserControl basicColorsEditorControl;

	private TabPage browserSafeColorsTabPage;

	private BrowserSafeColorEditorUserControl browserSafeColorEditorControl;

	private TabPage basicCcolorsTabPage;

	[Browsable(false)]
	public IExternalColorEditorInformationProvider ExternalColorEditorInformationProvider
	{
		get
		{
			return _ExternalColorEditorInformationProvider;
		}
		set
		{
			_ExternalColorEditorInformationProvider = value;
		}
	}

	[Browsable(false)]
	public Color SelectedColor
	{
		get
		{
			if (base.DesignMode)
			{
				return Color.Empty;
			}
			if (tabControl.SelectedTab == basicCcolorsTabPage)
			{
				return basicColorsEditorControl.SelectedColor;
			}
			if (tabControl.SelectedTab == customColorsTabPage)
			{
				return customColorEditorControl.SelectedColor;
			}
			if (tabControl.SelectedTab == browserSafeColorsTabPage)
			{
				return browserSafeColorEditorControl.SelectedColor;
			}
			if (tabControl.SelectedTab == webColorsTabPage)
			{
				return webColorEditorControl.SelectedColor;
			}
			if (tabControl.SelectedTab == systemColorsTabPage)
			{
				return systemColorEditorControl.SelectedColor;
			}
			if (tabControl.SelectedTab == schemeColorsTabPage)
			{
				return schemesColorEditorControl.SelectedColor;
			}
			return Color.Empty;
		}
		set
		{
			if (!base.DesignMode)
			{
				basicColorsEditorControl.SelectedColor = value;
				customColorEditorControl.SelectedColor = value;
				webColorEditorControl.SelectedColor = value;
				browserSafeColorEditorControl.SelectedColor = value;
				systemColorEditorControl.SelectedColor = value;
				schemesColorEditorControl.SelectedColor = value;
				tryToSetCorrectTabPage(value);
			}
		}
	}

	[Browsable(false)]
	public string SelectedClarionColor
	{
		get
		{
			return ToClarion(SelectedColor);
		}
		set
		{
			SelectedColor = FromClarion(value);
		}
	}

	internal string StoreID => $"{((ColorEditorForm)base.Parent).StoreID}.{GetType().Name}.{base.Name}.{Text}";

	private bool allowColorSchemes
	{
		get
		{
			if (ExternalColorEditorInformationProvider == null)
			{
				return false;
			}
			IColorScheme[] colorSchemes = ExternalColorEditorInformationProvider.ColorSchemes;
			if (colorSchemes == null || colorSchemes.Length <= 0)
			{
				return false;
			}
			return true;
		}
	}

	public event EventHandler NeedUpdateUI;

	public event EventHandler ColorSelected;

	public ColorEditorUserControl()
	{
		InitializeComponent();
	}

	private void DoColorSelected()
	{
		if (savingColor)
		{
			if (tabControl.SelectedTab == customColorsTabPage)
			{
				savingColor = false;
				basicColorsEditorControl.SaveColor(customColorEditorControl.SelectedColor);
				tabControl.SelectedTab = basicCcolorsTabPage;
				DoNeedUpdateUI(this, EventArgs.Empty);
			}
		}
		else if (this.ColorSelected != null)
		{
			this.ColorSelected(this, EventArgs.Empty);
		}
	}

	private void OnColorEditorControl_ColorSelected(object sender, EventArgs e)
	{
		DoColorSelected();
	}

	public static string ToClarion(Color netColor)
	{
		if (netColor == Color.Empty)
		{
			return "COLOR:NONE";
		}
		if (netColor == Color.Black)
		{
			return "COLOR:Black";
		}
		if (netColor == Color.Maroon)
		{
			return "COLOR:Maroon";
		}
		if (netColor == Color.Green)
		{
			return "COLOR:Green";
		}
		if (netColor == Color.Olive)
		{
			return "COLOR:Olive";
		}
		if (netColor == Color.Orange)
		{
			return "COLOR:Orange";
		}
		if (netColor == Color.Navy)
		{
			return "COLOR:Navy";
		}
		if (netColor == Color.Purple)
		{
			return "COLOR:Purple";
		}
		if (netColor == Color.Teal)
		{
			return "COLOR:Teal";
		}
		if (netColor == Color.Gray)
		{
			return "COLOR:Gray";
		}
		if (netColor == Color.Silver)
		{
			return "COLOR:Silver";
		}
		if (netColor == Color.Red)
		{
			return "COLOR:Red";
		}
		if (netColor == Color.Lime)
		{
			return "COLOR:Lime";
		}
		if (netColor == Color.Yellow)
		{
			return "COLOR:Yellow";
		}
		if (netColor == Color.Blue)
		{
			return "COLOR:Blue";
		}
		if (netColor == Color.Fuchsia)
		{
			return "COLOR:Fuchsia";
		}
		if (netColor == Color.Aqua)
		{
			return "COLOR:Aqua";
		}
		if (netColor == Color.White)
		{
			return "COLOR:White";
		}
		if (netColor == SystemColors.ScrollBar)
		{
			return "COLOR:SCROLLBAR";
		}
		if (netColor == SystemColors.ActiveCaption)
		{
			return "COLOR:ACTIVECAPTION";
		}
		if (netColor == SystemColors.InactiveCaption)
		{
			return "COLOR:INACTIVECAPTION";
		}
		if (netColor == SystemColors.Menu)
		{
			return "COLOR:MENU";
		}
		if (netColor == SystemColors.MenuBar)
		{
			return "COLOR:MENUBAR";
		}
		if (netColor == SystemColors.Window)
		{
			return "COLOR:WINDOW";
		}
		if (netColor == SystemColors.WindowFrame)
		{
			return "COLOR:WINDOWFRAME";
		}
		if (netColor == SystemColors.MenuText)
		{
			return "COLOR:MENUTEXT";
		}
		if (netColor == SystemColors.WindowText)
		{
			return "COLOR:WINDOWTEXT";
		}
		if (netColor == SystemColors.ActiveCaptionText)
		{
			return "COLOR:CAPTIONTEXT";
		}
		if (netColor == SystemColors.ActiveBorder)
		{
			return "COLOR:ACTIVEBORDER";
		}
		if (netColor == SystemColors.InactiveBorder)
		{
			return "COLOR:INACTIVEBORDER";
		}
		if (netColor == SystemColors.AppWorkspace)
		{
			return "COLOR:APPWORKSPACE";
		}
		if (netColor == SystemColors.Highlight)
		{
			return "COLOR:HIGHLIGHT";
		}
		if (netColor == SystemColors.HighlightText)
		{
			return "COLOR:HIGHLIGHTTEXT";
		}
		if (netColor == SystemColors.InactiveCaptionText)
		{
			return "COLOR:INACTIVECAPTIONTEXT";
		}
		if (netColor == SystemColors.GrayText)
		{
			return "COLOR:GRAYTEXT";
		}
		if (netColor == SystemColors.ButtonFace)
		{
			return "COLOR:BTNFACE";
		}
		if (netColor == SystemColors.ButtonShadow)
		{
			return "COLOR:BTNSHADOW";
		}
		if (netColor == SystemColors.ButtonHighlight)
		{
			return "COLOR:BTNHIGHLIGHT";
		}
		if (netColor == SystemColors.ControlText)
		{
			return "COLOR:BTNTEXT";
		}
		return ToClarionHex(netColor);
	}

	public static string ToClarionHex(Color netColor)
	{
		return $"00{netColor.B:X2}{netColor.G:X2}{netColor.R:X2}h";
	}

	public static Color FromClarion(string claColor)
	{
		if (claColor.ToUpper().StartsWith("COLOR:"))
		{
			return FromClarionNamedColor(claColor);
		}
		claColor = claColor.ToUpper().Trim('H', ' ');
		if (claColor.Length > 6)
		{
			claColor = claColor.Substring(claColor.Length - 6);
		}
		if (claColor.Length == 6)
		{
			string s = claColor.Substring(0, 2);
			string s2 = claColor.Substring(2, 2);
			string s3 = claColor.Substring(4, 2);
			try
			{
				int red = short.Parse(s3, NumberStyles.HexNumber);
				int green = short.Parse(s2, NumberStyles.HexNumber);
				int blue = short.Parse(s, NumberStyles.HexNumber);
				return Color.FromArgb(red, green, blue);
			}
			catch
			{
			}
		}
		return Color.Black;
	}

	private static Color FromClarionNamedColor(string claNamedColor)
	{
		Color result = Color.Empty;
		string text = claNamedColor.ToUpper();
		if (text.StartsWith("COLOR:"))
		{
			switch (text)
			{
			case "COLOR:NONE":
				result = Color.Empty;
				break;
			case "COLOR:BLACK":
				result = Color.Black;
				break;
			case "COLOR:MAROON":
				result = Color.Maroon;
				break;
			case "COLOR:GREEN":
				result = Color.Green;
				break;
			case "COLOR:OLIVE":
				result = Color.Olive;
				break;
			case "COLOR:ORANGE":
				result = Color.Orange;
				break;
			case "COLOR:NAVY":
				result = Color.Navy;
				break;
			case "COLOR:PURPLE":
				result = Color.Purple;
				break;
			case "COLOR:TEAL":
				result = Color.Teal;
				break;
			case "COLOR:GRAY":
				result = Color.Gray;
				break;
			case "COLOR:SILVER":
				result = Color.Silver;
				break;
			case "COLOR:RED":
				result = Color.Red;
				break;
			case "COLOR:LIME":
				result = Color.Lime;
				break;
			case "COLOR:YELLOW":
				result = Color.Yellow;
				break;
			case "COLOR:BLUE":
				result = Color.Blue;
				break;
			case "COLOR:FUCHSIA":
				result = Color.Fuchsia;
				break;
			case "COLOR:AQUA":
				result = Color.Aqua;
				break;
			case "COLOR:WHITE":
				result = Color.White;
				break;
			case "COLOR:SCROLLBAR":
				result = SystemColors.ScrollBar;
				break;
			case "COLOR:ACTIVECAPTION":
				result = SystemColors.ActiveCaption;
				break;
			case "COLOR:INACTIVECAPTION":
				result = SystemColors.InactiveCaption;
				break;
			case "COLOR:MENU":
				result = SystemColors.Menu;
				break;
			case "COLOR:MENUBAR":
				result = SystemColors.MenuBar;
				break;
			case "COLOR:WINDOW":
				result = SystemColors.Window;
				break;
			case "COLOR:WINDOWFRAME":
				result = SystemColors.WindowFrame;
				break;
			case "COLOR:MENUTEXT":
				result = SystemColors.MenuText;
				break;
			case "COLOR:WINDOWTEXT":
				result = SystemColors.WindowText;
				break;
			case "COLOR:CAPTIONTEXT":
				result = SystemColors.ActiveCaptionText;
				break;
			case "COLOR:ACTIVEBORDER":
				result = SystemColors.ActiveBorder;
				break;
			case "COLOR:INACTIVEBORDER":
				result = SystemColors.InactiveBorder;
				break;
			case "COLOR:APPWORKSPACE":
				result = SystemColors.AppWorkspace;
				break;
			case "COLOR:HIGHLIGHT":
				result = SystemColors.Highlight;
				break;
			case "COLOR:HIGHLIGHTTEXT":
				result = SystemColors.HighlightText;
				break;
			case "COLOR:INACTIVECAPTIONTEXT":
				result = SystemColors.InactiveCaptionText;
				break;
			case "COLOR:GRAYTEXT":
				result = SystemColors.GrayText;
				break;
			case "COLOR:BTNFACE":
				result = SystemColors.ButtonFace;
				break;
			case "COLOR:BTNSHADOW":
				result = SystemColors.ButtonShadow;
				break;
			case "COLOR:BTNHIGHLIGHT":
				result = SystemColors.ButtonHighlight;
				break;
			case "COLOR:BTNTEXT":
				result = SystemColors.ControlText;
				break;
			}
		}
		return result;
	}

	private void tryToSetCorrectTabPage(Color originalColor)
	{
		Color color = originalColor;
		TabPage tabPage = null;
		if (color == Color.Empty)
		{
			color = Color.Transparent;
		}
		if (color == Color.Transparent)
		{
			if (webColorEditorControl.ContainsColor(color))
			{
				tabPage = webColorsTabPage;
			}
		}
		else
		{
			List<ColorLookupElement> list = new List<ColorLookupElement>(_defaultLookupOrder);
			if (ExternalColorEditorInformationProvider != null)
			{
				ExternalColorEditorInformationProvider.AdjustColorSettingLookupOrder(list);
				list.AddRange(_defaultLookupOrder);
			}
			using List<ColorLookupElement>.Enumerator enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case ColorLookupElement.BasicColors:
					if (originalColor == Color.Empty)
					{
						color = Color.White;
					}
					if (basicColorsEditorControl.ContainsColor(color))
					{
						tabPage = basicCcolorsTabPage;
					}
					break;
				case ColorLookupElement.BrowserSafeColors:
					if (browserSafeColorEditorControl.ContainsColor(color))
					{
						tabPage = browserSafeColorsTabPage;
					}
					break;
				case ColorLookupElement.SchemeColors:
					if (allowColorSchemes && schemesColorEditorControl.ContainsColor(color))
					{
						tabPage = schemeColorsTabPage;
					}
					break;
				case ColorLookupElement.SystemColors:
					if (systemColorEditorControl.ContainsColor(color))
					{
						tabPage = systemColorsTabPage;
					}
					break;
				case ColorLookupElement.WebColors:
					if (webColorEditorControl.ContainsColor(color))
					{
						tabPage = webColorsTabPage;
					}
					break;
				default:
					tabPage = customColorsTabPage;
					break;
				}
				if (tabPage != null)
				{
					break;
				}
			}
		}
		if (tabPage == null)
		{
			tabPage = customColorsTabPage;
		}
		tabControl.SelectedTab = tabPage;
		_tabSet = true;
	}

	private void colorEditorUserControl_Load(object sender, EventArgs e)
	{
		if (ExternalColorEditorInformationProvider != null && !_tabSet)
		{
			tabControl.SelectedIndex = Convert.ToInt32(ExternalColorEditorInformationProvider.RestorePerUserPerWorkstationValue(StoreID + ".TabControl.SelectedIndex", tabControl.SelectedIndex.ToString()));
		}
		if (!allowColorSchemes)
		{
			tabControl.TabPages.Remove(schemeColorsTabPage);
		}
	}

	private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (ExternalColorEditorInformationProvider != null)
		{
			ExternalColorEditorInformationProvider.SavePerUserPerWorkstationValue(StoreID + ".TabControl.SelectedIndex", tabControl.SelectedIndex.ToString());
		}
		DoNeedUpdateUI(this, EventArgs.Empty);
		if (savingColor && tabControl.SelectedTab == basicCcolorsTabPage)
		{
			savingColor = false;
			basicColorsEditorControl.SaveColor(customColorEditorControl.SelectedColor);
		}
	}

	private void DoNeedUpdateUI(object sender, EventArgs e)
	{
		if (this.NeedUpdateUI != null)
		{
			this.NeedUpdateUI(this, EventArgs.Empty);
		}
	}

	private void OnColorEditorControl_NeedUpdateUI(object sender, EventArgs e)
	{
		DoNeedUpdateUI(null, null);
	}

	private void customColorEditorControl_NeedUpdateUI(object sender, EventArgs e)
	{
	}

	private void webColorEditorControl_NeedUpdateUI(object sender, EventArgs e)
	{
	}

	private void browserSafeColorEditorControl_NeedUpdateUI(object sender, EventArgs e)
	{
	}

	private void systemColorEditorControl_NeedUpdateUI(object sender, EventArgs e)
	{
	}

	private void schemesColorEditorControl_NeedUpdateUI(object sender, EventArgs e)
	{
	}

	private void basicColorsEditorControl_UserColorRequested(object sender, EventArgs e)
	{
		tabControl.SelectedTab = customColorsTabPage;
		savingColor = true;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.tabControl = new System.Windows.Forms.TabControl();
		this.customColorsTabPage = new System.Windows.Forms.TabPage();
		this.basicCcolorsTabPage = new System.Windows.Forms.TabPage();
		this.webColorsTabPage = new System.Windows.Forms.TabPage();
		this.browserSafeColorsTabPage = new System.Windows.Forms.TabPage();
		this.systemColorsTabPage = new System.Windows.Forms.TabPage();
		this.schemeColorsTabPage = new System.Windows.Forms.TabPage();
		this.customColorEditorControl = new ZetaColorEditor.Runtime.InternalControls.CustomColorEditorUserControl();
		this.basicColorsEditorControl = new ZetaColorEditor.Runtime.InternalControls.BasicColorsEditorUserControl();
		this.webColorEditorControl = new ZetaColorEditor.Runtime.InternalControls.WebColorEditorUserControl();
		this.browserSafeColorEditorControl = new ZetaColorEditor.Runtime.InternalControls.BrowserSafeColorEditorUserControl();
		this.systemColorEditorControl = new ZetaColorEditor.Runtime.InternalControls.SystemColorEditorUserControl();
		this.schemesColorEditorControl = new ZetaColorEditor.Runtime.InternalControls.SchemesColorEditorUserControl();
		this.tabControl.SuspendLayout();
		this.customColorsTabPage.SuspendLayout();
		this.basicCcolorsTabPage.SuspendLayout();
		this.webColorsTabPage.SuspendLayout();
		this.browserSafeColorsTabPage.SuspendLayout();
		this.systemColorsTabPage.SuspendLayout();
		this.schemeColorsTabPage.SuspendLayout();
		base.SuspendLayout();
		this.tabControl.Controls.Add(this.customColorsTabPage);
		this.tabControl.Controls.Add(this.basicCcolorsTabPage);
		this.tabControl.Controls.Add(this.webColorsTabPage);
		this.tabControl.Controls.Add(this.browserSafeColorsTabPage);
		this.tabControl.Controls.Add(this.systemColorsTabPage);
		this.tabControl.Controls.Add(this.schemeColorsTabPage);
		this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabControl.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.tabControl.Location = new System.Drawing.Point(0, 0);
		this.tabControl.Margin = new System.Windows.Forms.Padding(4);
		this.tabControl.MaximumSize = new System.Drawing.Size(490, 459);
		this.tabControl.MinimumSize = new System.Drawing.Size(490, 459);
		this.tabControl.Name = "tabControl";
		this.tabControl.SelectedIndex = 0;
		this.tabControl.Size = new System.Drawing.Size(490, 459);
		this.tabControl.TabIndex = 0;
		this.tabControl.SelectedIndexChanged += new System.EventHandler(tabControl_SelectedIndexChanged);
		this.customColorsTabPage.Controls.Add(this.customColorEditorControl);
		this.customColorsTabPage.Font = new System.Drawing.Font("Segoe UI", 7.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.customColorsTabPage.ImageKey = "colorwheel.png";
		this.customColorsTabPage.Location = new System.Drawing.Point(4, 29);
		this.customColorsTabPage.Margin = new System.Windows.Forms.Padding(4);
		this.customColorsTabPage.Name = "customColorsTabPage";
		this.customColorsTabPage.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
		this.customColorsTabPage.Size = new System.Drawing.Size(482, 426);
		this.customColorsTabPage.TabIndex = 0;
		this.customColorsTabPage.Text = "Custom";
		this.customColorsTabPage.UseVisualStyleBackColor = true;
		this.basicCcolorsTabPage.Controls.Add(this.basicColorsEditorControl);
		this.basicCcolorsTabPage.Location = new System.Drawing.Point(4, 29);
		this.basicCcolorsTabPage.Name = "basicCcolorsTabPage";
		this.basicCcolorsTabPage.Padding = new System.Windows.Forms.Padding(3);
		this.basicCcolorsTabPage.Size = new System.Drawing.Size(482, 426);
		this.basicCcolorsTabPage.TabIndex = 5;
		this.basicCcolorsTabPage.Text = "Basic";
		this.basicCcolorsTabPage.UseVisualStyleBackColor = true;
		this.webColorsTabPage.Controls.Add(this.webColorEditorControl);
		this.webColorsTabPage.ImageKey = "window_colors.png";
		this.webColorsTabPage.Location = new System.Drawing.Point(4, 29);
		this.webColorsTabPage.Margin = new System.Windows.Forms.Padding(4);
		this.webColorsTabPage.Name = "webColorsTabPage";
		this.webColorsTabPage.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
		this.webColorsTabPage.Size = new System.Drawing.Size(482, 426);
		this.webColorsTabPage.TabIndex = 1;
		this.webColorsTabPage.Text = "Web";
		this.webColorsTabPage.UseVisualStyleBackColor = true;
		this.browserSafeColorsTabPage.Controls.Add(this.browserSafeColorEditorControl);
		this.browserSafeColorsTabPage.ImageKey = "earth.png";
		this.browserSafeColorsTabPage.Location = new System.Drawing.Point(4, 29);
		this.browserSafeColorsTabPage.Margin = new System.Windows.Forms.Padding(4);
		this.browserSafeColorsTabPage.Name = "browserSafeColorsTabPage";
		this.browserSafeColorsTabPage.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
		this.browserSafeColorsTabPage.Size = new System.Drawing.Size(482, 426);
		this.browserSafeColorsTabPage.TabIndex = 4;
		this.browserSafeColorsTabPage.Text = "Browser-safe";
		this.browserSafeColorsTabPage.UseVisualStyleBackColor = true;
		this.systemColorsTabPage.Controls.Add(this.systemColorEditorControl);
		this.systemColorsTabPage.ImageKey = "monitor_rgb.png";
		this.systemColorsTabPage.Location = new System.Drawing.Point(4, 29);
		this.systemColorsTabPage.Margin = new System.Windows.Forms.Padding(4);
		this.systemColorsTabPage.Name = "systemColorsTabPage";
		this.systemColorsTabPage.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
		this.systemColorsTabPage.Size = new System.Drawing.Size(482, 426);
		this.systemColorsTabPage.TabIndex = 2;
		this.systemColorsTabPage.Text = "System";
		this.systemColorsTabPage.UseVisualStyleBackColor = true;
		this.schemeColorsTabPage.Controls.Add(this.schemesColorEditorControl);
		this.schemeColorsTabPage.ImageKey = "brush1.png";
		this.schemeColorsTabPage.Location = new System.Drawing.Point(4, 29);
		this.schemeColorsTabPage.Margin = new System.Windows.Forms.Padding(4);
		this.schemeColorsTabPage.Name = "schemeColorsTabPage";
		this.schemeColorsTabPage.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
		this.schemeColorsTabPage.Size = new System.Drawing.Size(482, 426);
		this.schemeColorsTabPage.TabIndex = 3;
		this.schemeColorsTabPage.Text = "Schemes";
		this.schemeColorsTabPage.UseVisualStyleBackColor = true;
		this.customColorEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.customColorEditorControl.Font = new System.Drawing.Font("Segoe UI", 7.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.customColorEditorControl.Location = new System.Drawing.Point(12, 11);
		this.customColorEditorControl.Margin = new System.Windows.Forms.Padding(5);
		this.customColorEditorControl.MinimumSize = new System.Drawing.Size(433, 386);
		this.customColorEditorControl.Name = "customColorEditorControl";
		this.customColorEditorControl.SelectedColor = System.Drawing.Color.Empty;
		this.customColorEditorControl.Size = new System.Drawing.Size(458, 404);
		this.customColorEditorControl.TabIndex = 0;
		this.customColorEditorControl.ColorSelected += new System.EventHandler(OnColorEditorControl_ColorSelected);
		this.customColorEditorControl.NeedUpdateUI += new System.EventHandler(OnColorEditorControl_NeedUpdateUI);
		this.basicColorsEditorControl.AutoScroll = true;
		this.basicColorsEditorControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.basicColorsEditorControl.CustomColors = null;
		this.basicColorsEditorControl.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.basicColorsEditorControl.Location = new System.Drawing.Point(44, 18);
		this.basicColorsEditorControl.Margin = new System.Windows.Forms.Padding(5);
		this.basicColorsEditorControl.MaximumSize = new System.Drawing.Size(385, 361);
		this.basicColorsEditorControl.MinimumSize = new System.Drawing.Size(385, 361);
		this.basicColorsEditorControl.Name = "basicColorsEditorControl";
		this.basicColorsEditorControl.SelectedColor = System.Drawing.Color.Empty;
		this.basicColorsEditorControl.Size = new System.Drawing.Size(385, 361);
		this.basicColorsEditorControl.TabIndex = 0;
		this.basicColorsEditorControl.UserColorRequested += new System.EventHandler(basicColorsEditorControl_UserColorRequested);
		this.basicColorsEditorControl.ColorSelected += new System.EventHandler(OnColorEditorControl_ColorSelected);
		this.basicColorsEditorControl.NeedUpdateUI += new System.EventHandler(OnColorEditorControl_NeedUpdateUI);
		this.webColorEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.webColorEditorControl.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.webColorEditorControl.Location = new System.Drawing.Point(12, 11);
		this.webColorEditorControl.Margin = new System.Windows.Forms.Padding(5);
		this.webColorEditorControl.Name = "webColorEditorControl";
		this.webColorEditorControl.SelectedColor = System.Drawing.Color.Empty;
		this.webColorEditorControl.Size = new System.Drawing.Size(458, 404);
		this.webColorEditorControl.TabIndex = 0;
		this.webColorEditorControl.ColorSelected += new System.EventHandler(OnColorEditorControl_ColorSelected);
		this.webColorEditorControl.NeedUpdateUI += new System.EventHandler(OnColorEditorControl_NeedUpdateUI);
		this.browserSafeColorEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.browserSafeColorEditorControl.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.browserSafeColorEditorControl.Location = new System.Drawing.Point(12, 11);
		this.browserSafeColorEditorControl.Margin = new System.Windows.Forms.Padding(5);
		this.browserSafeColorEditorControl.Name = "browserSafeColorEditorControl";
		this.browserSafeColorEditorControl.SelectedColor = System.Drawing.Color.Empty;
		this.browserSafeColorEditorControl.Size = new System.Drawing.Size(458, 404);
		this.browserSafeColorEditorControl.TabIndex = 0;
		this.browserSafeColorEditorControl.ColorSelected += new System.EventHandler(OnColorEditorControl_ColorSelected);
		this.browserSafeColorEditorControl.NeedUpdateUI += new System.EventHandler(OnColorEditorControl_NeedUpdateUI);
		this.systemColorEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.systemColorEditorControl.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.systemColorEditorControl.Location = new System.Drawing.Point(12, 11);
		this.systemColorEditorControl.Margin = new System.Windows.Forms.Padding(5);
		this.systemColorEditorControl.Name = "systemColorEditorControl";
		this.systemColorEditorControl.SelectedColor = System.Drawing.Color.Empty;
		this.systemColorEditorControl.Size = new System.Drawing.Size(458, 404);
		this.systemColorEditorControl.TabIndex = 0;
		this.systemColorEditorControl.ColorSelected += new System.EventHandler(OnColorEditorControl_ColorSelected);
		this.systemColorEditorControl.NeedUpdateUI += new System.EventHandler(OnColorEditorControl_NeedUpdateUI);
		this.schemesColorEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.schemesColorEditorControl.Location = new System.Drawing.Point(12, 11);
		this.schemesColorEditorControl.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
		this.schemesColorEditorControl.Name = "schemesColorEditorControl";
		this.schemesColorEditorControl.SelectedColor = System.Drawing.Color.Empty;
		this.schemesColorEditorControl.Size = new System.Drawing.Size(458, 404);
		this.schemesColorEditorControl.TabIndex = 0;
		this.schemesColorEditorControl.ColorSelected += new System.EventHandler(OnColorEditorControl_ColorSelected);
		this.schemesColorEditorControl.NeedUpdateUI += new System.EventHandler(OnColorEditorControl_NeedUpdateUI);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.Controls.Add(this.tabControl);
		base.Margin = new System.Windows.Forms.Padding(4);
		this.MaximumSize = new System.Drawing.Size(490, 459);
		this.MinimumSize = new System.Drawing.Size(490, 459);
		base.Name = "ColorEditorUserControl";
		base.Size = new System.Drawing.Size(490, 459);
		base.Load += new System.EventHandler(colorEditorUserControl_Load);
		this.tabControl.ResumeLayout(false);
		this.customColorsTabPage.ResumeLayout(false);
		this.basicCcolorsTabPage.ResumeLayout(false);
		this.webColorsTabPage.ResumeLayout(false);
		this.browserSafeColorsTabPage.ResumeLayout(false);
		this.systemColorsTabPage.ResumeLayout(false);
		this.schemeColorsTabPage.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
