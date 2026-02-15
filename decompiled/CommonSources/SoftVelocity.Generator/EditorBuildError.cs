using System.CodeDom.Compiler;

namespace SoftVelocity.Generator;

public class EditorBuildError : CompilerError
{
	private int errorInfoIndex;

	public int ErrorInfoIndex
	{
		get
		{
			return errorInfoIndex;
		}
		set
		{
			errorInfoIndex = value;
		}
	}

	public EditorBuildError(string fileName, int line, int column, string errorNumber, string errorText, int errorInfoIndex)
		: base(fileName, line, column, errorNumber, errorText)
	{
		this.errorInfoIndex = errorInfoIndex;
	}
}
