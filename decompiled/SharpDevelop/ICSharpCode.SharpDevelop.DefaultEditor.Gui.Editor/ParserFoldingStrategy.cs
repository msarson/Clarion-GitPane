using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;

public class ParserFoldingStrategy : IFoldingStrategy
{
	private void AddClassMembers(IClass c, List<FoldMarker> foldMarkers, IDocument document)
	{
		if (c.ClassType == ClassType.Delegate)
		{
			return;
		}
		DomRegion domRegion = c.BodyRegion;
		if (domRegion.IsEmpty)
		{
			domRegion = c.Region;
		}
		if (domRegion.BeginLine < domRegion.EndLine)
		{
			FoldMarker foldMarker = new FoldMarker(document, domRegion.BeginLine - 1, domRegion.BeginColumn - 1, domRegion.EndLine - 1, domRegion.EndColumn, (c.ClassType == ClassType.Enum) ? FoldType.MemberBody : FoldType.TypeBody);
			if (foldMarker.Length > 0)
			{
				foldMarkers.Add(foldMarker);
			}
		}
		foreach (IClass innerClass in c.InnerClasses)
		{
			AddClassMembers(innerClass, foldMarkers, document);
		}
		foreach (IMethod method in c.Methods)
		{
			if (method.Region.EndLine < method.BodyRegion.EndLine)
			{
				foldMarkers.Add(new FoldMarker(document, method.Region.EndLine - 1, method.Region.EndColumn - 1, method.BodyRegion.EndLine - 1, method.BodyRegion.EndColumn - 1, FoldType.MemberBody));
			}
		}
		foreach (IProperty property in c.Properties)
		{
			if (property.Region.EndLine < property.BodyRegion.EndLine)
			{
				foldMarkers.Add(new FoldMarker(document, property.Region.EndLine - 1, property.Region.EndColumn - 1, property.BodyRegion.EndLine - 1, property.BodyRegion.EndColumn - 1, FoldType.MemberBody));
			}
		}
		foreach (IEvent @event in c.Events)
		{
			if (@event.Region.EndLine < @event.BodyRegion.EndLine && !@event.BodyRegion.IsEmpty)
			{
				foldMarkers.Add(new FoldMarker(document, @event.Region.EndLine - 1, @event.Region.EndColumn - 1, @event.BodyRegion.EndLine - 1, @event.BodyRegion.EndColumn - 1, FoldType.MemberBody));
			}
		}
	}

	public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInfo)
	{
		if (!(parseInfo is ParseInformation { MostRecentCompilationUnit: not null } parseInformation))
		{
			return null;
		}
		List<FoldMarker> foldMarkers = GetFoldMarkers(document, parseInformation.MostRecentCompilationUnit);
		if (parseInformation.BestCompilationUnit != parseInformation.MostRecentCompilationUnit)
		{
			List<FoldMarker> foldMarkers2 = GetFoldMarkers(document, parseInformation.BestCompilationUnit);
			int num = ((foldMarkers.Count != 0) ? foldMarkers[foldMarkers.Count - 1].EndLine : 0);
			int totalNumberOfLines = document.TotalNumberOfLines;
			foreach (FoldMarker item in foldMarkers2)
			{
				if (item.StartLine > num && item.EndLine < totalNumberOfLines)
				{
					foldMarkers.Add(item);
				}
			}
		}
		return foldMarkers;
	}

	private List<FoldMarker> GetFoldMarkers(IDocument document, ICompilationUnit cu)
	{
		List<FoldMarker> list = new List<FoldMarker>();
		bool isFolded = document.FoldingManager.FoldMarker.Count == 0;
		foreach (FoldingRegion foldingRegion in cu.FoldingRegions)
		{
			list.Add(new FoldMarker(document, foldingRegion.Region.BeginLine - 1, foldingRegion.Region.BeginColumn - 1, foldingRegion.Region.EndLine - 1, foldingRegion.Region.EndColumn - 1, FoldType.Region, foldingRegion.Name, isFolded));
		}
		foreach (IClass @class in cu.Classes)
		{
			AddClassMembers(@class, list, document);
		}
		if (cu.DokuComments != null)
		{
			foreach (IComment dokuComment in cu.DokuComments)
			{
				list.Add(new FoldMarker(document, dokuComment.Region.BeginLine - 1, dokuComment.Region.BeginColumn - 1, dokuComment.Region.EndLine - 1, dokuComment.Region.EndColumn - 1));
			}
		}
		return list;
	}
}
