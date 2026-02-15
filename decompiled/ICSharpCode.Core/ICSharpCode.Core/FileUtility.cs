using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;
using SoftVelocity.Ide.Core;

namespace ICSharpCode.Core;

public static class FileUtility
{
	private class LoadWrapper
	{
		private NamedFileOperationDelegate loadFile;

		private string fileName;

		public LoadWrapper(NamedFileOperationDelegate loadFile, string fileName)
		{
			this.loadFile = loadFile;
			this.fileName = fileName;
		}

		public void Invoke()
		{
			loadFile(fileName);
		}
	}

	private const int BYTES_TO_READ = 8;

	private const string fileNameRegEx = "^([a-zA-Z]:)?[^:]+$";

	private static readonly char[] separators = new char[3]
	{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar,
		Path.VolumeSeparatorChar
	};

	private static string applicationRootPath = AppDomain.CurrentDomain.BaseDirectory;

	public static int MaxPathLength = 260;

	public static string ApplicationRootPath
	{
		get
		{
			return applicationRootPath;
		}
		set
		{
			applicationRootPath = value;
		}
	}

	public static string NETFrameworkInstallRoot
	{
		get
		{
			using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework");
			object value = registryKey.GetValue("InstallRoot");
			return (value == null) ? string.Empty : value.ToString();
		}
	}

	public static string NetSdkInstallRoot
	{
		get
		{
			string text = string.Empty;
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SDKs\\Windows\\v7.0A\\WinSDK-SDKTools");
			if (registryKey != null)
			{
				object value = registryKey.GetValue("InstallationFolder");
				text = ((value == null) ? string.Empty : value.ToString());
				registryKey.Close();
			}
			if (text.Length == 0)
			{
				RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Microsoft SDKs\\Windows\\v6.0");
				if (registryKey2 != null)
				{
					object value2 = registryKey2.GetValue("InstallationFolder");
					text = ((value2 == null) ? string.Empty : value2.ToString());
					registryKey2.Close();
				}
			}
			if (text.Length == 0)
			{
				RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework");
				if (registryKey3 != null)
				{
					object value3 = registryKey3.GetValue("sdkInstallRootv2.0");
					text = ((value3 == null) ? string.Empty : value3.ToString());
					registryKey3.Close();
				}
			}
			return text;
		}
	}

	public static event FileNameEventHandler FileLoaded;

	public static event FileNameCancelEventHandler FileLoading;

	public static event FileNameEventHandler FileSaved;

