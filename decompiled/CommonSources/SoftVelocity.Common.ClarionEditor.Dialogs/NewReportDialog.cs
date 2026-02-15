using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SoftVelocity.Common.ClarionEditor.Dialogs;

public class NewReportDialog : Form
{
	private string m_newReport;

	private StreamReader m_sr;

	private Hashtable m_reports = new Hashtable();

	private bool m_isAll;

	private static uint m_DefLocationValue = uint.MaxValue;

	private static int m_XLocation = (int)m_DefLocationValue;

	private static int m_YLocation = (int)m_DefLocationValue;

	private static int m_WSize = (int)m_DefLocationValue;

	private static int m_HSize = (int)m_DefLocationValue;

	private bool m_isInitializing;

	private IContainer components;

	private ListBox m_ReportList;

	private Button m_btnOK;

	private Button m_btnCancel;

	public string NewReport => m_newReport;

	public StreamReader StreamReader => m_sr;

	protected override void OnMove(EventArgs e)
	{
		if (!m_isInitializing)
		{
			m_XLocation = base.Left;
			m_YLocation = base.Top;
		}
		base.OnMove(e);
	}

	protected override void OnResize(EventArgs e)
	{
		if (!m_isInitializing)
		{
			m_WSize = base.Width;
			m_HSize = base.Height;
		}
		base.OnResize(e);
	}

	public NewReportDialog(StreamReader sr, bool isAll)
	{
		m_isAll = isAll;
		m_sr = sr;
		m_isInitializing = true;
		InitializeComponent();
		if (m_XLocation != (int)m_DefLocationValue || m_YLocation != (int)m_DefLocationValue)
		{
			base.StartPosition = FormStartPosition.Manual;
			base.Location = new Point(m_XLocation, m_YLocation);
		}
		else
		{
			base.StartPosition = FormStartPosition.CenterScreen;
		}
		if (m_WSize != (int)m_DefLocationValue || m_HSize != (int)m_DefLocationValue)
		{
			base.Size = new Size(m_WSize, m_HSize);
		}
		m_isInitializing = false;
		if (isAll)
		{
			Text = "New Structure";
		}
		ParseReportDefaults();
		if (m_ReportList.Items.Count > 0)
		{
			m_ReportList.SelectedIndex = 0;
		}
		else
		{
			m_btnOK.Enabled = false;
		}
	}

	private void m_btnOK_Click(object sender, EventArgs e)
	{
		int selectedIndex = m_ReportList.SelectedIndex;
		if (selectedIndex != -1 && m_reports.ContainsKey(selectedIndex))
		{
			m_newReport = (string)m_reports[selectedIndex];
			base.DialogResult = DialogResult.OK;
		}
		else
		{
			base.DialogResult = DialogResult.None;
		}
	}

	private bool IsEmptyLine(string line)
	{
		char[] trimChars = new char[2] { ' ', '\t' };
		string text = line.TrimStart(trimChars);
		if (text.Length == 0)
		{
			return true;
		}
		return false;
	}

	private bool IsLineAComment(string line)
	{
		string text = line.TrimStart();
		if (text.Length > 0 && text[0] == '!')
		{
			return true;
		}
		return false;
	}

	private bool IsLineATitle(string line, ref string cur_report_title)
	{
		if (line.Length > 5)
		{
			string text = line.Substring(0, 4);
			if (text == "!!> ")
			{
				cur_report_title = line.Substring(4, line.Length - 4);
				return true;
			}
		}
		return false;
	}

