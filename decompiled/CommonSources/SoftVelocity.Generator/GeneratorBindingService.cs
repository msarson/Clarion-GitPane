using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator;

public static class GeneratorBindingService
{
	private static bool initialized;

	private static GeneratorBindingDescriptor[] bindings = null;

	internal static void Initialize()
	{
		if (!initialized)
		{
			bindings = (GeneratorBindingDescriptor[])AddInTree.GetTreeNode("/Clarion/GeneratorBindings").BuildChildItems((object)null).ToArray(typeof(GeneratorBindingDescriptor));
			initialized = true;
		}
	}

	public static IGeneratorBinding GetBinding(string language)
	{
		Initialize();
		GeneratorBindingDescriptor[] array = bindings;
		foreach (GeneratorBindingDescriptor generatorBindingDescriptor in array)
		{
			if (generatorBindingDescriptor.CanAttachToLanguage(language))
			{
				return generatorBindingDescriptor.Binding;
			}
		}
		return null;
	}

	public static IGeneratorBinding GetBinding()
	{
		Initialize();
		if (bindings.Length > 0)
		{
			return bindings[0].Binding;
		}
		return null;
	}

	public static IAppViewContentEvents GetCurrentIAppViewContentEvents()
	{
		IWorkbenchWindow activeWorkbenchWindow = WorkbenchSingleton.Workbench.ActiveWorkbenchWindow;
		if (activeWorkbenchWindow == null)
		{
			return null;
		}
		if (!(activeWorkbenchWindow.ActiveViewContent is IAppViewContentEvents result))
		{
			return null;
		}
		return result;
	}
}
