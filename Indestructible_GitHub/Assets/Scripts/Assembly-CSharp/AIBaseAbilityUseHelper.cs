using UnityEngine;

public class AIBaseAbilityUseHelper : MonoBehaviour
{
	public enum UsageQualityContext
	{
		Attack = 0,
		Evade = 1
	}

	protected virtual void Start()
	{
	}

	public virtual float GetAbilityUsageQuality(UsageQualityContext context, Transform targetTransform)
	{
		return 0f;
	}
}
