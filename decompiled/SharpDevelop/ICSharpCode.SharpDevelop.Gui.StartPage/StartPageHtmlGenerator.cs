using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.StartPage;

public class StartPageHtmlGenerator
{
	private const string addinTreePath = "/SharpDevelop/StartPage/RecentOpenEvents";

	private string startPageLocation;

	private int m_nLeftTopImageWidth = 292;

	private int m_nRightTopImageWidth = 363;

	private bool m_bShowMilestoneContentImage;

	private string m_strTitle;

	private string m_strMetaDescription;

	private string m_strMetaKeywords;

	private string m_strMetaAuthor;

	private string m_strMetaCopyright;

	private string m_strRightBoxHtml;

	private string m_strContentBarText;

	private string m_strTopMenuSelectedItem;

	private string m_strLeftMenuSelectedItem;

	private string m_strVersionText;

	private string m_strVersionStatus;

	private List<MenuItem> TopMenu;

	private Dictionary<string, string> originalSectionNames = new Dictionary<string, string>();

	private Dictionary<string, List<RecentOpen.RecentOpenDescription>> recentFiles = new Dictionary<string, List<RecentOpen.RecentOpenDescription>>();

	private Dictionary<string, StringBuilder> recentPagesContent = new Dictionary<string, StringBuilder>();

	public string PrimaryColor => StartPageThemeService.PrimaryHtmlColor;

	public string SecondaryColor => StartPageThemeService.SecondaryHtmlColor;

	public string GridHeaderColor => StartPageThemeService.GridHeaderHtmlColor;

	public string GridBodyColor => StartPageThemeService.GridBodyHtmlColor;

	public string GridLineColor => StartPageThemeService.GridLineHtmlColor;

	public string GridAltBodyColor => StartPageThemeService.GridAltBodyHtmlColor;

	public string GridHoverColor => StartPageThemeService.GridHoverHtmlColor;

	public string Title
	{
		get
		{
			return m_strTitle;
		}
		set
		{
			m_strTitle = value;
		}
	}

	public bool ShowMilestoneContentImage
	{
		get
		{
			return m_bShowMilestoneContentImage;
		}
		set
		{
			m_bShowMilestoneContentImage = value;
		}
	}

	public string MetaDescription
	{
		get
		{
			return m_strMetaDescription;
		}
		set
		{
			m_strMetaDescription = value;
		}
	}

	public string MetaKeywords
	{
		get
		{
			return m_strMetaKeywords;
		}
		set
		{
			m_strMetaKeywords = value;
		}
	}

	public string MetaAuthor
	{
		get
		{
			return m_strMetaAuthor;
		}
		set
		{
			m_strMetaAuthor = value;
		}
	}

	public string MetaCopyright
	{
		get
		{
			return m_strMetaCopyright;
		}
		set
		{
			m_strMetaCopyright = value;
		}
	}

	public string ContentBarText
	{
		get
		{
			return m_strContentBarText;
		}
		set
		{
			m_strContentBarText = value;
		}
	}

	public string TopMenuSelectedItem
	{
		get
		{
			return m_strTopMenuSelectedItem;
		}
		set
		{
			m_strTopMenuSelectedItem = value;
		}
	}

	public string LeftMenuSelectedItem
	{
		get
		{
			return m_strLeftMenuSelectedItem;
		}
		set
		{
			m_strLeftMenuSelectedItem = value;
		}
	}

	public string VersionText
	{
		get
		{
			return m_strVersionText;
		}
		set
		{
			m_strVersionText = value;
		}
	}

	public string VersionStatus
	{
		get
		{
			return m_strVersionStatus;
		}
		set
		{
			m_strVersionStatus = value;
		}
	}

	public string RightBoxHtml
	{
		get
		{
			return m_strRightBoxHtml;
		}
		set
		{
			m_strRightBoxHtml = value;
		}
	}

