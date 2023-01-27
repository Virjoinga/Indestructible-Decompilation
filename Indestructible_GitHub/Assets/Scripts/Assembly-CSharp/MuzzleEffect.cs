using UnityEngine;

public class MuzzleEffect : ScriptableObject
{
	public string primaryEffectPrefab;

	public float primaryEmitterVelocityScale;

	public Vector4[] primaryOffsetSpeedSizeLifetimeArray;

	public string secondaryEffectPrefab;

	public float secondaryEmitterVelocityScale;

	public Vector4[] secondaryOffsetSpeedSizeLifetimeArray;

	private ParticleEffect _primaryEffect;

	private ParticleEffect _secondaryEffect;

	public void Use()
	{
		if (_primaryEffect == null)
		{
			EffectManager instance = EffectManager.instance;
			_primaryEffect = instance.GetParticleEffect(Resources.Load(primaryEffectPrefab) as GameObject);
			if (!string.IsNullOrEmpty(secondaryEffectPrefab) && secondaryOffsetSpeedSizeLifetimeArray.Length != 0)
			{
				_secondaryEffect = instance.GetParticleEffect(Resources.Load(secondaryEffectPrefab) as GameObject);
			}
			else
			{
				secondaryOffsetSpeedSizeLifetimeArray = null;
			}
		}
	}

	public void Play(Vector3 pos, Vector3 dir)
	{
		int num = 0;
		int num2 = primaryOffsetSpeedSizeLifetimeArray.Length;
		do
		{
			float value = Random.value;
			float num3 = value * 360f - 180f;
			value = value * 0.5f + 0.5f;
			Vector4 vector = primaryOffsetSpeedSizeLifetimeArray[num];
			_primaryEffect.Emit(pos + dir * vector.x, dir * vector.y, vector.z, vector.w, num3, num3);
		}
		while (++num != num2);
		if (secondaryOffsetSpeedSizeLifetimeArray != null)
		{
			num = 0;
			num2 = secondaryOffsetSpeedSizeLifetimeArray.Length;
			do
			{
				float value2 = Random.value;
				float num4 = value2 * 360f - 180f;
				value2 = value2 * 0.5f + 0.5f;
				Vector4 vector2 = secondaryOffsetSpeedSizeLifetimeArray[num];
				_secondaryEffect.Emit(pos + dir * vector2.x, dir * vector2.y, vector2.z, vector2.w, num4, num4);
			}
			while (++num != num2);
		}
	}

	public void Play(Vector3 pos, Vector3 dir, Vector3 emitterVelocity)
	{
		Vector3 vector = emitterVelocity * primaryEmitterVelocityScale;
		float deltaTime = Time.deltaTime;
		int num = 0;
		int num2 = primaryOffsetSpeedSizeLifetimeArray.Length;
		do
		{
			float value = Random.value;
			float num3 = value * 360f - 180f;
			value = value * 0.5f + 0.5f;
			Vector4 vector2 = primaryOffsetSpeedSizeLifetimeArray[num];
			Vector3 vector3 = vector + dir * vector2.y;
			_primaryEffect.Emit(pos + dir * vector2.x - vector3 * deltaTime, vector3, vector2.z * value, vector2.w * value, num3, num3);
		}
		while (++num != num2);
		if (secondaryOffsetSpeedSizeLifetimeArray != null)
		{
			vector = emitterVelocity * secondaryEmitterVelocityScale;
			num = 0;
			num2 = secondaryOffsetSpeedSizeLifetimeArray.Length;
			do
			{
				float value2 = Random.value;
				float num4 = value2 * 360f - 180f;
				value2 = value2 * 0.5f + 0.5f;
				Vector4 vector4 = secondaryOffsetSpeedSizeLifetimeArray[num];
				Vector3 vector5 = vector + dir * vector4.y;
				_secondaryEffect.Emit(pos + dir * vector4.x - vector5 * deltaTime, vector5, vector4.z * value2, vector4.w * value2, num4, num4);
			}
			while (++num != num2);
		}
	}
}
