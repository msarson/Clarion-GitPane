using System;
using System.IO;
using ICSharpCode.SharpDevelop.Dom;
using Microsoft.Win32;

namespace ICSharpCode.SharpDevelop;

public class NetCF20ProjectContentRegistry : ProjectContentRegistry
{
	public override IProjectContent Mscorlib => GetProjectContentForReference("mscorlib", "mscorlib", null);

	private static string GetInstallFolder()
	{
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETCompactFramework\\v2.0.0.0\\WindowsCE\\AssemblyFoldersEx");
		if (registryKey != null)
		{
			string result = registryKey.GetValue(null) as string;
			registryKey.Close();
			return result;
		}
		return null;
	}

	protected override IProjectContent LoadProjectContent(string itemInclude, string itemFileName, string itemEvaluatedName)
	{
		if (File.Exists(itemFileName))
		{
			return base.LoadProjectContent(itemInclude, itemFileName, itemEvaluatedName);
		}
		string installFolder = GetInstallFolder();
		if (!string.IsNullOrEmpty(installFolder) && File.Exists(Path.Combine(installFolder, "mscorlib.dll")))
		{
			string text = itemInclude;
			int num = text.IndexOf(',');
			if (num > 0)
			{
				text = text.Substring(0, num);
			}
			if (File.Exists(Path.Combine(installFolder, text + ".dll")))
			{
				ReflectionProjectContent reflectionProjectContent = CecilReader.LoadAssembly(Path.Combine(installFolder, text + ".dll"), this);
				if (reflectionProjectContent != null)
				{
					redirectedAssemblyNames.Add(text, reflectionProjectContent.AssemblyFullName);
				}
				return reflectionProjectContent;
			}
			if (File.Exists(Path.Combine(installFolder, text)))
			{
				ReflectionProjectContent reflectionProjectContent2 = CecilReader.LoadAssembly(Path.Combine(installFolder, text), this);
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
			string text2 = "Warning: .NET Compact Framework SDK is not installed." + Environment.NewLine;
			if (!TaskService.BuildMessageViewCategory.Text.Contains(text2))
			{
				TaskService.BuildMessageViewCategory.AppendText(text2);
			}
		}
		return base.LoadProjectContent(itemInclude, itemFileName, itemEvaluatedName);
	}
}
