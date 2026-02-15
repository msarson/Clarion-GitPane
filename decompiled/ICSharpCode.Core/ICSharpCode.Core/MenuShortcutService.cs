using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ICSharpCode.Core;

[Serializable]
public class MenuShortcutService
{
	public class CommandShortcut
	{
		private string codonId = "";

		private string originalShortcutKeys;

		private string newShortcutKeys;

		private string text;

		private Bitmap image;

		public string CodonId
		{
			get
			{
				return codonId;
			}
			set
			{
				codonId = value;
			}
		}

		public string OriginalShortcutKeys
		{
			get
			{
				return originalShortcutKeys;
			}
			set
			{
				originalShortcutKeys = value;
			}
		}

		public string NewShortcutKeys
		{
			get
			{
				return newShortcutKeys;
			}
			set
			{
				newShortcutKeys = value;
			}
		}

		[XmlIgnore]
		public string Text
		{
			get
			{
				if (string.IsNullOrEmpty(text))
				{
					return CodonId;
				}
				return text;
			}
		}

		[XmlIgnore]
		public Bitmap Image => image;

		public CommandShortcut()
		{
		}

		public CommandShortcut(string codonId, string originalShortcutKeysText, string newShortcutKeysText)
		{
			this.codonId = codonId.Trim();
			newShortcutKeys = newShortcutKeysText;
			originalShortcutKeys = originalShortcutKeysText;
		}

		public void SetTextAndImage(string description, Bitmap menuImage)
		{
			if (string.IsNullOrEmpty(description))
			{
				text = CodonId;
			}
			else
			{
				text = description;
			}
			image = menuImage;
		}
	}

	private const string propertyFileName = "MenuShorcuts.xml";

	private const string propertyUseFullName = "MenuShorcuts_UseFullName";

	private static Dictionary<string, CommandShortcut> commands = new Dictionary<string, CommandShortcut>();

	private static bool _dirty = false;

	private static bool inited = false;

	private static bool _UseFullName = false;

	private static bool autosave = true;

	private static bool Dirty
	{
		get
		{
			return _dirty;
		}
		set
		{
			_dirty = value;
			if (_dirty && autosave)
			{
				Save();
			}
		}
	}

	public static bool UseFullName
	{
		get
		{
			return _UseFullName;
		}
		set
		{
			if (_UseFullName != value)
			{
				_UseFullName = value;
				PropertyService.Set("MenuShorcuts_UseFullName", _UseFullName);
				Restore();
			}
		}
	}

	public static List<CommandShortcut> GetCommandsCopy()
	{
		List<CommandShortcut> list = new List<CommandShortcut>();
		list.AddRange(commands.Values);
		return list;
	}

	public static void SetCommandsValues(CommandShortcut[] value)
	{
		commands.Clear();
		foreach (CommandShortcut commandShortcut in value)
		{
			if (!commands.ContainsKey(commandShortcut.CodonId))
			{
				commands[commandShortcut.CodonId] = new CommandShortcut(commandShortcut.CodonId, commandShortcut.OriginalShortcutKeys, commandShortcut.NewShortcutKeys);
			}
		}
		Dirty = true;
	}

	public static void Initialize()
	{
		if (!inited)
		{
			inited = true;
			_UseFullName = PropertyService.Get("MenuShorcuts_UseFullName", _UseFullName);
		}
	}

	public static void SuspendAutosave()
	{
		autosave = false;
	}

	public static void RestoreAutosave()
	{
		autosave = true;
	}

	public static void Restore()
	{
		SuspendAutosave();
		commands.Clear();
		AddInTreeNode treeNode = AddInTree.GetTreeNode(null, throwOnNotFound: false);
		RestoreCommandsList(treeNode);
		RestoreAutosave();
		Save();
	}

