using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui.XmlForms;

namespace SoftVelocity.Common.Dialogs.FileDrivers;

public class SelectFileDriverDialog : FileDriverDialog
{
	public ListView.SelectedListViewItemCollection SelectedItems => ((ListView)((XmlForm)this).ControlDictionary["driverList"]).SelectedItems;

	public ListView.ListViewItemCollection Items => ((ListView)((XmlForm)this).ControlDictionary["driverList"]).Items;

	public string Title
	{
		set
		{
			((Control)(object)this).Text = value;
		}
	}

	public SelectFileDriverDialog(bool allowMultiSelect, bool browsableDriversOnly)
		: base(allowMultiSelect, browsableDriversOnly)
	{
	}

	public SelectFileDriverDialog(bool allowMultiSelect)
		: base(allowMultiSelect)
	{
	}

	public SelectFileDriverDialog()
	{
	}

	protected override void SetupForm()
	{
		((XmlForm)this).SetupFromXmlStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("CommonSources.Resources.FileDriversDialog.xfrm"));
		selectButtonName = "selectButton";
		((XmlForm)this).Get<Button>("register").Click += base.AddPressed;
	}
}
