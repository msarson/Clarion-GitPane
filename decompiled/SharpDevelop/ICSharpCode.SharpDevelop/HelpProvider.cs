using System;
using System.Collections;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop;

public class HelpProvider
{
	public static ArrayList GetProviders()
	{
		return AddInTree.BuildItems("/SharpDevelop/Services/HelpProvider", null, throwOnNotFound: false);
	}

	public static void ShowHelp(IClass c)
	{
		foreach (HelpProvider provider in GetProviders())
		{
			if (provider.TryShowHelp(c))
			{
				return;
			}
		}
		new HelpProvider().TryShowHelp(c);
	}

	public virtual bool TryShowHelp(IClass c)
	{
		return TryShowHelp(c.FullyQualifiedName);
	}

	public static void ShowHelp(IMember m)
	{
		foreach (HelpProvider provider in GetProviders())
		{
			if (provider.TryShowHelp(m))
			{
				return;
			}
		}
		new HelpProvider().TryShowHelp(m);
	}

	public virtual bool TryShowHelp(IMember m)
	{
		return TryShowHelp(m.FullyQualifiedName);
	}

	public static void ShowHelp(string fullTypeName)
	{
		foreach (HelpProvider provider in GetProviders())
		{
			if (provider.TryShowHelp(fullTypeName))
			{
				return;
			}
		}
		new HelpProvider().TryShowHelp(fullTypeName);
	}

	public virtual bool TryShowHelp(string fullTypeName)
	{
		FileService.OpenFile("http://msdn2.microsoft.com/library/" + Uri.EscapeDataString(fullTypeName));
		return true;
	}

	public static void ShowHelpByKeyword(string keyword)
	{
		foreach (HelpProvider provider in GetProviders())
		{
			if (provider.TryShowHelpByKeyword(keyword))
			{
				break;
			}
		}
	}

	public virtual bool TryShowHelpByKeyword(string keyword)
	{
		return false;
	}
}
