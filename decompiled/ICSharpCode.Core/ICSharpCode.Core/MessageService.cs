using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public static class MessageService
{
	public delegate void ShowErrorDelegate(Exception ex, string message);

	private delegate void ShowMessageDelegate(string message, string caption);

	private const int recursionDeepOnMessageInnerException = 1;

	private static Form mainForm;

	private static Cursor prevCursor = null;

	private static bool quiet = false;

	private static bool inited = false;

	private static ShowErrorDelegate customErrorReporter;

	private static string defaultMessageBoxTitle = "MessageBox";

	private static string productName = "Clarion";

	public static Form MainForm
	{
		get
		{
			return mainForm;
		}
		set
		{
			mainForm = value;
			mainForm.Closed += MainForm_Closed;
		}
	}

	public static bool QuietMode
	{
		get
		{
			if (!inited)
			{
				inited = true;
				quiet = PropertyService.Get("CoreProperties.QuietMode", defaultValue: true);
			}
			return quiet;
		}
		set
		{
			inited = true;
			quiet = value;
		}
	}

	public static ShowErrorDelegate CustomErrorReporter
	{
		get
		{
			return customErrorReporter;
		}
		set
		{
			customErrorReporter = value;
		}
	}

	public static string ProductName
	{
		get
		{
			return productName;
		}
		set
		{
			productName = value;
		}
	}

	public static string DefaultMessageBoxTitle
	{
		get
		{
			return defaultMessageBoxTitle;
		}
		set
		{
			defaultMessageBoxTitle = value;
		}
	}

	private static void SaveCursorAndArrow()
	{
		if (Cursor.Current != Cursors.Arrow)
		{
			SaveCursor();
			Cursor.Current = Cursors.Arrow;
		}
	}

	private static void SaveCursor()
	{
		prevCursor = Cursor.Current;
	}

	private static void RestoreCursor()
	{
		if (prevCursor != null)
		{
			Cursor.Current = prevCursor;
			prevCursor = null;
		}
	}

	private static void MainForm_Closed(object sender, EventArgs e)
	{
		mainForm.Closed -= MainForm_Closed;
		mainForm = null;
	}

	public static void ShowError(Exception ex)
	{
		ShowError(ex, null);
	}

	public static void ShowError(string message)
	{
		ShowError(null, message);
	}

	public static void ShowErrorFormatted(string formatstring, params string[] formatitems)
	{
		ShowError(null, Format(formatstring, formatitems));
	}

	private static string GetStringFromFile(string fname)
	{
		string result = string.Empty;
		if (File.Exists(fname))
		{
			FileStream fileStream = new FileStream(fname, FileMode.Open);
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				result = EncryptionService.DecryptString((string)binaryFormatter.Deserialize(fileStream));
			}
			catch (CryptographicException)
			{
			}
			catch (SerializationException ex2)
			{
				MessageBox.Show("Failed to Read Log. Reason: " + ex2.Message);
			}
			finally
			{
				fileStream.Close();
			}
		}
		return result;
	}

	public static string GetFullExceptionMessageString(Exception ex, string caption, string message)
	{
		string text = "";
		Version version = Assembly.GetEntryAssembly().GetName().Version;
		text = text + "Clarion Version : " + version.ToString() + Environment.NewLine;
		text = text + ".NET Version         : " + Environment.Version.ToString() + Environment.NewLine;
		text = text + "OS Version           : " + Environment.OSVersion.ToString() + Environment.NewLine;
		string text2 = null;
		try
		{
			text2 = CultureInfo.CurrentCulture.Name;
			string text3 = text;
			text = text3 + "Current culture      : " + CultureInfo.CurrentCulture.EnglishName + " (" + text2 + ")" + Environment.NewLine;
		}
		catch
		{
		}
		try
		{
			if (text2 == null || !text2.StartsWith(ResourceService.Language, StringComparison.OrdinalIgnoreCase))
			{
				text = text + "Current UI language  : " + ResourceService.Language + Environment.NewLine;
			}
		}
		catch
		{
		}
		try
		{
			if (IntPtr.Size != 4)
			{
				object obj3 = text;
				text = string.Concat(obj3, "Running as ", IntPtr.Size * 8, " bit process", Environment.NewLine);
			}
			if (SystemInformation.TerminalServerSession)
			{
				text = text + "Terminal Server Session" + Environment.NewLine;
			}
			if (SystemInformation.BootMode != BootMode.Normal)
			{
				object obj4 = text;
				text = string.Concat(obj4, "Boot Mode            : ", SystemInformation.BootMode, Environment.NewLine);
			}
		}
		catch
		{
		}
		object obj6 = text;
		text = string.Concat(obj6, "Working Set Memory   : ", Environment.WorkingSet / 1024, "kb", Environment.NewLine);
		object obj7 = text;
		text = string.Concat(obj7, "GC Heap Memory       : ", GC.GetTotalMemory(forceFullCollection: false) / 1024, "kb", Environment.NewLine);
		text += Environment.NewLine;
		if (message != null)
		{
			string text4 = text;
			text = text4 + caption + Environment.NewLine + message + Environment.NewLine;
		}
		text += GetExceptionTypeMessage(ex);
		text = text + "Exception thrown: " + Environment.NewLine;
		return text + ex.ToString();
	}

	public static string GetFullCallStackInformation()
	{
		StackTrace stackTrace = new StackTrace();
		StackFrame[] frames = stackTrace.GetFrames();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(Environment.NewLine);
		stringBuilder.AppendLine("StackTrace:");
		StackFrame stackFrame = null;
		for (uint num = 0u; num < frames.Length - 1; num++)
		{
			stackFrame = frames[num];
			MethodBase method = stackFrame.GetMethod();
			if (method.DeclaringType.Namespace + "." + method.DeclaringType.Name != "ICSharpCode.Core.MessageService")
			{
				stringBuilder.AppendLine(method.DeclaringType.Namespace + "." + method.DeclaringType.Name + "." + method.Name);
			}
		}
		return stringBuilder.ToString();
	}

	public static void WriteLog(Exception ex, string caption, string message)
	{
		WriteLog(GetFullExceptionMessageString(ex, caption, message));
	}

	public static void WriteLog(string txt)
	{
		WriteLog(txt, addCallStack: false);
	}

	public static void WriteLog(string txt, bool addCallStack)
	{
		if (string.IsNullOrEmpty(txt))
		{
			return;
		}
		string text = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), $"Clarion_{DateTime.Today.Month}_{DateTime.Today.Day}_{DateTime.Today.Year}.XLOG");
		string text2 = GetStringFromFile(text);
		if (string.IsNullOrEmpty(text2))
		{
			text2 = "Version: " + "10.0.12463" + " " + VersionService.Version.ToString() + "\r\n" + "(c) 2001-2016 SoftVelocity";
		}
		FileStream fileStream = new FileStream(text, FileMode.Create);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		try
		{
			text2 = ((!string.IsNullOrEmpty(text2)) ? (text2 + "\r\n***************************************************\r\n" + DateTime.Today.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "\r\n***************************************************\r\n" + txt) : ("***************************************************\r\n" + DateTime.Today.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString() + "\r\n***************************************************\r\n" + txt));
			if (addCallStack)
			{
				text2 = string.Concat(text2, "\r\n" + GetFullCallStackInformation());
			}
			binaryFormatter.Serialize(fileStream, EncryptionService.EncryptString(text2));
		}
		catch (SerializationException ex)
		{
			MessageBox.Show("Failed to Store Log. Reason: " + ex.Message);
		}
		finally
		{
			fileStream.Close();
		}
	}

	public static void ShowError(Exception ex, string message)
	{
		if (message == null)
		{
			message = string.Empty;
		}
		if (ex != null)
		{
			LoggingService.Error(message, ex);
			if (customErrorReporter != null)
			{
				customErrorReporter(ex, message);
				return;
			}
		}
		else
		{
			LoggingService.Error(message);
		}
		string msg = message + Environment.NewLine;
		if (ex != null)
		{
			msg += GetExceptionTypeMessage(ex);
			msg = msg + Environment.NewLine + "Exception occurred: " + ex.ToString();
		}
		WriteLog(msg);
		if (QuietMode && ex != null)
		{
			return;
		}
		if (MainForm == null)
		{
			SaveCursorAndArrow();
			MessageBox.Show(StringParser.Parse(msg), StringParser.Parse(ProductName + " - ${res:Global.ErrorText}"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
			RestoreCursor();
			return;
		}
		MethodInvoker methodInvoker = delegate
		{
			string text = StringParser.Parse(msg);
			string caption = ProductName + " - " + StringParser.Parse("${res:Global.ErrorText}");
			try
			{
				SaveCursorAndArrow();
				MessageBox.Show(MainForm, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				RestoreCursor();
			}
			catch (Exception)
			{
				SaveCursorAndArrow();
				MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				RestoreCursor();
			}
		};
		if (MainForm.InvokeRequired)
		{
			MainForm.BeginInvoke(methodInvoker);
		}
		else
		{
			methodInvoker();
		}
	}

	public static void ShowWarning(string message)
	{
		message = StringParser.Parse(message);
		LoggingService.Warn(message);
		string caption = ProductName + " - " + StringParser.Parse("${res:Global.WarningText}");
		if (MainForm == null)
		{
			SaveCursorAndArrow();
			MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, GetOptions(message, caption));
			RestoreCursor();
			return;
		}
		MethodInvoker methodInvoker = delegate
		{
			SaveCursorAndArrow();
			MessageBox.Show(MainForm, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, GetOptions(message, caption));
			RestoreCursor();
		};
		if (MainForm.InvokeRequired)
		{
			MainForm.BeginInvoke(methodInvoker);
		}
		else
		{
			methodInvoker();
		}
	}

	public static void ShowWarningFormatted(string formatstring, params string[] formatitems)
	{
		ShowWarning(Format(formatstring, formatitems));
	}

	public static bool AskQuestion(string question, string caption)
	{
		return AskQuestion(question, caption, defaultToYes: true);
	}

	public static bool AskQuestion(string question, string caption, bool defaultToYes)
	{
		SaveCursorAndArrow();
		DialogResult dialogResult = MessageBox.Show(MainForm, StringParser.Parse(question), StringParser.Parse(caption), MessageBoxButtons.YesNo, MessageBoxIcon.Question, (!defaultToYes) ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1, GetOptions(question, caption));
		RestoreCursor();
		return dialogResult == DialogResult.Yes;
	}

	private static MessageBoxOptions GetOptions(string text, string caption)
	{
		if (!IsRtlText(text))
		{
			return (MessageBoxOptions)0;
		}
		return MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
	}

	private static bool IsRtlText(string text)
	{
		if (!RightToLeftConverter.IsRightToLeft)
		{
			return false;
		}
		string text2 = StringParser.Parse(text);
		foreach (char c in text2)
		{
			if (char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AskQuestionFormatted(string caption, string formatstring, params string[] formatitems)
	{
		return AskQuestion(Format(formatstring, formatitems), caption);
	}

	public static bool AskQuestionFormatted(string formatstring, params string[] formatitems)
	{
		return AskQuestion(Format(formatstring, formatitems));
	}

	public static bool AskQuestion(string question)
	{
		return AskQuestion(StringParser.Parse(question), StringParser.Parse("${res:Global.QuestionText}"));
	}

	public static int ShowCustomDialog(string caption, string dialogText, int acceptButtonIndex, int cancelButtonIndex, params string[] buttontexts)
	{
		using CustomDialog customDialog = new CustomDialog(caption, dialogText, acceptButtonIndex, cancelButtonIndex, buttontexts);
		SaveCursorAndArrow();
		customDialog.ShowDialog(MainForm);
		RestoreCursor();
		return customDialog.Result;
	}

	public static int ShowCustomDialog(string caption, string dialogText, params string[] buttontexts)
	{
		return ShowCustomDialog(caption, dialogText, -1, -1, buttontexts);
	}

	public static string ShowInputBox(string caption, string dialogText, string defaultValue)
	{
		using InputBox inputBox = new InputBox(dialogText, caption, defaultValue);
		SaveCursorAndArrow();
		inputBox.ShowDialog(MainForm);
		RestoreCursor();
		return inputBox.Result;
	}

	public static void ShowMessage(string message)
	{
		ShowMessage(message, DefaultMessageBoxTitle);
	}

	public static void ShowMessageFormatted(string formatstring, params string[] formatitems)
	{
		ShowMessage(Format(formatstring, formatitems));
	}

	public static void ShowMessageFormatted(string caption, string formatstring, params string[] formatitems)
	{
		ShowMessage(Format(formatstring, formatitems), caption);
	}

	private static void DoShowMessage(string message, string caption)
	{
		DoShowMessage(null, message, caption);
	}

	public static string GetExceptionTypeMessage(Exception ex)
	{
		return GetExceptionTypeMessage(ex, 0);
	}

	private static string GetExceptionTypeMessage(Exception exP, int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Exception ex = exP;
		if (ex != null)
		{
			stringBuilder.AppendLine("");
			stringBuilder.Append("Exception Type: ");
			stringBuilder.AppendLine(ex.GetType().ToString());
			if (ex is TargetInvocationException || ex is TargetException || ex is TargetParameterCountException)
			{
				stringBuilder.AppendLine("An error happen on the action executed.");
				stringBuilder.AppendLine(ex.Message);
				if (ex.InnerException != null)
				{
					stringBuilder.AppendLine("");
					stringBuilder.Append("The InnerException Type: ");
					ex = ex.InnerException;
					stringBuilder.AppendLine(ex.GetType().ToString());
				}
			}
			if (ex is DirectoryNotFoundException)
			{
				DirectoryNotFoundException ex2 = (DirectoryNotFoundException)ex;
				stringBuilder.AppendLine("Directory Not Found.");
				stringBuilder.AppendLine(ex2.Message);
			}
			else if (ex is FileNotFoundException)
			{
				FileNotFoundException ex3 = (FileNotFoundException)ex;
				stringBuilder.Append("File Not Found: ");
				stringBuilder.AppendLine(ex3.FileName);
				stringBuilder.Append("Error Message: ");
				stringBuilder.AppendLine(ex3.Message);
			}
			else if (ex is FileLoadException)
			{
				FileLoadException ex4 = (FileLoadException)ex;
				stringBuilder.Append("Name of file that cound't load: ");
				stringBuilder.AppendLine(ex4.FileName);
				stringBuilder.Append("Error Message: ");
				stringBuilder.AppendLine(ex4.Message);
			}
			else if (ex.InnerException != null)
			{
				if (typeof(FileNotFoundException).IsAssignableFrom(ex.InnerException.GetType()))
				{
					FileNotFoundException ex5 = (FileNotFoundException)ex.InnerException;
					stringBuilder.Append("Name of file that cound't load: ");
					stringBuilder.AppendLine(ex5.FileName);
					stringBuilder.Append("Error Message: ");
					stringBuilder.AppendLine(ex5.Message);
					if (ex5.FileName.Contains("Microsoft.Build.Framework") || ex5.Message.Contains("Microsoft.Build.Framework"))
					{
						stringBuilder.AppendLine("MSBuild is not installed in this computer.");
						stringBuilder.AppendLine("Try re-installing the required Full .Net");
					}
					stringBuilder.Append("File Not Found: ");
					stringBuilder.AppendLine(ex5.FileName);
					stringBuilder.Append("Error Message: ");
					stringBuilder.AppendLine(ex5.Message);
				}
				else if (typeof(FileLoadException).IsAssignableFrom(ex.InnerException.GetType()))
				{
					FileLoadException ex6 = (FileLoadException)ex.InnerException;
					if (ex6.FileName.Contains("Microsoft.Build.Framework") || ex6.Message.Contains("Microsoft.Build.Framework"))
					{
						stringBuilder.AppendLine("MSBuild is not installed in this computer.");
						stringBuilder.AppendLine("Try re-installing the required Full .Net");
					}
					stringBuilder.Append("Name of file that cound't load: ");
					stringBuilder.AppendLine(ex6.FileName);
					stringBuilder.Append("Error Message: ");
					stringBuilder.AppendLine(ex6.Message);
				}
				else if (count == 1)
				{
					stringBuilder.AppendLine("");
					stringBuilder.AppendLine($"The InnerException ({count}) Information:");
					stringBuilder.Append("The InnerException Type: ");
					stringBuilder.AppendLine(ex.InnerException.GetType().ToString());
					stringBuilder.AppendLine("[...]");
				}
				else
				{
					stringBuilder.AppendLine("");
					stringBuilder.AppendLine($"The InnerException ({count + 1}) Information:");
					stringBuilder.AppendLine(GetExceptionTypeMessage(ex.InnerException, count + 1));
				}
			}
			stringBuilder.AppendLine("");
		}
		return stringBuilder.ToString();
	}

	private static void DoShowMessage(Exception ex, string message, string caption)
	{
		caption = StringParser.Parse(caption);
		message = StringParser.Parse(message);
		LoggingService.Info(message);
		if (ex != null)
		{
			message = message + Environment.NewLine + GetExceptionTypeMessage(ex);
			string txt = caption + Environment.NewLine + message + Environment.NewLine + "Exception occurred: " + ex.ToString();
			WriteLog(txt);
			message = message + Environment.NewLine + "***************************************************" + Environment.NewLine + "Please report the problem to Softvelocity and attach the xlog file.";
			if (!QuietMode)
			{
				message = (message = message + Environment.NewLine + "Exception occurred: " + ex.ToString());
			}
		}
		SaveCursorAndArrow();
		MessageBox.Show(mainForm, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, GetOptions(message, caption));
		RestoreCursor();
	}

	public static void ShowMessage(string message, string caption)
	{
		ShowMessage(null, message, caption);
	}

	public static void ShowMessage(Exception ex, string message, string caption)
	{
		if (mainForm != null && mainForm.InvokeRequired)
		{
			mainForm.Invoke(new ShowMessageDelegate(DoShowMessage), ex, message, caption);
		}
		else
		{
			DoShowMessage(ex, message, caption);
		}
	}

	private static string Format(string formatstring, string[] formatitems)
	{
		try
		{
			return string.Format(StringParser.Parse(formatstring), formatitems);
		}
		catch (FormatException)
		{
			StringBuilder stringBuilder = new StringBuilder(StringParser.Parse(formatstring));
			foreach (string value in formatitems)
			{
				stringBuilder.Append("\nItem: ");
				stringBuilder.Append(value);
			}
			return stringBuilder.ToString();
		}
	}
}
