using ICSharpCode.SharpDevelop.Dom;

namespace SoftVelocity.Common.CodeCompletion;

public class UnknownIdentifierResolveResult : ResolveResult
{
	private string identifier;

	private int typeParametersCount;

	public string Identifier => identifier;

	public int TypeParametersCount => typeParametersCount;

	public UnknownIdentifierResolveResult(IClass callingClass, IMember callingMember, string identifier, int typeParametersCount)
		: base(callingClass, callingMember, (IReturnType)null)
	{
		this.identifier = identifier;
		this.typeParametersCount = typeParametersCount;
	}
}
