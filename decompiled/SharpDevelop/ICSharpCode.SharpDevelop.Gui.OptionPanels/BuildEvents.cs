using System.IO;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class BuildEvents : AbstractProjectOptionPanel
{
	public override void LoadPanelContents()
	{
		SetupFromXmlResource("ProjectOptions.BuildEvents.xfrm");
		InitializeHelper();
		baseDirectory = Path.GetDirectoryName(project.OutputAssemblyFullPath);
		ConnectBrowseButton("preBuildEventBrowseButton", "preBuildEventTextBox", "${res:SharpDevelop.FileFilter.AllFiles}|*.*", TextBoxEditMode.EditRawProperty);
		ConnectBrowseButton("postBuildEventBrowseButton", "postBuildEventTextBox", "${res:SharpDevelop.FileFilter.AllFiles}|*.*", TextBoxEditMode.EditRawProperty);
		ConfigurationGuiBinding configurationGuiBinding = helper.BindString("preBuildEventTextBox", "PreBuildEvent", TextBoxEditMode.EditRawProperty);
		configurationGuiBinding.CreateLocationButton("preBuildEventTextBox");
		configurationGuiBinding = helper.BindString("postBuildEventTextBox", "PostBuildEvent", TextBoxEditMode.EditRawProperty);
		configurationGuiBinding.CreateLocationButton("postBuildEventTextBox");
		configurationGuiBinding = helper.BindEnum("runPostBuildEventComboBox", "RunPostBuildEvent", new RunPostBuildEvent[0]);
		configurationGuiBinding.CreateLocationButton("runPostBuildEventComboBox");
		helper.AddConfigurationSelector(this);
	}
}
