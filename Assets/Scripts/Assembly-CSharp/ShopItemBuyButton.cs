using Glu.Localization;
using UnityEngine;

public class ShopItemBuyButton : MonoBehaviour
{
	public enum ForceUpdate
	{
		None = 0,
		EquipDisabled = 1,
		Equip = 2,
		Buy = 3,
		BuySoft = 4,
		BuyHard = 5
	}

	public UIStateToggleBtn Button;

	public GameObject ObjectLockText;

	public SpriteText LockText;

	public SpriteText LockReason;

	public ShopItemCurrency Currency;

	public GameObject ObjectOldPriceCross;

	private string _pressState;

	private void SetPressed(bool pressed)
	{
		if (_pressState == "Disabled")
		{
			return;
		}
		if (pressed)
		{
			if (_pressState == string.Empty)
			{
				_pressState = Button.StateName;
				if (_pressState != "Blue")
				{
					Button.SetToggleState(_pressState + "Pressed", true);
				}
			}
		}
		else if (_pressState != string.Empty)
		{
			Button.SetToggleState(_pressState, true);
			_pressState = string.Empty;
		}
	}

	private void SetState(string state)
	{
		if (state == "Disabled")
		{
			Button.SetState(Button.states.Length - 1);
			_pressState = state;
		}
		else
		{
			Button.SetToggleState(state, true);
			_pressState = string.Empty;
		}
	}

	private void InvokeButtonTap()
	{
		if (Button.scriptWithMethodToInvoke != null)
		{
			Button.scriptWithMethodToInvoke.Invoke(Button.methodToInvoke, Button.delay);
		}
	}

	private void InvokeButtonTapDisabled()
	{
		if (Button.scriptWithMethodToInvoke != null)
		{
			string methodName = Button.methodToInvoke + "Disabled";
			Button.scriptWithMethodToInvoke.Invoke(methodName, Button.delay);
		}
	}

	private void InputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			if (_pressState == "Disabled")
			{
				InvokeButtonTapDisabled();
			}
			else if (_pressState == "Green" || _pressState == "Red")
			{
				InvokeButtonTap();
			}
			SetPressed(false);
			break;
		case POINTER_INFO.INPUT_EVENT.PRESS:
			SetPressed(true);
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
		case POINTER_INFO.INPUT_EVENT.DRAG:
			SetPressed(false);
			break;
		}
		ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
	}

	private void Awake()
	{
		Button.SetInputDelegate(InputDelegate);
		_pressState = string.Empty;
	}

	public void UpdateState(ShopItem item, ForceUpdate forceEquip)
	{
		if (!base.gameObject.active)
		{
			return;
		}
		switch (forceEquip)
		{
		case ForceUpdate.Buy:
			UpdateBuyState(item, (!item.HasCurrencyHard(ShopItemCurrency.Hard)) ? ShopItemCurrency.Soft : ShopItemCurrency.Hard);
			return;
		case ForceUpdate.BuyHard:
			UpdateBuyState(item, ShopItemCurrency.Hard);
			return;
		case ForceUpdate.BuySoft:
			UpdateBuyState(item, ShopItemCurrency.Soft);
			return;
		case ForceUpdate.Equip:
			UpdateEquipState(item, true);
			return;
		case ForceUpdate.EquipDisabled:
			UpdateEquipState(item, false);
			return;
		}
		if (MonoSingleton<Player>.Instance.IsBought(item))
		{
			UpdateEquipState(item, true);
		}
		else
		{
			UpdateBuyState(item, (!item.HasCurrencyHard(ShopItemCurrency.Hard)) ? ShopItemCurrency.Soft : ShopItemCurrency.Hard);
		}
	}

	private void UpdateEquipState(ShopItem item, bool enabled)
	{
		bool flag = MonoSingleton<Player>.Instance.IsEquipped(item);
		if (enabled)
		{
			if (flag)
			{
				if (item.IsRemovable())
				{
					MonoUtils.SetActive(ObjectLockText, false);
					Button.Text = Strings.GetString("IDS_SHOP_ITEM_REMOVE");
					SetState("Red");
				}
				else
				{
					MonoUtils.SetActive(ObjectLockText, false);
					Button.Text = Strings.GetString("IDS_SHOP_ITEM_EQUIPPED");
					SetState("Blue");
				}
			}
			else
			{
				MonoUtils.SetActive(ObjectLockText, false);
				Button.Text = Strings.GetString("IDS_SHOP_ITEM_EQUIP");
				SetState("Green");
			}
		}
		else
		{
			MonoUtils.SetActive(ObjectLockText, false);
			Button.Text = Strings.GetString("IDS_SHOP_ITEM_EQUIP");
			SetState("Disabled");
		}
	}

	private void UpdateBuyState(ShopItem item, ShopItemCurrency currency)
	{
		if (item.IsLocked(currency))
		{
			MonoUtils.SetActive(ObjectLockText, true);
			Button.Text = string.Empty;
			SetState("Disabled");
			LockText.Text = item.GetPriceString(currency);
			LockReason.Text = item.LockText();
			return;
		}
		Currency = currency;
		if (item.GetPackCount() > 0)
		{
			MonoUtils.SetActive(ObjectLockText, true);
			Button.Text = string.Empty;
			LockText.Text = item.GetPriceString(currency);
			LockReason.Text = Strings.GetString("IDS_NOT_ENOUGH_CURRENCY_BUTTON_BUY") + item.GetPackCountString();
		}
		else
		{
			MonoUtils.SetActive(ObjectLockText, false);
			Button.Text = item.GetPriceString(currency);
		}
		SetState("Green");
	}
}
