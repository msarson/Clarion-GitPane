using System.IO;
using System.Reflection;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Codons;

public class AddInTreeSyntaxMode : SyntaxMode
{
	private Runtime[] runtimes;

	public AddInTreeSyntaxMode(Runtime[] runtimes, string fileName, string name, string[] extensions)
		: base(fileName, name, extensions)
	{
		this.runtimes = runtimes;
	}

	public XmlTextReader CreateTextReader()
	{
		Runtime[] array = runtimes;
		foreach (Runtime runtime in array)
		{
			Assembly loadedAssembly = runtime.LoadedAssembly;
			if (loadedAssembly != null)
			{
				Stream manifestResourceStream = loadedAssembly.GetManifestResourceStream(base.FileName);
				if (manifestResourceStream != null)
				{
					return new XmlTextReader(manifestResourceStream);
				}
			}
		}
		return null;
	}
}
