using System.Collections;

namespace ICSharpCode.Core;

public class LazyLoadDoozer : IDoozer
{
	private AddIn addIn;

	private string name;

	private string className;

	public string Name => name;

	public string ClassName => className;

	public bool HandleConditions
	{
		get
		{
			IDoozer doozer = (IDoozer)addIn.CreateObject(className);
			if (doozer == null)
			{
				return false;
			}
			AddInTree.Doozers[name] = doozer;
			return doozer.HandleConditions;
		}
	}

	public LazyLoadDoozer(AddIn addIn, Properties properties)
	{
		this.addIn = addIn;
		name = properties["name"];
		className = properties["class"];
	}

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		IDoozer doozer = (IDoozer)addIn.CreateObject(className);
		if (doozer == null)
		{
			return null;
		}
		AddInTree.Doozers[name] = doozer;
		return doozer.BuildItem(caller, codon, subItems);
	}

	public override string ToString()
	{
		return $"[LazyLoadDoozer: className = {className}, name = {name}]";
	}
}
