using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Xml.Schema;
using System.Xml.Serialization;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui;

public class WebReference
{
	private List<ProjectItem> items;

	private string url = string.Empty;

	private string relativePath = string.Empty;

	private DiscoveryClientProtocol protocol;

	private IProject project;

	private string webReferencesDirectory = string.Empty;

	private string proxyNamespace = string.Empty;

	private string name = string.Empty;

	private WebReferenceUrl webReferenceUrl;

	public WebReferencesProjectItem WebReferencesProjectItem => GetWebReferencesProjectItem(Items);

	public WebReferenceUrl WebReferenceUrl
	{
		get
		{
			if (webReferenceUrl == null)
			{
				items = CreateProjectItems();
			}
			return webReferenceUrl;
		}
	}

	public string WebReferencesDirectory => webReferencesDirectory;

	public string Directory => Path.Combine(project.Directory, relativePath);

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
			OnNameChanged();
		}
	}

	public string ProxyNamespace
	{
		get
		{
			return proxyNamespace;
		}
		set
		{
			proxyNamespace = value;
		}
	}

	public List<ProjectItem> Items
	{
		get
		{
			if (items == null)
			{
				items = CreateProjectItems();
			}
			return items;
		}
	}

	public string WebProxyFileName => GetFullProxyFileName();

	public WebReference(IProject project, string url, string name, string proxyNamespace, DiscoveryClientProtocol protocol)
	{
		this.project = project;
		this.url = url;
		this.protocol = protocol;
		this.proxyNamespace = proxyNamespace;
		this.name = name;
		GetRelativePath();
	}

	public static bool ProjectContainsWebReferencesFolder(IProject project)
	{
		return GetWebReferencesProjectItem(project) != null;
	}

	public static bool ProjectContainsWebServicesReference(IProject project)
	{
		foreach (ProjectItem item in project.Items)
		{
			if (item.ItemType == ItemType.Reference && item.Include != null && item.Include.Trim().StartsWith("System.Web.Services", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public static WebReferencesProjectItem GetWebReferencesProjectItem(IProject project)
	{
		return GetWebReferencesProjectItem(project.Items);
	}

	public static string GetReferenceName(string webReferenceFolder, string name)
	{
		int num = 1;
		string text = name;
		string path = Path.Combine(webReferenceFolder, name);
		while (System.IO.Directory.Exists(path))
		{
			text = name + num;
			path = Path.Combine(webReferenceFolder, text);
			num++;
		}
		return text;
	}

	public static List<ProjectItem> GetFileItems(IProject project, string name)
	{
		List<ProjectItem> list = new List<ProjectItem>();
		WebReferencesProjectItem webReferencesProjectItem = GetWebReferencesProjectItem(project);
		if (webReferencesProjectItem != null)
		{
			string baseDirectory = Path.Combine(Path.Combine(project.Directory, webReferencesProjectItem.Include), name);
			foreach (ProjectItem item in project.Items)
			{
				if (item is FileProjectItem fileProjectItem && FileUtility.IsBaseDirectory(baseDirectory, fileProjectItem.FileName))
				{
					list.Add(fileProjectItem);
				}
			}
		}
		return list;
	}

	public WebReferenceChanges GetChanges(IProject project)
	{
		WebReferenceChanges webReferenceChanges = new WebReferenceChanges();
		List<ProjectItem> fileItems = GetFileItems(project, name);
		webReferenceChanges.NewItems.AddRange(GetNewItems(fileItems));
		webReferenceChanges.ItemsRemoved.AddRange(GetRemovedItems(fileItems));
		return webReferenceChanges;
	}

	public void Save()
	{
		System.IO.Directory.CreateDirectory(Directory);
		GenerateWebProxy();
		protocol.WriteAll(Directory, "Reference.map");
	}

	private ServiceDescriptionCollection GetServiceDescriptionCollection(DiscoveryClientProtocol protocol)
	{
		ServiceDescriptionCollection serviceDescriptionCollection = new ServiceDescriptionCollection();
		foreach (DictionaryEntry reference in protocol.References)
		{
			ContractReference contractReference = reference.Value as ContractReference;
			_ = reference.Value;
			if (contractReference != null)
			{
				serviceDescriptionCollection.Add(contractReference.Contract);
			}
		}
		return serviceDescriptionCollection;
	}

	private XmlSchemas GetXmlSchemas(DiscoveryClientProtocol protocol)
	{
		XmlSchemas xmlSchemas = new XmlSchemas();
		foreach (DictionaryEntry reference in protocol.References)
		{
			if (reference.Value is SchemaReference schemaReference)
			{
				xmlSchemas.Add(schemaReference.Schema);
			}
		}
		return xmlSchemas;
	}

	private void GenerateWebProxy()
	{
		GenerateWebProxy(proxyNamespace, GetFullProxyFileName(), GetServiceDescriptionCollection(protocol), GetXmlSchemas(protocol));
	}

	private static void GenerateWebProxy(string proxyNamespace, string fileName, ServiceDescriptionCollection serviceDescriptions, XmlSchemas schemas)
	{
		ServiceDescriptionImporter serviceDescriptionImporter = new ServiceDescriptionImporter();
		foreach (ServiceDescription serviceDescription in serviceDescriptions)
		{
			serviceDescriptionImporter.AddServiceDescription(serviceDescription, null, null);
		}
		foreach (XmlSchema schema in schemas)
		{
			serviceDescriptionImporter.Schemas.Add(schema);
		}
		CodeNamespace codeNamespace = new CodeNamespace(proxyNamespace);
		CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
		codeCompileUnit.Namespaces.Add(codeNamespace);
		serviceDescriptionImporter.Import(codeNamespace, codeCompileUnit);
		CodeDomProvider codeDomProvider = null;
		IParser parser = ParserService.GetParser(fileName);
		if (parser != null)
		{
			codeDomProvider = parser.Language.CodeDomProvider;
		}
		if (codeDomProvider != null)
		{
			StreamWriter streamWriter = new StreamWriter(fileName);
			CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
			codeGeneratorOptions.BracingStyle = "C";
			codeDomProvider.GenerateCodeFromCompileUnit(codeCompileUnit, streamWriter, codeGeneratorOptions);
			streamWriter.Close();
		}
	}

	private string GetFullProxyFileName()
	{
		return Path.Combine(project.Directory, GetProxyFileName());
	}

	private string GetProxyFileName()
	{
		string path = "Reference" + GetProxyFileNameExtension(project.Language);
		return Path.Combine(relativePath, path);
	}

	private string GetProxyFileNameExtension(string language)
	{
		LanguageBindingDescriptor codonPerLanguageName = LanguageBindingService.GetCodonPerLanguageName(language);
		if (codonPerLanguageName != null)
		{
			string[] codeFileExtensions = codonPerLanguageName.CodeFileExtensions;
			if (codeFileExtensions.Length > 0)
			{
				return codeFileExtensions[0];
			}
		}
		throw new NotSupportedException("Unsupported language: " + language);
	}

	private static WebReferencesProjectItem GetWebReferencesProjectItem(IEnumerable<ProjectItem> items)
	{
		foreach (ProjectItem item in items)
		{
			if (item.ItemType == ItemType.WebReferences)
			{
				return (WebReferencesProjectItem)item;
			}
		}
		return null;
	}

	private void OnNameChanged()
	{
		GetRelativePath();
		if (items != null)
		{
			items = CreateProjectItems();
		}
	}

	private void GetRelativePath()
	{
		ProjectItem webReferencesProjectItem = GetWebReferencesProjectItem(project);
		string text = ((webReferencesProjectItem == null) ? "Web References" : webReferencesProjectItem.Include.Trim('\\', '/'));
		webReferencesDirectory = Path.Combine(project.Directory, text);
		relativePath = Path.Combine(text, name);
	}

	private List<ProjectItem> CreateProjectItems()
	{
		List<ProjectItem> list = new List<ProjectItem>();
		if (!ProjectContainsWebReferencesFolder(project))
		{
			WebReferencesProjectItem webReferencesProjectItem = new WebReferencesProjectItem(project);
			webReferencesProjectItem.Include = "Web References\\";
			list.Add(webReferencesProjectItem);
		}
		webReferenceUrl = new WebReferenceUrl(project);
		webReferenceUrl.Include = url;
		webReferenceUrl.UpdateFromURL = url;
		webReferenceUrl.RelPath = relativePath;
		webReferenceUrl.Namespace = proxyNamespace;
		list.Add(webReferenceUrl);
		foreach (DictionaryEntry reference in protocol.References)
		{
			if (reference.Value is DiscoveryReference discoveryReference)
			{
				FileProjectItem fileProjectItem = new FileProjectItem(project, ItemType.None);
				fileProjectItem.Include = Path.Combine(relativePath, discoveryReference.DefaultFilename);
				list.Add(fileProjectItem);
			}
		}
		FileProjectItem fileProjectItem2 = new FileProjectItem(project, ItemType.Compile);
		fileProjectItem2.Include = GetProxyFileName();
		fileProjectItem2.SetEvaluatedMetadata("AutoGen", "True");
		fileProjectItem2.SetEvaluatedMetadata("DesignTime", "True");
		fileProjectItem2.DependentUpon = "Reference.map";
		list.Add(fileProjectItem2);
		FileProjectItem fileProjectItem3 = new FileProjectItem(project, ItemType.None);
		fileProjectItem3.Include = Path.Combine(relativePath, "Reference.map");
		fileProjectItem3.SetEvaluatedMetadata("Generator", "MSDiscoCodeGenerator");
		fileProjectItem3.SetEvaluatedMetadata("LastGenOutput", "Reference.cs");
		list.Add(fileProjectItem3);
		if (!ProjectContainsWebServicesReference(project))
		{
			ReferenceProjectItem item = new ReferenceProjectItem(project, "System.Web.Services");
			list.Add(item);
		}
		return list;
	}

	private List<ProjectItem> GetNewItems(List<ProjectItem> projectWebReferenceItems)
	{
		List<ProjectItem> list = new List<ProjectItem>();
		foreach (ProjectItem item in Items)
		{
			if (!(item is WebReferenceUrl) && !ContainsFileName(projectWebReferenceItems, item.FileName))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private List<ProjectItem> GetRemovedItems(List<ProjectItem> projectWebReferenceItems)
	{
		List<ProjectItem> list = new List<ProjectItem>();
		foreach (ProjectItem projectWebReferenceItem in projectWebReferenceItems)
		{
			if (!ContainsFileName(Items, projectWebReferenceItem.FileName))
			{
				list.Add(projectWebReferenceItem);
			}
		}
		return list;
	}

	private static bool ContainsFileName(List<ProjectItem> items, string fileName)
	{
		foreach (ProjectItem item in items)
		{
			if (FileUtility.IsEqualFileName(item.FileName, fileName))
			{
				return true;
			}
		}
		return false;
	}
}
