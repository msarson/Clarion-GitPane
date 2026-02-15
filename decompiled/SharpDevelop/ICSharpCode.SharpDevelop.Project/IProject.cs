using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public interface IProject : ISolutionFolder, IDisposable, IMementoCapable, ICanBeDirty
{
	ReadOnlyCollection<ProjectItem> Items { get; }

	ICollection<ItemType> AvailableFileItemTypes { get; }

	List<ProjectSection> ProjectSections { get; }

	LanguageProperties LanguageProperties { get; }

	string ProjectType { get; }

	IAmbience Ambience { get; }

	string FileName { get; set; }

	string Directory { get; }

	string TitleName { get; }

	string VersionName { get; }

	string AssemblyName { get; set; }

	string RootNamespace { get; set; }

	string OutputAssemblyFullPath { get; }

	string Language { get; }

	string AppDesignerFolder { get; }

	string ActiveConfiguration { get; set; }

	string ActivePlatform { get; set; }

	ICollection<string> ConfigurationNames { get; }

	ICollection<string> PlatformNames { get; }

	bool IsStartable { get; }

	IEnumerable<ProjectItem> GetItemsOfType(ItemType type);

	ItemType GetDefaultItemType(string fileName);

	void Save();

	void AddToolsVersionAttribute();

	bool IsFileInProject(string fileName);

	FileProjectItem FindFile(string fileName);

	void Start(bool withDebugging);

	ParseProjectContent CreateProjectContent();

	void StartBuild(BuildOptions options);

	ProjectItem CreateProjectItem(BuildItem item);
}
