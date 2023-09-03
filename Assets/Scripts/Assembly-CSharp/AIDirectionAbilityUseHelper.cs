using UnityEngine;

public class AIDirectionAbilityUseHelper : AIBaseAbilityUseHelper
{
	public enum EDirection
	{
		Forward = 0,
		Backward = 1
	}

	public EDirection Direction = EDirection.Backward;

	public float MinActivationDistance;

	public float MaxActivationDistance = 5f;

	private Transform _transform;

	protected override void Start()
	{
		base.Start();
		_transform = base.transform;
	}

	public override float GetAbilityUsageQuality(UsageQualityContext context, Transform targetTransform)
	{
		Vector3 rhs = targetTransform.position - _transform.position;
		float magnitude = rhs.magnitude;
		if (magnitude < MinActivationDistance || magnitude > MaxActivationDistance)
		{
			return 0f;
		}
		rhs.Normalize();
		float value = Vector3.Dot((Direction != EDirection.Backward) ? _transform.forward : (-_transform.forward), rhs);
		return Mathf.Clamp(value, 0f, 1f);
	}
}
