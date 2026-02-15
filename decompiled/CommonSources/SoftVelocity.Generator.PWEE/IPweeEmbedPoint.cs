using System.Collections.Generic;

namespace SoftVelocity.Generator.PWEE;

public interface IPweeEmbedPoint : IPweeText, IPweePart
{
	string Header { get; }

	string Data { set; }

	List<EditorBuildError> Errors { get; }

	uint SelectedErrorIndex { get; }
}
