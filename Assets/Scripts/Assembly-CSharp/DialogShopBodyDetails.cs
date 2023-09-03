using UnityEngine;

public class DialogShopBodyDetails : UIDialog
{
	public DialogShopVehiclePerson BodyPerson;

	public ShopItemBuyButton BuyButton;

	private ShopItemBody _body;

	private ShopItemStandard _item;

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void OnBuyButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (_item.IsBought())
		{
			_item.Equip();
			Close();
		}
		else if (_item.Buy(BuyButton.Currency))
		{
			Close();
		}
	}

	public void SetData(ShopItemBody body, ShopItemStandard item)
	{
		_body = body;
		_item = item;
	}

	public override void Activate()
	{
		base.Activate();
		ShopItemBody shopItemBody = null;
		if (_item != null)
		{
			shopItemBody = _item.Item as ShopItemBody;
		}
		else if (_body != null)
		{
			shopItemBody = _body;
		}
		BuyButton.UpdateState(shopItemBody, ShopItemBuyButton.ForceUpdate.None);
		BodyPerson.SetData(shopItemBody);
		if (_item == null)
		{
			MonoUtils.DetachAndDestroy(BuyButton);
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
