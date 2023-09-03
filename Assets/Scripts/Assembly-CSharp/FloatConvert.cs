public class FloatConvert
{
	public static uint NormalizedToUint(float value, int bitCount)
	{
		return (uint)(value * (float)((1 << bitCount) - 1));
	}

	public static float NormalizedFromUint(uint value, int bitCount)
	{
		return (float)value / (float)((1 << bitCount) - 1);
	}

	public static uint ToUint(float value, float min, float invRange, int bitCount)
	{
		return NormalizedToUint((value - min) * invRange, bitCount);
	}

	public static float FromUint(uint value, float min, float range, int bitCount)
	{
		return NormalizedFromUint(value, bitCount) * range + min;
	}

	public static uint NormalizedToUint4(float value)
	{
		return (uint)(value * 15f);
	}

	public static float NormalizedFromUint4(uint value)
	{
		return (float)value * (1f / 15f);
	}

	public static uint ToUint4(float value, float min, float invRange)
	{
		return (uint)((value - min) * (15f * invRange));
	}

	public static float FromUint4(uint value, float min, float range)
	{
		return (float)value * (range * (1f / 15f)) + min;
	}

	public static uint NormalizedToUint8(float value)
	{
		return (uint)(value * 255f);
	}

	public static float NormalizedFromUint8(uint value)
	{
		return (float)value * 0.003921569f;
	}

	public static uint ToUint8(float value, float min, float invRange)
	{
		return (uint)((value - min) * (255f * invRange));
	}

	public static float FromUint8(uint value, float min, float range)
	{
		return (float)value * (range * 0.003921569f) + min;
	}

	public static uint NormalizedToUint9(float value)
	{
		return (uint)(value * 511f);
	}

	public static float NormalizedFromUint9(uint value)
	{
		return (float)value * 0.0019569471f;
	}

	public static uint ToUint9(float value, float min, float invRange)
	{
		return (uint)((value - min) * (511f * invRange));
	}

	public static float FromUint9(uint value, float min, float range)
	{
		return (float)value * (range * 0.0019569471f) + min;
	}

	public static uint NormalizedToUint10(float value)
	{
		return (uint)(value * 1023f);
	}

	public static float NormalizedFromUint10(uint value)
	{
		return (float)value * 0.0009775171f;
	}

	public static uint ToUint10(float value, float min, float invRange)
	{
		return (uint)((value - min) * (1023f * invRange));
	}

	public static float FromUint10(uint value, float min, float range)
	{
		return (float)value * (range * 0.0009775171f) + min;
	}

	public static uint NormalizedToUint11(float value)
	{
		return (uint)(value * 2047f);
	}

	public static float NormalizedFromUint11(uint value)
	{
		return (float)value * 0.0004885198f;
	}

	public static uint ToUint11(float value, float min, float invRange)
	{
		return (uint)((value - min) * (2047f * invRange));
	}

	public static float FromUint11(uint value, float min, float range)
	{
		return (float)value * (range * 0.0004885198f) + min;
	}

	public static uint NormalizedToUint12(float value)
	{
		return (uint)(value * 4095f);
	}

	public static float NormalizedFromUint12(uint value)
	{
		return (float)value * 0.00024420026f;
	}

	public static uint ToUint12(float value, float min, float invRange)
	{
		return (uint)((value - min) * (4095f * invRange));
	}

	public static float FromUint12(uint value, float min, float range)
	{
		return (float)value * (range * 0.00024420026f) + min;
	}

	public static uint NormalizedToUint14(float value)
	{
		return (uint)(value * 16383f);
	}

	public static float NormalizedFromUint14(uint value)
	{
		return (float)value * 6.103888E-05f;
	}

	public static uint ToUint14(float value, float min, float invRange)
	{
		return (uint)((value - min) * (16383f * invRange));
	}

	public static float FromUint14(uint value, float min, float range)
	{
		return (float)value * (range * 6.103888E-05f) + min;
	}

	public static uint NormalizedToUint16(float value)
	{
		return (uint)(value * 65535f);
	}

	public static float NormalizedFromUint16(uint value)
	{
		return (float)value * 1.5259022E-05f;
	}

	public static uint ToUint16(float value, float min, float invRange)
	{
		return (uint)((value - min) * (65535f * invRange));
	}

	public static float FromUint16(uint value, float min, float range)
	{
		return (float)value * (range * 1.5259022E-05f) + min;
	}
}