	public virtual void PopulateTopMenu()
	{
		foreach (string recentOpenCategory in FileService.RecentOpen.RecentOpenCategories)
		{
			originalSectionNames[recentOpenCategory.ToLowerInvariant()] = recentOpenCategory;
			string text = StringParser.Parse("${res:StartPage.StartMenu." + recentOpenCategory + "}");
			if (string.IsNullOrEmpty(text))
			{
				text = recentOpenCategory;
			}
			TopMenu.Add(new MenuItem(text, "startpage://" + recentOpenCategory.ToLowerInvariant(), recentOpenCategory));
		}
	}

	public StartPageHtmlGenerator()
	{
		TopMenu = new List<MenuItem>();
		PopulateTopMenu();
		TopMenuSelectedItem = RecentOpen.defaultTypeProjects;
		VersionText = "version 2.1.0.2447";
		VersionStatus = "";
		RightBoxHtml = "";
		MetaAuthor = "SoftVelocity";
		MetaCopyright = "(c) 2001-2022 SoftVelocity";
	}

	public bool HasRecentEvent(string eventName, string sectionName)
	{
		if (originalSectionNames.ContainsKey(sectionName.ToLowerInvariant()))
		{
			sectionName = originalSectionNames[sectionName.ToLowerInvariant()];
		}
		AddInTreeNode treeNode = AddInTree.GetTreeNode("/SharpDevelop/StartPage/RecentOpenEvents", throwOnNotFound: false);
		if (treeNode != null)
		{
			string text = eventName + sectionName;
			foreach (Codon codon in treeNode.Codons)
			{
				if (codon.Id == text)
				{
					return true;
				}
			}
		}
		return false;
	}

	public AbstractCommand CreateRecentEvent(string eventName, string sectionName)
	{
		if (originalSectionNames.ContainsKey(sectionName.ToLowerInvariant()))
		{
			sectionName = originalSectionNames[sectionName.ToLowerInvariant()];
		}
		return AddInTree.BuildItem("/SharpDevelop/StartPage/RecentOpenEvents/" + eventName + sectionName, this) as AbstractCommand;
	}

	public string GetOriginalSectionName(string sectionName)
	{
		if (originalSectionNames.ContainsKey(sectionName.ToLowerInvariant()))
		{
			return originalSectionNames[sectionName.ToLowerInvariant()];
		}
		return sectionName;
	}

