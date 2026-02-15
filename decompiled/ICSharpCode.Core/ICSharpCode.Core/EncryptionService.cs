using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace ICSharpCode.Core;

public class EncryptionService
{
	private static byte[] _Key;

	private static byte[] Key
	{
		get
		{
			if (_Key == null)
			{
				_Key = Assembly.GetEntryAssembly().GetName().GetPublicKey();
			}
			return _Key;
		}
	}

	[DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
	public static extern bool ZeroMemory(ref string Destination, int Length);

	private static string GenerateKey()
	{
		DESCryptoServiceProvider dESCryptoServiceProvider = (DESCryptoServiceProvider)DES.Create();
		return Encoding.ASCII.GetString(dESCryptoServiceProvider.Key);
	}

	private static byte[] EncryptString(byte[] clearText, byte[] Key, byte[] IV)
	{
		MemoryStream memoryStream = new MemoryStream();
		Rijndael rijndael = Rijndael.Create();
		rijndael.Key = Key;
		rijndael.IV = IV;
		CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
		cryptoStream.Write(clearText, 0, clearText.Length);
		cryptoStream.Close();
		return memoryStream.ToArray();
	}

	public static string EncryptString(string clearText, string Password)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(clearText);
		PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, new byte[13]
		{
			73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
			100, 101, 118
		});
		byte[] inArray = EncryptString(bytes, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
		return Convert.ToBase64String(inArray);
	}

	public static string EncryptString(string clearText)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(clearText);
		PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Key, new byte[13]
		{
			73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
			100, 101, 118
		});
		byte[] inArray = EncryptString(bytes, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
		return Convert.ToBase64String(inArray);
	}

	private static byte[] DecryptString(byte[] cipherData, byte[] Key, byte[] IV)
	{
		MemoryStream memoryStream = new MemoryStream();
		Rijndael rijndael = Rijndael.Create();
		rijndael.Key = Key;
		rijndael.IV = IV;
		CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
		cryptoStream.Write(cipherData, 0, cipherData.Length);
		cryptoStream.Close();
		return memoryStream.ToArray();
	}

	public static string DecryptString(string cipherText, string Password)
	{
		byte[] cipherData = Convert.FromBase64String(cipherText);
		PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, new byte[13]
		{
			73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
			100, 101, 118
		});
		byte[] bytes = DecryptString(cipherData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
		return Encoding.Unicode.GetString(bytes);
	}

	public static string DecryptString(string cipherText)
	{
		byte[] cipherData = Convert.FromBase64String(cipherText);
		PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Key, new byte[13]
		{
			73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
			100, 101, 118
		});
		byte[] bytes = DecryptString(cipherData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
		return Encoding.Unicode.GetString(bytes);
	}
}
