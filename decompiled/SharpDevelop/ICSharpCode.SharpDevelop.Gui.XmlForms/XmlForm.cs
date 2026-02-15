using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public abstract class XmlForm : Form
{
	protected XmlLoader xmlLoader;

	public Dictionary<string, Control> ControlDictionary => xmlLoader.ControlDictionary;

	public Control Get(string key)
	{
		return xmlLoader.ControlDictionary[key];
	}

	public XmlForm()
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

	protected void SetupFromXmlResource(string assemblyName, string resourceName)
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		resourceName = assemblyName + ".Resources." + resourceName;
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
		RightToLeftConverter.ConvertRecursive(this);
		ResumeLayout(performLayout: true);
	}

	protected virtual void SetupXmlLoader()
	{
	}

	protected virtual void InitializeXmlComponents()
	{
	}
}
