using System.ComponentModel;
using System.IO;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class WebReferencesProjectItem : FileProjectItem
{
	[Browsable(false)]
	public string Directory => Path.Combine(base.Project.Directory, base.Include).Trim('\\', '/');

	public WebReferencesProjectItem(IProject project)
		: base(project, ItemType.WebReferences)
	{
	}

	internal WebReferencesProjectItem(IProject project, BuildItem buildItem)
		: base(project, buildItem)
	{
	}
}
