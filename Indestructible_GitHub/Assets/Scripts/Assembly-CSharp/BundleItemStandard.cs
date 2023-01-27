using Glu.Localization;
using UnityEngine;

public class BundleItemStandard : MonoBehaviour
{
	public ShopItem Item;

	public SpriteText ItemPersonText;

	public SpriteText ItemName;

	public UISingleSprite ItemSprite;

	public UISingleSprite ItemBackground;

	public SpriteText ItemDescription;

	public GameObject ObjectPerson;

	public GameObject ObjectItemName;

	public GameObject ObjectDetailsButton;

	public GameObject ObjectItemDescription;

	public GameObject ObjectBackgroundExpand;

	public GameObject ObjectItemBackground;

	public GameObject ObjectItemStripesItem;

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
			ShopItemVehicle vehicle = Item as ShopItemVehicle;
			Dialogs.ShopVehicleDetails(vehicle, null);
		}
		else if (Item.ItemType == ShopItemType.Body)
		{
			ShopItemBody body = Item as ShopItemBody;
			Dialogs.ShopBodyDetails(body, null);
		}
		else if (Item.ItemType == ShopItemType.Weapon)
		{
			ShopItemWeapon weapon = Item as ShopItemWeapon;
			Dialogs.ShopWeaponDetails(weapon, null);
		}
		GameAnalytics.EventItemDetailsDialog(Item);
	}

	protected virtual void Awake()
	{
		UIButton component = ObjectDetailsButton.GetComponent<UIButton>();
		component.SetInputDelegate(ButtonsResetDelegate);
		component.whenToInvoke = POINTER_INFO.INPUT_EVENT.TAP;
		component.scriptWithMethodToInvoke = this;
		component.methodToInvoke = "OnDetailsButtonTap";
	}

	protected virtual void Start()
	{
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

	public virtual void SetBodyData(ShopItemBody body)
	{
		Deactivate(ObjectItemName);
		Deactivate(ObjectItemDescription);
		Deactivate(ObjectBackgroundExpand);
		Deactivate(ObjectItemStripesItem);
		Activate(ObjectPerson);
		Activate(ObjectDetailsButton);
		Activate(ObjectItemBackground);
		ItemPersonText.Text = Strings.GetString(Item.nameId);
		SimpleSpriteUtils.ChangeTexture(ItemBackground, body.ItemBackground);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 98f, 84f, -11.40625f);
	}

	public virtual void SetVehicleData(ShopItemVehicle vehicle)
	{
		Deactivate(ObjectItemName);
		Deactivate(ObjectItemDescription);
		Deactivate(ObjectBackgroundExpand);
		Deactivate(ObjectItemStripesItem);
		Activate(ObjectPerson);
		Activate(ObjectDetailsButton);
		Activate(ObjectItemBackground);
		ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(vehicle.BodyId);
		ItemPersonText.Text = Strings.GetString(itemBody.nameId);
		SimpleSpriteUtils.ChangeTexture(ItemBackground, itemBody.ItemBackground);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, itemBody.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 98f, 84f, -11.40625f);
	}

	public virtual void SetWeaponData(ShopItemWeapon weapon)
	{
		Deactivate(ObjectPerson);
		Deactivate(ObjectItemDescription);
		Deactivate(ObjectItemBackground);
		Activate(ObjectItemName);
		Activate(ObjectDetailsButton);
		Activate(ObjectBackgroundExpand);
		Activate(ObjectItemStripesItem);
		ItemName.Text = Strings.GetString(Item.nameId);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 98f, 84f, 0f);
	}

	public virtual void SetArmorData(ShopItemArmor armor)
	{
		Deactivate(ObjectPerson);
		Deactivate(ObjectDetailsButton);
		Deactivate(ObjectItemBackground);
		Activate(ObjectItemName);
		Activate(ObjectItemDescription);
		Activate(ObjectBackgroundExpand);
		Activate(ObjectItemStripesItem);
		ItemName.Text = Strings.GetString(Item.nameId);
		string @string = Strings.GetString(Item.DescriptionId);
		ItemDescription.Text = string.Format(@string, armor.Damage);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 98f, 84f, 0f);
	}

	public virtual void SetComponentData(ShopItemComponent component)
	{
		Deactivate(ObjectPerson);
		Deactivate(ObjectDetailsButton);
		Deactivate(ObjectItemBackground);
		Activate(ObjectItemName);
		Activate(ObjectItemDescription);
		Activate(ObjectBackgroundExpand);
		Activate(ObjectItemStripesItem);
		ItemName.Text = Strings.GetString(Item.nameId);
		ItemDescription.Text = Strings.GetString(Item.DescriptionId);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.ItemSprite);
		SetItemSpritePlacement(ItemSprite, 98f, 84f, 0f);
	}

	public virtual void SetData(ShopScrollList shop, ShopItem item)
	{
		Item = item;
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
	}

	public virtual void ReleaseData()
	{
		SimpleSpriteUtils.UnloadTexture(ItemSprite);
		SimpleSpriteUtils.UnloadTexture(ItemBackground);
	}
}
