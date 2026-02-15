using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace ICSharpCode.SharpDevelop.Project;

public abstract class CompilableProject : MSBuildBasedProject
{
	protected readonly Set<string> reparseReferencesSensitiveProperties = new Set<string>();

	protected readonly Set<string> reparseCodeSensitiveProperties = new Set<string>();

	[Browsable(false)]
	public string IntermediateOutputFullPath
	{
		get
		{
			string text = GetEvaluatedProperty("IntermediateOutputPath");
			if (string.IsNullOrEmpty(text))
			{
				text = GetEvaluatedProperty("BaseIntermediateOutputPath");
				if (string.IsNullOrEmpty(text))
				{
					text = "obj";
				}
				text = Path.Combine(text, base.ActiveConfiguration);
			}
			return Path.Combine(base.Directory, text);
		}
	}

	[Browsable(false)]
	public string DocumentationFileFullPath
	{
		get
		{
			string evaluatedProperty = GetEvaluatedProperty("DocumentationFile");
			if (string.IsNullOrEmpty(evaluatedProperty))
			{
				return null;
			}
			return Path.Combine(base.Directory, evaluatedProperty);
		}
	}

	public abstract override string Language { get; }

	public abstract override LanguageProperties LanguageProperties { get; }

	public override string AssemblyName
	{
		get
		{
			return GetEvaluatedProperty("AssemblyName") ?? base.Name;
		}
		set
		{
			SetProperty("AssemblyName", value);
		}
	}

	public override string RootNamespace
	{
		get
		{
			return GetEvaluatedProperty("RootNamespace") ?? "";
		}
		set
		{
			SetProperty("RootNamespace", value);
		}
	}

	public override string OutputAssemblyFullPath
	{
		get
		{
			string path = GetEvaluatedProperty("OutputPath") ?? "";
			return Path.Combine(Path.Combine(base.Directory, path), AssemblyName + GetExtension(OutputType));
		}
	}

	[Browsable(false)]
	public OutputType OutputType
	{
		get
		{
			try
			{
				return (OutputType)Enum.Parse(typeof(OutputType), GetEvaluatedProperty("OutputType") ?? "Exe");
			}
			catch (ArgumentException)
			{
				return OutputType.Exe;
			}
		}
		set
		{
			SetProperty("OutputType", value.ToString());
		}
	}

	public override bool IsStartable
	{
		get
		{
			switch (StartAction)
			{
			case StartAction.Project:
				if (OutputType != OutputType.Exe)
				{
					return OutputType == OutputType.WinExe;
				}
				return true;
			case StartAction.Program:
				return StartProgram.Length > 0;
			case StartAction.StartURL:
				return StartUrl.Length > 0;
			default:
				return false;
			}
		}
	}

	[Browsable(false)]
	public string StartProgram
	{
		get
		{
			return GetEvaluatedProperty("StartProgram") ?? "";
		}
		set
		{
			SetProperty("StartProgram", string.IsNullOrEmpty(value) ? null : value);
		}
	}

	[Browsable(false)]
	public string StartUrl
	{
		get
		{
			return GetEvaluatedProperty("StartURL") ?? "";
		}
		set
		{
			SetProperty("StartURL", string.IsNullOrEmpty(value) ? null : value);
		}
	}

	[Browsable(false)]
	public StartAction StartAction
	{
		get
		{
			try
			{
				return (StartAction)Enum.Parse(typeof(StartAction), GetEvaluatedProperty("StartAction") ?? "Project");
			}
			catch (ArgumentException)
			{
				return StartAction.Project;
			}
		}
		set
		{
			SetProperty("StartAction", value.ToString());
		}
	}

	[Browsable(false)]
	public string StartArguments
	{
		get
		{
			return GetEvaluatedProperty("StartArguments") ?? "";
		}
		set
		{
			SetProperty("StartArguments", string.IsNullOrEmpty(value) ? null : value);
		}
	}

	[Browsable(false)]
	public string StartWorkingDirectory
	{
		get
		{
			return GetEvaluatedProperty("StartWorkingDirectory") ?? "";
		}
		set
		{
			SetProperty("StartWorkingDirectory", string.IsNullOrEmpty(value) ? null : value);
		}
	}

