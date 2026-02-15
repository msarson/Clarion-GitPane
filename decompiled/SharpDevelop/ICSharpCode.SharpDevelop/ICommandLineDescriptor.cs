namespace ICSharpCode.SharpDevelop;

internal interface ICommandLineDescriptor
{
	string ID { get; }

	string Switch { get; }

	string Description { get; }

	bool CanHaveMultiples { get; }

	int Parameters { get; }

	int OptionalParameters { get; }

	ICommandLine CommandLine { get; }

	string RunBefore { get; set; }

	string RunAfter { get; set; }
}
