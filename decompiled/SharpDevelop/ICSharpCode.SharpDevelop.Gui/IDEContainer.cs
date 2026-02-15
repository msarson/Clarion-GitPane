using System;
using System.ComponentModel;

namespace ICSharpCode.SharpDevelop.Gui;

internal class IDEContainer : Container
{
	private class IDESite : ISite, IServiceProvider
	{
		private string name = "";

		private IComponent component;

		private IDEContainer container;

		public IComponent Component => component;

		public IContainer Container => container;

		public bool DesignMode => false;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public IDESite(IComponent sitedComponent, IDEContainer site, string aName)
		{
			component = sitedComponent;
			container = site;
			name = aName;
		}

		public object GetService(Type serviceType)
		{
			return container.GetService(serviceType);
		}
	}

	private IServiceProvider serviceProvider;

	public IDEContainer(IServiceProvider sp)
	{
		serviceProvider = sp;
	}

	protected override object GetService(Type serviceType)
	{
		object service = base.GetService(serviceType);
		if (service == null)
		{
			service = serviceProvider.GetService(serviceType);
		}
		return service;
	}

	public ISite CreateSite(IComponent component)
	{
		return CreateSite(component, "UNKNOWN_SITE");
	}

	protected override ISite CreateSite(IComponent component, string name)
	{
		ISite site = base.CreateSite(component, name);
		return new IDESite(component, this, name);
	}
}
