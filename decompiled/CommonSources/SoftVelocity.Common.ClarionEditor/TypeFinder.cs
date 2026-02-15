using System;
using System.Reflection;

namespace SoftVelocity.Common.ClarionEditor;

public static class TypeFinder
{
	public static Type FindType(string assemblyName, string typeFullName)
	{
		Type result = null;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.GetName().Name == assemblyName)
			{
				result = assembly.GetType(typeFullName);
				break;
			}
		}
		return result;
	}
}
