using System;
using System.Collections;
using System.Text;

namespace ICSharpCode.SharpDevelop.DefaultEditor;

public class CharacterEncodings
{
	private class EncodingWrapper : IComparable
	{
		private Encoding _encoding;

		private int _cp;

		public int CodePage => _cp;

		public Encoding Encoding => _encoding;

		public string Name
		{
			get
			{
				if (_cp == 0)
				{
					return "System Default  [ " + Encoding.EncodingName + " ]";
				}
				return Encoding.EncodingName;
			}
		}

		public EncodingWrapper(int cp)
		{
			_encoding = Encoding.GetEncoding(cp);
			_cp = cp;
		}

		public override string ToString()
		{
			return _cp.ToString();
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (o == this)
			{
				return true;
			}
			if (o is EncodingWrapper)
			{
				return _cp == ((EncodingWrapper)o)._cp;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _cp;
		}

		int IComparable.CompareTo(object o)
		{
			return Name.CompareTo(((EncodingWrapper)o).Name);
		}
	}

	private static int[] _wellKnownCodePages;

	private static ArrayList _encodings;

	private static ArrayList _names;

	private static Hashtable _cp2index;

	public static IList Names => _names;

	private static IList GetSupportedEncodings()
	{
		ArrayList arrayList = new ArrayList();
		int[] wellKnownCodePages = _wellKnownCodePages;
		foreach (int cp in wellKnownCodePages)
		{
			try
			{
				arrayList.Add(new EncodingWrapper(cp));
			}
			catch
			{
			}
		}
		arrayList.Sort();
		return arrayList;
	}

	static CharacterEncodings()
	{
		_wellKnownCodePages = new int[99]
		{
			37, 437, 500, 708, 850, 852, 855, 857, 858, 860,
			861, 862, 863, 864, 865, 866, 869, 870, 874, 875,
			932, 936, 949, 950, 1026, 1047, 1140, 1141, 1142, 1143,
			1144, 1145, 1146, 1147, 1148, 1149, 1200, 1201, 1250, 1251,
			1252, 1253, 1254, 1255, 1256, 1257, 1258, 10000, 10007, 10017,
			10079, 20127, 20261, 20273, 20277, 20278, 20280, 20284, 20285, 20290,
			20297, 20420, 20424, 20866, 20871, 21025, 21866, 28591, 28592, 28593,
			28594, 28595, 28596, 28597, 28598, 28599, 28605, 38598, 50220, 50221,
			50222, 50225, 50227, 51932, 51936, 52936, 54936, 57002, 57003, 57004,
			57005, 57006, 57007, 57008, 57009, 57010, 57011, 65000, 65001
		};
		_encodings = new ArrayList();
		_encodings.AddRange(GetSupportedEncodings());
		_names = new ArrayList();
		_cp2index = new Hashtable();
		int num = 0;
		foreach (EncodingWrapper encoding in _encodings)
		{
			_names.Add(encoding.Name);
			_cp2index[encoding.CodePage] = num;
			num++;
		}
	}

	public static Encoding GetEncodingByIndex(int i)
	{
		if (i < 0 || i >= _encodings.Count)
		{
			return null;
		}
		return ((EncodingWrapper)_encodings[i]).Encoding;
	}

	public static Encoding GetEncodingByCodePage(int cp)
	{
		return GetEncodingByIndex(GetEncodingIndex(cp));
	}

	public static int GetEncodingIndex(int cp)
	{
		try
		{
			return (int)_cp2index[cp];
		}
		catch
		{
			return (int)_cp2index[Encoding.GetEncoding(0).CodePage];
		}
	}

	public static int GetCodePageByIndex(int i)
	{
		return GetEncodingByIndex(i)?.CodePage ?? (-1);
	}

	public static bool IsUnicode(Encoding encoding)
	{
		return IsUnicode(encoding.CodePage);
	}

	public static bool IsUnicode(int codePage)
	{
		if (codePage != 1200 && codePage != 1201 && codePage != 65000)
		{
			return codePage == 65001;
		}
		return true;
	}
}
