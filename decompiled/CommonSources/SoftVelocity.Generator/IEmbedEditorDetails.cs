using SoftVelocity.Generator.PWEE;

namespace SoftVelocity.Generator;

public interface IEmbedEditorDetails : IPweeDetails, IEditorDetails, IGenerator
{
	string Data { set; }

	ITextSection Text { get; }

	bool PWEEAvailable { get; }
}
