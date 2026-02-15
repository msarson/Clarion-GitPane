using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace CommonSources.Properties;

[DebuggerNonUserCode]
[CompilerGenerated]
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (object.ReferenceEquals(resourceMan, null))
			{
				ResourceManager resourceManager = new ResourceManager("CommonSources.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap arrowdown
	{
		get
		{
			object obj = ResourceManager.GetObject("arrowdown", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap arrowup
	{
		get
		{
			object obj = ResourceManager.GetObject("arrowup", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap ASTERISK
	{
		get
		{
			object obj = ResourceManager.GetObject("ASTERISK", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap ContractAll
	{
		get
		{
			object obj = ResourceManager.GetObject("ContractAll", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap ExpandAll
	{
		get
		{
			object obj = ResourceManager.GetObject("ExpandAll", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Question
	{
		get
		{
			object obj = ResourceManager.GetObject("Question", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
