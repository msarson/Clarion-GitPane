namespace SoftVelocity.Generator;

public interface IEditorDetails : IGenerator
{
	string Language { get; }

	EditorBuildError[] Errors { get; }

	int SelectedErrorIndex { get; }
}
