using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Widgets.SideBar;

namespace ICSharpCode.SharpDevelop.Gui;

public class SideBarView : IPadContent, IDisposable
{
	public static SharpDevelopSideBar sideBar;

	public Control Control => sideBar;

	public bool WantsEscape => false;

	public void RedrawContent()
	{
		if (sideBar != null)
		{
			sideBar.Refresh();
		}
	}

	public void Dispose()
	{
		if (sideBar != null)
		{
			SaveSideBarViewConfig();
			sideBar.Dispose();
			sideBar = null;
		}
	}

	public SideBarView()
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(Path.Combine(PropertyService.ConfigDirectory, "SideBarConfig.xml"));
			if (xmlDocument.DocumentElement.Attributes["version"] == null || xmlDocument.DocumentElement.Attributes["version"].InnerText != "1.0")
			{
				GenerateStandardSideBar();
			}
			else
			{
				sideBar = new SharpDevelopSideBar(xmlDocument.DocumentElement["SideBar"]);
			}
		}
		catch (Exception)
		{
			GenerateStandardSideBar();
		}
		sideBar.Dock = DockStyle.Fill;
	}

	private void GenerateStandardSideBar()
	{
		sideBar = new SharpDevelopSideBar();
		SideTab sideTab = new SideTab(sideBar, "${res:SharpDevelop.SideBar.GeneralCategory}");
		sideTab.DisplayName = StringParser.Parse(sideTab.Name);
		sideBar.Tabs.Add(sideTab);
		sideBar.ActiveTab = sideTab;
		sideTab = new SideTab(sideBar, "${res:SharpDevelop.SideBar.ClipboardRing}");
		sideTab.DisplayName = StringParser.Parse(sideTab.Name);
		sideTab.IsClipboardRing = true;
		sideTab.CanBeDeleted = false;
		sideTab.CanDragDrop = false;
		sideBar.Tabs.Add(sideTab);
	}

	public static void PutInClipboardRing(string text)
	{
		if (sideBar == null)
		{
			WorkbenchSingleton.Workbench.GetPad(typeof(SideBarView)).CreatePad();
			return;
		}
		sideBar.PutInClipboardRing(text);
		sideBar.Refresh();
	}

	public void SaveSideBarViewConfig()
	{
		if (sideBar != null)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<SideBarConfig version=\"1.0\"/>");
			xmlDocument.DocumentElement.AppendChild(sideBar.ToXmlElement(xmlDocument));
			FileUtility.ObservedSave(xmlDocument.Save, Path.Combine(PropertyService.ConfigDirectory, "SideBarConfig.xml"), FileErrorPolicy.ProvideAlternative);
		}
	}
}
