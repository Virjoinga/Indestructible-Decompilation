using Glu.Localization;
using UnityEngine;

public class DialogSelectVehicleBuyButton : MonoBehaviour
{
	public UIStateToggleBtn Button;

	public ShopItemCurrency Currency;

	public bool ZeroPrice;

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
				Button.SetToggleState(_pressState + "Pressed", true);
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

	private void InputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			if (_pressState == "Green" || _pressState == "Gray")
			{
				InvokeButtonTap();
			}
			ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
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
		case POINTER_INFO.INPUT_EVENT.MOVE:
			break;
		}
	}

	private void Awake()
	{
		Button.SetInputDelegate(InputDelegate);
		_pressState = string.Empty;
	}

	public void UpdateState(ShopItem item)
	{
		UpdateState(item, ShopItemCurrency.None);
	}

	public void UpdateState(ShopItem item, ShopItemCurrency currency)
	{
		if (item.GetPrice(currency) == 0 || ZeroPrice)
		{
			Button.Text = Strings.GetString("IDS_SHOP_ITEM_FREE");
			SetState("Gray");
		}
		else
		{
			Button.Text = item.GetPriceString(Currency);
			SetState("Green");
		}
	}
}