	public virtual void RenderCSS(StringBuilder builder)
	{
		Font font = FontService.GetFont(FontService.FontType.StartPage);
		Font font2 = new Font(font.FontFamily, font.SizeInPoints + 6f);
		Font font3 = new Font(font.FontFamily, font.SizeInPoints - 2f);
		builder.Append("<style TYPE=\"text/css\">");
		builder.Append("\r\n<!--");
		builder.Append(".balken {");
		builder.Append("background-color:#DCDDDE;");
		builder.Append("}");
		builder.Append(".copy {");
		builder.Append("font-size:12px;");
		builder.Append("font-family:Arial,Helvetica,sans-serif;");
		builder.Append("color:Black;");
		builder.Append("}");
		builder.Append(".navi {");
		builder.Append("font-size:" + font.Size + "px;");
		builder.Append("font-family:" + font.Name + ";");
		builder.Append("color:Black;");
		builder.Append("font-style:normal;");
		builder.Append("font-weight:bold;");
		builder.Append("}");
		builder.Append(".naviActiv {");
		builder.Append("font-size:" + font.Size + "px;");
		builder.Append("font-family:" + font.Name + ";");
		builder.Append("color:#DB4E2E;");
		builder.Append("font-weight:bold;");
		builder.Append("}");
		builder.Append(".text {");
		builder.Append("background-color:White;");
		builder.Append("font-family:Arial,Helvetica,sans-serif;");
		builder.Append("font-size:10px;");
		builder.Append("color:Black;");
		builder.Append("}");
		builder.Append(".septextP {");
		builder.Append("background-color:" + PrimaryColor + ";");
		builder.Append("font-family:Arial,Helvetica,sans-serif;");
		builder.Append("font-size:3px;");
		builder.Append("color:" + PrimaryColor + ";");
		builder.Append("}");
		builder.Append(".septextS {");
		builder.Append("background-color:" + SecondaryColor + ";");
		builder.Append("font-family:Arial,Helvetica,sans-serif;");
		builder.Append("font-size:3px;");
		builder.Append("color:" + SecondaryColor + ";");
		builder.Append("}");
		builder.Append(".copyRightText {");
		builder.Append("font-size:" + font3.Size + "px;");
		builder.Append("font-family:" + font3.Name + ";");
		builder.Append("font-style:normal;");
		builder.Append("}");
		builder.Append(".head {");
		builder.Append("font-size:" + font2.Size + "px;");
		builder.Append("font-family:" + font2.Name + ";");
		builder.Append("color:Black;");
		builder.Append("font-style:bold;");
		builder.Append("}");
		builder.Append(".naviListDevelop {");
		builder.Append("font-size:12px;");
		builder.Append("font-family:Arial,Helvetica,sans-serif;");
		builder.Append("color:#808285;");
		builder.Append("font-style:normal;");
		builder.Append("font-weight:bold;");
		builder.Append("}");
		builder.Append(".naviListDevelopActiv {");
		builder.Append("font-size:12px;");
		builder.Append("font-family:Arial,Helvetica,sans-serif;");
		builder.Append("color:#808285;");
		builder.Append("font-style:normal;");
		builder.Append("font-weight:bold;");
		builder.Append("}");
		builder.Append(".milestoneText {");
		builder.Append("font-family:Arial,Helvetica,sans-serif;");
		builder.Append("font-size:14;");
		builder.Append("color:White;");
		builder.Append("font-weight:bold;");
		builder.Append("}");
		builder.Append(".copyUnderlineBig {");
		builder.Append("font-size:12px;");
		builder.Append("font-family:Arial,Helvetica,sans-serif;");
		builder.Append("color:Black;");
		builder.Append("text-decoration:underline;");
		builder.Append("font-weight:bold;");
		builder.Append("}");
		builder.Append("div.tablediv");
		builder.Append("{");
		builder.Append("width: 100%; /* Forces tables to have correct right margins and top spacing */");
		builder.Append("margin-top: -.4em;");
		builder.Append("}");
		builder.Append("ol div.tablediv, ul div.tablediv, ol div.HxLinkTable, ul div.HxLinkTable");
		builder.Append("{");
		builder.Append("margin-top: 0em; /* Forces tables to have correct right margins and top spacing */");
		builder.Append("}");
		builder.Append("table.dtTABLE");
		builder.Append("{");
		builder.Append("font-size:" + font3.Size + "px;");
		builder.Append("font-family:" + font3.Name + ";");
		builder.Append("width: 100%; /* Forces tables to have correct right margin */");
		builder.Append("margin-top: .6em;");
		builder.Append("margin-bottom: .3em;");
		builder.Append("border-width: 1px 1px 0px 0px;");
		builder.Append("border-style: solid;");
		builder.Append("border-color: " + GridLineColor + ";");
		builder.Append("background-color: " + GridBodyColor + "; ");
		builder.Append("}");
		builder.Append("table.dtTABLE th, table.dtTABLE td");
		builder.Append("{ ");
		builder.Append("border-style: solid; /* Creates the cell border and color */");
		builder.Append("border-width: 0px 0px 1px 1px;");
		builder.Append("border-style: solid;");
		builder.Append("border-color: " + GridLineColor + ";");
		builder.Append("padding: 4px 6px;");
		builder.Append("text-align: left;");
		builder.Append("}");
		builder.Append("table.dtTABLE th");
		builder.Append("{ ");
		builder.Append("background: " + GridHeaderColor + "; /* Creates the shaded table header row */");
		builder.Append("vertical-align: bottom;");
		builder.Append("}");
		builder.Append("table.dtTABLE td");
		builder.Append("{");
		builder.Append("vertical-align: top;");
		builder.Append("}");
		builder.Append("table.dtTABLE tr:hover {background-color: " + GridHoverColor + ";}");
		builder.Append("button.cusbutton {");
		builder.Append("background-color:" + StartPageThemeService.ImagesButtonHtmlColor + ";");
		builder.Append("color:#FFF;");
		builder.Append("border: " + SecondaryColor + ";");
		builder.Append("border-style: solid;");
		builder.Append("border-width: 5px;");
		builder.Append("font-weight:600;");
		builder.Append("padding-top: 7px;");
		builder.Append("padding-left: 25px;");
		builder.Append("padding-right: 25px;");
		builder.Append("padding-bottom: 7px;");
		builder.Append("}");
		builder.Append("button.cusbutton:hover {");
		builder.Append("border: #FFF;");
		builder.Append("border-style: solid;");
		builder.Append("border-width: 5px;");
		builder.Append("}");
		builder.Append("\r\n-->");
		builder.Append("\r\n</style>");
	}

