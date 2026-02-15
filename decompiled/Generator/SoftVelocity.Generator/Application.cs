using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Clarion.Core.Redirection;
using Clarion.GEN;
using Clarion.PRJ;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common;
using SoftVelocity.DataDictionary.Schema;

namespace SoftVelocity.Generator;

public class Application : LocalizedObject, IDisposable
{
	private static Properties appprop = PropertyService.Get<Properties>("SoftVelocity.Generator.ApplicationService", new Properties());

	private static Properties w32appprop = PropertyService.Get<Properties>("Application", new Properties());

	private bool _Loaded;

	private bool _IsUnloading;

	internal bool InCheckLazyLoad;

	private string _FileName;

	private string _Language = string.Empty;

	private bool suspendUpdateLanguage;

	private LanguageBindingDescriptor _LanguageBinding;

	private IProject _iPrj;

	private string _ProjectName;

	private PRJFile _OldAppPrj;

	private PRJFile _AppPrj;

	private Win32App _App;

	private DateTime originalTimeStamp;

	private SolutionItem _SolutionItem;

	private bool _Closed;

	private bool _InApptree;

	private string _TargetType = string.Empty;

	private bool _BuildingDebugSet;

	private bool _BuildingDebug;

	private string _DictionaryFileName;

	private bool quietMode;

	private bool _IsBusy;

	private bool waitingForMergingToEnd;

	private bool _IsDirty;

	private string _ProgramModuleFileName;

	private List<BuildError> errors = new List<BuildError>();

	internal bool CallingEditError;

	private bool disposed;

	private int _canGenerate;

	[Browsable(false)]
	public bool IsUnloading => _IsUnloading;

	[Browsable(false)]
	public bool IsLoaded => _Loaded;

	internal bool LazyLoadRequired
	{
		get
		{
			if (Win32App == null)
			{
				return !IsClosed;
			}
			return false;
		}
	}

	[LocalizedProperty("${res:Clarion.Generator.Application.FileName}", Description = "${res:Clarion.Generator.Application.Name.Description}", Category = "${res:Clarion.Generator.Application.Information.Category}")]
	public string FileName
	{
		get
		{
			if (!IsClosed)
			{
				if (string.IsNullOrEmpty(_FileName))
				{
					return string.Empty;
				}
				return _FileName;
			}
			return string.Empty;
		}
	}

	[LocalizedProperty("${res:Clarion.Generator.Application.Name}", Description = "${res:Clarion.Generator.Application.Name.Description}", Category = "${res:Clarion.Generator.Application.Information.Category}")]
	public string Name
	{
		get
		{
			if (string.IsNullOrEmpty(FileName))
			{
				return string.Empty;
			}
			return Path.GetFileNameWithoutExtension(FileName);
		}
	}

	[LocalizedProperty("${res:Clarion.Generator.Application.ModificationDate}", Description = "${res:Clarion.Generator.Application.ModificationDate.Description}", Category = "${res:Clarion.Generator.Application.Information.Category}")]
	public DateTime ModificationDate
	{
		get
		{
			if (File.Exists(FileName))
			{
				return File.GetLastWriteTime(FileName);
			}
			return DateTime.MinValue;
		}
	}

	[LocalizedProperty("${res:Clarion.Generator.Application.Location}", Description = "${res:Clarion.Generator.Application.Location.Description}", Category = "${res:Clarion.Generator.Application.Information.Category}")]
	public string Location => Path.GetDirectoryName(FileName);

	[LocalizedProperty("${res:Clarion.Generator.Application.Language}", Description = "${res:Clarion.Generator.Application.Language.Description}", Category = "${res:Clarion.Generator.Application.Information.Category}")]
	public string Language
	{
		get
		{
			if (!IsClosed)
			{
				UpdateLanguage();
				return _Language;
			}
			return string.Empty;
		}
	}

	[Browsable(false)]
	public LanguageBindingDescriptor LanguageBinding
	{
		get
		{
			UpdateLanguage();
			return _LanguageBinding;
		}
	}

	[LocalizedProperty("${res:Clarion.Generator.Application.ProjectFileName}", Description = "${res:Clarion.Generator.Application.ProjectFileName.Description}", Category = "${res:Clarion.Generator.Application.Information.Category}")]
	public string ProjectFileName => _ProjectName;

	internal bool AppPrjInited
	{
		get
		{
			if (_OldAppPrj != null)
			{
				return _AppPrj != null;
			}
			return false;
		}
	}

	[Browsable(false)]
	internal PRJFile OldAppPrj
	{
		get
		{
			if (_OldAppPrj == null)
			{
				if (!IsLoaded)
				{
					PRJFile appProject = null;
					string appLanguage = string.Empty;
					ApplicationService.GetApplicationProjectFile(Name, out appProject, out appLanguage);
					_Language = appLanguage;
					return appProject;
				}
				_OldAppPrj = Win32App.ProjectFile;
			}
			return _OldAppPrj;
		}
		set
		{
			_OldAppPrj = value;
		}
	}