	public static bool FilesAreEqual(string file1, string file2)
	{
		FileInfo fileInfo = new FileInfo(file1);
		FileInfo fileInfo2 = new FileInfo(file2);
		if (!fileInfo.Exists || !fileInfo2.Exists || fileInfo.Length != fileInfo2.Length)
		{
			return false;
		}
		int num = (int)Math.Ceiling((double)fileInfo.Length / 8.0);
		using (FileStream fileStream = fileInfo.OpenRead())
		{
			using FileStream fileStream2 = fileInfo2.OpenRead();
			byte[] array = new byte[8];
			byte[] array2 = new byte[8];
			for (int i = 0; i < num; i++)
			{
				fileStream.Read(array, 0, 8);
				fileStream2.Read(array2, 0, 8);
				if (BitConverter.ToInt64(array, 0) != BitConverter.ToInt64(array2, 0))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static string NormalizePath(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return fileName;
		}
		if (fileName[0] == '.')
		{
			fileName = Directory.GetCurrentDirectory() + '\\' + fileName;
		}
		bool flag = false;
		int i;
		for (i = 0; i < fileName.Length && fileName[i] != '/' && fileName[i] != '\\'; i++)
		{
			if (fileName[i] == ':')
			{
				if (i > 1)
				{
					flag = true;
				}
				break;
			}
		}
		char c = (flag ? '/' : Path.DirectorySeparatorChar);
		StringBuilder stringBuilder = new StringBuilder();
		if ((!flag && fileName.StartsWith("\\\\")) || fileName.StartsWith("//"))
		{
			i = 2;
			stringBuilder.Append(c);
		}
		else
		{
			i = 0;
		}
		int num = i;
		bool flag2 = false;
		for (; i <= fileName.Length; i++)
		{
			if (i != fileName.Length && fileName[i] != '/' && fileName[i] != '\\')
			{
				continue;
			}
			int num2 = i - num;
			switch (num2)
			{
			case 0:
				if (flag || (i == 0 && Environment.OSVersion.Platform == PlatformID.Unix))
				{
					stringBuilder.Append(c);
				}
				else if (!flag && i == 0)
				{
					flag2 = true;
				}
				break;
			case 1:
				if (fileName[num] != '.')
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(c);
					}
					stringBuilder.Append(fileName[num]);
				}
				break;
			case 2:
				if (fileName[num] == '.' && fileName[num + 1] == '.')
				{
					int num3 = stringBuilder.Length - 1;
					while (num3 >= 0 && stringBuilder[num3] != c)
					{
						num3--;
					}
					if (num3 > 0)
					{
						stringBuilder.Length = num3;
					}
					break;
				}
				goto default;
			default:
				if (flag2)
				{
					stringBuilder.Append(c);
					flag2 = false;
				}
				else if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(c);
				}
				stringBuilder.Append(fileName, num, num2);
				break;
			}
			num = i + 1;
		}
		if (!flag)
		{
			if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] == c)
			{
				stringBuilder.Length--;
			}
			if (stringBuilder.Length == 2 && stringBuilder[1] == ':')
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string Combine(params string[] paths)
	{
		if (paths == null || paths.Length == 0)
		{
			return string.Empty;
		}
		string text = paths[0];
		for (int i = 1; i < paths.Length; i++)
		{
			text = Path.Combine(text, paths[i]);
		}
		return text;
	}

	public static bool IsUrl(string path)
	{
		return path.IndexOf(':') >= 2;
	}

	public static string GetRelativePath(string baseDirectoryPath, string absPath)
	{
		if (IsUrl(absPath) || IsUrl(baseDirectoryPath))
		{
			return absPath;
		}
		try
		{
			if (string.IsNullOrEmpty(baseDirectoryPath))
			{
				baseDirectoryPath = ".";
			}
			baseDirectoryPath = Path.GetFullPath(baseDirectoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			absPath = Path.GetFullPath(absPath);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException("GetRelativePath error '" + baseDirectoryPath + "' -> '" + absPath + "'", innerException);
		}
		string[] array = baseDirectoryPath.Split(separators);
		string[] array2 = absPath.Split(separators);
		int i;
		for (i = 0; i < Math.Min(array.Length, array2.Length) && array[i].Equals(array2[i], StringComparison.OrdinalIgnoreCase); i++)
		{
		}
		if (i == 0)
		{
			return absPath;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (i == array.Length && array.Length == array2.Length)
		{
			stringBuilder.Append('.');
			stringBuilder.Append(Path.DirectorySeparatorChar);
		}
		else
		{
			for (int j = i; j < array.Length; j++)
			{
				stringBuilder.Append("..");
				stringBuilder.Append(Path.DirectorySeparatorChar);
			}
		}
		stringBuilder.Append(string.Join(Path.DirectorySeparatorChar.ToString(), array2, i, array2.Length - i));
		return stringBuilder.ToString();
	}

	public static string GetAbsolutePath(string baseDirectoryPath, string relPath)
	{
		return Path.GetFullPath(Path.Combine(baseDirectoryPath, relPath));
	}

	public static bool IsEqualFileName(string fileName1, string fileName2)
	{
		if (string.IsNullOrEmpty(fileName1) || string.IsNullOrEmpty(fileName2))
		{
			return false;
		}
		char c = fileName1[fileName1.Length - 1];
		if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
		{
			fileName1 = fileName1.Substring(0, fileName1.Length - 1);
		}
		c = fileName2[fileName2.Length - 1];
		if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
		{
			fileName2 = fileName2.Substring(0, fileName2.Length - 1);
		}
		try
		{
			if (fileName1.Length < 2 || fileName1[1] != ':' || fileName1.IndexOf("/.") >= 0 || fileName1.IndexOf("\\.") >= 0)
			{
				fileName1 = Path.GetFullPath(fileName1);
			}
			if (fileName2.Length < 2 || fileName2[1] != ':' || fileName2.IndexOf("/.") >= 0 || fileName2.IndexOf("\\.") >= 0)
			{
				fileName2 = Path.GetFullPath(fileName2);
			}
		}
		catch (Exception)
		{
		}
		return string.Equals(fileName1, fileName2, StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsBaseDirectory(string baseDirectory, string testDirectory)
	{
		try
		{
			baseDirectory = Path.GetFullPath(baseDirectory).ToUpperInvariant();
			testDirectory = Path.GetFullPath(testDirectory).ToUpperInvariant();
			baseDirectory = baseDirectory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			testDirectory = testDirectory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			if (baseDirectory[baseDirectory.Length - 1] != Path.DirectorySeparatorChar)
			{
				baseDirectory += Path.DirectorySeparatorChar;
			}
			if (testDirectory[testDirectory.Length - 1] != Path.DirectorySeparatorChar)
			{
				testDirectory += Path.DirectorySeparatorChar;
			}
			return testDirectory.StartsWith(baseDirectory);
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static string RenameBaseDirectory(string fileName, string oldDirectory, string newDirectory)
	{
		fileName = Path.GetFullPath(fileName);
		oldDirectory = Path.GetFullPath(oldDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		newDirectory = Path.GetFullPath(newDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		if (IsBaseDirectory(oldDirectory, fileName))
		{
			if (fileName.Length == oldDirectory.Length)
			{
				return newDirectory;
			}
			return Path.Combine(newDirectory, fileName.Substring(oldDirectory.Length + 1));
		}
		return fileName;
	}

	public static void DeepCopy(string sourceDirectory, string destinationDirectory, bool overwrite)
	{
		if (!Directory.Exists(destinationDirectory))
		{
			Directory.CreateDirectory(destinationDirectory);
		}
		string[] files = Directory.GetFiles(sourceDirectory);
		foreach (string text in files)
		{
			File.Copy(text, Path.Combine(destinationDirectory, Path.GetFileName(text)), overwrite);
		}
		string[] directories = Directory.GetDirectories(sourceDirectory);
		foreach (string text2 in directories)
		{
			DeepCopy(text2, Path.Combine(destinationDirectory, Path.GetFileName(text2)), overwrite);
		}
	}

	public static List<string> SearchDirectory(string directory, string filemask, bool searchSubdirectories, bool ignoreHidden)
	{
		List<string> list = new List<string>();
		SearchDirectory(directory, filemask, list, searchSubdirectories, ignoreHidden);
		return list;
	}

	public static List<string> SearchDirectory(string directory, string filemask, bool searchSubdirectories)
	{
		return SearchDirectory(directory, filemask, searchSubdirectories, ignoreHidden: false);
	}

	public static List<string> SearchDirectory(string directory, string filemask)
	{
		return SearchDirectory(directory, filemask, searchSubdirectories: true, ignoreHidden: false);
	}

	private static void SearchDirectory(string directory, string filemask, List<string> collection, bool searchSubdirectories, bool ignoreHidden)
	{
		string[] files = Directory.GetFiles(directory, filemask);
		string[] array = files;
		foreach (string text in array)
		{
			if (!ignoreHidden || (File.GetAttributes(text) & FileAttributes.Hidden) != FileAttributes.Hidden)
			{
				collection.Add(text);
			}
		}
		if (!searchSubdirectories)
		{
			return;
		}
		string[] directories = Directory.GetDirectories(directory);
		string[] array2 = directories;
		foreach (string text2 in array2)
		{
			if (!ignoreHidden || (File.GetAttributes(text2) & FileAttributes.Hidden) != FileAttributes.Hidden)
			{
				SearchDirectory(text2, filemask, collection, searchSubdirectories, ignoreHidden);
			}
		}
	}

	public static bool IsValidFileName(string fileName)
	{
		if (fileName == null || fileName.Length == 0 || fileName.Length >= MaxPathLength)
		{
			return false;
		}
		if (fileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
		{
			return false;
		}
		if (fileName.IndexOf('?') >= 0 || fileName.IndexOf('*') >= 0)
		{
			return false;
		}
		if (!Regex.IsMatch(fileName, "^([a-zA-Z]:)?[^:]+$"))
		{
			return false;
		}
		string text = Path.GetFileNameWithoutExtension(fileName);
		if (text != null)
		{
			text = text.ToUpperInvariant();
		}
		switch (text)
		{
		case "CON":
		case "PRN":
		case "AUX":
		case "NUL":
			return false;
		default:
		{
			char c = ((text.Length == 4) ? text[3] : '\0');
			if (text.StartsWith("COM") || text.StartsWith("LPT"))
			{
				return !char.IsDigit(c);
			}
			return true;
		}
		}
	}

	public static bool IsValidDirectoryName(string name)
	{
		if (!IsValidFileName(name))
		{
			return false;
		}
		if (name.IndexOfAny(new char[2]
		{
			Path.AltDirectorySeparatorChar,
			Path.DirectorySeparatorChar
		}) >= 0)
		{
			return false;
		}
		if (name.Trim(' ').Length == 0)
		{
			return false;
		}
		return true;
	}

	public static bool TestFileExists(string filename)
	{
		if (!File.Exists(filename))
		{
			MessageService.ShowWarning(StringParser.Parse("${res:Fileutility.CantFindFileError}", new string[1, 2] { { "FILE", filename } }));
			return false;
		}
		return true;
	}

	public static bool IsDirectory(string filename)
	{
		if (!Directory.Exists(filename))
		{
			return false;
		}
		FileAttributes attributes = File.GetAttributes(filename);
		return (attributes & FileAttributes.Directory) != 0;
	}

	public static Regex[] ToRegEx(string filePatterns)
	{
		string[] array = filePatterns.Split(';');
		List<Regex> list = new List<Regex>(array.Length);
		string[] array2 = array;
		foreach (string text in array2)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('^');
			string text2 = text;
			for (int j = 0; j < text2.Length; j++)
			{
				char c = text2[j];
				switch (c)
				{
				case '?':
					stringBuilder.Append('.');
					break;
				case '*':
					stringBuilder.Append(".*");
					break;
				default:
					stringBuilder.Append(Regex.Escape(c.ToString()));
					break;
				}
			}
			if (text[text.Length - 1] != '*')
			{
				stringBuilder.Append('$');
			}
			list.Add(new Regex(stringBuilder.ToString(), RegexOptions.IgnoreCase));
		}
		return list.ToArray();
	}

	public static bool Matches(string file, Regex[] patterns)
	{
		foreach (Regex regex in patterns)
		{
			if (regex.IsMatch(file))
			{
				return true;
			}
		}
		return false;
	}

	private static bool MatchN(string src, int srcidx, string pattern, int patidx)
	{
		int length = pattern.Length;
		int length2 = src.Length;
		while (patidx != length)
		{
			char c = pattern[patidx++];
			switch (c)
			{
			case '?':
				if (srcidx == src.Length)
				{
					return false;
				}
				srcidx++;
				break;
			default:
				if (srcidx == src.Length || src[srcidx] != c)
				{
					return false;
				}
				srcidx++;
				break;
			case '*':
				if (patidx == pattern.Length)
				{
					return true;
				}
				while (srcidx < length2)
				{
					if (MatchN(src, srcidx, pattern, patidx))
					{
						return true;
					}
					srcidx++;
				}
				return false;
			}
		}
		return srcidx == length2;
	}

	private static bool Match(string src, string pattern)
	{
		if (pattern[0] == '*')
		{
			int num = pattern.Length;
			int length = src.Length;
			while (--num > 0)
			{
				if (pattern[num] == '*')
				{
					return MatchN(src, 0, pattern, 0);
				}
				if (length-- == 0)
				{
					return false;
				}
				if (pattern[num] != src[length] && pattern[num] != '?')
				{
					return false;
				}
			}
			return true;
		}
		return MatchN(src, 0, pattern, 0);
	}

	public static bool MatchesPattern(string filename, string pattern)
	{
		filename = filename.ToUpper();
		pattern = pattern.ToUpper();
		string[] array = pattern.Split(';');
		string[] array2 = array;
		foreach (string pattern2 in array2)
		{
			if (Match(filename, pattern2))
			{
				return true;
			}
		}
		return false;
	}

	public static FileOperationResult ObservedSave(FileOperationDelegate saveFile, string fileName, string message, FileErrorPolicy policy)
	{
		try
		{
			saveFile();
			if (!string.IsNullOrEmpty(fileName))
			{
				OnFileSaved(new FileNameEventArgs(fileName));
			}
			return FileOperationResult.OK;
		}
		catch (Exception exceptionGot)
		{
			switch (policy)
			{
			case FileErrorPolicy.Inform:
			{
				using (SaveErrorInformDialog saveErrorInformDialog = new SaveErrorInformDialog(fileName, message, "${res:FileUtilityService.ErrorWhileSaving}", exceptionGot))
				{
					saveErrorInformDialog.ShowDialog();
				}
				break;
			}
			case FileErrorPolicy.ProvideAlternative:
			{
				using (SaveErrorChooseDialog saveErrorChooseDialog = new SaveErrorChooseDialog(fileName, message, "${res:FileUtilityService.ErrorWhileSaving}", exceptionGot, chooseLocationEnabled: false))
				{
					switch (saveErrorChooseDialog.ShowDialog())
					{
					case DialogResult.Retry:
						return ObservedSave(saveFile, fileName, message, policy);
					case DialogResult.Ignore:
						return FileOperationResult.Failed;
					case DialogResult.OK:
					case DialogResult.Cancel:
					case DialogResult.Abort:
						break;
					}
				}
				break;
			}
			}
		}
		return FileOperationResult.Failed;
	}

	public static FileOperationResult ObservedSave(FileOperationDelegate saveFile, string fileName, FileErrorPolicy policy)
	{
		return ObservedSave(saveFile, fileName, ResourceService.GetString("ICSharpCode.Services.FileUtilityService.CantSaveFileStandardText"), policy);
	}

	public static FileOperationResult ObservedSave(FileOperationDelegate saveFile, string fileName)
	{
		return ObservedSave(saveFile, fileName, FileErrorPolicy.Inform);
	}

	public static FileOperationResult ObservedSave(NamedFileOperationDelegate saveFileAs, string fileName, string message, FileErrorPolicy policy, bool withCreateDir)
	{
		try
		{
			if (withCreateDir)
			{
				string directoryName = Path.GetDirectoryName(fileName);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
			}
			saveFileAs(fileName);
			if (!string.IsNullOrEmpty(fileName))
			{
				OnFileSaved(new FileNameEventArgs(fileName));
			}
			return FileOperationResult.OK;
		}
		catch (Exception exceptionGot)
		{
			switch (policy)
			{
			default:
				goto end_IL_0044;
			case FileErrorPolicy.Inform:
			{
				using (SaveErrorInformDialog saveErrorInformDialog = new SaveErrorInformDialog(fileName, message, "${res:FileUtilityService.ErrorWhileSaving}", exceptionGot))
				{
					saveErrorInformDialog.ShowDialog();
				}
				goto end_IL_0044;
			}
			case FileErrorPolicy.ProvideAlternative:
				break;
			}
			while (true)
			{
				using (SaveErrorChooseDialog saveErrorChooseDialog = new SaveErrorChooseDialog(fileName, message, "${res:FileUtilityService.ErrorWhileSaving}", exceptionGot, chooseLocationEnabled: true))
				{
					switch (saveErrorChooseDialog.ShowDialog())
					{
					case DialogResult.OK:
					{
						using (SoftVelocity.Ide.Core.SaveFileDialog saveFileDialog = FileDialogService.SaveFileDialog())
						{
							saveFileDialog.OverwritePrompt = true;
							saveFileDialog.AddExtension = true;
							saveFileDialog.CheckFileExists = false;
							saveFileDialog.CheckPathExists = true;
							saveFileDialog.Title = "Choose alternate file name";
							saveFileDialog.FileName = fileName;
							if (saveFileDialog.ShowDialog() == DialogResult.OK)
							{
								return ObservedSave(saveFileAs, saveFileDialog.FileName, message, policy);
							}
						}
						continue;
					}
					case DialogResult.Retry:
						return ObservedSave(saveFileAs, fileName, message, policy);
					case DialogResult.Ignore:
						return FileOperationResult.Failed;
					case DialogResult.Cancel:
					case DialogResult.Abort:
						break;
					}
				}
				break;
			}
			end_IL_0044:;
		}
		return FileOperationResult.Failed;
	}

	public static FileOperationResult ObservedSave(NamedFileOperationDelegate saveFileAs, string fileName, string message, FileErrorPolicy policy)
	{
		return ObservedSave(saveFileAs, fileName, message, policy, withCreateDir: true);
	}

	public static FileOperationResult ObservedSave(NamedFileOperationDelegate saveFileAs, string fileName, FileErrorPolicy policy)
	{
		return ObservedSave(saveFileAs, fileName, ResourceService.GetString("ICSharpCode.Services.FileUtilityService.CantSaveFileStandardText"), policy);
	}

	public static FileOperationResult ObservedSave(NamedFileOperationDelegate saveFileAs, string fileName)
	{
		return ObservedSave(saveFileAs, fileName, FileErrorPolicy.Inform);
	}

	public static FileOperationResult ObservedLoad(FileOperationDelegate loadFile, string fileName, string message, FileErrorPolicy policy)
	{
		try
		{
			if (!OnFileLoading(fileName))
			{
				return FileOperationResult.Failed;
			}
			loadFile();
			OnFileLoaded(new FileNameEventArgs(fileName));
			return FileOperationResult.OK;
		}
		catch (Exception exceptionGot)
		{
			switch (policy)
			{
			case FileErrorPolicy.Inform:
			{
				using (SaveErrorInformDialog saveErrorInformDialog = new SaveErrorInformDialog(fileName, message, "${res:FileUtilityService.ErrorWhileLoading}", exceptionGot))
				{
					saveErrorInformDialog.ShowDialog();
				}
				break;
			}
			case FileErrorPolicy.ProvideAlternative:
			{
				using (SaveErrorChooseDialog saveErrorChooseDialog = new SaveErrorChooseDialog(fileName, message, "${res:FileUtilityService.ErrorWhileLoading}", exceptionGot, chooseLocationEnabled: false))
				{
					switch (saveErrorChooseDialog.ShowDialog())
					{
					case DialogResult.Retry:
						return ObservedLoad(loadFile, fileName, message, policy);
					case DialogResult.Ignore:
						return FileOperationResult.Failed;
					case DialogResult.OK:
					case DialogResult.Cancel:
					case DialogResult.Abort:
						break;
					}
				}
				break;
			}
			}
		}
		return FileOperationResult.Failed;
	}

	public static FileOperationResult ObservedLoad(FileOperationDelegate loadFile, string fileName, FileErrorPolicy policy)
	{
		return ObservedLoad(loadFile, fileName, ResourceService.GetString("ICSharpCode.Services.FileUtilityService.CantLoadFileStandardText"), policy);
	}

	public static FileOperationResult ObservedLoad(FileOperationDelegate loadFile, string fileName)
	{
		return ObservedLoad(loadFile, fileName, FileErrorPolicy.Inform);
	}

	public static FileOperationResult ObservedLoad(NamedFileOperationDelegate saveFileAs, string fileName, string message, FileErrorPolicy policy)
	{
		return ObservedLoad(new LoadWrapper(saveFileAs, fileName).Invoke, fileName, message, policy);
	}

	public static FileOperationResult ObservedLoad(NamedFileOperationDelegate saveFileAs, string fileName, FileErrorPolicy policy)
	{
		return ObservedLoad(saveFileAs, fileName, ResourceService.GetString("ICSharpCode.Services.FileUtilityService.CantLoadFileStandardText"), policy);
	}

	public static FileOperationResult ObservedLoad(NamedFileOperationDelegate saveFileAs, string fileName)
	{
		return ObservedLoad(saveFileAs, fileName, FileErrorPolicy.Inform);
	}

	public static bool OnFileLoading(string fileName)
	{
		FileNameCancelEventArgs e = new FileNameCancelEventArgs(fileName);
		if (FileUtility.FileLoading != null)
		{
			FileUtility.FileLoading(null, e);
		}
		return !e.Cancel;
	}

	private static void OnFileLoaded(FileNameEventArgs e)
	{
		if (FileUtility.FileLoaded != null)
		{
			FileUtility.FileLoaded(null, e);
		}
	}

	private static void OnFileSaved(FileNameEventArgs e)
	{
		if (FileUtility.FileSaved != null)
		{
			FileUtility.FileSaved(null, e);
		}
	}
}
