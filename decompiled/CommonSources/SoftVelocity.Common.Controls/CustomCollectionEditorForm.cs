using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using CommonSources.Properties;

namespace SoftVelocity.Common.Controls;

public class CustomCollectionEditorForm : Form
{
	public delegate void InstanceEventHandler(object sender, object instance);

	private IList _Collection;

	private Type lastItemType;

	private ArrayList backupList;

	private IContainer components;

	private EditLevel _EditLevel;

	protected PropertyGrid pg_PropGrid;

	protected Button btn_Add;

	protected Button btn_Remove;

	protected Button btn_Up;

	protected Button btn_Down;

	private Panel pan_Items;

	private Panel pan_MainPan;

	private Splitter spl_Splitter;

	private Panel pan_ButtonsPan;

	protected Button btn_OK;

	protected Button btn_Cancel;

	protected TreeView tv_Items;

	private Panel pan_PropGridPan;

	private CustomCollectionEditor attachedEditor;

	public IList Collection
	{
		get
		{
			return _Collection;
		}
		set
		{
			_Collection = value;
			backupList = new ArrayList(value);
			ProccessCollection(value);
			RefreshValues();
		}
	}

	[Category("Behavior")]
	public EditLevel EditLevel
	{
		get
		{
			return _EditLevel;
		}
		set
		{
			if (value != _EditLevel)
			{
				_EditLevel = value;
				OnEditLevelChanged(new EventArgs());
			}
		}
	}

	[Category("Behavior")]
	public ImageList ImageList
	{
		get
		{
			return tv_Items.ImageList;
		}
		set
		{
			tv_Items.ImageList = value;
		}
	}

	public event InstanceEventHandler InstanceCreated;

	public event InstanceEventHandler DestroyingInstance;

	public event InstanceEventHandler ItemRemoved;

	public event InstanceEventHandler ItemAdded;

	public CustomCollectionEditorForm()
	{
		InitializeComponent();
		RefreshValues();
	}

	protected virtual Type GetItemType(IList coll)
	{
		PropertyInfo property = coll.GetType().GetProperty("Item", new Type[1] { typeof(int) });
		return property.PropertyType;
	}

	protected virtual Type[] CreateNewItemTypes(IList coll)
	{
		return new Type[1] { GetItemType(coll) };
	}

	protected virtual object CreateInstance(Type itemType)
	{
		object obj = Activator.CreateInstance(itemType, nonPublic: true);
		OnInstanceCreated(obj);
		return obj;
	}

	protected virtual void DestroyInstance(object instance)
	{
		OnDestroyingInstance(instance);
		if (instance is IDisposable)
		{
			((IDisposable)instance).Dispose();
		}
		instance = null;
	}

	protected virtual void OnDestroyingInstance(object instance)
	{
		if (this.DestroyingInstance != null)
		{
			this.DestroyingInstance(this, instance);
		}
	}

	protected virtual void OnInstanceCreated(object instance)
	{
		if (this.InstanceCreated != null)
		{
			this.InstanceCreated(this, instance);
		}
	}

	protected virtual void OnItemRemoved(object item)
	{
		if (this.ItemRemoved != null)
		{
			this.ItemRemoved(this, item);
		}
	}

	protected virtual void OnItemAdded(object Item)
	{
		if (this.ItemAdded != null)
		{
			this.ItemAdded(this, Item);
		}
	}

	private void MoveItem(IList list, int index, int step)
	{
		if (index > -1 && index < list.Count && index + step > -1 && index + step < list.Count)
		{
			int index2 = index + step;
			object value = list[index2];
			list[index2] = list[index];
			list[index] = value;
			value = null;
		}
	}

	protected internal TItem[] GenerateTItemArray(IList collection)
	{
		TItem[] array = new TItem[0];
		if (collection != null && collection.Count > 0)
		{
			array = new TItem[collection.Count];
			for (int i = 0; i < collection.Count; i++)
			{
				array[i] = CreateTItem(collection[i]);
			}
		}
		return array;
	}

	protected virtual TItem CreateTItem(object reffObject)
	{
		TItem tItem = new TItem(this, reffObject);
		SetProperties(tItem, reffObject);
		return tItem;
	}

	protected virtual void SetProperties(TItem titem, object reffObject)
	{
		PropertyInfo property = titem.Value.GetType().GetProperty("Name");
		if (property != null)
		{
			titem.Text = property.GetValue(titem.Value, null).ToString();
		}
		else
		{
			titem.Text = titem.Value.ToString();
		}
	}

