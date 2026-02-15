using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public static class LanguageService
{
	private static string languagePath;

	private static ImageList languageImageList;

	private static ArrayList languages;

	public static ImageList LanguageImageList => languageImageList;

	public static ArrayList Languages => languages;

	static LanguageService()
	{
		languagePath = FileUtility.Combine(PropertyService.DataDirectory, "resources", "languages");
		languageImageList = null;
		languages = null;
		languageImageList = new ImageList();
		languageImageList.ColorDepth = ColorDepth.Depth32Bit;
		languages = new ArrayList();
		LanguageImageList.ImageSize = new Size(46, 38);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(Path.Combine(languagePath, "LanguageDefinition.xml"));
		XmlNodeList childNodes = xmlDocument.DocumentElement.ChildNodes;
		foreach (XmlNode item in childNodes)
		{
			if (item is XmlElement xmlElement)
			{
				languages.Add(new Language(xmlElement.Attributes["name"].InnerText, xmlElement.Attributes["code"].InnerText, LanguageImageList.Images.Count));
				LanguageImageList.Images.Add(new Bitmap(Path.Combine(languagePath, xmlElement.Attributes["icon"].InnerText)));
			}
		}
	}
}
