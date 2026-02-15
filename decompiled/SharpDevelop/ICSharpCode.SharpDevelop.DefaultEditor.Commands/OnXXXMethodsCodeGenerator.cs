using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class OnXXXMethodsCodeGenerator : CodeGeneratorBase
{
	private class EventWrapper
	{
		private IEvent evt;

		public IEvent Event => evt;

		public EventWrapper(IEvent evt)
		{
			this.evt = evt;
		}

		public override string ToString()
		{
			IAmbience currentAmbience = AmbienceService.CurrentAmbience;
			currentAmbience.ConversionFlags = ConversionFlags.None;
			return currentAmbience.Convert(evt);
		}
	}

	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.EventOnXXX}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.EventOnXXX.Hint}";

	public override int ImageIndex => 50;

	protected override void InitContent()
	{
		foreach (IEvent @event in currentClass.Events)
		{
			base.Content.Add(new EventWrapper(@event));
		}
	}

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		foreach (EventWrapper item in items)
		{
			nodes.Add(codeGen.CreateOnEventMethod(item.Event));
		}
	}
}
