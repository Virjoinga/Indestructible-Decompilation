using UnityEngine;

public class PartialArmorDestructible : Destructible
{
	public float forwardDamageReduceFactor;

	public float sideDamageReduceFactor;

	public float backDamageReduceFactor;

	public override DamageResult Damage(float damage, INetworkWeapon weapon)
	{
		if (_hp <= 0f)
		{
			return DamageResult.Ignored;
		}
		CallDamagedEvent(damage, this, weapon);
		float num = Mathf.Max(1f - _damageReducer, 0f);
		float num2 = damage;
		MainWeapon mainWeapon = weapon as MainWeapon;
		if (mainWeapon != null)
		{
			float num3 = Vector3.Dot(mainWeapon.lastHitNormal, base.transform.forward);
			float num4 = ((!(num3 > 0f)) ? 0f : (num3 * forwardDamageReduceFactor * damage));
			float num5 = ((!(num3 < 0f)) ? 0f : ((0f - num3) * backDamageReduceFactor * damage));
			float num6 = Mathf.Abs(Vector3.Dot(mainWeapon.lastHitNormal, base.transform.right));
			float num7 = num6 * sideDamageReduceFactor * damage;
			num2 = damage - num4 - num5 - num7;
		}
		float num8 = _hp - num2 * num;
		SetHP(num8);
		if (num8 <= 0f)
		{
			IDTGame instance = IDTGame.Instance;
			if (instance != null)
			{
				instance.Destructed(this, DestructionReason.Weapon, weapon);
			}
			return DamageResult.Destructed;
		}
		return DamageResult.Damaged;
	}

	//[RPC]
	protected override void InitHP(float hp)
	{
		base.InitHP(hp);
	}

	//[RPC]
	protected override void UpdateHP(float hp)
	{
		base.UpdateHP(hp);
	}

	//[RPC]
	protected override void Activated(long posVal, long rotVal)
	{
		base.Activated(posVal, rotVal);
	}

	//[RPC]
	protected override void Registered(int id)
	{
		base.Registered(id);
	}
}
