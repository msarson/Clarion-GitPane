using System;
using System.Collections;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Internal.Templates;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class TemplateCompletionDataProvider : AbstractCompletionDataProvider
{
	private class TemplateCompletionData : ICompletionData, IComparable
	{
		private CodeTemplate template;

		public int ImageIndex => 0;

		public string Text
		{
			get
			{
				return template.Shortcut + "\t" + template.Description;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public string Description => template.Text;

		public double Priority => 0.0;

		public bool InsertAction(TextArea textArea, char ch)
		{
			((SharpDevelopTextAreaControl)textArea.MotherTextEditorControl).InsertTemplate(template);
			return false;
		}

		public TemplateCompletionData(CodeTemplate template)
		{
			this.template = template;
		}

		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is TemplateCompletionData))
			{
				return -1;
			}
			return template.Shortcut.CompareTo(((TemplateCompletionData)obj).template.Shortcut);
		}
	}

	private ImageList imageList = new ImageList();

	public override ImageList ImageList => imageList;

	public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
	{
		preSelection = "";
		imageList.Images.Add(IconService.GetBitmap("Icons.16x16.TextFileIcon"));
		CodeTemplateGroup templateGroupPerFilename = CodeTemplateLoader.GetTemplateGroupPerFilename(fileName);
		if (templateGroupPerFilename == null)
		{
			return null;
		}
		ArrayList arrayList = new ArrayList();
		foreach (CodeTemplate template in templateGroupPerFilename.Templates)
		{
			arrayList.Add(new TemplateCompletionData(template));
		}
		return (ICompletionData[])arrayList.ToArray(typeof(ICompletionData));
	}
}
