using System;
using System.Globalization;

namespace ICSharpCode.SharpDevelop;

public class DefaultNavigationPoint : INavigationPoint, IComparable
{
	private string fileName;

	private object data;

	public virtual string FileName => fileName;

	public virtual string Description => string.Format(CultureInfo.CurrentCulture, "{0}: {1}", new object[2] { fileName, data });

	public virtual string FullDescription => Description;

	public virtual string ToolTip => Description;

	public virtual int Index => 0;

	public object NavigationData
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public DefaultNavigationPoint()
		: this(string.Empty, null)
	{
	}

	public DefaultNavigationPoint(string fileName)
		: this(fileName, null)
	{
	}

	public DefaultNavigationPoint(string fileName, object data)
	{
		this.fileName = ((fileName == null) ? string.Empty : fileName);
		this.data = data;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "[{0}: {1}]", new object[2]
		{
			GetType().Name,
			Description
		});
	}

	public virtual void JumpTo()
	{
		FileService.JumpToFilePosition(FileName, 0, 0);
	}

	public void FileNameChanged(string newName)
	{
		fileName = ((newName == null) ? string.Empty : newName);
	}

	public virtual void ContentChanging(object sender, EventArgs e)
	{
	}

	public override bool Equals(object obj)
	{
		DefaultNavigationPoint defaultNavigationPoint = obj as DefaultNavigationPoint;
		if (object.ReferenceEquals(defaultNavigationPoint, null))
		{
			return false;
		}
		return FileName == defaultNavigationPoint.FileName;
	}

	public override int GetHashCode()
	{
		return FileName.GetHashCode();
	}

	public virtual int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (GetType() != obj.GetType())
		{
			return GetType().Name.CompareTo(obj.GetType().Name);
		}
		DefaultNavigationPoint defaultNavigationPoint = obj as DefaultNavigationPoint;
		return FileName.CompareTo(defaultNavigationPoint.FileName);
	}

	public static bool operator ==(DefaultNavigationPoint p1, DefaultNavigationPoint p2)
	{
		return object.Equals(p1, p2);
	}

	public static bool operator !=(DefaultNavigationPoint p1, DefaultNavigationPoint p2)
	{
		return !(p1 == p2);
	}

	public static bool operator <(DefaultNavigationPoint p1, DefaultNavigationPoint p2)
	{
		if (!(p1 == null))
		{
			return p1.CompareTo(p2) < 0;
		}
		return p2 != null;
	}

	public static bool operator >(DefaultNavigationPoint p1, DefaultNavigationPoint p2)
	{
		if (!(p1 == null))
		{
			return p1.CompareTo(p2) > 0;
		}
		return false;
	}
}
