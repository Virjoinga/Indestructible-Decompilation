using System;
using UnityEngine;

[Serializable]
public class Differential
{
	public float staticDistribution = 0.5f;

	public float maxTransferFactor = 1f;

	public float DistributeTorque(float side1Value, float side2Value)
	{
		return DistributeTorque(side1Value, side2Value, staticDistribution, maxTransferFactor);
	}

	public static float DistributeTorque(float side1Value, float side2Value, float staticDistribution, float maxTransferFactor)
	{
		float num = Mathf.Abs(side1Value);
		float num2 = Mathf.Abs(side2Value);
		float num3 = num - num2;
		if (num3 != 0f)
		{
			float num4 = num3 / Mathf.Max(num, num2);
			float to = Mathf.Lerp(num4, 0f - num4, maxTransferFactor) * 0.5f + 0.5f;
			return Mathf.Lerp(staticDistribution, to, Mathf.Abs(num4));
		}
		return staticDistribution;
	}
}
