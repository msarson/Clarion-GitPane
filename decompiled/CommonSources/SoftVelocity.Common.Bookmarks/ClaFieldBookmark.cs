using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.Bookmarks;

public class ClaFieldBookmark : ClaMemberBookmark
{
	private IField field;

	public override int IconIndex => ClassBrowserIconService.GetIcon(field);

	public ClaFieldBookmark(IDocument document, IField field, bool showMenu)
		: base(document, (IMember)(object)field, showMenu)
	{
		this.field = field;
	}
}
