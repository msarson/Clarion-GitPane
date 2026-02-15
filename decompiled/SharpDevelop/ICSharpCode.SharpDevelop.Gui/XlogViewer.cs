using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class XlogViewer : UserControl
{
	private string _FileName;

	private IContainer components;

	private TextBox XlogFileContent;

	public string FileName => _FileName;

	public XlogViewer()
	{
		InitializeComponent();
	}

	public void LoadFile(string fileName)
	{
		_FileName = fileName;
		XlogFileContent.Text = GetStringFromFile();
		XlogFileContent.TabStop = false;
	}

	private string GetStringFromFile()
	{
		if (FileName == null)
		{
			return string.Empty;
		}
		string result = string.Empty;
		if (File.Exists(FileName))
		{
			FileStream fileStream = new FileStream(FileName, FileMode.Open);
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				result = EncryptionService.DecryptString((string)binaryFormatter.Deserialize(fileStream));
			}
			catch (CryptographicException)
			{
				try
				{
					fileStream.Close();
					fileStream = new FileStream(FileName, FileMode.Open);
					BinaryFormatter binaryFormatter2 = new BinaryFormatter();
					result = Encoding.ASCII.GetString(Convert.FromBase64String((string)binaryFormatter2.Deserialize(fileStream)));
				}
				catch (Exception ex2)
				{
					MessageService.ShowError("Failed to Read Old Format log. Reason: " + ex2.Message);
					throw;
				}
			}
			catch (SerializationException ex3)
			{
				MessageService.ShowError("Failed to Read log. Reason: " + ex3.Message);
				throw;
			}
			finally
			{
				fileStream.Close();
			}
		}
		return result;
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
		this.XlogFileContent = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.XlogFileContent.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.XlogFileContent.Location = new System.Drawing.Point(16, 16);
		this.XlogFileContent.Multiline = true;
		this.XlogFileContent.Name = "XlogFileContent";
		this.XlogFileContent.ReadOnly = true;
		this.XlogFileContent.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.XlogFileContent.Size = new System.Drawing.Size(552, 489);
		this.XlogFileContent.TabIndex = 0;
		this.XlogFileContent.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.AutoSize = true;
		base.Controls.Add(this.XlogFileContent);
		base.Name = "XlogViewer";
		base.Size = new System.Drawing.Size(588, 594);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
