using System;
using System.Collections;
using Glu.Localization;
using UnityEngine;

public class ShopItemStandard : MonoBehaviour
{
	public ShopItem Item;

	public SpriteText ItemPersonText;

	public SpriteText ItemName;

	public UISingleSprite ItemSprite;

	public UISingleSprite ItemBackground;

	public ShopItemParameters ItemParameters;

	public SpriteText ItemDescription;

	public ShopItemGradeLevel ItemGradeLevel;

	public ShopItemBuyButton BuyButton;

	public ShopItemBuyButton BuyButtonDual;

	public SpriteText BuyButtonBadgeText;

	public UISingleSprite BundleLineTop;

	public UISingleSprite BundleLineBottom;

	public SpriteText BundleTimer;

	public GameObject ObjectPerson;

	public GameObject ObjectItemName;

	public GameObject ObjectSkinsButton;

	public GameObject ObjectDetailsButton;

	public GameObject ObjectParameters;

	public GameObject ObjectItemDescription;

	public GameObject ObjectBackgroundExpand;

	public GameObject ObjectItemBackground;

	public GameObject ObjectBuyButtonDual;

	public GameObject ObjectItemStripesItem;

	public GameObject ObjectBuyButtonBadge;

	public GameObject ObjectBundleLines;

	public GameObject ObjectBundleTimer;

	public GameObject ObjectGradeLevel;

	public GameObject ObjectCount;

	protected ShopScrollList _shop;

	protected bool _equipBoughtOnBuyButton = true;

	protected bool _equipBoughtOnDualBuyButton;

