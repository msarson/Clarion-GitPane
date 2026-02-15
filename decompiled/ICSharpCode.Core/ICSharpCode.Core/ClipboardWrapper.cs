using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public static class ClipboardWrapper
{
	[ThreadStatic]
	private static int SafeSetClipboardDataVersion;

	public static bool ContainsText
	{
		get
		{
			try
			{
				LoggingService.Debug("ContainsText called");
				return Clipboard.ContainsText();
			}
			catch (ExternalException)
			{
				return false;
			}
		}
	}

	public static string GetText()
	{
		try
		{
			return Clipboard.GetText();
		}
		catch (ExternalException)
		{
			return Clipboard.GetText();
		}
	}

	public static void SetText(string text)
	{
		DataObject dataObject = new DataObject();
		dataObject.SetData(DataFormats.UnicodeText, autoConvert: true, text);
		SetDataObject(dataObject);
	}

	public static IDataObject GetDataObject()
	{
		try
		{
			return Clipboard.GetDataObject();
		}
		catch (ExternalException)
		{
			try
			{
				return Clipboard.GetDataObject();
			}
			catch (ExternalException)
			{
				return new DataObject();
			}
		}
	}

	public static void SetDataObject(object data)
	{
		SafeSetClipboard(data);
	}

	private static void SafeSetClipboard(object dataObject)
	{
		int version = ++SafeSetClipboardDataVersion;
		try
		{
			Clipboard.SetDataObject(dataObject, copy: true);
		}
		catch (ExternalException)
		{
			Timer timer = new Timer();
			timer.Interval = 100;
			timer.Tick += delegate
			{
				timer.Stop();
				timer.Dispose();
				if (SafeSetClipboardDataVersion == version)
				{
					try
					{
						Clipboard.SetDataObject(dataObject, copy: true, 10, 50);
					}
					catch (ExternalException)
					{
					}
				}
			};
			timer.Start();
		}
	}
}
