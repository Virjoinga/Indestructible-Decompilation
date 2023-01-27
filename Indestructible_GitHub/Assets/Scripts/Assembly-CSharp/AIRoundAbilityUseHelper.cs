using UnityEngine;

public class AIRoundAbilityUseHelper : AIBaseAbilityUseHelper
{
	public float ActivationRadius = 5f;

	private Transform _transform;

	protected override void Start()
	{
		base.Start();
		_transform = base.transform;
	}

	public override float GetAbilityUsageQuality(UsageQualityContext context, Transform targetTransform)
	{
		if (ActivationRadius <= 0f)
		{
			return 0f;
		}
		float magnitude = (targetTransform.position - _transform.position).magnitude;
		return Mathf.Clamp(ActivationRadius - magnitude, 0f, ActivationRadius) / ActivationRadius;
	}
}
