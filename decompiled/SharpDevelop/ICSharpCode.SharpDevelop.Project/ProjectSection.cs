using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ICSharpCode.SharpDevelop.Project;

public class ProjectSection
{
	private string name;

	private string sectionType;

	private List<SolutionItem> items = new List<SolutionItem>();

	private static Regex sectionPattern = new Regex("\\s*(?<Key>.*\\S)\\s*=\\s*(?<Value>.*\\S)\\s*", RegexOptions.Compiled);

	public string Name => name;

	public string SectionType => sectionType;

	public List<SolutionItem> Items => items;

	public ProjectSection(string name, string sectionType)
	{
		this.name = name;
		this.sectionType = sectionType;
	}

	public void AppendSection(StringBuilder sb, string indentString)
	{
		foreach (SolutionItem item in items)
		{
			item.AppendItem(sb, indentString);
		}
	}

	public static ProjectSection ReadGlobalSection(TextReader sr, string name, string sectionType)
	{
		return ReadSection(sr, name, sectionType, "EndGlobalSection");
	}

	public static ProjectSection ReadProjectSection(TextReader sr, string name, string sectionType)
	{
		return ReadSection(sr, name, sectionType, "EndProjectSection");
	}

	private static ProjectSection ReadSection(TextReader sr, string name, string sectionType, string endTag)
	{
		ProjectSection projectSection = new ProjectSection(name, sectionType);
		while (true)
		{
			string text = sr.ReadLine();
			if (text == null || text.Trim() == endTag)
			{
				break;
			}
			Match match = sectionPattern.Match(text);
			if (match.Success)
			{
				projectSection.Items.Add(new SolutionItem(match.Result("${Key}"), match.Result("${Value}")));
			}
		}
		return projectSection;
	}
}
