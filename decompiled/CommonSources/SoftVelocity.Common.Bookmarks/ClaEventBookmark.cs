using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace SoftVelocity.Common.Bookmarks;

public class ClaEventBookmark : ClaMemberBookmark
{
	private IEvent @event;

	public override int IconIndex => ClassBrowserIconService.GetIcon(@event);

	public ClaEventBookmark(IDocument document, IEvent @event, bool showMenu)
		: base(document, (IMember)(object)@event, showMenu)
	{
		this.@event = @event;
	}
}
