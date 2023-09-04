using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class PanelShop : PanelAtlasController
{
	public CustomizePowerMeter PowerMeter;

	public ShopScrollList ScrollList;

	public int SlotIndex;

	private string _shopFeedGroup;

	private ShopScrollList.FeedDelegateType _shopFeedDelegate;

	public void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (_shopFeedGroup != string.Empty)
		{
			ScrollList.FeedDelegate = _shopFeedDelegate;
			ScrollList.FeedGroup(_shopFeedGroup);
			ResetShopDelegates();
		}
		else
		{
			Owner.ActivatePreviousPanel();
		}
	}

	private void OnGetMoreGoldButtonTap()
	{
		if ((int)MonoSingleton<Player>.Instance.Level > 1)
		{
			GamePlayHaven.Placement("bank_launch");
		}
		Dialogs.IAPShop("iaps_hard", true);
	}

	private void OnMoreGamesButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GamePlayHaven.Placement("more_games", true);
	}

	public override void OnActivate()
	{
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		dictionary.Add("de", 5.5f);
		dictionary.Add("es", 4.5f);
		dictionary.Add("fr", 5f);
		dictionary.Add("it", 5f);
		dictionary.Add("ru", 5.5f);
		Dictionary<string, float> dictionary2 = dictionary;
		if (dictionary2.ContainsKey(Strings.Locale))
		{
			GameObject gameObject = GameObject.Find("PlayButton");
			if ((bool)gameObject)
			{
				GameObject gameObject2 = gameObject.gameObject.transform.Find("ButtonText").gameObject;
				if ((bool)gameObject2)
				{
					SpriteText spriteText = gameObject2.GetComponent("SpriteText") as SpriteText;
					if ((bool)spriteText)
					{
						spriteText.SetCharacterSize(dictionary2[Strings.Locale]);
					}
				}
			}
		}
		base.OnActivate();
		ScrollList.ResetDelegates();
		ResetShopDelegates();
		GarageManager garageManager = Owner as GarageManager;
		MonoUtils.SetActive(garageManager.GarageObjects, false);
		PowerMeter.UpdateValue();
		if ((int)MonoSingleton<Player>.Instance.Level > 1)
		{
			GamePlayHaven.Placement("store_launch");
		}
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		ScrollList.Clear();
		GarageManager garageManager = Owner as GarageManager;
		MonoUtils.SetActive(garageManager.GarageObjects, true);
		garageManager.UpdateVehicle();
	}

	public void SaveShopDelegates()
	{
		_shopFeedGroup = ScrollList.GetCurrentGroup();
		_shopFeedDelegate = ScrollList.FeedDelegate;
	}

	public void ResetShopDelegates()
	{
		_shopFeedGroup = string.Empty;
		_shopFeedDelegate = null;
	}

	public void UpdateVehicle()
	{
		PowerMeter.UpdateValue();
	}

	private void Update()
	{
		if (MonoSingleton<GameController>.Instance.BackKeyReleased() && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}
}
