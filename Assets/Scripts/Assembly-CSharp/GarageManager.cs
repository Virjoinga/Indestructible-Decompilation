using System.Collections;
using Glu.DynamicContentPipeline;
using UnityEngine;

public class GarageManager : PanelManager
{
	public bool GarageLeft;

	public PanelGarage GaragePanel;

	public Transform VehicleTransform;

	public Transform GarageObjects;

	private GameObject _vehicle;

	private GameObject _weapon;

	private GameObject _armor;

	private GameObject _body;

	private ShopItemVehicle _vehicleItem;

	private ShopItemWeapon _weaponItem;

	private ShopItemArmor _armorItem;

	private ShopItemBody _bodyItem;

	private static bool _playHavenGameLaunch;

	private static int _lastGamesPlayed = -1;

	private static bool? _firstGamePlayed;

	private string _forceBundleId;

	private bool _forcedBundleIsBossBundle;

	private bool _forceBundleCoroutineStarted;

	private int m_TjFeatureAppCnt;

	private bool m_ggnDisplayed;

	protected override void Start()
	{
		AJavaTools.UI.StopIndeterminateProgress();
		base.Start();
		if (!MonoSingleton<Player>.Instance.Tutorial.IsVehicleChoosen())
		{
			Dialogs.SelectVehicle();
		}
		else
		{
			Activate();
		}
		if (DynamicContent.CheckForUpdates())
		{
			Dialogs.ApplicationShouldBeUpdatedDialog();
		}
		else
		{
			StartCoroutine(CheckForTheGameUpdates());
		}
		if (!_firstGamePlayed.HasValue)
		{
			_firstGamePlayed = MonoSingleton<Player>.Instance.Tutorial.IsFirstGamePlayed();
		}
		StartCoroutine(PlayHavenGameLaunchRequest(false));
		StartCoroutine(WaitFacebookLogin());
		SpriteAtlasUtils.LoadMaterial("DialogsMaterial2");
		SpriteAtlasUtils.SetLocalizationMaterial();
		MonoSingleton<Player>.Instance.Challenges.Update();
		StartCoroutine(CheckAchievements());
		StartCoroutine(ResumeComponents());
		MonoSingleton<DialogsQueue>.Instance.Resume();
		UpdateVehicle();
		Object @object = GameObject.Find("EZAnimator");
		if (@object != null)
		{
			Object.Destroy(@object);
		}
		if (AJavaTools.Util.IsFirstLaunch())
		{
			Debug.Log("*** calling RestoreTransactions ***");
			AInAppPurchase.RestoreTransactions();
		}
		AInAppPurchase.RequestPendingPurchases();
	}

	public void Activate()
	{
		if (MonoSingleton<Player>.Instance.IsEnoughPower())
		{
			ActivatePanel("PanelGarage");
		}
		else
		{
			ActivatePanel("PanelCustomization");
			Dialogs.TooMuchPowerUsed();
		}
		m_TjFeatureAppCnt = PlayerPrefs.GetInt("INDTJFA.CNT");
		PlayerPrefs.SetInt("INDTJFA.CNT", ++m_TjFeatureAppCnt);
	}

	protected override void Awake()
	{
		base.Awake();
		MonoSingleton<NotificationsQueue>.Instance.Clear();
		MonoSingleton<GameController>.Instance.resumeEvent += OnRestore;
	}

	private void OnDestroy()
	{
		GWalletHelper.HideGGN();
		m_ggnDisplayed = false;
		if (MonoSingleton<GameController>.Exists())
		{
			MonoSingleton<GameController>.Instance.resumeEvent -= OnRestore;
		}
		SpriteAtlasUtils.UnloadMaterial("DialogsMaterial2");
		SpriteAtlasUtils.SetDefaultLocalizationMaterial();
	}

	private IEnumerator CheckForTheGameUpdates()
	{
		while (true)
		{
			DynamicContent.CheckForUpdates();
			yield return new WaitForSeconds(10f);
		}
	}

