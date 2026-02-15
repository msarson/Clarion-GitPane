using System;
using System.Reflection;

namespace ICSharpCode.Core;

public class AppDomainLaunchHelper : MarshalByRefObject
{
	public object LaunchMethod(string assemblyFile, string typeName, string methodName, object[] arguments)
	{
		Type type = Assembly.LoadFrom(assemblyFile).GetType(typeName);
		return type.InvokeMember(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, arguments);
	}

	public static object LaunchInAppDomain(AppDomain domain, Type type, string methodName, params object[] arguments)
	{
		AppDomainLaunchHelper appDomainLaunchHelper = (AppDomainLaunchHelper)domain.CreateInstanceFromAndUnwrap(typeof(AppDomainLaunchHelper).Assembly.Location, typeof(AppDomainLaunchHelper).FullName);
		return appDomainLaunchHelper.LaunchMethod(type.Assembly.Location, type.FullName, methodName, arguments);
	}
}
