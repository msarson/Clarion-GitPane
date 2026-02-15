using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class PropertyBookmark : ClassMemberBookmark
{
	private IProperty property;

	public override int IconIndex => ClassBrowserIconService.GetIcon(property);

	public PropertyBookmark(IDocument document, IProperty property)
		: base(document, property)
	{
		this.property = property;
	}
}
