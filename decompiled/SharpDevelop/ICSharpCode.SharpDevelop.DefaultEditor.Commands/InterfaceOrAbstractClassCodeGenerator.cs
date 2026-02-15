using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public abstract class InterfaceOrAbstractClassCodeGenerator : CodeGeneratorBase
{
	protected class ClassWrapper
	{
		private IReturnType c;

		public IReturnType ClassType => c;

		public ClassWrapper(IReturnType c)
		{
			this.c = c;
		}

		public override string ToString()
		{
			IAmbience currentAmbience = AmbienceService.CurrentAmbience;
			currentAmbience.ConversionFlags = ConversionFlags.None;
			return currentAmbience.Convert(c);
		}
	}

	public override int ImageIndex => 26;
}
