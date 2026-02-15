using System;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using SoftVelocity.Common.ClarionEditor;

namespace SoftVelocity.Common.CodeCompletion;

public abstract class ClaAbstractCodeCompletionDataProvider : CodeCompletionDataProvider
{
	protected ClarionCommonTextAreaControl ta;

	protected bool mergeOverriddenMethods;

	protected ClaAbstractCodeCompletionDataProvider(bool mergeOverriddenMethods)
	{
		this.mergeOverriddenMethods = mergeOverriddenMethods;
	}

	protected ClaAbstractCodeCompletionDataProvider(bool mergeOverriddenMethods, ExpressionResult expression)
		: base(expression)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		this.mergeOverriddenMethods = mergeOverriddenMethods;
	}

	public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
	{
		ta = textArea.MotherTextEditorControl as ClarionCommonTextAreaControl;
		return ((AbstractCodeCompletionDataProvider)this).GenerateCompletionData(fileName, textArea, charTyped);
	}

	protected override CodeCompletionData CreateItem(object o, ExpressionContext context)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		if (o is string)
		{
			return (CodeCompletionData)(object)new ClaCodeCompletionData(ta, o.ToString(), 3);
		}
		if (o is IClass)
		{
			return (CodeCompletionData)(object)new ClaCodeCompletionData(ta, (IClass)o);
		}
		if (o is IProperty)
		{
			IProperty val = (IProperty)o;
			if (((IMember)val).Name != null && ((AbstractCodeCompletionDataProvider)this).insertedPropertiesElements[((IMember)val).Name] == null)
			{
				((AbstractCodeCompletionDataProvider)this).insertedPropertiesElements[((IMember)val).Name] = val;
				return (CodeCompletionData)(object)new ClaCodeCompletionData(ta, val);
			}
		}
		else if (o is IMethod)
		{
			IMethod val2 = (IMethod)o;
			if (((IMember)val2).Name != null)
			{
				ClaCodeCompletionData claCodeCompletionData = new ClaCodeCompletionData(ta, val2);
				if (!mergeOverriddenMethods || ((AbstractCodeCompletionDataProvider)this).insertedElements[((IMember)val2).Name] == null)
				{
					((AbstractCodeCompletionDataProvider)this).insertedElements[((IMember)val2).Name] = claCodeCompletionData;
					return (CodeCompletionData)(object)claCodeCompletionData;
				}
				ClaCodeCompletionData claCodeCompletionData2 = (ClaCodeCompletionData)((AbstractCodeCompletionDataProvider)this).insertedElements[((IMember)val2).Name];
				((CodeCompletionData)claCodeCompletionData2).Overloads = ((CodeCompletionData)claCodeCompletionData2).Overloads + 1;
			}
		}
		else
		{
			if (o is IField)
			{
				return (CodeCompletionData)(object)new ClaCodeCompletionData(ta, (IField)o);
			}
			if (!(o is IEvent))
			{
				throw new ApplicationException("Unknown object: " + o);
			}
			IEvent val3 = (IEvent)o;
			if (((IMember)val3).Name != null && ((AbstractCodeCompletionDataProvider)this).insertedEventElements[((IMember)val3).Name] == null)
			{
				((AbstractCodeCompletionDataProvider)this).insertedEventElements[((IMember)val3).Name] = val3;
				return (CodeCompletionData)(object)new ClaCodeCompletionData(ta, val3);
			}
		}
		return null;
	}

	public override CompletionDataProviderKeyResult ProcessKey(char key)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (key == ':')
		{
			((AbstractCompletionDataProvider)this).InsertSpace = false;
			return (CompletionDataProviderKeyResult)0;
		}
		return ((AbstractCompletionDataProvider)this).ProcessKey(key);
	}
}
