using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.Core;

namespace SearchAndReplace;

public static class SearchOptions
{
	private const string searchPropertyKey = "SearchAndReplaceProperties";

	private static Properties properties;

	private static string findPattern;

	private static string replacePattern;

	private static bool _ShowResults;

	private static AbstractSearchAndReplaceBinding directoryBinding;

	private static AbstractSearchAndReplaceBinding currentDocumentDirectoryBinding;

	private static AbstractSearchAndReplaceBinding currentSelectionBinding;

	private static AbstractSearchAndReplaceBinding currentDocumentBinding;

	private static List<AbstractSearchAndReplaceBinding> bindings;

	private static AbstractSearchAndReplaceBinding binding;

	private static bool storedOptions;

	private static string _OldLookIn;

	private static string _OldLookInFiletypes;

	private static string _OldReplacePattern;

	private static string _OldFindPattern;

	private static bool _OldMatchCase;

	private static bool _OldMatchWholeWord;

	private static bool _OldIncludeSubdirectories;

	private static bool _OldMultiLineMatch;

	private static SearchStrategyType _OldSearchStrategyType;

	private static AbstractSearchAndReplaceBinding _OldSearchAndReplaceBinding;

	public static Properties Properties => properties;

	public static bool ShowResults
	{
		get
		{
			return _ShowResults;
		}
		set
		{
			_ShowResults = value;
		}
	}

	public static string FindPattern
	{
		get
		{
			return findPattern;
		}
		set
		{
			if (value != FindPattern)
			{
				findPattern = value;
				string[] findPatterns = FindPatterns;
				int num = findPatterns.Length;
				int maxNumberOfFindPatterns = MaxNumberOfFindPatterns;
				string[] array = ((num < maxNumberOfFindPatterns) ? new string[num + 1] : new string[maxNumberOfFindPatterns]);
				Array.Copy(findPatterns, 0, array, 1, array.Length - 1);
				array[0] = value;
				FindPatterns = array;
			}
		}
	}

	public static string CurrentFindPattern
	{
		get
		{
			return findPattern;
		}
		set
		{
			findPattern = value;
		}
	}

	public static string[] FindPatterns
	{
		get
		{
			if (!properties.Contains("FindPatterns"))
			{
				return new string[0];
			}
			return properties.Get("FindPatterns", "").Split('ÿ');
		}
		set
		{
			properties.Set("FindPatterns", string.Join("ÿ", value));
		}
	}

	public static int MaxNumberOfFindPatterns
	{
		get
		{
			if (!properties.Contains("MaxNumberOfFindPatterns"))
			{
				return 1000;
			}
			return properties.Get("MaxNumberOfFindPatterns", 1000);
		}
		set
		{
			if (value > 1000)
			{
				value = 1000;
			}
			if (value < 2)
			{
				value = 100;
			}
			properties.Set("MaxNumberOfFindPatterns", value);
		}
	}

	public static string ReplacePattern
	{
		get
		{
			if (!properties.Contains("ReplacePatterns"))
			{
				return "";
			}
			return replacePattern;
		}
		set
		{
			if (value != ReplacePattern)
			{
				string[] replacePatterns = ReplacePatterns;
				string[] array = new string[replacePatterns.Length + 1];
				replacePatterns.CopyTo(array, 1);
				array[0] = value;
				ReplacePatterns = array;
				replacePattern = value;
			}
		}
	}

	public static string[] ReplacePatterns
	{
		get
		{
			if (!properties.Contains("ReplacePatterns"))
			{
				return new string[0];
			}
			return properties.Get("ReplacePatterns", "").Split('ÿ');
		}
		set
		{
			properties.Set("ReplacePatterns", string.Join("ÿ", value));
		}
	}

	public static bool MatchCase
	{
		get
		{
			return properties.Get("MatchCase", defaultValue: false);
		}
		set
		{
			properties.Set("MatchCase", value);
		}
	}

	public static bool IncludeSubdirectories
	{
		get
		{
			return properties.Get("IncludeSubdirectories", defaultValue: false);
		}
		set
		{
			properties.Set("IncludeSubdirectories", value);
		}
	}

	public static bool IncludeReadOnlyBlocks
	{
		get
		{
			return properties.Get("IncludeReadOnlyBlocks", defaultValue: true);
		}
		set
		{
			properties.Set("IncludeReadOnlyBlocks", value);
		}
	}

	public static bool MatchWholeWord
	{
		get
		{
			return properties.Get("MatchWholeWord", defaultValue: false);
		}
		set
		{
			properties.Set("MatchWholeWord", value);
		}
	}

	public static bool MultiLineMatch
	{
		get
		{
			return properties.Get("MultiLineMatch", defaultValue: true);
		}
		set
		{
			properties.Set("MultiLineMatch", value);
		}
	}

