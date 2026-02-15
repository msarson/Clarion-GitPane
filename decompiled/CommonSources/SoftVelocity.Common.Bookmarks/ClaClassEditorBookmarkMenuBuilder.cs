using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Gui.ClassBrowser;
using ICSharpCode.SharpDevelop.Project;
using SoftVelocity.Common.ClassBrowser;
using SoftVelocity.Common.Parser.IDE.Ast;

namespace SoftVelocity.Common.Bookmarks;

public abstract class ClaClassEditorBookmarkMenuBuilder : ClaClassBookmarkMenuBuilder
{
	public override ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		IClass tag;
		if (owner is ClaClassNode claClassNode)
		{
			tag = claClassNode.Class;
		}
		else
		{
			ClaClassBookmark claClassBookmark = (ClaClassBookmark)owner;
			tag = claClassBookmark.Class;
		}
		List<ToolStripItem> list = new List<ToolStripItem>();
		list.AddRange(base.BuildSubmenu(codon, owner));
		if (ProjectService.CurrentProject != null)
		{
			if (list.Count > 0)
			{
				list.Add(new ToolStripSeparator());
			}
			MenuCommand val = new MenuCommand("Show in Class Browser", (EventHandler)ShowInClassBrowser);
			((ToolStripItem)(object)val).Tag = tag;
			list.Add((ToolStripItem)(object)val);
		}
		return list.ToArray();
	}

	private void ShowInClassBrowser(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		MenuCommand val = (MenuCommand)sender;
		ClaClass claClass = ((ToolStripItem)(object)val).Tag as ClaClass;
		if (claClass == null && ((ToolStripItem)(object)val).Tag is CompoundClass)
		{
			foreach (IClass part in ((CompoundClass)((ToolStripItem)(object)val).Tag).GetParts())
			{
				if (part is ClaClass)
				{
					claClass = (ClaClass)(object)part;
					break;
				}
			}
		}
		if (claClass == null)
		{
			return;
		}
		PadDescriptor pad = WorkbenchSingleton.Workbench.GetPad(typeof(ClassBrowserPad));
		if (pad != null)
		{
			pad.CreatePad();
			pad.BringPadToFront();
			if (ClassBrowserPad.Instance != null && ProjectService.CurrentProject != null)
			{
				ClassBrowserPad.Instance.SelectNode(ProjectService.CurrentProject, MakeClassPathInternal(claClass, addClassName: false), (claClass is ClaGlobalClass) ? ClaGlobalClass.globalClassName : claClass.Name);
			}
		}
	}

	protected abstract string MakeClassPathInternal(ClaClass c, bool addClassName);
}
