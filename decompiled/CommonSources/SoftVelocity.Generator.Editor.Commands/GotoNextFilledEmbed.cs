namespace SoftVelocity.Generator.Editor.Commands;

public class GotoNextFilledEmbed : AbstractGenEditorCommand
{
	public override bool IsEnabled
	{
		get
		{
			CommonGenEditor genEditor = base.GenEditor;
			if (genEditor != null)
			{
				return !genEditor.IsOnLastFilledEmbed;
			}
			return false;
		}
		set
		{
		}
	}

	public override void Run()
	{
		base.GenEditor?.GotoNextFilledEmbed();
	}
}
