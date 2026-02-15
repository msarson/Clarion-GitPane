using System;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop;

public abstract class Net1xProjectContentRegistry : ProjectContentRegistry
{
	protected abstract string DotnetVersion { get; }

	public override IProjectContent Mscorlib => GetProjectContentForReference("mscorlib", "mscorlib", null);

	protected override IProjectContent LoadProjectContent(string itemInclude, string itemFileName, string itemEvaluatedName)
	{
		if (File.Exists(itemFileName))
		{
			return base.LoadProjectContent(itemInclude, itemFileName, itemEvaluatedName);
		}
		string path = Path.Combine(FileUtility.NETFrameworkInstallRoot, DotnetVersion);
		if (File.Exists(Path.Combine(path, "mscorlib.dll")))
		{
			string text = itemInclude;
			int num = text.IndexOf(',');
			if (num > 0)
			{
				text = text.Substring(0, num);
			}
			if (File.Exists(Path.Combine(path, text + ".dll")))
			{
				ReflectionProjectContent reflectionProjectContent = CecilReader.LoadAssembly(Path.Combine(path, text + ".dll"), this);
				if (reflectionProjectContent != null)
				{
					redirectedAssemblyNames.Add(text, reflectionProjectContent.AssemblyFullName);
				}
				return reflectionProjectContent;
			}
			if (File.Exists(Path.Combine(path, text)))
			{
				ReflectionProjectContent reflectionProjectContent2 = CecilReader.LoadAssembly(Path.Combine(path, text), this);
				if (reflectionProjectContent2 != null)
				{
					redirectedAssemblyNames.Add(Path.GetFileNameWithoutExtension(text), reflectionProjectContent2.AssemblyFullName);
				}
				return reflectionProjectContent2;
			}
			if (itemEvaluatedName != null && File.Exists(itemEvaluatedName))
			{
				ReflectionProjectContent reflectionProjectContent3 = CecilReader.LoadAssembly(itemEvaluatedName, this);
				if (reflectionProjectContent3 != null)
				{
					redirectedAssemblyNames.Add(Path.GetFileNameWithoutExtension(itemEvaluatedName), reflectionProjectContent3.AssemblyFullName);
				}
				return reflectionProjectContent3;
			}
		}
		else
		{
			string text2 = "Warning: Target .NET Framework version " + DotnetVersion + " is not installed." + Environment.NewLine;
			if (!TaskService.BuildMessageViewCategory.Text.Contains(text2))
			{
				TaskService.BuildMessageViewCategory.AppendText(text2);
			}
		}
		return base.LoadProjectContent(itemInclude, itemFileName, itemEvaluatedName);
	}
}
