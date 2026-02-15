using SoftVelocity.Common.ClarionEditor;

namespace SoftVelocity.Common.CodeCompletion;

public class ClaPreCompletionData : ClaCodeCompletionData
{
	public override bool IsPre => true;

	public ClaPreCompletionData(ClarionCommonTextAreaControl textArea, string s, int imageIndex)
		: base(textArea, s + ":", imageIndex)
	{
		base.DeclText = "(prefix)";
	}
}
