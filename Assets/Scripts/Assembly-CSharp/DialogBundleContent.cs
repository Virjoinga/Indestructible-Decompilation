using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class DialogBundleContent : UIDialog
{
	public UIPageScrollList ScrollList;

	public UIExpandSprite Background;

	public UIBorderSprite NameBackground;

	public SpriteText NameText;

	public SpriteText NewPrice;

	public SpriteText OldPrice;

	public SpriteText TimerText;

	public SpriteText CloseButtonText;

	public string CloseButtonLoco = string.Empty;

	public string CloseButtonBossLoco = string.Empty;

	public SpriteText BundleContentText;

	public string StdBundleContentLoco = string.Empty;

	public GameObject Boss;

	public GameObject Timer;

	private PlayerBundle _bundle;

	private int _seconds = -1;

	private bool _bossBundle;

	private List<ShopItem> _items = new List<ShopItem>();

	protected override void Awake()
	{
		base.Awake();
		Vector2 screenSize = UITools.GetScreenSize();
		Vector2 viewableArea = ScrollList.viewableArea;
		ScrollList.viewableArea = new Vector2(screenSize.x - 70f, viewableArea.y);
		Transform component = ScrollList.GetComponent<Transform>();
		Vector3 localPosition = component.localPosition;
		localPosition.x -= 35f;
		component.localPosition = localPosition;
	}

	private void OnBuyButtonTap()
	{
		if (MonoSingleton<Player>.Instance.Buy(_bundle.Item))
		{
			MonoSingleton<Player>.Instance.Achievements.UpdateGarage();
			MonoSingleton<Player>.Instance.Save();
		}
	}

	private void OnCloseButtonTap()
	{
		Close();
	}

	public override void Close()
	{
		ScrollList.FreeItems();
		UnityEngine.Object @object = UnityEngine.Object.FindObjectOfType(typeof(GarageManager));
		GarageManager garageManager = @object as GarageManager;
		if (_bossBundle)
		{
			PanelManagerPanel panelManagerPanel = garageManager.ActivateDirectly("PanelGarage");
			return;
		}
		PanelManagerPanel panelManagerPanel2 = garageManager.ActivateDirectly("PanelShop");
		PanelShop panelShop = panelManagerPanel2 as PanelShop;
		panelShop.PreviousPanel = "PanelGarage";
		MonoUtils.SetActive(panelShop.PowerMeter, false);
		panelShop.ScrollList.FeedVehicles(string.Empty);
	}

	public override void Activate()
	{
		MonoUtils.SetActive(this, true);
	}

	private void EventFillData(IUIObject container, int index)
	{
		BundleItemStandard component = container.gameObject.GetComponent<BundleItemStandard>();
		component.SetData(null, _items[index]);
	}

	private void EventFreeData(IUIObject container, int index)
	{
		BundleItemStandard component = container.gameObject.GetComponent<BundleItemStandard>();
		component.ReleaseData();
	}

	public void SetData(ShopItemBundle bundle, bool checkPlayer, bool bossBundle)
	{
		if (checkPlayer)
		{
			_bundle = MonoSingleton<Player>.Instance.GetPlayerBundle(bundle.id);
		}
		else
		{
			_bundle = new PlayerBundle();
			_bundle.Activate(bundle, 0L);
		}
		string @string = Strings.GetString(bundle.nameId);
		NameText.Text = @string.Replace("\n", " ");
		NameBackground.SetInternalWidth(NameText.TotalWidth);
		ScrollList.ScrollListTo(0f);
		ScrollList.FreeItems();
		PlayerBundle.FillingFlags filling;
		_items = _bundle.GetItems(out filling);
		ScrollList.EventFillData = EventFillData;
		ScrollList.EventFreeData = EventFreeData;
		ScrollList.Init(_items.Count);
		ScrollList.ScrollToItemIndex(0, 1f);
		Vector2 screenSize = UITools.GetScreenSize();
		Background.SetInternalWidth(screenSize.x);
		NewPrice.Text = _bundle.Item.GetPriceString(ShopItemCurrency.None);
		OldPrice.Text = _bundle.GetRegularHardPriceString();
		_bossBundle = bossBundle;
		if (_bossBundle)
		{
			MonoUtils.SetActive(Timer, false);
			MonoUtils.SetActive(Boss, true);
			CloseButtonText.Text = Strings.GetString(CloseButtonBossLoco);
			BundleContentText.Text = Strings.GetString(StdBundleContentLoco);
		}
		else
		{
			MonoUtils.SetActive(Timer, true);
			MonoUtils.SetActive(Boss, false);
			CloseButtonText.Text = Strings.GetString(CloseButtonLoco);
			BundleContentText.Text = Strings.GetString(StdBundleContentLoco);
		}
	}

	private void Update()
	{
		if (!_bossBundle)
		{
			long ticks = DateTime.UtcNow.Ticks;
			float seconds = _bundle.GetSeconds(ticks);
			int num = (int)seconds;
			if (num != _seconds)
			{
				_seconds = num;
				TimeSpan timeSpan = new TimeSpan(0, 0, _seconds);
				int num2 = Mathf.FloorToInt((float)timeSpan.TotalHours);
				string @string = Strings.GetString("IDS_BUNDLE_CONTENT_TIMER_FORMAT");
				TimerText.Text = string.Format(@string, num2, timeSpan.Minutes, timeSpan.Seconds);
			}
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				OnCloseButtonTap();
			}
		}
	}
}
