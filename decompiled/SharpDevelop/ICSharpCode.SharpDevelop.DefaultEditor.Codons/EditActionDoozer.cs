using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.TextEditor.Actions;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Codons;

public class EditActionDoozer : IDoozer
{
	public bool HandleConditions => false;

	public object BuildItem(object caller, Codon codon, ArrayList subItems)
	{
		IEditAction editAction = (IEditAction)codon.AddIn.CreateObject(codon.Properties["class"]);
		string[] array = codon.Properties["keys"].Split(',');
		Keys[] array2 = new Keys[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string[] array3 = array[i].Split('|');
			Keys keys = (Keys)Keys.Space.GetType().InvokeMember(array3[0], BindingFlags.GetField, null, Keys.Space, new object[0]);
			for (int j = 1; j < array3.Length; j++)
			{
				keys |= (Keys)Keys.Space.GetType().InvokeMember(array3[j], BindingFlags.GetField, null, Keys.Space, new object[0]);
			}
			array2[i] = keys;
		}
		editAction.Keys = array2;
		return editAction;
	}
}
