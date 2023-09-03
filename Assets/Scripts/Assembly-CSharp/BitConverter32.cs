using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct BitConverter32
{
	[FieldOffset(0)]
	public readonly int intValue;

	[FieldOffset(0)]
	public readonly uint uintValue;

	[FieldOffset(0)]
	public readonly float floatValue;

	public BitConverter32(int value)
		: this()
	{
		intValue = value;
	}

	public BitConverter32(uint value)
		: this()
	{
		uintValue = value;
	}

	public BitConverter32(float value)
		: this()
	{
		floatValue = value;
	}

	public static float ToFloat(int value)
	{
		return new BitConverter32(value).floatValue;
	}

	public static float ToFloat(uint value)
	{
		return new BitConverter32(value).floatValue;
	}

	public static int ToInt(float value)
	{
		return new BitConverter32(value).intValue;
	}

	public static uint ToUint(float value)
	{
		return new BitConverter32(value).uintValue;
	}
}
