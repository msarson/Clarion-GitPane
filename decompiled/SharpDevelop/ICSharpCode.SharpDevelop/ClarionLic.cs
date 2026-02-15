using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.Core;
using SeriousBit.Licensing;
using SoftVelocity.Licensing;

namespace ICSharpCode.SharpDevelop;

public class ClarionLic
{
	private delegate int[] Del();

	internal const string propNameString = "Name";

	internal const string __propSerialString = "Serial";

	internal const string __propClarionSharpSerialString = "ClarionSharpSerial";

	internal const string invalidSerialText = "The Serial number used to activate Clarion is invalid or expired";

	private static Properties _prop = null;

	internal static int remainingDays = -1;

	private static string _propSerialString;

	private static string _propClarionSharpSerialString;

	private static int _SerialMajor = 0;

	private static int _SerialMinor = 0;

	private static EventWaitHandle locker;

	private static Properties prop
	{
		get
		{
			if (_prop == null)
			{
				_prop = PropertyService.Get("SoftVelocity.Lic", new Properties());
				if (!_prop.Contains("Name"))
				{
					_prop.Set("Name", "");
					_prop.Set(propSerialString, "");
					_prop.Set(propClarionSharpSerialString, "");
				}
			}
			return _prop;
		}
	}

	internal static string BasicName => prop.Get("Name", "").ToString().Trim();

	internal static string Name
	{
		get
		{
			if (remainingDays > -1)
			{
				return $"{BasicName} - Remaining days: {remainingDays.ToString()}";
			}
			return BasicName;
		}
	}

	private static string SerialW => prop.Get(propSerialString, "").ToString().Trim();

	private static string SerialS => prop.Get(propClarionSharpSerialString, "").ToString().Trim();

	internal static string propSerialString
	{
		get
		{
			if (_propSerialString == null)
			{
				if (SerialMajor > 9 || (SerialMajor == 9 && SerialMinor > 0))
				{
					_propSerialString = "Serial_" + SerialString;
				}
				else
				{
					_propSerialString = "Serial";
				}
			}
			return _propSerialString;
		}
	}

	internal static string propClarionSharpSerialString
	{
		get
		{
			if (_propClarionSharpSerialString == null)
			{
				if (SerialMajor > 9 || (SerialMajor == 9 && SerialMinor > 0))
				{
					_propClarionSharpSerialString = "ClarionSharpSerial_" + SerialString;
				}
				else
				{
					_propClarionSharpSerialString = "ClarionSharpSerial";
				}
			}
			return _propClarionSharpSerialString;
		}
	}

	private static int SerialMajor
	{
		get
		{
			if (_SerialMajor == 0)
			{
				_SerialMajor = Assembly.GetEntryAssembly().GetName().Version.Major;
			}
			return _SerialMajor;
		}
	}

	private static int SerialMinor
	{
		get
		{
			if (_SerialMinor == 0)
			{
				_SerialMinor = Assembly.GetEntryAssembly().GetName().Version.Minor;
			}
			return _SerialMinor;
		}
	}

	private static decimal Serial => Convert.ToDecimal(SerialString, new CultureInfo("en-US", useUserOverride: false));

	private static string SerialString => GetSerialString(SerialMajor, SerialMinor);

	public static string GetSerialString(int pSerialMajor, int pSerialMinor)
	{
		if (pSerialMinor == 0)
		{
			return pSerialMajor.ToString();
		}
		return $"{pSerialMajor.ToString()}.{pSerialMinor.ToString()}";
	}

	private static void InitV(out bool win32, out bool dotNet)
	{
		win32 = false;
		dotNet = false;
		foreach (AddIn addIn in AddInTree.AddIns)
		{
			if (addIn.Name.Equals("ClarionWindowsBinding"))
			{
				win32 = true;
			}
			else if (addIn.Name.Equals("ClarionNetBinding"))
			{
				dotNet = true;
			}
			if (win32 && dotNet)
			{
				break;
			}
		}
	}

