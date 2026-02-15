using System;
using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.SharpDevelop.Project;

public abstract class ConfigurationGuiBinding
{
	private ConfigurationGuiHelper helper;

	private string property;

	private bool treatPropertyValueAsLiteral = true;

	private PropertyStorageLocations defaultLocation = PropertyStorageLocations.Base;

	private PropertyStorageLocations location;

	private ChooseStorageLocationButton storageLocationButton;

	private bool isFirstGet = true;

	public MSBuildBasedProject Project => helper.Project;

	public ConfigurationGuiHelper Helper
	{
		get
		{
			return helper;
		}
		internal set
		{
			helper = value;
		}
	}

	public string Property
	{
		get
		{
			return property;
		}
		internal set
		{
			property = value;
		}
	}

	public bool TreatPropertyValueAsLiteral
	{
		get
		{
			return treatPropertyValueAsLiteral;
		}
		set
		{
			treatPropertyValueAsLiteral = value;
		}
	}

	public PropertyStorageLocations DefaultLocation
	{
		get
		{
			return defaultLocation;
		}
		set
		{
			defaultLocation = value;
		}
	}

	public PropertyStorageLocations Location
	{
		get
		{
			return location;
		}
		set
		{
			if (location != value)
			{
				location = value;
				if (storageLocationButton != null)
				{
					storageLocationButton.StorageLocation = value;
				}
				helper.IsDirty = true;
			}
		}
	}

	public ChooseStorageLocationButton CreateLocationButton()
	{
		ChooseStorageLocationButton chooseStorageLocationButton = new ChooseStorageLocationButton();
		if (location == PropertyStorageLocations.Unchanged)
		{
			chooseStorageLocationButton.StorageLocation = defaultLocation;
		}
		else
		{
			chooseStorageLocationButton.StorageLocation = location;
		}
		RegisterLocationButton(chooseStorageLocationButton);
		return chooseStorageLocationButton;
	}

	public void RegisterLocationButton(ChooseStorageLocationButton btn)
	{
		storageLocationButton = btn;
		btn.StorageLocationChanged += delegate(object sender, EventArgs e)
		{
			Location = ((ChooseStorageLocationButton)sender).StorageLocation;
		};
	}

	public ChooseStorageLocationButton CreateLocationButtonInPanel(string panelName)
	{
		ChooseStorageLocationButton chooseStorageLocationButton = CreateLocationButton();
		Control control = Helper.ControlDictionary[panelName];
		foreach (Control control2 in control.Controls)
		{
			if ((control2.Anchor & AnchorStyles.Left) == AnchorStyles.Left)
			{
				control2.Left += chooseStorageLocationButton.Width + 8;
				if ((control2.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
				{
					control2.Width -= chooseStorageLocationButton.Width + 8;
				}
			}
		}
		chooseStorageLocationButton.Location = new Point(4, (control.ClientSize.Height - chooseStorageLocationButton.Height) / 2);
		control.Controls.Add(chooseStorageLocationButton);
		control.Controls.SetChildIndex(chooseStorageLocationButton, 0);
		return chooseStorageLocationButton;
	}

	public ChooseStorageLocationButton CreateLocationButton(string controlName)
	{
		return CreateLocationButton(Helper.ControlDictionary[controlName]);
	}

	public ChooseStorageLocationButton CreateLocationButton(Control replacedControl)
	{
		ChooseStorageLocationButton chooseStorageLocationButton = CreateLocationButton();
		chooseStorageLocationButton.Location = new Point(replacedControl.Left, replacedControl.Top + (replacedControl.Height - chooseStorageLocationButton.Height) / 2);
		replacedControl.Left += chooseStorageLocationButton.Width + 4;
		replacedControl.Width -= chooseStorageLocationButton.Width + 4;
		replacedControl.Parent.Controls.Add(chooseStorageLocationButton);
		replacedControl.Parent.Controls.SetChildIndex(chooseStorageLocationButton, replacedControl.Parent.Controls.IndexOf(replacedControl));
		return chooseStorageLocationButton;
	}

	public T Get<T>(T defaultValue)
	{
		if (isFirstGet)
		{
			isFirstGet = false;
			return helper.GetProperty(property, defaultValue, treatPropertyValueAsLiteral, out location);
		}
		return helper.GetProperty(property, defaultValue, treatPropertyValueAsLiteral);
	}

	public void Set<T>(T value)
	{
		if (location == PropertyStorageLocations.Unchanged)
		{
			location = defaultLocation;
		}
		helper.SetProperty(property, value, treatPropertyValueAsLiteral, location);
	}

	public abstract void Load();

	public abstract bool Save();
}
