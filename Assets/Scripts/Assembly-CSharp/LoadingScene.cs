using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class LoadingScene : MonoBehaviour
{
	public enum Content
	{
		None = 0,
		Game = 1,
		Image = 2
	}

	public static Content ContentType;

	public static string Image = string.Empty;

	public static string Level = "GarageScene";

	public static bool WaitForMatchObjects;

	public static Action<string> finishDelegate;

	public static bool IsRunning;

	public SpriteText DebugText;

	public SpriteRoot Background;

	public SpriteText ProfilerString;

	public LoadingGameScreen LoadingScreen;

	public Camera UICamera;

	public float WaitTimeSeconds;

	public float DisappearTimeSeconds;

	private bool _isDone;

	private float _disappearTime;

	private bool _spawnNotification;

	private bool _tapToStart;

	private bool _acceptTapToStart;

	private string _loadingLevel;

	private void Start()
	{
		IsRunning = true;
		Background.SetSize(Screen.width, Screen.height);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		SingleGame.playOnEnable = false;
		MonoSingleton<NotificationsQueue>.Instance.Clear();
		StartCoroutine(WaitForLoadLevel());
		LoadingScreen.Activate(this);
	}

	public static void SetTextAlpha(SpriteText text, float a)
	{
		Color color = text.color;
		text.SetColor(new Color(color.r, color.g, color.b, color.a * a));
	}

	public static void SetRootAlpha(SpriteRoot root, float a)
	{
		Color color = root.color;
		root.SetColor(new Color(color.r, color.g, color.b, color.a * a));
	}

	private void SetAlpha(float a)
	{
		LoadingScreen.SetAlpha(a);
		SetRootAlpha(Background, a);
	}

	private void DestroyCamera()
	{
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Camera));
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object @object in array2)
		{
			Camera camera = @object as Camera;
			if (camera.name == UICamera.name)
			{
				UnityEngine.Object.Destroy(camera.gameObject);
				break;
			}
		}
	}

	[Conditional("IDT_CHEATS")]
	private void SetDebugText(string text)
	{
		DebugText.Text = text;
	}

	private IEnumerator WaitForLoadLevel()
	{
		yield return Resources.UnloadUnusedAssets();
		_loadingLevel = Level;
		yield return Application.LoadLevelAsync(_loadingLevel);
		if (MonoSingleton<Player>.Instance.Tutorial.IsFirstGamePlayed() && MonoSingleton<Player>.Instance.LastPlayedMode != "boss")
		{
			yield return new WaitForSeconds(1f);
		}
		if (WaitForMatchObjects)
		{
			while (VehiclesManager.instance == null)
			{
				yield return null;
			}
			while (IDTGame.Instance == null)
			{
				yield return null;
			}
			IDTGame game = IDTGame.Instance;
			if (game is SurvivalGame)
			{
				if (!game.IsBossFight)
				{
					_tapToStart = true;
				}
				else
				{
					(game as SurvivalGame).DelayedActivateGame();
				}
				StartCoroutine(Done());
				yield break;
			}
			if (game is MultiplayerGame)
			{
				_spawnNotification = true;
			}
			VehiclesManager i = VehiclesManager.instance;
			i.playerVehicleActivatedEvent += PlayerActivated;
		}
		else
		{
			StartCoroutine(Done());
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		if (!string.IsNullOrEmpty(_loadingLevel))
		{
			string loadingLevel = _loadingLevel;
			_loadingLevel = null;
			if (finishDelegate != null)
			{
				Action<string> action = finishDelegate;
				finishDelegate = null;
				action(loadingLevel);
			}
		}
	}

	private void PlayerActivated(Vehicle vehicle)
	{
		VehiclesManager instance = VehiclesManager.instance;
		instance.playerVehicleActivatedEvent -= PlayerActivated;
		StartCoroutine(Done());
	}

	private IEnumerator Done()
	{
		float waitTimeFinish = Time.realtimeSinceStartup + WaitTimeSeconds;
		while (waitTimeFinish < Time.realtimeSinceStartup)
		{
			yield return null;
		}
		DestroyCamera();
		GC.Collect();
		GC.WaitForPendingFinalizers();
		yield return Resources.UnloadUnusedAssets();
		if (_tapToStart)
		{
			_acceptTapToStart = true;
			LoadingScreen.TapToStart();
		}
		else
		{
			Disappear();
		}
	}

	public void Disappear()
	{
		_disappearTime = Time.realtimeSinceStartup;
		_isDone = true;
		IsRunning = false;
	}

	private void Update()
	{
		if (_isDone)
		{
			float disappearTimeSeconds = DisappearTimeSeconds;
			disappearTimeSeconds -= Time.realtimeSinceStartup - _disappearTime;
			if (disappearTimeSeconds <= 0f)
			{
				disappearTimeSeconds = 0f;
				SpawnNotification();
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				SetAlpha(disappearTimeSeconds / DisappearTimeSeconds);
			}
		}
	}

	private void SpawnNotification()
	{
		if (_spawnNotification)
		{
			UnityEngine.Object original = Resources.Load("Dialogs/NotificationSpawn");
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(original);
			NotificationSpawn component = gameObject.GetComponent<NotificationSpawn>();
			MonoSingleton<NotificationsQueue>.Instance.Show(component);
		}
	}

	private void OnLoadingSceneTap()
	{
		if (_acceptTapToStart)
		{
			LoadingScreen.StopAnimations();
			Disappear();
			SurvivalGame survivalGame = IDTGame.Instance as SurvivalGame;
			survivalGame.ActivateGame();
		}
	}
}
