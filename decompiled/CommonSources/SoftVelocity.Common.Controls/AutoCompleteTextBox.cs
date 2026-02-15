using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SoftVelocity.Common.Controls;

public class AutoCompleteTextBox : TextBox
{
	private ListBox _listBox;

	private bool _isAdded;

	private string[] _values;

	private string _formerValue = string.Empty;

	public string[] Values
	{
		get
		{
			return _values;
		}
		set
		{
			_values = value;
		}
	}

	public List<string> SelectedValues
	{
		get
		{
			string[] collection = Text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			return new List<string>(collection);
		}
	}

	public AutoCompleteTextBox()
	{
		InitializeComponent();
		ResetListBox();
	}

	private void InitializeComponent()
	{
		this._listBox = new System.Windows.Forms.ListBox();
		this._listBox.Width = base.Width;
		base.Resize += new System.EventHandler(AutoCompleteTextBox_Resize);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(this_KeyDown);
		base.KeyUp += new System.Windows.Forms.KeyEventHandler(this_KeyUp);
		this._listBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(_listBox_MouseDoubleClick);
		base.ParentChanged += new System.EventHandler(AutoCompleteTextBox_ParentChanged);
	}

	private void AutoCompleteTextBox_ParentChanged(object sender, EventArgs e)
	{
		_listBox.Parent = base.Parent;
		if (base.Parent != null)
		{
			base.Parent.Resize += Parent_Resize;
		}
	}

	private void Parent_Resize(object sender, EventArgs e)
	{
		RefreshHeight();
	}

	private void _listBox_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		InsertWord((string)_listBox.SelectedItem);
		ResetListBox();
		_formerValue = Text;
	}

	private void AutoCompleteTextBox_Resize(object sender, EventArgs e)
	{
		_listBox.Width = base.Width;
	}

	private void ShowListBox()
	{
		if (!_isAdded)
		{
			base.Parent.Controls.Add(_listBox);
			_listBox.Left = base.Left;
			_listBox.Top = base.Top + base.Height;
			_isAdded = true;
		}
		_listBox.Width = base.Width;
		_listBox.Visible = true;
		_listBox.BringToFront();
	}

	private void ResetListBox()
	{
		_listBox.Visible = false;
	}

	private void this_KeyUp(object sender, KeyEventArgs e)
	{
		UpdateListBox();
	}

	private void this_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Tab:
		case Keys.Return:
			if (_listBox.Visible)
			{
				InsertWord((string)_listBox.SelectedItem);
				ResetListBox();
				_formerValue = Text;
			}
			break;
		case Keys.Down:
			if (_listBox.Visible && _listBox.SelectedIndex < _listBox.Items.Count - 1)
			{
				_listBox.SelectedIndex++;
			}
			break;
		case Keys.Up:
			if (_listBox.Visible && _listBox.SelectedIndex > 0)
			{
				_listBox.SelectedIndex--;
			}
			break;
		}
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (keyData == Keys.Return && _listBox.Visible && _listBox.SelectedIndex > 0)
		{
			return true;
		}
		if (keyData == Keys.Tab)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	private void UpdateListBox()
	{
		if (Text == _formerValue)
		{
			return;
		}
		_formerValue = Text;
		string word = GetWord();
		if (_values != null && word.Length > 0)
		{
			string[] array = Array.FindAll(_values, (string x) => x.StartsWith(word, StringComparison.OrdinalIgnoreCase) && !SelectedValues.Contains(x));
			if (array.Length > 0)
			{
				ShowListBox();
				_listBox.Items.Clear();
				Array.ForEach(array, delegate(string x)
				{
					_listBox.Items.Add(x);
				});
				_listBox.SelectedIndex = 0;
				_listBox.Height = 0;
				Focus();
				RefreshHeight();
			}
			else
			{
				ResetListBox();
			}
		}
		else
		{
			ResetListBox();
		}
	}

	private void RefreshHeight()
	{
		int num = 0;
		Form form = FindForm();
		int num2 = form.ClientSize.Height - base.Top - base.Height;
		using (_listBox.CreateGraphics())
		{
			for (int i = 0; i < _listBox.Items.Count; i++)
			{
				num += _listBox.GetItemHeight(i);
				if (num >= num2)
				{
					break;
				}
				_listBox.Height = num;
			}
			if (_listBox.Height < num2 && _listBox.Height < base.Height)
			{
				_listBox.Height = base.Height;
			}
		}
	}

	private string GetWord()
	{
		string text = Text;
		int num = base.SelectionStart;
		int num2 = text.LastIndexOf(' ', (num >= 1) ? (num - 1) : 0);
		num2 = ((num2 != -1) ? (num2 + 1) : 0);
		int num3 = text.IndexOf(' ', num);
		num3 = ((num3 == -1) ? text.Length : num3);
		int length = ((num3 - num2 >= 0) ? (num3 - num2) : 0);
		return text.Substring(num2, length);
	}

	private void InsertWord(string newTag)
	{
		string text = Text;
		int num = base.SelectionStart;
		int num2 = text.LastIndexOf(' ', (num >= 1) ? (num - 1) : 0);
		num2 = ((num2 != -1) ? (num2 + 1) : 0);
		int num3 = text.IndexOf(' ', num);
		string text2 = text.Substring(0, num2) + newTag;
		string text3 = text2 + ((num3 == -1) ? "" : text.Substring(num3, text.Length - num3));
		Text = text3;
		base.SelectionStart = text2.Length;
	}
}
