using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class SelectedCulturePanel : AbstractOptionPanel
{
	private Label newCulture;

	private Label descr;

	private Label culture;

	private ListView listView;

	private static readonly string uiLanguageProperty = "CoreProperties.UILanguage";

	private string SelectedCulture
	{
		get
		{
			if (listView.SelectedItems.Count > 0)
			{
				return listView.SelectedItems[0].SubItems[1].Text;
			}
			return null;
		}
	}

	private string SelectedCountry
	{
		get
		{
			if (listView.SelectedItems.Count > 0)
			{
				return listView.SelectedItems[0].Text;
			}
			return null;
		}
	}

	public override bool ReceiveDialogMessage(DialogMessage message)
	{
		if (message == DialogMessage.OK && SelectedCulture != null)
		{
			PropertyService.Set(uiLanguageProperty, SelectedCulture);
		}
		return true;
	}

	private void ChangeCulture(object sender, EventArgs e)
	{
		newCulture.Text = ResourceService.GetString("Dialog.Options.IDEOptions.SelectCulture.UILanguageSetToLabel") + " " + SelectedCountry;
	}

	private string GetCulture(string languageCode)
	{
		foreach (Language language in LanguageService.Languages)
		{
			if (languageCode.StartsWith(language.Code))
			{
				return language.Name;
			}
		}
		return "English";
	}

	public SelectedCulturePanel()
	{
		InitializeComponent();
		listView.LargeImageList = LanguageService.LanguageImageList;
		foreach (Language language in LanguageService.Languages)
		{
			listView.Items.Add(new ListViewItem(new string[2] { language.Name, language.Code }, language.ImageIndex));
		}
		culture.Text = ResourceService.GetString("Dialog.Options.IDEOptions.SelectCulture.CurrentUILanguageLabel") + " " + GetCulture(PropertyService.Get(uiLanguageProperty, "en"));
		descr.Text = ResourceService.GetString("Dialog.Options.IDEOptions.SelectCulture.DescriptionText");
	}

	private void InitializeComponent()
	{
		this.newCulture = new System.Windows.Forms.Label();
		this.descr = new System.Windows.Forms.Label();
		this.culture = new System.Windows.Forms.Label();
		this.listView = new System.Windows.Forms.ListView();
		base.SuspendLayout();
		this.newCulture.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.newCulture.Location = new System.Drawing.Point(16, 213);
		this.newCulture.Name = "newCulture";
		this.newCulture.Size = new System.Drawing.Size(562, 28);
		this.newCulture.TabIndex = 0;
		this.newCulture.Text = "New Culture";
		this.descr.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.descr.Location = new System.Drawing.Point(16, 253);
		this.descr.Name = "descr";
		this.descr.Size = new System.Drawing.Size(562, 107);
		this.descr.TabIndex = 1;
		this.descr.Text = "Description";
		this.culture.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.culture.Location = new System.Drawing.Point(16, 179);
		this.culture.Name = "culture";
		this.culture.Size = new System.Drawing.Size(562, 28);
		this.culture.TabIndex = 2;
		this.culture.Text = "Current Culture";
		this.listView.Activation = System.Windows.Forms.ItemActivation.OneClick;
		this.listView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.listView.Location = new System.Drawing.Point(12, 13);
		this.listView.Name = "listView";
		this.listView.Size = new System.Drawing.Size(566, 144);
		this.listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
		this.listView.TabIndex = 3;
		this.listView.UseCompatibleStateImageBehavior = false;
		this.listView.ItemActivate += new System.EventHandler(ChangeCulture);
		base.Controls.Add(this.listView);
		base.Controls.Add(this.culture);
		base.Controls.Add(this.descr);
		base.Controls.Add(this.newCulture);
		base.Name = "SelectedCulturePanel";
		base.Size = new System.Drawing.Size(592, 366);
		base.ResumeLayout(false);
	}
}
