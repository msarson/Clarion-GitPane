using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public static class CodeCompletionOptions
{
	private static Properties properties = PropertyService.Get("CodeCompletionOptions", new Properties());

	public static Properties Properties => properties;

	public static bool EnableCodeCompletion
	{
		get
		{
			return properties.Get("EnableCC", defaultValue: true);
		}
		set
		{
			properties.Set("EnableCC", value);
		}
	}

	public static bool DataUsageCacheEnabled
	{
		get
		{
			return properties.Get("DataUsageCacheEnabled", defaultValue: true);
		}
		set
		{
			properties.Set("DataUsageCacheEnabled", value);
		}
	}

	public static int DataUsageCacheItemCount
	{
		get
		{
			return properties.Get("DataUsageCacheItemCount", 500);
		}
		set
		{
			properties.Set("DataUsageCacheItemCount", value);
		}
	}

	public static bool TooltipsEnabled
	{
		get
		{
			return properties.Get("TooltipsEnabled", defaultValue: true);
		}
		set
		{
			properties.Set("TooltipsEnabled", value);
		}
	}

	public static bool TooltipsOnlyWhenDebugging
	{
		get
		{
			return properties.Get("TooltipsOnlyWhenDebugging", defaultValue: false);
		}
		set
		{
			properties.Set("TooltipsOnlyWhenDebugging", value);
		}
	}

	public static bool KeywordCompletionEnabled
	{
		get
		{
			return properties.Get("KeywordCompletionEnabled", defaultValue: true);
		}
		set
		{
			properties.Set("KeywordCompletionEnabled", value);
		}
	}

	public static bool CompleteWhenTyping
	{
		get
		{
			return properties.Get("CompleteWhenTyping", defaultValue: true);
		}
		set
		{
			properties.Set("CompleteWhenTyping", value);
		}
	}

	public static bool ShrinkListWhenTyping
	{
		get
		{
			return properties.Get("ShrinkListWhenTyping", defaultValue: true);
		}
		set
		{
			properties.Set("ShrinkListWhenTyping", value);
		}
	}

	public static bool NewLineOnEnterAfterFullWord
	{
		get
		{
			return properties.Get("NewLineOnEnterAfterFullWord", defaultValue: true);
		}
		set
		{
			properties.Set("NewLineOnEnterAfterFullWord", value);
		}
	}

	public static bool CompleteOnInsertionKey
	{
		get
		{
			return properties.Get("CompleteOnInsertionKey", defaultValue: false);
		}
		set
		{
			properties.Set("CompleteOnInsertionKey", value);
		}
	}

	public static bool InsightEnabled
	{
		get
		{
			return properties.Get("InsightEnabled", defaultValue: true);
		}
		set
		{
			properties.Set("InsightEnabled", value);
		}
	}

	public static bool InsightRefreshOnComma
	{
		get
		{
			return properties.Get("InsightRefreshOnComma", defaultValue: true);
		}
		set
		{
			properties.Set("InsightRefreshOnComma", value);
		}
	}
}
