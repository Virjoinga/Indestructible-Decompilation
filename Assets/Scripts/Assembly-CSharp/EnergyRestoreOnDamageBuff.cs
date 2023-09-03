using UnityEngine;

public class EnergyRestoreOnDamageBuff : Buff
{
	public float DamageToEnergyConversion = 0.1f;

	private Destructible _destructible;

	public GameObject ActivationFX;

	public float FXPeriod = 3f;

	private AbilitiesFXDispatcher _fxDispatcher;

	private float _prevFXTime;

	public EnergyRestoreOnDamageBuff(EnergyRestoreOnDamageConf config)
		: base(config)
	{
		DamageToEnergyConversion = config.DamageToEnergyConversion;
		ActivationFX = config.ActivationFX;
		FXPeriod = config.FXPeriod;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_destructible = _vehicle.destructible;
		if ((bool)_destructible)
		{
			_destructible.damagedEvent += OnDamaged;
		}
		if (ActivationFX != null)
		{
			_fxDispatcher = targetGO.GetComponent<AbilitiesFXDispatcher>();
			if ((bool)_fxDispatcher)
			{
				_fxDispatcher.AddCustomFX(ActivationFX);
			}
		}
	}

	protected override void OnFinish()
	{
		if ((bool)_destructible)
		{
			_destructible.damagedEvent -= OnDamaged;
		}
		base.OnFinish();
	}

	private void OnDamaged(float damage, Destructible destructible, INetworkWeapon weapon)
	{
		_vehicle.AddEnergy(damage * DamageToEnergyConversion);
		if (Time.time - _prevFXTime >= FXPeriod)
		{
			_prevFXTime = Time.time;
			if ((bool)ActivationFX)
			{
				_fxDispatcher.PlayCustomFX(ActivationFX.name);
			}
		}
	}
}
