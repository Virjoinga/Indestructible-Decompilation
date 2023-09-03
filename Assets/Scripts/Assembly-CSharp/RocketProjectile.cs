using UnityEngine;

public class RocketProjectile : Projectile
{
	public float aimRange = 15f;

	public float aimDirectionFactor = 0.5f;

	public float aimAngularSpeed = 90f;

	public float trailOffset;

	public RocketTrailEffect trailEffect;

	private Weapon _weapon;

	private Collider _aimTarget;

	private float _invSpeed;

	private bool _hasTrailEffect;

	public override void Init(float projectileSpeed, ProjectileCannon cannon)
	{
		base.Init(projectileSpeed, cannon);
		_invSpeed = 1f / speed;
		aimAngularSpeed *= 0.04f;
		_weapon = cannon;
		if (_hasTrailEffect = trailEffect != null)
		{
			trailEffect.Use();
		}
	}

	public override void Launch(Vector3 origin, Vector3 normalizedDirection, float distance)
	{
		base.Launch(origin, normalizedDirection, distance);
		_aimTarget = null;
	}

	private void DetectAimTarget()
	{
		Vector2 lhs = new Vector2(velocity.x, velocity.z);
		lhs.Normalize();
		Collider aimTarget = _aimTarget;
		float num = aimDirectionFactor;
		Collider[] array = Physics.OverlapSphere(position, aimRange, _weapon.damageLayers);
		int i = 0;
		for (int num2 = array.Length; i != num2; i++)
		{
			Collider collider = array[i];
			if (_weapon.IsFoe(collider))
			{
				Vector3 center = collider.bounds.center;
				Vector2 rhs = new Vector2(center.x - position.x, center.z - position.z);
				rhs.Normalize();
				float num3 = Vector2.Dot(lhs, rhs);
				if (num < num3)
				{
					num = num3;
					aimTarget = collider;
				}
			}
		}
		_aimTarget = aimTarget;
	}

	public override void UpdateProjectile(float dt)
	{
		base.UpdateProjectile(dt);
		if (_aimTarget == null)
		{
			DetectAimTarget();
		}
		if (_aimTarget != null)
		{
			Vector3 vector = _aimTarget.bounds.center - position;
			vector.Normalize();
			Vector3 vector2 = velocity * (1f / speed);
			float num = ((vector2.x * vector.x + vector2.z * vector.z) * 0.5f + 0.5f) * dt * aimAngularSpeed;
			vector2.x += (vector.x - vector2.x) * num;
			vector2.y += vector.y - vector2.y;
			vector2.z += (vector.z - vector2.z) * num;
			vector2.Normalize();
			velocity = vector2 * speed;
			base.transform.forward = vector2;
		}
		if (_hasTrailEffect)
		{
			trailEffect.Play(position, trailOffset, _invSpeed, velocity);
		}
	}
}
