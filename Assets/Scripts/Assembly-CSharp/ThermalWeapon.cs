using System.Collections;
using UnityEngine;

public abstract class ThermalWeapon : MainWeapon, IDOTWeapon, INetworkWeapon
{
	public float minEnergyReserveScale = 3f;

	public DOTWeaponImp dotImplementation = new DOTWeaponImp();

	private YieldInstruction _netSerializeIntervalInstruction;

	public DOTWeaponImp dotInterface
	{
		get
		{
			return dotImplementation;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		dotImplementation.Init(this);
		if (PhotonNetwork.room != null)
		{
			_netSerializeIntervalInstruction = new WaitForSeconds(1f / (float)PhotonNetwork.sendRateOnSerialize);
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		dotImplementation.Reset();
		StopFire();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		StopFire();
	}

	protected override IEnumerator FireLoop()
	{
		do
		{
			if (TryConsumeFirstShotEnergy())
			{
				base.gunTurret.SetDirection();
				StartFire();
				do
				{
					MakeShot();
					float waitTime = GetFireInterval();
					while (true)
					{
						yield return null;
						float dt = Time.deltaTime;
						float num;
						waitTime = (num = waitTime - dt);
						if (num < 0f)
						{
							break;
						}
						base.gunTurret.LerpDirection(dt);
					}
				}
				while (shouldFire && TryConsumeShotEnergy());
				StopFire();
			}
			base.gunTurret.LerpDirection(Time.deltaTime);
			yield return null;
		}
		while (shouldFire);
		FireLoopEnd();
	}

	protected override IEnumerator AvatarFireLoop()
	{
		base.gunTurret.SetAvatarDirection();
		StartFire();
		while (true)
		{
			ConsumeShotEnergy();
			MakeShot();
			float waitTime = GetFireInterval();
			do
			{
				yield return null;
				float dt = Time.deltaTime;
				base.gunTurret.LerpAvatarDirection(dt);
				waitTime -= dt;
			}
			while (0f < waitTime);
			if (!base.shouldAvatarFire)
			{
				yield return _netSerializeIntervalInstruction;
				if (!base.shouldAvatarFire)
				{
					break;
				}
			}
		}
		StopFire();
		FireLoopEnd();
	}

	protected virtual void StartFire()
	{
		base.audioHelper.PlayIfEnabled();
	}

	protected abstract void MakeShot();

	protected virtual void StopFire()
	{
		base.audioHelper.StopIfEnabled();
	}

	protected bool TryConsumeFirstShotEnergy()
	{
		float shotEnergyConsumption = GetShotEnergyConsumption();
		return !(base.vehicle != null) || base.vehicle.TryConsumeEnergy(shotEnergyConsumption, shotEnergyConsumption * minEnergyReserveScale);
	}

	protected override DamageResult Damage(Destructible destructible, float damage)
	{
		DamageResult damageResult = base.Damage(destructible, damage);
		if (damageResult == DamageResult.Damaged)
		{
			dotImplementation.Heat(destructible);
		}
		return damageResult;
	}

	protected virtual DamageResult Damage(Destructible destructible, float damage, float heat)
	{
		DamageResult damageResult = base.Damage(destructible, damage);
		if (damageResult == DamageResult.Damaged)
		{
			dotImplementation.Heat(destructible, heat);
		}
		return damageResult;
	}
}
