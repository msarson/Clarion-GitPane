using System;
using System.Reflection;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop;

public static class ClassBrowserIconService
{
	public const int NamespaceIndex = 3;

	public const int CombineIndex = 14;

	public const int ConstIndex = 15;

	public const int GotoArrowIndex = 13;

	public const int LocalVariableIndex = 16;

	public const int ParameterIndex = 17;

	public const int ClassIndex = 18;

	public const int StructIndex = 22;

	public const int InterfaceIndex = 26;

	public const int EnumIndex = 30;

	public const int MethodIndex = 34;

	public const int PropertyIndex = 38;

	public const int FieldIndex = 42;

	public const int DelegateIndex = 46;

	public const int EventIndex = 50;

	public const int IndexerIndex = 54;

	private const int internalModifierOffset = 1;

	private const int protectedModifierOffset = 2;

	private const int privateModifierOffset = 3;

	private static ImageList imglist;

	public static ImageList ImageList => imglist;

	private static int GetModifierOffset(ModifierEnum modifier)
	{
		if ((modifier & ModifierEnum.Public) == ModifierEnum.Public)
		{
			return 0;
		}
		if ((modifier & ModifierEnum.Protected) == ModifierEnum.Protected)
		{
			return 2;
		}
		if ((modifier & ModifierEnum.Internal) == ModifierEnum.Internal)
		{
			return 1;
		}
		return 3;
	}

	public static int GetIcon(IMember member)
	{
		if (member is IMethod)
		{
			return GetIcon(member as IMethod);
		}
		if (member is IProperty)
		{
			return GetIcon(member as IProperty);
		}
		if (member is IField)
		{
			return GetIcon(member as IField);
		}
		if (member is IEvent)
		{
			return GetIcon(member as IEvent);
		}
		throw new ArgumentException("unknown member type");
	}

	public static int GetIcon(IMethod method)
	{
		return 34 + GetModifierOffset(method.Modifiers);
	}

	public static int GetIcon(IProperty property)
	{
		if (property.IsIndexer)
		{
			return 54 + GetModifierOffset(property.Modifiers);
		}
		return 38 + GetModifierOffset(property.Modifiers);
	}

	public static int GetIcon(IField field)
	{
		if (field.IsConst)
		{
			return 15;
		}
		if (field.IsParameter)
		{
			return 17;
		}
		if (field.IsLocalVariable)
		{
			return 16;
		}
		return 42 + GetModifierOffset(field.Modifiers);
	}

	public static int GetIcon(IEvent evt)
	{
		return 50 + GetModifierOffset(evt.Modifiers);
	}

	public static int GetIcon(IClass c)
	{
		int num = 18;
		switch (c.ClassType)
		{
		case ClassType.Delegate:
			num = 46;
			break;
		case ClassType.Enum:
			num = 30;
			break;
		case ClassType.Struct:
			num = 22;
			break;
		case ClassType.Interface:
			num = 26;
			break;
		}
		return num + GetModifierOffset(c.Modifiers);
	}

	public static int GetIcon(MethodBase methodinfo)
	{
		if (methodinfo.IsAssembly)
		{
			return 35;
		}
		if (methodinfo.IsPrivate)
		{
			return 37;
		}
		if (!methodinfo.IsPrivate && !methodinfo.IsPublic)
		{
			return 36;
		}
		return 34;
	}

	public static int GetIcon(PropertyInfo propertyinfo)
	{
		if (propertyinfo.CanRead && propertyinfo.GetGetMethod(nonPublic: true) != null)
		{
			return 38 + GetIcon(propertyinfo.GetGetMethod(nonPublic: true)) - 34;
		}
		if (propertyinfo.CanWrite && propertyinfo.GetSetMethod(nonPublic: true) != null)
		{
			return 38 + GetIcon(propertyinfo.GetSetMethod(nonPublic: true)) - 34;
		}
		return 38;
	}

	public static int GetIcon(FieldInfo fieldinfo)
	{
		if (fieldinfo.IsLiteral)
		{
			return 15;
		}
		if (fieldinfo.IsAssembly)
		{
			return 43;
		}
		if (fieldinfo.IsPrivate)
		{
			return 45;
		}
		if (!fieldinfo.IsPrivate && !fieldinfo.IsPublic)
		{
			return 44;
		}
		return 42;
	}

	public static int GetIcon(EventInfo eventinfo)
	{
		if (eventinfo.GetAddMethod(nonPublic: true) != null)
		{
			return 50 + GetIcon(eventinfo.GetAddMethod(nonPublic: true)) - 34;
		}
		return 50;
	}

	public static int GetIcon(Type type)
	{
		int num = 18;
		if (type.IsValueType)
		{
			num = 22;
		}
		if (type.IsEnum)
		{
			num = 30;
		}
		if (type.IsInterface)
		{
			num = 26;
		}
		if (type.IsSubclassOf(typeof(Delegate)))
		{
			num = 46;
		}
		if (type.IsNestedPrivate)
		{
			return num + 3;
		}
		if (type.IsNotPublic || type.IsNestedAssembly)
		{
			return num + 1;
		}
		if (type.IsNestedFamily)
		{
			return num + 2;
		}
		return num;
	}

	static ClassBrowserIconService()
	{
		imglist = new ImageList();
		imglist.ColorDepth = ColorDepth.Depth32Bit;
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Assembly"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.OpenAssembly"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Library"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.NameSpace"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.SubTypes"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.SuperTypes"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ClosedFolderBitmap"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.OpenFolderBitmap"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Reference"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ClosedReferenceFolder"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.OpenReferenceFolder"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ResourceFileIcon"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Event"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.SelectionArrow"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.CombineIcon"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Literal"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Local"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Parameter"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Class"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalClass"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedClass"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateClass"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Struct"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalStruct"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedStruct"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateStruct"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Interface"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalInterface"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedInterface"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateInterface"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Enum"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalEnum"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedEnum"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateEnum"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Method"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalMethod"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedMethod"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateMethod"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Property"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalProperty"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedProperty"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateProperty"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Field"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalField"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedField"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateField"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Delegate"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalDelegate"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedDelegate"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateDelegate"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Event"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalEvent"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedEvent"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateEvent"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.Indexer"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.InternalIndexer"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.ProtectedIndexer"));
		imglist.Images.Add(ResourceService.GetBitmap("Icons.16x16.PrivateIndexer"));
	}
}
