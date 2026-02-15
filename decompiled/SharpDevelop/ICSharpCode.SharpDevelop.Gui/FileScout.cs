using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class FileScout : UserControl, IPadContent, IDisposable
{
	private Splitter splitter1 = new Splitter();

	private FileList filelister = new FileList();

	private ShellTree filetree = new ShellTree();

	public Control Control => this;

	public bool WantsEscape => false;

	public void RedrawContent()
	{
	}

	public FileScout()
	{
		filetree.Dock = DockStyle.Top;
		filetree.BorderStyle = BorderStyle.Fixed3D;
		filetree.Location = new Point(0, 22);
		filetree.Size = new Size(184, 157);
		filetree.TabIndex = 1;
		filetree.AfterSelect += DirectorySelected;
		ImageList imageList = new ImageList
		{
			ColorDepth = ColorDepth.Depth32Bit,
			Images = 
			{
				(Image)ResourceService.GetBitmap("Icons.16x16.ClosedFolderBitmap"),
				(Image)ResourceService.GetBitmap("Icons.16x16.OpenFolderBitmap"),
				(Image)ResourceService.GetBitmap("Icons.16x16.FLOPPY"),
				(Image)ResourceService.GetBitmap("Icons.16x16.DRIVE"),
				(Image)ResourceService.GetBitmap("Icons.16x16.CDROM"),
				(Image)ResourceService.GetBitmap("Icons.16x16.NETWORK"),
				(Image)ResourceService.GetBitmap("Icons.16x16.Desktop"),
				(Image)ResourceService.GetBitmap("Icons.16x16.PersonalFiles"),
				(Image)ResourceService.GetBitmap("Icons.16x16.MyComputer")
			}
		};
		filetree.ImageList = imageList;
		filelister.Dock = DockStyle.Fill;
		filelister.BorderStyle = BorderStyle.Fixed3D;
		filelister.Location = new Point(0, 184);
		filelister.Sorting = SortOrder.Ascending;
		filelister.Size = new Size(184, 450);
		filelister.TabIndex = 3;
		filelister.ItemActivate += FileSelected;
		splitter1.Dock = DockStyle.Top;
		splitter1.Location = new Point(0, 179);
		splitter1.Size = new Size(184, 5);
		splitter1.TabIndex = 2;
		splitter1.TabStop = false;
		splitter1.MinSize = 50;
		splitter1.MinExtra = 50;
		base.Controls.Add(filelister);
		base.Controls.Add(splitter1);
		base.Controls.Add(filetree);
	}

	private void DirectorySelected(object sender, TreeViewEventArgs e)
	{
		filelister.ShowFilesInPath(filetree.NodePath + Path.DirectorySeparatorChar);
	}

	private void FileSelected(object sender, EventArgs e)
	{
		foreach (FileList.FileListItem selectedItem in filelister.SelectedItems)
		{
			IProjectLoader projectLoader = ProjectService.GetProjectLoader(selectedItem.FullName);
			if (projectLoader != null)
			{
				projectLoader.Load(selectedItem.FullName);
			}
			else
			{
				FileService.OpenFile(selectedItem.FullName);
			}
		}
	}
}
