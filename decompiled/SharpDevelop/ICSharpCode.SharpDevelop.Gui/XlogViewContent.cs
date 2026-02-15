using System.IO;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Gui;

internal class XlogViewContent : AbstractViewContent
{
	private XlogViewer _viewer;

	private XlogViewer viewer
	{
		get
		{
			if (_viewer == null)
			{
				_viewer = new XlogViewer();
			}
			return _viewer;
		}
	}

	public override Control Control => viewer;

	public override string FileName
	{
		get
		{
			return viewer.FileName;
		}
		set
		{
			viewer.LoadFile(value);
			base.FileName = value;
			base.TitleName = Path.GetFileName(value);
			base.IsDirty = false;
		}
	}

	public override void Load(string fileName)
	{
		FileName = fileName;
	}
}
