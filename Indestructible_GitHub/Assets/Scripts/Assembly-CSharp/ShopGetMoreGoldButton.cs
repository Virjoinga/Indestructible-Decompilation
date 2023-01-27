using UnityEngine;

public class ShopGetMoreGoldButton : MonoBehaviour
{
	public UIButton Button;

	public UIAnimation ButtonAnimation;

	private void InputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			MonoUtils.SetActive(ButtonAnimation, true);
			ButtonAnimation.Rewind();
			ButtonAnimation.Play();
			break;
		case POINTER_INFO.INPUT_EVENT.PRESS:
			MonoUtils.SetActive(ButtonAnimation, false);
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE:
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
			break;
		}
	}

	private void OnDestroy()
	{
		Button.RemoveInputDelegate(InputDelegate);
	}

	private void Awake()
	{
		Button.AddInputDelegate(InputDelegate);
	}
}
