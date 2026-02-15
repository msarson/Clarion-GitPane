namespace ICSharpCode.SharpDevelop.Project;

public interface IProjectAllowChangeConfigurations
{
	bool RenameProjectConfiguration(string oldName, string newName);

	bool RenameProjectPlatform(string oldName, string newName);

	bool AddProjectConfiguration(string newName, string copyFrom);

	bool AddProjectPlatform(string newName, string copyFrom);

	bool RemoveProjectConfiguration(string name);

	bool RemoveProjectPlatform(string name);
}
