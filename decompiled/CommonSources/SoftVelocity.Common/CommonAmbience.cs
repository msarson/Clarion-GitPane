using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpDevelop.Dom;

namespace SoftVelocity.Common;

public abstract class CommonAmbience : AbstractAmbience
{
	private static string[,] typeConversionList;

	protected static Dictionary<string, string> typeConversionTable;

	public static Dictionary<string, string> TypeConversionTable => typeConversionTable;

	static CommonAmbience()
	{
		typeConversionList = new string[25, 2]
		{
			{ "SYSTEM.BOOLEAN", "BOOL" },
			{ "SYSTEM.BYTE", "BYTE" },
			{ "SYSTEM.SBYTE", "SBYTE" },
			{ "SYSTEM.CHAR", "CHAR" },
			{ "SYSTEM.ENUM", "ENUM" },
			{ "SYSTEM.INT16", "SHORT" },
			{ "SYSTEM.INT32", "SIGNED" },
			{ "SYSTEM.INT64", "LONG" },
			{ "SYSTEM.UINT16", "USHORT" },
			{ "SYSTEM.UINT32", "UNSIGNED" },
			{ "SYSTEM.UINT64", "ULONG" },
			{ "SYSTEM.SINGLE", "SREAL" },
			{ "SYSTEM.DOUBLE", "REAL" },
			{ "SYSTEM.DECIMAL", "DECIMAL" },
			{ "SYSTEM.STRING", "STRING" },
			{ "CLARION.CLADECIMAL", "CLADECIMAL" },
			{ "CLARION.CLASTRING", "CLASTRING" },
			{ "CLARION.CLATIME", "TIME" },
			{ "CLARION.CLADATE", "DATE" },
			{ "CLARION.CLAANY", "ANY" },
			{ "CLARION.CVIEW", "VIEW" },
			{ "CLARION.CQUEUE", "QUEUE" },
			{ "CLARION.CFILE", "FILE" },
			{ "CLARION.CGROUP", "GROUP" },
			{ "CLARION.CKEY", "KEY" }
		};
		typeConversionTable = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		for (int i = 0; i < typeConversionList.GetLength(0); i++)
		{
			typeConversionTable[typeConversionList[i, 0]] = typeConversionList[i, 1];
		}
	}

	public CommonAmbience()
	{
		((AbstractAmbience)this).ConversionFlags = (ConversionFlags)0;
	}

	protected static bool ModifierIsSet(ModifierEnum modifier, ModifierEnum query)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return (ModifierEnum)(modifier & query) == query;
	}

	protected string GetModifier(IDecoration decoration)
	{
		if (decoration == null)
		{
			return string.Empty;
		}
		string text = "";
		if (((AbstractAmbience)this).IncludeHTMLMarkup)
		{
			text += "<i>";
		}
		if (decoration.IsStatic)
		{
			text += "static ";
		}
		else if (decoration.IsSealed)
		{
			text += "final ";
		}
		else if (decoration.IsVirtual)
		{
			text += "virtual ";
		}
		else if (decoration.IsOverride)
		{
			text += "override ";
		}
		else if (decoration.IsNew)
		{
			text += "new ";
		}
		if (((AbstractAmbience)this).IncludeHTMLMarkup)
		{
			text += "</i>";
		}
		return text;
	}

	protected void UnpackNestedType(StringBuilder builder, IReturnType returnType)
	{
		if (returnType == null)
		{
			return;
		}
		if (returnType.IsArrayReturnType)
		{
			builder.Append('[');
			int arrayDimensions = returnType.CastToArrayReturnType().ArrayDimensions;
			for (int i = 1; i < arrayDimensions; i++)
			{
				builder.Append(',');
			}
			builder.Append(']');
			UnpackNestedType(builder, returnType.CastToArrayReturnType().ArrayElementType);
		}
		else
		{
			if (!returnType.IsConstructedReturnType)
			{
				return;
			}
			UnpackNestedType(builder, returnType.CastToConstructedReturnType().UnboundType);
			builder.Append('<');
			IList<IReturnType> typeArguments = returnType.CastToConstructedReturnType().TypeArguments;
			for (int j = 0; j < typeArguments.Count; j++)
			{
				if (j > 0)
				{
					builder.Append(", ");
				}
				builder.Append(((AbstractAmbience)this).Convert(typeArguments[j]));
			}
			builder.Append('>');
		}
	}

	public override string ConvertEnd(IClass c)
	{
		return "END";
	}

	public override string ConvertEnd(IMethod m)
	{
		return string.Empty;
	}

	public override string WrapComment(string comment)
	{
		return "! " + comment;
	}

	public override string GetIntrinsicTypeName(string dotNetTypeName)
	{
		if (typeConversionTable.ContainsKey(dotNetTypeName))
		{
			return typeConversionTable[dotNetTypeName];
		}
		return dotNetTypeName;
	}
}
