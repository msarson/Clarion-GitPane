using System.Text;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop;

public class NetAmbience : AbstractAmbience
{
	public override string Convert(ModifierEnum modifier)
	{
		return "";
	}

	public override string Convert(IClass c)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (base.ShowModifiers)
		{
			switch (c.ClassType)
			{
			case ClassType.Delegate:
				stringBuilder.Append("Delegate");
				break;
			case ClassType.Class:
				stringBuilder.Append("Class");
				break;
			case ClassType.Module:
				stringBuilder.Append("Module");
				break;
			case ClassType.Struct:
				stringBuilder.Append("Structure");
				break;
			case ClassType.Interface:
				stringBuilder.Append("Interface");
				break;
			case ClassType.Enum:
				stringBuilder.Append("Enumeration");
				break;
			}
			stringBuilder.Append(' ');
		}
		if (base.UseFullyQualifiedNames)
		{
			stringBuilder.Append(c.FullyQualifiedName);
		}
		else
		{
			stringBuilder.Append(c.Name);
		}
		if (c.TypeParameters.Count > 0)
		{
			stringBuilder.Append('<');
			for (int i = 0; i < c.TypeParameters.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(c.TypeParameters[i].Name);
			}
			stringBuilder.Append('>');
		}
		if (c.ClassType == ClassType.Delegate)
		{
			stringBuilder.Append('(');
			foreach (IMethod method in c.Methods)
			{
				if (method.Name != "Invoke")
				{
					continue;
				}
				for (int j = 0; j < method.Parameters.Count; j++)
				{
					stringBuilder.Append(Convert(method.Parameters[j]));
					if (j + 1 < method.Parameters.Count)
					{
						stringBuilder.Append(", ");
					}
				}
			}
			stringBuilder.Append(')');
			if (c.Methods.Count > 0 && base.ShowReturnType)
			{
				stringBuilder.Append(" : ");
				stringBuilder.Append(Convert(c.Methods[0].ReturnType));
			}
		}
		else if (base.ShowInheritanceList && c.BaseTypes.Count > 0)
		{
			stringBuilder.Append(" : ");
			for (int k = 0; k < c.BaseTypes.Count; k++)
			{
				stringBuilder.Append(c.BaseTypes[k]);
				if (k + 1 < c.BaseTypes.Count)
				{
					stringBuilder.Append(", ");
				}
			}
		}
		if (base.IncludeBodies)
		{
			stringBuilder.Append("\n{");
		}
		return stringBuilder.ToString();
	}

	public override string ConvertEnd(IClass c)
	{
		return "}";
	}

	public override string Convert(IField field)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (base.ShowModifiers)
		{
			stringBuilder.Append("Field");
			stringBuilder.Append(' ');
		}
		if (base.UseFullyQualifiedNames)
		{
			stringBuilder.Append(field.FullyQualifiedName);
		}
		else
		{
			stringBuilder.Append(field.Name);
		}
		if (field.ReturnType != null && base.ShowReturnType)
		{
			stringBuilder.Append(" : ");
			stringBuilder.Append(Convert(field.ReturnType));
		}
		return stringBuilder.ToString();
	}

	public override string Convert(IProperty property)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (base.ShowModifiers)
		{
			if (property.IsIndexer)
			{
				stringBuilder.Append("Indexer ");
			}
			else
			{
				stringBuilder.Append("Property ");
			}
		}
		if (base.UseFullyQualifiedNames)
		{
			stringBuilder.Append(property.FullyQualifiedName);
		}
		else
		{
			stringBuilder.Append(property.Name);
		}
		if (property.Parameters.Count > 0)
		{
			stringBuilder.Append('(');
		}
		for (int i = 0; i < property.Parameters.Count; i++)
		{
			stringBuilder.Append(Convert(property.Parameters[i]));
			if (i + 1 < property.Parameters.Count)
			{
				stringBuilder.Append(", ");
			}
		}
		if (property.Parameters.Count > 0)
		{
			stringBuilder.Append(')');
		}
		if (property.ReturnType != null && base.ShowReturnType)
		{
			stringBuilder.Append(" : ");
			stringBuilder.Append(Convert(property.ReturnType));
		}
		return stringBuilder.ToString();
	}

	public override string Convert(IEvent e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (base.ShowModifiers)
		{
			stringBuilder.Append("Event ");
		}
		if (base.UseFullyQualifiedNames)
		{
			stringBuilder.Append(e.FullyQualifiedName);
		}
		else
		{
			stringBuilder.Append(e.Name);
		}
		if (e.ReturnType != null && base.ShowReturnType)
		{
			stringBuilder.Append(" : ");
			stringBuilder.Append(Convert(e.ReturnType));
		}
		return stringBuilder.ToString();
	}

	public override string Convert(IMethod m)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (base.ShowModifiers)
		{
			stringBuilder.Append("Method ");
		}
		if (base.UseFullyQualifiedNames)
		{
			stringBuilder.Append(m.FullyQualifiedName);
		}
		else
		{
			stringBuilder.Append(m.Name);
		}
		if (m.TypeParameters.Count > 0)
		{
			stringBuilder.Append('<');
			for (int i = 0; i < m.TypeParameters.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(m.TypeParameters[i].Name);
			}
			stringBuilder.Append('>');
		}
		stringBuilder.Append('(');
		for (int j = 0; j < m.Parameters.Count; j++)
		{
			stringBuilder.Append(Convert(m.Parameters[j]));
			if (j + 1 < m.Parameters.Count)
			{
				stringBuilder.Append(", ");
			}
		}
		stringBuilder.Append(")");
		if (m.ReturnType != null && base.ShowReturnType)
		{
			stringBuilder.Append(" : ");
			stringBuilder.Append(Convert(m.ReturnType));
		}
		if (base.IncludeBodies)
		{
			stringBuilder.Append(" {");
		}
		return stringBuilder.ToString();
	}

	public override string ConvertEnd(IMethod m)
	{
		return "}";
	}

	public override string Convert(IReturnType returnType)
	{
		if (returnType == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string dotNetName = returnType.DotNetName;
		if (base.UseFullyQualifiedNames)
		{
			stringBuilder.Append(dotNetName);
		}
		else
		{
			int length = returnType.Namespace.Length;
			stringBuilder.Append(dotNetName, length, dotNetName.Length - length);
		}
		return stringBuilder.ToString();
	}

	public override string Convert(IParameter param)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (base.ShowParameterNames)
		{
			stringBuilder.Append(param.Name);
			stringBuilder.Append(" : ");
		}
		stringBuilder.Append(Convert(param.ReturnType));
		if (param.IsRef)
		{
			stringBuilder.Append("&");
		}
		return stringBuilder.ToString();
	}

	public override string WrapAttribute(string attribute)
	{
		return "[" + attribute + "]";
	}

	public override string WrapComment(string comment)
	{
		return "// " + comment;
	}

	public override string GetIntrinsicTypeName(string dotNetTypeName)
	{
		return dotNetTypeName;
	}
}
