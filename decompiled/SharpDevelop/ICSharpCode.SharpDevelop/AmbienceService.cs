using System;
using System.CodeDom.Compiler;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop;

public static class AmbienceService
{
	private const string ambienceProperty = "SharpDevelop.UI.CurrentAmbience";

	private const string codeGenerationProperty = "SharpDevelop.UI.CodeGenerationOptions";

	private const string textEditorProperty = "TextEditorSettings";

	private static AmbienceReflectionDecorator defaultAmbience;

	private static string tempAmbianceLanguageName;

	public static Properties CodeGenerationProperties => PropertyService.Get("SharpDevelop.UI.CodeGenerationOptions", new Properties());

	public static bool GenerateDocumentComments => CodeGenerationProperties.Get("GenerateDocumentComments", defaultValue: true);

	public static bool GenerateAdditionalComments => CodeGenerationProperties.Get("GenerateAdditionalComments", defaultValue: true);

	public static bool UseFullyQualifiedNames => CodeGenerationProperties.Get("UseFullyQualifiedNames", defaultValue: true);

	public static bool UseProjectAmbienceIfPossible
	{
		get
		{
			return PropertyService.Get("SharpDevelop.UI.UseProjectAmbience", defaultValue: true);
		}
		set
		{
			PropertyService.Set("SharpDevelop.UI.UseProjectAmbience", value);
		}
	}

	public static AmbienceReflectionDecorator CurrentAmbience
	{
		get
		{
			if (tempAmbianceLanguageName != null)
			{
				string language = tempAmbianceLanguageName;
				tempAmbianceLanguageName = null;
				IAmbience ambienceForLanguage = GetAmbienceForLanguage(language);
				return new AmbienceReflectionDecorator(ambienceForLanguage);
			}
			if (UseProjectAmbienceIfPossible)
			{
				IProject currentProject = ProjectService.CurrentProject;
				if (currentProject != null)
				{
					IAmbience ambience = currentProject.Ambience;
					if (ambience != null)
					{
						return new AmbienceReflectionDecorator(ambience);
					}
				}
			}
			if (defaultAmbience == null)
			{
				string defaultAmbienceName = DefaultAmbienceName;
				IAmbience ambienceForLanguage2 = GetAmbienceForLanguage(defaultAmbienceName);
				defaultAmbience = new AmbienceReflectionDecorator(ambienceForLanguage2);
			}
			return defaultAmbience;
		}
	}

	public static string DefaultAmbienceName
	{
		get
		{
			try
			{
				return PropertyService.Get("SharpDevelop.UI.CurrentAmbience", "Clarion");
			}
			catch (Exception)
			{
				return PropertyService.Get("SharpDevelop.UI.CurrentAmbience", "Clarion.Net");
			}
		}
		set
		{
			PropertyService.Set("SharpDevelop.UI.CurrentAmbience", value);
		}
	}

	public static event EventHandler AmbienceChanged;

	static AmbienceService()
	{
		tempAmbianceLanguageName = null;
		PropertyService.PropertyChanged += PropertyChanged;
		ApplyCodeGenerationPropertiesToNRefactory();
	}

	private static void ApplyCodeGenerationPropertiesToNRefactory()
	{
		ICSharpCode.SharpDevelop.Dom.Refactoring.CodeGeneratorOptions options = LanguageProperties.CSharp.CodeGenerator.Options;
		ICSharpCode.SharpDevelop.Dom.Refactoring.CodeGeneratorOptions options2 = LanguageProperties.VBNet.CodeGenerator.Options;
		System.CodeDom.Compiler.CodeGeneratorOptions createCodeGeneratorOptions = new CodeDOMGeneratorUtility().CreateCodeGeneratorOptions;
		options.EmptyLinesBetweenMembers = createCodeGeneratorOptions.BlankLinesBetweenMembers;
		options2.EmptyLinesBetweenMembers = createCodeGeneratorOptions.BlankLinesBetweenMembers;
		options2.BracesOnSameLine = (options.BracesOnSameLine = CodeGenerationProperties.Get("StartBlockOnSameLine", defaultValue: true));
		options.IndentString = createCodeGeneratorOptions.IndentString;
		options2.IndentString = createCodeGeneratorOptions.IndentString;
	}

	public static void UseAmbianceOnce(string ambianceLanguageName)
	{
		tempAmbianceLanguageName = ambianceLanguageName;
	}

	private static IAmbience GetAmbienceForLanguage(string language)
	{
		IAmbience ambience = null;
		try
		{
			ambience = (IAmbience)AddInTree.BuildItem("/SharpDevelop/Workbench/Ambiences/" + language, null);
		}
		catch (TreePathNotFoundException)
		{
		}
		if (ambience == null)
		{
			try
			{
				ambience = (IAmbience)AddInTree.BuildItem("/SharpDevelop/Workbench/Ambiences/Clarion", null);
			}
			catch
			{
				ambience = (IAmbience)AddInTree.BuildItem("/SharpDevelop/Workbench/Ambiences/Clarion.Net", null);
			}
			if (ambience == null)
			{
				MessageService.ShowError("${res:ICSharpCode.SharpDevelop.Services.AmbienceService.AmbienceNotFoundError}");
			}
		}
		return ambience;
	}

	private static void PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.Key == "SharpDevelop.UI.CurrentAmbience")
		{
			defaultAmbience = null;
			OnAmbienceChanged(EventArgs.Empty);
		}
		if (e.Key == "SharpDevelop.UI.CodeGenerationOptions" || e.Key == "TextEditorSettings")
		{
			ApplyCodeGenerationPropertiesToNRefactory();
		}
	}

	private static void OnAmbienceChanged(EventArgs e)
	{
		if (AmbienceService.AmbienceChanged != null)
		{
			AmbienceService.AmbienceChanged(null, e);
		}
	}
}
