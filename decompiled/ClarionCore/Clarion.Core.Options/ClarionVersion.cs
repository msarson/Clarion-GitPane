using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ICSharpCode.Core;

namespace Clarion.Core.Options;

public class ClarionVersion
{
	private string name;

	private Properties data;

	private string prefix;

	private string tail;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (!name.Equals(value))
			{
				Versions.RenameVersion(name, value);
				name = value;
			}
		}
	}

	public Properties Properties => data;

	private string ACompiler
	{
		get
		{
			Properties compilers = Compilers;
			return compilers[compilers.Elements[0]];
		}
	}

	public string Prefix
	{
		get
		{
			if (prefix == null)
			{
				string aCompiler = ACompiler;
				int index = 1;
				StringBuilder stringBuilder = new StringBuilder(2);
				while (char.IsDigit(aCompiler[index]))
				{
					stringBuilder.Append(aCompiler[index++]);
				}
				prefix = stringBuilder.ToString();
				if (string.IsNullOrEmpty(prefix) || CwVersion >= 7300)
				{
					prefix = "la";
				}
			}
			return prefix;
		}
	}

	public string Tail
	{
		get
		{
			if (tail == null)
			{
				string aCompiler = ACompiler;
				int index = aCompiler.IndexOf('.') - 1;
				tail = ((aCompiler.ToLower()[index] == 'x') ? "x" : string.Empty);
			}
			return tail;
		}
	}

	public int DDETimeOut
	{
		get
		{
			return data.Get("DdeTimeOut", 0);
		}
		set
		{
			data.Set("DdeTimeOut", value);
		}
	}

	public bool IsWindowsVersion
	{
		get
		{
			return data.Get("IsWindowsVersion", defaultValue: false);
		}
		set
		{
			data.Set("IsWindowsVersion", value);
		}
	}

	public bool Is62
	{
		get
		{
			return data.Get("IsClarion62", defaultValue: false);
		}
		set
		{
			data.Set("IsClarion62", value);
		}
	}

	public int CwVersion
	{
		get
		{
			int num = data.Get("CWVersion", 0);
			if (num == 0)
			{
				SetCwVersion();
				num = data.Get("CWVersion", 0);
			}
			return num;
		}
		set
		{
			data.Set("CWVersion", value);
			if (!data.Contains("WideAppgenDialogs"))
			{
				if (value >= 11000)
				{
					SupportsWideAppgenDialogs = true;
				}
				else
				{
					SupportsWideAppgenDialogs = false;
				}
			}
		}
	}

	public string OvlFile
	{
		get
		{
			return data.Get("ovl", "");
		}
		set
		{
			data.Set("ovl", value);
		}
	}

	public string IniFile
	{
		get
		{
			return data.Get("ini", "");
		}
		set
		{
			data.Set("ini", value);
		}
	}

	public string Libsrc
	{
		get
		{
			return data.Get("libsrc", "");
		}
		set
		{
			data.Set("libsrc", value);
		}
	}

	public string Path
	{
		get
		{
			return data.Get("path", "");
		}
		set
		{
			data.Set("path", value);
		}
	}

	public bool SupportsWideAppgenDialogs
	{
		get
		{
			return data.Get("WideAppgenDialogs", defaultValue: false);
		}
		set
		{
			data.Set("WideAppgenDialogs", value);
		}
	}

	public RedirectionVersion RedirectionFile
	{
		get
		{
			return new RedirectionVersion(data.Get("RedirectionFile", new Properties()));
		}
		set
		{
			data.Set("RedirectionFile", value.Properties);
		}
	}

	public Properties Compilers
	{
		get
		{
			return data.Get("Compilers", new Properties());
		}
		set
		{
			data.Set("Compilers", value);
		}
	}

	private void Setup(string name)
	{
		this.name = name;
		data = new Properties();
	}

	internal ClarionVersion(string name)
	{
		Setup(name);
	}

	internal ClarionVersion(string name, string path)
	{
		Setup(name);
		data.Set("path", path);
	}

	internal ClarionVersion(string name, Properties p)
	{
		this.name = name;
		data = p;
	}

	internal void SetCwVersion()
	{
		int num = 0;
		if (IsWindowsVersion)
		{
			string text = Compilers["clw"];
			if (text.Equals("c4clwx.exe", StringComparison.InvariantCultureIgnoreCase))
			{
				text = "clarion4.exe";
			}
			text = System.IO.Path.Combine(Path, text);
			try
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(text);
				num = ((versionInfo.FileMajorPart == 0) ? (versionInfo.FileMinorPart * 1000 + versionInfo.FileBuildPart) : ((versionInfo.FileMajorPart <= 6) ? (versionInfo.FileMajorPart * 1000 + (versionInfo.FileMinorPart >> 16)) : (versionInfo.FileMajorPart * 1000 + versionInfo.FileMinorPart)));
			}
			catch
			{
				return;
			}
		}
		else
		{
			num = 2000;
		}
		CwVersion = num;
	}

	public string Expand(string fileName, string l)
	{
		int num = fileName.IndexOf('%');
		if (num != -1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num2 = 0;
			while (num != -1)
			{
				if (num > num2)
				{
					stringBuilder.Append(fileName.Substring(num2, num - num2));
				}
				if (fileName[num + 2] != '%')
				{
					return fileName;
				}
				switch (fileName[num + 1])
				{
				case 'V':
				case 'v':
					stringBuilder.Append(Prefix);
					break;
				case 'X':
				case 'x':
					stringBuilder.Append(Tail);
					break;
				case 'L':
				case 'l':
					if (!string.IsNullOrEmpty(l))
					{
						stringBuilder.Append(l);
					}
					break;
				default:
					return fileName;
				}
				num2 = num + 3;
				num = fileName.IndexOf('%', num2);
			}
			stringBuilder.Append(fileName.Substring(num2));
			return stringBuilder.ToString();
		}
		return fileName;
	}
}
