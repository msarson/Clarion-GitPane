namespace SoftVelocity.Generator.UI;

public interface IAppTitleManager
{
	string HeaderTitle { get; }

	void RemoveCurrentHeaderTitle();

	void SetHeaderTitle(string title);

	void ReplaceHeaderTitle(string title);

	void AppendHeaderTitle(string title);
}
