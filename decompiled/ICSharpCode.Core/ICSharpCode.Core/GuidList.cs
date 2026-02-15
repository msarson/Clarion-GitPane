using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ICSharpCode.Core;

[Serializable]
public class GuidList : List<Guid>, ISerializable
{
	public GuidList()
	{
	}

	public GuidList(SerializationInfo info, StreamingContext context)
		: this()
	{
		int @int = info.GetInt16("count");
		for (int i = 0; i < @int; i++)
		{
			Add((Guid)info.GetValue("id" + i, typeof(Guid)));
		}
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("count", base.Count);
		for (int i = 0; i < base.Count; i++)
		{
			info.AddValue("id" + i, base[i]);
		}
	}
}