	private IEnumerator PlayHavenGameLaunchRequest(bool restore)
	{
		if (!MonoSingleton<Player>.Instance.Tutorial.IsVehicleChoosen())
		{
			_playHavenGameLaunch = true;
			yield break;
		}
		yield return new WaitForSeconds(1f);
		int gamesPlayed2 = MonoSingleton<Player>.Instance.Statistics.TotalGamesPlayed;
		bool showGameLaunch = _lastGamesPlayed == gamesPlayed2 || !_playHavenGameLaunch || restore || gamesPlayed2 < 1;
		_lastGamesPlayed = gamesPlayed2;
		if (showGameLaunch)
		{
			if ((int)MonoSingleton<Player>.Instance.Level > 1 || _firstGamePlayed.Value)
			{
				GamePlayHaven.Placement("game_launch");
			}
			else
			{
				GamePlayHaven.Placement("game_launch_tutorial");
			}
			_playHavenGameLaunch = true;
			yield break;
		}
		if (_firstGamePlayed.Value && gamesPlayed2 == 1)
		{
			MonoSingleton<Player>.Instance.Statistics.TotalGamesPlayed++;
			gamesPlayed2++;
		}
		if (gamesPlayed2 == 1)
		{
			GamePlayHaven.Placement("tutorial_end");
			yield break;
		}
		gamesPlayed2--;
		if (gamesPlayed2 < 11)
		{
			string s = "mission_end_" + gamesPlayed2;
			GamePlayHaven.Placement(s);
		}
		else
		{
			GamePlayHaven.Placement("mission_end_11plus");
		}
	}

	private void OnRestore()
	{
		if (!MonoSingleton<GameController>.Instance.SuspendBecauseOfIAP)
		{
			StopCoroutine("PlayHavenGameLaunchRequest");
			StartCoroutine(PlayHavenGameLaunchRequest(true));
		}
	}

	private IEnumerator ResumeComponents()
	{
		yield return new WaitForSeconds(2f);
		MonoSingleton<ShopController>.Instance.ResumeIAPProcessor();
		MonoSingleton<GameController>.Instance.EnableTapJoyPointsRetrieval();
	}

	private IEnumerator CheckAchievements()
	{
		yield return new WaitForSeconds(3f);
		while (!MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			yield return null;
		}
		MonoSingleton<Player>.Instance.Achievements.UpdateGameOver();
		MonoSingleton<Player>.Instance.Achievements.UpdateGarage();
		MonoSingleton<Player>.Instance.Achievements.Send();
		MonoSingleton<Player>.Instance.Save();
	}

	public void LoadLevel(string name)
	{
		GarageLeft = true;
		if (_activePanel != null)
		{
			_activePanel.OnDeactivate();
			_activePanel = null;
		}
		MonoSingleton<ShopController>.Instance.PauseIAPProcessor();
		MonoSingleton<GameController>.Instance.DisableTapJoyPointsRetrieval();
		MonoSingleton<Player>.Instance.StartLevel(name);
	}

	private GameObject ChangeItem(ShopItemGarage item, ShopItemGarage current, GameObject currentObject, Transform parent)
	{
		bool flag = item == null || current == null;
		if (!flag)
		{
			flag = item.id != current.id;
		}
		if (flag)
		{
			if (currentObject != null)
			{
				Object.Destroy(currentObject);
				currentObject = null;
			}
			if (item != null)
			{
				GameObject original = Resources.Load(item.GaragePrefab) as GameObject;
				currentObject = (GameObject)Object.Instantiate(original);
				current = item;
				Transform component = currentObject.GetComponent<Transform>();
				component.parent = parent;
				component.localPosition = Vector3.zero;
			}
		}
		return currentObject;
	}

