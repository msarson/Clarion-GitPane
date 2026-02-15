using ICSharpCode.FormsDesigner;

namespace SoftVelocity.Common.FormDesigner;

public class FormsDesignerInitializeCompNotFoundException : FormsDesignerLoadException
{
	public FormsDesignerInitializeCompNotFoundException(string str)
		: base(str)
	{
	}
}
