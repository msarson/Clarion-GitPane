namespace ICSharpCode.SharpDevelop.Gui;

public interface ISelectReferenceDialog
{
	void AddReference(ReferenceType referenceType, string referenceName, string referenceLocation, object tag);
}