	protected virtual void RefreshValues()
	{
		tv_Items.BeginUpdate();
		tv_Items.Nodes.Clear();
		tv_Items.Nodes.AddRange(GenerateTItemArray(Collection));
		tv_Items.EndUpdate();
	}

	protected virtual EditLevel SetEditLevel(IList collection)
	{
		return EditLevel.FullEdit;
	}

	private void SetCollectionEditLevel(IList collection)
	{
		switch (SetEditLevel(collection))
		{
		case EditLevel.FullEdit:
			btn_Remove.Enabled = Remove_CanEnable();
			btn_Add.Enabled = Add_CanEnable();
			break;
		case EditLevel.AddOnly:
		{
			Button button4 = btn_Remove;
			Remove_CanEnable();
			button4.Enabled = false;
			btn_Add.Enabled = Add_CanEnable();
			break;
		}
		case EditLevel.RemoveOnly:
		{
			Button button3 = btn_Add;
			Add_CanEnable();
			button3.Enabled = false;
			btn_Remove.Enabled = Remove_CanEnable();
			break;
		}
		case EditLevel.ReadOnly:
		{
			Button button = btn_Remove;
			Remove_CanEnable();
			button.Enabled = false;
			Button button2 = btn_Add;
			Add_CanEnable();
			button2.Enabled = false;
			break;
		}
		}
	}

	private bool Add_CanEnable()
	{
		if (EditLevel == EditLevel.FullEdit || EditLevel == EditLevel.AddOnly)
		{
			return true;
		}
		return false;
	}

	private bool Remove_CanEnable()
	{
		if (EditLevel == EditLevel.FullEdit || EditLevel == EditLevel.RemoveOnly)
		{
			return true;
		}
		return false;
	}

	protected virtual void RefreshAvailableTypes(IList collection)
	{
	}

	private void ProccessCollection(IList collection)
	{
		SetCollectionEditLevel(collection);
	}

	private void btn_OK_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btn_Cancel_Click(object sender, EventArgs e)
	{
		UndoChanges(backupList, Collection);
		Close();
	}

	private void btn_Remove_Click(object sender, EventArgs e)
	{
		tv_Items.BeginUpdate();
		TItem tItem = (TItem)tv_Items.SelectedNode;
		if (tItem != null)
		{
			int index = tItem.Index;
			TItem tItem2 = (TItem)tItem.Parent;
			if (tItem2 != null)
			{
				tItem2.Nodes.Remove(tItem);
				tItem2.SubItems.Remove(tItem.Value);
				if (tItem2.Nodes.Count > index)
				{
					tv_Items.SelectedNode = tItem2.Nodes[index];
				}
				else if (tItem2.Nodes.Count > 0)
				{
					tv_Items.SelectedNode = tItem2.Nodes[index - 1];
				}
				else
				{
					tv_Items.SelectedNode = tItem2;
				}
			}
			else
			{
				tv_Items.Nodes.Remove(tItem);
				Collection.Remove(tItem.Value);
				if (tv_Items.Nodes.Count > index)
				{
					tv_Items.SelectedNode = tv_Items.Nodes[index];
				}
				else if (tv_Items.Nodes.Count > 0)
				{
					tv_Items.SelectedNode = tv_Items.Nodes[index - 1];
				}
				else
				{
					pg_PropGrid.SelectedObject = null;
				}
			}
			OnItemRemoved(tItem.Value);
		}
		tv_Items.EndUpdate();
	}

	private void btn_Up_Click(object sender, EventArgs e)
	{
		tv_Items.BeginUpdate();
		TItem tItem = (TItem)tv_Items.SelectedNode;
		if (tItem != null && tItem.PrevNode != null)
		{
			int index = tItem.PrevNode.Index;
			TItem tItem2 = (TItem)tItem.Parent;
			if (tItem2 != null)
			{
				MoveItem(tItem2.SubItems, tItem2.SubItems.IndexOf(tItem.Value), -1);
				SetProperties(tItem2, tItem2.Value);
				tv_Items.SelectedNode = tItem2.Nodes[index];
			}
			else
			{
				MoveItem(Collection, Collection.IndexOf(tItem.Value), -1);
				tv_Items.Nodes.Clear();
				tv_Items.Nodes.AddRange(GenerateTItemArray(Collection));
				tv_Items.SelectedNode = tv_Items.Nodes[index];
			}
		}
		tv_Items.EndUpdate();
	}

