using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.TextEditor.Document;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.ClarionEditor;

public class ClaFoldingStrategy : IFoldingStrategy
{
	private bool changeCustomLines;

	public ClaFoldingStrategy()
	{
		changeCustomLines = true;
	}

	public ClaFoldingStrategy(bool changeCustomLines)
	{
		this.changeCustomLines = changeCustomLines;
	}

	private void AddClassMembers(ClaClass c, List<FoldMarker> foldMarkers, IDocument document, string fileName)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Expected O, but got Unknown
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Expected O, but got Unknown
		if (c == null || (int)c.ClassType == 4)
		{
			return;
		}
		DomRegion bodyRegion = c.BodyRegion;
		int beginLine = ((DomRegion)(ref bodyRegion)).BeginLine;
		DomRegion bodyRegion2 = c.BodyRegion;
		if (beginLine < ((DomRegion)(ref bodyRegion2)).EndLine && c.ClaRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
		{
			DomRegion bodyRegion3 = c.BodyRegion;
			int num = ((DomRegion)(ref bodyRegion3)).BeginLine - 1;
			DomRegion bodyRegion4 = c.BodyRegion;
			int num2 = ((DomRegion)(ref bodyRegion4)).BeginColumn - 1;
			DomRegion bodyRegion5 = c.BodyRegion;
			int num3 = ((DomRegion)(ref bodyRegion5)).EndLine - 1;
			DomRegion bodyRegion6 = c.BodyRegion;
			FoldMarker val = new FoldMarker(document, num, num2, num3, ((DomRegion)(ref bodyRegion6)).EndColumn - 1, (FoldType)3);
			if (((AbstractSegment)val).Length > 0)
			{
				foldMarkers.Add(val);
			}
		}
		foreach (IClass innerClass in c.InnerClasses)
		{
			AddClassMembers(innerClass as ClaClass, foldMarkers, document, fileName);
		}
		foreach (IMethod method in c.Methods)
		{
			if (method is ClaMethod)
			{
				AddMethod((ClaMethod)(object)method, foldMarkers, document, fileName);
			}
		}
		foreach (IField field in c.Fields)
		{
			if (field is ClaField { ClaBodyRegion: { IsEmpty: false }, ClaBodyRegion: { BeginLine: var beginLine2 }, ClaBodyRegion: var claBodyRegion3 } claField && beginLine2 < claBodyRegion3.EndLine && claField.ClaBodyRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
			{
				FoldMarker val2 = new FoldMarker(document, claField.ClaBodyRegion.BeginLine - 1, claField.ClaBodyRegion.BeginColumn - 1, claField.ClaBodyRegion.EndLine - 1, claField.ClaBodyRegion.EndColumn - 1, (FoldType)1);
				if (((AbstractSegment)val2).Length > 0)
				{
					foldMarkers.Add(val2);
				}
			}
		}
		foreach (IProperty property in c.Properties)
		{
			bool flag = true;
			if (property is ClaProperty)
			{
				ClaProperty claProperty = (ClaProperty)(object)property;
				if (!claProperty.ClaBodyRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
				{
					flag = false;
				}
				if (claProperty.Getter != null)
				{
					AddMethod(claProperty.Getter, foldMarkers, document, fileName);
				}
				if (claProperty.Setter != null)
				{
					AddMethod(claProperty.Setter, foldMarkers, document, fileName);
				}
			}
			DomRegion bodyRegion7 = ((IMember)property).BodyRegion;
			if (!((DomRegion)(ref bodyRegion7)).IsEmpty && flag)
			{
				DomRegion bodyRegion8 = ((IMember)property).BodyRegion;
				int num4 = ((DomRegion)(ref bodyRegion8)).BeginLine - 1;
				DomRegion bodyRegion9 = ((IMember)property).BodyRegion;
				int num5 = ((DomRegion)(ref bodyRegion9)).BeginColumn - 1;
				DomRegion bodyRegion10 = ((IMember)property).BodyRegion;
				int num6 = ((DomRegion)(ref bodyRegion10)).EndLine - 1;
				DomRegion bodyRegion11 = ((IMember)property).BodyRegion;
				FoldMarker val3 = new FoldMarker(document, num4, num5, num6, ((DomRegion)(ref bodyRegion11)).EndColumn - 1, (FoldType)1);
				if (((AbstractSegment)val3).Length > 0)
				{
					foldMarkers.Add(val3);
				}
			}
		}
		foreach (IEvent @event in c.Events)
		{
			DomRegion region = ((IMember)@event).Region;
			int endLine = ((DomRegion)(ref region)).EndLine;
			DomRegion bodyRegion12 = ((IMember)@event).BodyRegion;
			if (endLine < ((DomRegion)(ref bodyRegion12)).EndLine)
			{
				DomRegion bodyRegion13 = ((IMember)@event).BodyRegion;
				if (!((DomRegion)(ref bodyRegion13)).IsEmpty)
				{
					DomRegion region2 = ((IMember)@event).Region;
					int num7 = ((DomRegion)(ref region2)).EndLine - 1;
					DomRegion region3 = ((IMember)@event).Region;
					int num8 = ((DomRegion)(ref region3)).EndColumn - 1;
					DomRegion bodyRegion14 = ((IMember)@event).BodyRegion;
					int num9 = ((DomRegion)(ref bodyRegion14)).EndLine - 1;
					DomRegion bodyRegion15 = ((IMember)@event).BodyRegion;
					foldMarkers.Add(new FoldMarker(document, num7, num8, num9, ((DomRegion)(ref bodyRegion15)).EndColumn - 1, (FoldType)1));
				}
			}
		}
		foreach (ClaMethod unresolvedDefinition in c.UnresolvedDefinitions)
		{
			AddMethod(unresolvedDefinition, foldMarkers, document, fileName);
		}
	}

	private void AddMethod(ClaMethod m, List<FoldMarker> foldMarkers, IDocument document, string fileName)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		if (m == null)
		{
			return;
		}
		DomRegion bodyRegion = m.BodyRegion;
		if (!((DomRegion)(ref bodyRegion)).IsEmpty && m.ClaBodyRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
		{
			DomRegion bodyRegion2 = m.BodyRegion;
			int num = ((DomRegion)(ref bodyRegion2)).BeginLine - 1;
			DomRegion bodyRegion3 = m.BodyRegion;
			int num2 = ((DomRegion)(ref bodyRegion3)).BeginColumn - 1;
			DomRegion bodyRegion4 = m.BodyRegion;
			int num3 = ((DomRegion)(ref bodyRegion4)).EndLine - 1;
			DomRegion bodyRegion5 = m.BodyRegion;
			FoldMarker val = new FoldMarker(document, num, num2, num3, ((DomRegion)(ref bodyRegion5)).EndColumn - 1, (FoldType)1);
			if (((AbstractSegment)val).Length > 0)
			{
				foldMarkers.Add(val);
			}
		}
		foreach (ClaRoutine routine in m.Routines)
		{
			DomRegion bodyRegion6 = routine.BodyRegion;
			if (!((DomRegion)(ref bodyRegion6)).IsEmpty && routine.ClaBodyRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
			{
				DomRegion bodyRegion7 = routine.BodyRegion;
				int num4 = ((DomRegion)(ref bodyRegion7)).BeginLine - 1;
				DomRegion bodyRegion8 = routine.BodyRegion;
				int num5 = ((DomRegion)(ref bodyRegion8)).BeginColumn - 1;
				DomRegion bodyRegion9 = routine.BodyRegion;
				int num6 = ((DomRegion)(ref bodyRegion9)).EndLine - 1;
				DomRegion bodyRegion10 = routine.BodyRegion;
				FoldMarker val2 = new FoldMarker(document, num4, num5, num6, ((DomRegion)(ref bodyRegion10)).EndColumn - 1, (FoldType)1);
				if (((AbstractSegment)val2).Length > 0)
				{
					foldMarkers.Add(val2);
				}
			}
		}
		foreach (IClass localType in m.LocalTypes)
		{
			AddClassMembers(localType as ClaClass, foldMarkers, document, fileName);
		}
		foreach (IField localVariable in m.LocalVariables)
		{
			if (localVariable is ClaField { ClaBodyRegion: { IsEmpty: false }, ClaBodyRegion: { BeginLine: var beginLine }, ClaBodyRegion: var claBodyRegion3 } claField && beginLine < claBodyRegion3.EndLine && claField.ClaBodyRegion.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
			{
				FoldMarker val3 = new FoldMarker(document, claField.ClaBodyRegion.BeginLine - 1, claField.ClaBodyRegion.BeginColumn - 1, claField.ClaBodyRegion.EndLine - 1, claField.ClaBodyRegion.EndColumn - 1, (FoldType)1);
				if (((AbstractSegment)val3).Length > 0)
				{
					foldMarkers.Add(val3);
				}
			}
		}
		foreach (IMethod localMethod in m.LocalMethods)
		{
			AddMethod(localMethod as ClaMethod, foldMarkers, document, fileName);
		}
	}

	public List<FoldMarker> GenerateFoldMarkers(IDocument document, string fileName, object parseInfo)
	{
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		ParseInformation val = (ParseInformation)((parseInfo is ParseInformation) ? parseInfo : null);
		if (val == null || val.MostRecentCompilationUnit == null)
		{
			return null;
		}
		List<FoldMarker> foldMarkers = GetFoldMarkers(document, val.MostRecentCompilationUnit);
		if (val.BestCompilationUnit != val.MostRecentCompilationUnit)
		{
			List<FoldMarker> foldMarkers2 = GetFoldMarkers(document, val.BestCompilationUnit);
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
		if (changeCustomLines && val.MostRecentCompilationUnit is ClaCompilationUnit)
		{
			document.CustomLineManager.Clear();
			if (document.HighlightingStrategy != null)
			{
				HighlightColor colorFor = document.HighlightingStrategy.GetColorFor("OmittedCode");
				foreach (ClaDomOmitRegion omitRegion in ((ClaCompilationUnit)(object)val.MostRecentCompilationUnit).OmitRegions)
				{
					if (omitRegion.Region.FileName.Equals(val.MostRecentCompilationUnit.FileName, StringComparison.InvariantCultureIgnoreCase))
					{
						if (omitRegion.Omitted)
						{
							document.CustomLineManager.AddCustomLine(omitRegion.Region.BeginLine - 1, omitRegion.Region.EndLine - 1, colorFor.BackgroundColor, false);
						}
						FoldMarker val2 = new FoldMarker(document, omitRegion.Region.BeginLine - 1, omitRegion.Region.BeginColumn - 1, omitRegion.Region.EndLine - 1, omitRegion.Region.EndColumn - 1, (FoldType)2);
						if (((AbstractSegment)val2).Length > 0)
						{
							foldMarkers.Add(val2);
						}
					}
				}
			}
		}
		return foldMarkers;
	}

	private List<FoldMarker> GetFoldMarkers(IDocument document, ICompilationUnit cu)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		List<FoldMarker> list = new List<FoldMarker>();
		bool flag = document.FoldingManager.FoldMarker.Count == 0;
		foreach (FoldingRegion foldingRegion in cu.FoldingRegions)
		{
			DomRegion region = foldingRegion.Region;
			int num = ((DomRegion)(ref region)).BeginLine - 1;
			DomRegion region2 = foldingRegion.Region;
			int num2 = ((DomRegion)(ref region2)).BeginColumn - 1;
			DomRegion region3 = foldingRegion.Region;
			int num3 = ((DomRegion)(ref region3)).EndLine - 1;
			DomRegion region4 = foldingRegion.Region;
			list.Add(new FoldMarker(document, num, num2, num3, ((DomRegion)(ref region4)).EndColumn - 1, (FoldType)2, foldingRegion.Name, flag));
		}
		foreach (IClass @class in cu.Classes)
		{
			AddClassMembers(@class as ClaClass, list, document, cu.FileName);
		}
		if (cu is ClaCompilationUnit)
		{
			ClaCompilationUnit claCompilationUnit = (ClaCompilationUnit)(object)cu;
			foreach (ClaDomBodyRegion miscObjectsRegion in claCompilationUnit.MiscObjectsRegions)
			{
				if (!miscObjectsRegion.IsEmpty && miscObjectsRegion.FileName.Equals(cu.FileName, StringComparison.InvariantCultureIgnoreCase))
				{
					FoldMarker val = new FoldMarker(document, miscObjectsRegion.BeginLine - 1, miscObjectsRegion.BeginColumn - 1, miscObjectsRegion.EndLine - 1, miscObjectsRegion.EndColumn - 1, (FoldType)1);
					if (((AbstractSegment)val).Length > 0)
					{
						list.Add(val);
					}
				}
			}
		}
		if (cu.DokuComments != null)
		{
			foreach (IComment dokuComment in cu.DokuComments)
			{
				DomRegion region5 = dokuComment.Region;
				int num4 = ((DomRegion)(ref region5)).BeginLine - 1;
				DomRegion region6 = dokuComment.Region;
				int num5 = ((DomRegion)(ref region6)).BeginColumn - 1;
				DomRegion region7 = dokuComment.Region;
				int num6 = ((DomRegion)(ref region7)).EndLine - 1;
				DomRegion region8 = dokuComment.Region;
				list.Add(new FoldMarker(document, num4, num5, num6, ((DomRegion)(ref region8)).EndColumn - 1));
			}
		}
		list.Sort();
		return list;
	}
}