	public virtual void RenderHeaderSection(StringBuilder builder)
	{
		builder.Append("<!DOCTYPE html><head><title>");
		builder.Append(Title);
		builder.Append("</title>\r\n");
		RenderCSS(builder);
		builder.Append("<META HTTP-EQUIV=\"X-UA-Compatible content-type: text/html; charset= ISO-8859-1 content='IE=edge'\">\r\n");
		builder.Append("<META NAME=\"robots\" CONTENT=\"FOLLOW,INDEX\">\r\n");
		builder.Append("<meta name=\"Author\" content=\"");
		builder.Append(MetaAuthor);
		builder.Append("\">\r\n<META NAME=\"copyright\" CONTENT=\"");
		builder.Append(MetaCopyright);
		builder.Append("\">\r\n<meta http-equiv=\"Description\" name=\"Description\" content=\"");
		builder.Append(MetaDescription);
		builder.Append("\">\r\n<meta http-equiv=\"Keywords\" name=\"Keywords\" content=\"");
		builder.Append(MetaKeywords);
		builder.Append("\">");
		builder.Append("\r\n</head>\r\n<body bgcolor=\"" + SecondaryColor + "\">\r\n");
	}

	public virtual void RenderPageEndSection(StringBuilder builder)
	{
		builder.Append("</body>\r\n</html>\r\n");
	}

