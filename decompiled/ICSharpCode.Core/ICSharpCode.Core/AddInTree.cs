using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace ICSharpCode.Core;

public static class AddInTree
{
	private static List<AddIn> addIns;

	private static AddInTreeNode rootNode;

	private static Dictionary<string, IDoozer> doozers;

	private static Dictionary<string, IConditionEvaluator> conditionEvaluators;

	public static IList<AddIn> AddIns => addIns.AsReadOnly();

	public static Dictionary<string, IDoozer> Doozers => doozers;

	public static Dictionary<string, IConditionEvaluator> ConditionEvaluators => conditionEvaluators;

	static AddInTree()
	{
		addIns = new List<AddIn>();
		rootNode = new AddInTreeNode();
		doozers = new Dictionary<string, IDoozer>();
		conditionEvaluators = new Dictionary<string, IConditionEvaluator>();
		doozers.Add("Class", new ClassDoozer());
		doozers.Add("FileFilter", new FileFilterDoozer());
		doozers.Add("String", new StringDoozer());
		doozers.Add("Icon", new IconDoozer());
		doozers.Add("MenuItem", new MenuItemDoozer());
		doozers.Add("ToolbarItem", new ToolbarItemDoozer());
		doozers.Add("Include", new IncludeDoozer());
		conditionEvaluators.Add("Compare", new CompareConditionEvaluator());
		conditionEvaluators.Add("Ownerstate", new OwnerStateConditionEvaluator());
	}

