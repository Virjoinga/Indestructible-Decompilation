using UnityEngine;

public class DialogIAPShopItem : MonoBehaviour
{
	public SpriteText DiscountText;

	public SpriteText PriceText;

	public SpriteText ValueText;

	public UISingleSprite Sprite;

	public GameObject ObjectContent;

	public GameObject ObjectLoading;

	public GameObject ObjectUnavailable;

	public IAPShopItemSimple Item;

	private IAPShopConfigItem.State _state;

	private DialogIAPShop _dialog;

	private UIButton _button;

	private void Awake()
	{
		_button = GetComponent<UIButton>();
	}

	private void OnButtonTap()
	{
		if (!_dialog.Background)
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			_dialog.Buy(this);
		}
	}

	private void ButtonInputDelegate(ref POINTER_INFO ptr)
	{
		if (!AInAppPurchase.BillingSupported)
		{
			ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
		}
	}

	public void SetData(DialogIAPShop dialog, IAPShopItemSimple item)
	{
		Item = item;
		_dialog = dialog;
		if (AInAppPurchase.BillingSupported)
		{
			_state = IAPShopConfigItem.State.AvailableForPurchase;
			MonoUtils.SetActive(ObjectUnavailable, false);
			MonoUtils.SetActive(ObjectLoading, false);
			MonoUtils.SetActive(ObjectContent, true);
			UpdateData();
		}
		_button.SetInputDelegate(ButtonInputDelegate);
		UpdateState();
	}

	private void UpdateData()
	{
		PriceText.Text = Item.GetPriceAsString();
		ValueText.Text = Item.GetValueString();
		DiscountText.Text = Item.GetDiscountString();
		SimpleSpriteUtils.ChangeTexture(Sprite, Item.ItemSprite);
	}

	private void UpdateState()
	{
		if (Item == null || _state == IAPShopConfigItem.State.AvailableForPurchase || _state == IAPShopConfigItem.State.UnavailableForPurchase)
		{
			return;
		}
		IAPShopConfigItem.State state = Item.state;
		if (_state != state)
		{
			_state = state;
			switch (state)
			{
			case IAPShopConfigItem.State.AvailableForPurchase:
				MonoUtils.SetActive(ObjectUnavailable, false);
				MonoUtils.SetActive(ObjectLoading, false);
				MonoUtils.SetActive(ObjectContent, true);
				UpdateData();
				break;
			case IAPShopConfigItem.State.WaitingForRetrival:
				break;
			case IAPShopConfigItem.State.CheckingAvailability:
				break;
			case IAPShopConfigItem.State.Undefined:
			case IAPShopConfigItem.State.UnavailableForPurchase:
				MonoUtils.SetActive(ObjectUnavailable, true);
				MonoUtils.SetActive(ObjectContent, false);
				MonoUtils.SetActive(ObjectLoading, false);
				break;
			}
		}
	}

	private void Update()
	{
		UpdateState();
	}

	protected virtual void OnDestroy()
	{
		SimpleSpriteUtils.UnloadTexture(Sprite);
	}
}
