using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.StartPage;

internal class StartPageThemeService
{
	private class StartPageImages
	{
		private Color _ButtonImageColor;

		private Color _BackgroundGradientTop;

		private Color _BackgroundGradientBottom;

		private Bitmap _LeftImage;

		private Bitmap _MidleImage;

		private Bitmap _RightImage;

		private Bitmap LeftLogo;

		private Bitmap _blind;

		private string _BlindImageStream;

		private Bitmap _dot_listing;

		private string _Dot_listingImageStream;

		private Bitmap _line_hor_black;

		private string _Line_hor_blackImageStream;

		private Bitmap _pixel_weiss;

		private string _Pixel_weissImageStream;

		private Bitmap LeftLine;

		private Bitmap MidleLine;

		private Bitmap RightLine;

		private Bitmap _FolderButtonImage;

		private Bitmap _DeleteButtonImage;

		public Color LineColor
		{
			get
			{
				if (BackgroundGradientTop != BackgroundGradientBottom)
				{
					return Color.FromArgb((BackgroundGradientTop.R + BackgroundGradientBottom.R) / 2, (BackgroundGradientTop.G + BackgroundGradientBottom.G) / 2, (BackgroundGradientTop.B + BackgroundGradientBottom.B) / 2);
				}
				return Color.FromArgb(BackgroundGradientTop.R / 2, BackgroundGradientTop.G / 2, BackgroundGradientTop.B / 2);
			}
		}

		public Color ButtonImageColor
		{
			get
			{
				return _ButtonImageColor;
			}
			set
			{
				_ButtonImageColor = value;
			}
		}

		public Color BackgroundGradientTop
		{
			get
			{
				return _BackgroundGradientTop;
			}
			set
			{
				_BackgroundGradientTop = value;
			}
		}

		public Color BackgroundGradientBottom
		{
			get
			{
				return _BackgroundGradientBottom;
			}
			set
			{
				_BackgroundGradientBottom = value;
			}
		}

		public Bitmap LeftImage => _LeftImage;

		public string LeftImageStream => ImageToBase64String(LeftImage);

		public Bitmap MidleImage => _MidleImage;

		public string MidleImageStream => ImageToBase64String(MidleImage);

		public Bitmap RightImage => _RightImage;

		public string RightImageStream => ImageToBase64String(RightImage);

		public Bitmap BlindImage => _blind;

		public string BlindImageStream
		{
			get
			{
				if (_BlindImageStream == null)
				{
					_BlindImageStream = ImageToBase64String(BlindImage);
				}
				return _BlindImageStream;
			}
		}

		public Bitmap Dot_listingImage => _dot_listing;

		public string Dot_listingImageStream
		{
			get
			{
				if (_Dot_listingImageStream == null)
				{
					_Dot_listingImageStream = ImageToBase64String(Dot_listingImage);
				}
				return _Dot_listingImageStream;
			}
		}

		public Bitmap Line_hor_blackImage => _line_hor_black;

		public string Line_hor_blackImageStream
		{
			get
			{
				if (_Line_hor_blackImageStream == null)
				{
					_Line_hor_blackImageStream = ImageToBase64String(Line_hor_blackImage);
				}
				return _Line_hor_blackImageStream;
			}
		}

		public Bitmap Pixel_weissImage => _pixel_weiss;

		public string Pixel_weissImageStream
		{
			get
			{
				if (_Pixel_weissImageStream == null)
				{
					_Pixel_weissImageStream = ImageToBase64String(Pixel_weissImage);
				}
				return _Pixel_weissImageStream;
			}
		}

		public Bitmap FolderButtonImage => _FolderButtonImage;

		public Bitmap DeleteButtonImage => _DeleteButtonImage;

		public string FolderButtonImageStream => ImageToBase64String(FolderButtonImage);

		public string DeleteButtonImageStream => ImageToBase64String(DeleteButtonImage);

		public StartPageImages()
		{
			BackgroundGradientTop = Color.FromArgb(150, 205, 235);
			BackgroundGradientBottom = Color.FromArgb(58, 146, 192);
			ButtonImageColor = Color.FromArgb(58, 146, 192);
			AssignImages();
			ResetColors();
		}

		private string ImageToBase64String(Image image)
		{
			using MemoryStream memoryStream = new MemoryStream();
			image.Save(memoryStream, image.RawFormat);
			return Convert.ToBase64String(memoryStream.ToArray());
		}

