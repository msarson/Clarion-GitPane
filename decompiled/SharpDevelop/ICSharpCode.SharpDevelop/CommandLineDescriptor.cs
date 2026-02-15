using System;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop;

public class CommandLineDescriptor : ICommandLineDescriptor
{
	private string id;

	private string clSwitch;

	private string description;

	private string commandLineClass;

	private string runBefore;

	private string runAfter;

	private AddIn addin;

	private ICommandLine commandLine;

	private bool hasMultiples;

	private int optionalParameters;

	private int parameters;

	public string ID => id;

	public string Switch => clSwitch;

	public string Class => commandLineClass;

	public string Description => description;

	public bool CanHaveMultiples => hasMultiples;

	public int Parameters => parameters;

	public int OptionalParameters => optionalParameters;

	public ICommandLine CommandLine
	{
		get
		{
			if (commandLine == null && addin != null)
			{
				commandLine = addin.CreateObject(commandLineClass) as ICommandLine;
				addin = null;
			}
			return commandLine;
		}
	}

	public string RunBefore
	{
		get
		{
			return runBefore;
		}
		set
		{
			runBefore = value;
		}
	}

	public string RunAfter
	{
		get
		{
			return runAfter;
		}
		set
		{
			runAfter = value;
		}
	}

	public CommandLineDescriptor(Codon codon)
	{
		id = codon.Id;
		clSwitch = codon.Properties["switch"];
		description = codon.Properties["description"];
		commandLineClass = codon.Properties["class"];
		runBefore = codon.Properties["runbefore"];
		runAfter = codon.Properties["runafter"];
		hasMultiples = !codon.Properties.Contains("multi") || bool.Parse(codon.Properties["multi"]);
		parameters = ((!codon.Properties.Contains("parameters")) ? 1 : int.Parse(codon.Properties["parameters"]));
		optionalParameters = (codon.Properties.Contains("optionalparameters") ? int.Parse(codon.Properties["optionalparameters"]) : 0);
		addin = codon.AddIn;
	}

	public CommandLineDescriptor(string clSwitch, int parameters, string desc)
	{
		this.clSwitch = clSwitch;
		this.parameters = parameters;
		description = desc;
	}

	public override bool Equals(object obj)
	{
		if (obj is CommandLineDescriptor)
		{
			return Switch.Equals(((CommandLineDescriptor)obj).Switch, StringComparison.InvariantCultureIgnoreCase);
		}
		if (obj is string)
		{
			return Switch.Equals((string)obj, StringComparison.InvariantCultureIgnoreCase);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Switch.GetHashCode();
	}
}