	private void btn_Down_Click(object sender, EventArgs e)
	{
		tv_Items.BeginUpdate();
		TItem tItem = (TItem)tv_Items.SelectedNode;
		if (tItem != null && tItem.NextNode != null)
		{
			int index = tItem.NextNode.Index;
			TItem tItem2 = (TItem)tItem.Parent;
			if (tItem2 != null)
			{
				MoveItem(tItem2.SubItems, tItem2.SubItems.IndexOf(tItem.Value), 1);
				SetProperties(tItem2, tItem2.Value);
				tv_Items.SelectedNode = tItem2.Nodes[index];
			}
			else
			{
				MoveItem(Collection, Collection.IndexOf(tItem.Value), 1);
				tv_Items.Nodes.Clear();
				tv_Items.Nodes.AddRange(GenerateTItemArray(Collection));
				tv_Items.SelectedNode = tv_Items.Nodes[index];
			}
		}
		tv_Items.EndUpdate();
	}

	private void pg_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
	{
		tv_Items.BeginUpdate();
		TItem tItem = (TItem)tv_Items.SelectedNode;
		SetProperties(tItem, tItem.Value);
		tv_Items.EndUpdate();
	}

	private void tv_Items_BeforeSelect(object sender, TreeViewCancelEventArgs e)
	{
		pg_PropGrid.SelectedObject = ((TItem)e.Node).Value;
	}

	private void tv_Items_AfterSelect(object sender, TreeViewEventArgs e)
	{
		TItem tItem = (TItem)e.Node;
		if (tItem.Value.GetType() != lastItemType)
		{
			lastItemType = tItem.Value.GetType();
			IList collection = ((tItem.Parent == null) ? Collection : ((TItem)tItem.Parent).SubItems);
			ProccessCollection(collection);
		}
	}

	private void pg_PropGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
	{
		if (attachedEditor != null)
		{
			attachedEditor.CollectionChanged -= ValChanged;
			attachedEditor = null;
		}
		if (e.NewSelection.Value is IList)
		{
			attachedEditor = (CustomCollectionEditor)e.NewSelection.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
			if (attachedEditor != null)
			{
				attachedEditor.CollectionChanged += ValChanged;
			}
		}
	}

	private void ValChanged(object sender, object instance, object value)
	{
		tv_Items.BeginUpdate();
		TItem titem = (TItem)tv_Items.SelectedNode;
		SetProperties(titem, instance);
		tv_Items.EndUpdate();
	}

	private void UndoChanges(IList source, IList dest)
	{
		foreach (object item in dest)
		{
			if (!source.Contains(item))
			{
				DestroyInstance(item);
				OnItemRemoved(item);
			}
		}
		dest.Clear();
		CopyItems(source, dest);
	}

	private void CopyItems(IList source, IList dest)
	{
		foreach (object item in source)
		{
			dest.Add(item);
			OnItemAdded(item);
		}
	}

