using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.OptionPanels;

public class ReferencePaths : AbstractProjectOptionPanel
{
	public class SemicolonSeparatedStringListBinding : ConfigurationGuiBinding
	{
		private StringListEditor editor;

		public SemicolonSeparatedStringListBinding(StringListEditor editor)
		{
			this.editor = editor;
		}

		public override void Load()
		{
			string[] array = Get("").Split(';');
			if (array.Length == 1 && array[0].Length == 0)
			{
				editor.LoadList(new string[0]);
			}
			else
			{
				editor.LoadList(array);
			}
		}

		public override bool Save()
		{
			Set(string.Join(";", editor.GetList()));
			return true;
		}
	}

	public override void LoadPanelContents()
	{
		InitializeHelper();
		StringListEditor stringListEditor = new StringListEditor();
		stringListEditor.BrowseForDirectory = true;
		stringListEditor.ListCaption = StringParser.Parse("&${res:Dialog.ProjectOptions.ReferencePaths}:");
		stringListEditor.TitleText = StringParser.Parse("&${res:Dialog.ExportProjectToHtml.FolderLabel}");
		stringListEditor.AddButtonText = StringParser.Parse("${res:Dialog.ProjectOptions.ReferencePaths.AddPath}");
		stringListEditor.ListChanged += delegate
		{
			base.IsDirty = true;
		};
		SemicolonSeparatedStringListBinding semicolonSeparatedStringListBinding = new SemicolonSeparatedStringListBinding(stringListEditor);
		helper.AddBinding("ReferencePath", semicolonSeparatedStringListBinding);
		base.Controls.Add(stringListEditor);
		semicolonSeparatedStringListBinding.CreateLocationButton(stringListEditor);
		helper.AddConfigurationSelector(this);
	}
}
