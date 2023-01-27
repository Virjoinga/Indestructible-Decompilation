using System;
using Glu.Localization;
using UnityEngine;

public class PlayerBoostButton : MonoBehaviour
{
	public DialogGameRewards Dialog;

	public SpriteText Label;

	public PackedSprite LoadingRect;

	public SpriteText LoadingLabel;

	public PackedSprite BackgroundMoneySoft;

	public PackedSprite BackgroundExperience;

	public SpriteText LabelMoneySoft;

	public SpriteText LabelExperience;

	private PlayerBoost _boost;

	private int _boostTimeCached = -1;

	private string _backgroundCached = "Normal";

	private IAPShopConfigItem.State _stateCached = IAPShopConfigItem.State.AvailableForPurchase;

	private IAPShopItemBoost _boostItem;

	private UIButton _button;

	private void Awake()
	{
		_button = GetComponent<UIButton>();
		GameConfiguration configuration = MonoSingleton<GameController>.Instance.Configuration;
		string id = ((!configuration.Boosting.BoostForever) ? "iap_boost_1" : "iap_boost_2");
		_boostItem = MonoSingleton<ShopController>.Instance.GetItemBoost(id);
	}

	private void UpdateBoostTimer()
	{
		float seconds = _boost.GetSeconds();
		int num = Mathf.CeilToInt(seconds);
		if (_boostTimeCached != num)
		{
			_boostTimeCached = num;
			if (_boostTimeCached <= 0)
			{
				GetNextBoost();
				return;
			}
			TimeSpan timeSpan = new TimeSpan(0, 0, _boostTimeCached);
			int num2 = Mathf.FloorToInt((float)timeSpan.TotalHours);
			string @string = Strings.GetString("IDS_GAME_REWARDS_DOUBLE_BOUGHT");
			Label.Text = string.Format(@string, "\u001c", "\u001e", num2, timeSpan.Minutes, timeSpan.Seconds);
		}
	}

	public void GetNextBoost()
	{
		_boost = null;
		_boostTimeCached = -1;
		_stateCached = IAPShopConfigItem.State.Undefined;
		MonoSingleton<Player>.Instance.UpdateBoosts();
		string text = "Normal";
		if (MonoSingleton<Player>.Instance.BoughtBoosts.Count > 0)
		{
			_boost = MonoSingleton<Player>.Instance.BoughtBoosts[0];
			if (_boost.BoostForever)
			{
				string @string = Strings.GetString("IDS_GAME_REWARDS_DOUBLE_FOREVER_BOUGHT");
				_button.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
				_button.Text = string.Format(@string, "\u001c", "\u001e");
			}
			else
			{
				_button.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
				UpdateBoostTimer();
			}
			text = "Boost";
			ShowLoading(false);
			if (_boost.BoostForever)
			{
				MonoUtils.SetActive(_button, false);
				UIExpandSprite component = Dialog.GetComponent<UIExpandSprite>();
				component.SetHeight(component.Height - 20f);
				Dialog.ObjectContent.localPosition = new Vector3(0f, -12f, 0f);
				Dialog.CloseButton.GetComponent<UIAlign>().UpdateAlignment();
			}
		}
		else if (_boostItem != null)
		{
			UpdateState();
		}
		if (_backgroundCached != text)
		{
			_backgroundCached = text;
			BackgroundMoneySoft.PlayAnim(_backgroundCached);
			BackgroundExperience.PlayAnim(_backgroundCached);
			if (_backgroundCached == "Normal")
			{
				LabelMoneySoft.Color = Color.white;
				LabelExperience.Color = Color.white;
			}
			else
			{
				LabelMoneySoft.Color = Color.yellow;
				LabelExperience.Color = Color.yellow;
			}
		}
	}

	private void ShowLoading(bool show)
	{
		MonoUtils.SetActive(Label, !show);
		MonoUtils.SetActive(LoadingLabel, show);
		MonoUtils.SetActive(LoadingRect, show);
	}

	private void UpdateState()
	{
		if ((_boost == null || !_boost.BoostForever) && _boostItem != null && _stateCached != IAPShopConfigItem.State.AvailableForPurchase)
		{
			_stateCached = IAPShopConfigItem.State.AvailableForPurchase;
			_button.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
			string id = "IDS_GAME_REWARDS_DOUBLE";
			GameConfiguration configuration = MonoSingleton<GameController>.Instance.Configuration;
			if (configuration.Boosting.BoostForever)
			{
				id = "IDS_GAME_REWARDS_DOUBLE_FOREVER";
			}
			string @string = Localization.GetString(id);
			_button.Text = string.Format(@string, "\u001c", "\u001e");
			ShowLoading(false);
		}
	}

	private void Update()
	{
		if (_boost == null)
		{
			if (_boostItem != null)
			{
				UpdateState();
			}
		}
		else if (!_boost.BoostForever)
		{
			UpdateBoostTimer();
		}
	}

	public void Buy()
	{
		if (_boostItem != null)
		{
			GameAnalytics.EventIAPItemClicked(_boostItem);
			string text = _boostItem.productId.ToLower();
			text = text.Replace(".indesttm.", ".indestructible.");
			AInAppPurchase.RequestPurchase(text, string.Empty);
			Dialogs.IAPBuy(_boostItem);
		}
	}
}
