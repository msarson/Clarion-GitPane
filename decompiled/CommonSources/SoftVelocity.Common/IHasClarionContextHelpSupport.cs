namespace SoftVelocity.Common;

public interface IHasClarionContextHelpSupport
{
	string HelpText { get; }

	bool HelpTextIsKeyword { get; }
}
