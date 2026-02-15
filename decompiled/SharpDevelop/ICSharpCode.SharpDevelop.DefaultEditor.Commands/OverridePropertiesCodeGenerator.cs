using System;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.DefaultEditor.Commands;

public class OverridePropertiesCodeGenerator : CodeGeneratorBase
{
	private class PropertyWrapper : IComparable
	{
		private IProperty property;

		public IProperty Property => property;

		public int CompareTo(object other)
		{
			return property.Name.CompareTo(((PropertyWrapper)other).property.Name);
		}

		public PropertyWrapper(IProperty property)
		{
			this.property = property;
		}

		public override string ToString()
		{
			IAmbience currentAmbience = AmbienceService.CurrentAmbience;
			currentAmbience.ConversionFlags = ConversionFlags.ShowParameterNames;
			return currentAmbience.Convert(property);
		}
	}

	public override string CategoryName => "${res:ICSharpCode.SharpDevelop.CodeGenerator.OverrideProperties}";

	public override string Hint => "${res:ICSharpCode.SharpDevelop.CodeGenerator.OverrideProperties.Hint}";

	public override int ImageIndex => 38;

	protected override void InitContent()
	{
		IProperty[] overridableProperties = OverrideCompletionDataProvider.GetOverridableProperties(currentClass);
		foreach (IProperty property in overridableProperties)
		{
			base.Content.Add(new PropertyWrapper(property));
		}
		base.Content.Sort();
	}

	public override void GenerateCode(List<AbstractNode> nodes, IList items)
	{
		foreach (PropertyWrapper item in items)
		{
			nodes.Add(codeGen.GetOverridingMethod(item.Property, classFinderContext));
		}
	}
}
