using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThermodonPassiveAbility : PassiveAbilityBase
{
	private class AttackerInfo
	{
		public Destructible destructible;

		public Transform transform;

		public PhotonView photonView;

		public float returnedDamage;
	}

	public float DamageReturnPart = 0.15f;

	public float Radius = 30f;

	private PhotonView _ownerPhotonView;

	private MainWeapon _ownerWeapon;

	private Destructible _destructible;

	private float _radius2;

	private Dictionary<MainWeapon, AttackerInfo> _attackers = new Dictionary<MainWeapon, AttackerInfo>();

	private void Start()
	{
		_radius2 = Radius * Radius;
		_ownerWeapon = GetComponentInChildren<MainWeapon>();
		_destructible = GetComponent<Destructible>();
		_ownerPhotonView = GetComponent<PhotonView>();
		if ((bool)_destructible)
		{
			_destructible.damagedEvent += OnDamaged;
		}
		StartCoroutine(UpdateDamage());
	}

	private void OnEnable()
	{
		StartCoroutine(UpdateDamage());
	}

	private void OnDamaged(float damage, Destructible destructible, INetworkWeapon weapon)
	{
		MainWeapon mainWeapon = weapon as MainWeapon;
		if (mainWeapon != null)
		{
			AttackerInfo value = null;
			if (!_attackers.TryGetValue(mainWeapon, out value))
			{
				value = new AttackerInfo();
				value.transform = mainWeapon.transform.root;
				value.destructible = value.transform.GetComponentInChildren<Destructible>();
				value.photonView = mainWeapon.photonView;
				_attackers.Add(mainWeapon, value);
			}
			if ((bool)value.destructible && (value.transform.position - base.transform.position).sqrMagnitude <= _radius2)
			{
				float num = damage * DamageReturnPart * _effectScale;
				value.returnedDamage += num;
			}
		}
	}

	private IEnumerator UpdateDamage()
	{
		YieldInstruction period = new WaitForSeconds(0.3f);
		while (true)
		{
			foreach (KeyValuePair<MainWeapon, AttackerInfo> attacker in _attackers)
			{
				AttackerInfo info = attacker.Value;
				if (info.returnedDamage > 0f)
				{
					if (PhotonNetwork.offlineMode)
					{
						DamageAttacker(info.destructible.id, info.returnedDamage);
					}
					else if (info.photonView != null)
					{
						_ownerPhotonView.RPC("DamageAttacker", info.photonView.owner, info.destructible.id, info.returnedDamage);
					}
					info.returnedDamage = 0f;
				}
			}
			yield return period;
		}
	}

	//[RPC]
	private void DamageAttacker(int destructibleId, float damage)
	{
		Destructible destructible = Destructible.Find(destructibleId);
		if ((bool)destructible)
		{
			destructible.Damage(damage, _ownerWeapon);
		}
	}

	public override PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Thermodon;
	}
}
