using System.Collections;
using System.ComponentModel.Design.Serialization;
using ICSharpCode.FormsDesigner;
using ICSharpCode.TextEditor;
using SoftVelocity.Common.Parser.Ast;

namespace SoftVelocity.Common.FormDesigner;

public class ClaStructureDesignerLoaderProvider : IDesignerLoaderProvider
{
	protected TextEditorControl textEditorControl;

	protected ClaDesignerGenerator.FormDesignerModeenum m_mode = ClaDesignerGenerator.FormDesignerModeenum.Standart;

	protected bool m_isWindowWindow;

	protected ControlContainer m_rcd;

	protected ArrayList m_arr;

	public ClaStructureDesignerLoaderProvider(TextEditorControl textEditorControl)
	{
		this.textEditorControl = textEditorControl;
	}

	public bool PreCreateInitProvider(ClaDesignerGenerator.FormDesignerModeenum mode, ControlContainer rcd, ArrayList arr, bool isWindowWindow)
	{
		m_arr = arr;
		m_rcd = rcd;
		m_mode = mode;
		m_isWindowWindow = isWindowWindow;
		return true;
	}

	public virtual DesignerLoader CreateLoader(IDesignerGenerator generator)
	{
		if (m_arr == null || m_rcd == null || (m_mode != ClaDesignerGenerator.FormDesignerModeenum.ReportDesigner && m_mode != ClaDesignerGenerator.FormDesignerModeenum.WindowDesigner))
		{
			return null;
		}
		return new ClaDesignerLoader(textEditorControl, generator, m_mode, m_rcd, m_arr, m_isWindowWindow);
	}
}
