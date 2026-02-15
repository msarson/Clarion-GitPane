namespace SearchAndReplace;

public class TextSelection
{
	private int offset;

	private int length;

	public int Length
	{
		get
		{
			return length;
		}
		set
		{
			length = value;
		}
	}

	public int Offset
	{
		get
		{
			return offset;
		}
		set
		{
			offset = value;
		}
	}

	public TextSelection(int offset, int length)
	{
		this.offset = offset;
		this.length = length;
	}

	public static bool IsInsideRange(int position, int offset, int length)
	{
		if (position >= offset)
		{
			return position < offset + length;
		}
		return false;
	}
}
