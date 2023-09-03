using UnityEngine;

public class Util : MonoBehaviour
{
	public static float TrueAngle(float InAngle)
	{
		if (Mathf.Abs(InAngle) > 360f)
		{
			InAngle %= 360f;
		}
		if (InAngle >= 180f)
		{
			InAngle -= 360f;
		}
		if (InAngle <= -180f)
		{
			InAngle += 360f;
		}
		return InAngle;
	}

	public static uint Round32ToNextPowerOfTwo(uint value)
	{
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		value++;
		return value;
	}

	public static uint Round16ToNextPowerOfTwo(uint value)
	{
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value++;
		return value;
	}
}
