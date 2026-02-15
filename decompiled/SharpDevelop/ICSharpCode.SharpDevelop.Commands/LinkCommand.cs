using System;
using System.Diagnostics;
using System.IO;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class LinkCommand : AbstractMenuCommand
{
	private string site;

	public LinkCommand(string site)
	{
		this.site = site;
	}

	public override void Run()
	{
		if (site.StartsWith("home://"))
		{
			string text = Path.Combine(FileUtility.ApplicationRootPath, site.Substring(7).Replace('/', Path.DirectorySeparatorChar));
			try
			{
				Process.Start(text);
				return;
			}
			catch (Exception)
			{
				MessageService.ShowError("Can't execute/view " + text + "\n Please check that the file exists and that you can open this file.");
				return;
			}
		}
		FileService.OpenFile(site);
	}
}
