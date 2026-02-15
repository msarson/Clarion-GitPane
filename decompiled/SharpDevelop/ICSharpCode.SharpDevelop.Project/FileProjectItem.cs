using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Widgets.DesignTimeSupport;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project;

public class FileProjectItem : ProjectItem
{
	private sealed class BuildActionEditor : DropDownEditor
	{
		protected override Control CreateDropDownControl(ITypeDescriptorContext context, IWindowsFormsEditorService editorService)
		{
			if (context.Instance is FileProjectItem { Project: not null } fileProjectItem)
			{
				return new DropDownEditorListBox(editorService, GetNames(fileProjectItem.Project.AvailableFileItemTypes));
			}
			return new DropDownEditorListBox(editorService, GetNames(ItemType.DefaultFileItems));
		}

		private static IEnumerable<string> GetNames(IEnumerable<ItemType> itemTypes)
		{
			return Linq.Select(itemTypes, (ItemType it) => it.ItemName);
		}
	}

	private sealed class CustomToolEditor : DropDownEditor
	{
		protected override Control CreateDropDownControl(ITypeDescriptorContext context, IWindowsFormsEditorService editorService)
		{
			if (context.Instance is FileProjectItem item)
			{
				return new DropDownEditorListBox(editorService, CustomToolsService.GetCompatibleCustomToolNames(item));
			}
			return new DropDownEditorListBox(editorService, CustomToolsService.GetCustomToolNames());
		}
	}

	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectFile.BuildAction}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectFile.BuildAction.Description}")]
	[Editor(typeof(BuildActionEditor), typeof(UITypeEditor))]
	public string BuildAction
	{
		get
		{
			return base.ItemType.ItemName;
		}
		set
		{
			base.ItemType = new ItemType(value);
		}
	}

	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectFile.CopyToOutputDirectory}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectFile.CopyToOutputDirectory.Description}")]
	public CopyToOutputDirectory CopyToOutputDirectory
	{
		get
		{
			return GetEvaluatedMetadata("CopyToOutputDirectory", CopyToOutputDirectory.Never);
		}
		set
		{
			SetEvaluatedMetadata("CopyToOutputDirectory", value);
		}
	}

	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectFile.CustomTool}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectFile.CustomTool.Description}")]
	[Editor(typeof(CustomToolEditor), typeof(UITypeEditor))]
	public string CustomTool
	{
		get
		{
			return GetEvaluatedMetadata("Generator");
		}
		set
		{
			SetEvaluatedMetadata("Generator", value);
		}
	}

	[LocalizedProperty("${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectFile.CustomToolNamespace}", Description = "${res:ICSharpCode.SharpDevelop.Internal.Project.ProjectFile.CustomToolNamespace.Description}")]
	public string CustomToolNamespace
	{
		get
		{
			return GetEvaluatedMetadata("CustomToolNamespace");
		}
		set
		{
			SetEvaluatedMetadata("CustomToolNamespace", value);
			CustomToolsService.RunCustomTool(this, showMessageBoxOnErrors: false);
		}
	}

	[Browsable(false)]
	public string DependentUpon
	{
		get
		{
			return GetEvaluatedMetadata("DependentUpon");
		}
		set
		{
			SetEvaluatedMetadata("DependentUpon", value);
		}
	}

	[Browsable(false)]
	public string SubType
	{
		get
		{
			return GetEvaluatedMetadata("SubType");
		}
		set
		{
			SetEvaluatedMetadata("SubType", value);
		}
	}

	[Browsable(false)]
	public bool IsLink
	{
		get
		{
			if (!HasMetadata("Link"))
			{
				return !FileUtility.IsBaseDirectory(base.Project.Directory, FileName);
			}
			return true;
		}
	}

	[Browsable(false)]
	public string VirtualName
	{
		get
		{
			if (HasMetadata("Link"))
			{
				return GetEvaluatedMetadata("Link");
			}
			if (FileUtility.IsBaseDirectory(base.Project.Directory, FileName))
			{
				return base.Include;
			}
			return Path.GetFileName(base.Include);
		}
	}

	public FileProjectItem(IProject project, ItemType itemType, string include)
		: base(project, itemType, include)
	{
	}

	public FileProjectItem(IProject project, ItemType itemType)
		: base(project, itemType)
	{
	}

	protected internal FileProjectItem(IProject project, BuildItem buildItem)
		: base(project, buildItem)
	{
	}
}
