using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms;

public class XmlLoader
{
	private Dictionary<string, Control> controlDictionary = new Dictionary<string, Control>();

	private object customizationObject;

	private Form mainForm;

	private Hashtable tooltips = new Hashtable();

	private string acceptButtonName = string.Empty;

	private string cancelButtonName = string.Empty;

	private IStringValueFilter stringValueFilter;

	private IObjectCreator objectCreator = new DefaultObjectCreator();

	private IPropertyValueCreator propertyValueCreator;

	private static readonly Regex propertySet = new Regex("(?<Property>[\\w]+)\\s*=\\s*(?<Value>[\\w\\d]+)", RegexOptions.Compiled);

	private string acceptButton = "";

	private string cancelButton = "";

	private int num;

	public Dictionary<string, Control> ControlDictionary => controlDictionary;

	public IStringValueFilter StringValueFilter
	{
		get
		{
			return stringValueFilter;
		}
		set
		{
			stringValueFilter = value;
		}
	}

	public IObjectCreator ObjectCreator
	{
		get
		{
			return objectCreator;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			objectCreator = value;
		}
	}

	public IPropertyValueCreator PropertyValueCreator
	{
		get
		{
			return propertyValueCreator;
		}
		set
		{
			propertyValueCreator = value;
		}
	}

	public T Get<T>(string name) where T : Control
	{
		string text = name + typeof(T).Name;
		if (!ControlDictionary.ContainsKey(text))
		{
			throw new ArgumentException("Control " + text + " not found!", "name");
		}
		return ControlDictionary[text] as T;
	}

