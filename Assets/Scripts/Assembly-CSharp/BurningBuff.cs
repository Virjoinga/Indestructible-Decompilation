using UnityEngine;

public class BurningBuff : Buff, INetworkWeapon
{
	public delegate void OnBurnDelegate(INetworkWeapon _weaponOwner, Vehicle vehicle, Destructible target, float damage);

	private Destructible _destructible;

	public static float _damage = 10f;

	private AbilitiesFXDispatcher _fx;

	private INetworkWeapon _weaponOwner;

	private float _accumulatedHeat;

	private float _burningHeat = 100f;

	private bool _burn;

	public MatchPlayer player
	{
		get
		{
			return _weaponOwner.player;
		}
	}

	public DamageType damageType
	{
		get
		{
			return _weaponOwner.damageType;
		}
	}

	public event OnBurnDelegate onBurningEvent;

	private void Start()
	{
		GlobalConfig instance = GlobalConfig.Instance;
		if (instance != null)
		{
			_damage = instance.BurningDebuffTickValue;
			base.duration = instance.BurningDebuffPeriod;
		}
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_destructible = _target.GetComponentInChildren<Destructible>();
		_weaponOwner = instigator as INetworkWeapon;
		base.isUpdatable = true;
		base.updateRate = 1f;
		base.shouldUpdateFromStart = false;
		base.isVisible = false;
	}

	protected override void OnTick(int tickCount)
	{
		if (_burn)
		{
			float damage = _damage * base.effectScale * (float)tickCount;
			_destructible.Damage(damage, this);
			if (this.onBurningEvent != null)
			{
				this.onBurningEvent(_weaponOwner, _vehicle, _destructible, damage);
			}
		}
		if (!(_accumulatedHeat > 0f))
		{
			return;
		}
		_accumulatedHeat -= 3f;
		if (_accumulatedHeat <= 0f)
		{
			_accumulatedHeat = 0f;
			if ((bool)_fx)
			{
				_fx.DeactivateBurnEffect(true);
			}
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		_fx = _target.GetComponent<AbilitiesFXDispatcher>();
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		if ((bool)_fx)
		{
			_fx.DeactivateBurnEffect(true);
		}
	}

	public void AddHeat(float heat)
	{
		_accumulatedHeat += heat;
		if (_accumulatedHeat >= _burningHeat)
		{
			Burn();
		}
		else if (!_burn)
		{
			base.duration = 0f;
		}
	}

	public void Burn()
	{
		base.startTime = Time.time;
		_lastTick = Time.time;
		_accumulatedHeat = _burningHeat;
		if ((bool)_fx && !_burn)
		{
			_fx.ActivateBurnEffect(true);
		}
		if (!_burn)
		{
			_burn = true;
			base.isVisible = true;
			_buffSystem.InvokeBuffStarted(this);
		}
	}
}
