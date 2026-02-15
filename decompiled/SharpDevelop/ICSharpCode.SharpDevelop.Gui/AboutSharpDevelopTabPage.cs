using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class AboutSharpDevelopTabPage : UserControl
{
	[Serializable]
	private class ClownFishException : Exception
	{
		public ClownFishException()
		{
		}

		public ClownFishException(string message)
			: base(message)
		{
		}

		public ClownFishException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected ClownFishException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	private Label buildLabel = new Label();

	private TextBox buildTextBox = new TextBox();

	private Label versionLabel = new Label();

	private TextBox versionTextBox = new TextBox();

	private Label sponsorLabel = new Label();

	private Label EEPELabel = new Label();

	public static string LicenseSentenceN => $"This Product is Registered to: {ClarionLic.Name}";

	public static string LicenseSentence
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(StringParser.Parse("${res:Dialog.About.License}", new string[1, 2] { { "License", "Copyright SoftVelocity." } }));
			stringBuilder.AppendLine(LicenseSentenceN);
			return stringBuilder.ToString();
		}
	}

	public AboutSharpDevelopTabPage()
	{
		Version version = Assembly.GetEntryAssembly().GetName().Version;
		versionTextBox.Text = version.Major + "." + version.Minor + "." + version.Build;
		buildTextBox.Text = version.Revision.ToString();
		versionLabel.Location = new Point(8, 10);
		versionLabel.Text = ResourceService.GetString("Dialog.About.label1Text");
		versionLabel.Size = new Size(64, 16);
		versionLabel.TabIndex = 1;
		base.Controls.Add(versionLabel);
		versionTextBox.Location = new Point(76, 8);
		versionTextBox.ReadOnly = true;
		versionTextBox.TabIndex = 4;
		versionTextBox.Size = new Size(48, 20);
		base.Controls.Add(versionTextBox);
		buildLabel.Location = new Point(128, 10);
		buildLabel.Text = ResourceService.GetString("Dialog.About.label2Text");
		buildLabel.Size = new Size(48, 16);
		buildLabel.TabIndex = 2;
		base.Controls.Add(buildLabel);
		EEPELabel.Location = new Point(262, 10);
		EEPELabel.Text = VersionService.VersionShort;
		EEPELabel.Size = new Size(30, 16);
		base.Controls.Add(EEPELabel);
		buildTextBox.Location = new Point(180, 8);
		buildTextBox.ReadOnly = true;
		buildTextBox.TabIndex = 3;
		buildTextBox.Size = new Size(72, 20);
		base.Controls.Add(buildTextBox);
		sponsorLabel.Location = new Point(8, 34);
		sponsorLabel.Text = LicenseSentence;
		sponsorLabel.Size = new Size(362, 140);
		sponsorLabel.TabIndex = 8;
		base.Controls.Add(sponsorLabel);
		Dock = DockStyle.Fill;
	}

	private void ThrowExceptionButtonClick(object sender, EventArgs e)
	{
		throw new ClownFishException();
	}
}
