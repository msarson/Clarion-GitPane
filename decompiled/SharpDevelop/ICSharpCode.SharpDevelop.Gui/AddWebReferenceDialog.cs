using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Win32;

namespace ICSharpCode.SharpDevelop.Gui;

public class AddWebReferenceDialog : Form
{
	private delegate DiscoveryDocument DiscoverAnyAsync(string url);

	private delegate void DiscoveredWebServicesHandler(DiscoveryClientProtocol protocol);

	private delegate void AuthenticationHandler(Uri uri, string authenticationType);

	private WebServiceDiscoveryClientProtocol discoveryClientProtocol;

	private CredentialCache credentialCache = new CredentialCache();

	private int initialFormWidth;

	private int initialUrlComboBoxWidth;

	private string namespacePrefix = string.Empty;

	private Uri discoveryUri;

	private IProject project;

	private WebReference webReference;

	private Button cancelButton;

	private Button addButton;

	private TextBox referenceNameTextBox;

	private Label referenceNameLabel;

	private TabPage webBrowserTabPage;

	private TabPage webServicesTabPage;

	private ToolStrip toolStrip;

	private WebBrowser webBrowser;

	private TabControl tabControl;

	private ToolStripButton goButton;

	private ToolStripComboBox urlComboBox;

	private ToolStripButton stopButton;

	private ToolStripButton refreshButton;

	private ToolStripButton forwardButton;

	private ToolStripButton backButton;

	private WebServicesView webServicesView;

	public string NamespacePrefix
	{
		get
		{
			return namespacePrefix;
		}
		set
		{
			namespacePrefix = value;
		}
	}

	public WebReference WebReference => webReference;

	private bool IsValidReferenceName
	{
		get
		{
			if (referenceNameTextBox.Text.Length > 0 && referenceNameTextBox.Text.IndexOf('\\') == -1 && !ContainsInvalidDirectoryChar(referenceNameTextBox.Text))
			{
				return true;
			}
			return false;
		}
	}

	public AddWebReferenceDialog(IProject project)
	{
		InitializeComponent();
		AddMruList();
		AddImages();
		AddStringResources();
		initialFormWidth = base.Width;
		initialUrlComboBoxWidth = urlComboBox.Width;
		this.project = project;
	}

