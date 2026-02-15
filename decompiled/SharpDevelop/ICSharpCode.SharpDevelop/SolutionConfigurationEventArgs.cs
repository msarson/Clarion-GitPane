using System;

namespace ICSharpCode.SharpDevelop;

public class SolutionConfigurationEventArgs : EventArgs
{
	private string configuration;

	public string Configuration => configuration;

	public SolutionConfigurationEventArgs(string configuration)
	{
		this.configuration = configuration;
	}
}