	protected virtual void OnSkinsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		_shop.ShowSkins(this);
	}

	protected virtual void ButtonsResetDelegate(ref POINTER_INFO ptr)
	{
		POINTER_INFO.INPUT_EVENT evt = ptr.evt;
		if (evt == POINTER_INFO.INPUT_EVENT.DRAG)
		{
			UIButton uIButton = ptr.targetObj as UIButton;
			if (uIButton != null)
			{
				uIButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
				ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
			}
		}
	}

	protected virtual void OnDetailsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (Item.ItemType == ShopItemType.Vehicle)
		{
			Dialogs.ShopVehicleDetails(null, this);
		}
		else if (Item.ItemType == ShopItemType.Body)
		{
			Dialogs.ShopBodyDetails(null, this);
		}
		else if (Item.ItemType == ShopItemType.Weapon)
		{
			Dialogs.ShopWeaponDetails(null, this);
		}
		else if (Item.ItemType == ShopItemType.Bundle)
		{
			PanelManager owner = _shop.ParentPanelShop.Owner;
			PanelManagerPanel panelManagerPanel = owner.ActivateDirectly("PanelBundle");
			PanelBundle panelBundle = panelManagerPanel as PanelBundle;
			panelBundle.SetData(Item as ShopItemBundle, true, false);
		}
		GameAnalytics.EventItemDetailsDialog(Item);
	}

	public bool Equip()
	{
		return _shop.EquipItem(this);
	}

	public bool IsBought()
	{
		return MonoSingleton<Player>.Instance.IsBought(Item);
	}

	public bool Buy(ShopItemCurrency currency)
	{
		return _shop.BuyItem(this, currency);
	}

	private void OnBuyButtonTap()
	{
		if (IsBought() && _equipBoughtOnBuyButton)
		{
			Equip();
		}
		else
		{
			Buy(BuyButton.Currency);
		}
	}

	private void OnBuyButtonTapDisabled()
	{
		if (Item.IsLocked(BuyButton.Currency))
		{
			int influencePoints = Item.Lock.GetInfluencePoints();
			if (influencePoints > 0)
			{
				Dialogs.InfluencePointsLocked(influencePoints);
			}
		}
	}

	private void OnBuyButtonDualTap()
	{
		if (IsBought() && _equipBoughtOnDualBuyButton)
		{
			Equip();
		}
		else
		{
			Buy(BuyButtonDual.Currency);
		}
	}

	protected virtual void Awake()
	{
		BuyButton.Button.whenToInvoke = POINTER_INFO.INPUT_EVENT.TAP;
		BuyButton.Button.scriptWithMethodToInvoke = this;
		BuyButton.Button.methodToInvoke = "OnBuyButtonTap";
		BuyButtonDual.Button.whenToInvoke = POINTER_INFO.INPUT_EVENT.TAP;
		BuyButtonDual.Button.scriptWithMethodToInvoke = this;
		BuyButtonDual.Button.methodToInvoke = "OnBuyButtonDualTap";
		UIButton component = ObjectSkinsButton.GetComponent<UIButton>();
		component.SetInputDelegate(ButtonsResetDelegate);
		component.whenToInvoke = POINTER_INFO.INPUT_EVENT.TAP;
		component.scriptWithMethodToInvoke = this;
		component.methodToInvoke = "OnSkinsButtonTap";
		UIButton component2 = ObjectDetailsButton.GetComponent<UIButton>();
		component2.SetInputDelegate(ButtonsResetDelegate);
		component2.whenToInvoke = POINTER_INFO.INPUT_EVENT.TAP;
		component2.scriptWithMethodToInvoke = this;
		component2.methodToInvoke = "OnDetailsButtonTap";
	}

	protected virtual void Start()
	{
		BundleLineTop.SetSize(108.4375f, 5f);
		BundleLineBottom.SetSize(108.4375f, 5f);
	}

	public static void Deactivate(GameObject o)
	{
		if (!(o == null))
		{
			MonoUtils.SetActive(o, false);
		}
	}

	public static void Activate(GameObject o)
	{
		if (!(o == null))
		{
			MonoUtils.SetActive(o, true);
		}
	}

	protected virtual void SetItemSpritePlacement(UISingleSprite sprite, float width, float height, float y)
	{
		ItemSprite.SetSize(width, height);
		Transform component = ItemSprite.GetComponent<Transform>();
		Vector3 localPosition = component.localPosition;
		localPosition.y = y;
		component.localPosition = localPosition;
	}

	private void SetDescriptionPlacement(float charSize, float y)
	{
		if (ItemDescription.characterSize != charSize)
		{
			ItemDescription.SetCharacterSize(charSize);
		}
		Transform component = ItemDescription.GetComponent<Transform>();
		Vector3 localPosition = component.localPosition;
		localPosition.y = y;
		component.localPosition = localPosition;
	}

	public virtual void SetBodyData(ShopItemBody body)
	{
		Deactivate(ObjectItemName);
		Deactivate(ObjectParameters);
		Deactivate(ObjectSkinsButton);
		Deactivate(ObjectItemDescription);
		Deactivate(ObjectBackgroundExpand);
		Deactivate(ObjectItemStripesItem);
		Deactivate(ObjectBuyButtonBadge);
		Deactivate(ObjectBundleLines);
		Deactivate(ObjectBundleTimer);
		Deactivate(ObjectGradeLevel);
		Deactivate(ObjectCount);
		ItemPersonText.Text = Strings.GetString(Item.nameId);
		SimpleSpriteUtils.ChangeTexture(ItemBackground, body.ItemBackground);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 109.375f, 93.75f, -11.40625f);
	}

	public virtual void SetBundleData(ShopItemBundle bundle)
	{
		Deactivate(ObjectItemName);
		Deactivate(ObjectParameters);
		Deactivate(ObjectSkinsButton);
		Deactivate(ObjectItemStripesItem);
		Deactivate(ObjectBackgroundExpand);
		Deactivate(ObjectGradeLevel);
		Deactivate(ObjectCount);
		Activate(ObjectPerson);
		Activate(ObjectItemBackground);
		Activate(ObjectItemDescription);
		Activate(ObjectBuyButtonBadge);
		Activate(ObjectBundleLines);
		Activate(ObjectBundleTimer);
		ItemPersonText.Text = Strings.GetString(bundle.nameId);
		ItemDescription.Text = Strings.GetString(bundle.DescriptionId);
		SetDescriptionPlacement(4.25f, -38f);
		SimpleSpriteUtils.ChangeTexture(ItemBackground, bundle.ItemBackground);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, bundle.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 104.375f, 76.25f, 0f);
		PlayerBundle playerBundle = MonoSingleton<Player>.Instance.GetPlayerBundle(bundle.id);
		string @string = Strings.GetString("IDS_SHOP_ITEM_SAVE_BADGE");
		@string = string.Format(@string, playerBundle.GetSaveHardPriceString());
		BuyButtonBadgeText.Text = @string.Replace(" ", string.Empty);
		BuyButtonBadgeText.SetCharacterSize(5f);
		StartCoroutine(BundleTimerRoutine());
	}

	private IEnumerator BundleTimerRoutine()
	{
		PlayerBundle bundle = MonoSingleton<Player>.Instance.GetPlayerBundle(Item.id);
		if (bundle == null)
		{
			yield break;
		}
		int seconds = -1;
		while (true)
		{
			long ticks = DateTime.UtcNow.Ticks;
			float s = bundle.GetSeconds(ticks);
			int time = (int)s;
			if (time != seconds)
			{
				seconds = time;
				TimeSpan t = new TimeSpan(0, 0, seconds);
				int hours = Mathf.FloorToInt((float)t.TotalHours);
				string text = Strings.GetString("IDS_SHOP_ITEM_BUNDLE_TIMER_FORMAT");
				BundleTimer.Text = string.Format(text, hours, t.Minutes, t.Seconds);
			}
			yield return null;
		}
	}

	public virtual void SetVehicleData(ShopItemVehicle vehicle)
	{
		Deactivate(ObjectItemName);
		Deactivate(ObjectParameters);
		Deactivate(ObjectItemDescription);
		Deactivate(ObjectBackgroundExpand);
		Deactivate(ObjectItemStripesItem);
		Deactivate(ObjectBuyButtonBadge);
		Deactivate(ObjectBundleLines);
		Deactivate(ObjectBundleTimer);
		Deactivate(ObjectCount);
		Activate(ObjectItemBackground);
		Activate(ObjectSkinsButton);
		Activate(ObjectPerson);
		GameConfiguration configuration = MonoSingleton<GameController>.Instance.Configuration;
		bool flag = configuration.VehicleGrade.Enabled;
		MonoUtils.SetActive(ObjectGradeLevel, flag);
		if (flag)
		{
			ItemGradeLevel.SetData(vehicle);
		}
		ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(vehicle.BodyId);
		ItemPersonText.Text = Strings.GetString(itemBody.nameId);
		SimpleSpriteUtils.ChangeTexture(ItemBackground, itemBody.ItemBackground);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, itemBody.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 109.375f, 93.75f, -11.40625f);
	}

	public virtual void SetWeaponData(ShopItemWeapon weapon)
	{
		Deactivate(ObjectPerson);
		Deactivate(ObjectSkinsButton);
		Deactivate(ObjectItemDescription);
		Deactivate(ObjectItemBackground);
		Deactivate(ObjectBuyButtonBadge);
		Deactivate(ObjectBundleLines);
		Deactivate(ObjectBundleTimer);
		Deactivate(ObjectGradeLevel);
		Deactivate(ObjectCount);
		ItemName.Text = Strings.GetString(Item.nameId);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 109.375f, 93.75f, 2f);
	}

	public virtual void SetArmorData(ShopItemArmor armor)
	{
		Deactivate(ObjectPerson);
		Deactivate(ObjectSkinsButton);
		Deactivate(ObjectDetailsButton);
		Deactivate(ObjectItemBackground);
		Deactivate(ObjectBuyButtonBadge);
		Deactivate(ObjectBundleLines);
		Deactivate(ObjectBundleTimer);
		Deactivate(ObjectGradeLevel);
		Deactivate(ObjectCount);
		ItemName.Text = Strings.GetString(Item.nameId);
		string @string = Strings.GetString(Item.DescriptionId);
		ItemDescription.Text = string.Format(@string, armor.Damage);
		SetDescriptionPlacement(5f, -33f);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 109.375f, 93.75f, 10.5f);
	}

	public virtual void SetComponentData(ShopItemComponent component)
	{
		Deactivate(ObjectPerson);
		Deactivate(ObjectSkinsButton);
		Deactivate(ObjectDetailsButton);
		Deactivate(ObjectItemBackground);
		Deactivate(ObjectBuyButtonBadge);
		Deactivate(ObjectBundleLines);
		Deactivate(ObjectBundleTimer);
		Deactivate(ObjectGradeLevel);
		Deactivate(ObjectCount);
		ItemName.Text = Strings.GetString(Item.nameId);
		ItemDescription.Text = Strings.GetString(Item.DescriptionId);
		SetDescriptionPlacement(5f, -33f);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 109.375f, 93.75f, 10.5f);
	}

	public virtual void SetAmmunitionData(ShopItemAmmunition ammunition)
	{
		Deactivate(ObjectPerson);
		Deactivate(ObjectParameters);
		Deactivate(ObjectSkinsButton);
		Deactivate(ObjectDetailsButton);
		Deactivate(ObjectItemBackground);
		Deactivate(ObjectBuyButtonBadge);
		Deactivate(ObjectBundleLines);
		Deactivate(ObjectBundleTimer);
		Deactivate(ObjectGradeLevel);
		_equipBoughtOnBuyButton = true;
		_equipBoughtOnDualBuyButton = false;
		ItemName.Text = Strings.GetString(Item.nameId);
		ItemDescription.Text = Strings.GetString(Item.DescriptionId);
		SetDescriptionPlacement(4.5f, -34f);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 109.375f, 93.75f, 3.5f);
		UpdateCounter();
	}

	public virtual void SetData(ShopScrollList shop, ShopItem item)
	{
		Item = item;
		_shop = shop;
		_equipBoughtOnBuyButton = true;
		_equipBoughtOnDualBuyButton = false;
		StopAllCoroutines();
		if (item.ItemType == ShopItemType.Body)
		{
			SetBodyData(item as ShopItemBody);
		}
		else if (Item.ItemType == ShopItemType.Vehicle)
		{
			SetVehicleData(item as ShopItemVehicle);
		}
		else if (Item.ItemType == ShopItemType.Weapon)
		{
			SetWeaponData(item as ShopItemWeapon);
		}
		else if (Item.ItemType == ShopItemType.Armor)
		{
			SetArmorData(item as ShopItemArmor);
		}
		else if (Item.ItemType == ShopItemType.Component)
		{
			SetComponentData(item as ShopItemComponent);
		}
		else if (Item.ItemType == ShopItemType.Bundle)
		{
			SetBundleData(item as ShopItemBundle);
		}
		else if (Item.ItemType == ShopItemType.Ammunition)
		{
			SetAmmunitionData(item as ShopItemAmmunition);
		}
		UpdateParameters();
		UpdateState();
	}

	public virtual void UpdateState()
	{
		if (Item.IsDualPriced() && Item.ItemType != ShopItemType.Ammunition)
		{
			BuyButton.UpdateState(Item, ShopItemBuyButton.ForceUpdate.None);
			bool flag = !MonoSingleton<Player>.Instance.IsBought(Item);
			MonoUtils.SetActive(BuyButtonDual, flag);
			BuyButtonDual.UpdateState(Item, ShopItemBuyButton.ForceUpdate.BuySoft);
		}
		else if (Item.ItemType == ShopItemType.Ammunition)
		{
			bool flag2 = ((ShopItemAmmunition)Item).IsMatchedWeapon(MonoSingleton<Player>.Instance.SelectedVehicle.Weapon);
			bool flag3 = MonoSingleton<Player>.Instance.IsBought(Item);
			MonoUtils.SetActive(BuyButtonDual, true);
			BuyButton.UpdateState(Item, (!flag2 || !flag3) ? ShopItemBuyButton.ForceUpdate.EquipDisabled : ShopItemBuyButton.ForceUpdate.Equip);
			BuyButtonDual.UpdateState(Item, ShopItemBuyButton.ForceUpdate.Buy);
			UpdateCounter();
		}
		else
		{
			BuyButton.UpdateState(Item, ShopItemBuyButton.ForceUpdate.None);
			MonoUtils.SetActive(BuyButtonDual, false);
		}
		if (!MonoSingleton<Player>.Instance.IsBought(Item))
		{
			if (Item.overrideTimeLeft.Ticks > 0)
			{
				MonoUtils.SetActive(ObjectBuyButtonBadge, true);
				BuyButtonBadgeText.SetCharacterSize(6f);
				BuyButtonBadgeText.Text = Item.SaleText;
				MonoUtils.SetActive(BuyButton.ObjectLockText, true);
				MonoUtils.SetActive(BuyButton.ObjectOldPriceCross, true);
				BuyButton.Button.Text = string.Empty;
				BuyButton.LockReason.Text = Item.GetOldPriceString();
				BuyButton.LockText.Text = Item.GetPriceString(ShopItemCurrency.None);
			}
			else
			{
				MonoUtils.SetActive(BuyButton.ObjectOldPriceCross, false);
				if (Item.ItemType != ShopItemType.Bundle)
				{
					MonoUtils.SetActive(ObjectBuyButtonBadge, false);
				}
			}
		}
		else
		{
			MonoUtils.SetActive(BuyButton.ObjectOldPriceCross, false);
			MonoUtils.SetActive(ObjectBuyButtonBadge, false);
		}
	}

	public virtual void UpdateParameters()
	{
		ItemParameters.SetData(Item, _shop.ParentPanelShop.SlotIndex);
	}

	public virtual void UpdateCounter()
	{
		if (!(ObjectCount == null))
		{
			string text = string.Empty;
			if (Item.GetCount() >= 0)
			{
				text = Item.GetCount().ToString();
			}
			if (Item.GetPackCount() < 0 || string.IsNullOrEmpty(text))
			{
				MonoUtils.SetActive(ObjectCount, false);
				return;
			}
			MonoUtils.SetActive(ObjectCount, true);
			ShopItemParameter component = ObjectCount.GetComponent<ShopItemParameter>();
			Transform component2 = ObjectCount.GetComponent<Transform>();
			Vector3 localPosition = component2.localPosition;
			component2.localPosition = localPosition;
			component.Value.SetColor(Color.white);
			component.Value.Text = string.Format(Strings.GetString("IDS_AMMUNITION_OWNED"), text);
			float internalWidth = component.Value.TotalWidth + component.WidthOffset;
			component.Background.SetInternalWidth(internalWidth);
		}
	}

	public virtual void ReleaseData()
	{
		SimpleSpriteUtils.UnloadTexture(ItemSprite);
		SimpleSpriteUtils.UnloadTexture(ItemBackground);
	}
}
