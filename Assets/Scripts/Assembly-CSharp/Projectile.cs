using UnityEngine;

public class Projectile : MonoBehaviour
{
	public Vector3 position;

	public Vector3 velocity;

	public float speed;

	public float distance;

	protected Transform _transform;

	private Renderer _renderer;

	public bool isVisible
	{
		get
		{
			return _renderer.isVisible;
		}
	}

	public virtual void Init(float projectileSpeed, ProjectileCannon cannon)
	{
		speed = projectileSpeed;
		_transform = base.transform;
		_renderer = base.GetComponent<Renderer>();
		base.enabled = false;
		base.gameObject.SetActiveRecursively(false);
	}

	public virtual void Launch(Vector3 origin, Vector3 normalizedDirection, float distance)
	{
		velocity = normalizedDirection * speed;
		position = origin;
		_transform.position = origin;
		_transform.rotation = Quaternion.LookRotation(normalizedDirection);
		this.distance = distance;
		base.gameObject.SetActiveRecursively(true);
		base.enabled = true;
		_renderer.enabled = true;
	}

	public virtual void UpdateProjectile(float dt)
	{
		position += velocity * dt;
		_transform.position = position;
		distance -= speed * dt;
	}

	public void Deactivate()
	{
		base.enabled = false;
		base.gameObject.SetActiveRecursively(false);
	}
}
