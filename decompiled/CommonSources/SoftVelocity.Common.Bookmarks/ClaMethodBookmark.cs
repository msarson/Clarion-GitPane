using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.ClassBrowser;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.Bookmarks;

public class ClaMethodBookmark : ClaMemberBookmark
{
	private IMethod method;

	public override int IconIndex
	{
		get
		{
			if (method is ClaRoutine)
			{
				return ClaClassNode.RoutineIcon;
			}
			if (method is ClaMethod && ((ClaMethod)(object)method).IsAccessor)
			{
				return ClassBrowserIconService.GetIcon((IProperty)(object)((ClaMethod)(object)method).DeclaringProperty);
			}
			return ClassBrowserIconService.GetIcon(method);
		}
	}

	public ClaMethodBookmark(IDocument document, IMethod method, bool showMenu)
		: base(document, (IMember)(object)method, showMenu)
	{
		this.method = method;
	}

	public ClaMethodBookmark(IDocument document, IMethod method, int line, bool showMenu)
		: base(document, (IMember)(object)method, line, showMenu)
	{
		this.method = method;
	}
}
