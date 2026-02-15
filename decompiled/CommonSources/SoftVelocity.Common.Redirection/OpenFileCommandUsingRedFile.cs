using ICSharpCode.Core;

namespace SoftVelocity.Common.Redirection;

public class OpenFileCommandUsingRedFile : IConditionEvaluator
{
	private static bool _useRed = true;

	public static bool Value
	{
		get
		{
			return PropertyService.Get<bool>("UseRedirectionFileOpen", _useRed, "FileDialog", new string[0]);
		}
		set
		{
			_useRed = value;
			PropertyService.Get<bool>("UseRedirectionFileOpen", _useRed, "FileDialog", new string[0]);
		}
	}

	public bool IsValid(object caller, Condition condition)
	{
		_useRed = PropertyService.Get<bool>("UseRedirectionFileOpen", _useRed, "FileDialog", new string[0]);
		return _useRed;
	}
}
