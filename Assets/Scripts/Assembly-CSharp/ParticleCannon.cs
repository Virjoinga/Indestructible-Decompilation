using UnityEngine;

[AddComponentMenu("Indestructible/Weapons/Particle Cannon")]
public class ParticleCannon : Cannon
{
	public ParticleSystem projectileEmitter;

	public float maxDeltaTimeToShareCollision = 0.25f;

	public float maxDistanceToShareCollision = 6f;

	private GameObject _projectileEmitterObject;

	private static ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[64];

	[RPC]
	public override void SetFireInterval(float value)
	{
		base.SetFireInterval(value);
	}

	[RPC]
	public override void SetRange(float value)
	{
		base.SetRange(value);
		projectileEmitter.startLifetime = value / projectileSpeed;
	}

	protected override void Start()
	{
		base.Start();
		projectileEmitter.startSpeed = projectileSpeed;
		projectileEmitter.startLifetime = GetRange() / projectileSpeed;
		_projectileEmitterObject = projectileEmitter.gameObject;
		maxDistanceToShareCollision *= maxDistanceToShareCollision;
	}

	protected override void MakeShot()
	{
		base.MakeShot();
		projectileEmitter.Emit(1);
	}

	public override bool DoUpdate()
	{
		if (!projectileEmitter.IsAlive(true))
		{
			if (!base.gameObject.active)
			{
				_projectileEmitterObject.SetActiveRecursively(false);
			}
			UpdateStopped();
			return false;
		}
		if (!_projectileEmitterObject.active)
		{
			_projectileEmitterObject.SetActiveRecursively(true);
		}
		int particles = projectileEmitter.GetParticles(_particles);
		float deltaTime = Time.deltaTime;
		if (particles != 0 && 1E-05f < deltaTime)
		{
			bool flag = false;
			float num = projectileSpeed * deltaTime;
			float num2 = num * 1.125f;
			float num3 = -1000f;
			Vector3 vector = new Vector3(0f, 0f, 0f);
			Vector3 normal = Vector3.up;
			bool flag2 = false;
			bool flag3 = false;
			Collider collider = null;
			Destructible destructible = null;
			CachedObject.Cache cache = null;
			for (int i = 0; i != particles; i++)
			{
				if (_particles[i].randomValue < 0f)
				{
					continue;
				}
				Vector3 velocity = _particles[i].velocity;
				Vector3 vector2 = _particles[i].position;
				float lifetime = _particles[i].remainingLifetime;
				float num4 = lifetime - num3;
				if (0f <= num4 && num4 < maxDeltaTimeToShareCollision)
				{
					Vector3 vector3 = vector2 + velocity * num4;
					if ((vector3 - vector).sqrMagnitude < maxDistanceToShareCollision)
					{
						if (!flag2)
						{
							continue;
						}
						if (flag3)
						{
							if (!destructible.isActive)
							{
								continue;
							}
							Damage(destructible, vector3, velocity);
						}
						(cache.RetainObject() as HitEffect).Activate(vector3, normal);
						_particles[i].randomValue = -1f;
						_particles[i].remainingLifetime = num4;
						flag = true;
						continue;
					}
				}
				if (Physics.Raycast(vector2, velocity, out _hitInfo, num2, base.collisionLayers))
				{
					if (IsFoe(_hitInfo.collider))
					{
						goto IL_02a6;
					}
					Vector3 vector4 = velocity * (0.1f / projectileSpeed);
					float num5 = num2 - 0.1f;
					while (!(num5 <= 0f))
					{
						vector2 = _hitInfo.point + vector4;
						if (!Physics.Raycast(vector2, velocity, out _hitInfo, num5, base.collisionLayers))
						{
							break;
						}
						if (IsFoe(_hitInfo.collider))
						{
							goto IL_02a6;
						}
						num5 -= 0.1f;
					}
				}
				flag2 = false;
				num3 = lifetime - deltaTime;
				vector = vector2 + velocity * deltaTime;
				continue;
				IL_02a6:
				collider = _hitInfo.collider;
				vector = _hitInfo.point;
				normal = _hitInfo.normal;
				num3 = lifetime - deltaTime * (_hitInfo.distance / num);
				if (flag3 = CheckFoeDamageAbilty(collider, out destructible))
				{
					Damage(destructible, vector, velocity);
				}
				cache = GetHitEffectCache(GetColliderInfo(collider).surfaceType);
				(cache.RetainObject() as HitEffect).Activate(vector, normal);
				_particles[i].randomValue = -1f;
				_particles[i].remainingLifetime = 0f;
				flag = true;
				flag2 = true;
			}
			if (flag)
			{
				projectileEmitter.SetParticles(_particles, particles);
			}
		}
		return true;
	}
}
