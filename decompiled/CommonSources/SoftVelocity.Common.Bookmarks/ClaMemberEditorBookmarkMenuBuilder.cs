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

public abstract class ClaMemberEditorBookmarkMenuBuilder : ClaMemberBookmarkMenuBuilder
{
	public override ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		IMember member;
		if (owner is ClaMemberNode claMemberNode)
		{
			member = claMemberNode.Member;
		}
		else
		{
			ClaMemberBookmark claMemberBookmark = (ClaMemberBookmark)owner;
			member = claMemberBookmark.Member;
		}
		List<ToolStripItem> list = new List<ToolStripItem>();
		list.AddRange(base.BuildSubmenu(codon, owner));
		if (member is ClaAbstractMember && ProjectService.CurrentProject != null)
		{
			if (list.Count > 0)
			{
				list.Add(new ToolStripSeparator());
			}
			MenuCommand val = new MenuCommand("Show in Class Browser", (EventHandler)ShowInClassBrowser);
			((ToolStripItem)(object)val).Tag = member;
			list.Add((ToolStripItem)(object)val);
		}
		return list.ToArray();
	}

	private void ShowInClassBrowser(object sender, EventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		MenuCommand val = (MenuCommand)sender;
		if (!(((ToolStripItem)(object)val).Tag is ClaAbstractMember claAbstractMember))
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
				ClassBrowserPad.Instance.SelectNode(ProjectService.CurrentProject, MakeMemberPath(claAbstractMember), ClaMemberNode.GetText((IMember)(object)claAbstractMember));
			}
		}
	}

	protected abstract string MakeMemberPath(ClaAbstractMember m);
}
