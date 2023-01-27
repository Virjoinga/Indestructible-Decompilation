using System;
using UnityEngine;

[AddComponentMenu("Indestructible/Weapons/Flamethrower")]
public class Flamethrower : ThermalWeapon
{
	private struct Projectile
	{
		public float lifeTime;

		public Vector3 velocity;

		public Vector3 position;
	}

	public float flameEmissionIntervalScale = 0.05f;

	public float flameSpeed = 30f;

	public float flameStartSizeScale = 1.7f;

	public float flameLifeTime = 1f;

	public int smokeRelativeEmissionCooldown = 2;

	public float smokeRelativeVelocityFactor = 1.05f;

	public float smokeRelativeSizeFactor = 1.1f;

	public float smokeRelativeLifetimeFactor = 1.35f;

	public float projectileSpeed = 25f;

	public float minFlameRadius = 0.25f;

	public float maxFlameRadius = 1.5f;

	public float flameRadiusGrow = 1.75f;

	public float projectileEmitOffset = 1.5f;

	public float inheritVelocityFactor = 0.85f;

	public GameObject flameEffectPrefab;

	public GameObject smokeEffectPrefab;

	private int _throwProjectileCooldown;

	private int _lastProjectileIndex;

	private Projectile[] _projectilePool;

	private Vector3 _prevEmitterPosition;

	private Vector3 _emitterVelocity;

	private float _flameEmissionCooldown;

	private int _smokeEmissionCooldown;

	private float _projectileLifeTime;

	private ParticleEffect _flameEffect;

	private ParticleEffect _smokeEffect;

	private UpdateAgent _updateAgent;

	private bool _isFlameThrowing;

	private bool _isUpdating;

	//[RPC]
	public override void SetFireInterval(float value)
	{
		base.SetFireInterval(value);
		CheckProjectilePool();
	}

	protected override void Awake()
	{
		base.Awake();
		_projectileLifeTime = GetRange() / projectileSpeed - projectileEmitOffset / projectileSpeed;
		CheckProjectilePool();
		_flameEffect = EffectManager.instance.GetParticleEffect(flameEffectPrefab);
		_smokeEffect = EffectManager.instance.GetParticleEffect(smokeEffectPrefab);
	}

	protected override void Start()
	{
		base.Start();
		_updateAgent = MonoSingleton<UpdateAgent>.Instance;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_isUpdating)
		{
			_updateAgent.StopUpdate(this);
		}
	}

	protected override void MakeShot()
	{
		_throwProjectileCooldown = 2;
		if (!_isUpdating)
		{
			_isUpdating = true;
			_updateAgent.StartUpdate(this);
		}
		RegShot();
	}

	protected override void StartFire()
	{
		_prevEmitterPosition = base.transform.position;
		_isFlameThrowing = true;
		base.StartFire();
	}

	protected override void StopFire()
	{
		_isFlameThrowing = false;
		base.StopFire();
	}

	public override bool DoUpdate()
	{
		float deltaTime = Time.deltaTime;
		Vector3 position = base.transform.position;
		_emitterVelocity = (position - _prevEmitterPosition) * (1f / deltaTime);
		_prevEmitterPosition = position;
		if (--_throwProjectileCooldown == 0)
		{
			if (_projectilePool.Length <= ++_lastProjectileIndex)
			{
				_lastProjectileIndex = 0;
			}
			ThrowProjectile(ref _projectilePool[_lastProjectileIndex]);
		}
		bool isUpdating = false;
		int num = _lastProjectileIndex;
		while (UpdateProjectile(ref _projectilePool[num]))
		{
			isUpdating = true;
			if (num == 0)
			{
				num = _projectilePool.Length;
			}
			if (--num == _lastProjectileIndex)
			{
				break;
			}
		}
		if (_isFlameThrowing)
		{
			if ((_flameEmissionCooldown -= deltaTime) < 0f)
			{
				float value = UnityEngine.Random.value;
				_flameEmissionCooldown += _flameEffect.GetRndEmissionInterval(value) * flameEmissionIntervalScale;
				Vector3 vector = base.gunTurret.forward * flameSpeed + _emitterVelocity * inheritVelocityFactor;
				float num2 = _flameEffect.GetRndSize(value) * flameStartSizeScale;
				float value2 = UnityEngine.Random.value;
				float num3 = _flameEffect.GetRndSize(value2) * flameLifeTime;
				float rotation = 360f * value2;
				float num4 = 800f * value - 400f;
				_flameEffect.Emit(position, vector, num2, num3, rotation, num4);
				_flameEffect.Emit(position + vector * deltaTime, vector, num2, num3 * 0.25f, rotation, num4);
				if (_smokeEmissionCooldown == 0)
				{
					_smokeEmissionCooldown = smokeRelativeEmissionCooldown;
					_smokeEffect.Emit(position, vector * smokeRelativeVelocityFactor, num2 * smokeRelativeSizeFactor, num3 * smokeRelativeLifetimeFactor, rotation, num4 * 0.5f);
				}
				else
				{
					_smokeEmissionCooldown--;
				}
			}
			return true;
		}
		return _isUpdating = isUpdating;
	}

	private void ThrowProjectile(ref Projectile projectile)
	{
		projectile.lifeTime = 0f;
		projectile.velocity = base.gunTurret.forward * projectileSpeed + _emitterVelocity * inheritVelocityFactor;
		projectile.position = _prevEmitterPosition + base.gunTurret.forward * projectileEmitOffset;
	}

	private bool UpdateProjectile(ref Projectile projectile)
	{
		float deltaTime = Time.deltaTime;
		float num = (projectile.lifeTime += deltaTime);
		if (_projectileLifeTime < num)
		{
			return false;
		}
		projectile.position += projectile.velocity * deltaTime;
		Collider[] array = Physics.OverlapSphere(projectile.position, CalcRadius(num), base.damageLayers);
		if (array.Length != 0)
		{
			Destructible[] destructibles;
			int num2 = FilterColliders(array, out destructibles);
			for (int i = 0; i < num2; i++)
			{
				Destructible destructible = destructibles[i];
				Damage(destructible, GetDamage() * deltaTime, dotImplementation.GetHeat() * deltaTime);
			}
		}
		return true;
	}

	private float CalcRadius(float lifeTime)
	{
		float num = minFlameRadius + flameRadiusGrow * lifeTime;
		if (maxFlameRadius < num)
		{
			num = maxFlameRadius;
		}
		return num;
	}

	private void CheckProjectilePool()
	{
		float num = 1f / GetFireInterval();
		int num2 = Mathf.CeilToInt(_projectileLifeTime * num);
		if (_projectilePool == null)
		{
			_projectilePool = new Projectile[num2];
		}
		else if (_projectilePool.Length < num2)
		{
			Projectile[] array = new Projectile[num2];
			Array.Copy(_projectilePool, 0, array, 0, _lastProjectileIndex + 1);
			int num3 = _lastProjectileIndex + 1;
			int num4 = _projectilePool.Length - num3;
			if (0 < num4)
			{
				Array.Copy(_projectilePool, num3, array, num2 - num4, num4);
			}
			_projectilePool = array;
		}
	}

	private void OnDrawGizmos()
	{
		int num = _lastProjectileIndex;
		while (!(_projectileLifeTime < _projectilePool[num].lifeTime))
		{
			Gizmos.DrawWireSphere(_projectilePool[num].position, CalcRadius(_projectilePool[num].lifeTime));
			if (num == 0)
			{
				num = _projectilePool.Length;
			}
			if (--num == _lastProjectileIndex)
			{
				break;
			}
		}
	}
}
