public class StrikerPassiveAbility : DOTBasedPassiveAbility
{
	public int maxCharges = 5;

	public float BuffDuration = 5f;

	public float DamageBoost = 0.15f;

	private Buff _buff;

	private int numberOfCharges;

	private bool isDischarging;

	private BuffSystem _buffSystem;

	protected override void Start()
	{
		base.Start();
		_buffSystem = GetComponent<BuffSystem>();
	}

	protected override void HandleLimitBreak()
	{
		base.HandleLimitBreak();
		if (++numberOfCharges >= maxCharges)
		{
			Discharge();
		}
	}

	public override void OnDamage(Destructible destructible, float damage)
	{
		if (!isDischarging)
		{
			base.OnDamage(destructible, damage);
		}
	}

	private void Discharge()
	{
		isDischarging = true;
		numberOfCharges = 0;
		if (!(_buffSystem == null))
		{
			DamageBoostBuff damageBoostBuff = _buffSystem.AddBuffSuspended<DamageBoostBuff>(base.gameObject);
			if (damageBoostBuff != null)
			{
				_buffSystem.BuffEndedEvent += OnBuffEnd;
				damageBoostBuff.BoostDamageType = DamageBoostBuff.BoostType.Explosive;
				damageBoostBuff.duration = BuffDuration;
				damageBoostBuff.AbsoluteValue = false;
				damageBoostBuff.DamageBoost = DamageBoost * _effectScale;
				damageBoostBuff.FXVisible = true;
				damageBoostBuff.isVisible = true;
				damageBoostBuff.StartBuff();
				_buff = damageBoostBuff;
			}
		}
	}

	public void OnBuffEnd(Buff buff)
	{
		if (_buff == buff)
		{
			_buff = null;
			isDischarging = false;
		}
	}

	public override PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Striker;
	}
}