	public object CreateObjectFromFileDefinition(string fileName)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(fileName);
		XmlElement xmlElement = xmlDocument.DocumentElement;
		if (xmlDocument.DocumentElement.Attributes["version"] != null)
		{
			xmlElement = (XmlElement)xmlDocument.DocumentElement.ChildNodes[0];
		}
		customizationObject = objectCreator.CreateObject(XmlConvert.DecodeName(xmlElement.Name), xmlElement);
		SetUpObject(customizationObject, xmlElement);
		return customizationObject;
	}

	public object CreateObjectFromXmlDefinition(string xmlContent)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xmlContent);
		XmlElement xmlElement = xmlDocument.DocumentElement;
		if (xmlDocument.DocumentElement.Attributes["version"] != null)
		{
			xmlElement = (XmlElement)xmlDocument.DocumentElement.ChildNodes[0];
		}
		customizationObject = objectCreator.CreateObject(XmlConvert.DecodeName(xmlElement.Name), xmlElement);
		SetUpObject(customizationObject, xmlElement);
		return customizationObject;
	}

	public void LoadObjectFromFileDefinition(object customizationObject, string fileName)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(fileName);
		LoadObjectFromXmlDocument(customizationObject, xmlDocument);
	}

	public void LoadObjectFromStream(object customizationObject, Stream stream)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(stream);
		LoadObjectFromXmlDocument(customizationObject, xmlDocument);
	}

	public void LoadObjectFromXmlDocument(object customizationObject, XmlDocument doc)
	{
		this.customizationObject = customizationObject;
		XmlElement element = doc.DocumentElement;
		if (doc.DocumentElement.Attributes["version"] != null)
		{
			element = (XmlElement)doc.DocumentElement.ChildNodes[0];
		}
		SetUpObject(customizationObject, element);
		if (customizationObject is Form)
		{
			Form form = (Form)customizationObject;
			if (acceptButtonName != null && acceptButtonName.Length > 0)
			{
				form.AcceptButton = (Button)controlDictionary[acceptButtonName];
			}
			if (cancelButtonName != null && cancelButtonName.Length > 0)
			{
				form.CancelButton = (Button)controlDictionary[cancelButtonName];
			}
		}
		if (tooltips.Count <= 0)
		{
			return;
		}
		ToolTip toolTip = new ToolTip();
		foreach (DictionaryEntry tooltip in tooltips)
		{
			toolTip.SetToolTip((Control)tooltip.Key, tooltip.Value.ToString());
		}
	}

	public void LoadObjectFromXmlDefinition(string xmlContent)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(xmlContent);
		_ = xmlDocument.DocumentElement;
		if (xmlDocument.DocumentElement.Attributes["version"] != null)
		{
			_ = (XmlElement)xmlDocument.DocumentElement.ChildNodes[0];
		}
		SetUpObject(customizationObject, xmlDocument.DocumentElement);
	}

	private void SetUpObject(object currentObject, XmlElement element)
	{
		foreach (XmlNode childNode in element.ChildNodes)
		{
			if (childNode is XmlElement)
			{
				XmlElement el = (XmlElement)childNode;
				SetAttributes(currentObject, el);
			}
		}
		if (currentObject is Control)
		{
			((Control)currentObject).ResumeLayout(performLayout: false);
		}
	}

	private void SetValue(object o, string propertyName, string val)
	{
		try
		{
			PropertyInfo property = o.GetType().GetProperty(propertyName);
			switch (propertyName)
			{
			case "AcceptButton":
				acceptButton = val.Split(' ')[0];
				return;
			case "CancelButton":
				cancelButton = val.Split(' ')[0];
				return;
			case "ToolTip":
				tooltips[o] = val;
				return;
			}
			if (val.StartsWith("{") && val.EndsWith("}"))
			{
				val = val.Substring(1, val.Length - 2);
				object obj = null;
				if (property.CanWrite)
				{
					Type type = objectCreator.GetType(property.PropertyType.FullName);
					obj = type.Assembly.CreateInstance(property.PropertyType.FullName);
				}
				else
				{
					obj = property.GetValue(o, null);
				}
				Match match = propertySet.Match(val);
				while (match.Success)
				{
					SetValue(obj, match.Result("${Property}"), match.Result("${Value}"));
					match = match.NextMatch();
				}
				if (property.CanWrite)
				{
					property.SetValue(o, obj, null);
				}
			}
			else if (property.PropertyType.IsEnum)
			{
				property.SetValue(o, Enum.Parse(property.PropertyType, val), null);
			}
			else if (property.PropertyType == typeof(Color))
			{
				string text = val.Substring(val.IndexOf('[') + 1).Replace("]", "");
				string[] array = text.Split(',', '=');
				if (array.Length > 1)
				{
					property.SetValue(o, Color.FromArgb(int.Parse(array[1]), int.Parse(array[3]), int.Parse(array[5]), int.Parse(array[7])), null);
				}
				else
				{
					property.SetValue(o, Color.FromName(text), null);
				}
			}
			else if (val.Length > 0)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
				property.SetValue(o, converter.ConvertFromInvariantString(val), null);
			}
		}
		catch (Exception innerException)
		{
			throw new ApplicationException("error while setting property " + propertyName + " of object " + o.ToString() + " to value '" + val + "'", innerException);
		}
	}

	private void SetAttributes(object o, XmlElement el)
	{
		if (el.Name == "AcceptButton")
		{
			mainForm = (Form)o;
			acceptButtonName = el.Attributes["value"].InnerText.Split(' ')[0];
			return;
		}
		if (el.Name == "CancelButton")
		{
			mainForm = (Form)o;
			cancelButtonName = el.Attributes["value"].InnerText.Split(' ')[0];
			return;
		}
		if (el.Name == "ToolTip")
		{
			string innerText = el.Attributes["value"].InnerText;
			tooltips[o] = ((stringValueFilter != null) ? stringValueFilter.GetFilteredValue(innerText) : innerText);
			return;
		}
		if (el.Attributes["value"] != null)
		{
			string innerText2 = el.Attributes["value"].InnerText;
			try
			{
				SetValue(o, el.Name, (stringValueFilter != null) ? stringValueFilter.GetFilteredValue(innerText2) : innerText2);
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
		if (el.Attributes["event"] != null)
		{
			try
			{
				EventInfo eventInfo = o.GetType().GetEvent(el.Name);
				eventInfo.AddEventHandler(o, Delegate.CreateDelegate(eventInfo.EventHandlerType, customizationObject, el.Attributes["event"].InnerText));
				return;
			}
			catch (Exception)
			{
				return;
			}
		}
		Type type = o.GetType();
		PropertyInfo property;
		if (type.Name == "TableLayoutPanel")
		{
			property = type.GetProperty(el.Name, typeof(TableLayoutControlCollection));
			if (property == null)
			{
				property = type.GetProperty(el.Name);
			}
		}
		else
		{
			property = type.GetProperty(el.Name);
		}
		object value = property.GetValue(o, null);
		if (value is IList)
		{
			foreach (XmlNode childNode in el.ChildNodes)
			{
				if (childNode is XmlElement)
				{
					XmlElement xmlElement = (XmlElement)childNode;
					object obj = objectCreator.CreateObject(XmlConvert.DecodeName(xmlElement.Name), xmlElement);
					if (obj != null)
					{
						if (obj is IComponent)
						{
							string text = null;
							if (xmlElement["Name"] != null && xmlElement["Name"].Attributes["value"] != null)
							{
								text = xmlElement["Name"].Attributes["value"].InnerText;
							}
							if (text == null || text.Length == 0)
							{
								text = "CreatedObject" + num++;
							}
						}
						SetUpObject(obj, xmlElement);
						if (obj is Control)
						{
							string name = ((Control)obj).Name;
							if (name != null && name.Length > 0)
							{
								ControlDictionary[name] = (Control)obj;
							}
						}
						if (obj != null)
						{
							if (value is TableLayoutControlCollection)
							{
								Control control = (Control)obj;
								TableLayoutControlCollection tableLayoutControlCollection = (TableLayoutControlCollection)value;
								TableLayoutPanel tableLayoutPanel = (TableLayoutPanel)o;
								int row = 0;
								bool flag = false;
								if (xmlElement["Row"] != null && xmlElement["Row"].Attributes["value"] != null)
								{
									flag = true;
									row = int.Parse(xmlElement["Row"].Attributes["value"].InnerText);
								}
								int column = 0;
								bool flag2 = false;
								if (xmlElement["Column"] != null && xmlElement["Column"].Attributes["value"] != null)
								{
									flag2 = true;
									column = int.Parse(xmlElement["Column"].Attributes["value"].InnerText);
								}
								if ((!flag || !flag2) && xmlElement["CellPosition"] != null && xmlElement["CellPosition"].Attributes["value"] != null)
								{
									string innerText3 = xmlElement["CellPosition"].Attributes["value"].InnerText;
									string[] array = innerText3.Split(',');
									if (!flag2)
									{
										column = int.Parse(array[0]);
									}
									if (!flag)
									{
										row = int.Parse(array[1]);
									}
								}
								tableLayoutControlCollection.Add(control, column, row);
								if (xmlElement["ColumnSpan"] != null && xmlElement["ColumnSpan"].Attributes["value"] != null)
								{
									tableLayoutPanel.SetColumnSpan(control, int.Parse(xmlElement["ColumnSpan"].Attributes["value"].InnerText));
								}
								if (xmlElement["RowSpan"] != null && xmlElement["RowSpan"].Attributes["value"] != null)
								{
									tableLayoutPanel.SetRowSpan(control, int.Parse(xmlElement["RowSpan"].Attributes["value"].InnerText));
								}
							}
							else
							{
								((IList)value).Add(obj);
							}
						}
					}
				}
			}
			return;
		}
		object obj2 = objectCreator.CreateObject(o.GetType().GetProperty(el.Name).PropertyType.Name, el);
		if (obj2 is IComponent)
		{
			obj2.GetType().GetProperty("Name");
			string text2 = null;
			if (el["Name"] != null && el["Name"].Attributes["value"] != null)
			{
				text2 = el["Name"].Attributes["value"].InnerText;
			}
			if (text2 == null || text2.Length == 0)
			{
				text2 = "CreatedObject" + num++;
			}
			obj2 = objectCreator.CreateObject(text2, el);
		}
		SetUpObject(obj2, el);
		property.SetValue(o, obj2, null);
	}
}
