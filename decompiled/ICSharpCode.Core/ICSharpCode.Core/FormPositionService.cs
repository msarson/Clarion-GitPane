using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.Core;

public class FormPositionService : IFormPositionService
{
	private const string windowPositions = "WindowPositions";

	private static FormPositionService _Instance = new FormPositionService();

	public static FormPositionService Instance => _Instance;

	private void _StorePosition(Form form, Properties windowPos)
	{
		if (form == null || windowPos == null)
		{
			return;
		}
		if (form.FormBorderStyle == FormBorderStyle.Sizable || form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
		{
			if (form.WindowState == FormWindowState.Normal)
			{
				windowPos.Set("bounds", form.Bounds);
			}
		}
		else
		{
			windowPos.Set("location", form.Location);
		}
	}

	private void _RestorePosition(Form form, Properties windowPos)
	{
		if (form != null && windowPos != null)
		{
			if (form.FormBorderStyle == FormBorderStyle.Sizable || form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				form.Bounds = Validate(windowPos.Get("bounds", GetDefaultBounds(form)));
			}
			else
			{
				form.Location = Validate(windowPos.Get("location", GetDefaultLocation(form)), form.Size);
			}
		}
	}

	public void StorePosition(Form form)
	{
		StorePosition(form, null);
	}

	public void StorePosition(Form form, string formName)
	{
		if (form != null)
		{
			Properties properties = new Properties();
			_StorePosition(form, properties);
			Properties properties2 = PropertyService.Get<Properties>("WindowPositions", null);
			if (properties2 == null)
			{
				properties2 = new Properties();
			}
			if (string.IsNullOrEmpty(formName))
			{
				properties2.Set(form.GetType().ToString(), properties);
			}
			else
			{
				properties2.Set(formName, properties);
			}
			PropertyService.Set("WindowPositions", properties2);
		}
	}

	public void RestorePosition(Form form)
	{
		RestorePosition(form, null);
	}

	public void RestorePosition(Form form, string formName)
	{
		if (form != null)
		{
			Properties properties = PropertyService.Get<Properties>("WindowPositions", null);
			if (properties != null)
			{
				Properties properties2 = null;
				properties2 = ((!string.IsNullOrEmpty(formName)) ? properties.Get<Properties>(formName, null) : properties.Get<Properties>(form.GetType().ToString(), null));
				_RestorePosition(form, properties2);
			}
		}
	}

	public void Apply(Form form)
	{
		Apply(form, null);
	}

	public void Apply(Form form, string formName)
	{
		form.StartPosition = FormStartPosition.Manual;
		RestorePosition(form, formName);
		form.Closing += delegate
		{
			StorePosition(form, formName);
		};
	}

	private Rectangle Validate(Rectangle bounds)
	{
		Rectangle workingArea = Screen.FromPoint(new Point(bounds.X, bounds.Y)).WorkingArea;
		Rectangle workingArea2 = Screen.FromPoint(new Point(bounds.X + bounds.Width, bounds.Y)).WorkingArea;
		if (bounds.Y < workingArea.Y - 5 && bounds.Y < workingArea2.Y - 5)
		{
			bounds.Y = workingArea.Y - 5;
		}
		if (bounds.X < workingArea.X - bounds.Width / 2)
		{
			bounds.X = workingArea.X - bounds.Width / 2;
		}
		else if (bounds.X > workingArea2.Right - bounds.Width / 2)
		{
			bounds.X = workingArea2.Right - bounds.Width / 2;
		}
		return bounds;
	}

	private Point Validate(Point location, Size size)
	{
		return Validate(new Rectangle(location, size)).Location;
	}

	private Rectangle GetDefaultBounds(Form form)
	{
		return new Rectangle(GetDefaultLocation(form), form.Size);
	}

	private Point GetDefaultLocation(Form form)
	{
		Form mainParent = GetMainParent(form);
		if (mainParent != null)
		{
			Rectangle bounds = mainParent.Bounds;
			Size size = form.Size;
			return new Point(bounds.Left + (bounds.Width - size.Width) / 2, bounds.Top + (bounds.Height - size.Height) / 2);
		}
		return form.Location;
	}

	private Form GetMainParent(Form form)
	{
		if (form.ParentForm == null)
		{
			return form.ParentForm;
		}
		return GetMainParent(form.ParentForm);
	}
}
