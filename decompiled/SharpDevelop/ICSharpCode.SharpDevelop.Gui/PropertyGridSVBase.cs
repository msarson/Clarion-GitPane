using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using VisualHint.SmartPropertyGrid;

namespace ICSharpCode.SharpDevelop.Gui;

public class PropertyGridSVBase : PropertyGridSV
{
	private bool thisParentFormFormClosingAdded;

	private bool _autoPreserveLabelSize = true;

	private bool firstObject;

	private int _LabelColumnWidth;

	public bool AutoPreserveLabelSize
	{
		get
		{
			return _autoPreserveLabelSize;
		}
		set
		{
			_autoPreserveLabelSize = value;
		}
	}

	public PropertyGridSVBase()
	{
		Font = FontService.GetFont(FontService.FontType.ListControls);
		base.AutoResizeLabelColumn = false;
		base.LabelWidthRatioChanged += OnLabelWidthRatioChanged;
		base.ParentChanged += OnParentChanged;
		base.SelectedObjectChanged += OnSelectedObjectChanged;
	}

	private void OnParentChanged(object sender, EventArgs e)
	{
		base.ParentChanged -= OnParentChanged;
		if (base.ParentForm != null)
		{
			thisParentFormFormClosingAdded = true;
			base.ParentForm.FormClosing += OnParentForm_FormClosing;
		}
	}

	private void OnSelectedObjectChanged(object sender, SelectedObjectChangedEventArgs e)
	{
		if (!firstObject && SelectedObjects != null && SelectedObjects.Length != 0)
		{
			firstObject = true;
			if (AutoPreserveLabelSize && !base.AutoResizeLabelColumn)
			{
				RestoreLabelSize();
			}
		}
	}

	public void RestoreLabelSize()
	{
		SetLabelColumnWidth(_LabelColumnWidth = PropertyService.Get("PropertyGridLabelSize", _LabelColumnWidth, base.Name));
	}

	public void StoreLabelSize()
	{
		PropertyService.Set("PropertyGridLabelSize", _LabelColumnWidth, base.Name);
	}

	private void OnParentForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (base.ParentForm != null && thisParentFormFormClosingAdded)
		{
			base.ParentForm.FormClosing -= OnParentForm_FormClosing;
		}
		if (AutoPreserveLabelSize)
		{
			RestoreLabelSize();
		}
	}

	private void OnLabelWidthRatioChanged(object sender, EventArgs e)
	{
		if (SelectedObjects != null && SelectedObjects.Length != 0 && firstObject)
		{
			_LabelColumnWidth = base.LabelColumnWidth;
		}
	}
}
