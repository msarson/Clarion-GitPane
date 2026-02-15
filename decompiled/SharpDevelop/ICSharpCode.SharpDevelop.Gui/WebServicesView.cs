using System;
using System.Drawing;
using System.Web.Services.Description;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class WebServicesView : UserControl
{
	private const int ServiceDescriptionImageIndex = 0;

	private const int ServiceImageIndex = 1;

	private const int PortImageIndex = 2;

	private const int OperationImageIndex = 3;

	private ColumnHeader propertyColumnHeader;

	private ColumnHeader valueColumnHeader;

	private TreeView webServicesTreeView;

	private ListView webServicesListView;

	private SplitContainer splitContainer;

	public WebServicesView()
	{
		InitializeComponent();
		AddImages();
		AddStringResources();
	}

	public void Clear()
	{
		webServicesListView.Items.Clear();
		webServicesTreeView.Nodes.Clear();
	}

	public void Add(ServiceDescriptionCollection serviceDescriptions)
	{
		if (serviceDescriptions.Count == 0)
		{
			return;
		}
		webServicesListView.BeginUpdate();
		try
		{
			foreach (ServiceDescription serviceDescription in serviceDescriptions)
			{
				Add(serviceDescription);
			}
		}
		finally
		{
			webServicesListView.EndUpdate();
		}
	}

	private void InitializeComponent()
	{
		this.splitContainer = new System.Windows.Forms.SplitContainer();
		this.webServicesTreeView = new System.Windows.Forms.TreeView();
		this.webServicesListView = new System.Windows.Forms.ListView();
		this.propertyColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.valueColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.splitContainer.Panel1.SuspendLayout();
		this.splitContainer.Panel2.SuspendLayout();
		this.splitContainer.SuspendLayout();
		base.SuspendLayout();
		this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer.Location = new System.Drawing.Point(0, 0);
		this.splitContainer.Name = "splitContainer";
		this.splitContainer.Panel1.Controls.Add(this.webServicesTreeView);
		this.splitContainer.Panel2.Controls.Add(this.webServicesListView);
		this.splitContainer.Size = new System.Drawing.Size(471, 305);
		this.splitContainer.SplitterDistance = 156;
		this.splitContainer.TabIndex = 1;
		this.webServicesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.webServicesTreeView.Location = new System.Drawing.Point(0, 0);
		this.webServicesTreeView.Name = "webServicesTreeView";
		this.webServicesTreeView.Size = new System.Drawing.Size(156, 305);
		this.webServicesTreeView.TabIndex = 0;
		this.webServicesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(WebServicesTreeViewAfterSelect);
		this.webServicesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.propertyColumnHeader, this.valueColumnHeader });
		this.webServicesListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.webServicesListView.Location = new System.Drawing.Point(0, 0);
		this.webServicesListView.Name = "webServicesListView";
		this.webServicesListView.Size = new System.Drawing.Size(311, 305);
		this.webServicesListView.TabIndex = 2;
		this.webServicesListView.UseCompatibleStateImageBehavior = false;
		this.webServicesListView.View = System.Windows.Forms.View.Details;
		this.propertyColumnHeader.Text = "Property";
		this.propertyColumnHeader.Width = 120;
		this.valueColumnHeader.Text = "Value";
		this.valueColumnHeader.Width = 191;
		base.Controls.Add(this.splitContainer);
		base.Name = "WebServicesView";
		base.Size = new System.Drawing.Size(471, 305);
		this.splitContainer.Panel1.ResumeLayout(false);
		this.splitContainer.Panel2.ResumeLayout(false);
		this.splitContainer.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	private void WebServicesTreeViewAfterSelect(object sender, TreeViewEventArgs e)
	{
		webServicesListView.Items.Clear();
		if (e.Node.Tag is ServiceDescription)
		{
			ServiceDescription serviceDescription = (ServiceDescription)e.Node.Tag;
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.RetrievalUriProperty}");
			listViewItem.SubItems.Add(serviceDescription.RetrievalUrl);
			webServicesListView.Items.Add(listViewItem);
		}
		else if (e.Node.Tag is Service)
		{
			Service service = (Service)e.Node.Tag;
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.DocumentationProperty}");
			listViewItem.SubItems.Add(service.Documentation);
			webServicesListView.Items.Add(listViewItem);
		}
		else if (e.Node.Tag is Port)
		{
			Port port = (Port)e.Node.Tag;
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.DocumentationProperty}");
			listViewItem.SubItems.Add(port.Documentation);
			webServicesListView.Items.Add(listViewItem);
			listViewItem = new ListViewItem();
			listViewItem.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.BindingProperty}");
			listViewItem.SubItems.Add(port.Binding.Name);
			webServicesListView.Items.Add(listViewItem);
			listViewItem = new ListViewItem();
			listViewItem.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.ServiceNameProperty}");
			listViewItem.SubItems.Add(port.Service.Name);
			webServicesListView.Items.Add(listViewItem);
		}
		else if (e.Node.Tag is Operation)
		{
			Operation operation = (Operation)e.Node.Tag;
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.DocumentationProperty}");
			listViewItem.SubItems.Add(operation.Documentation);
			webServicesListView.Items.Add(listViewItem);
			listViewItem = new ListViewItem();
			listViewItem.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.ParametersProperty}");
			listViewItem.SubItems.Add(operation.ParameterOrderString);
			webServicesListView.Items.Add(listViewItem);
		}
	}

	private void Add(ServiceDescription description)
	{
		TreeNode treeNode = new TreeNode(GetName(description));
		treeNode.Tag = description;
		treeNode.ImageIndex = 0;
		treeNode.SelectedImageIndex = 0;
		webServicesTreeView.Nodes.Add(treeNode);
		foreach (Service service in description.Services)
		{
			TreeNode treeNode2 = new TreeNode(service.Name);
			treeNode2.Tag = service;
			treeNode2.ImageIndex = 1;
			treeNode2.SelectedImageIndex = 1;
			treeNode.Nodes.Add(treeNode2);
			foreach (Port port in service.Ports)
			{
				TreeNode treeNode3 = new TreeNode(port.Name);
				treeNode3.Tag = port;
				treeNode3.ImageIndex = 2;
				treeNode3.SelectedImageIndex = 2;
				treeNode2.Nodes.Add(treeNode3);
				System.Web.Services.Description.Binding binding = description.Bindings[port.Binding.Name];
				if (binding == null)
				{
					continue;
				}
				PortType portType = description.PortTypes[binding.Type.Name];
				if (portType == null)
				{
					continue;
				}
				foreach (Operation operation in portType.Operations)
				{
					TreeNode treeNode4 = new TreeNode(operation.Name);
					treeNode4.Tag = operation;
					treeNode4.ImageIndex = 3;
					treeNode4.SelectedImageIndex = 3;
					treeNode3.Nodes.Add(treeNode4);
				}
			}
		}
		webServicesTreeView.ExpandAll();
	}

	private string GetName(ServiceDescription description)
	{
		if (description.Name != null)
		{
			return description.Name;
		}
		if (description.RetrievalUrl != null)
		{
			Uri uri = new Uri(description.RetrievalUrl);
			if (uri.Segments.Length > 0)
			{
				return uri.Segments[uri.Segments.Length - 1];
			}
			return uri.Host;
		}
		return string.Empty;
	}

	private void AddImages()
	{
		ImageList imageList = new ImageList();
		imageList.Images.Add(ResourceService.GetBitmap("Icons.16x16.Library"));
		imageList.Images.Add(ResourceService.GetBitmap("Icons.16x16.Interface"));
		imageList.Images.Add(ResourceService.GetBitmap("Icons.16x16.Class"));
		imageList.Images.Add(ResourceService.GetBitmap("Icons.16x16.Method"));
		webServicesTreeView.ImageList = imageList;
	}

	private void AddStringResources()
	{
		valueColumnHeader.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.ValueColumnHeader}");
		propertyColumnHeader.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.PropertyColumnHeader}");
	}
}
