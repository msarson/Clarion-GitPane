using ICSharpCode.Core;
using SoftVelocity.Generator.Editor;

namespace SoftVelocity.Generator;

public static class GeneratorEditorsService
{
	private static bool initialized;

	private static GenEditorDescriptor[] editors = null;

	internal static void Initialize()
	{
		if (!initialized)
		{
			editors = (GenEditorDescriptor[])AddInTree.GetTreeNode("/Clarion/GeneratorEditors").BuildChildItems((object)null).ToArray(typeof(GenEditorDescriptor));
			initialized = true;
		}
	}

	public static CommonGenEditor CreateEditor(string language)
	{
		Initialize();
		GenEditorDescriptor[] array = editors;
		foreach (GenEditorDescriptor genEditorDescriptor in array)
		{
			if (genEditorDescriptor.CanAttachToLanguage(language))
			{
				return genEditorDescriptor.CreateEditor();
			}
		}
		return null;
	}
}
