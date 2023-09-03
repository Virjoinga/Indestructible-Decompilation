using UnityEngine;

public class CustomSinglePlayStarter : MonoBehaviour
{
	public static CustomSinglePlayStarter _instance;

	private string _config;

	private string _levelName;

	private bool _forceDestroyOnLoad;

	private GameModeConf _configInstance;

	public static void StartCustomGame(string config, string levelName)
	{
		if (_instance != null)
		{
			Object.Destroy(_instance.gameObject);
		}
		GameObject gameObject = new GameObject("SingleGame:" + config);
		_instance = gameObject.AddComponent<CustomSinglePlayStarter>();
		_instance._config = config;
		_instance._levelName = levelName;
		Object.DontDestroyOnLoad(gameObject);
		MonoSingleton<Player>.Instance.StartMatchLevel(levelName, null);
	}

	public static void StartCustomGame(GameModeConf configInstance, string levelName)
	{
		if (_instance != null)
		{
			Object.Destroy(_instance.gameObject);
		}
		GameObject gameObject = new GameObject("SingleGame:" + configInstance.ToString());
		_instance = gameObject.AddComponent<CustomSinglePlayStarter>();
		_instance._configInstance = configInstance;
		_instance._levelName = levelName;
		Object.DontDestroyOnLoad(gameObject);
		MonoSingleton<Player>.Instance.StartMatchLevel(levelName, null);
	}

	private void Configure()
	{
		Debug.Log("CustomSinglePlayStarter.Configure:" + _config);
		if (_configInstance != null)
		{
			_configInstance.Configure(base.gameObject);
		}
		if (!string.IsNullOrEmpty(_config))
		{
			GameModeConf gameModeConf = BundlesUtils.Load("Assets/Bundles/" + _config + ".asset") as GameModeConf;
			if (gameModeConf != null)
			{
				gameModeConf.Configure(base.gameObject);
			}
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		Debug.Log(string.Format("CustomSinglePlayStarter.OnLevelWasLoaded: level={0}, _forceDestroyOnLoad={1}, loadedLevelName={2}, matchLevelName={3}, _config={4}", level, _forceDestroyOnLoad, Application.loadedLevelName, _levelName, _config));
		if (_forceDestroyOnLoad)
		{
			Object.Destroy(base.gameObject);
		}
		if (_levelName == Application.loadedLevelName)
		{
			Configure();
			_forceDestroyOnLoad = true;
		}
	}
}
