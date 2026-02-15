using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace SoftVelocity.Generator.OptionPanels;

public class ApplicationServiceOptionPanel : AbstractOptionPanel
{
	public override void LoadPanelContents()
	{
		((XmlUserControl)this).SetupFromXmlStream(((object)this).GetType().Assembly.GetManifestResourceStream("SoftVelocity.Generator.Resources.ApplicationServiceGeneralOptionsPanel.xfrm"));
		((XmlUserControl)this).Get<CheckBox>("enableOnSolutionLoadedEditApp").Checked = ApplicationServiceSettings.OnSolutionLoadedEditApp;
		((XmlUserControl)this).Get<CheckBox>("enableOnSolutionLoadedAllwaysAllowLibHunter").Checked = ApplicationServiceSettings.OnSolutionLoadedAllwaysAllowLibHunter;
		((XmlUserControl)this).Get<CheckBox>("cacheAppAfterEdit").Checked = ApplicationServiceSettings.CacheApplicationAfterEdit;
		((XmlUserControl)this).Get<CheckBox>("defaultEditMode").Checked = ApplicationServiceSettings.EditDataAsTxa;
		((XmlUserControl)this).Get<CheckBox>("restoreSolutionViewState").Checked = ApplicationServiceSettings.RestoreSolutionViewState;
		((XmlUserControl)this).Get<CheckBox>("alwaysShowDateTime").Checked = ApplicationServiceSettings.AlwaysShowChangedDateTime;
		((XmlUserControl)this).Get<CheckBox>("haveLocator").Checked = ApplicationServiceSettings.HaveLocator;
	}

	public override bool StorePanelContents()
	{
		ApplicationServiceSettings.OnSolutionLoadedEditApp = ((XmlUserControl)this).Get<CheckBox>("enableOnSolutionLoadedEditApp").Checked;
		ApplicationServiceSettings.OnSolutionLoadedAllwaysAllowLibHunter = ((XmlUserControl)this).Get<CheckBox>("enableOnSolutionLoadedAllwaysAllowLibHunter").Checked;
		ApplicationServiceSettings.CacheApplicationAfterEdit = ((XmlUserControl)this).Get<CheckBox>("cacheAppAfterEdit").Checked;
		ApplicationServiceSettings.EditDataAsTxa = ((XmlUserControl)this).Get<CheckBox>("defaultEditMode").Checked;
		ApplicationServiceSettings.HaveLocator = ((XmlUserControl)this).Get<CheckBox>("haveLocator").Checked;
		ApplicationServiceSettings.RestoreSolutionViewState = ((XmlUserControl)this).Get<CheckBox>("restoreSolutionViewState").Checked;
		ApplicationServiceSettings.AlwaysShowChangedDateTime = ((XmlUserControl)this).Get<CheckBox>("alwaysShowDateTime").Checked;
		ApplicationServiceSettings.Store();
		return true;
	}
}
