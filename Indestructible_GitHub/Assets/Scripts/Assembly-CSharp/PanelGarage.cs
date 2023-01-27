using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelGarage : PanelAtlasController
{
	public GarageScrollList GarageScrollList;

	public NewVehiclesButton NewVehiclesButton;

	public GarageCustomizeButton CustomizeButton;

	public void OnProfileButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GameAnalytics.EventProfileButtonTap();
		GarageManager garageManager = Owner as GarageManager;
		garageManager.ActivatePanel("PanelProfile");
	}

	public void OnPaintButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GameAnalytics.EventPaintJobsButtonTap();
		GarageManager garageManager = Owner as GarageManager;
		PanelManagerPanel panelManagerPanel = garageManager.ActivatePanel("PanelShop");
		PanelShop panelShop = panelManagerPanel as PanelShop;
		MonoUtils.SetActive(panelShop.PowerMeter, false);
		panelShop.ScrollList.FeedBodies(string.Empty);
	}

	public static void StartPractice()
	{
		MonoSingleton<Player>.Instance.LastPlayedType = "multiplayer";
		MonoSingleton<Player>.Instance.LastPlayedGame = "TeamDeathmatchConf";
		MonoSingleton<Player>.Instance.LastPlayedMode = "practice";
		MonoSingleton<Player>.Instance.LastPlayedMap = "koh_iceberg";
		MonoSingleton<Player>.Instance.LastPlayedPlayers = 4;
		GameObject gameObject = new GameObject("Match");
		MultiplayerMatch match = gameObject.AddComponent<MultiplayerMatch>();
		SelectMatchmakingPanel.StartPractice(match);
	}

	public static bool NeedOpenAmmunitionPanel()
	{
		if ((int)MonoSingleton<Player>.Instance.Level < 6)
		{
			if (MonoSingleton<Player>.Instance.Statistics.TotalBoughtAmmunition < 1)
			{
				return false;
			}
		}
		else if ((int)MonoSingleton<Player>.Instance.Level < 13 && MonoSingleton<Player>.Instance.Statistics.TotalBoughtAmmunition < 1)
		{
			if (MonoSingleton<Player>.Instance.Statistics.PanelAmmunitionShownLevel < (int)MonoSingleton<Player>.Instance.Level)
			{
				MonoSingleton<Player>.Instance.Statistics.PanelAmmunitionShownLevel = MonoSingleton<Player>.Instance.Level;
				MonoSingleton<Player>.Instance.Save();
				return true;
			}
			return false;
		}
		return MonoSingleton<Player>.Instance.SelectedVehicle.Ammunition == null;
	}

	public static void StartSelectScene(GarageManager manager)
	{
		if (MonoSingleton<Player>.Instance.Tutorial.IsFirstGamePlayed())
		{
			if (MonoSingleton<Player>.Instance.Tutorial.IsNicknameSet())
			{
				manager.LoadLevel("SelectScene");
			}
			else
			{
				Dialogs.EnterPlayerName();
			}
		}
		else
		{
			StartPractice();
		}
	}

	public void OnPlayButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		MonoSingleton<Player>.Instance.Tutorial.SetPlayButtonTap();
		GameAnalytics.EventPlayButtonTap(base.Name);
		if (!MonoSingleton<Player>.Instance.IsEnoughFuel())
		{
			Dialogs.RefuelVehicle();
		}
		else if (NeedOpenAmmunitionPanel())
		{
			GarageManager garageManager = Owner as GarageManager;
			garageManager.ActivatePanel("PanelAmmunition");
		}
		else
		{
			GarageManager manager = Owner as GarageManager;
			StartSelectScene(manager);
		}
	}

	public void OnNewVehicleButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GameAnalytics.EventBuyNewVehiclesButtonTap();
		GarageManager garageManager = Owner as GarageManager;
		List<PlayerBundle> activeBundles = MonoSingleton<Player>.Instance.GetActiveBundles();
		if (activeBundles.Count == 0)
		{
			PanelManagerPanel panelManagerPanel = garageManager.ActivatePanel("PanelShop");
			PanelShop panelShop = panelManagerPanel as PanelShop;
			MonoUtils.SetActive(panelShop.PowerMeter, false);
			panelShop.ScrollList.FeedVehicles(string.Empty);
		}
		else
		{
			PanelManagerPanel panelManagerPanel2 = garageManager.ActivateDirectly("PanelBundle");
			PanelBundle panelBundle = panelManagerPanel2 as PanelBundle;
			PlayerBundle playerBundle = activeBundles[0];
			panelBundle.SetData(playerBundle.Item, true, false);
		}
	}

	public void OnDailyChallengesButtonTap()
	{
		Dialogs.DailyChallenges();
	}

	public void CustomizeButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GameAnalytics.EventCustomizeButtonTap("garage");
		GarageManager garageManager = Owner as GarageManager;
		garageManager.ActivatePanel("PanelCustomization");
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		CustomizeButton.OnDeactivate();
	}

	public override void OnActivate()
	{
		base.OnActivate();
		GarageScrollList.FeedItems();
		CustomizeButton.OnActivate();
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		GarageScrollList.Select(selectedVehicle.Vehicle.id, true);
		NewVehiclesButton.OnActivate();
		MonoSingleton<DialogsQueue>.Instance.Resume();
		StartCoroutine(ShowRateMe());
		GWalletHelper.ShowNotification("LAUNCH");
	}

	public void ChangeVehicle(GarageItemStandard item)
	{
		GarageManager garageManager = Owner as GarageManager;
		garageManager.ChangeVehicle(item.Item);
	}

	private IEnumerator ShowRateMe()
	{
		if (MonoSingleton<Player>.Instance.Statistics.RateMeLastOfferLevel < 0)
		{
			yield break;
		}
		yield return new WaitForSeconds(2f);
		int level = MonoSingleton<Player>.Instance.Level;
		int[] showLevels = new int[2] { 5, 10 };
		int lastOfferLevel = MonoSingleton<Player>.Instance.Statistics.RateMeLastOfferLevel;
		for (int i = 0; i < showLevels.Length; i++)
		{
			if (level >= showLevels[i] && showLevels[i] > lastOfferLevel)
			{
				Dialogs.RateMe();
				break;
			}
		}
	}

	private void Update()
	{
		if (MonoSingleton<GameController>.Instance.BackKeyReleased() && MonoSingleton<DialogsQueue>.Instance.IsEmpty() && !GWalletHelper.IsGWalletDisplayActive())
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			Dialogs.QuitConfirmation();
		}
	}
}
