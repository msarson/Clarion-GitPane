using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Properties;

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
				ResourceManager resourceManager = new ResourceManager("Properties.Resources", typeof(Resources).Assembly);
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

	internal static Bitmap SplashScreen
	{
		get
		{
			object obj = ResourceManager.GetObject("SplashScreen", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap SplashScreenEE
	{
		get
		{
			object obj = ResourceManager.GetObject("SplashScreenEE", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap SplashScreenPE
	{
		get
		{
			object obj = ResourceManager.GetObject("SplashScreenPE", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
