using UnityEngine;

public class EnemyInfoIndicator : GameplayObjectIndicator
{
	public SpriteText EnemyName;

	public EnemyHealthBar EnemyHealth;

	private Transform _targetTransform;

	private Vector3 _cachePosition;

	private Vector3 _offset;

	public void Startup(GameObject o, Vector3 offset, bool showName, bool showHealth)
	{
		_targetTransform = o.GetComponent<Transform>();
		_offset = offset;
		if (showName)
		{
			Vehicle component = o.GetComponent<Vehicle>();
			if (component != null && component.player != null)
			{
				EnemyName.Text = component.player.name;
				EnemyName.SetColor(MonoSingleton<Player>.Instance.GetTeamColor(component.player.teamID));
			}
		}
		else if (EnemyName != null)
		{
			Object.Destroy(EnemyName.gameObject);
		}
		if (showHealth)
		{
			EnemyHealth.Initialize(o);
		}
		else if (EnemyHealth != null)
		{
			Object.Destroy(EnemyHealth.gameObject);
		}
	}

	private void UpdatePosition()
	{
		Vector3 position = MainToUICamera((_cachePosition = _targetTransform.position) + _offset);
		position.z = _transform.position.z;
		_transform.position = position;
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (_initialized)
		{
			UpdatePosition();
		}
	}
}
