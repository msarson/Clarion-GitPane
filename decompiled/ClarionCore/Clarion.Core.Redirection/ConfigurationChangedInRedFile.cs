using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;

namespace Clarion.Core.Redirection;

public class ConfigurationChangedInRedFile : SoftEventHandler
{
	private bool changeConfig;

	internal bool ConfigurationChanged
	{
		get
		{
			return changeConfig;
		}
		set
		{
			changeConfig = value;
		}
	}

	private void ConfigChanged(object o, SolutionConfigurationEventArgs e)
	{
		changeConfig = true;
	}

	public ConfigurationChangedInRedFile()
	{
		ProjectService.SolutionConfigurationChanged += ConfigChanged;
	}

	public override void Detach()
	{
		ProjectService.SolutionConfigurationChanged -= ConfigChanged;
	}
}
