using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace SoftVelocity.Generator.UI;

public static class GenEditorsBindingService
{
	private static GenEditorBindingDescriptor[] editorBindings;

	private static GenEditorBindingDescriptor[] reportBindings;

	private static GenEditorBindingDescriptor[] windowBindings;

	internal static void GetEditorBindings()
	{
		if (editorBindings == null)
		{
			editorBindings = (GenEditorBindingDescriptor[])AddInTree.GetTreeNode("/Clarion/GenEditorsBindings/TextEditor").BuildChildItems((object)null).ToArray(typeof(GenEditorBindingDescriptor));
		}
	}

	internal static void GetReportBindings()
	{
		if (reportBindings == null)
		{
			reportBindings = (GenEditorBindingDescriptor[])AddInTree.GetTreeNode("/Clarion/GenEditorsBindings/Report").BuildChildItems((object)null).ToArray(typeof(GenEditorBindingDescriptor));
		}
	}

	internal static void GetWindowBindings()
	{
		if (windowBindings == null)
		{
			windowBindings = (GenEditorBindingDescriptor[])AddInTree.GetTreeNode("/Clarion/GenEditorsBindings/Window").BuildChildItems((object)null).ToArray(typeof(GenEditorBindingDescriptor));
		}
	}

	public static IViewContent GetTextEditor(string language)
	{
		GetEditorBindings();
		if (editorBindings == null)
		{
			return null;
		}
		GenEditorBindingDescriptor[] array = editorBindings;
		foreach (GenEditorBindingDescriptor genEditorBindingDescriptor in array)
		{
			if (language.Equals(genEditorBindingDescriptor.Language, StringComparison.InvariantCultureIgnoreCase))
			{
				return genEditorBindingDescriptor.Binding;
			}
		}
		return null;
	}

	public static IViewContent GetReportEditor(string language)
	{
		GetReportBindings();
		if (reportBindings == null)
		{
			return null;
		}
		GenEditorBindingDescriptor[] array = reportBindings;
		foreach (GenEditorBindingDescriptor genEditorBindingDescriptor in array)
		{
			if (language.Equals(genEditorBindingDescriptor.Language, StringComparison.InvariantCultureIgnoreCase))
			{
				return genEditorBindingDescriptor.Binding;
			}
		}
		return null;
	}

	public static IViewContent GetWindowEditor(string language)
	{
		GetWindowBindings();
		if (windowBindings == null)
		{
			return null;
		}
		GenEditorBindingDescriptor[] array = windowBindings;
		foreach (GenEditorBindingDescriptor genEditorBindingDescriptor in array)
		{
			if (language.Equals(genEditorBindingDescriptor.Language, StringComparison.InvariantCultureIgnoreCase))
			{
				return genEditorBindingDescriptor.Binding;
			}
		}
		return null;
	}
}
