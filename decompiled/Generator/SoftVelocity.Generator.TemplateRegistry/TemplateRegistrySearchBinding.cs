using SoftVelocity.Common;

namespace SoftVelocity.Generator.TemplateRegistry;

internal class TemplateRegistrySearchBinding : RedirectionSearchBinding
{
	private static TemplateRegistrySearchBinding instance;

	public static TemplateRegistrySearchBinding Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new TemplateRegistrySearchBinding();
			}
			return instance;
		}
	}
}
