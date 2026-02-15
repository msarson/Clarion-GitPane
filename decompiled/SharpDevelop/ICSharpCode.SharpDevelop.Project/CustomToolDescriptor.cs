using System.Text.RegularExpressions;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

internal sealed class CustomToolDescriptor
{
	private string name;

	private string fileNamePattern;

	private string className;

	private ICustomTool tool;

	private AddIn addIn;

	public string Name => name;

	public ICustomTool Tool
	{
		get
		{
			if (tool == null)
			{
				tool = (ICustomTool)addIn.CreateObject(className);
			}
			return tool;
		}
	}

	public bool CanRunOnFile(string fileName)
	{
		if (string.IsNullOrEmpty(fileNamePattern))
		{
			return true;
		}
		return Regex.IsMatch(fileName, fileNamePattern, RegexOptions.IgnoreCase);
	}

	public CustomToolDescriptor(string name, string fileNamePattern, string className, AddIn addIn)
	{
		this.name = name;
		this.fileNamePattern = fileNamePattern;
		this.className = className;
		this.addIn = addIn;
	}
}
