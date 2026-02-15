using System;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui;

public class ShowOutputFromComboBox : AbstractComboBoxCommand
{
	private ComboBox comboBox;

	protected override void OnOwnerChanged(EventArgs e)
	{
		base.OnOwnerChanged(e);
		ToolBarComboBox toolBarComboBox = (ToolBarComboBox)Owner;
		comboBox = toolBarComboBox.ComboBox;
		SetItems();
		CompilerMessageView.Instance.MessageCategoryAdded += CompilerMessageViewMessageCategoryAdded;
		CompilerMessageView.Instance.SelectedCategoryIndexChanged += CompilerMessageViewSelectedCategoryIndexChanged;
		comboBox.SelectedIndex = 0;
		comboBox.SelectedIndexChanged += ComboBoxSelectedIndexChanged;
	}

	private void CompilerMessageViewSelectedCategoryIndexChanged(object sender, EventArgs e)
	{
		if (comboBox.SelectedIndex != CompilerMessageView.Instance.SelectedCategoryIndex)
		{
			comboBox.SelectedIndex = CompilerMessageView.Instance.SelectedCategoryIndex;
		}
	}

	private void ComboBoxSelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboBox.SelectedIndex != CompilerMessageView.Instance.SelectedCategoryIndex)
		{
			CompilerMessageView.Instance.SelectedCategoryIndex = comboBox.SelectedIndex;
		}
	}

	private void CompilerMessageViewMessageCategoryAdded(object sender, EventArgs e)
	{
		SetItems();
	}

	private void SetItems()
	{
		comboBox.Items.Clear();
		foreach (MessageViewCategory messageCategory in CompilerMessageView.Instance.MessageCategories)
		{
			comboBox.Items.Add(StringParser.Parse(messageCategory.DisplayCategory));
		}
	}

	public override void Run()
	{
	}
}
