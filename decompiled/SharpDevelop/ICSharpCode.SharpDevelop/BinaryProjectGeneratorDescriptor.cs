using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class BinaryProjectGeneratorDescriptor
{
	private AddIn addin;

	private string generatorPath;

	private IBinaryProjectGenerator _Generator;

	public IBinaryProjectGenerator Generator
	{
		get
		{
			if (generatorPath != null)
			{
				if (_Generator == null)
				{
					_Generator = (IBinaryProjectGenerator)addin.CreateObject(generatorPath);
				}
				generatorPath = null;
				addin = null;
			}
			return _Generator;
		}
	}

	public BinaryProjectGeneratorDescriptor(AddIn addin, string generatorPath)
	{
		this.addin = addin;
		this.generatorPath = generatorPath;
	}
}