	public virtual void RenderPageTopSection(string section, StringBuilder builder)
	{
		builder.Append("<div class=\"balken\" style=\"position:absolute;left:0px;top:0px;width:100%;height:72px;background-color: '" + PrimaryColor + "'\">");
		builder.Append("<table border=0 cellspacing=0 cellpadding=0 height=72px width=\"100%\"><TR>\r\n");
		builder.Append("<td height=72px background=\"data:image/png;base64," + StartPageThemeService.MidleImageStream);
		builder.Append("\">\r\n");
		builder.Append("<img src=\"data:image/png;base64," + StartPageThemeService.LeftImageStream + "\" width=");
		builder.Append(m_nLeftTopImageWidth.ToString());
		builder.Append(" height=72px></td>\r\n");
		builder.Append("<td height=72px width=\"100%\" background=\"data:image/png;base64," + StartPageThemeService.MidleImageStream);
		builder.Append("\">&nbsp;</td>\r\n");
		builder.Append("<td height=72px background=\"data:image/png;base64," + StartPageThemeService.MidleImageStream);
		builder.Append("\">\r\n");
		builder.Append("<img src=\"data:image/png;base64," + StartPageThemeService.RightImageStream + "\" width=");
		builder.Append(m_nRightTopImageWidth.ToString());
		builder.Append(" height=72px></td>");
		builder.Append("\r\n</TR></table>\r\n");
		builder.Append("<table border=0 cellspacing=0 cellpadding=0 width=\"100%\" bgcolor=\"" + PrimaryColor + "\">");
		builder.Append("<tr>");
		builder.Append("<td><img src=\"data:image/png;base64," + StartPageThemeService.BlindImageStream + "\" height=0></td>\r\n");
		builder.Append("<td align=left bgcolor=\"" + PrimaryColor + "\">\r\n");
		builder.Append("<table border=0 cellspacing=0 cellpadding=0 bgcolor=\"" + PrimaryColor + "\">\r\n");
		builder.Append("<tr>");
		builder.Append("<td>");
		builder.Append("<p class=\"septextP\">&nbsp;</p>");
		builder.Append("</td>");
		builder.Append("<td></td>");
		builder.Append("</tr>");
		builder.Append("<tr>\r\n");
		builder.Append("<td width=20><img src=\"data:image/png;base64," + StartPageThemeService.BlindImageStream + "\" height=0></td>\r\n");
		int num = TopMenu.Count;
		foreach (MenuItem item in TopMenu)
		{
			num--;
			builder.Append("<td class=\"navi");
			if (item.Id.Equals(m_strTopMenuSelectedItem, StringComparison.OrdinalIgnoreCase))
			{
				builder.Append("Activ\">");
				builder.Append(item.Caption);
				builder.Append("</td>\r\n");
			}
			else
			{
				builder.Append("\"><a href=\"");
				builder.Append(item.URL);
				builder.Append("\">");
				builder.Append(item.Caption);
				builder.Append("</a></td>\r\n");
			}
			if (num != 0)
			{
				builder.Append("<td width=13>");
				builder.Append("<img src=\"data:image/png;base64," + StartPageThemeService.Line_hor_blackImageStream + "\" width=1 height=0></td>");
			}
		}
		builder.Append("</tr></table>\r\n");
		builder.Append("</td></tr>");
		builder.Append("<tr height=10>");
		builder.Append("<td>");
		builder.Append("<p class=\"septextP\">&nbsp;</p>");
		builder.Append("</td>");
		builder.Append("<td></td>");
		builder.Append("</tr>");
		builder.Append("<tr>");
		builder.Append("<td ><img src=\"data:image/png;base64," + StartPageThemeService.BlindImageStream + "\" height=0></td>");
		builder.Append("<td width=\"100%\" ");
		builder.Append(" bgcolor=\"" + PrimaryColor + "\"");
		builder.Append(" class=\"navi\">");
		builder.Append("<p class=\"head\">&nbsp;&nbsp;&nbsp;&nbsp;");
		builder.Append(ContentBarText);
		builder.Append("</p>");
		builder.Append("</td>\r\n</tr>");
		builder.Append("<tr height=10>");
		builder.Append("<td>");
		builder.Append("<p class=\"septextP\">&nbsp;</p>");
		builder.Append("</td>");
		builder.Append("<td></td>");
		builder.Append("</tr>");
		builder.Append("<tr bgcolor=\"" + SecondaryColor + "\">");
		builder.Append("<td width=13 bgcolor=\"" + SecondaryColor + "\"></td>");
		builder.Append("<td width=\"100%\" bgcolor=\"" + SecondaryColor + "\">");
		builder.Append("<table border=0 cellspacing=0 cellpadding=0 width=\"100%\" bgcolor=\"" + SecondaryColor + "\">");
		builder.Append("<tr>");
		builder.Append("<td width=20><img src=\"data:image/png;base64," + StartPageThemeService.BlindImageStream + "\" width=20 height=0></td>");
		builder.Append("<td>\r\n");
		RenderRecentTable(section, builder);
		builder.Append("\r\n</td>");
		builder.Append("<td width=20><img src=\"data:image/png;base64," + StartPageThemeService.BlindImageStream + "\" width=20 height=0></td>");
		builder.Append("</tr>");
		builder.Append("</table>\r\n");
		builder.Append("</td>");
		builder.Append("</tr>");
		builder.Append("</table></div>\r\n");
	}

	public RecentOpen.RecentOpenDescription GetRecentFileDescription(string sectionName, int index)
	{
		if (originalSectionNames.ContainsKey(sectionName.ToLowerInvariant()))
		{
			sectionName = originalSectionNames[sectionName.ToLowerInvariant()];
		}
		if (!recentFiles.ContainsKey(sectionName))
		{
			return null;
		}
		if (index >= recentFiles[sectionName].Count)
		{
			return null;
		}
		return recentFiles[sectionName][index];
	}

	public void RemoveRecentFileDescription(string sectionName, int index)
	{
		if (originalSectionNames.ContainsKey(sectionName.ToLowerInvariant()))
		{
			sectionName = originalSectionNames[sectionName.ToLowerInvariant()];
		}
		FileService.RecentOpen.RemoveItem(sectionName, index);
	}

