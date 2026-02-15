using System;
using System.Collections.Generic;

namespace SoftVelocity.Generator;

public class ClarionTemplateParsedFunction : IComparer<ClarionTemplateParsedFunction>, IComparer<KeyValuePair<string, List<ClarionTemplateParsedFunction>>>
{
	private int lineNumber;

	private string functionType;

	private string functionName;

	private string functionDescription;

	public int LineNumber => lineNumber;

	public string FunctionType => functionType;

	public string FunctionName => functionName;

	public string FunctionDescription
	{
		get
		{
			if (functionDescription == null)
			{
				return "";
			}
			return functionDescription;
		}
	}

	public ClarionTemplateParsedFunction(int lineNumber, string functionType, string functionName, string functionDescription)
	{
		this.lineNumber = lineNumber;
		this.functionType = functionType;
		this.functionName = functionName;
		this.functionDescription = functionDescription;
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(functionDescription))
		{
			return functionName + " - " + functionDescription;
		}
		return functionName;
	}

	public int Compare(object x, object y)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public int Compare(ClarionTemplateParsedFunction x, ClarionTemplateParsedFunction y)
	{
		if (x == null || y == null)
		{
			return 0;
		}
		int num = 0;
		if (x.functionType == y.functionType)
		{
			num = x.functionName.CompareTo(y.functionName);
			if (num == 0)
			{
				num = x.FunctionDescription.CompareTo(y.FunctionDescription);
			}
		}
		else
		{
			num = GetTemplateTypeSort(x.functionType).CompareTo(GetTemplateTypeSort(y.functionType));
		}
		return num;
	}

	internal static int GetTemplateTypeSort(string templateType)
	{
		if (templateType == null)
		{
			return -1;
		}
		return templateType switch
		{
			"PROGRAM" => 1, 
			"SYSTEM" => 2, 
			"MODULE" => 3, 
			"PROCEDURE" => 4, 
			"CONTROL" => 5, 
			"CODE" => 6, 
			"EXTENSION" => 7, 
			"GROUP" => 8, 
			_ => 0, 
		};
	}

	public int Compare(KeyValuePair<string, List<ClarionTemplateParsedFunction>> x, KeyValuePair<string, List<ClarionTemplateParsedFunction>> y)
	{
		return GetTemplateTypeSort(x.Key).CompareTo(GetTemplateTypeSort(y.Key));
	}
}
