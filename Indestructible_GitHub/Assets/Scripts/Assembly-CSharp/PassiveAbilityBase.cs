using Glu;

public class PassiveAbilityBase : MonoBehaviour
{
	protected float _effectScale = 1f;

	public float GetBaseEffectScale()
	{
		return 1f;
	}

	public float GetEffectScale()
	{
		return _effectScale;
	}

	public virtual void SetEffectScale(float newEffectScale)
	{
		_effectScale = newEffectScale;
	}

	public virtual PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Base;
	}
}
