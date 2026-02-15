using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Common;

public class DynaInvoke
{
	private class DynaClassInfo
	{
		public Type type;

		public object ClassObject;

		public DynaClassInfo()
		{
		}

		public DynaClassInfo(Type t, object c)
		{
			type = t;
			ClassObject = c;
		}
	}

	private static Dictionary<string, Assembly> AssemblyReferences = new Dictionary<string, Assembly>();

	private static Dictionary<string, DynaClassInfo> ClassReferences = new Dictionary<string, DynaClassInfo>();

	private static void splitClassName(string ClassNameAndMethod, ref string ClassName, ref string MethodName)
	{
		ClassName = "";
		MethodName = "";
		string[] array = ClassNameAndMethod.Split('.');
		if (array.Length > 0)
		{
			if (array.Length > 1)
			{
				MethodName = array[array.Length - 1];
			}
			ClassName = array[0];
			for (int i = 1; i < array.Length - 1; i++)
			{
				ClassName = ClassName + "." + array[i];
			}
		}
	}

	private static DynaClassInfo GetClassReference(string AssemblyName, string ClassName, bool isStatic)
	{
		string text = AssemblyName + "." + ClassName;
		if (!ClassReferences.ContainsKey(text))
		{
			Assembly assembly = null;
			if (!AssemblyReferences.ContainsKey(AssemblyName))
			{
				if (File.Exists(AssemblyName))
				{
					assembly = Assembly.LoadFrom(AssemblyName);
					AssemblyReferences.Add(AssemblyName, assembly);
				}
				else
				{
					string fileName = Path.GetFileName(AssemblyName);
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					foreach (Assembly assembly2 in assemblies)
					{
						if (!assembly2.IsDynamic)
						{
							string fileName2 = Path.GetFileName(assembly2.Location);
							if (string.Equals(fileName2, fileName, StringComparison.OrdinalIgnoreCase))
							{
								assembly = assembly2;
							}
						}
					}
					if (assembly != null)
					{
						AssemblyReferences.Add(AssemblyName, assembly);
					}
				}
			}
			else
			{
				assembly = AssemblyReferences[AssemblyName];
			}
			Type type = assembly.GetType(ClassName);
			if (type != null)
			{
				DynaClassInfo dynaClassInfo = null;
				dynaClassInfo = ((!isStatic) ? new DynaClassInfo(type, Activator.CreateInstance(type)) : new DynaClassInfo(type, null));
				ClassReferences.Add(text, dynaClassInfo);
				return dynaClassInfo;
			}
			throw new Exception("could not instantiate class " + text);
		}
		return ClassReferences[text];
	}

	private static object InvokeMethod(DynaClassInfo ci, string MethodName, params object[] args)
	{
		return ci.type.InvokeMember(MethodName, BindingFlags.InvokeMethod, null, ci.ClassObject, args);
	}

	public static object InvokeMethod(string AssemblyName, string ClassName, string MethodName, params object[] args)
	{
		return InvokeMethod(AssemblyName, ClassName, MethodName, isStatic: false, args);
	}

	public static object InvokeMethodStatic(string AssemblyName, string ClassName, string MethodName, params object[] args)
	{
		return InvokeMethod(AssemblyName, ClassName, MethodName, isStatic: true, args);
	}

	public static object InvokeMethod(string AssemblyName, string ClassNameAndMethod, params object[] args)
	{
		return InvokeMethod(AssemblyName, ClassNameAndMethod, isStatic: false, args);
	}

	public static object InvokeMethodStatic(string AssemblyName, string ClassNameAndMethod, params object[] args)
	{
		return InvokeMethod(AssemblyName, ClassNameAndMethod, isStatic: true, args);
	}

	public static object SafeThreadCallMethodStatic(string AssemblyName, string ClassNameAndMethod, params object[] args)
	{
		if (WorkbenchSingleton.InvokeRequired)
		{
			return WorkbenchSingleton.SafeThreadFunction<string, string, object[], object>((Func<string, string, object[], object>)SafeThreadCallMethodStatic, AssemblyName, ClassNameAndMethod, args);
		}
		return InvokeMethod(AssemblyName, ClassNameAndMethod, isStatic: true, args);
	}

	private static object InvokeMethod(string AssemblyName, string ClassNameAndMethod, bool isStatic, params object[] args)
	{
		if (ClassNameAndMethod != null)
		{
			string ClassName = null;
			string MethodName = null;
			splitClassName(ClassNameAndMethod, ref ClassName, ref MethodName);
			return InvokeMethod(AssemblyName, ClassName, MethodName, isStatic, args);
		}
		return null;
	}

	private static object InvokeMethod(string AssemblyName, string ClassName, string MethodName, bool isStatic, params object[] args)
	{
		DynaClassInfo classReference = GetClassReference(AssemblyName, ClassName, isStatic);
		return InvokeMethod(classReference, MethodName, args);
	}
}
