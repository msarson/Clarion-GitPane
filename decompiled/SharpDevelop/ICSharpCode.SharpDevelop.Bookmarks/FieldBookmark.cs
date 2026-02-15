using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class FieldBookmark : ClassMemberBookmark
{
	private IField field;

	public override int IconIndex => ClassBrowserIconService.GetIcon(field);

	public FieldBookmark(IDocument document, IField field)
		: base(document, field)
	{
		this.field = field;
	}
}