	private bool IsLineAReport(string line, ref string cur_report_buf)
	{
		if (char.IsLetter(line[0]))
		{
			int num = 0;
			char c = line[0];
			while (num < line.Length && c != ' ' && c != '\t')
			{
				c = line[++num];
			}
			while (num < line.Length && (c == ' ' || c == '\t'))
			{
				c = line[++num];
			}
			if (num < line.Length - 6)
			{
				string text = line.Substring(num, 6);
				if (text.ToUpperInvariant() == "REPORT" && !char.IsLetterOrDigit(line[num + 6]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ParseReportDefaults()
	{
		string cur_report_title = "";
		string cur_report_buf = "";
		bool flag = false;
		bool flag2 = false;
		string text;
		if (m_isAll)
		{
			while ((text = m_sr.ReadLine()) != null)
			{
				if (!flag)
				{
					if (IsLineATitle(text, ref cur_report_title))
					{
						flag = true;
					}
					continue;
				}
				if (!flag2)
				{
					flag2 = true;
					cur_report_buf += text;
					cur_report_buf += "\r\n";
					continue;
				}
				string item = cur_report_title;
				if (IsLineATitle(text, ref cur_report_title))
				{
					int num = m_ReportList.Items.Add(item);
					if (!m_reports.ContainsKey(num))
					{
						m_reports[num] = cur_report_buf.Clone();
					}
					cur_report_buf = null;
					flag = true;
					flag2 = false;
				}
				else
				{
					cur_report_buf += text;
					cur_report_buf += "\r\n";
				}
			}
			if (flag && flag2 && cur_report_title != null && cur_report_buf != null)
			{
				int num2 = m_ReportList.Items.Add(cur_report_title);
				if (!m_reports.ContainsKey(num2))
				{
					m_reports[num2] = cur_report_buf.Clone();
				}
			}
			return true;
		}
		while ((text = m_sr.ReadLine()) != null)
		{
			if (!flag)
			{
				if (IsLineATitle(text, ref cur_report_title))
				{
					flag = true;
				}
				continue;
			}
			if (!flag2)
			{
				if (!IsLineAReport(text, ref cur_report_buf))
				{
					if (!IsEmptyLine(text))
					{
						if (IsLineAComment(text))
						{
							cur_report_buf += text;
							cur_report_buf += "\r\n";
						}
						else if (IsLineATitle(text, ref cur_report_title))
						{
							cur_report_buf = null;
						}
						else
						{
							flag = false;
							flag2 = false;
						}
					}
				}
				else
				{
					flag2 = true;
					cur_report_buf += text;
					cur_report_buf += "\r\n";
				}
				continue;
			}
			string item2 = cur_report_title;
			if (IsLineATitle(text, ref cur_report_title))
			{
				int num3 = m_ReportList.Items.Add(item2);
				if (!m_reports.ContainsKey(num3))
				{
					m_reports[num3] = cur_report_buf.Clone();
				}
				cur_report_buf = null;
				flag = true;
				flag2 = false;
			}
			else
			{
				cur_report_buf += text;
				cur_report_buf += "\r\n";
			}
		}
		if (flag && flag2 && cur_report_title != null && cur_report_buf != null)
		{
			int num4 = m_ReportList.Items.Add(cur_report_title);
			if (!m_reports.ContainsKey(num4))
			{
				m_reports[num4] = cur_report_buf.Clone();
			}
		}
		return true;
	}

	private void m_ReportList_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		m_btnOK_Click(null, new EventArgs());
		Close();
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
		this.m_ReportList = new System.Windows.Forms.ListBox();
		this.m_btnOK = new System.Windows.Forms.Button();
		this.m_btnCancel = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.m_ReportList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.m_ReportList.FormattingEnabled = true;
		this.m_ReportList.Location = new System.Drawing.Point(13, 13);
		this.m_ReportList.Name = "m_ReportList";
		this.m_ReportList.Size = new System.Drawing.Size(182, 160);
		this.m_ReportList.TabIndex = 0;
		this.m_ReportList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(m_ReportList_MouseDoubleClick);
		this.m_btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnOK.Location = new System.Drawing.Point(13, 180);
		this.m_btnOK.Name = "m_btnOK";
		this.m_btnOK.Size = new System.Drawing.Size(75, 23);
		this.m_btnOK.TabIndex = 1;
		this.m_btnOK.Text = "&OK";
		this.m_btnOK.UseVisualStyleBackColor = true;
		this.m_btnOK.Click += new System.EventHandler(m_btnOK_Click);
		this.m_btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_btnCancel.Location = new System.Drawing.Point(120, 180);
		this.m_btnCancel.Name = "m_btnCancel";
		this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
		this.m_btnCancel.TabIndex = 2;
		this.m_btnCancel.Text = "&Cancel";
		this.m_btnCancel.UseVisualStyleBackColor = true;
		base.AcceptButton = this.m_btnOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_btnCancel;
		base.ClientSize = new System.Drawing.Size(207, 216);
		base.Controls.Add(this.m_btnCancel);
		base.Controls.Add(this.m_btnOK);
		base.Controls.Add(this.m_ReportList);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "NewReportDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "New Report";
		base.ResumeLayout(false);
	}
}
