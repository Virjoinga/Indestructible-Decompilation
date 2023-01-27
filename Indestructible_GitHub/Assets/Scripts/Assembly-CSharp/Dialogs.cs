using Glu.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Dialogs
{
	public static DialogTemplate CreateTemplate()
	{
		Object original = Resources.Load("Dialogs/DialogTemplate");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		return gameObject.GetComponent<DialogTemplate>();
	}

	public static void IAPPurchaseSuccessful(IAPShopItem item)
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		if (item is IAPShopItemSimple)
		{
			IAPShopItemSimple iAPShopItemSimple = item as IAPShopItemSimple;
			string @string = Strings.GetString("IDS_IAP_SUCCESSFUL_TEXT");
			dialogTemplate.Body.Text = string.Format(@string, iAPShopItemSimple.GetValueString());
			dialogTemplate.Title.Text = Strings.GetString("IDS_IAP_SUCCESSFUL_TITLE");
			dialogTemplate.RightButton.Text = Strings.GetString("IDS_IAP_SUCCESSFUL_BUTTON_OK");
		}
		else if (item is IAPShopItemBoost)
		{
			dialogTemplate.Title.Text = Strings.GetString("IDS_IAP_SUCCESSFUL_TITLE");
			string id = "IDS_IAP_SUCCESSFUL_BOOST";
			GameConfiguration configuration = MonoSingleton<GameController>.Instance.Configuration;
			if (configuration.Boosting.BoostForever)
			{
				id = "IDS_IAP_SUCCESSFUL_BOOST_FOREVER";
			}
			dialogTemplate.Body.Text = Strings.GetString(id);
			dialogTemplate.RightButton.Text = Strings.GetString("IDS_IAP_SUCCESSFUL_BUTTON_OK");
		}
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void IAPBuy(IAPShopItem item)
	{
		Object original = Resources.Load("Dialogs/DialogIAPBuy");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogIAPBuy component = gameObject.GetComponent<DialogIAPBuy>();
		MonoSingleton<DialogsQueue>.Instance.Show(component);
		component.Item = item;
	}

	public static void IAPShop(string groupId, bool buyMoreButtonUsed)
	{
		GameAnalytics.EventAddMoreCurrency(groupId);
		if (Application.internetReachability != 0)
		{
			Object original = Resources.Load("Dialogs/DialogIAPShop");
			GameObject gameObject = (GameObject)Object.Instantiate(original);
			DialogIAPShop component = gameObject.GetComponent<DialogIAPShop>();
			MonoSingleton<DialogsQueue>.Instance.Show(component);
			component.GroupId = groupId;
		}
		else if (buyMoreButtonUsed)
		{
			IAPNoNetworkAvailable();
		}
		else
		{
			NotEnoughCurrency(groupId, false);
		}
	}

	public static void DialogIAPRestore()
	{
		Object original = Resources.Load("Dialogs/DialogIAPRestore");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogIAPRestore component = gameObject.GetComponent<DialogIAPRestore>();
		MonoSingleton<DialogsQueue>.Instance.Show(component);
	}

	public static void IAPRestoreFailed()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_IAP_RESTORE_FAILED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_IAP_RESTORE_FAILED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_IAP_RESTORE_FAILED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void IAPRestoreEmpty()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_IAP_RESTORE_EMPTY_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_IAP_RESTORE_EMPTY_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_IAP_RESTORE_EMPTY_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void NotEnoughCurrency(string groupId, bool showBuyMoreButton)
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_NOT_ENOUGH_CURRENCY_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_NOT_ENOUGH_CURRENCY_TITLE");
		if (showBuyMoreButton)
		{
			dialogTemplate.LeftButton.Text = Strings.GetString("IDS_NOT_ENOUGH_CURRENCY_BUTTON_CANCEL");
			dialogTemplate.RightButton.Text = Strings.GetString("IDS_NOT_ENOUGH_CURRENCY_BUTTON_BUY");
			dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
			{
				IAPShop(groupId, true);
				dialog.Close();
			};
			dialogTemplate.OnLeftButtonDelegate = delegate(DialogTemplate dialog)
			{
				dialog.Close();
			};
		}
		else
		{
			dialogTemplate.RightButton.Text = Strings.GetString("IDS_NOT_ENOUGH_CURRENCY_BUTTON_OK");
			dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
			{
				dialog.Close();
			};
		}
	}

	public static void NotEnoughPower()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_NOT_ENOUGH_POWER_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_NOT_ENOUGH_POWER_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_NOT_ENOUGH_POWER_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void TooMuchPowerUsed()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		dialogTemplate.Body.Text = Strings.GetString("IDS_TOO_MUCH_POWER_USED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_TOO_MUCH_POWER_USED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_TOO_MUCH_POWER_USED_BUTTON_OK");
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void IAPTurnedOff()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_IAP_TURNED_OFF_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_IAP_TURNED_OFF_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_IAP_TURNED_OFF_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void IAPCancelled()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_IAP_CANCELLED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_IAP_CANCELLED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_IAP_CANCELLED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			GamePlayHaven.Placement("iap_cancelled_in_store");
			dialog.Close();
		};
	}

	public static void IAPFailed(bool turnedOn)
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_IAP_FAILED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_IAP_FAILED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_IAP_FAILED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void IAPNoNetworkAvailable()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_IAP_NO_NETWORK_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_IAP_NO_NETWORK_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_IAP_NO_NETWORK_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void MatchmakingFailed(PanelManager m)
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_MATCHMAKING_FAILED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_MATCHMAKING_FAILED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_MATCHMAKING_FAILED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
			m.ActivateDirectly("SelectMapPanel");
		};
		dialogTemplate.OnBackKeyDelegate = dialogTemplate.OnRightButtonDelegate;
	}

	public static void GameDisconnected()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_GAME_DISCONNECTED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_GAME_DISCONNECTED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_GAME_DISCONNECTED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			MonoSingleton<Player>.Instance.StartLevel("GarageScene");
			dialog.Close();
		};
		dialogTemplate.OnBackKeyDelegate = dialogTemplate.OnRightButtonDelegate;
	}

	public static void ConnectionFailed()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_CONNECTION_FAILED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_CONNECTION_FAILED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_CONNECTION_FAILED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void ResetTalentsConfirmation(TalentsTalents talents)
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		ShopItemPrice itemPrice = MonoSingleton<ShopController>.Instance.GetItemPrice("price_talents_reset");
		string @string = Strings.GetString("IDS_RESET_TALENTS_CONFIRMATION_TEXT");
		dialogTemplate.Body.Text = string.Format(@string, itemPrice.GetPriceString(ShopItemCurrency.None, false, true));
		dialogTemplate.Title.Text = Strings.GetString("IDS_RESET_TALENTS_CONFIRMATION_TITLE");
		dialogTemplate.LeftButton.Text = Strings.GetString("IDS_RESET_TALENTS_CONFIRMATION_BUTTON_CANCEL");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_RESET_TALENTS_CONFIRMATION_BUTTON_OK");
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			int talentPointsSpent = MonoSingleton<Player>.Instance.GetTalentPointsSpent();
			if (MonoSingleton<Player>.Instance.ResetTalents())
			{
				GameAnalytics.EventResetTalents(talentPointsSpent);
				talents.SelectDefault();
				talents.UpdateTalents();
				dialog.Close();
			}
		};
		dialogTemplate.OnLeftButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void HelpDialog()
	{
		Object original = Resources.Load("Dialogs/DialogHelp");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogHelp component = gameObject.GetComponent<DialogHelp>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
	}

	public static void SettingsDialog()
	{
		Object original = Resources.Load("Dialogs/DialogSettings");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogSettings component = gameObject.GetComponent<DialogSettings>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
	}

	public static void AboutDialog()
	{
		Object original = Resources.Load("Dialogs/DialogAbout");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogAbout component = gameObject.GetComponent<DialogAbout>();
		MonoSingleton<DialogsQueue>.Instance.Show(component);
	}

	public static void CreditsDialog()
	{
		Object original = Resources.Load("Dialogs/DialogCredits");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogCredits component = gameObject.GetComponent<DialogCredits>();
		MonoSingleton<DialogsQueue>.Instance.Show(component);
	}

	public static void SecretDialog()
	{
		Object original = Resources.Load("Dialogs/DialogSecret");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogSecret component = gameObject.GetComponent<DialogSecret>();
		MonoSingleton<DialogsQueue>.Instance.Show(component);
	}

	public static void ApplicationGotForcedUpdateDialog()
	{
	}

	public static void ApplicationGotUpdateDialog()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_APP_GOT_UPDATE_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_APP_GOT_UPDATE_TITLE");
		dialogTemplate.LeftButton.Text = Strings.GetString("IDS_APP_GOT_UPDATE_BUTTON_CANCEL");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_APP_GOT_UPDATE_BUTTON_OK");
		dialogTemplate.OnLeftButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
			Application.OpenURL(ForcedUpdate.GetUpdateURL());
		};
	}

	public static void ApplicationFailedToLoadAssetBundlesDialog()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_APP_FAILED_TO_LOAD_ASSET_BUNDLES_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_APP_FAILED_TO_LOAD_ASSET_BUNDLES_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_APP_FAILED_TO_LOAD_ASSET_BUNDLES_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			MonoSingleton<Player>.Instance.Save();
			dialog.Close();
			SceneManager.LoadSceneAsync(0);
		};
		dialogTemplate.OnBackKeyDelegate = dialogTemplate.OnRightButtonDelegate;
	}

	public static void ApplicationShouldBeUpdatedDialog()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_APP_SHOULD_BE_UPDATED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_APP_SHOULD_BE_UPDATED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_APP_SHOULD_BE_UPDATED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			MonoSingleton<Player>.Instance.Save();
			dialog.Close();
			SceneManager.LoadSceneAsync(0);
		};
		dialogTemplate.OnBackKeyDelegate = dialogTemplate.OnRightButtonDelegate;
	}

	public static void TapJoyPointsReceived(int points)
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		string @string = Strings.GetString("IDS_TAPJOY_POINTS_RECEIVED_TEXT");
		dialogTemplate.Body.Text = string.Format(@string, points, Strings.GetString("IDS_CURRENCY_HARD"));
		dialogTemplate.Title.Text = Strings.GetString("IDS_TAPJOY_POINTS_RECEIVED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_TAPJOY_POINTS_RECEIVED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void ShopVehicleDetails(ShopItemVehicle vehicle, ShopItemStandard item)
	{
		Object original = Resources.Load("Dialogs/DialogShopVehicleDetails");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogShopVehicleDetails component = gameObject.GetComponent<DialogShopVehicleDetails>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
		component.SetData(vehicle, item);
	}

	public static void ShopVehicleDetails(GarageItemStandard item)
	{
		Object original = Resources.Load("Dialogs/DialogShopVehicleDetails");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogShopVehicleDetails component = gameObject.GetComponent<DialogShopVehicleDetails>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
		component.SetData(item);
	}

	public static void ShopBodyDetails(ShopItemBody body, ShopItemStandard item)
	{
		Object original = Resources.Load("Dialogs/DialogShopBodyDetails");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogShopBodyDetails component = gameObject.GetComponent<DialogShopBodyDetails>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
		component.SetData(body, item);
	}

	public static void ShopWeaponDetails(ShopItemWeapon weapon, ShopItemStandard item)
	{
		Object original = Resources.Load("Dialogs/DialogShopWeaponDetails");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogShopWeaponDetails component = gameObject.GetComponent<DialogShopWeaponDetails>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
		component.SetData(weapon, item);
	}

	public static void EnterPlayerName()
	{
		Object original = Resources.Load("Dialogs/DialogPlayerName");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogPlayerName component = gameObject.GetComponent<DialogPlayerName>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
	}

	public static void SelectVehicle()
	{
		Object original = Resources.Load("Dialogs/DialogSelectVehicle");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogSelectVehicle component = gameObject.GetComponent<DialogSelectVehicle>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
	}

	public static void GameRewards(ref IDTGame.Reward reward, IDTGame game)
	{
		Object original = Resources.Load("Dialogs/DialogGameRewards");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogGameRewards component = gameObject.GetComponent<DialogGameRewards>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
		component.SetData(ref reward, game);
	}

	public static void LeagueChange(int leagueChange)
	{
		if (leagueChange != 0)
		{
			GameAnalytics.EventLeagueChanged(leagueChange);
			Object original = Resources.Load("Dialogs/DialogLeagueChange");
			GameObject gameObject = (GameObject)Object.Instantiate(original);
			DialogLeagueChange component = gameObject.GetComponent<DialogLeagueChange>();
			MonoSingleton<DialogsQueue>.Instance.Show(component);
			component.SetData(leagueChange);
		}
	}

	public static void PauseGame()
	{
		if (IDTGame.Instance != null)
		{
			Object original = Resources.Load("Dialogs/DialogPause");
			GameObject gameObject = (GameObject)Object.Instantiate(original);
			DialogPause component = gameObject.GetComponent<DialogPause>();
			MonoSingleton<DialogsQueue>.Instance.Add(component);
		}
	}

	public static void RefuelVehicle()
	{
		Object original = Resources.Load("Dialogs/DialogRefuel");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogRefuel component = gameObject.GetComponent<DialogRefuel>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
	}

	public static void LevelUpDialog(int levelsGained)
	{
		if (levelsGained > 0 && !MonoSingleton<DialogsQueue>.Instance.ContainDialog<DialogLevelUp>())
		{
			Object original = Resources.Load("Dialogs/DialogLevelUp");
			GameObject gameObject = (GameObject)Object.Instantiate(original);
			DialogLevelUp component = gameObject.GetComponent<DialogLevelUp>();
			MonoSingleton<DialogsQueue>.Instance.Add(component);
		}
	}

	public static void CustomMatchInviteReceived()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.Body.Text = "You have been invited to custom match! Do you want to join game?";
		dialogTemplate.Title.Text = "DECLINE";
		dialogTemplate.RightButton.Text = "JOIN!";
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void InfluencePointsLocked(int points)
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Add(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_INFLUENCE_POINTS_LOCKED_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_INFLUENCE_POINTS_LOCKED_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_INFLUENCE_POINTS_LOCKED_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void FacebookNameIncorrect()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Body.Text = Strings.GetString("IDS_FACEBOOK_NAME_INCORRECT_TEXT");
		dialogTemplate.Title.Text = Strings.GetString("IDS_FACEBOOK_NAME_INCORRECT_TITLE");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_FACEBOOK_NAME_INCORRECT_BUTTON_OK");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
	}

	public static void RateMe()
	{
		Object original = Resources.Load("Dialogs/DialogRateMe");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogRateMe component = gameObject.GetComponent<DialogRateMe>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
	}

	public static void BundleContent(ShopItemBundle bundle)
	{
		Object original = Resources.Load("Dialogs/DialogBundleContent");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogBundleContent component = gameObject.GetComponent<DialogBundleContent>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
		component.SetData(bundle, true, false);
	}

	public static void SelectServer()
	{
		Object original = Resources.Load("Dialogs/DialogSelectServer");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogSelectServer component = gameObject.GetComponent<DialogSelectServer>();
		MonoSingleton<DialogsQueue>.Instance.Show(component);
	}

	public static void DailyChallenges()
	{
		Object original = Resources.Load("Dialogs/DialogDailyChallenges");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogDailyChallenges component = gameObject.GetComponent<DialogDailyChallenges>();
		MonoSingleton<DialogsQueue>.Instance.Show(component);
	}

	public static void QuitConfirmation()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Title.Text = Strings.GetString("quit_confirm_title");
		dialogTemplate.Body.Text = Strings.GetString("quit_confirm_body");
		dialogTemplate.LeftButton.Text = Strings.GetString("quit_button_ok");
		dialogTemplate.RightButton.Text = Strings.GetString("quit_button_cancel");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
		dialogTemplate.OnLeftButtonDelegate = delegate
		{
			MonoSingleton<GameController>.Instance.QuitApplicaiton();
		};
		dialogTemplate.OnBackKeyDelegate = dialogTemplate.OnRightButtonDelegate;
	}

	public static void PostRateGameAward()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Title.Text = Strings.GetString("rate_game_award_title");
		dialogTemplate.Body.Text = Strings.GetString("rate_game_award_body");
		dialogTemplate.LeftButton.Text = Strings.GetString("quit_button_ok");
		dialogTemplate.OnLeftButtonDelegate = delegate(DialogTemplate dialog)
		{
			dialog.Close();
		};
		dialogTemplate.OnBackKeyDelegate = dialogTemplate.OnLeftButtonDelegate;
	}

	public static void SetNotification()
	{
		DialogTemplate dialogTemplate = CreateTemplate();
		MonoSingleton<DialogsQueue>.Instance.Show(dialogTemplate);
		dialogTemplate.Title.Text = Strings.GetString("IDS_SETTINGS_DIALOG_NOTIFICATIONS");
		dialogTemplate.Body.Text = Strings.GetString("notifications_prompt_body");
		dialogTemplate.RightButton.Text = Strings.GetString("IDS_SETTINGS_DIALOG_BUTTON_ON");
		dialogTemplate.LeftButton.Text = Strings.GetString("IDS_SETTINGS_DIALOG_BUTTON_OFF");
		dialogTemplate.OnRightButtonDelegate = delegate(DialogTemplate dialog)
		{
			if (!MonoSingleton<SettingsController>.Instance.NotificationsEnabled)
			{
				MonoSingleton<SettingsController>.Instance.ToggleNotifications();
			}
			MonoSingleton<SettingsController>.Instance.NotificationsPrompt = true;
			MonoSingleton<SettingsController>.Instance.Save();
			dialog.Close();
		};
		dialogTemplate.OnLeftButtonDelegate = delegate(DialogTemplate dialog)
		{
			if (MonoSingleton<SettingsController>.Instance.NotificationsEnabled)
			{
				MonoSingleton<SettingsController>.Instance.ToggleNotifications();
			}
			MonoSingleton<SettingsController>.Instance.NotificationsPrompt = true;
			MonoSingleton<SettingsController>.Instance.Save();
			dialog.Close();
		};
		dialogTemplate.OnBackKeyDelegate = dialogTemplate.OnRightButtonDelegate;
	}

	public static void BossVictory(bool winner)
	{
		Object original = Resources.Load("Dialogs/DialogBossFightFinished");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DialogBossFightFinished component = gameObject.GetComponent<DialogBossFightFinished>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
		component.SetData(winner);
	}

	public static void OpenBundle(string bundleId, bool bossBundle)
	{
		Object original = Resources.Load("Dialogs/DummyShowBundleDialog");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DummyOpenBundleDialog component = gameObject.GetComponent<DummyOpenBundleDialog>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
		component.SetData(bundleId, bossBundle);
	}

	public static void OpenCustomizeLoadoutPanelDummy()
	{
		Object original = Resources.Load("Dialogs/DialogCustomLoadout");
		GameObject gameObject = (GameObject)Object.Instantiate(original);
		DummyOpenCustomLoadoutDialog component = gameObject.GetComponent<DummyOpenCustomLoadoutDialog>();
		MonoSingleton<DialogsQueue>.Instance.Add(component);
	}
}
