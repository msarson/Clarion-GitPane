using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Commands;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public class ConfigurationGuiHelper : ICanBeDirty
{
	private class CheckBoxBinding : ConfigurationGuiBinding
	{
		private CheckBox control;

		private bool defaultValue;

		public CheckBoxBinding(CheckBox control, bool defaultValue)
		{
			this.control = control;
			this.defaultValue = defaultValue;
		}

		public override void Load()
		{
			control.Checked = Get(defaultValue);
		}

		public override bool Save()
		{
			string text = Get("True");
			if (text == "true" || text == "false")
			{
				Set(control.Checked.ToString().ToLowerInvariant());
			}
			else
			{
				Set(control.Checked.ToString());
			}
			return true;
		}
	}

	private class SimpleTextBinding : ConfigurationGuiBinding
	{
		private Control control;

		private Func<string> defaultValueProvider;

		public SimpleTextBinding(Control control, Func<string> defaultValueProvider)
		{
			this.defaultValueProvider = defaultValueProvider;
			this.control = control;
		}

		public override void Load()
		{
			control.Text = Get(defaultValueProvider());
		}

		public override bool Save()
		{
			if (control.Text == defaultValueProvider())
			{
				Set("");
			}
			else
			{
				Set(control.Text);
			}
			return true;
		}
	}

	private class SimpleIntBinding : ConfigurationGuiBinding
	{
		private NumericUpDown control;

		private int defaultValue;

		public SimpleIntBinding(NumericUpDown control, int defaultValue)
		{
			this.control = control;
			this.defaultValue = defaultValue;
		}

		public override void Load()
		{
			if (!int.TryParse(Get(defaultValue.ToString(NumberFormatInfo.InvariantInfo)), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result))
			{
				result = defaultValue;
			}
			control.Text = result.ToString();
		}

		public override bool Save()
		{
			string s = control.Text.Trim();
			NumberStyles style = NumberStyles.Integer;
			Set(int.Parse(s, style, NumberFormatInfo.InvariantInfo).ToString(NumberFormatInfo.InvariantInfo));
			return true;
		}
	}

	private class HexadecimalBinding : ConfigurationGuiBinding
	{
		private TextBoxBase textBox;

		private int defaultValue;

		public HexadecimalBinding(TextBoxBase textBox, int defaultValue)
		{
			this.textBox = textBox;
			this.defaultValue = defaultValue;
		}

		public override void Load()
		{
			if (!int.TryParse(Get(defaultValue.ToString(NumberFormatInfo.InvariantInfo)), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var result))
			{
				result = defaultValue;
			}
			textBox.Text = "0x" + result.ToString("x", NumberFormatInfo.InvariantInfo);
		}

		public override bool Save()
		{
			string text = textBox.Text.Trim();
			NumberStyles style = NumberStyles.Integer;
			if (text.StartsWith("0x"))
			{
				text = text.Substring(2);
				style = NumberStyles.HexNumber;
			}
			if (!int.TryParse(text, style, NumberFormatInfo.InvariantInfo, out var result))
			{
				textBox.Focus();
				MessageService.ShowMessage("${res:Dialog.ProjectOptions.PleaseEnterValidNumber}");
				return false;
			}
			Set(result.ToString(NumberFormatInfo.InvariantInfo));
			return true;
		}
	}

	private class ComboBoxBinding : ConfigurationGuiBinding
	{
		private ComboBox control;

		private string[] values;

		private string defaultValue;

		public ComboBoxBinding(ComboBox control, string[] values, string defaultValue)
		{
			this.control = control;
			this.values = values;
			this.defaultValue = defaultValue;
		}

		public override void Load()
		{
			string text = Get(defaultValue);
			int i;
			for (i = 0; i < values.Length && !text.Equals(values[i], StringComparison.OrdinalIgnoreCase); i++)
			{
			}
			if (i == values.Length)
			{
				i = 0;
			}
			control.SelectedIndex = i;
		}

		public override bool Save()
		{
			Set(values[control.SelectedIndex]);
			return true;
		}
	}

	private class RadioEnumBinding<T> : ConfigurationGuiBinding where T : struct
	{
		private KeyValuePair<T, RadioButton>[] values;

		internal RadioEnumBinding(KeyValuePair<T, RadioButton>[] values)
		{
			this.values = values;
		}

		public override void Load()
		{
			T val = Get(values[0].Key);
			int i;
			for (i = 0; i < values.Length && !val.Equals(values[i].Key); i++)
			{
			}
			if (i == values.Length)
			{
				i = 0;
			}
			values[i].Value.Checked = true;
		}

		public override bool Save()
		{
			KeyValuePair<T, RadioButton>[] array = values;
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<T, RadioButton> keyValuePair = array[i];
				if (keyValuePair.Value.Checked)
				{
					Set(keyValuePair.Key);
					break;
				}
			}
			return true;
		}
	}

	private sealed class ConfigurationSelector : Panel
	{
		private ConfigurationGuiHelper helper;

		private Label configurationLabel = new Label();

		private Label information = new Label();

		private ComboBox configurationComboBox = new ComboBox();

		private Label platformLabel = new Label();

		private ComboBox platformComboBox = new ComboBox();

		private Control line = new Control();

		private bool resettingIndex;

		public ConfigurationSelector(ConfigurationGuiHelper helper)
		{
			this.helper = helper;
			base.Height = 56;
			configurationLabel.Text = StringParser.Parse("${res:Dialog.ProjectOptions.Configuration}:");
			configurationLabel.TextAlign = ContentAlignment.MiddleRight;
			configurationLabel.Location = new Point(4, 4);
			configurationLabel.Width = 90;
			configurationComboBox.Location = new Point(20 + configurationLabel.Right, 4);
			configurationComboBox.Width = 110;
			configurationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			information.Text = StringParser.Parse("${res:Dialog.ProjectOptions.Information}");
			information.Location = new Point(8, 4 + configurationLabel.Bottom);
			information.Width = 450;
			platformLabel.Text = StringParser.Parse("${res:Dialog.ProjectOptions.Platform}:");
			platformLabel.TextAlign = ContentAlignment.MiddleRight;
			platformLabel.Location = new Point(4 + configurationComboBox.Right, 4);
			platformLabel.Width = 68;
			platformComboBox.Location = new Point(4 + platformLabel.Right, 4);
			platformComboBox.Width = 110;
			platformComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			line.Bounds = new Rectangle(4, 54, base.Width - 8, 54);
			line.BackColor = SystemColors.ControlDark;
			base.Controls.AddRange(new Control[6] { configurationLabel, configurationComboBox, platformLabel, platformComboBox, information, line });
			line.Anchor |= AnchorStyles.Right;
			FillBoxes();
			configurationComboBox.SelectedIndexChanged += ConfigurationChanged;
			platformComboBox.SelectedIndexChanged += ConfigurationChanged;
		}

		private void FillBoxes()
		{
			configurationComboBox.Items.Clear();
			List<string> list = Linq.ToList(helper.Project.ConfigurationNames);
			list.Sort();
			configurationComboBox.Items.AddRange(list.ToArray());
			platformComboBox.Items.Clear();
			list = Linq.ToList(helper.Project.PlatformNames);
			list.Sort();
			platformComboBox.Items.AddRange(list.ToArray());
			ResetIndex();
		}

		private void ResetIndex()
		{
			resettingIndex = true;
			configurationComboBox.SelectedIndex = configurationComboBox.Items.IndexOf(helper.Configuration);
			platformComboBox.SelectedIndex = platformComboBox.Items.IndexOf(helper.Platform);
			resettingIndex = false;
		}

		private void ConfigurationChanged(object sender, EventArgs e)
		{
			if (resettingIndex)
			{
				return;
			}
			if (helper.IsDirty)
			{
				if (!MessageService.AskQuestion("${res:Dialog.ProjectOptions.ContinueSwitchConfiguration}"))
				{
					ResetIndex();
					return;
				}
				if (!helper.Save())
				{
					ResetIndex();
					return;
				}
			}
			helper.Configuration = (string)configurationComboBox.SelectedItem;
			helper.Platform = (string)platformComboBox.SelectedItem;
			helper.Load();
		}
	}

	public const int ConfigurationSelectorHeight = 56;

	private MSBuildBasedProject project;

	private Dictionary<string, Control> controlDictionary;

	private List<ConfigurationGuiBinding> bindings = new List<ConfigurationGuiBinding>();

	private bool dirty;

	private string configuration;

	private string platform;

	private static Func<string> GetEmptyString = () => "";

	public MSBuildBasedProject Project => project;

	internal Dictionary<string, Control> ControlDictionary => controlDictionary;

	public bool IsDirty
	{
		get
		{
			return dirty;
		}
		set
		{
			if (dirty != value)
			{
				dirty = value;
				if (this.DirtyChanged != null)
				{
					this.DirtyChanged(this, EventArgs.Empty);
				}
			}
		}
	}

	public string Configuration
	{
		get
		{
			return configuration;
		}
		set
		{
			configuration = value;
		}
	}

	public string Platform
	{
		get
		{
			return platform;
		}
		set
		{
			platform = value;
		}
	}

	public event EventHandler Loading;

	public event EventHandler Loaded;

	public event EventHandler Saved;

	public event EventHandler DirtyChanged;

	public ConfigurationGuiHelper(MSBuildBasedProject project, Dictionary<string, Control> controlDictionary)
	{
		this.project = project;
		this.controlDictionary = controlDictionary;
		configuration = project.ActiveConfiguration;
		platform = project.ActivePlatform;
	}

	public T GetProperty<T>(string propertyName, T defaultValue, bool treatPropertyValueAsLiteral)
	{
		string v = ((!treatPropertyValueAsLiteral) ? project.GetUnevalatedProperty(configuration, platform, propertyName) : project.GetProperty(configuration, platform, propertyName));
		return GenericConverter.FromString(v, defaultValue);
	}

	public T GetProperty<T>(string propertyName, T defaultValue, bool treatPropertyValueAsLiteral, out PropertyStorageLocations location)
	{
		string v = ((!treatPropertyValueAsLiteral) ? project.GetUnevalatedProperty(configuration, platform, propertyName, out location) : project.GetProperty(configuration, platform, propertyName, out location));
		return GenericConverter.FromString(v, defaultValue);
	}

	public void SetProperty<T>(string propertyName, T value, bool treatPropertyValueAsLiteral, PropertyStorageLocations location)
	{
		project.SetProperty(configuration, platform, propertyName, GenericConverter.ToString(value), location, treatPropertyValueAsLiteral);
	}

	public void AddBinding(string property, ConfigurationGuiBinding binding)
	{
		binding.Property = property;
		binding.Helper = this;
		binding.Load();
		bindings.Add(binding);
	}

	public void Load()
	{
		if (this.Loading != null)
		{
			this.Loading(this, EventArgs.Empty);
		}
		foreach (ConfigurationGuiBinding binding in bindings)
		{
			binding.Load();
		}
		if (this.Loaded != null)
		{
			this.Loaded(this, EventArgs.Empty);
		}
		IsDirty = false;
	}

	public bool Save()
	{
		foreach (ConfigurationGuiBinding binding in bindings)
		{
			if (!binding.Save())
			{
				return false;
			}
		}
		if (this.Saved != null)
		{
			this.Saved(this, EventArgs.Empty);
		}
		IsDirty = false;
		return true;
	}

	private void ControlValueChanged(object sender, EventArgs e)
	{
		IsDirty = true;
	}

	public ConfigurationGuiBinding BindBoolean(string control, string property, bool defaultValue)
	{
		return BindBoolean(controlDictionary[control], property, defaultValue);
	}

	public ConfigurationGuiBinding BindBoolean(Control control, string property, bool defaultValue)
	{
		if (control is CheckBox checkBox)
		{
			CheckBoxBinding checkBoxBinding = new CheckBoxBinding(checkBox, defaultValue);
			AddBinding(property, checkBoxBinding);
			checkBox.CheckedChanged += ControlValueChanged;
			return checkBoxBinding;
		}
		throw new ApplicationException("Cannot bind " + control.GetType().Name + " to bool property.");
	}

	[Obsolete("Please explicitly specify textBoxEditMode")]
	public ConfigurationGuiBinding BindString(string control, string property)
	{
		return BindString(controlDictionary[control], property, TextBoxEditMode.EditEvaluatedProperty, GetEmptyString);
	}

	[Obsolete("Please explicitly specify textBoxEditMode")]
	public ConfigurationGuiBinding BindString(Control control, string property)
	{
		return BindString(control, property, TextBoxEditMode.EditEvaluatedProperty, GetEmptyString);
	}

	public ConfigurationGuiBinding BindString(string control, string property, TextBoxEditMode textBoxEditMode)
	{
		return BindString(controlDictionary[control], property, textBoxEditMode, GetEmptyString);
	}

	public ConfigurationGuiBinding BindString(Control control, string property, TextBoxEditMode textBoxEditMode)
	{
		return BindString(control, property, textBoxEditMode, GetEmptyString);
	}

	public ConfigurationGuiBinding BindString(Control control, string property, TextBoxEditMode textBoxEditMode, Func<string> defaultValueProvider)
	{
		if (control is TextBoxBase || control is ComboBox)
		{
			SimpleTextBinding simpleTextBinding = new SimpleTextBinding(control, defaultValueProvider);
			if (textBoxEditMode == TextBoxEditMode.EditEvaluatedProperty)
			{
				simpleTextBinding.TreatPropertyValueAsLiteral = true;
			}
			else
			{
				simpleTextBinding.TreatPropertyValueAsLiteral = false;
			}
			AddBinding(property, simpleTextBinding);
			control.TextChanged += ControlValueChanged;
			if (control is ComboBox)
			{
				control.KeyDown += ComboBoxKeyDown;
			}
			return simpleTextBinding;
		}
		throw new ApplicationException("Cannot bind " + control.GetType().Name + " to string property.");
	}

	private void ComboBoxKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == (Keys.S | Keys.Control))
		{
			e.Handled = true;
			new SaveFile().Run();
		}
	}

	public ConfigurationGuiBinding BindInt(string control, string property, int defaultValue)
	{
		return BindInt(controlDictionary[control], property, defaultValue);
	}

	public ConfigurationGuiBinding BindInt(Control control, string property, int defaultValue)
	{
		if (control is NumericUpDown)
		{
			SimpleIntBinding simpleIntBinding = new SimpleIntBinding((NumericUpDown)control, defaultValue);
			AddBinding(property, simpleIntBinding);
			control.TextChanged += ControlValueChanged;
			return simpleIntBinding;
		}
		throw new ApplicationException("Cannot bind " + control.GetType().Name + " to int property.");
	}

	public ConfigurationGuiBinding BindHexadecimal(TextBoxBase textBox, string property, int defaultValue)
	{
		HexadecimalBinding hexadecimalBinding = new HexadecimalBinding(textBox, defaultValue);
		AddBinding(property, hexadecimalBinding);
		textBox.TextChanged += ControlValueChanged;
		return hexadecimalBinding;
	}

	public ConfigurationGuiBinding BindEnum<T>(string control, string property, params T[] values) where T : struct
	{
		return BindEnum(controlDictionary[control], property, values);
	}

	public ConfigurationGuiBinding BindEnum<T>(Control control, string property, params T[] values) where T : struct
	{
		Type typeFromHandle = typeof(T);
		if (values == null || values.Length == 0)
		{
			values = (T[])Enum.GetValues(typeFromHandle);
		}
		if (control is ComboBox comboBox)
		{
			T[] array = values;
			foreach (T val in array)
			{
				object[] customAttributes = typeFromHandle.GetField(Enum.GetName(typeFromHandle, val)).GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
				string item = ((customAttributes.Length <= 0) ? Enum.GetName(typeFromHandle, val) : StringParser.Parse((customAttributes[0] as DescriptionAttribute).Description));
				comboBox.Items.Add(item);
			}
			string[] array2 = new string[values.Length];
			for (int j = 0; j < values.Length; j++)
			{
				array2[j] = values[j].ToString();
			}
			ComboBoxBinding comboBoxBinding = new ComboBoxBinding(comboBox, array2, array2[0]);
			AddBinding(property, comboBoxBinding);
			comboBox.SelectedIndexChanged += ControlValueChanged;
			comboBox.KeyDown += ComboBoxKeyDown;
			return comboBoxBinding;
		}
		throw new ApplicationException("Cannot bind " + control.GetType().Name + " to enum property.");
	}

	public ConfigurationGuiBinding BindStringEnum(string control, string property, string defaultValue, params KeyValuePair<string, string>[] entries)
	{
		return BindStringEnum(controlDictionary[control], property, defaultValue, entries);
	}

	public ConfigurationGuiBinding BindStringEnum(Control control, string property, string defaultValue, params KeyValuePair<string, string>[] entries)
	{
		if (control is ComboBox comboBox)
		{
			string[] array = new string[entries.Length];
			for (int i = 0; i < entries.Length; i++)
			{
				array[i] = entries[i].Key;
				comboBox.Items.Add(StringParser.Parse(entries[i].Value));
			}
			ComboBoxBinding comboBoxBinding = new ComboBoxBinding(comboBox, array, defaultValue);
			AddBinding(property, comboBoxBinding);
			comboBox.SelectedIndexChanged += ControlValueChanged;
			comboBox.KeyDown += ComboBoxKeyDown;
			return comboBoxBinding;
		}
		throw new ApplicationException("Cannot bind " + control.GetType().Name + " to enum property.");
	}

	public ConfigurationGuiBinding BindRadioEnum<T>(string property, params KeyValuePair<T, RadioButton>[] values) where T : struct
	{
		RadioEnumBinding<T> radioEnumBinding = new RadioEnumBinding<T>(values);
		AddBinding(property, radioEnumBinding);
		foreach (KeyValuePair<T, RadioButton> keyValuePair in values)
		{
			keyValuePair.Value.CheckedChanged += ControlValueChanged;
		}
		return radioEnumBinding;
	}

	public Control CreateConfigurationSelector()
	{
		return new ConfigurationSelector(this);
	}

	public void AddConfigurationSelector(Control parent)
	{
		foreach (Control control2 in parent.Controls)
		{
			control2.Top += 56;
		}
		Control control = CreateConfigurationSelector();
		control.Width = parent.ClientSize.Width;
		parent.Controls.Add(control);
		parent.Controls.SetChildIndex(control, 0);
		control.Anchor |= AnchorStyles.Right;
	}
}