	public static string LookIn
	{
		get
		{
			return properties.Get("LookIn", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Clarion Projects"));
		}
		set
		{
			properties.Set("LookIn", value);
		}
	}

	public static string LookInFiletypes
	{
		get
		{
			return properties.Get("LookInFiletypes", "*.*");
		}
		set
		{
			properties.Set("LookInFiletypes", value);
		}
	}

	public static AbstractSearchAndReplaceBinding DirectoryBinding
	{
		get
		{
			Init();
			return directoryBinding;
		}
		set
		{
			directoryBinding = value;
		}
	}

	public static AbstractSearchAndReplaceBinding CurrentDocumentDirectoryBinding
	{
		get
		{
			Init();
			return currentDocumentDirectoryBinding;
		}
		set
		{
			currentDocumentDirectoryBinding = value;
		}
	}

	public static AbstractSearchAndReplaceBinding CurrentSelectionBinding
	{
		get
		{
			Init();
			return currentSelectionBinding;
		}
		set
		{
			currentSelectionBinding = value;
		}
	}

	public static AbstractSearchAndReplaceBinding CurrentDocumentBinding
	{
		get
		{
			Init();
			return currentDocumentBinding;
		}
		set
		{
			currentDocumentBinding = value;
		}
	}

	public static List<AbstractSearchAndReplaceBinding> Bindings
	{
		get
		{
			Init();
			return bindings;
		}
	}

	public static AbstractSearchAndReplaceBinding SearchAndReplaceBinding
	{
		get
		{
			if (binding == null)
			{
				string text = properties.Get("SearchAndReplaceBinding", string.Empty);
				if (text != string.Empty)
				{
					foreach (AbstractSearchAndReplaceBinding binding in Bindings)
					{
						if (binding.ToString() == text)
						{
							SearchOptions.binding = binding;
							break;
						}
					}
				}
				if (SearchOptions.binding == null)
				{
					int num = Bindings.IndexOf(CurrentDocumentBinding);
					if (num != -1 && Bindings[num].Active)
					{
						SearchOptions.binding = CurrentDocumentBinding;
					}
					else
					{
						SearchOptions.binding = DirectoryBinding;
					}
				}
			}
			return SearchOptions.binding;
		}
		set
		{
			binding = value;
			if (value != null)
			{
				properties.Set("SearchAndReplaceBinding", value.ToString());
			}
		}
	}

	public static SearchStrategyType SearchStrategyType
	{
		get
		{
			return properties.Get("SearchStrategyType", SearchStrategyType.Normal);
		}
		set
		{
			properties.Set("SearchStrategyType", value);
		}
	}

	private static void Init()
	{
		if (bindings == null)
		{
			SearchAndReplaceDescriptor[] array = (SearchAndReplaceDescriptor[])AddInTree.GetTreeNode("/AddIns/DefaultTextEditor/Search/Engine").BuildChildItems(null).ToArray(typeof(SearchAndReplaceDescriptor));
			bindings = new List<AbstractSearchAndReplaceBinding>(array.Length);
			SearchAndReplaceDescriptor[] array2 = array;
			foreach (SearchAndReplaceDescriptor searchAndReplaceDescriptor in array2)
			{
				bindings.Add(searchAndReplaceDescriptor.Binding);
			}
		}
	}

	static SearchOptions()
	{
		findPattern = "";
		replacePattern = "";
		_ShowResults = true;
		storedOptions = false;
		properties = PropertyService.Get("SearchAndReplaceProperties", new Properties());
	}

	public static void Preserve()
	{
		_OldFindPattern = FindPattern;
		_OldSearchStrategyType = SearchStrategyType;
		_OldLookIn = LookIn;
		_OldLookInFiletypes = LookInFiletypes;
		_OldReplacePattern = ReplacePattern;
		_OldMatchCase = MatchCase;
		_OldMatchWholeWord = MatchWholeWord;
		_OldIncludeSubdirectories = IncludeSubdirectories;
		_OldMultiLineMatch = MultiLineMatch;
		_OldSearchAndReplaceBinding = SearchAndReplaceBinding;
		storedOptions = true;
	}

	public static void Restore()
	{
		if (storedOptions)
		{
			FindPattern = _OldFindPattern;
			SearchStrategyType = _OldSearchStrategyType;
			LookIn = _OldLookIn;
			LookInFiletypes = _OldLookInFiletypes;
			ReplacePattern = _OldReplacePattern;
			MatchCase = _OldMatchCase;
			MatchWholeWord = _OldMatchWholeWord;
			IncludeSubdirectories = _OldIncludeSubdirectories;
			MultiLineMatch = _OldMultiLineMatch;
			SearchAndReplaceBinding = _OldSearchAndReplaceBinding;
			storedOptions = false;
		}
	}
}
