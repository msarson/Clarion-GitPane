using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class AttributesDataProvider : CtrlSpaceCompletionDataProvider
{
	private bool removeAttributeSuffix = true;

	public bool RemoveAttributeSuffix
	{
		get
		{
			return removeAttributeSuffix;
		}
		set
		{
			removeAttributeSuffix = value;
		}
	}

	public AttributesDataProvider(IProjectContent pc)
		: this(ExpressionContext.TypeDerivingFrom(pc.GetClass("System.Attribute"), isObjectCreation: true))
	{
	}

	public AttributesDataProvider(ExpressionContext context)
		: base(context)
	{
		base.ForceNewExpression = true;
	}

	public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
	{
		ICompletionData[] array = base.GenerateCompletionData(fileName, textArea, charTyped);
		if (removeAttributeSuffix)
		{
			ICompletionData[] array2 = array;
			foreach (ICompletionData completionData in array2)
			{
				if (completionData.Text.EndsWith("Attribute"))
				{
					completionData.Text = completionData.Text.Substring(0, completionData.Text.Length - 9);
				}
			}
		}
		return array;
	}
}
