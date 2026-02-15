using System;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class ChooseStorageLocationButton : Button
{
	private ToolStripMenuItem[] menuItems;

	private PropertyStorageLocations storageLocation;

	public PropertyStorageLocations StorageLocation
	{
		get
		{
			return storageLocation;
		}
		set
		{
			value = (((value & PropertyStorageLocations.ConfigurationAndPlatformSpecific) == 0) ? (value | PropertyStorageLocations.Base) : (value & ~PropertyStorageLocations.Base));
			if (storageLocation != value)
			{
				storageLocation = value;
				Image image = base.Image;
				base.Image = CreateImage(value);
				image?.Dispose();
				menuItems[0].Checked = (value & PropertyStorageLocations.ConfigurationSpecific) == PropertyStorageLocations.ConfigurationSpecific;
				menuItems[1].Checked = (value & PropertyStorageLocations.PlatformSpecific) == PropertyStorageLocations.PlatformSpecific;
				menuItems[2].Checked = (value & PropertyStorageLocations.UserFile) == PropertyStorageLocations.UserFile;
				if (this.StorageLocationChanged != null)
				{
					this.StorageLocationChanged(this, EventArgs.Empty);
				}
			}
		}
	}

	public event EventHandler StorageLocationChanged;

	public ChooseStorageLocationButton()
	{
		base.Size = new Size(20, 20);
		ContextMenuStrip = new ContextMenuStrip();
		menuItems = new ToolStripMenuItem[3]
		{
			CreateMenuItem("${res:Dialog.ProjectOptions.ConfigurationSpecific}", PropertyStorageLocations.ConfigurationSpecific),
			CreateMenuItem("${res:Dialog.ProjectOptions.PlatformSpecific}", PropertyStorageLocations.PlatformSpecific),
			CreateMenuItem("${res:Dialog.ProjectOptions.StoreInUserFile}", PropertyStorageLocations.UserFile)
		};
		ContextMenuStrip.Items.AddRange(menuItems);
		ContextMenuStrip.Items.Add(new ToolStripSeparator());
		ContextMenuStrip.Items.Add(StringParser.Parse("${res:Global.HelpButtonText}"), null, delegate
		{
			MessageService.ShowMessage("${res:Dialog.ProjectOptions.StorageLocationHelp}");
		});
	}

	private ToolStripMenuItem CreateMenuItem(string text, PropertyStorageLocations location)
	{
		ToolStripMenuItem item = new ToolStripMenuItem(StringParser.Parse(text));
		item.CheckOnClick = true;
		item.CheckedChanged += delegate
		{
			if (item.Checked)
			{
				StorageLocation |= location;
			}
			else
			{
				StorageLocation &= ~location;
			}
		};
		return item;
	}

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);
		ContextMenuStrip.Show(this, new Point(base.Width / 2, base.Height / 2));
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			base.Image.Dispose();
			ContextMenuStrip.Dispose();
		}
	}

	public static Image CreateImage(PropertyStorageLocations location)
	{
		Bitmap result = new Bitmap(12, 12);
		using (Graphics graphics = Graphics.FromImage(result))
		{
			graphics.Clear(Color.Transparent);
			Brush brush = (location & PropertyStorageLocations.ConfigurationAndPlatformSpecific) switch
			{
				PropertyStorageLocations.ConfigurationSpecific => Brushes.Blue, 
				PropertyStorageLocations.PlatformSpecific => Brushes.Red, 
				PropertyStorageLocations.ConfigurationAndPlatformSpecific => Brushes.Violet, 
				_ => Brushes.Black, 
			};
			if ((location & PropertyStorageLocations.UserFile) == PropertyStorageLocations.UserFile)
			{
				graphics.FillEllipse(brush, 0, 0, 7, 7);
				DrawU(graphics, 7, 5);
			}
			else
			{
				graphics.FillEllipse(brush, 2, 2, 8, 8);
			}
		}
		return result;
	}

	private static void DrawU(Graphics g, int x, int y)
	{
		g.DrawLine(Pens.DarkGreen, x, y, x, y + 6 - 1);
		g.DrawLine(Pens.DarkGreen, x + 4, y, x + 4, y + 6 - 1);
		g.DrawLine(Pens.DarkGreen, x + 1, y + 6, x + 4 - 1, y + 6);
	}
}
