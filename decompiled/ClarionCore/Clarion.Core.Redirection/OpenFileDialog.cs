using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using SoftVelocity.Ide.Core;

namespace Clarion.Core.Redirection;

public class OpenFileDialog : SoftVelocity.Ide.Core.OpenFileDialog
{
	private bool checkExists;

	private bool useRelativePaths;

	private bool expand;

	private RedirectionFile redFile;

	private string curDir;

	private bool checkBinary;

	private static Properties dirList;

	public override bool CheckFileExists
	{
		get
		{
			return checkExists;
		}
		set
		{
			checkExists = value;
		}
	}

	public string CurrentDirectory
	{
		get
		{
			if (curDir == null)
			{
				curDir = RedirectionFile.CurrentDirectory;
			}
			return curDir;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				curDir = value;
			}
		}
	}

	public bool CheckBinary
	{
		get
		{
			return checkBinary;
		}
		set
		{
			checkBinary = value;
		}
	}

	public bool ExpandName
	{
		set
		{
			expand = value;
		}
	}

	public override string FileName
	{
		get
		{
			return GetPath(fd.FileName);
		}
		set
		{
			fd.FileName = value;
		}
	}

	public override string[] FileNames
	{
		get
		{
			List<string> list = new List<string>();
			string[] fileNames = base.FileNames;
			foreach (string fName in fileNames)
			{
				list.Add(GetPath(fName));
			}
			return list.ToArray();
		}
	}

	private static Properties DirectoryList
	{
		get
		{
			if (dirList == null)
			{
				dirList = PropertyService.Get("StartingDirectories", new Properties());
			}
			return dirList;
		}
	}

	internal static void InitialiseRedirection()
	{
		FileDialogService.UseRedirectionFile = PropertyService.Get("SoftVelocity.Gui.FileDialog.RememberInitialDirectory", defaultValue: false);
		FileDialogService.RegisterOpener(new RedOpenFileDialogMaker());
	}

	private bool Exists(string fName)
	{
		return redFile.Exists(fName, RedirectionFile.CurrentDirectory);
	}

	private string RedName(string fName)
	{
		return RedName(fName, expanded: false);
	}

	private string RedName(string fName, bool expanded)
	{
		string fileName = Path.GetFileName(fName);
		CurrentDirectory = Path.GetDirectoryName(fName);
		if (Exists(fileName))
		{
			string fullPath = Path.GetFullPath(redFile.OpenName(fileName, RedirectionFile.CurrentDirectory));
			string fullPath2 = Path.GetFullPath(fName);
			if (fullPath.Equals(fullPath2, StringComparison.InvariantCultureIgnoreCase))
			{
				return fileName;
			}
			if (CheckBinary && Exists(fullPath) && Exists(fullPath2))
			{
				byte[] array = File.ReadAllBytes(fullPath);
				byte[] array2 = File.ReadAllBytes(fullPath2);
				if (array.Length == array2.Length)
				{
					bool flag = false;
					for (uint num = 0u; num < array.Length; num++)
					{
						if (array[num] != array2[num])
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return fileName;
					}
				}
			}
			if (expanded)
			{
				return fullPath;
			}
			return fileName;
		}
		return fName;
	}

	private string _GetPath(string fName)
	{
		if (!expand)
		{
			return RedName(fName);
		}
		if (File.Exists(fName))
		{
			return fName;
		}
		string fileName = RedName(fName);
		if (redFile.Exists(fileName, CurrentDirectory))
		{
			return redFile.OpenName(fileName, CurrentDirectory);
		}
		return fName;
	}

	private string GetPath(string fName)
	{
		string text = _GetPath(fName);
		if (useRelativePaths && text.Contains(Path.DirectorySeparatorChar.ToString()))
		{
			text = FileUtility.GetRelativePath(RedirectionFile.CurrentDirectory, text);
		}
		return text;
	}

	public OpenFileDialog(RedirectionFile redFile)
		: this(redFile, useRelativePaths: true)
	{
	}

	internal OpenFileDialog(RedirectionFile redFile, bool useRelativePaths)
	{
		if (redFile == null)
		{
			throw new ArgumentNullException("RedirectionFile");
		}
		fd.CheckFileExists = false;
		fd.RestoreDirectory = true;
		checkExists = false;
		expand = true;
		this.redFile = redFile;
		checkBinary = false;
		this.useRelativePaths = useRelativePaths;
	}

	protected override string GetStartDir(string extension)
	{
		string text = DirectoryList.Get(MakePropertyName(extension), string.Empty);
		if (text == string.Empty)
		{
			string fileName = extension.Replace("*", Guid.NewGuid().ToString()).Replace('?', '_');
			try
			{
				text = Path.GetDirectoryName(redFile.CreateName(fileName, CurrentDirectory));
			}
			catch (Exception)
			{
			}
		}
		return text;
	}

	private static string MakePropertyName(string name)
	{
		return name.Replace("*", "U002A").Replace("?", "U003F");
	}

	internal static void SetStartingDir(string defaultExt, string dir)
	{
		DirectoryList.Set(MakePropertyName(defaultExt), dir);
	}

	protected override void UpdateStartingDirectory(string defaultExt, string dir)
	{
		base.UpdateStartingDirectory(defaultExt, dir);
		SetStartingDir(defaultExt, dir);
	}

	protected override bool NeedToDoDialogAgain(DialogResult res)
	{
		if (res == DialogResult.OK && checkExists && !Exists(Path.GetFileName(base.FileName)) && !File.Exists(base.FileName))
		{
			if (base.Multiselect)
			{
				if (base.Multiselect)
				{
					return FileNames.Length == 1;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected override string CorrectFileName(string fName)
	{
		return RedName(fName, expanded: true);
	}
}
