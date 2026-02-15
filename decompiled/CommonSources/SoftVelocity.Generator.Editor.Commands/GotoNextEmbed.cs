namespace SoftVelocity.Generator.Editor.Commands;

public class GotoNextEmbed : AbstractGenEditorCommand
{
	public override bool IsEnabled
	{
		get
		{
			CommonGenEditor genEditor = base.GenEditor;
			if (genEditor != null)
			{
				return !genEditor.IsOnLastEmbed;
			}
			return false;
		}
		set
		{
		}
	}

	public override void Run()
	{
		base.GenEditor?.GotoNextEmbed();
	}
}
