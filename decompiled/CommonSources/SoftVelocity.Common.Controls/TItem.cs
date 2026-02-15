using System.Collections;
using System.Windows.Forms;

namespace SoftVelocity.Common.Controls;

public class TItem : TreeNode
{
	private object _Value;

	private CustomCollectionEditorForm ced;

	private IList _SubItems;

	public object Value
	{
		get
		{
			return _Value;
		}
		set
		{
			_Value = value;
		}
	}

	public IList SubItems
	{
		get
		{
			return _SubItems;
		}
		set
		{
			_SubItems = value;
			base.Nodes.Clear();
			if (value != null)
			{
				base.Nodes.AddRange(ced.GenerateTItemArray(value));
			}
		}
	}

	public TItem(CustomCollectionEditorForm ced, object Value)
	{
		this.ced = ced;
		_Value = Value;
	}

	public TItem(CustomCollectionEditorForm ced, object Value, int ImageIndex)
	{
		this.ced = ced;
		_Value = Value;
		base.ImageIndex = ImageIndex;
	}

	public TItem(CustomCollectionEditorForm ced, object Value, int ImageIndex, int SelectedImageIndex)
	{
		this.ced = ced;
		_Value = Value;
		base.ImageIndex = ImageIndex;
		base.SelectedImageIndex = SelectedImageIndex;
	}
}
