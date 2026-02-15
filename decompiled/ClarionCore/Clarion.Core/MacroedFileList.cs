using System;
using System.Collections.Generic;
using Clarion.Core.Options;

namespace Clarion.Core;

public class MacroedFileList : SortedList<string, object>
{
	private ClarionVersion ver;

	public string Convert(string fName)
	{
		return ver.Expand(fName, null).ToUpper();
	}

	public MacroedFileList(ClarionVersion version)
		: base((IComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
		ver = version;
	}

	public MacroedFileList(ClarionVersion version, int initialSize)
		: base(initialSize)
	{
		ver = version;
	}

	public bool AddToList(string fName)
	{
		return AddToList(fName, fName);
	}

	public bool AddToList(string fName, object other)
	{
		fName = Convert(fName);
		if (!base.ContainsKey(fName))
		{
			Add(fName, other);
			return true;
		}
		return false;
	}

	public new bool Remove(string fName)
	{
		return base.Remove(Convert(fName));
	}

	public new bool ContainsKey(string key)
	{
		return base.ContainsKey(Convert(key));
	}
}