	private static void RestoreCommandsList(AddInTreeNode addinNode)
	{
		if (addinNode == null)
		{
			return;
		}
		string text = null;
		foreach (Codon codon in addinNode.Codons)
		{
			if (codon.Properties.Contains("type"))
			{
				text = codon.Properties["type"];
			}
			if (!codon.Name.Equals("MenuItem", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if (codon.Properties.Contains("type"))
			{
				switch (text)
				{
				case "Item":
				case "Command":
				case "CheckBox":
					break;
				default:
					continue;
				}
			}
			string empty = string.Empty;
			if (codon.Properties.Contains("shortcut"))
			{
				empty = codon.Properties["shortcut"];
				AddCommand(codon.ShortcutId, empty);
			}
		}
		foreach (AddInTreeNode value in addinNode.ChildNodes.Values)
		{
			RestoreCommandsList(value);
		}
	}

	public static void Save()
	{
		if (!Directory.Exists(PropertyService.ConfigDirectory))
		{
			Directory.CreateDirectory(PropertyService.ConfigDirectory);
		}
		bool flag = File.Exists(Path.Combine(PropertyService.ConfigDirectory, "MenuShorcuts.xml"));
		if ((flag && Dirty) || !flag)
		{
			List<CommandShortcut> serializableObject = new List<CommandShortcut>(commands.Values);
			ObjectXMLSerializer<List<CommandShortcut>>.Save(serializableObject, Path.Combine(PropertyService.ConfigDirectory, "MenuShorcuts.xml"));
		}
		Dirty = false;
	}

	public static void Load()
	{
		Dirty = false;
		if (!File.Exists(Path.Combine(PropertyService.ConfigDirectory, "MenuShorcuts.xml")))
		{
			return;
		}
		commands.Clear();
		try
		{
			List<CommandShortcut> list = ObjectXMLSerializer<List<CommandShortcut>>.Load(Path.Combine(PropertyService.ConfigDirectory, "MenuShorcuts.xml"));
			foreach (CommandShortcut item in list)
			{
				commands.Add(item.CodonId, item);
			}
		}
		catch
		{
		}
	}

	public static void RemoveCommand(string codonId)
	{
		if (commands.ContainsKey(codonId))
		{
			commands.Remove(codonId);
			Dirty = true;
		}
	}

	private static void AddCommand(string codonId, string originalShortcutKeysText)
	{
		if (!commands.ContainsKey(codonId))
		{
			commands[codonId] = new CommandShortcut(codonId, originalShortcutKeysText, "");
			Dirty = true;
		}
	}

	public static void ChangeCommandShortcut(string codonId, string newShortcutKeysText)
	{
		if (commands.ContainsKey(codonId) && commands[codonId].NewShortcutKeys != newShortcutKeysText)
		{
			commands[codonId].NewShortcutKeys = newShortcutKeysText;
			Dirty = true;
		}
	}

	public static string GetShortcut(string codonId, string originalShortcutKeysText)
	{
		string shortcut = GetShortcut(codonId);
		if (string.IsNullOrEmpty(shortcut))
		{
			AddCommand(codonId, originalShortcutKeysText);
			return originalShortcutKeysText;
		}
		if (commands[codonId].OriginalShortcutKeys != originalShortcutKeysText)
		{
			commands[codonId].OriginalShortcutKeys = originalShortcutKeysText;
			Dirty = true;
		}
		return shortcut;
	}

	public static bool SetShortcutTextAndImage(string codonId, string text, Bitmap image)
	{
		if (commands.ContainsKey(codonId))
		{
			commands[codonId].SetTextAndImage(text, image);
			return true;
		}
		return false;
	}

	public static string GetShortcut(string codonId)
	{
		if (commands.ContainsKey(codonId))
		{
			if (!string.IsNullOrEmpty(commands[codonId].NewShortcutKeys))
			{
				return commands[codonId].NewShortcutKeys;
			}
			return commands[codonId].OriginalShortcutKeys;
		}
		return null;
	}

	public static Keys GetShortcutKey(string codonId)
	{
		string shortcut = GetShortcut(codonId);
		if (!string.IsNullOrEmpty(shortcut))
		{
			return ParseShortcut(shortcut);
		}
		return Keys.None;
	}

	public static Keys ParseShortcut(string shortcutString)
	{
		Keys keys = Keys.None;
		if (!string.IsNullOrEmpty(shortcutString) && shortcutString.Trim().Length > 0)
		{
			try
			{
				shortcutString = shortcutString.Trim();
				string[] array = shortcutString.Split('|');
				foreach (string value in array)
				{
					keys |= (Keys)Enum.Parse(typeof(Keys), value);
				}
			}
			catch (Exception ex)
			{
				MessageService.ShowError(ex);
				return Keys.None;
			}
		}
		return keys;
	}

	public static string[] GetCommandsName()
	{
		List<string> list = new List<string>(commands.Keys);
		return list.ToArray();
	}

	public static void ValidateCommands(List<string> currentcodons)
	{
		if (commands.Count <= 0)
		{
			return;
		}
		SuspendAutosave();
		List<string> list = new List<string>(commands.Keys);
		string text = null;
		for (int i = 0; i < list.Count; i++)
		{
			text = list[i];
			if (!currentcodons.Contains(text))
			{
				commands.Remove(text);
				Dirty = true;
			}
		}
		RestoreAutosave();
		if (Dirty)
		{
			Save();
		}
	}
}
