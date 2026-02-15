using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.Refactoring;

public class Reference
{
	private string fileName;

	private int offset;

	private int length;

	private string expression;

	private ResolveResult resolveResult;

	public string FileName => fileName;

	public int Offset => offset;

	public int Length => length;

	public string Expression => expression;

	public ResolveResult ResolveResult => resolveResult;

	public Reference(string fileName, int offset, int length, string expression, ResolveResult resolveResult)
	{
		this.fileName = fileName;
		this.offset = offset;
		this.length = length;
		this.expression = expression;
		this.resolveResult = resolveResult;
	}
}
