using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class MethodBookmark : ClassMemberBookmark
{
	private IMethod method;

	public override int IconIndex => ClassBrowserIconService.GetIcon(method);

	public MethodBookmark(IDocument document, IMethod method)
		: base(document, method)
	{
		this.method = method;
	}
}
