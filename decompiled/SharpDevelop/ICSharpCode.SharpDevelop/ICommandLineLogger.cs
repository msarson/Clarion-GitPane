namespace ICSharpCode.SharpDevelop;

public interface ICommandLineLogger
{
	void Message(string message);

	void Error(string errorcode, string error);

	void Error(string errorcode, string error, string fileName, int line);

	void Error(string errorcode, string error, string fileName, int line, int column);

	void Warning(string warningcode, string warning);

	void Warning(string warningcode, string warning, string fileName, int line);

	void Warning(string warningcode, string warning, string fileName, int line, int column);
}
