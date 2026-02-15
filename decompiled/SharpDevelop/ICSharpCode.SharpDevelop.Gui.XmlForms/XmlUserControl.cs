using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public abstract class XmlUserControl : UserControl
{
	protected XmlLoader xmlLoader;

	public virtual Dictionary<string, Control> ControlDictionary
	{
		get
		{
			if (xmlLoader == null)
			{
				return null;
			}
			return xmlLoader.ControlDictionary;
		}
	}

	public XmlUserControl()
	{
	}

	public T Get<T>(string name) where T : Control
	{
		return xmlLoader.Get<T>(name);
	}

	protected void SetupFromXmlResource(string resourceName)
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		resourceName = "Resources." + resourceName;
		SetupFromXmlStream(callingAssembly.GetManifestResourceStream(resourceName));
	}

	protected void SetupFromXmlStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		SuspendLayout();
		xmlLoader = new XmlLoader();
		SetupXmlLoader();
		if (stream != null)
		{
			xmlLoader.LoadObjectFromStream(this, stream);
		}
		InitializeXmlComponents();
		ResumeLayout(performLayout: false);
	}

	protected virtual void SetupXmlLoader()
	{
	}

	protected virtual void InitializeXmlComponents()
	{
	}
}
