using System.Windows.Forms;
using Clarion.ASL;
using Clarion.GEN;
using ICSharpCode.SharpDevelop.Gui;
using SoftVelocity.CWPInvoke;

namespace SoftVelocity.Generator.UI;

public class TemplateRegistryControl_ViewContent : CWControl_ViewContent
{
	private int GenInstance;

	public override Control Control => _Container;

	protected override int InstID => GenInstance;

	public override string FileName => Win32Generator.TemplateRegistryName;

	public TemplateRegistryControl_ViewContent(int id)
	{
		_Container = new TemplateRegistryControl_Container(this);
		GenInstance = id;
		((AbstractViewContent)this).IsDirty = false;
		((AbstractViewContent)this).TitleName = "Template Registry";
		((AbstractViewContent)this).UntitledName = "";
		CWDialogService.Instance.ValidateView += ValidateObject;
	}

	public override void Dispose()
	{
		CWDialogService.Instance.ValidateView -= ValidateObject;
		base.Dispose();
	}

	internal override void ParentFormClosed()
	{
	}

	internal override void AllControlsClosed()
	{
		base.AllControlsClosed();
	}

	private void ValidateObject(UINetBinding CWObj, ref IViewContent content)
	{
		if (ValidObject(CWObj))
		{
			content = (IViewContent)(object)this;
		}
	}
}
