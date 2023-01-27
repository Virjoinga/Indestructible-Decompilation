using System.Collections.Generic;
using UnityEngine;

public class NewVehiclesButton : MonoBehaviour
{
	public UIAnimation ButtonAnimation;

	public PackedSprite ButtonOverlay;

	public UIScrollList ScrollList;

	public float EdgeOffset;

	public float Offset;

	private UIStateButtonShade _button;

	private bool _requireAnimation;

	private bool _haveActiveBundle;

	private Transform _transform;

	private float _rightEdge;

	private void OnTap(UIStateButtonShade button)
	{
		if (_haveActiveBundle)
		{
			MonoSingleton<Player>.Instance.Tutorial.SetLimitedBundleButtonTap();
		}
		else
		{
			MonoSingleton<Player>.Instance.Tutorial.SetNewVehiclesButtonTap();
		}
		MonoUtils.SetActive(ButtonAnimation, false);
	}

	private void OnPressed(UIStateButtonShade button, bool pressed)
	{
		if (_requireAnimation)
		{
			if (pressed)
			{
				MonoUtils.SetActive(ButtonAnimation, false);
				return;
			}
			MonoUtils.SetActive(ButtonAnimation, true);
			ButtonAnimation.Rewind();
			ButtonAnimation.Play();
		}
	}

	private bool RequireAnimation()
	{
		if (_haveActiveBundle)
		{
			if (!MonoSingleton<Player>.Instance.Tutorial.IsLimitedBundleButtonTap())
			{
				return true;
			}
		}
		else if (!MonoSingleton<Player>.Instance.Tutorial.IsNewVehiclesButtonTap())
		{
			return true;
		}
		return false;
	}

	private void Awake()
	{
		_transform = GetComponent<Transform>();
		_button = GetComponent<UIStateButtonShade>();
		_button.OnPressedEvent += OnPressed;
		_button.OnTapEvent += OnTap;
		_rightEdge = UITools.GetScreenSize().x / 2f - EdgeOffset;
	}

	public void OnActivate()
	{
		UpdatePosition();
		if (MonoSingleton<GameController>.Instance.Configuration.Bundles.Enabled)
		{
			MonoSingleton<Player>.Instance.UpdateBundles();
			List<PlayerBundle> activeBundles = MonoSingleton<Player>.Instance.GetActiveBundles();
			_haveActiveBundle = activeBundles.Count > 0;
		}
		else
		{
			_haveActiveBundle = false;
		}
		if (_haveActiveBundle)
		{
			_button.SetToggleState("Green", true);
			ButtonOverlay.PlayAnim("GreenGlow");
		}
		else
		{
			_button.SetToggleState("Red", true);
			ButtonOverlay.PlayAnim("RedGlow");
		}
		_requireAnimation = RequireAnimation();
		MonoUtils.SetActive(ButtonAnimation, _requireAnimation);
	}

	private void OnDestroy()
	{
		_button.OnPressedEvent -= OnPressed;
		_button.OnTapEvent -= OnTap;
	}

	private float LastItemEdge()
	{
		int count = ScrollList.Count;
		if (count <= 0)
		{
			return 0f;
		}
		IUIListObject item = ScrollList.GetItem(count - 1);
		Transform component = item.gameObject.GetComponent<Transform>();
		return component.position.x + item.BottomRightEdge.x;
	}

	public void UpdatePosition()
	{
		Vector3 position = _transform.position;
		position.x = LastItemEdge() + Offset;
		if (position.x > _rightEdge)
		{
			position.x = _rightEdge;
		}
		_transform.position = position;
	}
}
