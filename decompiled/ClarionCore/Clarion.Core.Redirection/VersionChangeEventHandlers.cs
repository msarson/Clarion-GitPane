using System;
using Clarion.Core.Options;

namespace Clarion.Core.Redirection;

public class VersionChangeEventHandlers : SoftEventHandler
{
	private RedirectionVersion redVer;

	private Guid id;

	private void MacrosChanged(object sender, MacrosChangedEvent newMacro)
	{
		GuidLinkedList<RedirectionFile.Macros>.Instance.Object(id).MacrosChanged(newMacro);
	}

	internal void Init(Guid id, RedirectionVersion redVer)
	{
		this.id = id;
		this.redVer = redVer;
		RedirectionVersion redirectionVersion = this.redVer;
		redirectionVersion.MacrosChanged = (EventHandler<MacrosChangedEvent>)Delegate.Combine(redirectionVersion.MacrosChanged, new EventHandler<MacrosChangedEvent>(MacrosChanged));
	}

	public override void Detach()
	{
		if (redVer != null)
		{
			RedirectionVersion redirectionVersion = redVer;
			redirectionVersion.MacrosChanged = (EventHandler<MacrosChangedEvent>)Delegate.Remove(redirectionVersion.MacrosChanged, new EventHandler<MacrosChangedEvent>(MacrosChanged));
			redVer = null;
		}
	}
}
