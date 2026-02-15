using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class OverrideMethodsCodeGenerator : CodeGeneratorBase
{
	private class MethodWrapper : IComparable
	{
		private IMethod method;

		private string cachedStringRepresentation;

		public IMethod Method => method;

		public int CompareTo(object other)
		{
			return ToString().CompareTo(((MethodWrapper)other).ToString());
		}

		public MethodWrapper(IMethod method)
		{
			this.method = method;
		}

		public override bool Equals(object obj)
		{
			MethodWrapper methodWrapper = (MethodWrapper)obj;
			if (method.Name != methodWrapper.method.Name)
			{
				return false;
			}
			return 0 == DiffUtility.Compare(method.Parameters, methodWrapper.method.Parameters);
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override string ToString()
		{
			if (cachedStringRepresentation == null)
			{
				IAmbience currentAmbience = AmbienceService.CurrentAmbience;
				currentAmbience.ConversionFlags = ConversionFlags.ShowParameterNames;
				cachedStringRepresentation = currentAmbience.Convert(method);
			}
			return cachedStringRepresentation;
		}
	}

	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.OverrideMethods}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.OverrideMethods.Hint}";

	public override int ImageIndex => 34;

	protected override void InitContent()
	{
		IMethod[] overridableMethods = OverrideCompletionDataProvider.GetOverridableMethods(currentClass);
		foreach (IMethod method in overridableMethods)
		{
			base.Content.Add(new MethodWrapper(method));
		}
		base.Content.Sort();
	}

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		foreach (MethodWrapper item in items)
		{
			nodes.Add(codeGen.GetOverridingMethod(item.Method, classFinderContext));
		}
	}
}