	private void InitializeComponent()
	{
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.backButton = new System.Windows.Forms.ToolStripButton();
		this.forwardButton = new System.Windows.Forms.ToolStripButton();
		this.refreshButton = new System.Windows.Forms.ToolStripButton();
		this.stopButton = new System.Windows.Forms.ToolStripButton();
		this.urlComboBox = new System.Windows.Forms.ToolStripComboBox();
		this.goButton = new System.Windows.Forms.ToolStripButton();
		this.tabControl = new System.Windows.Forms.TabControl();
		this.webBrowserTabPage = new System.Windows.Forms.TabPage();
		this.webBrowser = new System.Windows.Forms.WebBrowser();
		this.webServicesTabPage = new System.Windows.Forms.TabPage();
		this.webServicesView = new ICSharpCode.SharpDevelop.Gui.WebServicesView();
		this.referenceNameLabel = new System.Windows.Forms.Label();
		this.referenceNameTextBox = new System.Windows.Forms.TextBox();
		this.addButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.toolStrip.SuspendLayout();
		this.tabControl.SuspendLayout();
		this.webBrowserTabPage.SuspendLayout();
		this.webServicesTabPage.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.backButton, this.forwardButton, this.refreshButton, this.stopButton, this.urlComboBox, this.goButton });
		this.toolStrip.Location = new System.Drawing.Point(0, 0);
		this.toolStrip.Name = "toolStrip";
		this.toolStrip.Size = new System.Drawing.Size(668, 26);
		this.toolStrip.Stretch = true;
		this.toolStrip.TabIndex = 0;
		this.toolStrip.Text = "toolStrip";
		this.toolStrip.Leave += new System.EventHandler(ToolStripLeave);
		this.toolStrip.Enter += new System.EventHandler(ToolStripEnter);
		this.toolStrip.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(ToolStripPreviewKeyDown);
		this.backButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.backButton.Enabled = false;
		this.backButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.backButton.Name = "backButton";
		this.backButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
		this.backButton.Size = new System.Drawing.Size(23, 23);
		this.backButton.Text = "Back";
		this.backButton.Click += new System.EventHandler(BackButtonClick);
		this.forwardButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.forwardButton.Enabled = false;
		this.forwardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.forwardButton.Name = "forwardButton";
		this.forwardButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
		this.forwardButton.Size = new System.Drawing.Size(23, 23);
		this.forwardButton.Text = "forward";
		this.forwardButton.Click += new System.EventHandler(ForwardButtonClick);
		this.refreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.refreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.refreshButton.Name = "refreshButton";
		this.refreshButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
		this.refreshButton.Size = new System.Drawing.Size(23, 23);
		this.refreshButton.Text = "Refresh";
		this.refreshButton.Click += new System.EventHandler(RefreshButtonClick);
		this.stopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.stopButton.Enabled = false;
		this.stopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.stopButton.Name = "stopButton";
		this.stopButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
		this.stopButton.Size = new System.Drawing.Size(23, 23);
		this.stopButton.Text = "Stop";
		this.stopButton.ToolTipText = "Stop";
		this.stopButton.Click += new System.EventHandler(StopButtonClick);
		this.urlComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
		this.urlComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
		this.urlComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
		this.urlComboBox.Name = "urlComboBox";
		this.urlComboBox.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
		this.urlComboBox.Size = new System.Drawing.Size(480, 26);
		this.urlComboBox.SelectedIndexChanged += new System.EventHandler(UrlComboBoxSelectedIndexChanged);
		this.urlComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(UrlComboBoxKeyDown);
		this.goButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.goButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.goButton.Name = "goButton";
		this.goButton.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
		this.goButton.Size = new System.Drawing.Size(23, 23);
		this.goButton.Text = "Open";
		this.goButton.ToolTipText = "Open";
		this.goButton.Click += new System.EventHandler(GoButtonClick);
		this.tabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tabControl.Controls.Add(this.webBrowserTabPage);
		this.tabControl.Controls.Add(this.webServicesTabPage);
		this.tabControl.Location = new System.Drawing.Point(0, 31);
		this.tabControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.tabControl.Name = "tabControl";
		this.tabControl.SelectedIndex = 0;
		this.tabControl.Size = new System.Drawing.Size(668, 176);
		this.tabControl.TabIndex = 1;
		this.webBrowserTabPage.Controls.Add(this.webBrowser);
		this.webBrowserTabPage.Location = new System.Drawing.Point(4, 25);
		this.webBrowserTabPage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.webBrowserTabPage.Name = "webBrowserTabPage";
		this.webBrowserTabPage.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.webBrowserTabPage.Size = new System.Drawing.Size(660, 147);
		this.webBrowserTabPage.TabIndex = 0;
		this.webBrowserTabPage.Text = "WSDL";
		this.webBrowserTabPage.UseVisualStyleBackColor = true;
		this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
		this.webBrowser.Location = new System.Drawing.Point(4, 4);
		this.webBrowser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.webBrowser.MinimumSize = new System.Drawing.Size(27, 25);
		this.webBrowser.Name = "webBrowser";
		this.webBrowser.Size = new System.Drawing.Size(652, 139);
		this.webBrowser.TabIndex = 0;
		this.webBrowser.TabStop = false;
		this.webBrowser.CanGoForwardChanged += new System.EventHandler(WebBrowserCanGoForwardChanged);
		this.webBrowser.CanGoBackChanged += new System.EventHandler(WebBrowserCanGoBackChanged);
		this.webBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(WebBrowserNavigating);
		this.webBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(WebBrowserNavigated);
		this.webServicesTabPage.Controls.Add(this.webServicesView);
		this.webServicesTabPage.Location = new System.Drawing.Point(4, 25);
		this.webServicesTabPage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.webServicesTabPage.Name = "webServicesTabPage";
		this.webServicesTabPage.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.webServicesTabPage.Size = new System.Drawing.Size(660, 147);
		this.webServicesTabPage.TabIndex = 1;
		this.webServicesTabPage.Text = "Available Web Services";
		this.webServicesTabPage.UseVisualStyleBackColor = true;
		this.webServicesView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.webServicesView.Location = new System.Drawing.Point(4, 4);
		this.webServicesView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.webServicesView.Name = "webServicesView";
		this.webServicesView.Size = new System.Drawing.Size(652, 139);
		this.webServicesView.TabIndex = 0;
		this.referenceNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.referenceNameLabel.Location = new System.Drawing.Point(12, 219);
		this.referenceNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.referenceNameLabel.Name = "referenceNameLabel";
		this.referenceNameLabel.Size = new System.Drawing.Size(171, 25);
		this.referenceNameLabel.TabIndex = 2;
		this.referenceNameLabel.Text = "&Reference Name:";
		this.referenceNameLabel.UseCompatibleTextRendering = true;
		this.referenceNameTextBox.Enabled = false;
		this.referenceNameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.referenceNameTextBox.Location = new System.Drawing.Point(169, 220);
		this.referenceNameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.referenceNameTextBox.Name = "referenceNameTextBox";
		this.referenceNameTextBox.Size = new System.Drawing.Size(387, 22);
		this.referenceNameTextBox.TabIndex = 4;
		this.addButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.addButton.Enabled = false;
		this.addButton.Location = new System.Drawing.Point(565, 220);
		this.addButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.addButton.Name = "addButton";
		this.addButton.Size = new System.Drawing.Size(97, 26);
		this.addButton.TabIndex = 6;
		this.addButton.Text = "&Add";
		this.addButton.UseCompatibleTextRendering = true;
		this.addButton.UseVisualStyleBackColor = true;
		this.addButton.Click += new System.EventHandler(AddButtonClick);
		this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(565, 247);
		this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(97, 26);
		this.cancelButton.TabIndex = 7;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.UseCompatibleTextRendering = true;
		this.cancelButton.UseVisualStyleBackColor = true;
		this.cancelButton.Click += new System.EventHandler(CancelButtonClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(668, 288);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.addButton);
		base.Controls.Add(this.referenceNameTextBox);
		base.Controls.Add(this.referenceNameLabel);
		base.Controls.Add(this.tabControl);
		base.Controls.Add(this.toolStrip);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.MinimumSize = new System.Drawing.Size(676, 321);
		base.Name = "AddWebReferenceDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Add Web Reference";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(AddWebReferenceDialogFormClosing);
		base.Resize += new System.EventHandler(AddWebReferenceDialogResize);
		this.toolStrip.ResumeLayout(false);
		this.toolStrip.PerformLayout();
		this.tabControl.ResumeLayout(false);
		this.webBrowserTabPage.ResumeLayout(false);
		this.webServicesTabPage.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void AddMruList()
	{
		try
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\TypedURLs");
			if (registryKey != null)
			{
				string[] valueNames = registryKey.GetValueNames();
				foreach (string name in valueNames)
				{
					urlComboBox.Items.Add((string)registryKey.GetValue(name));
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void ToolStripPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyCode == Keys.Tab)
		{
			if (goButton.Selected && e.Modifiers != Keys.Shift)
			{
				toolStrip.TabStop = true;
			}
			else if (backButton.Selected && e.Modifiers == Keys.Shift)
			{
				toolStrip.TabStop = true;
			}
		}
	}

	private void ToolStripEnter(object sender, EventArgs e)
	{
		toolStrip.TabStop = false;
	}

	private void ToolStripLeave(object sender, EventArgs e)
	{
		toolStrip.TabStop = true;
	}

	private void BackButtonClick(object sender, EventArgs e)
	{
		try
		{
			webBrowser.GoBack();
		}
		catch (Exception)
		{
		}
	}

	private void ForwardButtonClick(object sender, EventArgs e)
	{
		try
		{
			webBrowser.GoForward();
		}
		catch (Exception)
		{
		}
	}

	private void StopButtonClick(object sender, EventArgs e)
	{
		webBrowser.Stop();
		StopDiscovery();
		addButton.Enabled = false;
	}

	private void RefreshButtonClick(object sender, EventArgs e)
	{
		webBrowser.Refresh();
	}

	private void GoButtonClick(object sender, EventArgs e)
	{
		BrowseUrl(urlComboBox.Text);
	}

	private void BrowseUrl(string url)
	{
		webBrowser.Focus();
		webBrowser.Navigate(url);
	}

	private void CancelButtonClick(object sender, EventArgs e)
	{
		Close();
	}

	private void WebBrowserNavigating(object sender, WebBrowserNavigatingEventArgs e)
	{
		Cursor = Cursors.WaitCursor;
		stopButton.Enabled = true;
		webServicesView.Clear();
	}

	private void WebBrowserNavigated(object sender, WebBrowserNavigatedEventArgs e)
	{
		Cursor = Cursors.Default;
		stopButton.Enabled = false;
		urlComboBox.Text = webBrowser.Url.ToString();
		StartDiscovery(e.Url);
	}

	private void WebBrowserCanGoForwardChanged(object sender, EventArgs e)
	{
		forwardButton.Enabled = webBrowser.CanGoForward;
	}

	private void WebBrowserCanGoBackChanged(object sender, EventArgs e)
	{
		backButton.Enabled = webBrowser.CanGoBack;
	}

	private string GetReferenceName()
	{
		if (discoveryUri != null)
		{
			return discoveryUri.Host;
		}
		return string.Empty;
	}

	private bool ContainsInvalidDirectoryChar(string item)
	{
		char[] invalidPathChars = Path.GetInvalidPathChars();
		foreach (char value in invalidPathChars)
		{
			if (item.IndexOf(value) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private void StartDiscovery(Uri uri)
	{
		StartDiscovery(uri, new DiscoveryNetworkCredential(CredentialCache.DefaultNetworkCredentials, "Default"));
	}

	private void StartDiscovery(Uri uri, DiscoveryNetworkCredential credential)
	{
		StopDiscovery();
		discoveryUri = uri;
		DiscoverAnyAsync discoverAnyAsync = discoveryClientProtocol.DiscoverAny;
		AsyncCallback callback = DiscoveryCompleted;
		discoveryClientProtocol.Credentials = credential;
		discoverAnyAsync.BeginInvoke(uri.AbsoluteUri, callback, new AsyncDiscoveryState(discoveryClientProtocol, uri, credential));
	}

	private void DiscoveryCompleted(IAsyncResult result)
	{
		AsyncDiscoveryState asyncDiscoveryState = (AsyncDiscoveryState)result.AsyncState;
		WebServiceDiscoveryClientProtocol protocol = asyncDiscoveryState.Protocol;
		bool flag = false;
		lock (this)
		{
			flag = object.ReferenceEquals(discoveryClientProtocol, protocol);
		}
		if (!flag)
		{
			return;
		}
		DiscoveredWebServicesHandler method = DiscoveredWebServices;
		try
		{
			DiscoverAnyAsync discoverAnyAsync = (DiscoverAnyAsync)((AsyncResult)result).AsyncDelegate;
			discoverAnyAsync.EndInvoke(result);
			if (!asyncDiscoveryState.Credential.IsDefaultAuthenticationType)
			{
				AddCredential(asyncDiscoveryState.Uri, asyncDiscoveryState.Credential);
			}
			Invoke(method, protocol);
		}
		catch (Exception exception)
		{
			if (protocol.IsAuthenticationRequired)
			{
				HttpAuthenticationHeader authenticationHeader = protocol.GetAuthenticationHeader();
				AuthenticationHandler method2 = AuthenticateUser;
				Invoke(method2, asyncDiscoveryState.Uri, authenticationHeader.AuthenticationType);
			}
			else
			{
				LoggingService.Error("DiscoveryCompleted", exception);
				object[] args = new object[1];
				Invoke(method, args);
			}
		}
	}

	private void StopDiscovery()
	{
		lock (this)
		{
			if (discoveryClientProtocol != null)
			{
				try
				{
					discoveryClientProtocol.Abort();
				}
				catch (NotImplementedException)
				{
				}
				catch (ObjectDisposedException)
				{
				}
				discoveryClientProtocol.Dispose();
			}
			discoveryClientProtocol = new WebServiceDiscoveryClientProtocol();
		}
	}

	private void AddWebReferenceDialogFormClosing(object sender, FormClosingEventArgs e)
	{
		StopDiscovery();
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		urlComboBox.Focus();
	}

	private ServiceDescriptionCollection GetServiceDescriptions(DiscoveryClientProtocol protocol)
	{
		ServiceDescriptionCollection serviceDescriptionCollection = new ServiceDescriptionCollection();
		protocol.ResolveOneLevel();
		foreach (DictionaryEntry reference in protocol.References)
		{
			if (reference.Value is ContractReference contractReference)
			{
				serviceDescriptionCollection.Add(contractReference.Contract);
			}
		}
		return serviceDescriptionCollection;
	}

	private void DiscoveredWebServices(DiscoveryClientProtocol protocol)
	{
		if (protocol != null)
		{
			addButton.Enabled = true;
			referenceNameTextBox.Enabled = true;
			referenceNameTextBox.Text = GetReferenceName();
			webServicesView.Add(GetServiceDescriptions(protocol));
			webReference = new WebReference(project, discoveryUri.AbsoluteUri, referenceNameTextBox.Text, CreateNamespaceNameFromRefName(referenceNameTextBox.Text), protocol);
		}
		else
		{
			webReference = null;
			addButton.Enabled = false;
			referenceNameTextBox.Enabled = false;
			webServicesView.Clear();
		}
	}

	private string CreateNamespaceNameFromRefName(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(namespacePrefix);
		if (text.Length > 0)
		{
			if (!string.IsNullOrEmpty(namespacePrefix))
			{
				stringBuilder.Append('.');
			}
			char c = text[0];
			if (!char.IsLetter(c) && c != '_')
			{
				stringBuilder.Append('_');
			}
			for (int i = 0; i < text.Length; i++)
			{
				c = text[i];
				if (!char.IsLetterOrDigit(c) && c != '.' && c != '_')
				{
					c = '_';
				}
				stringBuilder.Append(c);
			}
		}
		string[] array = stringBuilder.ToString().Split('.');
		stringBuilder = new StringBuilder();
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (text2.Length > 0)
			{
				stringBuilder.Append(".");
				char c2 = text2[0];
				if (!char.IsLetter(c2) && c2 != '_')
				{
					stringBuilder.Append('_');
				}
				stringBuilder.Append(text2);
			}
		}
		return stringBuilder.ToString().TrimStart('.');
	}

	private void UrlComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		BrowseUrl(urlComboBox.Text);
	}

	private void UrlComboBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return && urlComboBox.Text.Length > 0)
		{
			BrowseUrl(urlComboBox.Text);
		}
	}

	private void AddWebReferenceDialogResize(object sender, EventArgs e)
	{
		int num = base.Width - initialFormWidth;
		urlComboBox.Width = initialUrlComboBoxWidth + num;
	}

	private void AddButtonClick(object sender, EventArgs e)
	{
		try
		{
			if (!IsValidReferenceName)
			{
				MessageService.ShowError(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.InvalidReferenceNameError}"));
				return;
			}
			webReference.Name = WebReference.GetReferenceName(webReference.WebReferencesDirectory, referenceNameTextBox.Text);
			webReference.ProxyNamespace = CreateNamespaceNameFromRefName(webReference.Name);
			base.DialogResult = DialogResult.OK;
			Close();
		}
		catch (Exception ex)
		{
			MessageService.ShowError(ex);
		}
	}

	private void AddImages()
	{
		goButton.Image = ResourceService.GetBitmap("Icons.16x16.OpenFileIcon");
		refreshButton.Image = ResourceService.GetBitmap("Icons.16x16.BrowserRefresh");
		backButton.Image = ResourceService.GetBitmap("Icons.16x16.BrowserBefore");
		forwardButton.Image = ResourceService.GetBitmap("Icons.16x16.BrowserAfter");
		stopButton.Image = ResourceService.GetBitmap("Icons.16x16.BrowserCancel");
		base.Icon = ResourceService.GetIcon("Icons.16x16.WebSearchIcon");
	}

	private void AddStringResources()
	{
		Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.DialogTitle}");
		refreshButton.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.RefreshButtonTooltip}");
		refreshButton.ToolTipText = refreshButton.Text;
		backButton.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.BackButtonTooltip}");
		backButton.ToolTipText = backButton.Text;
		forwardButton.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.ForwardButtonTooltip}");
		forwardButton.ToolTipText = forwardButton.Text;
		referenceNameLabel.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.ReferenceNameLabel}");
		goButton.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.GoButtonTooltip}");
		goButton.ToolTipText = goButton.Text;
		addButton.Text = StringParser.Parse("${res:Global.AddButtonText}");
		cancelButton.Text = StringParser.Parse("${res:Global.CancelButtonText}");
		stopButton.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.StopButtonTooltip}");
		stopButton.ToolTipText = stopButton.Text;
		webServicesTabPage.Text = StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Dialogs.AddWebReferenceDialog.WebServicesTabPageTitle}");
		webServicesTabPage.ToolTipText = webServicesTabPage.Text;
	}

	private void AuthenticateUser(Uri uri, string authenticationType)
	{
		DiscoveryNetworkCredential discoveryNetworkCredential = (DiscoveryNetworkCredential)credentialCache.GetCredential(uri, authenticationType);
		if (discoveryNetworkCredential != null)
		{
			StartDiscovery(uri, discoveryNetworkCredential);
			return;
		}
		using UserCredentialsDialog userCredentialsDialog = new UserCredentialsDialog(uri.ToString(), authenticationType);
		if (DialogResult.OK == userCredentialsDialog.ShowDialog())
		{
			StartDiscovery(uri, userCredentialsDialog.Credential);
		}
	}

	private void AddCredential(Uri uri, DiscoveryNetworkCredential credential)
	{
		NetworkCredential credential2 = credentialCache.GetCredential(uri, credential.AuthenticationType);
		if (credential2 != null)
		{
			credentialCache.Remove(uri, credential.AuthenticationType);
		}
		credentialCache.Add(uri, credential.AuthenticationType, credential);
	}
}
