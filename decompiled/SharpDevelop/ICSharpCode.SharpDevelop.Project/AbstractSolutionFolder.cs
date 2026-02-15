using System.ComponentModel;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Project;

public abstract class AbstractSolutionFolder : LocalizedObject, ISolutionFolder
{
	private readonly object syncRoot = new object();

	private ISolutionFolderContainer parent;

	private string typeGuid;

	private string idGuid;

	private string location;

	private string name;

	[Browsable(false)]
	public object SyncRoot => syncRoot;

	[Browsable(false)]
	public virtual Solution ParentSolution
	{
		get
		{
			lock (syncRoot)
			{
				if (parent != null)
				{
					return parent.ParentSolution;
				}
				return null;
			}
		}
	}

	[Browsable(false)]
	public virtual string IdGuid
	{
		get
		{
			return idGuid;
		}
		set
		{
			if (!value.StartsWith("{"))
			{
				idGuid = "{" + value.ToUpperInvariant() + "}";
			}
			else
			{
				idGuid = value;
			}
		}
	}

	[Browsable(false)]
	public string Location
	{
		get
		{
			return location;
		}
		set
		{
			location = value;
		}
	}

	[Browsable(false)]
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	[Browsable(false)]
	public ISolutionFolderContainer Parent
	{
		get
		{
			return parent;
		}
		set
		{
			lock (syncRoot)
			{
				parent = value;
			}
		}
	}

	[Browsable(false)]
	public virtual string TypeGuid
	{
		get
		{
			return typeGuid;
		}
		set
		{
			typeGuid = value;
		}
	}
}
