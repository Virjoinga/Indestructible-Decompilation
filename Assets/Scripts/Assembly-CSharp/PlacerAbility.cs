using UnityEngine;

public class PlacerAbility : CooldownAbility
{
	private const int NetGroupStartID = 2;

	public GameObject AbilityInstPrefab;

	public Vector3 SpawnOffset = new Vector3(0f, 0f, 0f);

	private int _redTeamLayer = LayerMask.NameToLayer("PlayerTeam0");

	private int _blueTeamLayer = LayerMask.NameToLayer("PlayerTeam1");

	private CachedObject.Cache _placerCache;

	protected override void Start()
	{
		base.Start();
		_placerCache = ObjectCacheManager.Instance.GetCache(AbilityInstPrefab);
	}

	protected override void OnAbilityStart()
	{
		base.OnAbilityStart();
		Place();
	}

	private void Place()
	{
		if (AbilityInstPrefab != null)
		{
			Vector3 pos = base.gameObject.transform.TransformPoint(SpawnOffset);
			CachedObject cachedObject = _placerCache.Activate(pos, AbilityInstPrefab.transform.rotation);
			BaseAbilityPlacing componentInChildren = cachedObject.GetComponentInChildren<BaseAbilityPlacing>();
			componentInChildren.SetOwnerObject(base.gameObject);
			int layer = base.gameObject.layer;
			int teamWithRPC = ((layer != _redTeamLayer) ? ((layer == _blueTeamLayer) ? 1 : (-1)) : 0);
			componentInChildren.SetTeamWithRPC(teamWithRPC);
			componentInChildren.SetEffectScaleWithRPC(base.EffectScale);
		}
	}

	[RPC]
	protected override void AbilityActivated()
	{
		base.AbilityActivated();
	}
}