	public void RenderOpenAndNewButton(string sectionName, StringBuilder builder)
	{
		if (originalSectionNames.ContainsKey(sectionName.ToLowerInvariant()))
		{
			sectionName = originalSectionNames[sectionName.ToLowerInvariant()];
		}
		if (!FileService.RecentOpen.IsCategoryExists(sectionName))
		{
			return;
		}
		StringBuilder stringBuilder = null;
		if (recentPagesContent.ContainsKey(sectionName))
		{
			stringBuilder = recentPagesContent[sectionName];
		}
		if (stringBuilder == null)
		{
			string text = null;
			if (HasRecentEvent("Open", sectionName))
			{
				text = " Open " + StringParser.Parse("${res:StartPage.StartMenu.ToolTip" + sectionName + "}");
				LoggingService.Debug(text);
				builder.Append(string.Format("<button class='cusbutton' title=\"" + text + "\" id=\"Open{0}\">{1}</button>\n", sectionName, StringParser.Parse("${res:StartPage.StartMenu.Open" + sectionName + "Button}")));
			}
			if (HasRecentEvent("New", sectionName))
			{
				text = " Create " + StringParser.Parse("${res:StartPage.StartMenu.ToolTip" + sectionName + "}");
				builder.Append(string.Format("<button class='cusbutton' title=\"" + text + "\" id=\"New{0}\">{1}</button>\n", sectionName, StringParser.Parse("${res:StartPage.StartMenu.New" + sectionName + "Button}")));
			}
			builder.Append("<BR>");
		}
	}

