using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ICSharpCode.SharpDevelop.Gui.CustomizableStrips;

public class CustomAppearancePropertyEditor : UITypeEditor
{
	private AppearanceEditor _AppearanceEditor;

	private Control _EditControl;

	protected IWindowsFormsEditorService IEditorService;

	private AppearanceEditor AppearanceEditor
	{
		get
		{
			return _AppearanceEditor;
		}
		set
		{
			_AppearanceEditor = value;
		}
	}

	private Control EditControl
	{
		get
		{
			return _EditControl;
		}
		set
		{
			_EditControl = value;
		}
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		try
		{
			if (context != null && provider != null)
			{
				IEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (IEditorService != null)
				{
					string name = context.PropertyDescriptor.Name;
					EditControl = GetEditControl(name, RuntimeHelpers.GetObjectValue(value));
					if (EditControl != null)
					{
						IEditorService.ShowDialog((Form)EditControl);
						context.OnComponentChanged();
						return GetEditedValue(EditControl, name, RuntimeHelpers.GetObjectValue(value));
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return base.EditValue(context, provider, RuntimeHelpers.GetObjectValue(value));
	}

	private Control GetEditControl(string PropertyName, object CurrentValue)
	{
		if (CurrentValue is AppearanceProperties ap)
		{
			AppearanceEditor = new AppearanceEditor(ap);
			return AppearanceEditor;
		}
		return null;
	}

	private object GetEditedValue(Control EditControl, string PropertyName, object OldValue)
	{
		if (AppearanceEditor == null || AppearanceEditor.DialogResult == DialogResult.Cancel)
		{
			return OldValue;
		}
		return AppearanceEditor.CustomAppearance;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}
}
