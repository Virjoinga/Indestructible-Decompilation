using System;
using UnityEngine;

[AddComponentMenu("Indestructible/Weapons/Projectile Cannon")]
public class ProjectileCannon : Cannon
{
	public float AOERadius = 1f;

	public float explosionForce;

	public float constDamageFactor;

	public float sqrDamageFactor = 1f;

	public GameObject projectilePrefab;

	public GameObject outOfRangeHitEffect;

	public float detonationShakeCameraMagnitude;

	public float detonationShakeCameraDuration;

	private int _nextProjectileIndex;

	private Projectile[] _projectilePool;

	private int _activeProjectilesCount;

	private Transform _detonationTransform;

	private CachedObject.Cache _outOfRangeHitEffectCache;

	//[RPC]
	public override void SetFireInterval(float value)
	{
		base.SetFireInterval(value);
		CheckProjectilePool();
	}

	//[RPC]
	public override void SetRange(float value)
	{
		base.SetRange(value);
		CheckProjectilePool();
	}

	protected override void Start()
	{
		base.Start();
		CheckProjectilePool();
		if (outOfRangeHitEffect != null)
		{
			_outOfRangeHitEffectCache = ObjectCacheManager.Instance.GetCache(outOfRangeHitEffect);
		}
	}

	protected override void MakeShot()
	{
		base.MakeShot();
		Projectile projectile = _projectilePool[_nextProjectileIndex++];
		if (_projectilePool.Length == _nextProjectileIndex)
		{
			_nextProjectileIndex = 0;
		}
		if (!projectile.enabled)
		{
			_activeProjectilesCount++;
		}
		projectile.Launch(muzzleTransform.position, muzzleTransform.forward, GetRange());
	}

	public override bool DoUpdate()
	{
		int num = _activeProjectilesCount;
		if (num <= 0)
		{
			UpdateStopped();
			return false;
		}
		float deltaTime = Time.deltaTime;
		float num2 = projectileSpeed * deltaTime * 1.125f;
		int num3 = _nextProjectileIndex - 1;
		do
		{
			IL_0034:
			if (num3 < 0)
			{
				num3 = _projectilePool.Length - 1;
			}
			Projectile projectile = _projectilePool[num3--];
			if (projectile == null || !projectile.enabled)
			{
				goto IL_0034;
			}
			num--;
			if (0f < projectile.distance)
			{
				if (Physics.Raycast(projectile.position, projectile.velocity, out _hitInfo, num2, base.collisionLayers))
				{
					if (!IsFoe(_hitInfo.collider))
					{
						float num4 = num2 - 0.1f;
						if (0f < num4)
						{
							Vector3 vector = projectile.velocity * (0.1f / projectileSpeed);
							while (true)
							{
								Vector3 origin = _hitInfo.point + vector;
								if (!Physics.Raycast(origin, projectile.velocity, out _hitInfo, num4, base.collisionLayers))
								{
									break;
								}
								if (!IsFoe(_hitInfo.collider) && 0f < (num4 -= 0.1f))
								{
									continue;
								}
								goto IL_0156;
							}
							goto IL_0238;
						}
					}
					goto IL_0156;
				}
				goto IL_0238;
			}
			if (weaponDamageType == DamageType.Explosive)
			{
				Damage(projectile.position, AOERadius, constDamageFactor, sqrDamageFactor, explosionForce);
			}
			if (_outOfRangeHitEffectCache != null)
			{
				HitEffect hitEffect = _outOfRangeHitEffectCache.RetainObject() as HitEffect;
				if (projectile.isVisible)
				{
					hitEffect.Activate(projectile.position);
				}
				else
				{
					hitEffect.ActivateInvisible(projectile.position);
				}
			}
			DeactivateProjectile(projectile);
			continue;
			IL_0238:
			projectile.UpdateProjectile(deltaTime);
			continue;
			IL_0156:
			Destructible destructible;
			if (weaponDamageType == DamageType.Explosive)
			{
				Damage(_hitInfo.point, AOERadius, constDamageFactor, sqrDamageFactor, explosionForce);
			}
			else if (CheckFoeDamageAbilty(_hitInfo.collider, out destructible))
			{
				Damage(destructible, _hitInfo.point, projectile.velocity);
			}
			HitEffect hitEffect2 = GetHitEffectCache(GetColliderInfo(_hitInfo.collider).surfaceType).RetainObject() as HitEffect;
			if (projectile.isVisible)
			{
				hitEffect2.Activate(_hitInfo.point, _hitInfo.normal);
			}
			else
			{
				hitEffect2.ActivateInvisible(_hitInfo.point);
			}
			DeactivateProjectile(projectile);
		}
		while (0 < num);
		return true;
	}

	private void DeactivateProjectile(Projectile projectile)
	{
		_activeProjectilesCount--;
		projectile.Deactivate();
	}

	private void CheckProjectilePool()
	{
		int num = Mathf.CeilToInt((GetRange() + 1f) / (projectileSpeed * GetFireInterval()));
		int i;
		if (_projectilePool == null)
		{
			i = 0;
			_projectilePool = new Projectile[num];
		}
		else
		{
			i = _projectilePool.Length;
			if (num <= i)
			{
				return;
			}
			Array.Resize(ref _projectilePool, num);
		}
		for (; i < num; i++)
		{
			Projectile component = ((GameObject)UnityEngine.Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity)).GetComponent<Projectile>();
			component.Init(projectileSpeed, this);
			_projectilePool[i] = component;
		}
	}
}
