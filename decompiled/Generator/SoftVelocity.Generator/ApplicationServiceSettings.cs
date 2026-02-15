using ICSharpCode.Core;

namespace SoftVelocity.Generator;

public sealed class ApplicationServiceSettings
{
	public static readonly string PropertiesName;

	private static Properties prop;

	public static bool RestoreSolutionViewState
	{
		get
		{
			return prop.Get<bool>("RestoreSolutionViewState", false);
		}
		internal set
		{
			prop.Set<bool>("RestoreSolutionViewState", value);
		}
	}

	public static bool OnSolutionLoadedEditApp
	{
		get
		{
			return prop.Get<bool>("OnSolutionLoadedEditApp", true);
		}
		internal set
		{
			prop.Set<bool>("OnSolutionLoadedEditApp", value);
		}
	}

	public static bool CacheApplicationAfterEdit
	{
		get
		{
			return prop.Get<bool>("cacheAppAfterEdit", false);
		}
		internal set
		{
			prop.Set<bool>("cacheAppAfterEdit", value);
		}
	}

	public static bool OnSolutionLoadedAllwaysAllowLibHunter
	{
		get
		{
			return prop.Get<bool>("OnSolutionLoadedAllwaysAllowLibHunter", false);
		}
		internal set
		{
			prop.Set<bool>("OnSolutionLoadedAllwaysAllowLibHunter", value);
		}
	}

	public static bool EditDataAsTxa
	{
		get
		{
			return prop.Get<bool>("EditDataAsTxa", false);
		}
		internal set
		{
			prop.Set<bool>("EditDataAsTxa", value);
		}
	}

	public static bool HaveLocator
	{
		get
		{
			return prop.Get<bool>("HaveLocator", true);
		}
		internal set
		{
			prop.Set<bool>("HaveLocator", value);
		}
	}

	public static bool AlwaysShowChangedDateTime
	{
		get
		{
			return prop.Get<bool>("AlwaysShowChangedDateTime", true);
		}
		internal set
		{
			prop.Set<bool>("AlwaysShowChangedDateTime", value);
		}
	}

	public static ApplicationService.ApplicationsSort DefaultApplicationsListSort
	{
		get
		{
			return prop.Get<ApplicationService.ApplicationsSort>("DefaultApplicationsListSort", ApplicationService.ApplicationsSort.ByName);
		}
		internal set
		{
			prop.Set<ApplicationService.ApplicationsSort>("DefaultApplicationsListSort", value);
		}
	}

	static ApplicationServiceSettings()
	{
		PropertiesName = "SoftVelocity.Generator.ApplicationService";
		Reload();
	}

	public static void Reload()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		prop = PropertyService.Get<Properties>(PropertiesName, new Properties());
	}

	public static void Store()
	{
		PropertyService.Set<Properties>(PropertiesName, prop);
	}
}
