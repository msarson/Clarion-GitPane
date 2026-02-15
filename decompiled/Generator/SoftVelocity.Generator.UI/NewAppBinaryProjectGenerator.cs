using System;
using System.IO;
using Clarion.GEN;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Internal.Templates;

namespace SoftVelocity.Generator.UI;

internal class NewAppBinaryProjectGenerator : IBinaryProjectGenerator
{
	public bool ProjectCreated => false;

	public string ProjectCreatedName
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool GenerateFiles(ProjectTemplate projectTemplate, ProjectCreateInformation projectCreateInformation)
	{
		string fileName = Path.Combine(projectCreateInformation.ProjectBasePath, projectCreateInformation.ProjectName + ".app");
		Win32App win32App = ApplicationService.NewApp(fileName, projectTemplate.LanguageName);
		if (win32App != null)
		{
			ApplicationService.PushApplication(win32App.FileName);
			projectCreateInformation.ProjectName = Path.GetFileNameWithoutExtension(win32App.FileName);
			return true;
		}
		return false;
	}
}
