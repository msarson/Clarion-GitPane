using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ICSharpCode.SharpDevelop.Project;

public class BuildResults
{
	private List<BuildError> errors = new List<BuildError>();

	private ReadOnlyCollection<BuildError> readOnlyErrors;

	private BuildResultCode result;

	private int errorCount;

	private int warningCount;

	public ReadOnlyCollection<BuildError> Errors
	{
		get
		{
			lock (errors)
			{
				if (readOnlyErrors == null)
				{
					readOnlyErrors = Array.AsReadOnly(errors.ToArray());
				}
				return readOnlyErrors;
			}
		}
	}

	public BuildResultCode Result
	{
		get
		{
			return result;
		}
		set
		{
			result = value;
		}
	}

	public int ErrorCount => errorCount;

	public int WarningCount => warningCount;

	public void Add(BuildError error)
	{
		if (error == null)
		{
			throw new ArgumentNullException("error");
		}
		lock (errors)
		{
			readOnlyErrors = null;
			errors.Add(error);
			if (error.IsWarning)
			{
				warningCount++;
			}
			else
			{
				errorCount++;
			}
		}
	}
}
