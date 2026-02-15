using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Properties;

namespace ICSharpCode.SharpDevelop;

public class SplashScreenForm : Form
{
	private static SplashScreenForm splashScreen;

	private static List<string> requestedFileList = new List<string>();

	private static List<string> parameterList = new List<string>();

	private PictureBox pictureBox1;

	private Label labelUser;

	private Bitmap bitmap;

	public static SplashScreenForm SplashScreen
	{
		get
		{
			return splashScreen;
		}
		set
		{
			splashScreen = value;
		}
	}

	public SplashScreenForm()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.ShowInTaskbar = false;
		bitmap = new Bitmap(typeof(SplashScreenForm).Assembly.GetManifestResourceStream("Resources.SplashScreenEE.png"));
		base.ClientSize = bitmap.Size;
		RectangleF layoutRectangle = new RectangleF(7f, 174f, 180f, 19f);
		Graphics graphics = Graphics.FromImage(bitmap);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		graphics.DrawString(typeof(SplashScreenForm).Assembly.GetName().Version.ToString(), new Font("Tahoma", 12f), Brushes.Black, layoutRectangle);
		graphics.Flush();
		BackgroundImage = bitmap;
	}

	public static void ShowSplashScreen()
	{
		splashScreen = new SplashScreenForm();
		splashScreen.Show();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && bitmap != null)
		{
			bitmap.Dispose();
			bitmap = null;
		}
		base.Dispose(disposing);
	}

	public static string[] GetParameterList()
	{
		return parameterList.ToArray();
	}

	public static string[] GetRequestedFileList()
	{
		return requestedFileList.ToArray();
	}

	public static void SetCommandLineArgs(string[] args)
	{
		requestedFileList.Clear();
		parameterList.Clear();
		foreach (string text in args)
		{
			if (text.Length == 0)
			{
				continue;
			}
			if (text[0] == '-' || text[0] == '/')
			{
				int startIndex = 1;
				if (text.Length >= 2 && text[0] == '-' && text[1] == '-')
				{
					startIndex = 2;
				}
				parameterList.Add(text.Substring(startIndex));
			}
			else
			{
				requestedFileList.Add(text);
			}
		}
	}

	private void InitializeComponent()
	{
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.labelUser = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.pictureBox1.Image = Properties.Resources.SplashScreenEE;
		this.pictureBox1.Location = new System.Drawing.Point(0, 0);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(490, 322);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
		this.pictureBox1.TabIndex = 0;
		this.pictureBox1.TabStop = false;
		this.labelUser.Location = new System.Drawing.Point(7, 174);
		this.labelUser.Name = "labelUser";
		this.labelUser.Size = new System.Drawing.Size(92, 19);
		this.labelUser.TabIndex = 1;
		this.labelUser.Text = "99.9.99999";
		this.AutoSize = true;
		base.ClientSize = new System.Drawing.Size(488, 321);
		base.Controls.Add(this.labelUser);
		base.Controls.Add(this.pictureBox1);
		base.Name = "SplashScreenForm";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