	protected virtual void OnEditLevelChanged(EventArgs e)
	{
		switch (EditLevel)
		{
		case EditLevel.FullEdit:
			btn_Add.Enabled = true;
			btn_Remove.Enabled = true;
			break;
		case EditLevel.AddOnly:
			btn_Add.Enabled = true;
			btn_Remove.Enabled = false;
			break;
		case EditLevel.RemoveOnly:
			btn_Add.Enabled = false;
			btn_Remove.Enabled = true;
			break;
		case EditLevel.ReadOnly:
			btn_Add.Enabled = false;
			btn_Remove.Enabled = false;
			break;
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		spl_Splitter.SplitPosition = 240;
	}

	private void btn_Add_Click(object sender, EventArgs e)
	{
		tv_Items.BeginUpdate();
		if (Collection != null)
		{
			Type itemType = GetItemType(Collection);
			object obj = CreateInstance(itemType);
			TItem tItem = CreateTItem(obj);
			TItem tItem2 = (TItem)tv_Items.SelectedNode;
			if (tItem2 != null)
			{
				int index = tItem2.Index + 1;
				IList list;
				TreeNodeCollection nodes;
				if (tItem2.Parent != null)
				{
					list = ((TItem)tItem2.Parent).SubItems;
					nodes = tItem2.Parent.Nodes;
				}
				else
				{
					list = Collection;
					nodes = tv_Items.Nodes;
				}
				list.Insert(index, obj);
				nodes.Insert(index, tItem);
			}
			else
			{
				Collection.Add(obj);
				tv_Items.Nodes.Add(tItem);
			}
			OnItemAdded(obj);
			tv_Items.SelectedNode = tItem;
		}
		tv_Items.EndUpdate();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.pg_PropGrid = new System.Windows.Forms.PropertyGrid();
		this.btn_Add = new System.Windows.Forms.Button();
		this.btn_Remove = new System.Windows.Forms.Button();
		this.btn_Up = new System.Windows.Forms.Button();
		this.btn_Down = new System.Windows.Forms.Button();
		this.pan_Items = new System.Windows.Forms.Panel();
		this.tv_Items = new System.Windows.Forms.TreeView();
		this.pan_MainPan = new System.Windows.Forms.Panel();
		this.spl_Splitter = new System.Windows.Forms.Splitter();
		this.pan_PropGridPan = new System.Windows.Forms.Panel();
		this.pan_ButtonsPan = new System.Windows.Forms.Panel();
		this.btn_Cancel = new System.Windows.Forms.Button();
		this.btn_OK = new System.Windows.Forms.Button();
		this.pan_Items.SuspendLayout();
		this.pan_MainPan.SuspendLayout();
		this.pan_PropGridPan.SuspendLayout();
		this.pan_ButtonsPan.SuspendLayout();
		base.SuspendLayout();
		this.pg_PropGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pg_PropGrid.BackColor = System.Drawing.SystemColors.Control;
		this.pg_PropGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
		this.pg_PropGrid.Location = new System.Drawing.Point(7, 6);
		this.pg_PropGrid.Name = "pg_PropGrid";
		this.pg_PropGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
		this.pg_PropGrid.Size = new System.Drawing.Size(224, 341);
		this.pg_PropGrid.TabIndex = 3;
		this.pg_PropGrid.ToolbarVisible = false;
		this.pg_PropGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(pg_PropertyValueChanged);
		this.pg_PropGrid.SelectedGridItemChanged += new System.Windows.Forms.SelectedGridItemChangedEventHandler(pg_PropGrid_SelectedGridItemChanged);
		this.btn_Add.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btn_Add.Location = new System.Drawing.Point(8, 310);
		this.btn_Add.Name = "btn_Add";
		this.btn_Add.Size = new System.Drawing.Size(80, 38);
		this.btn_Add.TabIndex = 4;
		this.btn_Add.Text = "Add";
		this.btn_Add.Click += new System.EventHandler(btn_Add_Click);
		this.btn_Remove.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btn_Remove.Location = new System.Drawing.Point(108, 310);
		this.btn_Remove.Name = "btn_Remove";
		this.btn_Remove.Size = new System.Drawing.Size(80, 38);
		this.btn_Remove.TabIndex = 6;
		this.btn_Remove.Text = "Remove";
		this.btn_Remove.Click += new System.EventHandler(btn_Remove_Click);
		this.btn_Up.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btn_Up.Image = CommonSources.Properties.Resources.arrowup;
		this.btn_Up.Location = new System.Drawing.Point(196, 8);
		this.btn_Up.Name = "btn_Up";
		this.btn_Up.Size = new System.Drawing.Size(23, 32);
		this.btn_Up.TabIndex = 1;
		this.btn_Up.Click += new System.EventHandler(btn_Up_Click);
		this.btn_Down.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btn_Down.Image = CommonSources.Properties.Resources.arrowdown;
		this.btn_Down.Location = new System.Drawing.Point(196, 48);
		this.btn_Down.Name = "btn_Down";
		this.btn_Down.Size = new System.Drawing.Size(23, 32);
		this.btn_Down.TabIndex = 2;
		this.btn_Down.Click += new System.EventHandler(btn_Down_Click);
		this.pan_Items.Controls.Add(this.tv_Items);
		this.pan_Items.Controls.Add(this.btn_Down);
		this.pan_Items.Controls.Add(this.btn_Remove);
		this.pan_Items.Controls.Add(this.btn_Add);
		this.pan_Items.Controls.Add(this.btn_Up);
		this.pan_Items.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pan_Items.Location = new System.Drawing.Point(0, 0);
		this.pan_Items.Name = "pan_Items";
		this.pan_Items.Size = new System.Drawing.Size(228, 355);
		this.pan_Items.TabIndex = 9;
		this.tv_Items.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tv_Items.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.tv_Items.FullRowSelect = true;
		this.tv_Items.HideSelection = false;
		this.tv_Items.Indent = 25;
		this.tv_Items.Location = new System.Drawing.Point(7, 7);
		this.tv_Items.Name = "tv_Items";
		this.tv_Items.Size = new System.Drawing.Size(182, 297);
		this.tv_Items.TabIndex = 0;
		this.tv_Items.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(tv_Items_BeforeSelect);
		this.tv_Items.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(tv_Items_AfterSelect);
		this.pan_MainPan.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pan_MainPan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pan_MainPan.Controls.Add(this.spl_Splitter);
		this.pan_MainPan.Controls.Add(this.pan_Items);
		this.pan_MainPan.Controls.Add(this.pan_PropGridPan);
		this.pan_MainPan.Location = new System.Drawing.Point(6, 6);
		this.pan_MainPan.Name = "pan_MainPan";
		this.pan_MainPan.Size = new System.Drawing.Size(468, 357);
		this.pan_MainPan.TabIndex = 11;
		this.spl_Splitter.BackColor = System.Drawing.SystemColors.ControlDark;
		this.spl_Splitter.Dock = System.Windows.Forms.DockStyle.Right;
		this.spl_Splitter.Location = new System.Drawing.Point(226, 0);
		this.spl_Splitter.MinExtra = 216;
		this.spl_Splitter.MinSize = 208;
		this.spl_Splitter.Name = "spl_Splitter";
		this.spl_Splitter.Size = new System.Drawing.Size(2, 355);
		this.spl_Splitter.TabIndex = 10;
		this.spl_Splitter.TabStop = false;
		this.pan_PropGridPan.Controls.Add(this.pg_PropGrid);
		this.pan_PropGridPan.Dock = System.Windows.Forms.DockStyle.Right;
		this.pan_PropGridPan.Location = new System.Drawing.Point(228, 0);
		this.pan_PropGridPan.Name = "pan_PropGridPan";
		this.pan_PropGridPan.Size = new System.Drawing.Size(238, 355);
		this.pan_PropGridPan.TabIndex = 10;
		this.pan_ButtonsPan.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pan_ButtonsPan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.pan_ButtonsPan.Controls.Add(this.btn_Cancel);
		this.pan_ButtonsPan.Controls.Add(this.btn_OK);
		this.pan_ButtonsPan.Location = new System.Drawing.Point(6, 369);
		this.pan_ButtonsPan.Name = "pan_ButtonsPan";
		this.pan_ButtonsPan.Size = new System.Drawing.Size(468, 57);
		this.pan_ButtonsPan.TabIndex = 12;
		this.btn_Cancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btn_Cancel.Location = new System.Drawing.Point(361, 7);
		this.btn_Cancel.Name = "btn_Cancel";
		this.btn_Cancel.Size = new System.Drawing.Size(98, 42);
		this.btn_Cancel.TabIndex = 7;
		this.btn_Cancel.Text = "Cancel";
		this.btn_Cancel.Click += new System.EventHandler(btn_Cancel_Click);
		this.btn_OK.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btn_OK.Location = new System.Drawing.Point(247, 7);
		this.btn_OK.Name = "btn_OK";
		this.btn_OK.Size = new System.Drawing.Size(98, 42);
		this.btn_OK.TabIndex = 6;
		this.btn_OK.Text = "Ok";
		this.btn_OK.Click += new System.EventHandler(btn_OK_Click);
		base.AcceptButton = this.btn_OK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.BackColor = System.Drawing.SystemColors.Control;
		base.CancelButton = this.btn_Cancel;
		base.ClientSize = new System.Drawing.Size(480, 432);
		base.ControlBox = false;
		base.Controls.Add(this.pan_ButtonsPan);
		base.Controls.Add(this.pan_MainPan);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		this.MinimumSize = new System.Drawing.Size(480, 300);
		base.Name = "CustomCollectionEditorForm";
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "CustomCollectionEditor";
		base.TopMost = true;
		this.pan_Items.ResumeLayout(false);
		this.pan_MainPan.ResumeLayout(false);
		this.pan_PropGridPan.ResumeLayout(false);
		this.pan_ButtonsPan.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
