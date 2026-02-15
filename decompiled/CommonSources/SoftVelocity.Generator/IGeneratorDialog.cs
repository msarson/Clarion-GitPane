namespace SoftVelocity.Generator;

public interface IGeneratorDialog
{
	bool TryClose();

	bool HaveChanges();

	void Discard();
}
