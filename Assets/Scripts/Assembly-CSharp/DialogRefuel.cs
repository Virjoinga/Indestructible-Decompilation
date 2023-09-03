using System;
using Glu.Localization;
using UnityEngine;

public class DialogRefuel : UIDialog
{
	public UIButton GallonButton;

	public SpriteText GallonText;

	public SpriteText GallonPrice;

	public UIButton FullButton;

	public SpriteText FullText;

	public SpriteText FullPrice;

	public SpriteText FullLabel;

	public InclinedProgressBarSimple FuelMeter;

	public PackedSprite FuelIcon;

	public SpriteText RefuelTime;

	private ShopItemPrice _priceGallon;

	private ShopItemPrice _priceFreeze;

	private int _time;

	private bool _frozen;

	private void OnCloseButtonTap()
	{
		Close();
	}

	private void OnGallonButtonTap()
	{
		if (MonoSingleton<Player>.Instance.Buy(_priceGallon))
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Refuel);
			GameAnalytics.EventGallonPurchased(_priceGallon);
			MonoSingleton<Player>.Instance.SelectedVehicle.Fuel.AddGallon();
			MonoSingleton<Player>.Instance.Save();
			UpdateContent();
		}
	}

	private void OnFullButtonTap()
	{
		if (MonoSingleton<Player>.Instance.Buy(_priceFreeze))
		{
			FullButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Refuel);
			GameAnalytics.EventDayTankPurchased(_priceFreeze);
			MonoSingleton<Player>.Instance.SelectedVehicle.Fuel.Freeze();
			MonoSingleton<Player>.Instance.Save();
			UpdateContent();
			UpdateFreeze();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_priceGallon = MonoSingleton<ShopController>.Instance.GetItemPrice("price_refuel_gallon");
		GallonText.Text = Strings.GetString("IDS_REFUEL_VEHICLE_GALLON_TEXT");
		GameConfiguration configuration = MonoSingleton<GameController>.Instance.Configuration;
		if (configuration.Fueling.FreezeForever)
		{
			_priceFreeze = MonoSingleton<ShopController>.Instance.GetItemPrice("price_refuel_freeze_forever");
			FullLabel.Text = Strings.GetString("IDS_REFUEL_VEHICLE_PERMANENT_BUTTON");
			FullText.Text = Strings.GetString("IDS_REFUEL_VEHICLE_PERMANENT_TEXT");
		}
		else
		{
			_priceFreeze = MonoSingleton<ShopController>.Instance.GetItemPrice("price_refuel_freeze");
			FullLabel.Text = Strings.GetString("IDS_REFUEL_VEHICLE_FULL_BUTTON");
			FullText.Text = Strings.GetString("IDS_REFUEL_VEHICLE_FULL_TEXT");
		}
	}

	private void UpdateContent()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle.Fuel.IsFull())
		{
			GallonButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		}
		else
		{
			GallonButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
		}
		if (selectedVehicle.Fuel.FreezeForever)
		{
			if (selectedVehicle.Fuel.IsFrozen())
			{
				FullButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
			}
			else
			{
				FullButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
			}
		}
		FuelMeter.Position = selectedVehicle.Fuel.GetLevelRelative();
	}

	private void UpdateFreeze()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		bool flag = selectedVehicle.Fuel.IsFrozen();
		if (flag && !_frozen)
		{
			_frozen = true;
			FuelMeter.Play("Yellow");
			FuelIcon.PlayAnim("Yellow");
		}
		else if (!flag && _frozen)
		{
			_frozen = false;
			FuelMeter.Play("White");
			FuelIcon.PlayAnim("White");
		}
	}

	private void Update()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		float refuelSeconds = selectedVehicle.Fuel.GetRefuelSeconds();
		int num = Mathf.CeilToInt(refuelSeconds);
		if (_time != num)
		{
			_time = num;
			if (_time == 0)
			{
				RefuelTime.Text = Strings.GetString("IDS_REFUEL_VEHICLE_TIME_FINISHED");
				UpdateContent();
			}
			else
			{
				TimeSpan timeSpan = new TimeSpan(0, 0, _time);
				int num2 = Mathf.FloorToInt((float)timeSpan.TotalHours);
				string @string = Strings.GetString("IDS_REFUEL_VEHICLE_TIME");
				RefuelTime.Text = string.Format(@string, num2, timeSpan.Minutes, timeSpan.Seconds);
			}
			FuelMeter.Position = selectedVehicle.Fuel.GetLevelRelative();
			UpdateFreeze();
		}
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			Close();
		}
	}

	public override void Activate()
	{
		base.Activate();
		GallonPrice.Text = _priceGallon.GetPriceString(ShopItemCurrency.None);
		FullPrice.Text = _priceFreeze.GetPriceString(ShopItemCurrency.None);
		_time = -1;
		UpdateContent();
		Update();
	}
}
