using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Clarion.Core.Redirection;
using Clarion.GEN;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common;

namespace SoftVelocity.Generator.CommandLine;

internal class GenerateApplication : GeneratorCL
{
	public override bool Enabled => (int)VersionService.Version == 1;

	private void Generate(string slnFile, List<string> appFileNames, ICommandLineLogger logger, RedirectionFile red)
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (appFileNames.Count <= 0)
		{
			return;
		}
		if (string.IsNullOrEmpty(slnFile))
		{
			slnFile = Path.ChangeExtension(appFileNames[0], "sln");
		}
		bool flag = !File.Exists(slnFile);
		try
		{
			if (!flag)
			{
				ProjectService.LoadSolution(slnFile);
			}
			string text = null;
			List<Application> list = new List<Application>();
			string text2 = null;
			foreach (string appFileName in appFileNames)
			{
				text2 = ((!red.Exists(appFileName, ".")) ? red.CreateName(appFileName, ".") : red.OpenName(appFileName, "."));
				text = Path.ChangeExtension(text2, "cwproj");
				if (File.Exists(text))
				{
					ProjectService.LoadProject(text);
				}
				Application application = ApplicationService.FetchApplication(text2);
				application.QuietConvert = SetForceUpgrade.forceUpgrade;
				list.Add(application);
			}
			AppWatcher.Instance.CanExport = false;
			Win32Generator.CommandLineLogger = logger;
			ApplicationService.GenerateApplication(list, SetConditionalGeneration.generationMode, SetDebugGeneration.debugMode);
			foreach (string appFileName2 in appFileNames)
			{
				FileUtility.ObservedSave(new FileOperationDelegate(DummyOp), appFileName2);
			}
		}
		finally
		{
			if (flag && File.Exists(slnFile))
			{
				try
				{
					File.Delete(slnFile);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void Generate(string file, ICommandLineLogger logger, RedirectionFile red)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		string text = Path.ChangeExtension(file, "cwproj");
		string path = Path.ChangeExtension(file, "sln");
		bool flag = !File.Exists(path);
		ApplicationService.OnApplicationLoading(file);
		try
		{
			if (File.Exists(text))
			{
				ProjectService.LoadProject(text);
			}
			AppWatcher.Instance.CanExport = false;
			Win32Generator.CommandLineLogger = logger;
			Application application = ApplicationService.FetchApplication(file);
			application.QuietConvert = SetForceUpgrade.forceUpgrade;
			ApplicationService.GenerateApplication(application, SetConditionalGeneration.generationMode, SetDebugGeneration.debugMode);
			FileUtility.ObservedSave(new FileOperationDelegate(DummyOp), file);
		}
		finally
		{
			if (flag && File.Exists(path))
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void DummyOp()
	{
	}

	private void GenerateSln(string slnfile, ICommandLineLogger logger, RedirectionFile red)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Invalid comparison between Unknown and I4
		List<string> list = new List<string>();
		using (StreamReader streamReader = new StreamReader(red.OpenName(slnfile, "."), Encoding.Default, detectEncodingFromByteOrderMarks: true))
		{
			string text = "";
			bool flag = false;
			Regex regex = new Regex(".*ProjectSection *\\( *SolutionItems *\\)");
			while (!flag && (text = streamReader.ReadLine()) != null)
			{
				if (regex.IsMatch(text))
				{
					flag = true;
				}
			}
			if (flag)
			{
				while ((text = streamReader.ReadLine()) != null)
				{
					string[] array = text.Split('=');
					if (array.Length < 2)
					{
						break;
					}
					string item = array[1].Trim();
					list.Add(item);
				}
			}
			streamReader.Close();
		}
		if (list.Count <= 0)
		{
			return;
		}
		string currentDirectory = Directory.GetCurrentDirectory();
		string directoryName = Path.GetDirectoryName(slnfile);
		bool flag2 = !string.IsNullOrEmpty(directoryName);
		if (flag2)
		{
			Directory.SetCurrentDirectory(directoryName);
		}
		if (logger != null)
		{
			logger.Message(string.Format(ResourceService.GetString("Clarion.Generator.CommandLine.Generating"), slnfile));
		}
		if ((int)VersionService.Version == 1)
		{
			Generate(slnfile, list, logger, red);
		}
		else
		{
			foreach (string item2 in list)
			{
				if (logger != null)
				{
					logger.Message(string.Format(ResourceService.GetString("Clarion.Generator.CommandLine.Generating"), item2));
				}
				Generate(item2, logger, red);
			}
		}
		if (flag2)
		{
			Directory.SetCurrentDirectory(currentDirectory);
		}
	}

	protected override void DoRun(List<List<string>> parameters, ICommandLineLogger logger, RedirectionFile red)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		Properties val = PropertyService.Get<Properties>("ClarionWindows.GeneralOptions", new Properties());
		string text = val.Get<string>("ProjectParsingCondition", Enum.GetName(typeof(CommonClarionProject.ProjectParsingConditions), CommonClarionProject.ProjectParsingConditions.ParseHandCodedAndOpenedApp));
		val.Set<string>("ProjectParsingCondition", Enum.GetName(typeof(CommonClarionProject.ProjectParsingConditions), CommonClarionProject.ProjectParsingConditions.DoNotParse));
		foreach (List<string> parameter in parameters)
		{
			string text2 = parameter[0];
			if (text2.ToLower().EndsWith(".app"))
			{
				Generate(text2, logger, red);
			}
			else if (text2.ToLower().EndsWith(".sln"))
			{
				GenerateSln(text2, logger, red);
			}
			else
			{
				logger.Error("GEN000", "Only Application or Solution (containing application) can be processed to generate code.");
			}
		}
		val.Set<string>("ProjectParsingCondition", text);
	}
}
