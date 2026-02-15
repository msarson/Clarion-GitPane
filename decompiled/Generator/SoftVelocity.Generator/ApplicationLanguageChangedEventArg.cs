namespace SoftVelocity.Generator;

public class ApplicationLanguageChangedEventArg
{
	private string _FileName;

	private string _OldLanguage;

	private string _NewLanguage;

	public string FileName => _FileName;

	public string OldLanguage => _OldLanguage;

	public string NewLanguage => _NewLanguage;

	public ApplicationLanguageChangedEventArg(string fileName, string oldLanguage, string newLanguage)
	{
		_FileName = fileName;
		_OldLanguage = oldLanguage;
		_NewLanguage = newLanguage;
	}
}
