namespace SoftVelocity.Generator.UI;

public enum ProcedureAttributes
{
	NotEditable = 0,
	Properties = 1,
	ExtraProperties = 2,
	Procedures = 4,
	Extension = 8,
	Tables = 0x10,
	Window = 0x20,
	Report = 0x40,
	Data = 0x80,
	Embeds = 0x100,
	Winform = 0x200,
	Webform = 0x400,
	Compactform = 0x800
}
