using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Commands;

public abstract class AbstractClipboardCommand : AbstractMenuCommand
{
	private class TextBoxWrapper : IClipboardHandler
	{
		private TextBoxBase textBox;

		public bool EnableCut
		{
			get
			{
				if (!textBox.ReadOnly)
				{
					return textBox.SelectionLength > 0;
				}
				return false;
			}
		}

		public bool EnableCopy => textBox.SelectionLength > 0;

		public bool EnablePaste => !textBox.ReadOnly;

		public bool EnableDelete
		{
			get
			{
				if (!textBox.ReadOnly)
				{
					return textBox.SelectionLength > 0;
				}
				return false;
			}
		}

		public bool EnableSelectAll => textBox.TextLength > 0;

		public TextBoxWrapper(TextBoxBase textBox)
		{
			this.textBox = textBox;
		}

		public void Cut()
		{
			textBox.Cut();
		}

		public void Copy()
		{
			textBox.Copy();
		}

		public void Paste()
		{
			textBox.Paste();
		}

		public void Delete()
		{
			textBox.SelectedText = "";
		}

		public void SelectAll()
		{
			textBox.SelectAll();
		}
	}

	private class ComboBoxWrapper : IClipboardHandler
	{
		private ComboBox comboBox;

		public bool EnableCut => comboBox.SelectionLength > 0;

		public bool EnableCopy => comboBox.SelectionLength > 0;

		public bool EnablePaste => ClipboardWrapper.ContainsText;

		public bool EnableDelete => true;

		public bool EnableSelectAll => comboBox.Text.Length > 0;

		public ComboBoxWrapper(ComboBox comboBox)
		{
			this.comboBox = comboBox;
		}

		public void Cut()
		{
			ClipboardWrapper.SetText(comboBox.SelectedText);
			comboBox.SelectedText = "";
		}

		public void Copy()
		{
			ClipboardWrapper.SetText(comboBox.SelectedText);
		}

		public void Paste()
		{
			comboBox.SelectedText = ClipboardWrapper.GetText();
		}

		public void Delete()
		{
			comboBox.SelectedText = "";
		}

		public void SelectAll()
		{
			comboBox.SelectAll();
		}
	}

	public override bool IsEnabled
	{
		get
		{
			IClipboardHandler clipboardHandler = WorkbenchSingleton.Workbench.ActiveContent as IClipboardHandler;
			if (clipboardHandler == null)
			{
				clipboardHandler = GetClipboardHandlerWrapper(WorkbenchSingleton.ActiveControl);
			}
			if (clipboardHandler != null)
			{
				return GetEnabled(clipboardHandler);
			}
			return false;
		}
	}

	protected abstract bool GetEnabled(IClipboardHandler editable);

	protected abstract void Run(IClipboardHandler editable);

	public static IClipboardHandler GetClipboardHandlerWrapper(Control ctl)
	{
		if (ctl is TextBoxBase textBox)
		{
			return new TextBoxWrapper(textBox);
		}
		if (ctl is ComboBox { DropDownStyle: not ComboBoxStyle.DropDownList } comboBox)
		{
			return new ComboBoxWrapper(comboBox);
		}
		return null;
	}

	public override void Run()
	{
		IClipboardHandler clipboardHandler = WorkbenchSingleton.Workbench.ActiveContent as IClipboardHandler;
		if (clipboardHandler == null)
		{
			clipboardHandler = GetClipboardHandlerWrapper(WorkbenchSingleton.ActiveControl);
		}
		if (clipboardHandler != null)
		{
			Run(clipboardHandler);
		}
	}
}
