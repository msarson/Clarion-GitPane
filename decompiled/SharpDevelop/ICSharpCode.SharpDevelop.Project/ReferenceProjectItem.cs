using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public class ReferenceProjectItem : ProjectItem
{
	private sealed class ReplaceDefaultValueDescriptor : PropertyDescriptor
	{
		private PropertyDescriptor baseDescriptor;

		private bool newDefaultValue;

		public override string DisplayName => baseDescriptor.DisplayName;

		public override string Description => baseDescriptor.Description;

		public override Type ComponentType => baseDescriptor.ComponentType;

		public override bool IsReadOnly => baseDescriptor.IsReadOnly;

		public override Type PropertyType => baseDescriptor.PropertyType;

		public override bool ShouldSerializeValue(object component)
		{
			return (bool)GetValue(component) != newDefaultValue;
		}

		public override void ResetValue(object component)
		{
			SetValue(component, newDefaultValue);
		}

		public ReplaceDefaultValueDescriptor(PropertyDescriptor baseDescriptor, bool newDefaultValue)
			: base(baseDescriptor)
		{
			this.baseDescriptor = baseDescriptor;
			this.newDefaultValue = newDefaultValue;
		}

		public override bool CanResetValue(object component)
		{
			return baseDescriptor.CanResetValue(component);
		}

		public override object GetValue(object component)
		{
			return baseDescriptor.GetValue(component);
		}

		public override void SetValue(object component, object value)
		{
			baseDescriptor.SetValue(component, value);
		}
	}

	private string evaluatedReferencePath;

	[Browsable(false)]
	public string EvaluatedReferencePath
	{
		get
		{
			return evaluatedReferencePath;
		}
		set
		{
			evaluatedReferencePath = value;
		}
	}

	[Browsable(false)]
	public string HintPath
	{
		get
		{
			return GetEvaluatedMetadata("HintPath");
		}
		set
		{
			SetEvaluatedMetadata("HintPath", value);
		}
	}

	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.Aliases}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.Aliases.Description}")]
	[DefaultValue("global")]
	public string Aliases
	{
		get
		{
			return GetEvaluatedMetadata("Aliases", "global");
		}
		set
		{
			SetEvaluatedMetadata("Aliases", value);
		}
	}

	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.SpecificVersion}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.SpecificVersion.Description}")]
	[DefaultValue(false)]
	public bool SpecificVersion
	{
		get
		{
			return GetEvaluatedMetadata("SpecificVersion", defaultValue: false);
		}
		set
		{
			SetEvaluatedMetadata("SpecificVersion", value);
		}
	}

	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.LocalCopy}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.LocalCopy.Description}")]
	public bool Private
	{
		get
		{
			return GetEvaluatedMetadata("Private", !IsGacReference);
		}
		set
		{
			SetEvaluatedMetadata("Private", value);
		}
	}

	[ReadOnly(true)]
	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.Name}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.Name.Description}")]
	public string Name
	{
		get
		{
			AssemblyName assemblyName = GetAssemblyName();
			if (assemblyName != null)
			{
				return assemblyName.Name;
			}
			return base.Include;
		}
	}

	[ReadOnly(true)]
	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.Version}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.Version.Description}")]
	public Version Version => GetAssemblyName()?.Version;

	[ReadOnly(true)]
	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.Culture}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.Culture.Description}")]
	public string Culture
	{
		get
		{
			AssemblyName assemblyName = GetAssemblyName();
			if (assemblyName != null && assemblyName.CultureInfo != null)
			{
				return assemblyName.CultureInfo.Name;
			}
			return null;
		}
	}

	[ReadOnly(true)]
	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.PublicKeyToken}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectReference.PublicKeyToken.Description}")]
	public string PublicKeyToken
	{
		get
		{
			AssemblyName assemblyName = GetAssemblyName();
			if (assemblyName != null)
			{
				byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
				if (publicKeyToken != null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					byte[] array = publicKeyToken;
					foreach (byte b in array)
					{
						stringBuilder.Append(b.ToString("x2"));
					}
					return stringBuilder.ToString();
				}
			}
			return null;
		}
	}

	[ReadOnly(true)]
	public override string FileName
	{
		get
		{
			if (base.Project != null)
			{
				string directory = base.Project.Directory;
				string hintPath = HintPath;
				try
				{
					if (hintPath != null && hintPath.Length > 0)
					{
						return FileUtility.GetAbsolutePath(directory, hintPath);
					}
					string absolutePath = FileUtility.GetAbsolutePath(directory, base.Include);
					if (File.Exists(absolutePath))
					{
						return absolutePath;
					}
					if (File.Exists(absolutePath + ".dll"))
					{
						return absolutePath + ".dll";
					}
					if (File.Exists(absolutePath + ".exe"))
					{
						return absolutePath + ".exe";
					}
				}
				catch
				{
				}
			}
			return base.Include;
		}
		set
		{
		}
	}

	[Browsable(false)]
	public string EvaluatedFileName
	{
		get
		{
			if (EvaluatedReferencePath != null)
			{
				return EvaluatedReferencePath;
			}
			return FileName;
		}
	}

	[Browsable(false)]
	public bool IsGacReference => !Path.IsPathRooted(FileName);

	protected ReferenceProjectItem(IProject project, ItemType itemType)
		: base(project, itemType)
	{
	}

	public ReferenceProjectItem(IProject project)
		: base(project, ItemType.Reference)
	{
	}

	public ReferenceProjectItem(IProject project, string include)
		: base(project, ItemType.Reference, include)
	{
	}

	protected internal ReferenceProjectItem(IProject project, BuildItem buildItem)
		: base(project, buildItem)
	{
	}

	private AssemblyName GetAssemblyName()
	{
		AssemblyName result = null;
		try
		{
			if (base.ItemType == ItemType.Reference)
			{
				result = ((EvaluatedReferencePath == null) ? new AssemblyName(base.Include) : AssemblyName.GetAssemblyName(EvaluatedReferencePath));
			}
		}
		catch (ArgumentException)
		{
		}
		catch (FileNotFoundException)
		{
		}
		catch (FileLoadException)
		{
		}
		return result;
	}

	protected override void FilterProperties(PropertyDescriptorCollection globalizedProps)
	{
		base.FilterProperties(globalizedProps);
		PropertyDescriptor propertyDescriptor = globalizedProps["Private"];
		globalizedProps.Remove(propertyDescriptor);
		globalizedProps.Add(new ReplaceDefaultValueDescriptor(propertyDescriptor, !IsGacReference));
	}

	public override string TypeName()
	{
		return "Reference Properties";
	}
}
