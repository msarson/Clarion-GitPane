using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using SoftVelocity.Generator.Src.UIBinding.ApplicationEditor.Dialogs;

namespace SoftVelocity.Generator.Commands;

public class EditABCPathsCommand : AbstractMenuCommand
{
	public override void Run()
	{
		EditABCPaths editABCPaths = new EditABCPaths();
		try
		{
			((Form)(object)editABCPaths).ShowDialog();
		}
		finally
		{
			((IDisposable)editABCPaths)?.Dispose();
		}
	}
}
