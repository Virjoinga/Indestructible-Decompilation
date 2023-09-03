using System;
using UnityEngine;

public class RocketTrailEffect : ScriptableObject
{
	private enum EmissionState
	{
		ShouldEmitFire = 1,
		ShouldEmitSmoke = 2
	}

	public string firePrefab;

	public float fireEmissionRate;

	public Vector4 primaryFireOffsetVelocitySizeLifetime;

	public Vector4 secondaryFireOffsetVelocitySizeLifetime;

	public string smokePrefab;

	public float smokeEmissionRateScale;

	public Vector4 smokeOffsetVelocitySizeLifetime;

	[NonSerialized]
	private EmissionState _emissionState;

	[NonSerialized]
	private float _prevCalculationTime;

	[NonSerialized]
	private float _fireEmissionAccCount;

	[NonSerialized]
	private float _smokeEmissionAccCount;

	[NonSerialized]
	private float _rotation;

	[NonSerialized]
	private float _angularVelocity;

	[NonSerialized]
	private ParticleEffect _fireEffect;

	[NonSerialized]
	private float _smokeSize;

	[NonSerialized]
	private float _smokeLifetime;

	[NonSerialized]
	private ParticleEffect _smokeEffect;

	[NonSerialized]
	private bool _hasFireEffect;

	[NonSerialized]
	private bool _hasSmokeEffect;

	public void Use()
	{
		if (_fireEffect == null)
		{
			if (!string.IsNullOrEmpty(firePrefab))
			{
				_fireEffect = EffectManager.instance.GetParticleEffect(Resources.Load(firePrefab) as GameObject);
			}
			_fireEmissionAccCount = 1f;
			_hasFireEffect = _fireEffect != null;
			_emissionState &= (EmissionState)(-2);
		}
		if (_smokeEffect == null)
		{
			if (!string.IsNullOrEmpty(smokePrefab))
			{
				_smokeEffect = EffectManager.instance.GetParticleEffect(Resources.Load(smokePrefab) as GameObject);
			}
			_smokeEmissionAccCount = 1f;
			_hasSmokeEffect = _smokeEffect != null;
			_emissionState &= (EmissionState)(-3);
		}
	}

	public void Play(Vector3 position, float baseOffset, float invSpeed, Vector3 velocity)
	{
		float time = Time.time;
		float num = time - _prevCalculationTime;
		if (0.0001f < num)
		{
			_prevCalculationTime = time;
			_emissionState = (EmissionState)0;
			if (_hasSmokeEffect)
			{
				float value = UnityEngine.Random.value;
				_smokeEmissionAccCount += smokeEmissionRateScale * _smokeEffect.GetRndEmissionRate(value) * num;
				if (1f <= _smokeEmissionAccCount)
				{
					int num2 = (int)_smokeEmissionAccCount;
					_smokeEmissionAccCount -= num2;
					_rotation = 360f * value;
					_angularVelocity = 720f * value - 360f;
					_smokeSize = _smokeEffect.GetRndSize(value) * smokeOffsetVelocitySizeLifetime.z;
					_smokeLifetime = _smokeEffect.rndLifetime * smokeOffsetVelocitySizeLifetime.w;
					_emissionState |= EmissionState.ShouldEmitSmoke;
				}
			}
			if (_hasFireEffect)
			{
				_fireEmissionAccCount += fireEmissionRate * num;
				if (1f <= _fireEmissionAccCount)
				{
					int num3 = (int)_fireEmissionAccCount;
					_fireEmissionAccCount -= num3;
					if (_emissionState == (EmissionState)0)
					{
						float value2 = UnityEngine.Random.value;
						_rotation = 360f * value2;
						_angularVelocity = 720f * value2 - 360f;
					}
					_emissionState |= EmissionState.ShouldEmitFire;
				}
			}
		}
		if ((_emissionState & EmissionState.ShouldEmitFire) != 0)
		{
			_fireEffect.Emit(position - velocity * ((primaryFireOffsetVelocitySizeLifetime.x + baseOffset) * invSpeed), velocity * primaryFireOffsetVelocitySizeLifetime.y, primaryFireOffsetVelocitySizeLifetime.z, primaryFireOffsetVelocitySizeLifetime.w, _rotation, _angularVelocity);
			if (0f < secondaryFireOffsetVelocitySizeLifetime.w)
			{
				_fireEffect.Emit(position - velocity * ((secondaryFireOffsetVelocitySizeLifetime.x + baseOffset) * invSpeed), velocity * secondaryFireOffsetVelocitySizeLifetime.y, secondaryFireOffsetVelocitySizeLifetime.z, secondaryFireOffsetVelocitySizeLifetime.w, _rotation, _angularVelocity);
			}
		}
		if ((_emissionState & EmissionState.ShouldEmitSmoke) != 0)
		{
			_smokeEffect.Emit(position - velocity * ((smokeOffsetVelocitySizeLifetime.x + baseOffset) * invSpeed), velocity * smokeOffsetVelocitySizeLifetime.y, _smokeSize, _smokeLifetime, _rotation, _angularVelocity);
		}
	}
}
