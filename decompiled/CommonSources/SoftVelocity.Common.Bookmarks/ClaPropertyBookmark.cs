using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.Bookmarks;

public class ClaPropertyBookmark : ClaMemberBookmark
{
	private IProperty property;

	public override int IconIndex => ClassBrowserIconService.GetIcon(property);

	public ClaPropertyBookmark(IDocument document, IProperty property, bool showMenu)
		: base(document, (IMember)(object)property, showMenu)
	{
		this.property = property;
	}

	public ClaPropertyBookmark(IDocument document, IProperty property, int line, bool showMenu)
		: base(document, (IMember)(object)property, line, showMenu)
	{
		this.property = property;
	}
}
