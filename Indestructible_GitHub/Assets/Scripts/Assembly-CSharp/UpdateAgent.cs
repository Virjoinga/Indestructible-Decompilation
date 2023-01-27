using UnityEngine;

public class UpdateAgent : MonoSingleton<UpdateAgent>
{
	private int _activeObjectCount;

	private IUpdatable[] _activeObjects = new IUpdatable[128];

	public void StartUpdate(IUpdatable obj)
	{
		_activeObjects[_activeObjectCount] = obj;
		if (_activeObjectCount == 0)
		{
			base.enabled = true;
		}
		_activeObjectCount++;
	}

	public void StopUpdate(IUpdatable obj)
	{
		int num = 0;
		while (num != _activeObjectCount)
		{
			if (_activeObjects[num] == obj)
			{
				_activeObjectCount--;
				_activeObjects[num] = _activeObjects[_activeObjectCount];
				_activeObjects[_activeObjectCount] = null;
			}
			else
			{
				num++;
			}
		}
		if (_activeObjectCount == 0)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		int num = 0;
		while (num != _activeObjectCount)
		{
			if (!_activeObjects[num].DoUpdate())
			{
				_activeObjectCount--;
				_activeObjects[num] = _activeObjects[_activeObjectCount];
				_activeObjects[_activeObjectCount] = null;
			}
			else
			{
				num++;
			}
		}
		if (_activeObjectCount == 0)
		{
			base.enabled = false;
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		int num = 0;
		while (num != _activeObjectCount)
		{
			IUpdatable updatable = _activeObjects[num];
			if (updatable is Object && updatable as Object == null)
			{
				_activeObjectCount--;
				_activeObjects[num] = _activeObjects[_activeObjectCount];
				_activeObjects[_activeObjectCount] = null;
			}
			else
			{
				num++;
			}
		}
		if (_activeObjectCount == 0)
		{
			base.enabled = false;
		}
	}
}
