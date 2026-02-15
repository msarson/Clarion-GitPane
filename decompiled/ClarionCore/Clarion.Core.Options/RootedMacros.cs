using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Clarion.Core.Options;

internal class RootedMacros : ReadOnlyCollection<KeyValuePair<string, string>>
{
	public string this[string s]
	{
		get
		{
			using (IEnumerator<KeyValuePair<string, string>> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, string> current = enumerator.Current;
					if (current.Key.Equals(s))
					{
						return current.Value;
					}
				}
			}
			return null;
		}
	}

	private static List<KeyValuePair<string, string>> Setup(string bin)
	{
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		int num = bin.IndexOf("\\bin", 1, StringComparison.OrdinalIgnoreCase);
		for (int num2 = num; num2 != -1; num2 = bin.IndexOf("\\bin", num + 1, StringComparison.OrdinalIgnoreCase))
		{
			num = num2;
		}
		if (num == -1)
		{
			list.Add(new KeyValuePair<string, string>("root", bin));
		}
		else
		{
			list.Add(new KeyValuePair<string, string>("root", bin.Substring(0, num)));
		}
		list.Add(new KeyValuePair<string, string>("reddir", bin));
		return list;
	}

	public RootedMacros()
		: base((IList<KeyValuePair<string, string>>)Setup(Versions.RedDir))
	{
	}

	public RootedMacros(string bin)
		: base((IList<KeyValuePair<string, string>>)Setup(bin))
	{
	}
}
