using System.Collections.Generic;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Codons;

public class AddInTreeSyntaxModeProvider : ISyntaxModeFileProvider
{
	private const string syntaxModePath = "/SharpDevelop/ViewContent/DefaultTextEditor/SyntaxModes";

	private List<SyntaxMode> syntaxModes;

	public ICollection<SyntaxMode> SyntaxModes => syntaxModes;

	public AddInTreeSyntaxModeProvider()
	{
		syntaxModes = AddInTree.BuildItems<SyntaxMode>("/SharpDevelop/ViewContent/DefaultTextEditor/SyntaxModes", this, throwOnNotFound: false);
	}

	public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
	{
		return ((AddInTreeSyntaxMode)syntaxMode).CreateTextReader();
	}

	public void UpdateSyntaxModeList()
	{
	}
}
