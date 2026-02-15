using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Commands;

public class NavigationHistoryMenuBuilder : ISubmenuBuilder
{
	private int numberOfAdditionalItems = 2;

	private ToolStripItem[] BuildMenuFlat(ICollection<INavigationPoint> points, int additionalItems)
	{
		ToolStripItem[] array = new ToolStripItem[points.Count + additionalItems];
		MenuCommand menuCommand = null;
		INavigationPoint navigationPoint = null;
		List<INavigationPoint> list = new List<INavigationPoint>(points);
		int num = points.Count - 1;
		int num2 = 0;
		while (num2 < points.Count)
		{
			navigationPoint = list[num - num2];
			menuCommand = new MenuCommand(navigationPoint.Description, NavigateTo);
			menuCommand.Tag = navigationPoint;
			array[num2++] = menuCommand;
		}
		return array;
	}

	private ToolStripItem[] BuildMenuByFile(ICollection<INavigationPoint> points, int additionalItems)
	{
		Dictionary<string, List<INavigationPoint>> dictionary = new Dictionary<string, List<INavigationPoint>>();
		List<string> list = new List<string>();
		foreach (INavigationPoint point in points)
		{
			if (point.FileName == null)
			{
				throw new ApplicationException("should not get here!");
			}
			if (!list.Contains(point.FileName))
			{
				list.Add(point.FileName);
				dictionary.Add(point.FileName, new List<INavigationPoint>());
			}
			if (!dictionary[point.FileName].Contains(point))
			{
				dictionary[point.FileName].Add(point);
			}
		}
		list.Sort();
		ToolStripItem[] array = new ToolStripItem[list.Count + additionalItems];
		ToolStripMenuItem toolStripMenuItem = null;
		MenuCommand menuCommand = null;
		int num = 0;
		foreach (string item in list)
		{
			toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Text = Path.GetFileName(item);
			toolStripMenuItem.ToolTipText = item;
			foreach (INavigationPoint item2 in dictionary[item])
			{
				menuCommand = new MenuCommand(item2.Description, NavigateTo);
				menuCommand.Tag = item2;
				toolStripMenuItem.DropDownItems.Add(menuCommand);
			}
			array[num++] = toolStripMenuItem;
		}
		return array;
	}

	public ToolStripItem[] BuildSubmenu(Codon codon, object owner)
	{
		MenuCommand menuCommand = null;
		if (NavigationService.CanNavigateBack || NavigationService.CanNavigateForwards)
		{
			ICollection<INavigationPoint> points = NavigationService.Points;
			ToolStripItem[] array = BuildMenuByFile(points, numberOfAdditionalItems);
			int num = array.Length - numberOfAdditionalItems;
			array[num++] = new ToolStripSeparator();
			menuCommand = new MenuCommand("${res:XML.MainMenu.Navigation.ClearHistory}", ClearHistory);
			array[num++] = menuCommand;
			return array;
		}
		return null;
	}

	public void NavigateTo(object sender, EventArgs e)
	{
		MenuCommand menuCommand = (MenuCommand)sender;
		NavigationService.Go((INavigationPoint)menuCommand.Tag);
	}

	public void ClearHistory(object sender, EventArgs e)
	{
		NavigationService.ClearHistory();
	}
}