	private void ChangeGarageVehicle(GarageVehicle item)
	{
		_vehicle = ChangeItem(item.Vehicle, _vehicleItem, _vehicle, VehicleTransform);
		if (_vehicle != null)
		{
			bool flag = _vehicleItem == null;
			if (!flag)
			{
				flag = item.Vehicle.id != _vehicleItem.id;
			}
			if (flag)
			{
				_weaponItem = null;
				_armorItem = null;
				_bodyItem = null;
			}
			Transform component = _vehicle.GetComponent<Transform>();
			_armor = ChangeItem(item.Armor, _armorItem, _armor, component);
			_body = ChangeItem(item.Body, _bodyItem, _body, component);
			Transform parent = component.Find("Weapon");
			_weapon = ChangeItem(item.Weapon, _weaponItem, _weapon, parent);
		}
		_vehicleItem = item.Vehicle;
		_weaponItem = item.Weapon;
		_armorItem = item.Armor;
		_bodyItem = item.Body;
	}

	public void ChangeVehicle(GarageVehicle item)
	{
		ChangeGarageVehicle(item);
		MonoSingleton<Player>.Instance.SelectedVehicle = item;
	}

	public void UpdateVehicle()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		ChangeGarageVehicle(selectedVehicle);
	}

	private void Update()
	{
		if (!(_activePanel != null) || LoadingScene.IsRunning || BootScene.IsRunning)
		{
			return;
		}
		if (MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			if (!MonoSingleton<SettingsController>.Instance.NotificationsPrompt)
			{
				Dialogs.SetNotification();
			}
			else if (!m_ggnDisplayed)
			{
				GWalletHelper.ShowGGN();
				m_ggnDisplayed = true;
			}
		}
		else if (m_ggnDisplayed)
		{
			GWalletHelper.HideGGN();
			m_ggnDisplayed = false;
		}
	}

	public void ForceOpenBundleOffer(string bundleId, bool bossBundle)
	{
		_forceBundleId = bundleId;
		_forcedBundleIsBossBundle = bossBundle;
		CheckForcedBundleOffer();
	}

	public void CheckForcedBundleOffer()
	{
		StartCoroutine(OpenForceBundleOffer());
	}

	private IEnumerator OpenForceBundleOffer()
	{
		_forceBundleCoroutineStarted = true;
		while (!string.IsNullOrEmpty(_forceBundleId))
		{
			if (MonoSingleton<DialogsQueue>.Instance.IsEmpty() && GetActivePanel().name == "PanelGarage")
			{
				ShopItemBundle bundle = MonoSingleton<ShopController>.Instance.GetItemBundle(_forceBundleId);
				if (bundle != null && !bundle.IsOwned())
				{
					PanelManagerPanel panel = ActivatePanel("PanelBundle");
					PanelBundle panelBundle = panel as PanelBundle;
					panelBundle.SetData(bundle, false, _forcedBundleIsBossBundle);
				}
				_forceBundleId = null;
				_forcedBundleIsBossBundle = false;
				break;
			}
			yield return new WaitForSeconds(0.3f);
		}
		_forceBundleCoroutineStarted = false;
	}

	private IEnumerator WaitFacebookLogin()
	{
		while (!ASocial.Facebook.IsLoggedIn())
		{
			yield return new WaitForSeconds(1f);
		}
		OnFacebookLoggedIn();
	}

	private void OnFacebookLoggedIn()
	{
		bool facebookLoginRewarded = MonoSingleton<Player>.Instance.Statistics.FacebookLoginRewarded;
		if (!facebookLoginRewarded)
		{
			GameAnalytics.EventFacebookLoggedIn("GarageManager", facebookLoginRewarded);
			MonoSingleton<Player>.Instance.Statistics.FacebookLoginRewarded = true;
			MonoSingleton<Player>.Instance.AddMoneyHard(10, "CREDIT_IN_GAME_AWARD", "Facebook Login", "FACEBOOK");
			MonoSingleton<Player>.Instance.Save();
		}
	}
}
