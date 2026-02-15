using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class DefaultObjectCreator : IObjectCreator
{
	public virtual Type GetType(string name)
	{
		Type type = typeof(Control).Assembly.GetType(name);
		if (type == null)
		{
			type = typeof(Point).Assembly.GetType(name);
		}
		if (type == null)
		{
			type = typeof(string).Assembly.GetType(name);
		}
		if (type == null)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Assembly[] array = assemblies;
			foreach (Assembly assembly in array)
			{
				type = assembly.GetType(name);
				if (type != null)
				{
					break;
				}
			}
		}
		return type;
	}

	public virtual object CreateObject(string name, XmlElement el)
	{
		try
		{
			object obj = typeof(Control).Assembly.CreateInstance(name);
			if (obj == null)
			{
				obj = typeof(Point).Assembly.CreateInstance(name);
			}
			if (obj == null)
			{
				obj = typeof(string).Assembly.CreateInstance(name);
			}
			if (obj == null)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				Assembly[] array = assemblies;
				foreach (Assembly assembly in array)
				{
					obj = assembly.CreateInstance(name);
					if (obj != null)
					{
						break;
					}
				}
			}
			if (obj is Control)
			{
				((Control)obj).SuspendLayout();
			}
			return obj;
		}
		catch (Exception)
		{
			return null;
		}
	}
}
