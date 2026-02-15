using System;
using System.Collections.Generic;

namespace Clarion.Core.Options;

public class MacrosChangedEvent : EventArgs
{
	private Dictionary<string, string> newValues;

	public Dictionary<string, string> List => newValues;

	public MacrosChangedEvent(Dictionary<string, string> newValues)
	{
		this.newValues = newValues;
	}
}
