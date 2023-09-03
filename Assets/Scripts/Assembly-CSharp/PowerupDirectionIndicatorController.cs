using Glu.Localization;
using UnityEngine;

public class PowerupDirectionIndicatorController : MonoBehaviour
{
	public GameObject IndicatorPrefab;

	public string ActivationText = "IDS_POWERUP_SPAWNED_TEXT";

	private CachedObject.Cache _cache;

	private CachedObject _indicator;

	private void OnEnable()
	{
		base.enabled = true;
		if (base.enabled)
		{
			if (IsInit())
			{
				ActivateAndNotificate();
			}
			else
			{
				Init();
			}
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void Start()
	{
		ActivateAndNotificate();
	}

	private void ActivateAndNotificate()
	{
		_indicator = _cache.Activate();
		PowerupDirectionIndicator component = _indicator.GetComponent<PowerupDirectionIndicator>();
		component.PowerupTransform = GetComponent<Transform>();
		string @string = Strings.GetString(ActivationText);
		MonoSingleton<NotificationsQueue>.Instance.AddText(@string);
	}

	private bool IsInit()
	{
		return _cache != null;
	}

	private void Init()
	{
		_cache = ObjectCacheManager.Instance.GetCache(IndicatorPrefab);
	}

	private void OnDisable()
	{
		if (_indicator != null)
		{
			_indicator.Deactivate();
		}
	}
}
