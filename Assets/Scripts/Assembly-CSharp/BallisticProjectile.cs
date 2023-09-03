using UnityEngine;

public class BallisticProjectile : Projectile
{
	private float _gravityFactor;

	public override void Init(float projectileSpeed, ProjectileCannon cannon)
	{
		base.Init(projectileSpeed, cannon);
		_gravityFactor = Physics.gravity.y;
	}

	public override void UpdateProjectile(float dt)
	{
		base.UpdateProjectile(dt);
		velocity.y += _gravityFactor * dt;
		_transform.forward = velocity;
	}
}
