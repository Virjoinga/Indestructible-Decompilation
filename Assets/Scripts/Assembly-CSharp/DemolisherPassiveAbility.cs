using UnityEngine;

public class DemolisherPassiveAbility : DOTBasedPassiveAbility
{
	public float chanceToGetHeal = 5f;

	public float healValuePercent = 15f;

	public GameObject ActivateFX;

	private PhotonView _photonView;

	private CachedObject.Cache _activateFXCache;

	protected override void Start()
	{
		base.Start();
		_photonView = ((PhotonNetwork.room != null) ? GetComponent<PhotonView>() : null);
		if ((bool)ActivateFX)
		{
			_activateFXCache = ObjectCacheManager.Instance.GetCache(ActivateFX);
		}
	}

	protected override void HandleLimitBreak()
	{
		base.HandleLimitBreak();
		float value = Random.value;
		if (value < chanceToGetHeal / 100f)
		{
			ChanceHit();
		}
	}

	private void ChanceHit()
	{
		if (_photonView != null)
		{
			Heal();
			_photonView.RPC("Heal", PhotonTargets.Others);
		}
	}

	[RPC]
	private void Heal()
	{
		if (_destructible != null && _destructible.isMine)
		{
			float num = _destructible.GetMaxHP() * healValuePercent * _effectScale / 100f;
			Debug.Log("heal " + num);
			_destructible.Heal(num);
		}
		if (_activateFXCache != null)
		{
			_activateFXCache.Activate(base.gameObject.transform.position);
		}
	}

	public override PassiveAbilityType GetAbilityType()
	{
		return PassiveAbilityType.Demolisher;
	}
}
