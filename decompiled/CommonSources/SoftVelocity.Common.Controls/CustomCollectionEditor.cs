using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SoftVelocity.Common.Controls;

public class CustomCollectionEditor : UITypeEditor
{
	public delegate void CollectionChangedEventHandler(object sender, object instance, object value);

	private ITypeDescriptorContext _context;

	private IWindowsFormsEditorService edSvc;

	public event CollectionChangedEventHandler CollectionChanged;

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (context != null && context.Instance != null && provider != null)
		{
			_context = context;
			edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (edSvc != null)
			{
				CustomCollectionEditorForm customCollectionEditorForm = CreateForm();
				customCollectionEditorForm.ItemAdded += ItemAdded;
				customCollectionEditorForm.ItemRemoved += ItemRemoved;
				customCollectionEditorForm.Collection = (CollectionBase)value;
				context.OnComponentChanging();
				if (edSvc.ShowDialog(customCollectionEditorForm) == DialogResult.OK)
				{
					OnCollectionChanged(context.Instance, value);
					context.OnComponentChanged();
				}
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		if (context != null && context.Instance != null)
		{
			return UITypeEditorEditStyle.Modal;
		}
		return base.GetEditStyle(context);
	}

	private void ItemAdded(object sender, object item)
	{
		if (_context != null && _context.Container != null && item is IComponent component)
		{
			_context.Container.Add(component);
		}
	}

	private void ItemRemoved(object sender, object item)
	{
		if (_context != null && _context.Container != null && item is IComponent component)
		{
			_context.Container.Remove(component);
		}
	}

	protected virtual void OnCollectionChanged(object instance, object value)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, instance, value);
		}
	}

	protected virtual CustomCollectionEditorForm CreateForm()
	{
		return new CustomCollectionEditorForm();
	}
}