	[Browsable(false)]
	public override string TypeGuid
	{
		get
		{
			return LanguageBindingService.GetCodonPerLanguageName(Language).Guid;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public static string GetExtension(OutputType outputType)
	{
		switch (outputType)
		{
		case OutputType.Exe:
		case OutputType.WinExe:
			return ".exe";
		case OutputType.Module:
			return ".netmodule";
		default:
			return ".dll";
		}
	}

	protected CompilableProject(IMSBuildEngineProvider engineProvider)
		: base(engineProvider.BuildEngine)
	{
	}

	protected virtual void SetTargetFrameworkVersion()
	{
		SetProperty("TargetFrameworkVersion", "v4.0");
	}

	protected override void Create(ProjectCreateInformation information)
	{
		base.Create(information);
		base.MSBuildProject.DefaultTargets = "Build";
		OutputType = OutputType.Exe;
		RootNamespace = information.RootNamespace;
		AssemblyName = information.ProjectName;
		SetTargetFrameworkVersion();
		SetProperty("Debug", null, "OutputPath", "bin\\Debug\\", PropertyStorageLocations.ConfigurationSpecific, treatPropertyValueAsLiteral: true);
		SetProperty("Release", null, "OutputPath", "bin\\Release\\", PropertyStorageLocations.ConfigurationSpecific, treatPropertyValueAsLiteral: true);
		SetProperty("Debug", null, "DebugSymbols", "True", PropertyStorageLocations.ConfigurationSpecific, treatPropertyValueAsLiteral: true);
		SetProperty("Release", null, "DebugSymbols", "False", PropertyStorageLocations.ConfigurationSpecific, treatPropertyValueAsLiteral: true);
		SetProperty("Debug", null, "DebugType", "Full", PropertyStorageLocations.ConfigurationSpecific, treatPropertyValueAsLiteral: true);
		SetProperty("Release", null, "DebugType", "None", PropertyStorageLocations.ConfigurationSpecific, treatPropertyValueAsLiteral: true);
	}

	protected override ParseProjectContent CreateProjectContent()
	{
		return ParseProjectContent.CreateUninitalized(this);
	}

	protected void Start(string program, bool withDebugging)
	{
		ProcessStartInfo processStartInfo = new ProcessStartInfo();
		processStartInfo.FileName = Path.Combine(base.Directory, program);
		string text = StringParser.Parse(StartWorkingDirectory);
		if (text.Length == 0)
		{
			processStartInfo.WorkingDirectory = Path.GetDirectoryName(processStartInfo.FileName);
		}
		else
		{
			processStartInfo.WorkingDirectory = Path.Combine(base.Directory, text);
		}
		processStartInfo.Arguments = StringParser.Parse(StartArguments);
		if (!File.Exists(processStartInfo.FileName))
		{
			MessageService.ShowError(processStartInfo.FileName + " does not exist and cannot be started.");
		}
		else if (!System.IO.Directory.Exists(processStartInfo.WorkingDirectory))
		{
			MessageService.ShowError("Working directory " + processStartInfo.WorkingDirectory + " does not exist; the process cannot be started. You can specify the working directory in the project options.");
		}
		else if (withDebugging)
		{
			DebuggerService.CurrentDebugger.Start(processStartInfo);
		}
		else
		{
			DebuggerService.CurrentDebugger.StartWithoutDebugging(processStartInfo);
		}
	}

	public override void Start(bool withDebugging)
	{
		switch (StartAction)
		{
		case StartAction.Project:
			Start(OutputAssemblyFullPath, withDebugging);
			break;
		case StartAction.Program:
			Start(StartProgram, withDebugging);
			break;
		case StartAction.StartURL:
			FileService.OpenFile("browser://" + StartUrl);
			break;
		default:
			throw new InvalidEnumArgumentException("StartAction", (int)StartAction, typeof(StartAction));
		}
	}

	protected override void OnActiveConfigurationChanged(EventArgs e)
	{
		base.OnActiveConfigurationChanged(e);
		if (!isLoading && !ProjectService.IsLoading)
		{
			ParserService.Reparse(this, initReferences: true, parseCode: true);
		}
	}

	protected override void OnActivePlatformChanged(EventArgs e)
	{
		base.OnActivePlatformChanged(e);
		if (!isLoading && !ProjectService.IsLoading)
		{
			ParserService.Reparse(this, initReferences: true, parseCode: true);
		}
	}

	protected override void OnPropertyChanged(ProjectPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (reparseReferencesSensitiveProperties.Contains(e.PropertyName))
		{
			ParserService.Reparse(this, initReferences: true, parseCode: false);
		}
		if (reparseCodeSensitiveProperties.Contains(e.PropertyName))
		{
			ParserService.Reparse(this, initReferences: false, parseCode: true);
		}
	}

	public override ItemType GetDefaultItemType(string fileName)
	{
		string extension = Path.GetExtension(fileName);
		if (".resx".Equals(extension, StringComparison.OrdinalIgnoreCase) || ".resources".Equals(extension, StringComparison.OrdinalIgnoreCase))
		{
			return ItemType.EmbeddedResource;
		}
		if (".xaml".Equals(extension, StringComparison.OrdinalIgnoreCase))
		{
			return ItemType.Page;
		}
		return base.GetDefaultItemType(fileName);
	}
}
