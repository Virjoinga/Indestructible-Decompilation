using System.Collections.Generic;
using UnityEngine;

public class DialogLevelUp : UIDialog
{
	public SpriteText LevelNumber;

	public SpriteText LevelLabel;

	public GameObject ItemPrefab;

	public Transform ObjectLeftContent;

	public Transform ObjectRightContent;

	public UIScrollList ScrollList;

	private GarageManager _manager;

	private void ReleaseItems()
	{
		int count = ScrollList.Count;
		for (int i = 0; i < count; i++)
		{
			IUIListObject item = ScrollList.GetItem(i);
			DialogLevelUpItem component = item.gameObject.GetComponent<DialogLevelUpItem>();
			component.Release();
		}
	}

	public override void Close()
	{
		ReleaseItems();
		base.Close();
	}

	public void OnItemButtonTap(DialogLevelUpItem item)
	{
		Close();
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		string groupId = item.Item.GroupId;
		if (!string.IsNullOrEmpty(groupId))
		{
			PanelManagerPanel panelManagerPanel = _manager.ActivatePanel("PanelShop");
			ShopScrollList scrollList = (panelManagerPanel as PanelShop).ScrollList;
			if (item.Item.ItemType == ShopItemType.Vehicle)
			{
				scrollList.FeedVehicles(item.Item.id);
			}
			else if (item.Item.ItemType == ShopItemType.Weapon)
			{
				scrollList.FeedWeapons(item.Item.id);
			}
			else if (item.Item.ItemType == ShopItemType.Component)
			{
				scrollList.FeedComponents(item.Item.id, 0);
			}
			else if (item.Item.ItemType == ShopItemType.Armor)
			{
				scrollList.FeedArmors(item.Item.id);
			}
			GameAnalytics.EventDialogLevelUpAction("shop");
		}
	}

	private void OnTalentButtonTap()
	{
		GameAnalytics.EventDialogLevelUpAction("talents");
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		_manager.ActivatePanel("PanelTalents");
		Close();
	}

	private void OnCloseButtonTap()
	{
		GameAnalytics.EventDialogLevelUpAction("close");
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public void CollectItems(int level, string groupId, List<ShopItem> items)
	{
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup(groupId);
		string id = MonoSingleton<Player>.Instance.SelectedVehicle.Vehicle.id;
		foreach (ShopConfigGroup.Reference itemRef in group.itemRefs)
		{
			bool? visible = itemRef.visible;
			if (visible.HasValue && !visible.Value)
			{
				continue;
			}
			ShopItem shopItem = itemRef.item as ShopItem;
			if (shopItem.IsLocked() || shopItem.Lock.GetLevel() < level)
			{
				continue;
			}
			if (shopItem.ItemType == ShopItemType.Armor)
			{
				ShopItemArmor shopItemArmor = shopItem as ShopItemArmor;
				if (id != shopItemArmor.VehicleId)
				{
					continue;
				}
			}
			items.Add(shopItem);
		}
	}

	public void SetData()
	{
		ScrollList.ClearList(false);
		int level = MonoSingleton<Player>.Instance.Level;
		LevelNumber.Text = level.ToString();
		List<ShopItem> list = new List<ShopItem>();
		CollectItems(level, "vehicles", list);
		CollectItems(level, "weapons", list);
		CollectItems(level, "components", list);
		CollectItems(level, "armors", list);
		if (list.Count > 0)
		{
			foreach (ShopItem item in list)
			{
				GameObject gameObject = (GameObject)Object.Instantiate(ItemPrefab);
				DialogLevelUpItem component = gameObject.GetComponent<DialogLevelUpItem>();
				component.SetData(this, item);
				ScrollList.AddItem(gameObject);
			}
			return;
		}
		MonoUtils.DetachAndDestroy(ObjectRightContent);
		Vector3 localPosition = ObjectLeftContent.localPosition;
		ObjectLeftContent.localPosition = new Vector3(0f, 0f, localPosition.z);
		UIExpandSprite component2 = GetComponent<UIExpandSprite>();
		component2.SetWidth(170.7f);
	}

	public override void Activate()
	{
		SetData();
		base.Activate();
		float num = LevelNumber.TotalWidth / 2f;
		if (num > 33.5f)
		{
			num = 33.5f;
		}
		Transform component = LevelNumber.GetComponent<Transform>();
		Vector3 localPosition = component.localPosition;
		component.localPosition = new Vector3(num, localPosition.y, localPosition.z);
		GameObject gameObject = GameObject.Find("GarageManager");
		_manager = gameObject.GetComponent<GarageManager>();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
