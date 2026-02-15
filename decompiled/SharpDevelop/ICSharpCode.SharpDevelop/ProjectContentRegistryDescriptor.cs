using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public sealed class ProjectContentRegistryDescriptor
{
	private Codon codon;

	private ProjectContentRegistry registry;

	public ProjectContentRegistry Registry => registry ?? (registry = (ProjectContentRegistry)codon.AddIn.CreateObject(codon.Properties["class"]));

	public bool IsRegistryLoaded => registry != null;

	public bool UseRegistryForProject(IProject project)
	{
		return codon.GetFailedAction(project) == ConditionFailedAction.Nothing;
	}

	public ProjectContentRegistryDescriptor(Codon codon)
	{
		if (codon == null)
		{
			throw new ArgumentNullException("codon");
		}
		this.codon = codon;
	}
}
