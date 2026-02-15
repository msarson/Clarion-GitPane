using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public abstract class AbstractProjectOptionPanel : AbstractOptionPanel, ICanBeDirty
{
	protected ConfigurationGuiHelper helper;

	protected MSBuildBasedProject project;

	public bool IsDirty
	{
		get
		{
			return helper.IsDirty;
		}
		set
		{
			helper.IsDirty = value;
		}
	}

	public event EventHandler DirtyChanged
	{
		add
		{
			helper.DirtyChanged += value;
		}
		remove
		{
			helper.DirtyChanged -= value;
		}
	}

	protected void InitializeHelper()
	{
		project = (MSBuildBasedProject)((Properties)CustomizationObject).Get("Project");
		baseDirectory = project.Directory;
		helper = new ConfigurationGuiHelper(project, ControlDictionary);
	}

	public override bool StorePanelContents()
	{
		return helper.Save();
	}
}
