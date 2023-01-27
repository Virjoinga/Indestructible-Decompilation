using Glu.Localization;
using UnityEngine;

public class DialogSelectVehicleItem : MonoBehaviour
{
	public ShopItem Item;

	public SpriteText ItemName;

	public UISingleSprite ItemSprite;

	public UISingleSprite ItemBackground;

	public UISingleSprite ItemPersonIcon;

	public DialogSelectVehicleBuyButton BuyButton;

	public UIProgressBar GradeLevelMeter;

	public bool ZeroPrice;

	private DialogSelectVehicle _dialog;

	private void OnBuyButtonTap()
	{
		if (!_dialog.VehicleBought)
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			_dialog.Buy(this);
		}
	}

	protected virtual void Awake()
	{
		BuyButton.Button.whenToInvoke = POINTER_INFO.INPUT_EVENT.TAP;
		BuyButton.Button.scriptWithMethodToInvoke = this;
		BuyButton.Button.methodToInvoke = "OnBuyButtonTap";
	}

	protected virtual void Start()
	{
	}

	private void SetBodyData(ShopItemBody body)
	{
		SimpleSpriteUtils.ChangeTexture(ItemBackground, body.ItemBackground);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, body.ItemSprite);
		SimpleSpriteUtils.ChangeTexture(ItemPersonIcon, body.PersonIcon);
		ItemName.Text = Strings.GetString(body.nameId);
	}

	public void SetStartCount(int stars)
	{
		GradeLevelMeter.Value = (float)stars / 3f;
	}

	public virtual void SetData(DialogSelectVehicle dialog, ShopItem item, bool zeroPrice)
	{
		Item = item;
		_dialog = dialog;
		ZeroPrice = zeroPrice;
		ShopItemBody bodyData = null;
		if (item.ItemType == ShopItemType.Body)
		{
			bodyData = item as ShopItemBody;
		}
		else if (Item.ItemType == ShopItemType.Vehicle)
		{
			ShopItemVehicle shopItemVehicle = item as ShopItemVehicle;
			bodyData = MonoSingleton<ShopController>.Instance.GetItemBody(shopItemVehicle.BodyId);
		}
		SetBodyData(bodyData);
		UpdateState();
	}

	public virtual void UpdateState()
	{
		BuyButton.ZeroPrice = ZeroPrice;
		BuyButton.UpdateState(Item);
	}

	public virtual void OnDestroy()
	{
		SimpleSpriteUtils.UnloadTexture(ItemSprite);
		SimpleSpriteUtils.UnloadTexture(ItemBackground);
	}
}
