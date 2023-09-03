using Glu.Localization;
using UnityEngine;

public class DialogShopVehicleDetails : UIDialog
{
	public DialogShopVehiclePerson BodyPerson;

	public DialogShopVehicleAbilities BodyAbilities;

	public ShopItemBuyButton BuyButton;

	public UIButton RefuelButton;

	public UIButton CustomizeButton;

	public Transform OffensePointer;

	public float OffensePointerRange;

	private ShopItemVehicle _vehicle;

	private ShopItemStandard _shopItem;

	private GarageItemStandard _garageItem;

	private bool _lowestDialogLevel = true;

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void OnCustomizeButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Object @object = Object.FindObjectOfType(typeof(GarageManager));
		GarageManager garageManager = @object as GarageManager;
		if (garageManager != null)
		{
			garageManager.ActivatePanel("PanelCustomization");
			GameAnalytics.EventCustomizeButtonTap("details");
		}
		Close();
	}

	private void OnRefuelButtonTap()
	{
		_lowestDialogLevel = false;
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Dialogs.RefuelVehicle();
		Close();
	}

	private void OnBuyButtonTap()
	{
		_lowestDialogLevel = false;
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (_shopItem.IsBought())
		{
			_shopItem.Equip();
			Close();
		}
		else if (_shopItem.Buy(BuyButton.Currency))
		{
			Close();
		}
	}

	public void SetData(ShopItemVehicle vehicle, ShopItemStandard item)
	{
		_vehicle = vehicle;
		_shopItem = item;
	}

	public void SetData(GarageItemStandard item)
	{
		_garageItem = item;
	}

	public override void Activate()
	{
		BurstlyController.HideAd();
		base.Activate();
		ShopItemVehicle shopItemVehicle = null;
		ShopItemBody data = null;
		if (_shopItem != null)
		{
			shopItemVehicle = _shopItem.Item as ShopItemVehicle;
			data = MonoSingleton<ShopController>.Instance.GetItemBody(shopItemVehicle.BodyId);
		}
		else if (_garageItem != null)
		{
			shopItemVehicle = _garageItem.Item.Vehicle;
			data = _garageItem.Item.Body;
		}
		else if (_vehicle != null)
		{
			shopItemVehicle = _vehicle;
			data = MonoSingleton<ShopController>.Instance.GetItemBody(shopItemVehicle.BodyId);
		}
		BuyButton.UpdateState(shopItemVehicle, ShopItemBuyButton.ForceUpdate.None);
		BodyAbilities.SetData(shopItemVehicle);
		BodyPerson.SetData(data);
		ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(shopItemVehicle.BodyId);
		BodyPerson.PersonText.Text = Strings.GetString(itemBody.DescriptionId);
		if (_vehicle != null)
		{
			MonoUtils.DetachAndDestroy(BuyButton);
			MonoUtils.DetachAndDestroy(RefuelButton);
			MonoUtils.DetachAndDestroy(CustomizeButton);
		}
		else if (MonoSingleton<Player>.Instance.IsBought(shopItemVehicle))
		{
			MonoUtils.DetachAndDestroy(BuyButton);
		}
		else
		{
			MonoUtils.DetachAndDestroy(RefuelButton);
			MonoUtils.DetachAndDestroy(CustomizeButton);
		}
		float num = (float)shopItemVehicle.GetOffense() / 6f;
		Vector3 localPosition = OffensePointer.localPosition;
		localPosition.x = num * OffensePointerRange;
		OffensePointer.localPosition = localPosition;
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			if (_lowestDialogLevel)
			{
				OnCloseButtonTap();
			}
			else
			{
				_lowestDialogLevel = true;
			}
		}
	}
}
