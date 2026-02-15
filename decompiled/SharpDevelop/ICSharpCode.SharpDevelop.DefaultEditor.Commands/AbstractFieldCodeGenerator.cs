using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public abstract class AbstractFieldCodeGenerator : CodeGeneratorBase
{
	public class FieldWrapper
	{
		private IField field;

		public IField Field => field;

		public FieldWrapper(IField field)
		{
			this.field = field;
		}

		public override string ToString()
		{
			IAmbience currentAmbience = AmbienceService.CurrentAmbience;
			currentAmbience.ConversionFlags = ConversionFlags.ShowModifiers | ConversionFlags.ShowReturnType;
			return currentAmbience.Convert(field);
		}
	}

	protected override void InitContent()
	{
		foreach (IField field in currentClass.Fields)
		{
			base.Content.Add(new FieldWrapper(field));
		}
	}
}
