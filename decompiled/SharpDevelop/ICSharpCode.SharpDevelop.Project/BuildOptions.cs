using System;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop.Project;

public class BuildOptions : IDisposable
{
	private BuildCallback callback;

	private BuildTarget target;

	private IDictionary<string, string> additionalProperties;

	private bool disposed;

	public BuildCallback Callback => callback;

	public BuildTarget Target => target;

	public IDictionary<string, string> AdditionalProperties => additionalProperties;

	public BuildOptions()
	{
		target = BuildTarget.Build;
	}

	public BuildOptions(BuildTarget target, BuildCallback callback)
	{
		this.callback = callback;
		this.target = target;
		additionalProperties = new Dictionary<string, string>();
	}

	public BuildOptions(BuildTarget target, BuildCallback callback, IDictionary<string, string> additionalProperties)
	{
		this.callback = callback;
		this.target = target;
		this.additionalProperties = additionalProperties;
	}

	public void Dispose()
	{
		if (!disposed)
		{
			callback = null;
			additionalProperties = null;
			disposed = true;
		}
	}
}