	[Browsable(false)]
	internal PRJFile AppPrj
	{
		get
		{
			if (_AppPrj == null)
			{
				if (IsLoaded)
				{
					_AppPrj = Win32App.ProjectFile;
				}
				else
				{
					PRJFile appProject = null;
					string appLanguage = string.Empty;
					ApplicationService.GetApplicationProjectFile(Name, out appProject, out appLanguage);
					_Language = appLanguage;
					_AppPrj = appProject;
				}
			}
			return _AppPrj;
		}
	}

	private Win32App Win32App
	{
		get
		{
			return _App;
		}
		set
		{
			if (_App != null)
			{
				_App.IsDirtyChanged -= OnWin32AppIsDirtyChanged;
				_App.IsBusyChanged -= OnWin32AppIsBusyChanged;
				_App.ApplicationSaved -= OnWin32AppSaved;
			}
			_App = value;
		}
	}

	internal bool Win32Loaded
	{
		get
		{
			if (_App != null)
			{
				return _AppPrj != null;
			}
			return false;
		}
	}

	internal int InstID
	{
		get
		{
			if (Win32App == null)
			{
				return 0;
			}
			return Win32App.Id;
		}
	}

	[Browsable(false)]
	internal bool InEdit
	{
		get
		{
			if (IsClosed || !IsLoaded)
			{
				return false;
			}
			return Win32App.Editing;
		}
	}

	[Browsable(false)]
	internal bool InApptree
	{
		get
		{
			if (!InEdit)
			{
				return false;
			}
			return _InApptree;
		}
	}

	[Browsable(false)]
	public bool IsClosed => _Closed;

	[Browsable(false)]
	public bool IsOnSolution
	{
		get
		{
			if (_SolutionItem != null)
			{
				return true;
			}
			return false;
		}
	}

	[LocalizedProperty("${res:Clarion.Generator.Application.TargetType}", Description = "${res:Clarion.Generator.Application.TargetType.Description}", Category = "${res:Clarion.Generator.Application.Information.Category}")]
	public string TargetType => _TargetType;

	[Browsable(false)]
	public bool BuildingDebug
	{
		get
		{
			bool flag = CheckLazyLoad();
			if (!_BuildingDebugSet || !flag)
			{
				_BuildingDebugSet = true;
				if (!IsClosed && IsLoaded)
				{
					_BuildingDebug = Win32App.BuildingDebug;
				}
			}
			if (flag)
			{
				Unload(7L);
			}
			return _BuildingDebug;
		}
		set
		{
			if (Win32App != null)
			{
				Win32App.BuildingDebug = value;
			}
		}
	}

	internal string DictionaryFileName
	{
		get
		{
			bool flag = false;
			if (_DictionaryFileName == null)
			{
				flag = CheckLazyLoad();
			}
			UpdateDictionaryFileName();
			if (flag)
			{
				Unload(8L);
			}
			return _DictionaryFileName;
		}
	}

	internal bool QuietConvert
	{
		get
		{
			return quietMode;
		}
		set
		{
			quietMode = value;
		}
	}

	internal bool IsBusy => _IsBusy;

	internal bool IsDirty
	{
		get
		{
			if (!IsClosed && IsLoaded)
			{
				_IsDirty = Win32App.IsDirty;
			}
			return _IsDirty;
		}
	}

	[LocalizedProperty("${res:Clarion.Generator.Application.IsReadOnly}", Description = "${res:Clarion.Generator.Application.IsReadOnly.Description}", Category = "${res:Clarion.Generator.Application.Information.Category}")]
	public bool IsReadOnly
	{
		get
		{
			if (string.IsNullOrEmpty(FileName))
			{
				return false;
			}
			FileAttributes attributes = File.GetAttributes(FileName);
			if ((attributes & FileAttributes.ReadOnly) != 0)
			{
				return true;
			}
			return false;
		}
	}

	internal FileSchema FileSchema
	{
		get
		{
			if (!IsClosed && IsLoaded)
			{
				return Win32App.Schema;
			}
			return null;
		}
	}

	[Browsable(false)]
	public string ProgramModuleFileName
	{
		get
		{
			if (_ProgramModuleFileName == null)
			{
				bool flag = false;
				flag = CheckLazyLoad();
				UpdateProgramModuleFileName();
				if (flag)
				{
					Unload(3L);
				}
			}
			return _ProgramModuleFileName;
		}
	}

	[Browsable(false)]
	public Module ProgramModule
	{
		get
		{
			if (!IsClosed && IsLoaded)
			{
				return Win32App.ProgramModule;
			}
			return null;
		}
	}

	[Browsable(false)]
	public string[] ProcedureNames
	{
		get
		{
			if (!IsClosed && IsLoaded)
			{
				return Win32App.ProcedureNames;
			}
			return null;
		}
	}

	[Browsable(false)]
	public Module[] Modules
	{
		get
		{
			if (!IsClosed && IsLoaded)
			{
				return Win32App.Modules;
			}
			return null;
		}
	}

