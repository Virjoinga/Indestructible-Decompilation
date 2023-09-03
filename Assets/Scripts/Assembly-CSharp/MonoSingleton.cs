using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : Component
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if ((Object)_instance == (Object)null)
			{
				string text = typeof(T).ToString();
				GameObject gameObject = new GameObject(text);
				_instance = gameObject.AddComponent<T>();
				Object.DontDestroyOnLoad(gameObject);
			}
			return _instance;
		}
	}

	private static bool IsSingleton()
	{
		string text = typeof(T).ToString();
		GameObject gameObject = GameObject.Find(text);
		return gameObject == null;
	}

	public static bool Exists()
	{
		return (Object)_instance != (Object)null;
	}

	public virtual T Create()
	{
		return _instance;
	}

	protected virtual void Awake()
	{
	}

	protected virtual void OnDestroy()
	{
		_instance = (T)null;
	}

	public static void DestroyInstance()
	{
		if ((Object)_instance != (Object)null)
		{
			Object.DestroyImmediate(_instance.gameObject, false);
			_instance = (T)null;
		}
	}
}
