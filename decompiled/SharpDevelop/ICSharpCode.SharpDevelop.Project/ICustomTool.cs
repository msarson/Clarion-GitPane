namespace ICSharpCode.SharpDevelop.Project;

public interface ICustomTool
{
	void GenerateCode(FileProjectItem item, CustomToolContext context);
}
