using System.Diagnostics;
using System.IO;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project.Commands;

public class GenerateProjectDocumentation : AbstractMenuCommand
{
	public override void Run()
	{
		if (!(ProjectService.CurrentProject is CompilableProject { OutputAssemblyFullPath: var outputAssemblyFullPath } compilableProject))
		{
			return;
		}
		if (!File.Exists(outputAssemblyFullPath))
		{
			MessageService.ShowMessage("${res:ProjectComponent.ContextMenu.GenerateDocumentation.ProjectNeedsToBeCompiled}");
			return;
		}
		string documentationFileFullPath = compilableProject.DocumentationFileFullPath;
		if (documentationFileFullPath == null)
		{
			MessageService.ShowMessage("${res:ProjectComponent.ContextMenu.GenerateDocumentation.NeedToEditBuildOptions}");
			return;
		}
		if (!File.Exists(documentationFileFullPath))
		{
			MessageService.ShowMessage("${res:ProjectComponent.ContextMenu.GenerateDocumentation.ProjectNeedsToBeCompiled2}");
			return;
		}
		string text = Path.ChangeExtension(outputAssemblyFullPath, ".ndoc");
		if (!File.Exists(text))
		{
			using StreamWriter streamWriter = File.CreateText(text);
			streamWriter.WriteLine("<project>");
			streamWriter.WriteLine("    <assemblies>");
			streamWriter.WriteLine("        <assembly location=\"" + outputAssemblyFullPath + "\" documentation=\"" + documentationFileFullPath + "\" />");
			streamWriter.WriteLine("    </assemblies>");
			streamWriter.WriteLine("</project>");
		}
		string text2 = Path.Combine(FileUtility.ApplicationRootPath, "bin/Tools/NDoc");
		ProcessStartInfo processStartInfo = new ProcessStartInfo(Path.Combine(text2, "NDocGui.exe"), '"' + text + '"');
		processStartInfo.WorkingDirectory = text2;
		processStartInfo.UseShellExecute = false;
		Process.Start(processStartInfo);
	}
}
