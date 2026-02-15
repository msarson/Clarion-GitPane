using System;
using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Project;

public class ProjectNodeSite : ISite, IServiceProvider
{
	private string m_name;

	private static ProjectNodeSite instance;

	private IComponent m_component;

	internal static ProjectNodeSite Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ProjectNodeSite();
			}
			return instance;
		}
	}

	public virtual IComponent Component
	{
		get
		{
			return m_component;
		}
		set
		{
			m_component = value;
		}
	}

	public virtual IContainer Container => null;

	public virtual bool DesignMode => false;

	public virtual string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	private ProjectNodeSite()
	{
	}

	public virtual object GetService(Type serviceType)
	{
		return null;
	}
}
