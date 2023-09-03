using UnityEngine;

public class PooledObject : CachedObject
{
	public class PoolCache : Cache
	{
		private uint _nextIndex;

		private uint _poolIndexMask;

		private CachedObject[] _pool;

		public PoolCache(GameObject prefab, int poolSize)
			: base(prefab)
		{
			_nextIndex = 0u;
			uint num = Util.Round16ToNextPowerOfTwo((uint)poolSize);
			_poolIndexMask = num - 1;
			_pool = new CachedObject[poolSize];
			for (int i = 0; i != poolSize; i++)
			{
				_pool[i] = InstantiateCachedObject();
			}
		}

		public override void Deactivated(CachedObject obj)
		{
		}

		public override CachedObject RetainObject()
		{
			CachedObject result = _pool[_nextIndex];
			_nextIndex = (_nextIndex + 1) & _poolIndexMask;
			return result;
		}
	}

	public int poolSize = 16;

	public override Cache CreateCache(GameObject prefab)
	{
		return new PoolCache(prefab, poolSize);
	}

	public override void Activate()
	{
	}

	public override void Deactivate()
	{
	}

	protected override void AwakeActiveState()
	{
	}
}
