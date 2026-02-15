using System.Collections;
using System.ComponentModel.Design.Serialization;
using ICSharpCode.FormsDesigner;
using ICSharpCode.TextEditor;
using SoftVelocity.Common.Parser.Ast;

namespace SoftVelocity.Common.FormDesigner;

public class ClaDesignerLoaderProvider : IDesignerLoaderProvider
{
	protected TextEditorControl textEditorControl;

	protected ClaDesignerGenerator.FormDesignerModeenum m_mode;

	protected bool m_isWindowWindow;

	protected ControlContainer m_rcd;

	protected ArrayList m_arr;

	public ClaDesignerLoaderProvider(TextEditorControl textEditorControl, ClaDesignerGenerator.FormDesignerModeenum mode, ControlContainer rcd, ArrayList arr, bool isWindowWindow)
	{
		m_arr = arr;
		m_rcd = rcd;
		m_mode = mode;
		m_isWindowWindow = isWindowWindow;
		this.textEditorControl = textEditorControl;
	}

	public virtual DesignerLoader CreateLoader(IDesignerGenerator generator)
	{
		return new ClaDesignerLoader(textEditorControl, generator, m_mode, m_rcd, m_arr, m_isWindowWindow);
	}

	public bool ReleaseClaASTree()
	{
		textEditorControl = null;
		m_arr = null;
		m_rcd = null;
		return true;
	}
}