	public static bool IsValid(bool askForSerial)
	{
		bool flag = false;
		bool momo = false;
		if (locker == null)
		{
			locker = new EventWaitHandle(initialState: true, EventResetMode.AutoReset, "_ClarionIdeLicenseLocker");
		}
		locker.WaitOne();
		try
		{
			bool win = false;
			bool dotNet = false;
			InitV(out win, out dotNet);
			if (!dotNet && !win)
			{
				return true;
			}
			string developerKey = License.DeveloperKey;
			string developerName = License.DeveloperName;
			SerialsManager manager = new SerialsManager(developerName, developerKey);
			manager.PublicKey = License.PublicKey;
			Del del = delegate
			{
				List<int> list = new List<int>();
				string[] privateIds = License.PrivateIds;
				if (privateIds != null)
				{
					string[] privateIds2 = License.PrivateIds;
					foreach (string serial in privateIds2)
					{
						list.Add(manager.GetID(serial));
					}
				}
				return list.ToArray();
			};
			manager.BlackList = del();
			if (askForSerial)
			{
				if (dotNet && string.IsNullOrEmpty(SerialS) && !string.IsNullOrEmpty(SerialW) && !IsW(manager, SerialW, out momo))
				{
					prop.Set(propClarionSharpSerialString, SerialW);
					prop.Remove(propSerialString);
					flag = true;
				}
				if (win)
				{
					bool flag2 = IsValidDate(manager, SerialW);
					if (string.IsNullOrEmpty(SerialW) || !flag2)
					{
						bool flag3 = true;
						if (!flag2 && !MessageService.AskQuestion(string.Format("Do you want to register your Clarion For Windows ?", MessageService.ProductName), "Product Activation"))
						{
							flag3 = false;
						}
						if (flag3)
						{
							using (ClarionLicForm clarionLicForm = new ClarionLicForm(manager, prop, win32: true))
							{
								if (clarionLicForm.ShowDialog() != DialogResult.OK)
								{
									prop.Set("Name", "The Serial number used to activate Clarion is invalid or expired");
									prop.Set(propSerialString, "The Serial number used to activate Clarion is invalid or expired");
									flag = true;
									return false;
								}
							}
							flag = true;
						}
					}
				}
				if (dotNet)
				{
					bool flag4 = IsValidDate(manager, SerialS);
					if (string.IsNullOrEmpty(SerialS) || !flag4)
					{
						bool flag5 = true;
						if (!flag4 && !MessageService.AskQuestion(string.Format("Do you want to register your Clarion# ?", MessageService.ProductName), "Product Activation"))
						{
							flag5 = false;
						}
						if (flag5)
						{
							using (ClarionLicForm clarionLicForm2 = new ClarionLicForm(manager, prop, win32: false))
							{
								if (clarionLicForm2.ShowDialog() != DialogResult.OK)
								{
									prop.Set("Name", "The Serial number used to activate Clarion is invalid or expired");
									prop.Set(propClarionSharpSerialString, "The Serial number used to activate Clarion is invalid or expired");
									flag = true;
									return false;
								}
							}
							flag = true;
						}
					}
				}
			}
			string name = Name;
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			string serialW = SerialW;
			string serialS = SerialS;
			if (win && string.IsNullOrEmpty(serialW))
			{
				return false;
			}
			if (dotNet && string.IsNullOrEmpty(serialS))
			{
				return false;
			}
			bool flag6 = true;
			if (win && !IsValid(manager, name, serialW, win32: true, out momo))
			{
				flag6 = false;
				prop.Set(propSerialString, "The Serial number used to activate Clarion is invalid or expired");
				flag = true;
			}
			bool flag7 = true;
			if (dotNet && !IsValid(manager, name, serialS, win32: false, out momo))
			{
				flag7 = false;
				prop.Set(propClarionSharpSerialString, "The Serial number used to activate Clarion is invalid or expired");
				flag = true;
			}
			if (!flag7 || !flag6)
			{
				prop.Set("Name", "The Serial number used to activate Clarion is invalid or expired");
				flag = true;
			}
			if (!flag7 || !flag6)
			{
				MessageService.ShowMessage("The Serial number used to activate Clarion is invalid or expired", MessageService.ProductName + " - Product Activation");
			}
			return flag7 && flag6;
		}
		catch (Exception ex)
		{
			try
			{
				if (!License.InfoExist())
				{
					MessageService.WriteLog("The ClarionFL.DLL was not found.");
				}
				if (string.IsNullOrEmpty(License.DeveloperKey))
				{
					MessageService.WriteLog("The licensing library information was corrupted.(K)");
				}
				if (string.IsNullOrEmpty(License.DeveloperName))
				{
					MessageService.WriteLog("The licensing library information was corrupted.(N)");
				}
			}
			catch (Exception ex2)
			{
				MessageService.ShowError(ex2, "Error accesing licensing library information.");
			}
			if (!License.InfoExist())
			{
				MessageService.WriteLog(ex, "There is a problem accessing some files in the installation directory.\nCheck for some files marked as read-only and or access rights to the directory files for the logged in user.", MessageService.ProductName + " - Product Validation");
				MessageService.ShowMessage("The installation is corrupt.\nIt is possible that some files are missing.", MessageService.ProductName + " - Product Validation");
			}
			else
			{
				MessageService.WriteLog(ex, "There is a problem accessing some files in the installation directory.\nCheck for some files marked as read-only and or access rights to the directory files for the logged in user.", MessageService.ProductName + " - Product Validation");
				MessageService.ShowError(ex, "There is a problem accessing some files in the installation directory.\nCheck for some files marked as read-only and or access rights to the directory files for the logged in user.");
			}
			return false;
		}
		finally
		{
			if (flag && !momo)
			{
				PropertyService.Save();
			}
			locker.Set();
		}
	}

