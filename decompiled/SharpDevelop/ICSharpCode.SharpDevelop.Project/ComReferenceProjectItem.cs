using System;
using System.ComponentModel;
using System.IO;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public sealed class ComReferenceProjectItem : ReferenceProjectItem
{
	[ReadOnly(true)]
	public string Guid
	{
		get
		{
			return GetEvaluatedMetadata("Guid");
		}
		set
		{
			SetEvaluatedMetadata("Guid", value);
		}
	}

	[ReadOnly(true)]
	public int VersionMajor
	{
		get
		{
			return GetEvaluatedMetadata("VersionMajor", 1);
		}
		set
		{
			SetEvaluatedMetadata("VersionMajor", value);
		}
	}

	[ReadOnly(true)]
	public int VersionMinor
	{
		get
		{
			return GetEvaluatedMetadata("VersionMinor", 0);
		}
		set
		{
			SetEvaluatedMetadata("VersionMinor", value);
		}
	}

	[ReadOnly(true)]
	public string Lcid
	{
		get
		{
			return GetEvaluatedMetadata("Lcid");
		}
		set
		{
			SetEvaluatedMetadata("Lcid", value);
		}
	}

	[ReadOnly(true)]
	public string WrapperTool
	{
		get
		{
			return GetEvaluatedMetadata("WrapperTool");
		}
		set
		{
			SetEvaluatedMetadata("WrapperTool", value);
		}
	}

	[ReadOnly(true)]
	public bool Isolated
	{
		get
		{
			return GetEvaluatedMetadata("Isolated", defaultValue: false);
		}
		set
		{
			SetEvaluatedMetadata("Isolated", value);
		}
	}

	public override string FileName
	{
		get
		{
			try
			{
				if (base.Project != null && base.Project.OutputAssemblyFullPath != null)
				{
					string text = Path.GetDirectoryName(base.Project.OutputAssemblyFullPath);
					string text2 = Path.Combine(text, "Interop." + base.Include + ".dll");
					if (File.Exists(text2))
					{
						return text2;
					}
					text2 = GetActiveXInteropFileName(text, base.Include);
					if (File.Exists(text2))
					{
						return text2;
					}
					if (base.Project is CompilableProject)
					{
						text = (base.Project as CompilableProject).IntermediateOutputFullPath;
						text2 = Path.Combine(text, "Interop." + base.Include + ".dll");
						if (File.Exists(text2))
						{
							return text2;
						}
					}
					text2 = GetActiveXInteropFileName(text, base.Include);
					if (File.Exists(text2))
					{
						return text2;
					}
				}
			}
			catch (Exception)
			{
			}
			return base.Include;
		}
		set
		{
		}
	}

	public ComReferenceProjectItem(IProject project, TypeLibrary library)
		: base(project, ItemType.COMReference)
	{
		base.Include = library.Name;
		Guid = library.Guid;
		VersionMajor = library.VersionMajor;
		VersionMinor = library.VersionMinor;
		Lcid = library.Lcid;
		WrapperTool = library.WrapperTool;
		Isolated = library.Isolated;
	}

	internal ComReferenceProjectItem(IProject project, BuildItem buildItem)
		: base(project, buildItem)
	{
	}

	private static string GetActiveXInteropFileName(string outputFolder, string include)
	{
		if (include.ToLowerInvariant().StartsWith("ax"))
		{
			return Path.Combine(outputFolder, "AxInterop." + include.Substring(2) + ".dll");
		}
		return null;
	}
}