		private void ChangeColor(Bitmap image, Color newColor)
		{
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					byte a = image.GetPixel(i, j).A;
					if (a != 0)
					{
						image.SetPixel(i, j, Color.FromArgb(a, newColor.R, newColor.G, newColor.B));
					}
				}
			}
		}

		private void AssignImages()
		{
			LeftLine = ResourceService.GetBitmap("StartPage.Left2");
			MidleLine = ResourceService.GetBitmap("StartPage.Middle2");
			RightLine = ResourceService.GetBitmap("StartPage.Right2");
			_LeftImage = ResourceService.GetBitmap("StartPage.Left3");
			_MidleImage = ResourceService.GetBitmap("StartPage.Middle3");
			_RightImage = ResourceService.GetBitmap("StartPage.Right3");
			_FolderButtonImage = ResourceService.GetBitmap("StartPage.FolderButton");
			_DeleteButtonImage = ResourceService.GetBitmap("StartPage.DeleteButton");
			LeftLogo = ResourceService.GetBitmap("StartPage.Left1");
			_blind = ResourceService.GetBitmap("StartPage.blind");
			_line_hor_black = ResourceService.GetBitmap("StartPage.line_hor_black");
			_pixel_weiss = ResourceService.GetBitmap("StartPage.pixel_weiss");
			_dot_listing = ResourceService.GetBitmap("StartPage.dot_listing");
		}

		public void ResetColors()
		{
			ChangeColor(_FolderButtonImage, ButtonImageColor);
			ChangeColor(_DeleteButtonImage, ButtonImageColor);
			ChangeColor(LeftLine, LineColor);
			ChangeColor(MidleLine, LineColor);
			ChangeColor(RightLine, LineColor);
			using (Graphics graphics = Graphics.FromImage(_LeftImage))
			{
				DrawImage(graphics, _LeftImage.Size, LeftLine);
				graphics.DrawImage(LeftLogo, new Rectangle(0, 0, _LeftImage.Size.Width, _LeftImage.Size.Height));
			}
			using (Graphics g = Graphics.FromImage(_MidleImage))
			{
				DrawImage(g, _MidleImage.Size, MidleLine);
			}
			using Graphics g2 = Graphics.FromImage(_RightImage);
			DrawImage(g2, _RightImage.Size, RightLine);
		}

		private void DrawImage(Graphics g, Size size, Bitmap line)
		{
			Rectangle rect = new Rectangle(0, 0, size.Width, size.Height);
			Region region = new Region(rect);
			LinearGradientBrush brush = new LinearGradientBrush(rect, BackgroundGradientTop, BackgroundGradientBottom, LinearGradientMode.Vertical);
			g.FillRegion(brush, region);
			g.DrawImage(line, new Rectangle(0, 0, size.Width, size.Height));
		}
	}

	private static Color _SecondaryColor;

	private static string _SecondaryHtmlColor;

	private static Color _PrimaryColor;

	private static string _PrimaryHtmlColor;

	private static Color _GridHeaderColor;

	private static string _GridHeaderHtmlColor;

	private static Color _GridBodyColor;

	private static string _GridBodyHtmlColor;

	private static Color _GridLineColor;

	private static string _GridLineHtmlColor;

	private static Color _GridAltBodyColor;

	private static string _GridAltBodyHtmlColor;

	private static Color _GridHoverColor;

	private static string _GridHoverHtmlColor;

	private static string _ImagesGradientBottomHtmlColor;

	private static string _ImagesGradientTopHtmlColor;

	private static string _ImagesButtonHtmlColor;

	private static StartPageImages _images;

	public static Color SecondaryColor => _SecondaryColor;

	public static string SecondaryHtmlColor => _SecondaryHtmlColor;

	public static Color PrimaryColor => _PrimaryColor;

	public static string PrimaryHtmlColor => _PrimaryHtmlColor;

	public static Color GridHeaderColor => _GridHeaderColor;

	public static string GridHeaderHtmlColor => _GridHeaderHtmlColor;

	public static Color GridBodyColor => _GridBodyColor;

	public static string GridBodyHtmlColor => _GridBodyHtmlColor;

	public static Color GridLineColor => _GridLineColor;

	public static string GridLineHtmlColor => _GridLineHtmlColor;

	public static Color GridAltBodyColor => _GridAltBodyColor;

	public static string GridAltBodyHtmlColor => _GridAltBodyHtmlColor;

	public static Color GridHoverColor => _GridHoverColor;

	public static string GridHoverHtmlColor => _GridHoverHtmlColor;

	public static string ImagesGradientBottomHtmlColor => _ImagesGradientBottomHtmlColor;

	public static string ImagesGradientTopHtmlColor => _ImagesGradientTopHtmlColor;

	public static string ImagesButtonHtmlColor => _ImagesButtonHtmlColor;

	private static StartPageImages Images
	{
		get
		{
			if (_images == null)
			{
				_images = new StartPageImages();
				SetColorTable();
			}
			return _images;
		}
	}

	internal static string LeftImageStream => Images.LeftImageStream;

	internal static string MidleImageStream => Images.MidleImageStream;

	internal static string RightImageStream => Images.RightImageStream;

	internal static string FolderButtonImageStream => Images.FolderButtonImageStream;

	internal static string DeleteButtonImageStream => Images.DeleteButtonImageStream;

	internal static string BlindImageStream => Images.BlindImageStream;

	internal static string Dot_listingImageStream => Images.Dot_listingImageStream;

	internal static string Line_hor_blackImageStream => Images.Line_hor_blackImageStream;

	internal static string Pixel_weissImageStream => Images.Pixel_weissImageStream;

	public static void SetColorTable()
	{
		if (ToolStripManager.Renderer is ToolStripProfessionalRenderer)
		{
			ToolStripProfessionalRenderer toolStripProfessionalRenderer = ToolStripManager.Renderer as ToolStripProfessionalRenderer;
			if (toolStripProfessionalRenderer.ColorTable is IStartPageCustomColor)
			{
				SetColors((IStartPageCustomColor)toolStripProfessionalRenderer.ColorTable);
			}
			else
			{
				SetColors(SystemColors.ControlDark, SystemColors.ActiveCaption, SystemColors.GradientInactiveCaption, SystemColors.ActiveCaption, SystemColors.GradientInactiveCaption, SystemColors.ControlLight, SystemColors.ControlLightLight, SystemColors.ControlLightLight, SystemColors.ActiveBorder, SystemColors.GradientActiveCaption);
			}
		}
		else
		{
			SetColors(SystemColors.ActiveCaption, SystemColors.ActiveCaption, SystemColors.GradientInactiveCaption, SystemColors.ActiveCaption, SystemColors.GradientInactiveCaption, SystemColors.ControlLight, SystemColors.ControlLightLight, SystemColors.ControlLightLight, SystemColors.ActiveBorder, SystemColors.GradientActiveCaption);
		}
	}

	private static void SetColors(ProfessionalColorTable colors)
	{
		SetColors(colors.ButtonPressedGradientMiddle, colors.ToolStripGradientBegin, colors.ToolStripGradientMiddle, colors.ToolStripPanelGradientBegin, colors.ToolStripPanelGradientEnd, colors.MenuStripGradientBegin, Color.White, colors.MenuStripGradientBegin, colors.ButtonPressedBorder, colors.MenuItemSelected);
	}

	private static void SetColors(IStartPageCustomColor colors)
	{
		SetColors(colors.StartPageButtonImageColor, colors.StartPageBackgroundGradientBegin, colors.StartPageBackgroundGradientEnd, colors.StartPagePrimaryColor, colors.StartPageSecondaryColor, colors.StartPageGridHeaderColor, colors.StartPageGridBodyColor, colors.StartPageGridAltBodyColor, colors.StartPageGridLineColor, colors.StartPageGridHoverColor);
	}

	public static void SetColors(Color buttonsColor, Color GradientTop, Color GradientBottom, Color primaryColor, Color secondaryColor, Color GridHeaderColor, Color GridBodyColor, Color GridAltBodyColor, Color GridLineColor, Color GridHoverColor)
	{
		Images.BackgroundGradientBottom = GradientBottom;
		Images.BackgroundGradientTop = GradientTop;
		Images.ButtonImageColor = buttonsColor;
		Images.ResetColors();
		_PrimaryColor = primaryColor;
		_SecondaryColor = secondaryColor;
		_PrimaryHtmlColor = GetHtmlColor(primaryColor);
		_SecondaryHtmlColor = GetHtmlColor(secondaryColor);
		_GridHeaderColor = GridHeaderColor;
		_GridHeaderHtmlColor = GetHtmlColor(GridHeaderColor);
		_GridBodyColor = GridBodyColor;
		_GridBodyHtmlColor = GetHtmlColor(GridBodyColor);
		_GridLineColor = GridLineColor;
		_GridLineHtmlColor = GetHtmlColor(GridLineColor);
		_GridAltBodyColor = GridAltBodyColor;
		_GridAltBodyHtmlColor = GetHtmlColor(GridAltBodyColor);
		_GridHoverColor = GridHoverColor;
		_GridHoverHtmlColor = GetHtmlColor(GridHoverColor);
		_ImagesGradientBottomHtmlColor = GetHtmlColor(GradientBottom);
		_ImagesGradientTopHtmlColor = GetHtmlColor(GradientTop);
		_ImagesButtonHtmlColor = GetHtmlColor(buttonsColor);
	}

	public static string GetHtmlColor(Color _color)
	{
		return $"#{_color.R:X2}{_color.G:X2}{_color.B:X2}";
	}
}