	public void RenderRecentTable(string sectionName, StringBuilder builder)
	{
		if (originalSectionNames.ContainsKey(sectionName.ToLowerInvariant()))
		{
			sectionName = originalSectionNames[sectionName.ToLowerInvariant()];
		}
		if (!FileService.RecentOpen.IsCategoryExists(sectionName))
		{
			return;
		}
		StringBuilder stringBuilder = null;
		if (recentPagesContent.ContainsKey(sectionName))
		{
			stringBuilder = recentPagesContent[sectionName];
		}
		if (stringBuilder == null)
		{
			stringBuilder = new StringBuilder();
			RenderOpenAndNewButton(sectionName, stringBuilder);
			stringBuilder.Append("<DIV bgcolor=\"" + SecondaryColor + "\"class='tablediv' width=\"100%\">");
			stringBuilder.Append("<TABLE CLASS='dtTABLE' CELLSPACING='0' width=\"100%\">\n");
			stringBuilder.Append(string.Format("<TR><TH>{0}</TH><TH width=\"45px\">{1}</TH><TH colspan=\"2\">{2}</TH></TR>\n", StringParser.Parse("${res:Global.Name}"), StringParser.Parse("${res:StartPage.StartMenu.ModifiedTable}"), StringParser.Parse("${res:StartPage.StartMenu.LocationTable}")));
			try
			{
				bool removeMissingFileEnties = RecentOpen.RemoveMissingFileEnties;
				int maximumEntriesPerCategory = RecentOpen.MaximumEntriesPerCategory;
				IList<RecentOpen.RecentOpenDescription> recentsFromCategory = FileService.RecentOpen.GetRecentsFromCategory(sectionName);
				recentFiles[sectionName] = new List<RecentOpen.RecentOpenDescription>(recentsFromCategory);
				int num = 0;
				for (int i = 0; i < recentsFromCategory.Count; i++)
				{
					string fileName = recentsFromCategory[i].FileName;
					Properties additionalProperties = recentsFromCategory[i].AdditionalProperties;
					if (!File.Exists(fileName) && removeMissingFileEnties && (additionalProperties == null || !additionalProperties.Get("AlwaysShow", defaultValue: false)))
					{
						continue;
					}
					num++;
					if (num > maximumEntriesPerCategory)
					{
						break;
					}
					if (num % 2 == 0)
					{
						stringBuilder.Append("<TR bgcolor=\"" + GridBodyColor + "\">");
					}
					else
					{
						stringBuilder.Append("<TR bgcolor=\"" + GridAltBodyColor + "\">");
					}
					stringBuilder.Append("<TD>");
					stringBuilder.Append("<a title=\"Open file " + Path.GetFileName(fileName) + "\" href=\"startpage://" + sectionName.ToLowerInvariant() + "/" + i + "\">");
					if (additionalProperties != null && additionalProperties.Contains("DisplayName"))
					{
						stringBuilder.Append(additionalProperties["DisplayName"]);
					}
					else if (sectionName.ToLowerInvariant() == "file")
					{
						stringBuilder.Append(Path.GetFileName(fileName));
					}
					else
					{
						stringBuilder.Append(Path.GetFileNameWithoutExtension(fileName));
					}
					stringBuilder.Append("</A>");
					stringBuilder.Append("</TD>");
					stringBuilder.Append("<TD style=\"text-align:right\">");
					try
					{
						FileInfo fileInfo = null;
						if (string.IsNullOrEmpty(Path.GetExtension(fileName)) || Path.GetExtension(fileName).ToUpper() != ".SLN")
						{
							fileInfo = new FileInfo(fileName);
						}
						else
						{
							string text = Path.ChangeExtension(fileName, "app");
							if (File.Exists(text))
							{
								fileInfo = new FileInfo(text);
							}
							else
							{
								text = Path.ChangeExtension(fileName, "xapp");
								fileInfo = ((!File.Exists(text)) ? new FileInfo(fileName) : new FileInfo(text));
							}
						}
						stringBuilder.Append(fileInfo.LastWriteTime.ToShortDateString());
					}
					catch
					{
					}
					int num2 = 0;
					num2 = i + 10000;
					stringBuilder.Append("</TD>");
					stringBuilder.Append("<TD>");
					stringBuilder.Append("<a title=\"Open folder\" href=\"startpage://" + sectionName.ToLowerInvariant() + "/" + num2 + "\">");
					stringBuilder.Append(Path.GetDirectoryName(fileName));
					stringBuilder.Append("</A>");
					stringBuilder.Append("</TD>");
					stringBuilder.Append("<TD width=\"15px\">");
					num2 = i + 20000;
					stringBuilder.Append("<A href=\"startpage://" + sectionName.ToLowerInvariant() + "/" + num2 + "\"><IMG height=16 width=16 border=0 alt=\"Remove file from list\" title=\"Remove file from list\" src=\"data:image/png;base64," + StartPageThemeService.DeleteButtonImageStream);
					stringBuilder.Append("\"></A>");
					stringBuilder.Append("</TD>");
					stringBuilder.Append("</TR>\n");
				}
			}
			catch
			{
			}
			stringBuilder.Append("</TABLE>");
			string arg = (PropertyService.Get("RemoveMissingRecents", defaultValue: true) ? "checked=\"checked\" " : string.Empty);
			stringBuilder.Append("<p class=\"copyRightText\">");
			stringBuilder.Append($"<input id=\"DeleteMissingCheckbox\" {arg} type=\"checkbox\"/>Remove missing recents from the list");
			stringBuilder.Append("</p>");
			stringBuilder.Append("</DIV>");
			stringBuilder.Append("<p class=\"copyRightText\">" + AboutSharpDevelopTabPage.LicenseSentenceN + "</p>");
			recentPagesContent[sectionName] = stringBuilder;
		}
		builder.Append(stringBuilder.ToString());
	}

	public string Render(string section)
	{
		startPageLocation = FileUtility.Combine(PropertyService.DataDirectory, "resources", "startpage");
		if (originalSectionNames.ContainsKey(section.ToLowerInvariant()))
		{
			section = originalSectionNames[section.ToLowerInvariant()];
		}
		string text = "${res:StartPage.StartMenu.Bar" + section + "Name}";
		ContentBarText = StringParser.Parse(text);
		if (ContentBarText == text)
		{
			ContentBarText = string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(2048);
		RenderHeaderSection(stringBuilder);
		RenderPageTopSection(section, stringBuilder);
		return stringBuilder.ToString();
	}
}
