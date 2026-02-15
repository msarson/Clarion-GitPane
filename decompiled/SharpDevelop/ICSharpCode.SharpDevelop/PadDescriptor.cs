using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public class PadDescriptor : IDisposable
{
	private string @class;

	private string title;

	private string icon;

	private string category;

	private string shortcut;

	private AddIn addIn;

	private Type padType;

	private IPadContent padContent;

	private bool padContentCreated;

	public string Title => title;

	public string Icon => icon;

	public string Category
	{
		get
		{
			return category;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			category = value;
		}
	}

	public string Shortcut
	{
		get
		{
			return shortcut;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			shortcut = value;
		}
	}

	public string Class => @class;

	public bool HasFocus
	{
		get
		{
			if (padContent == null)
			{
				return false;
			}
			return padContent.Control.ContainsFocus;
		}
	}

	public bool WantsEscape
	{
		get
		{
			if (padContent == null)
			{
				return false;
			}
			return padContent.WantsEscape;
		}
	}

	public IPadContent PadContent
	{
		get
		{
			CreatePad();
			return padContent;
		}
	}

	public PadDescriptor(Codon codon)
	{
		addIn = codon.AddIn;
		shortcut = codon.Properties["shortcut"];
		category = codon.Properties["category"];
		icon = codon.Properties["icon"];
		title = codon.Properties["title"];
		@class = codon.Properties["class"];
	}

	public PadDescriptor(Type padType, string title, string icon)
	{
		this.padType = padType;
		@class = padType.FullName;
		this.title = title;
		this.icon = icon;
		category = "none";
		shortcut = "";
	}

	public void Dispose()
	{
		if (padContent != null)
		{
			padContent.Dispose();
			padContent = null;
		}
	}

	public void RedrawContent()
	{
		if (padContent != null)
		{
			padContent.RedrawContent();
		}
	}

	public void CreatePad()
	{
		if (!padContentCreated)
		{
			padContentCreated = true;
			if (addIn != null)
			{
				padContent = (IPadContent)addIn.CreateObject(Class);
			}
			else
			{
				padContent = (IPadContent)Activator.CreateInstance(padType);
			}
		}
	}

	public void BringPadToFront()
	{
		CreatePad();
		if (padContent != null)
		{
			if (!WorkbenchSingleton.Workbench.WorkbenchLayout.IsVisible(this))
			{
				WorkbenchSingleton.Workbench.WorkbenchLayout.ShowPad(this);
			}
			WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(this);
		}
	}

	public void BringPadToFrontAndPin()
	{
		CreatePad();
		if (padContent != null)
		{
			if (!WorkbenchSingleton.Workbench.WorkbenchLayout.IsVisible(this))
			{
				WorkbenchSingleton.Workbench.WorkbenchLayout.ShowAndDockPad(this);
			}
			WorkbenchSingleton.Workbench.WorkbenchLayout.ActivateAndDockPad(this);
		}
	}

	public void ShowPad()
	{
		CreatePad();
		if (padContent != null && !WorkbenchSingleton.Workbench.WorkbenchLayout.IsVisible(this))
		{
			WorkbenchSingleton.Workbench.WorkbenchLayout.ShowPad(this);
		}
	}

	public void HidePad()
	{
		CreatePad();
		if (padContent != null && WorkbenchSingleton.Workbench.WorkbenchLayout.IsVisible(this))
		{
			WorkbenchSingleton.Workbench.WorkbenchLayout.HidePad(this);
		}
	}
}
