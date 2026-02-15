using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.Bookmarks;

public class EventBookmark : ClassMemberBookmark
{
	private IEvent @event;

	public override int IconIndex => ClassBrowserIconService.GetIcon(@event);

	public EventBookmark(IDocument document, IEvent @event)
		: base(document, @event)
	{
		this.@event = @event;
	}
}
