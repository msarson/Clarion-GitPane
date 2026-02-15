using System.IO;
using ICSharpCode.SharpDevelop.Project;

namespace SoftVelocity.Generator;

public sealed class ApplicationLoader : IProjectLoader
{
	public void Load(string fileName)
	{
		if (ApplicationService.IsValidFileName(fileName) && File.Exists(fileName))
		{
			try
			{
				ApplicationService.LoadApplication(fileName);
			}
			catch (ApplicationServiceException)
			{
			}
		}
	}
}
