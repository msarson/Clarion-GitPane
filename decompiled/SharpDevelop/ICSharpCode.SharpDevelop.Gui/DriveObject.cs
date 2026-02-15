using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class DriveObject
{
	private class NativeMethods
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int GetVolumeInformation(string volumePath, StringBuilder volumeNameBuffer, int volNameBuffSize, ref int volumeSerNr, ref int maxComponentLength, ref int fileSystemFlags, StringBuilder fileSystemNameBuffer, int fileSysBuffSize);

		[DllImport("kernel32.dll")]
		public static extern DriveType GetDriveType(string driveName);
	}

	private string text;

	private string drive;

	public string Drive => drive;

	public static string VolumeLabel(string volumePath)
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			int volumeSerNr = 0;
			NativeMethods.GetVolumeInformation(volumePath, stringBuilder, 128, ref volumeSerNr, ref volumeSerNr, ref volumeSerNr, null, 0);
			return stringBuilder.ToString();
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public static DriveType GetDriveType(string driveName)
	{
		return NativeMethods.GetDriveType(driveName);
	}

	public static Image GetImageForFile(string fileName)
	{
		return IconService.GetBitmap(IconService.GetImageForFile(fileName));
	}

	public DriveObject(string drive)
	{
		this.drive = drive;
		text = drive.Substring(0, 2);
		switch (GetDriveType(drive))
		{
		case DriveType.Removeable:
			text += " (${res:MainWindow.Windows.FileScout.DriveType.Removeable})";
			break;
		case DriveType.Fixed:
			text += " (${res:MainWindow.Windows.FileScout.DriveType.Fixed})";
			break;
		case DriveType.Cdrom:
			text += " (${res:MainWindow.Windows.FileScout.DriveType.CD})";
			break;
		case DriveType.Remote:
			text += " (${res:MainWindow.Windows.FileScout.DriveType.Remote})";
			break;
		}
		text = StringParser.Parse(text);
	}

	public override string ToString()
	{
		return text;
	}
}
