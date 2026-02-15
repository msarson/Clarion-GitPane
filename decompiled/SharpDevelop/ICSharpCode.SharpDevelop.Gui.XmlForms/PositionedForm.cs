using System;
using System.ComponentModel;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class PositionedForm : FormWithHelp
{
	private IContainer components;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	protected virtual string DialogName => null;

	public PositionedForm()
	{
		InitializeComponent();
		base.FormClosing += PositionedForm_FormClosing;
		base.Load += PositionedForm_Load;
	}

	private void PositionedForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		DoFormClosing(this, DialogName);
	}

	private void PositionedForm_Load(object sender, EventArgs e)
	{
		DoLoad(this, DialogName);
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		DoLoad(this, DialogName);
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		DoFormClosing(this, DialogName);
		base.OnFormClosing(e);
	}

	internal static void Form_Load(object sender, EventArgs e)
	{
		Form form = (Form)sender;
		if (form != null)
		{
			DoLoad(form, null);
		}
	}

	internal static void DoLoad(Form form, string dialogName)
	{
		if (form != null)
		{
			if (string.IsNullOrEmpty(dialogName))
			{
				FormPositionService.Instance.RestorePosition(form);
			}
			else
			{
				FormPositionService.Instance.RestorePosition(form, dialogName);
			}
		}
	}

	internal static void Form_FormClosing(object sender, FormClosingEventArgs e)
	{
		Form form = (Form)sender;
		if (form != null)
		{
			DoFormClosing(form, null);
		}
	}

	internal static void DoFormClosing(Form form, string dialogName)
	{
		if (form != null)
		{
			if (string.IsNullOrEmpty(dialogName))
			{
				FormPositionService.Instance.StorePosition(form);
			}
			else
			{
				FormPositionService.Instance.StorePosition(form, dialogName);
			}
		}
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
		this.components = new System.ComponentModel.Container();
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.Text = "PositionedForm";
	}
}
