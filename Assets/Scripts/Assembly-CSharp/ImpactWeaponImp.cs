using System;
using UnityEngine;

[Serializable]
public class ImpactWeaponImp
{
	public float baseConstHitImpulse = 5f;

	public float baseRelativeHitImpulse = 0.01f;

	private float _constHitImpulse;

	private float _relativeHitImpulse;

	private bool _hasHitImpulse;

	public void Init()
	{
		SetConstHitImpulse(baseConstHitImpulse);
		SetRelativeHitImpulse(baseRelativeHitImpulse);
	}

	public float GetBaseConstHitImpulse()
	{
		return baseConstHitImpulse;
	}

	public float GetConstHitImpulse()
	{
		return _constHitImpulse;
	}

	public void SetConstHitImpulse(float value)
	{
		_constHitImpulse = value;
		_hasHitImpulse = _constHitImpulse * _relativeHitImpulse != 0f;
	}

	public float GetBaseRelativeHitImpulse()
	{
		return baseRelativeHitImpulse;
	}

	public float GetRelativeHitImpulse()
	{
		return _relativeHitImpulse;
	}

	public void SetRelativeHitImpulse(float value)
	{
		_relativeHitImpulse = value;
		_hasHitImpulse = _constHitImpulse * _relativeHitImpulse != 0f;
	}

	public void AddHitImpulse(Destructible destructible, Vector3 position, Vector3 vector)
	{
		if (_hasHitImpulse)
		{
			Rigidbody rigidbody = destructible.rigidbody;
			if (rigidbody != null)
			{
				rigidbody.AddForceAtPosition(vector * (_constHitImpulse + _relativeHitImpulse * destructible.impactStability), position, ForceMode.Impulse);
			}
		}
	}
}