	private static bool IsValidVersion(string productVersionFullText, int copyNumber, bool isWindow, out bool momo)
	{
		momo = false;
		if (string.IsNullOrEmpty(productVersionFullText))
		{
			return false;
		}
		string[] array = ((!productVersionFullText.Contains("|")) ? new string[1] { productVersionFullText } : productVersionFullText.Split(new char[1] { '|' }, 2));
		if (array != null && array.Length > 0)
		{
			decimal num = 7m;
			string text = array[0];
			if (array.Length > 1)
			{
				try
				{
					num = Convert.ToDecimal(array[1], new CultureInfo("en-US", useUserOverride: false));
				}
				catch
				{
				}
			}
			if (num == Serial)
			{
				if (isWindow)
				{
					switch (text)
					{
					case "EE":
					case "PE":
						return true;
					case "SE":
					{
						momo = true;
						if (copyNumber < 1 || copyNumber > 99)
						{
							copyNumber = 1;
						}
						string text2 = copyNumber.ToString("D2");
						TimeBomb timeBomb = new TimeBomb(365, new Guid("{2CFF8569-3D13-4" + text2 + "4-83BE-07FC868D841F}"));
						try
						{
							timeBomb.Update();
						}
						catch (SecurityException)
						{
							MessageService.WriteLog("The IDE can't access the registry to verify the trial validity.", addCallStack: false);
							MessageService.ShowMessage("The IDE can't verify the trial validity. There was a problem on the product installation.", MessageService.ProductName + " - Product Validation");
							return false;
						}
						if (timeBomb.IsExpired || timeBomb.IsHacked || !timeBomb.IsTrialActive)
						{
							remainingDays = 0;
							return false;
						}
						remainingDays = timeBomb.DaysLeft;
						return true;
					}
					default:
						return false;
					}
				}
				if (text == "EE.Net" || text == "PE.Net")
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	private static bool IsValid(SerialsManager manager, string name, string serial, bool win32, out bool momo)
	{
		momo = false;
		if (manager.IsValid(serial) && IsValidDate(manager, serial))
		{
			string empty = string.Empty;
			int num = 0;
			string empty2 = string.Empty;
			string info = manager.GetInfo(serial);
			string[] array = info.Split(new char[1] { ':' }, 3);
			empty = array[0];
			num = Convert.ToInt32(array[1]);
			empty2 = array[2];
			if (name.Trim().ToUpper() == empty2.Trim().ToUpper() && IsValidVersion(empty, num, win32, out momo))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsW(SerialsManager manager, string serial, out bool momo)
	{
		momo = false;
		if (manager.IsValid(serial))
		{
			int num = 0;
			string empty = string.Empty;
			string info = manager.GetInfo(serial);
			string[] array = info.Split(new char[1] { ':' }, 3);
			empty = array[0];
			num = Convert.ToInt32(array[1]);
			if (IsValidVersion(empty, num, isWindow: true, out momo))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsValidDate(SerialsManager manager, string serial)
	{
		if (manager.IsValid(serial))
		{
			bool flag = manager.HasExpirationDate(serial);
			DateTime expirationDate = manager.GetExpirationDate(serial);
			if (!flag || (flag && expirationDate > DateTime.Today))
			{
				return true;
			}
		}
		return false;
	}
}
