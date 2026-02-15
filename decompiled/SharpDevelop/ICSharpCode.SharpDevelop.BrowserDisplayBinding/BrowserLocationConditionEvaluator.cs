using System;
using System.Text.RegularExpressions;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.BrowserDisplayBinding;

public class BrowserLocationConditionEvaluator : IConditionEvaluator
{
	public bool IsValid(object caller, Condition condition)
	{
		HtmlViewPane htmlViewPane = (HtmlViewPane)caller;
		string input = htmlViewPane.Url.ToString();
		string pattern = condition.Properties["urlRegex"];
		string text = condition.Properties["options"];
		if (text != null && text.Length > 0)
		{
			return Regex.IsMatch(input, pattern, (RegexOptions)Enum.Parse(typeof(RegexOptions), text, ignoreCase: true));
		}
		return Regex.IsMatch(input, pattern);
	}
}
