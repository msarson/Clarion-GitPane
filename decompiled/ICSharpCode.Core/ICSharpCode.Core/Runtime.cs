using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace ICSharpCode.Core;

public class Runtime
{
	private string hintPath;

	private string assembly;

	private Assembly loadedAssembly;

	private IList<LazyLoadDoozer> definedDoozers = new List<LazyLoadDoozer>();

	private IList<LazyConditionEvaluator> definedConditionEvaluators = new List<LazyConditionEvaluator>();

	private ICondition[] conditions;

	private bool isActive = true;

	private bool isAssemblyLoaded;

	public bool IsActive
	{
		get
		{
			if (conditions != null)
			{
				isActive = Condition.GetFailedAction(conditions, this) == ConditionFailedAction.Nothing;
				conditions = null;
			}
			return isActive;
		}
	}

	public string Assembly => assembly;

	public Assembly LoadedAssembly
	{
		get
		{
			if (!isAssemblyLoaded)
			{
				LoggingService.Info("Loading addin " + assembly);
				isAssemblyLoaded = true;
				try
				{
					if (assembly[0] == ':')
					{
						loadedAssembly = System.Reflection.Assembly.Load(assembly.Substring(1));
					}
					else if (assembly[0] == '$')
					{
						int num = assembly.IndexOf('/');
						if (num < 0)
						{
							throw new ApplicationException("Expected '/' in path beginning with '$'!");
						}
						string text = assembly.Substring(1, num - 1);
						foreach (AddIn addIn in AddInTree.AddIns)
						{
							if (addIn.Enabled && addIn.Manifest.Identities.ContainsKey(text))
							{
								string assemblyFile = Path.Combine(Path.GetDirectoryName(addIn.FileName), assembly.Substring(num + 1));
								loadedAssembly = System.Reflection.Assembly.LoadFrom(assemblyFile);
								break;
							}
						}
						if (loadedAssembly == null)
						{
							throw new FileNotFoundException("Could not find referenced AddIn " + text);
						}
					}
					else
					{
						loadedAssembly = System.Reflection.Assembly.LoadFrom(Path.Combine(hintPath, assembly));
					}
				}
				catch (FileNotFoundException ex)
				{
					MessageService.ShowError("The addin '" + assembly + "' could not be loaded:\n" + ex.ToString());
				}
				catch (BadImageFormatException ex2)
				{
					MessageService.ShowError("The addin '" + assembly + "' could not be loaded:\n" + ex2.ToString());
				}
				catch (FileLoadException ex3)
				{
					MessageService.ShowError("The addin '" + assembly + "' could not be loaded:\n" + ex3.ToString());
				}
			}
			return loadedAssembly;
		}
	}

	public IList<LazyLoadDoozer> DefinedDoozers => definedDoozers;

	public IList<LazyConditionEvaluator> DefinedConditionEvaluators => definedConditionEvaluators;

	public Runtime(string assembly, string hintPath)
	{
		this.assembly = assembly;
		this.hintPath = hintPath;
	}

	public object CreateInstance(string instance)
	{
		if (IsActive)
		{
			Assembly assembly = LoadedAssembly;
			if (assembly == null)
			{
				return null;
			}
			return assembly.CreateInstance(instance);
		}
		return null;
	}

	internal static void ReadSection(XmlReader reader, AddIn addIn, string hintPath)
	{
		Stack<ICondition> stack = new Stack<ICondition>();
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.EndElement:
				if (reader.LocalName == "Condition" || reader.LocalName == "ComplexCondition")
				{
					stack.Pop();
				}
				else if (reader.LocalName == "Runtime")
				{
					return;
				}
				break;
			case XmlNodeType.Element:
				switch (reader.LocalName)
				{
				case "Condition":
					stack.Push(Condition.Read(reader));
					break;
				case "ComplexCondition":
					stack.Push(Condition.ReadComplexCondition(reader));
					break;
				case "Import":
					addIn.Runtimes.Add(Read(addIn, reader, hintPath, stack));
					break;
				case "DisableAddIn":
					if (Condition.GetFailedAction(stack, addIn) == ConditionFailedAction.Nothing)
					{
						addIn.CustomErrorMessage = reader.GetAttribute("message");
					}
					break;
				default:
					throw new AddInLoadException("Unknown node in runtime section :" + reader.LocalName);
				}
				break;
			}
		}
	}

	internal static Runtime Read(AddIn addIn, XmlReader reader, string hintPath, Stack<ICondition> conditionStack)
	{
		if (reader.AttributeCount != 1)
		{
			throw new AddInLoadException("Import node requires ONE attribute.");
		}
		Runtime runtime = new Runtime(reader.GetAttribute(0), hintPath);
		if (conditionStack.Count > 0)
		{
			runtime.conditions = conditionStack.ToArray();
		}
		if (!reader.IsEmptyElement)
		{
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
				case XmlNodeType.EndElement:
					if (reader.LocalName == "Import")
					{
						return runtime;
					}
					break;
				case XmlNodeType.Element:
				{
					string localName = reader.LocalName;
					Properties properties = Properties.ReadFromAttributes(reader);
					switch (localName)
					{
					case "Doozer":
						if (!reader.IsEmptyElement)
						{
							throw new AddInLoadException("Doozer nodes must be empty!");
						}
						runtime.definedDoozers.Add(new LazyLoadDoozer(addIn, properties));
						break;
					case "ConditionEvaluator":
						if (!reader.IsEmptyElement)
						{
							throw new AddInLoadException("ConditionEvaluator nodes must be empty!");
						}
						runtime.definedConditionEvaluators.Add(new LazyConditionEvaluator(addIn, properties));
						break;
					default:
						throw new AddInLoadException("Unknown node in Import section:" + localName);
					}
					break;
				}
				}
			}
		}
		runtime.definedDoozers = (runtime.definedDoozers as List<LazyLoadDoozer>).AsReadOnly();
		runtime.definedConditionEvaluators = (runtime.definedConditionEvaluators as List<LazyConditionEvaluator>).AsReadOnly();
		return runtime;
	}
}