	[Browsable(false)]
	public Procedure[] Procedures
	{
		get
		{
			if (!IsClosed && IsLoaded)
			{
				return Win32App.Procedures;
			}
			return null;
		}
	}

	[Browsable(false)]
	public Procedure FirstProcedures
	{
		get
		{
			if (!IsClosed && IsLoaded)
			{
				return Win32App.FirstProcedure;
			}
			return null;
		}
	}

	[Browsable(false)]
	public bool CanGenerate => _canGenerate == 0;

	internal event LanguageChangedEventHandler LanguageChanged;

	public event EventHandler<ApplicationEventArgs> LoadedChanged;

	public event EventHandler<ApplicationEventArgs> Closed;

	public event EventHandler<ApplicationRenamedEventArgs> SavedAs;

	internal event BusyChangedEventHandler IsBusyChanged;

	internal event IsDirtyChangedEventHandler IsDirtyChanged;

	internal Application(string fileName, SolutionItem solutionItem)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		Win32App = null;
		_Closed = false;
		_FileName = fileName;
		_SolutionItem = solutionItem;
		_ProjectName = NameToProjectName();
		_iPrj = ProjectService.GetProject(_ProjectName);
		if (_iPrj != null && !(_iPrj is MissingProject))
		{
			_TargetType = Path.GetExtension(_iPrj.OutputAssemblyFullPath).TrimStart('.');
			if (!_TargetType.Equals("Exe", StringComparison.InvariantCultureIgnoreCase) && !_TargetType.Equals("Dll", StringComparison.InvariantCultureIgnoreCase) && !_TargetType.Equals("Lib", StringComparison.InvariantCultureIgnoreCase))
			{
				_TargetType = "Lib";
			}
			ProjectService.ProjectItemRemoved += OnProjectService_ProjectItemRemoved;
			ProjectService.SolutionClosed += SolutionClosed;
			ProjectService.SolutionFolderRemoved += new SolutionFolderEventHandler(OnProjectService_SolutionFolderRemoved);
		}
		SetLoaded(value: false);
	}

	internal Application(string fileName, Win32App app, SolutionItem solutionItem)
		: this(fileName, solutionItem)
	{
		if (app == null)
		{
			throw new ArgumentNullException("app");
		}
		LazyLoad(app);
	}

	private void SetLoaded(bool value)
	{
		if (_Loaded != value)
		{
			_Loaded = value;
			OnLoadedChanged();
		}
	}

	private bool GetUnloadOnFinish()
	{
		if (Win32App == null && !IsClosed)
		{
			return !IsLoaded;
		}
		return false;
	}

	internal bool CheckLazyLoad()
	{
		bool result = false;
		if (LazyLoadRequired)
		{
			StatusBarService.SetMessage($"Loading {Name} ...");
			result = !IsLoaded;
			InCheckLazyLoad = true;
			ApplicationService.OpenApplication(this);
			InCheckLazyLoad = false;
			result = result && IsLoaded;
		}
		return result;
	}

	internal void SetDebugState()
	{
		if (IsBusy || !IsLoaded || ProjectService.OpenSolution == null)
		{
			return;
		}
		string activeConfiguration = ProjectService.OpenSolution.Preferences.ActiveConfiguration;
		bool buildingDebug;
		if (activeConfiguration == "Debug")
		{
			buildingDebug = true;
		}
		else if (activeConfiguration == "Release")
		{
			buildingDebug = false;
		}
		else
		{
			IProject projectServiceProject = GetProjectServiceProject();
			if (projectServiceProject != null)
			{
				CommonClarionProject commonClarionProject = projectServiceProject as CommonClarionProject;
				buildingDebug = projectServiceProject == null || commonClarionProject.ConfigurationIsDebug(ProjectService.OpenSolution.Preferences.ActiveConfiguration);
			}
			else
			{
				buildingDebug = true;
			}
		}
		Win32App.BuildingDebug = buildingDebug;
	}

	internal void LazyLoad(Win32App app)
	{
		if (app != null && Win32App == null && !IsClosed && !IsLoaded && InCheckLazyLoad)
		{
			Win32App = app;
			suspendUpdateLanguage = false;
			PassErrorsToWin32App();
			SetLoaded(value: true);
			UpdateLanguage();
			UpdateDictionaryFileName();
			UpdateProgramModuleFileName();
			SetDebugState();
			_BuildingDebug = Win32App.BuildingDebug;
			_BuildingDebugSet = true;
			_ = OldAppPrj;
			_ = AppPrj;
			_IsBusy = Win32App.IsBusy;
			Win32App.IsDirtyChanged += OnWin32AppIsDirtyChanged;
			Win32App.IsBusyChanged += OnWin32AppIsBusyChanged;
			Win32App.ApplicationSaved += OnWin32AppSaved;
		}
		StatusBarService.ClearMessage();
	}

	protected string GetFileName()
	{
		return _FileName;
	}

	internal void UpdateLanguage()
	{
		if (suspendUpdateLanguage || IsClosed)
		{
			return;
		}
		if (IsLoaded)
		{
			if (Win32App.Language != _Language)
			{
				if (this.LanguageChanged != null)
				{
					this.LanguageChanged(new ApplicationLanguageChangedEventArg(FileName, _Language, Win32App.Language));
				}
				_Language = Win32App.Language;
				UpdateLanguageBinding();
				_ProjectName = NameToProjectName();
				_iPrj = null;
				GetProjectServiceProject();
			}
		}
		else if (_Language == string.Empty)
		{
			_Language = ApplicationService.GetApplicationLanguage(Name);
			UpdateLanguageBinding();
			_ProjectName = NameToProjectName();
		}
	}

	internal void UpdateLanguageBinding()
	{
		_LanguageBinding = LanguageBindingService.GetCodonPerLanguageName(_Language);
		if (_LanguageBinding == null)
		{
			_Language = ApplicationService.GetApplicationLanguage(Name);
			_LanguageBinding = LanguageBindingService.GetCodonPerLanguageName(_Language);
			if (_LanguageBinding == null)
			{
				throw new ApplicationServiceException(Name, string.Format(ResourceService.GetString("Clarion.Generator.ApplicationService.Exception.NoBinding"), Name, _Language));
			}
			suspendUpdateLanguage = true;
			_ProjectName = NameToProjectName();
		}
		if (string.IsNullOrEmpty(_ProjectName))
		{
			_ProjectName = NameToProjectName();
		}
	}

	public IProject GetProjectServiceProject()
	{
		if (_iPrj == null)
		{
			if (!string.IsNullOrEmpty(ProjectFileName))
			{
				_iPrj = ProjectService.GetProject(ProjectFileName);
			}
			if (_iPrj == null)
			{
				_iPrj = ProjectService.GetProject(NameToProjectName());
			}
		}
		return _iPrj;
	}

	internal void GeneratorVersionChanged(Win32GeneratorInstance newGen)
	{
		if (!IsClosed && IsLoaded)
		{
			Win32App.Close();
			_Closed = true;
			Win32App = null;
			Win32App = newGen.OpenApplication(_FileName);
			if (Win32App == null)
			{
				throw new ApplicationServiceException(FileName, ResourceService.GetString("Clarion.Generator.Error.AppLoadFailed"));
			}
			_Closed = false;
			OnWin32AppIsDirtyChanged(isDirtyValue: false);
		}
	}

	internal void SetIsOnApptree(bool value)
	{
		_InApptree = value;
	}

	internal void LinkToSolution(SolutionItem solutionItem)
	{
		if (_SolutionItem == null)
		{
			_SolutionItem = solutionItem;
			return;
		}
		throw new InvalidOperationException("The application is already linked into the solution.");
	}

	internal void Unload()
	{
		Unload(0L);
	}

	internal void Unload(long id)
	{
		if (!IsClosed && IsLoaded && !InEdit && IsLoaded)
		{
			_IsUnloading = true;
			StatusBarService.SetMessage($"Unloading {Name} ...");
			Win32App.Close();
			Win32App = null;
			SetLoaded(value: false);
			StatusBarService.ClearMessage();
			_IsUnloading = false;
		}
	}

	private void OnLoadedChanged()
	{
		if (this.LoadedChanged != null)
		{
			this.LoadedChanged(this, new ApplicationEventArgs(this));
		}
	}

	internal void Close(bool forceClose)
	{
		if (IsClosed || InEdit)
		{
			return;
		}
		string fileName = GetFileName();
		ResetTimeStamp();
		if (forceClose || ApplicationService.Closing || !IsOnSolution)
		{
			try
			{
				Unload(4L);
				_Closed = true;
				OnClosed();
				_SolutionItem = null;
			}
			catch (FileNotFoundException ex)
			{
				MessageBox.Show("The application file was not found" + Environment.NewLine + "when closing the file." + Environment.NewLine + "File Name:" + fileName + "Exception File Name:" + ex.FileName + Environment.NewLine, "File Error");
			}
		}
	}

	private void OnWin32AppSaved()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!IsClosed)
		{
			FileUtility.ObservedSave(new FileOperationDelegate(DummySaveClass.DummySaveFunc), FileName);
			originalTimeStamp = File.GetLastWriteTimeUtc(FileName);
		}
	}

	private void OnClosed()
	{
		if (this.Closed != null)
		{
			this.Closed(this, new ApplicationEventArgs(this));
			this.Closed = null;
			this.LoadedChanged = null;
		}
	}

	internal bool Generate()
	{
		return Generate(GenerationMode.GlobalOption, GenerationMode.GlobalOption);
	}

	internal bool Generate(GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		return Generate(null, conditionalGeneration, debugTraceGeneration);
	}

	internal bool Generate(IProject iPrj)
	{
		return Generate(iPrj, GenerationMode.GlobalOption, GenerationMode.GlobalOption);
	}

	internal bool Generate(IProject iPrj, GenerationMode conditionalGeneration, GenerationMode debugTraceGeneration)
	{
		if (IsBusy)
		{
			return false;
		}
		bool flag = false;
		try
		{
			if (!IsClosed)
			{
				DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(FileName);
				bool flag2 = CheckLazyLoad();
				try
				{
					if (!IsClosed && IsLoaded)
					{
						if (flag2)
						{
							StoreProjectSettings(iPrj);
						}
						string text = "";
						string text2 = "";
						if (iPrj != null && !string.IsNullOrEmpty(iPrj.OutputAssemblyFullPath))
						{
							text = Path.GetDirectoryName(iPrj.OutputAssemblyFullPath);
						}
						StatusBarService.SetMessage($"Generating {Name} ...");
						try
						{
							text2 = Win32App.OutputPath;
							if (text != text2)
							{
								Win32App.OutputPath = text;
							}
							flag = Win32App.Generate((int)conditionalGeneration, (int)debugTraceGeneration);
							if (text != text2)
							{
								Win32App.OutputPath = text2;
							}
						}
						finally
						{
							StatusBarService.ClearMessage();
							if (flag && IsOnSolution)
							{
								_AppPrj = null;
								_AppPrj = Win32App.ProjectFile;
							}
						}
					}
				}
				finally
				{
					if (flag2)
					{
						Save();
						Unload(5L);
					}
					if (!IsReadOnly)
					{
						File.SetLastWriteTimeUtc(FileName, lastWriteTimeUtc);
					}
				}
			}
		}
		catch (ApplicationServiceException ex)
		{
			ApplicationService.SetText(ex.ApplicationName);
			ApplicationService.SetText(GeneratorError.AppLoadFailed);
		}
		return flag;
	}

	internal void StoreProjectSettings()
	{
		IProject projectServiceProject = GetProjectServiceProject();
		if (projectServiceProject == null)
		{
			StoreProjectSettings(projectServiceProject);
		}
	}

	internal bool HasDifferentProjectSettings(IProject iPrj)
	{
		return HasDifferentProjectSettings(iPrj, storeDifferences: false);
	}

	private bool HasDifferentProjectSettings(IProject iPrj, bool storeDifferences)
	{
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		bool flag = false;
		List<Pragma> list = null;
		try
		{
			text = Win32App.Target.ToUpperInvariant();
			text2 = Win32App.TargetType.ToUpperInvariant();
			text3 = Win32App.NameSpace;
			text4 = Win32App.OutputPath;
			text5 = Win32App.LinkMode.ToUpperInvariant();
			flag = Win32App.BuildingDebug;
			list = Win32App.Pragmas;
		}
		catch
		{
		}
		string text6 = "";
		string text7 = "Dll";
		if (iPrj is IRTLTypeSupport)
		{
			text7 = ((IRTLTypeSupport)iPrj).RTLModel;
		}
		string text8 = null;
		string text9 = null;
		if (!string.IsNullOrEmpty(iPrj.OutputAssemblyFullPath))
		{
			text6 = Path.GetFileName(iPrj.OutputAssemblyFullPath);
			text8 = Path.GetExtension(iPrj.OutputAssemblyFullPath).TrimStart('.');
			text9 = Path.GetDirectoryName(iPrj.OutputAssemblyFullPath);
		}
		else
		{
			if (iPrj is MissingProject || iPrj is UnknownProject)
			{
				text8 = "Exe";
			}
			text9 = "";
		}
		if (string.IsNullOrEmpty(text8) || (!text8.Equals("Exe", StringComparison.InvariantCultureIgnoreCase) && !text8.Equals("Dll", StringComparison.InvariantCultureIgnoreCase) && !text8.Equals("Lib", StringComparison.InvariantCultureIgnoreCase)))
		{
			text8 = "Lib";
		}
		_TargetType = text8;
		bool flag2 = false;
		if (iPrj is MSBuildBasedProject)
		{
			string evaluatedProperty = ((MSBuildBasedProject)iPrj).GetEvaluatedProperty("DebugSymbols");
			flag2 = bool.TrueString.Equals(evaluatedProperty, StringComparison.InvariantCultureIgnoreCase);
		}
		string text10 = iPrj.RootNamespace;
		if (string.IsNullOrEmpty(text10))
		{
			text10 = "ClarionDefaultNamespace";
		}
		List<Pragma> list2 = ((iPrj is CommonClarionProject) ? ((CommonClarionProject)(object)iPrj).Pragmas : null);
		bool flag3 = false;
		if (!string.IsNullOrEmpty(text6))
		{
			if (text != text6.ToUpperInvariant())
			{
				flag3 = true;
			}
			else if (text2 != text8.ToUpperInvariant())
			{
				flag3 = true;
			}
			else if (text3 != text10)
			{
				flag3 = true;
			}
			else if (text4 != text9)
			{
				flag3 = true;
			}
			else if (text5 != text7.ToUpperInvariant())
			{
				flag3 = true;
			}
			else if (flag != flag2)
			{
				flag3 = true;
			}
			else if (list != null && list2 != null)
			{
				if (list.Count != list2.Count)
				{
					flag3 = true;
				}
				else if (list.Count > 0)
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (!((object)list[i]).Equals((object)list2[i]))
						{
							flag3 = true;
							break;
						}
					}
				}
			}
			else
			{
				flag3 = true;
			}
			if (flag3 && storeDifferences)
			{
				string text11 = ((object)Win32App.ProjectFile).ToString();
				Win32App.Target = text6;
				Win32App.TargetType = text8.ToUpperInvariant();
				Win32App.NameSpace = text10;
				Win32App.OutputPath = text9;
				Win32App.LinkMode = text7.ToUpperInvariant();
				Win32App.BuildingDebug = flag2;
				Win32App.Pragmas = list2;
				if (text11 != ((object)Win32App.ProjectFile).ToString())
				{
					return true;
				}
			}
		}
		return flag3;
	}

	internal void StoreProjectSettings(IProject iPrj)
	{
		if (IsBusy || iPrj == null || IsClosed)
		{
			return;
		}
		bool flag = CheckLazyLoad();
		if (!IsClosed && IsLoaded)
		{
			UpdateDictionaryFileName();
			UpdateProgramModuleFileName();
			if (HasDifferentProjectSettings(iPrj, storeDifferences: true))
			{
				Win32App.FlagProjectAsDirty();
				Save();
			}
		}
		if (flag)
		{
			Unload(6L);
		}
	}

	private void UpdateDictionaryFileName()
	{
		if (!IsClosed && Win32App != null && !IsClosed && IsLoaded)
		{
			_DictionaryFileName = Win32App.Dictionary;
		}
	}

	internal bool Import(string txaName, ImportClashMode clashMode)
	{
		if (!IsClosed && IsLoaded && !IsBusy)
		{
			return Win32App.Import(txaName, clashMode);
		}
		return false;
	}

	public bool ExportAll(string txaName)
	{
		return Export(txaName, all: true);
	}

	internal bool ExportSelected(string txaName)
	{
		return Export(txaName, all: false);
	}

	private bool Export(string txaName, bool all)
	{
		if (IsBusy)
		{
			MessageService.WriteLog($"Export to TXA Fail, Application is Busy File: {FileName} to {txaName}");
			return false;
		}
		File.Delete(txaName);
		string text = txaName + "tmp";
		string text2 = text;
		int num = 2;
		while (File.Exists(text2))
		{
			text2 = text + num++;
		}
		bool flag = false;
		bool flag2 = false;
		try
		{
			bool flag3 = CheckLazyLoad();
			if (!IsClosed && IsLoaded)
			{
				flag2 = true;
				flag = Win32App.Export(text2, all);
				if (flag)
				{
					RedirectionFile activeRedirectionFile = RedirectionFile.GetActiveRedirectionFile(true);
					text2 = activeRedirectionFile.CreateName(text2, ".");
					while (!File.Exists(text2))
					{
						System.Windows.Forms.Application.DoEvents();
						Thread.Sleep(10);
					}
					bool flag4 = true;
					while (flag4)
					{
						flag4 = false;
						try
						{
							using FileStream fileStream = File.Open(text2, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
							fileStream.Close();
						}
						catch (IOException ex)
						{
							MessageService.WriteLog((Exception)ex, "Exporting to TXA", $"Closing faile, tempText file is locked {FileName} to {txaName} from {text2}");
							flag4 = true;
							System.Windows.Forms.Application.DoEvents();
							Thread.Sleep(10);
						}
					}
				}
				if (flag3)
				{
					Unload(9L);
				}
				if (flag)
				{
					try
					{
						File.Move(text2, txaName);
					}
					catch (Exception ex2)
					{
						try
						{
							MessageService.WriteLog(ex2, "Exporting to TXA", $"Move Fail, Could not export {FileName} to {txaName} from {text2}");
							ApplicationService.SetText($"Move Fail, Could not export {FileName} to {txaName} from {text2}");
							File.Copy(text2, txaName, overwrite: true);
							File.Delete(text2);
						}
						catch (Exception ex3)
						{
							MessageService.WriteLog(ex3, "Exporting to TXA", $"Copy Fail, Could not export {FileName} to {txaName} from {text2}");
							ApplicationService.SetText($"Copy Fail, Could not export {FileName} to {txaName} from {text2}");
						}
					}
				}
			}
			else
			{
				MessageService.WriteLog($"Export to TXA Fail, Application could not open. File: {FileName} to {txaName}, IsClosed = {IsClosed}, IsLoaded:{IsLoaded}");
				ApplicationService.SetText($"Export to TXA Fail, Application could not open. File: {FileName} to {txaName}, IsClosed = {IsClosed}, IsLoaded:{IsLoaded}");
			}
		}
		finally
		{
			try
			{
				File.Delete(text2);
			}
			catch (Exception ex4)
			{
				if (flag2)
				{
					MessageService.WriteLog(ex4, "Exporting to TXA", string.Format("Delete Fail, Could not delete the temporary file {1} for {0}", FileName, text2));
				}
			}
		}
		return flag;
	}

	internal bool GenerateUtility()
	{
		if (IsBusy)
		{
			return false;
		}
		bool flag = CheckLazyLoad();
		if (!IsClosed && IsLoaded)
		{
			bool result = Win32App.GenerateUtility();
			if (flag)
			{
				Save();
				Unload(10L);
			}
			return result;
		}
		return false;
	}

	internal bool GenerateUtility(string utility, string parameters)
	{
		if (IsBusy)
		{
			return false;
		}
		bool flag = CheckLazyLoad();
		if (!IsClosed && IsLoaded)
		{
			bool result = Win32App.GenerateUtility(utility, parameters);
			if (flag)
			{
				Save();
				Unload();
			}
			return result;
		}
		return false;
	}

	internal bool Edit()
	{
		if (IsBusy)
		{
			return false;
		}
		bool flag = false;
		if (errors.Count > 0)
		{
			if (!CallingEditError)
			{
				flag = true;
			}
			ApplicationService.AllowClearErrors(value: false);
		}
		CheckLazyLoad();
		if (!IsClosed && IsLoaded)
		{
			StoreProjectSettings();
			originalTimeStamp = File.GetLastWriteTimeUtc(FileName);
			bool flag2 = Win32App.Edit();
			if (flag2)
			{
				Win32App.EditorClosed += OnEditorClosed;
			}
			if (flag)
			{
				ApplicationService.AllowClearErrors(value: true);
			}
			return flag2;
		}
		if (flag)
		{
			ApplicationService.AllowClearErrors(value: true);
		}
		return false;
	}

	internal void CloseEditSession()
	{
		if (!IsClosed && InEdit)
		{
			UpdateDictionaryFileName();
			UpdateProgramModuleFileName();
			Win32App.WindowClosed();
		}
	}

	private void ResetTimeStamp()
	{
		if (!string.IsNullOrEmpty(GetFileName()) && File.Exists(GetFileName()) && !File.GetLastWriteTimeUtc(GetFileName()).Equals(originalTimeStamp) && originalTimeStamp != DateTime.MinValue && !string.IsNullOrEmpty(GetFileName()))
		{
			File.SetLastWriteTimeUtc(GetFileName(), originalTimeStamp);
		}
	}

	private void OnEditorClosed()
	{
		_InApptree = false;
		Win32App.EditorClosed -= OnEditorClosed;
		ApplicationService.AllowClearErrors(value: true);
		if (!IsClosed && IsLoaded)
		{
			if (InEdit)
			{
				UpdateDictionaryFileName();
				UpdateProgramModuleFileName();
			}
			Win32App.EditClosed();
			if (IsOnSolution)
			{
				if (!appprop.Get<bool>("cacheAppAfterEdit", false))
				{
					Unload(1L);
				}
			}
			else
			{
				Close(forceClose: true);
			}
		}
		ResetTimeStamp();
	}

	internal bool Save()
	{
		if (IsBusy)
		{
			return false;
		}
		if (!IsClosed && IsLoaded)
		{
			StatusBarService.SetMessage($"Saving {Name}");
			bool result = Win32App.Save();
			OnWin32AppIsDirtyChanged(isDirtyValue: false);
			StatusBarService.ClearMessage();
			return result;
		}
		return false;
	}

	internal bool SaveAs(string destFileName)
	{
		if (IsBusy)
		{
			return false;
		}
		if (!IsClosed && IsLoaded)
		{
			StatusBarService.SetMessage($"Saving {Name} as {destFileName}");
			string fileName = FileName;
			bool flag = Win32App.SaveAs(destFileName);
			if (flag)
			{
				_FileName = destFileName;
				if (IsOnSolution)
				{
					_SolutionItem = null;
					_AppPrj = null;
				}
				OnSaveAs(fileName);
			}
			StatusBarService.ClearMessage();
			return flag;
		}
		return false;
	}

	private void OnSaveAs(string OldName)
	{
		if (this.SavedAs != null)
		{
			this.SavedAs(this, new ApplicationRenamedEventArgs(this, OldName));
		}
	}

	public void CheckForIsDirtyChanged()
	{
		if (!IsClosed && IsLoaded && _IsDirty != Win32App.IsDirty)
		{
			OnWin32AppIsDirtyChanged(Win32App.IsDirty);
		}
	}

	private void OnWin32AppIsBusyChanged(bool isBusyValue)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<bool>((Action<bool>)OnWin32AppIsBusyChanged, isBusyValue);
			return;
		}
		_IsBusy = isBusyValue;
		if (this.IsBusyChanged != null)
		{
			this.IsBusyChanged(isBusyValue);
		}
	}

	private void OnWin32AppIsDirtyChanged(bool isDirtyValue)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			WorkbenchSingleton.SafeThreadCall<bool>((Action<bool>)OnWin32AppIsDirtyChanged, isDirtyValue);
			return;
		}
		_IsDirty = isDirtyValue;
		if (this.IsDirtyChanged != null)
		{
			this.IsDirtyChanged(isDirtyValue);
		}
	}

	private void SolutionClosed(object sender, EventArgs e)
	{
		ProjectService.ProjectItemRemoved -= OnProjectService_ProjectItemRemoved;
		ProjectService.SolutionClosed -= SolutionClosed;
	}

	private void OnProjectService_SolutionFolderRemoved(object sender, SolutionFolderEventArgs e)
	{
		if (_iPrj != null && e.SolutionFolder.IdGuid == ((ISolutionFolder)_iPrj).IdGuid)
		{
			_iPrj = null;
		}
	}

	private void OnProjectService_ProjectItemRemoved(object sender, ProjectItemEventArgs e)
	{
		if (!(((ProjectEventArgs)e).Project.FileName == ProjectFileName) || e.ProjectItem == null)
		{
			return;
		}
		bool flag = CheckLazyLoad();
		Win32App.RemoveFile(e.ProjectItem.Include);
		if (flag && !waitingForMergingToEnd)
		{
			if (ProjectsMerger.Merging)
			{
				waitingForMergingToEnd = true;
				ProjectsMerger.MergingEnded += OnProjectsMerger_MergingEnded;
			}
			else
			{
				OnProjectsMerger_MergingEnded(null, null);
			}
		}
	}

	private void OnProjectsMerger_MergingEnded(object sender, ProjectsMerger.ProjectsMergerEventArgs e)
	{
		Save();
		Unload(2L);
		ProjectsMerger.MergingEnded -= OnProjectsMerger_MergingEnded;
		waitingForMergingToEnd = false;
	}

	internal bool Rename(string destFileName)
	{
		if (IsBusy)
		{
			return false;
		}
		if (!IsClosed && IsLoaded)
		{
			string fileName = FileName;
			if (Win32App.SaveAs(destFileName))
			{
				_FileName = destFileName;
				File.Delete(fileName);
				return true;
			}
		}
		return false;
	}

	internal string NameToProjectName()
	{
		if (!IsClosed)
		{
			return ApplicationService.ProjectFileName(FileName, LanguageBinding);
		}
		return string.Empty;
	}

	private void UpdateProgramModuleFileName()
	{
		if (!IsClosed && Win32App != null && !IsClosed && IsLoaded)
		{
			Module programModule = ProgramModule;
			if (programModule != null)
			{
				_ProgramModuleFileName = programModule.Name;
			}
		}
	}

	internal void ModulesSelectAll(bool select)
	{
		if (!IsClosed && IsLoaded)
		{
			Win32App.ModulesSelectAll(select);
		}
	}

	public void ClearErrorList()
	{
		if (Win32App != null)
		{
			Win32App.ClearErrorList();
		}
		errors.Clear();
	}

	public void AddError(BuildError error)
	{
		errors.Add(error);
		if (Win32App != null)
		{
			Win32App.AddError(error);
		}
	}

	private void PassErrorsToWin32App()
	{
		if (Win32App == null)
		{
			return;
		}
		Win32App.ClearErrorList();
		foreach (BuildError error in errors)
		{
			Win32App.AddError(error);
		}
	}

	public bool EditError(BuildError error)
	{
		if (w32appprop.Get<string>("TranslateErrors", "") != "off")
		{
			ApplicationService.AllowClearErrors(value: false);
			CheckLazyLoad();
			if (!IsClosed && IsLoaded)
			{
				CallingEditError = true;
				bool result = Win32App.EditError(error);
				ApplicationService.AllowClearErrors(value: true);
				return result;
			}
			ApplicationService.AllowClearErrors(value: true);
			return false;
		}
		if (FileService.OpenFile(error.FileName) != null)
		{
			FileService.JumpToFilePosition(error.FileName, error.Line - 1, error.Column - 1);
		}
		ApplicationService.AllowClearErrors(value: true);
		return true;
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			ClearErrorList();
			if (Win32App != null)
			{
				Win32App.Dispose();
			}
			if (_AppPrj != null)
			{
				_AppPrj.Dispose();
			}
			if (_OldAppPrj != null)
			{
				_OldAppPrj.Dispose();
			}
			Win32App = null;
			_AppPrj = null;
			_OldAppPrj = null;
			_LanguageBinding = null;
			_SolutionItem = null;
			errors = null;
			this.Closed = null;
			this.IsDirtyChanged = null;
			this.LanguageChanged = null;
			this.LoadedChanged = null;
			this.SavedAs = null;
		}
	}

	internal void ResumeGenerate()
	{
		if (_canGenerate > 0)
		{
			_canGenerate--;
			return;
		}
		throw new Exception("ResumeGenerate called with out calling SuspendGenerate.");
	}

	internal void SuspendGenerate()
	{
		if (_canGenerate > -1)
		{
			_canGenerate++;
		}
	}
}
