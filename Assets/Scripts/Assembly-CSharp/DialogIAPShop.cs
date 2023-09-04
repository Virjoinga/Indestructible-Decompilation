using UnityEngine;

public class DialogIAPShop : UIDialog
{
	public GameObject ShopIAPItemPrefab;

	public GameObject ShopIAPSpecialPrefab;

	public string GroupId = string.Empty;

	public GameObject[] DummyItems;

	private bool _background;

	public bool Background
	{
		get
		{
			return _background;
		}
	}

	private void OnCloseButtonTap()
	{
		if (!_background)
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			Close();
		}
	}

	private void OnGetFreeGoldTap()
	{
		if (!_background)
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			GameAnalytics.EventTapJoyButtonPressed("DialogIAPShop");
			AAds.Tapjoy.Launch(false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
	}

	public override void Activate()
	{
		base.Activate();
		FeedData(GroupId);
	}

	public override void Close()
	{
		base.Close();
	}

	public void Buy(DialogIAPShopItem item)
	{
		if (AInAppPurchase.BillingSupported)
		{
			GameAnalytics.EventIAPItemClicked(item.Item);
			string text = item.Item.productId.ToLower();
			text = text.Replace(".indesttm.", ".indestructible.");
			AInAppPurchase.RequestPurchase(text, string.Empty);
			if (GameConstants.BuildType == "google")
			{
				Dialogs.IAPBuy(item.Item);
			}
		}
	}

	private void FeedData(string groupId)
	{
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup(groupId);
		int num = Mathf.Min(DummyItems.Length, group.itemRefs.Count);
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = null;
			gameObject = ((i != num - 1) ? ((GameObject)Object.Instantiate(ShopIAPItemPrefab)) : ((GameObject)Object.Instantiate(ShopIAPSpecialPrefab)));
			Transform component = DummyItems[i].GetComponent<Transform>();
			Transform component2 = gameObject.GetComponent<Transform>();
			component2.position = component.position;
			component2.parent = _transform;
			DialogIAPShopItem component3 = gameObject.GetComponent<DialogIAPShopItem>();
			IAPShopItemSimple item = group.itemRefs[i].item as IAPShopItemSimple;
			component3.SetData(this, item);
		}
	}

	private void SetBackground(bool background)
	{
		if (_background != background)
		{
			_background = background;
			MonoSingleton<SettingsController>.Instance.MuteSound(background);
			MonoSingleton<SettingsController>.Instance.MuteMusic(background);
			if (!background)
			{
				GamePlayHaven.Placement("tj_closed");
			}
		}
	}

	private void Update()
	{
		if (MonoSingleton<GameController>.Instance.BackKeyReleased())
		{
			OnCloseButtonTap();
		}
	}
}