	public static bool ExistAddin(string addinName)
	{
		foreach (AddIn addIn in AddIns)
		{
			if (addIn.Name.Equals(addinName, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ExistsTreeNode(string path)
	{
		if (path == null || path.Length == 0)
		{
			return true;
		}
		string[] array = path.Split('/');
		AddInTreeNode addInTreeNode = rootNode;
		for (int i = 0; i < array.Length; i++)
		{
			if (!addInTreeNode.ChildNodes.ContainsKey(array[i]))
			{
				return false;
			}
			addInTreeNode = addInTreeNode.ChildNodes[array[i]];
		}
		return true;
	}

	public static AddInTreeNode GetTreeNode(string path)
	{
		return GetTreeNode(path, throwOnNotFound: true);
	}

	public static AddInTreeNode GetTreeNode(string path, bool throwOnNotFound)
	{
		if (path == null || path.Length == 0)
		{
			return rootNode;
		}
		string[] array = path.Split('/');
		AddInTreeNode addInTreeNode = rootNode;
		for (int i = 0; i < array.Length; i++)
		{
			if (!addInTreeNode.ChildNodes.ContainsKey(array[i]))
			{
				if (throwOnNotFound)
				{
					throw new TreePathNotFoundException(path);
				}
				return null;
			}
			addInTreeNode = addInTreeNode.ChildNodes[array[i]];
		}
		return addInTreeNode;
	}

	public static object BuildItem(string path, object caller)
	{
		int num = path.LastIndexOf('/');
		string path2 = path.Substring(0, num);
		string childItemID = path.Substring(num + 1);
		AddInTreeNode treeNode = GetTreeNode(path2);
		return treeNode.BuildChildItem(childItemID, caller, BuildItems(path, caller, throwOnNotFound: false));
	}

	public static ArrayList BuildItems(string path, object caller, bool throwOnNotFound)
	{
		AddInTreeNode treeNode = GetTreeNode(path, throwOnNotFound);
		if (treeNode == null)
		{
			return new ArrayList();
		}
		return treeNode.BuildChildItems(caller);
	}

	public static List<T> BuildItems<T>(string path, object caller)
	{
		return BuildItems<T>(path, caller, throwOnNotFound: true);
	}

	public static List<T> BuildItems<T>(string path, object caller, bool throwOnNotFound)
	{
		AddInTreeNode treeNode = GetTreeNode(path, throwOnNotFound);
		if (treeNode == null)
		{
			return new List<T>();
		}
		return treeNode.BuildChildItems<T>(caller);
	}

	private static AddInTreeNode CreatePath(AddInTreeNode localRoot, string path)
	{
		if (path == null || path.Length == 0)
		{
			return localRoot;
		}
		string[] array = path.Split('/');
		AddInTreeNode addInTreeNode = localRoot;
		for (int i = 0; i < array.Length; i++)
		{
			if (!addInTreeNode.ChildNodes.ContainsKey(array[i]))
			{
				addInTreeNode.ChildNodes[array[i]] = new AddInTreeNode();
			}
			addInTreeNode = addInTreeNode.ChildNodes[array[i]];
		}
		return addInTreeNode;
	}

	private static void AddExtensionPath(ExtensionPath path)
	{
		AddInTreeNode addInTreeNode = CreatePath(rootNode, path.Name);
		foreach (Codon codon in path.Codons)
		{
			addInTreeNode.Codons.Add(codon);
		}
	}

	public static void InsertAddIn(AddIn addIn)
	{
		if (addIn.Enabled)
		{
			foreach (ExtensionPath value in addIn.Paths.Values)
			{
				AddExtensionPath(value);
			}
			foreach (Runtime runtime in addIn.Runtimes)
			{
				if (!runtime.IsActive)
				{
					continue;
				}
				foreach (LazyLoadDoozer definedDoozer in runtime.DefinedDoozers)
				{
					if (Doozers.ContainsKey(definedDoozer.Name))
					{
						throw new AddInLoadException("Duplicate doozer: " + definedDoozer.Name);
					}
					Doozers.Add(definedDoozer.Name, definedDoozer);
				}
				foreach (LazyConditionEvaluator definedConditionEvaluator in runtime.DefinedConditionEvaluators)
				{
					if (ConditionEvaluators.ContainsKey(definedConditionEvaluator.Name))
					{
						throw new AddInLoadException("Duplicate condition evaluator: " + definedConditionEvaluator.Name);
					}
					ConditionEvaluators.Add(definedConditionEvaluator.Name, definedConditionEvaluator);
				}
			}
			string directoryName = Path.GetDirectoryName(addIn.FileName);
			foreach (string bitmapResource in addIn.BitmapResources)
			{
				string path = Path.Combine(directoryName, bitmapResource);
				ResourceManager imageManager = ResourceManager.CreateFileBasedResourceManager(Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path), null);
				ResourceService.RegisterNeutralImages(imageManager);
			}
			foreach (string stringResource in addIn.StringResources)
			{
				string path2 = Path.Combine(directoryName, stringResource);
				ResourceManager stringManager = ResourceManager.CreateFileBasedResourceManager(Path.GetFileNameWithoutExtension(path2), Path.GetDirectoryName(path2), null);
				ResourceService.RegisterNeutralStrings(stringManager);
			}
		}
		addIns.Add(addIn);
	}

	public static void RemoveAddIn(AddIn addIn)
	{
		if (addIn.Enabled)
		{
			throw new ArgumentException("Cannot remove enabled AddIns at runtime.");
		}
		addIns.Remove(addIn);
	}

	private static void DisableAddin(AddIn addIn, Dictionary<string, Version> dict, Dictionary<string, AddIn> addInDict)
	{
		addIn.Enabled = false;
		addIn.Action = AddInAction.DependencyError;
		foreach (string key in addIn.Manifest.Identities.Keys)
		{
			dict.Remove(key);
			addInDict.Remove(key);
		}
	}

	public static void Load(List<string> addInFiles, List<string> disabledAddIns)
	{
		List<AddIn> list = new List<AddIn>();
		Dictionary<string, Version> dictionary = new Dictionary<string, Version>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, AddIn> dictionary2 = new Dictionary<string, AddIn>(StringComparer.OrdinalIgnoreCase);
		foreach (string addInFile in addInFiles)
		{
			AddIn addIn;
			try
			{
				addIn = AddIn.Load(addInFile);
			}
			catch (AddInLoadException ex)
			{
				LoggingService.Error(ex);
				if (ex.InnerException != null)
				{
					MessageService.ShowError("Error loading AddIn " + addInFile + ":\n" + ex.InnerException.Message);
				}
				else
				{
					MessageService.ShowError("Error loading AddIn " + addInFile + ":\n" + ex.Message);
				}
				addIn = new AddIn();
				addIn.CustomErrorMessage = ex.Message;
			}
			if (addIn.Action == AddInAction.CustomError)
			{
				list.Add(addIn);
				continue;
			}
			addIn.Enabled = true;
			if (disabledAddIns != null && disabledAddIns.Count > 0)
			{
				foreach (string key in addIn.Manifest.Identities.Keys)
				{
					foreach (string disabledAddIn in disabledAddIns)
					{
						if (disabledAddIn.Equals(key, StringComparison.OrdinalIgnoreCase))
						{
							addIn.Enabled = false;
							break;
						}
					}
				}
			}
			if (addIn.Enabled)
			{
				foreach (KeyValuePair<string, Version> identity in addIn.Manifest.Identities)
				{
					if (dictionary.ContainsKey(identity.Key))
					{
						if (!dictionary2[identity.Key].FileName.Equals(addInFile, StringComparison.OrdinalIgnoreCase))
						{
							MessageService.ShowError("The Identity name '" + identity.Key + "' is used by multiple addins:\r\n'" + dictionary2[identity.Key].FileName + "'\r\n and \r\n'" + addInFile + "'\r\n\r\nIdentity names must be unique.\r\nTo avoid seeing this error message remove one of the above listed addins.");
							addIn.Enabled = false;
							addIn.Action = AddInAction.InstalledTwice;
							break;
						}
					}
					else
					{
						dictionary.Add(identity.Key, identity.Value);
						dictionary2.Add(identity.Key, addIn);
					}
				}
			}
			list.Add(addIn);
		}
		while (true)
		{
			int num = 0;
			while (true)
			{
				if (num < list.Count)
				{
					AddIn addIn2 = list[num];
					if (addIn2.Enabled)
					{
						Version versionFound;
						foreach (AddInReference conflict in addIn2.Manifest.Conflicts)
						{
							if (conflict.Check(dictionary, out versionFound))
							{
								MessageService.ShowError(addIn2.Name + " conflicts with " + conflict.ToString() + " and has been disabled.");
								DisableAddin(addIn2, dictionary, dictionary2);
								goto end_IL_03eb;
							}
						}
						foreach (AddInReference dependency in addIn2.Manifest.Dependencies)
						{
							if (!dependency.Check(dictionary, out versionFound))
							{
								if (versionFound != null)
								{
									MessageService.ShowError(addIn2.Name + " has not been loaded because it requires " + dependency.ToString() + ", but version " + versionFound.ToString() + " is installed.");
								}
								else
								{
									MessageService.ShowError(addIn2.Name + " has not been loaded because it requires " + dependency.ToString() + ".");
								}
								DisableAddin(addIn2, dictionary, dictionary2);
								goto end_IL_03eb;
							}
						}
					}
					num++;
					continue;
				}
				foreach (AddIn item in list)
				{
					InsertAddIn(item);
				}
				return;
				continue;
				end_IL_03eb:
				break;
			}
		}
	}
}
