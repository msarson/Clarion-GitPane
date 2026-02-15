using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop;

public static class DisplayBindingService
{
	private static readonly string displayBindingPath;

	private static DisplayBindingDescriptor[] bindings;

	public static IDisplayBinding GetBindingPerFileName(string filename)
	{
		return GetCodonPerFileName(filename)?.Binding;
	}

	public static IDisplayBinding GetBindingPerLanguageName(string languagename)
	{
		return GetCodonPerLanguageName(languagename)?.Binding;
	}

	private static DisplayBindingDescriptor GetCodonPerFileName(string filename)
	{
		DisplayBindingDescriptor[] array = bindings;
		foreach (DisplayBindingDescriptor displayBindingDescriptor in array)
		{
			if (!displayBindingDescriptor.IsSecondary && displayBindingDescriptor.CanAttachToFile(filename) && displayBindingDescriptor.Binding != null && displayBindingDescriptor.Binding.CanCreateContentForFile(filename))
			{
				return displayBindingDescriptor;
			}
		}
		return null;
	}

	private static DisplayBindingDescriptor GetCodonPerLanguageName(string languagename)
	{
		DisplayBindingDescriptor[] array = bindings;
		foreach (DisplayBindingDescriptor displayBindingDescriptor in array)
		{
			if (!displayBindingDescriptor.IsSecondary && displayBindingDescriptor.CanAttachToLanguage(languagename) && displayBindingDescriptor.Binding != null && displayBindingDescriptor.Binding.CanCreateContentForLanguage(languagename))
			{
				return displayBindingDescriptor;
			}
		}
		return null;
	}

	public static void AttachSubWindows(IViewContent viewContent, bool isReattaching)
	{
		DisplayBindingDescriptor[] array = bindings;
		foreach (DisplayBindingDescriptor displayBindingDescriptor in array)
		{
			if (!displayBindingDescriptor.IsSecondary || !displayBindingDescriptor.CanAttachToFile(viewContent.FileName ?? viewContent.UntitledName))
			{
				continue;
			}
			ISecondaryDisplayBinding secondaryBinding = displayBindingDescriptor.SecondaryBinding;
			if (secondaryBinding != null && (!isReattaching || secondaryBinding.ReattachWhenParserServiceIsReady) && secondaryBinding.CanAttachTo(viewContent))
			{
				ISecondaryViewContent[] array2 = displayBindingDescriptor.SecondaryBinding.CreateSecondaryViewContent(viewContent);
				if (array2 != null)
				{
					viewContent.SecondaryViewContents.AddRange(array2);
					continue;
				}
				MessageService.ShowError(string.Concat("Can't attach secondary view content. ", displayBindingDescriptor.SecondaryBinding, " returned null for ", viewContent, ".\n(should never happen)"));
			}
		}
	}

	static DisplayBindingService()
	{
		displayBindingPath = "/SharpDevelop/Workbench/DisplayBindings";
		bindings = null;
		bindings = (DisplayBindingDescriptor[])AddInTree.GetTreeNode(displayBindingPath).BuildChildItems(null).ToArray(typeof(DisplayBindingDescriptor));
	}
}
