using UnityEngine;

public class EnemyDirectionIndicatorController : MonoBehaviour
{
	public GameObject IndicatorPrefab;

	public bool AIControlled;

	private CachedObject.Cache _cache;

	private CachedObject _indicator;

	private void OnEnable()
	{
		bool flag = PhotonNetwork.room == null;
		if (AIControlled)
		{
			if (!flag)
			{
				base.enabled = false;
			}
		}
		else
		{
			PhotonView component = GetComponent<PhotonView>();
			if (component == null)
			{
				return;
			}
			if (component.isMine || flag)
			{
				base.enabled = false;
			}
		}
		if (base.enabled)
		{
			_cache = _cache ?? ObjectCacheManager.Instance.GetCache(IndicatorPrefab);
			_indicator = _cache.Activate();
			EnemyDirectionIndicator component2 = _indicator.GetComponent<EnemyDirectionIndicator>();
			component2.EnemyTransform = GetComponent<Transform>();
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void OnDisable()
	{
		if (_indicator != null)
		{
			_indicator.Deactivate();
		}
	}
}
