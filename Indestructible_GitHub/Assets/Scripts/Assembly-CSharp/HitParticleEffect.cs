using System;
using UnityEngine;

public class HitParticleEffect : ScriptableObject
{
	[Serializable]
	public class EffectOption
	{
		public string effectPrefab;

		[NonSerialized]
		public ParticleEffect effect;

		public int randomCount;

		public Vector2 randomSpeedMinRange;

		public float randomSizeScale;

		public float randomLifetimeScale;

		[NonSerialized]
		public float invRandomDispersionRange;

		[NonSerialized]
		public float invArrayDispersionRange;

		public float randomDispersion;

		public float arrayDispersion;

		public Vector4[] offsetSpeedSizeLifetimeArray;
	}

	public EffectOption[] effectOptions;

	public void Use()
	{
		EffectManager instance = EffectManager.instance;
		int i = 0;
		for (int num = effectOptions.Length; i != num; i++)
		{
			EffectOption effectOption = effectOptions[i];
			if (effectOption.effect != null)
			{
				break;
			}
			effectOption.effect = instance.GetParticleEffect(Resources.Load(effectOption.effectPrefab) as GameObject);
			effectOption.invRandomDispersionRange = 1f / (effectOption.randomDispersion + 1.00001f);
			effectOption.invArrayDispersionRange = 1f / (effectOption.arrayDispersion + 1.00001f);
		}
	}

	public void Play(Vector3 pos, Vector3 normal)
	{
		int i = 0;
		for (int num = effectOptions.Length; i != num; i++)
		{
			EffectOption effectOption = effectOptions[i];
			ParticleEffect effect = effectOption.effect;
			int num2 = effectOption.randomCount;
			while (0 < num2)
			{
				Vector3 randomDirection = GetRandomDirection(normal, effectOption.randomDispersion, effectOption.invRandomDispersionRange);
				effect.EmitRandomSizeLifetimeRotation(pos, randomDirection * (effectOption.randomSpeedMinRange.x + effectOption.randomSpeedMinRange.y * UnityEngine.Random.value), effectOption.randomSizeScale, effectOption.randomLifetimeScale);
				num2--;
			}
			Vector4[] offsetSpeedSizeLifetimeArray = effectOption.offsetSpeedSizeLifetimeArray;
			int j = 0;
			for (int num3 = offsetSpeedSizeLifetimeArray.Length; j != num3; j++)
			{
				Vector3 randomDirection2 = GetRandomDirection(normal, effectOption.arrayDispersion, effectOption.invArrayDispersionRange);
				Vector4 vector = offsetSpeedSizeLifetimeArray[j];
				effect.EmitRandomSizeLifetimeRotation(pos + randomDirection2 * vector.x, randomDirection2 * vector.y, vector.z, vector.w);
			}
		}
	}

	private Vector3 GetRandomDirection(Vector3 axis, float dispersion, float invDispersionRange)
	{
		Vector3 vector = new Vector3(UnityEngine.Random.value * 2f - 1f, UnityEngine.Random.value * 2f - 1f, UnityEngine.Random.value * 2f - 1f);
		vector *= 1f / (vector.magnitude + 0.0002f);
		float num = Vector3.Dot(axis, vector);
		if (num < dispersion)
		{
			num = (dispersion - num) * invDispersionRange;
			vector.x += (axis.x - vector.x) * num;
			vector.y += (axis.y - vector.y) * num;
			vector.z += (axis.z - vector.z) * num;
		}
		return vector;
	}
}
