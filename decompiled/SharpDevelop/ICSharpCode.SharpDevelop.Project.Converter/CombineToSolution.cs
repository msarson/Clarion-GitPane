using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Converter;

public static class CombineToSolution
{
	private static Regex combineLinePattern = new Regex("<Entry filename=\"(?<FileName>.*)\"", RegexOptions.Compiled);

	private static string ReadContent(string fileName)
	{
		using StreamReader streamReader = File.OpenText(fileName);
		return streamReader.ReadToEnd();
	}

	private static void ReadProjects(Solution newSolution, string fileName, List<string> projectFiles)
	{
		string directoryName = Path.GetDirectoryName(fileName);
		string input = ReadContent(fileName);
		Match match = combineLinePattern.Match(input);
		while (match.Success)
		{
			string text = Path.Combine(directoryName, match.Result("${FileName}"));
			if (".CMBX".Equals(Path.GetExtension(text), StringComparison.OrdinalIgnoreCase))
			{
				ReadProjects(newSolution, text, projectFiles);
			}
			else
			{
				projectFiles.Add(text);
			}
			match = match.NextMatch();
		}
	}

	private static bool IsVisualBasic(string prjx)
	{
		using XmlTextReader xmlTextReader = new XmlTextReader(prjx);
		xmlTextReader.Read();
		return xmlTextReader.GetAttribute("projecttype") == "VBNET";
	}

	public static void ConvertSolution(Solution newSolution, string fileName)
	{
		List<string> projectFiles = new List<string>();
		ReadProjects(newSolution, fileName, projectFiles);
		Convert(newSolution, projectFiles);
	}

	public static void ConvertProject(Solution newSolution, string projectFileName)
	{
		List<string> list = new List<string>();
		list.Add(projectFileName);
		Convert(newSolution, list);
	}

	private static void Convert(Solution newSolution, List<string> projectFiles)
	{
		PrjxToSolutionProject.Conversion conversion = new PrjxToSolutionProject.Conversion();
		foreach (string projectFile in projectFiles)
		{
			string projectName = PrjxToSolutionProject.Conversion.GetProjectName(projectFile);
			conversion.NameToGuid[projectName] = Guid.NewGuid();
			if (IsVisualBasic(projectFile))
			{
				conversion.NameToPath[projectName] = Path.ChangeExtension(projectFile, ".vbproj");
			}
			else
			{
				conversion.NameToPath[projectName] = Path.ChangeExtension(projectFile, ".csproj");
			}
		}
		foreach (string projectFile2 in projectFiles)
		{
			conversion.IsVisualBasic = IsVisualBasic(projectFile2);
			IProject folder = PrjxToSolutionProject.ConvertOldProject(projectFile2, conversion, newSolution);
			newSolution.AddFolder(folder);
		}
		if (conversion.Resources != null)
		{
			if (conversion.Resources.Count == 0)
			{
				MessageService.ShowMessage("${res:SharpDevelop.Solution.ImportResourceWarning}");
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder("${res:SharpDevelop.Solution.ImportResourceWarning}");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("${res:SharpDevelop.Solution.ImportResourceWarningErrorText}");
				foreach (string resource in conversion.Resources)
				{
					stringBuilder.AppendLine(resource);
				}
				MessageService.ShowMessage(stringBuilder.ToString());
			}
		}
		newSolution.Save();
	}
}
