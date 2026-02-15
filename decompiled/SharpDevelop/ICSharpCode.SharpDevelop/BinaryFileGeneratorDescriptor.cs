using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class BinaryFileGeneratorDescriptor
{
	private AddIn addin;

	private string generatorPath;

	private IBinaryFileGenerator _Generator;

	public IBinaryFileGenerator Generator
	{
		get
		{
			if (generatorPath != null)
			{
				if (_Generator == null)
				{
					_Generator = (IBinaryFileGenerator)addin.CreateObject(generatorPath);
				}
				generatorPath = null;
				addin = null;
			}
			return _Generator;
		}
	}

	public BinaryFileGeneratorDescriptor(AddIn addin, string generatorPath)
	{
		this.addin = addin;
		this.generatorPath = generatorPath;
	}
}
