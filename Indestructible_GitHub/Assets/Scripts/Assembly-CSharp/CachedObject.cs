using System.Collections.Generic;
using Glu;
using UnityEngine;

public class CachedObject : Glu.MonoBehaviour
{
	public abstract class Cache
	{
		protected GameObject _prefab;

		public Cache(GameObject prefab)
		{
			_prefab = prefab;
		}

		public CachedObject Activate()
		{
			CachedObject cachedObject = RetainObject();
			cachedObject.Activate();
			return cachedObject;
		}

		public CachedObject Activate(Vector3 pos)
		{
			CachedObject cachedObject = RetainObject();
			cachedObject.Activate(pos);
			return cachedObject;
		}

		public CachedObject Activate(Vector3 pos, Quaternion rot)
		{
			CachedObject cachedObject = RetainObject();
			cachedObject.Activate(pos, rot);
			return cachedObject;
		}

		public abstract void Deactivated(CachedObject obj);

		public abstract CachedObject RetainObject();

		protected virtual CachedObject InstantiateCachedObject()
		{
			return (Object.Instantiate(_prefab) as GameObject).GetComponent<CachedObject>();
		}
	}

	public class StackCache : Cache
	{
		private Stack<CachedObject> _instances;

		public StackCache(GameObject prefab)
			: base(prefab)
		{
			_instances = new Stack<CachedObject>();
		}

		public override void Deactivated(CachedObject obj)
		{
			_instances.Push(obj);
		}

		public override CachedObject RetainObject()
		{
			if (_instances.Count == 0)
			{
				CachedObject cachedObject = InstantiateCachedObject();
				cachedObject.cache = this;
				return cachedObject;
			}
			return _instances.Pop();
		}
	}

	private Transform _transform;

	private Cache _cache;

	public Cache cache
	{
		get
		{
			return _cache;
		}
		set
		{
			_cache = value;
		}
	}

	public new Transform transform
	{
		get
		{
			return _transform;
		}
	}

	public virtual Cache CreateCache(GameObject prefab)
	{
		return new StackCache(prefab);
	}

	public virtual void Activate(Vector3 pos, Quaternion rot)
	{
		_transform.localRotation = rot;
		Activate(pos);
	}

	public virtual void Activate(Vector3 pos)
	{
		_transform.localPosition = pos;
		Activate();
	}

	public virtual void Activate()
	{
		base.gameObject.SetActiveRecursively(true);
	}

	public virtual void Deactivate()
	{
		if (base.gameObject.active)
		{
			base.gameObject.SetActiveRecursively(false);
			cache.Deactivated(this);
		}
	}

	protected virtual void Awake()
	{
		_transform = GetComponent<Transform>();
		AwakeActiveState();
	}

	protected virtual void AwakeActiveState()
	{
		base.gameObject.SetActiveRecursively(false);
	}
}
