using System;

namespace ICSharpCode.SharpDevelop.Project;

public struct ItemType : IEquatable<ItemType>, IComparable<ItemType>
{
	public static readonly ItemType Reference = new ItemType("Reference");

	public static readonly ItemType ProjectReference = new ItemType("ProjectReference");

	public static readonly ItemType COMReference = new ItemType("COMReference");

	public static readonly ItemType Import = new ItemType("Import");

	public static readonly ItemType WebReferenceUrl = new ItemType("WebReferenceUrl");

	public static readonly ItemType Compile = new ItemType("Compile");

	public static readonly ItemType EmbeddedResource = new ItemType("EmbeddedResource");

	public static readonly ItemType None = new ItemType("None");

	public static readonly ItemType Content = new ItemType("Content");

	public static readonly ItemType ApplicationDefinition = new ItemType("ApplicationDefinition");

	public static readonly ItemType Page = new ItemType("Page");

	public static readonly ItemType BootstrapperFile = new ItemType("BootstrapperFile");

	public static readonly ItemType Application = new ItemType("Application");

	public static readonly ReadOnlyCollectionWrapper<ItemType> DefaultFileItems = new Set<ItemType>(Compile, EmbeddedResource, None, Content).AsReadOnly();

	public static readonly ItemType Resource = new ItemType("Resource");

	public static readonly ItemType Folder = new ItemType("Folder");

	public static readonly ItemType WebReferences = new ItemType("WebReferences");

	private readonly string itemName;

	public string ItemName => itemName;

	public ItemType(string itemName)
	{
		if (itemName == null)
		{
			throw new ArgumentNullException("itemName");
		}
		this.itemName = itemName;
	}

	public override string ToString()
	{
		return itemName;
	}

	public override bool Equals(object obj)
	{
		if (obj is ItemType)
		{
			return Equals((ItemType)obj);
		}
		return false;
	}

	public bool Equals(ItemType other)
	{
		return itemName == other.itemName;
	}

	public override int GetHashCode()
	{
		return itemName.GetHashCode();
	}

	public static bool operator ==(ItemType lhs, ItemType rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(ItemType lhs, ItemType rhs)
	{
		return !lhs.Equals(rhs);
	}

	public int CompareTo(ItemType other)
	{
		return itemName.CompareTo(other.itemName);
	}
}
