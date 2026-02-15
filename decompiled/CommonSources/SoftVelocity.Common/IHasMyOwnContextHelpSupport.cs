namespace SoftVelocity.Common;

public interface IHasMyOwnContextHelpSupport : IHasClarionContextHelpSupport
{
	string FullHelpFileName { get; }
}
