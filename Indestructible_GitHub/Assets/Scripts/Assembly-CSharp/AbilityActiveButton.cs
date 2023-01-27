using UnityEngine;

public class AbilityActiveButton : MonoBehaviour
{
	public delegate void OnTapDelegateType();

	public SpriteText Label;

	public GameObject TutorialArrow;

	public OnTapDelegateType OnTapDelegate;

	private bool _isPressed;

	private Collider _buttonCollider;

	private void ButtonInputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_isPressed = true;
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
			if (_isPressed)
			{
				CheckTutorialArrow();
				if (OnTapDelegate != null)
				{
					OnTapDelegate();
				}
			}
			_isPressed = false;
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			if (GetCollider() != ptr.hitInfo.collider)
			{
				ptr.evt = POINTER_INFO.INPUT_EVENT.MOVE_OFF;
			}
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			_isPressed = false;
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE:
			break;
		}
	}

	private Collider GetCollider()
	{
		if (_buttonCollider == null)
		{
			_buttonCollider = GetComponent<Collider>();
		}
		return _buttonCollider;
	}

	private void CheckTutorialArrow()
	{
		if (!MonoSingleton<Player>.Instance.Tutorial.IsAbilityButtonTap())
		{
			MonoSingleton<Player>.Instance.Tutorial.SetAbilityButtonTap();
			if (TutorialArrow != null)
			{
				Object.Destroy(TutorialArrow);
			}
		}
	}

	private void Awake()
	{
		if (MonoSingleton<Player>.Instance.Tutorial.IsAbilityButtonTap())
		{
			Object.Destroy(TutorialArrow);
		}
	}

	private void Start()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle != null)
		{
			ShopItemVehicle vehicle = selectedVehicle.Vehicle;
			if (vehicle != null)
			{
				SimpleSpriteUtils.ChangeTexture(base.gameObject, vehicle.Abilities.Active.Icon);
				UIButton component = GetComponent<UIButton>();
				component.SetInputDelegate(ButtonInputDelegate);
			}
		}
	}
}
