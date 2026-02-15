using System;
using System.Text;

namespace ICSharpCode.SharpDevelop.Project;

public class SolutionItem
{
	private string name;

	private string location;

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

	public string Location
	{
		get
		{
			return location;
		}
		set
		{
			location = value;
		}
	}

	public SolutionItem(string name, string location)
	{
		this.name = name;
		this.location = location;
	}

	public void AppendItem(StringBuilder sb, string indentString)
	{
		sb.Append(indentString);
		sb.Append(Name);
		sb.Append(" = ");
		sb.Append(Location);
		sb.Append(Environment.NewLine);
	}

	public override string ToString()
	{
		return $"[SolutionItem: location = {location}, name = {name}]";
	}
}
