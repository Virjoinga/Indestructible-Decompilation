using System.Collections.Generic;
using UnityEngine;

public class ObjectCacheManager : MonoBehaviour
{
	private Dictionary<GameObject, CachedObject.Cache> _cacheMap;

	private static ObjectCacheManager _instance;

	public static ObjectCacheManager Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject("ObjectCacheManager");
				_instance = gameObject.AddComponent<ObjectCacheManager>();
				_instance.Init();
			}
			return _instance;
		}
	}

	private void OnDestroy()
	{
		_instance = null;
	}

	public CachedObject.Cache GetCache(GameObject prefab)
	{
		CachedObject.Cache value;
		if (_cacheMap.TryGetValue(prefab, out value))
		{
			return value;
		}
		CachedObject component = prefab.GetComponent<CachedObject>();
		value = component.CreateCache(prefab);
		_cacheMap.Add(prefab, value);
		return value;
	}

	private void Init()
	{
		_cacheMap = new Dictionary<GameObject, CachedObject.Cache>();
	}
}
