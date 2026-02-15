using System;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.ClassBrowser;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.CodeCompletion.Commands;

public class GotoType : AbstractMenuCommand
{
	public override void Run()
	{
		if (ProjectService.OpenSolution != null && ProjectService.OpenSolution.HasProjects && !ParserService.LoadSolutionProjectsThreadRunning)
		{
			GotoPopupWindow gotoPopupWindow = new GotoPopupWindow(FillItemsList, ClassBrowserIconService.ImageList);
			gotoPopupWindow.ItemSelected += PopupItemSelected;
			gotoPopupWindow.Closed += PopupClosed;
			gotoPopupWindow.ShowAsContextMenu(WorkbenchSingleton.MainForm);
		}
	}

	private static void PopupClosed(object sender, EventArgs e)
	{
		GotoPopupWindow gotoPopupWindow = (GotoPopupWindow)sender;
		gotoPopupWindow.ItemSelected -= PopupItemSelected;
		gotoPopupWindow.Closed -= PopupClosed;
		gotoPopupWindow.Dispose();
	}

	private static void PopupItemSelected(object sender, SelectedItemEventArgs e)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		object selectedItem = e.SelectedItem;
		IClass val = (IClass)((selectedItem is IClass) ? selectedItem : null);
		if (val != null)
		{
			if (val is ClaClass)
			{
				string fileName = ((ClaClass)(object)val).ClaRegion.FileName;
				DomRegion region = val.Region;
				int num = ((DomRegion)(ref region)).BeginLine - 1;
				DomRegion region2 = val.Region;
				FileService.JumpToFilePosition(fileName, num, ((DomRegion)(ref region2)).BeginColumn - 1);
			}
			else if (val.CompilationUnit != null)
			{
				string fileName2 = val.CompilationUnit.FileName;
				DomRegion region3 = val.Region;
				int num2 = ((DomRegion)(ref region3)).BeginLine - 1;
				DomRegion region4 = val.Region;
				FileService.JumpToFilePosition(fileName2, num2, ((DomRegion)(ref region4)).BeginColumn - 1);
			}
			return;
		}
		object selectedItem2 = e.SelectedItem;
		IMethod val2 = (IMethod)((selectedItem2 is IMethod) ? selectedItem2 : null);
		if (val2 != null)
		{
			if (val2 is ClaMethod)
			{
				ClaMethod claMethod = (ClaMethod)(object)val2;
				FileService.JumpToFilePosition(claMethod.ClaBodyRegion.FileName, claMethod.ClaBodyRegion.BeginLine - 1, claMethod.ClaBodyRegion.BeginColumn - 1);
			}
			else if (((IDecoration)val2).DeclaringType != null && ((IDecoration)val2).DeclaringType.CompilationUnit != null)
			{
				string fileName3 = ((IDecoration)val2).DeclaringType.CompilationUnit.FileName;
				DomRegion bodyRegion = ((IMember)val2).BodyRegion;
				int num3 = ((DomRegion)(ref bodyRegion)).BeginLine - 1;
				DomRegion bodyRegion2 = ((IMember)val2).BodyRegion;
				FileService.JumpToFilePosition(fileName3, num3, ((DomRegion)(ref bodyRegion2)).BeginColumn - 1);
			}
		}
	}

	private static void FillItemsList(GotoPopupWindow.AddItemDelegate addMethod)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		foreach (IProjectContent allProjectContent in ParserService.AllProjectContents)
		{
			foreach (IClass @class in allProjectContent.Classes)
			{
				if (@class is ClaGlobalClass || (@class is CompoundClass && ((CompoundClass)@class).GetParts()[0] is ClaGlobalClass))
				{
					AddGlobalMembers(@class, addMethod);
					continue;
				}
				string description;
				if (@class.CompilationUnit is ClaCompilationUnit && ((ClaCompilationUnit)(object)@class.CompilationUnit).IsWin)
				{
					string arg = ((@class is ClaClass && !string.IsNullOrEmpty(((ClaClass)(object)@class).ClaRegion.FileName)) ? ((ClaClass)(object)@class).ClaRegion.FileName : string.Empty);
					description = $"(from {arg})";
				}
				else
				{
					description = $"(in {@class.Namespace})";
				}
				int sortOrder = 0;
				addMethod(@class.Name, description, ClaClassNode.GetIconIndexForClass(@class, ref sortOrder), @class);
			}
		}
	}

	private static void AddGlobalMembers(IClass c, GotoPopupWindow.AddItemDelegate addMethod)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		ClaGlobalClass claGlobalClass = ((c is ClaGlobalClass) ? ((ClaGlobalClass)(object)c) : ((ClaGlobalClass)(object)((CompoundClass)c).GetParts()[0]));
		bool isWin = ((ClaCompilationUnit)(object)claGlobalClass.CompilationUnit).IsWin;
		foreach (IMethod method in c.Methods)
		{
			if (method is ClaMethod { ClaBodyRegion: { IsEmpty: false } } claMethod)
			{
				string description = (isWin ? $"(from {claMethod.ClaBodyRegion.FileName})" : $"(in {c.Namespace})");
				addMethod(claMethod.Name, description, ClassBrowserIconService.GetIcon(method), claMethod);
			}
		}
	}
}
