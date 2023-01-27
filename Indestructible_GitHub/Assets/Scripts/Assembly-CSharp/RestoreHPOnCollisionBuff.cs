using UnityEngine;

public class RestoreHPOnCollisionBuff : Buff
{
	public bool AbsoluteValue = true;

	public float AddHP;

	public float DamageLevel = 10f;

	public GameObject ActivateFX;

	public GameObject VictimFX;

	private CollisionHealer _collisionHealer;

	public RestoreHPOnCollisionBuff(RestoreHPOnCollisionConf config)
		: base(config)
	{
		AbsoluteValue = config.AbsoluteValue;
		AddHP = config.AddHP;
		DamageLevel = config.DamageLevel;
		ActivateFX = config.ActivateFX;
		VictimFX = config.VictimFX;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_collisionHealer = targetGO.GetComponent<CollisionHealer>();
		if (_collisionHealer == null)
		{
			_collisionHealer = targetGO.AddComponent<CollisionHealer>();
		}
		_collisionHealer.AbsoluteValue = AbsoluteValue;
		_collisionHealer.AddHP = AddHP;
		_collisionHealer.ActivateDamageLevel = DamageLevel;
		_collisionHealer.Stacks = Stacks;
		_collisionHealer.EffectScale = base.effectScale;
		_collisionHealer.ActivateFX = ActivateFX;
		_collisionHealer.VictimFX = VictimFX;
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		if (_collisionHealer != null)
		{
			_collisionHealer.Stacks = Stacks;
		}
	}
}
