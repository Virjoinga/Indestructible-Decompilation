using UnityEngine;

public class TutorialPlayButton : MonoBehaviour
{
	public UIButton Button;

	public UIAnimation ButtonAnimation;

	private void InputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			MonoUtils.SetActive(ButtonAnimation, true);
			ButtonAnimation.Rewind();
			ButtonAnimation.Play();
			break;
		case POINTER_INFO.INPUT_EVENT.PRESS:
			MonoUtils.SetActive(ButtonAnimation, false);
			break;
		case POINTER_INFO.INPUT_EVENT.TAP:
			MonoSingleton<Player>.Instance.Tutorial.SetPlayButtonTap();
			Object.Destroy(ButtonAnimation.gameObject);
			Object.Destroy(this);
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
		if (MonoSingleton<Player>.Instance.Tutorial.IsPlayButtonTap())
		{
			Object.Destroy(ButtonAnimation.gameObject);
			Object.Destroy(this);
		}
		else
		{
			Button.AddInputDelegate(InputDelegate);
		}
	}
}
