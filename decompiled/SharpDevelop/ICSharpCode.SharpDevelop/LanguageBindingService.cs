using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop;

public static class LanguageBindingService
{
	private static IList<LanguageBindingDescriptor> bindings;

	static LanguageBindingService()
	{
		bindings = AddInTree.BuildItems<LanguageBindingDescriptor>("/SharpDevelop/Workbench/LanguageBindings", null, throwOnNotFound: false);
	}

	public static void SetBindings(IList<LanguageBindingDescriptor> bindings)
	{
		LanguageBindingService.bindings = bindings;
	}

	public static string GetProjectFileExtension(string languageName)
	{
		return GetCodonPerLanguageName(languageName)?.ProjectFileExtension;
	}

	public static ILanguageBinding GetBindingPerLanguageName(string languagename)
	{
		return GetCodonPerLanguageName(languagename)?.Binding;
	}

	public static ILanguageBinding GetBindingCodePerFileName(string filename)
	{
		return GetCodonPerCodeFileName(filename)?.Binding;
	}

	public static ILanguageBinding GetBindingPerProjectFile(string filename)
	{
		return GetCodonPerProjectFile(filename)?.Binding;
	}

	public static LanguageBindingDescriptor GetCodonPerLanguageName(string languagename)
	{
		foreach (LanguageBindingDescriptor binding in bindings)
		{
			if (binding.Language == languagename)
			{
				return binding;
			}
		}
		return null;
	}

	public static LanguageBindingDescriptor GetCodonPerCodeFileName(string filename)
	{
		string value = Path.GetExtension(filename).ToLowerInvariant();
		foreach (LanguageBindingDescriptor binding in bindings)
		{
			if (Array.IndexOf(binding.CodeFileExtensions, value) >= 0)
			{
				return binding;
			}
		}
		return null;
	}

	public static LanguageBindingDescriptor GetCodonPerProjectFile(string fileName)
	{
		string text = Path.GetExtension(fileName).ToUpperInvariant();
		foreach (LanguageBindingDescriptor binding in bindings)
		{
			if (binding.ProjectFileExtension.ToUpperInvariant() == text)
			{
				return binding;
			}
		}
		return null;
	}

	public static IProject LoadProject(IMSBuildEngineProvider provider, string location, string title)
	{
		return LoadProject(provider, location, title, "{" + Guid.Empty.ToString() + "}");
	}

	public static IProject LoadProject(IMSBuildEngineProvider provider, string location, string title, string projectTypeGuid)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (location == null)
		{
			throw new ArgumentNullException("location");
		}
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		if (projectTypeGuid == null)
		{
			throw new ArgumentNullException("projectTypeGuid");
		}
		IProject project;
		if (!File.Exists(location))
		{
			project = new MissingProject(location, title);
			project.TypeGuid = projectTypeGuid;
		}
		else
		{
			ILanguageBinding bindingPerProjectFile = GetBindingPerProjectFile(location);
			if (bindingPerProjectFile != null)
			{
				try
				{
					location = Path.GetFullPath(location);
				}
				catch (Exception)
				{
				}
				try
				{
					project = bindingPerProjectFile.LoadProject(provider, location, title);
				}
				catch (XmlException ex2)
				{
					project = new UnknownProject(location, title, ex2.Message, displayWarningToUser: true);
					project.TypeGuid = projectTypeGuid;
				}
				catch (InvalidProjectFileException ex3)
				{
					project = new UnknownProject(location, title, ex3.Message, displayWarningToUser: true);
					project.TypeGuid = projectTypeGuid;
				}
				catch (UnauthorizedAccessException ex4)
				{
					project = new UnknownProject(location, title, ex4.Message, displayWarningToUser: true);
					project.TypeGuid = projectTypeGuid;
				}
			}
			else
			{
				project = new UnknownProject(location, title);
				project.TypeGuid = projectTypeGuid;
			}
		}
		return project;
	}
}
