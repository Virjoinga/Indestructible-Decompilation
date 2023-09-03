using UnityEngine;

public class ButtonInputAdapter : MonoBehaviour
{
	public bool Draggable;

	protected AutoSpriteControlBase _control;

	protected Collider _collider;

	protected bool _pressed;

	protected bool _enabled = true;

	protected UIButton _buttonPush;

	protected UIStateToggleBtn _buttonState;

	protected Collider InputCollider
	{
		get
		{
			if (_collider == null)
			{
				_collider = GetComponent<Collider>();
			}
			return _collider;
		}
	}

	private void InvokeButtonTap()
	{
		if (!_enabled || OnTap())
		{
			return;
		}
		if (_buttonPush != null)
		{
			if (_buttonPush.scriptWithMethodToInvoke != null && !string.IsNullOrEmpty(_buttonPush.methodToInvoke))
			{
				_buttonPush.scriptWithMethodToInvoke.Invoke(_buttonPush.methodToInvoke, _buttonPush.delay);
			}
		}
		else if (_buttonState != null && _buttonState.scriptWithMethodToInvoke != null && !string.IsNullOrEmpty(_buttonState.methodToInvoke))
		{
			_buttonState.scriptWithMethodToInvoke.Invoke(_buttonState.methodToInvoke, _buttonState.delay);
		}
	}

	public virtual void SetState(string name)
	{
		int stateIndex = _control.GetStateIndex(name);
		if (stateIndex != -1)
		{
			_control.SetState(stateIndex);
		}
	}

	public virtual void SetEnabled(bool enabled)
	{
		if (enabled && !_enabled)
		{
			SetState("Normal");
			_enabled = true;
		}
		else if (!enabled && _enabled)
		{
			SetState("Disabled");
			_enabled = false;
		}
	}

	protected virtual void SetPressed(bool pressed)
	{
		if (pressed && !_pressed)
		{
			if (_buttonPush != null)
			{
				SetState("Active");
			}
			_pressed = true;
			OnPressed(true);
		}
		else if (!pressed && _pressed)
		{
			if (_buttonPush != null)
			{
				SetState("Normal");
			}
			_pressed = false;
			OnPressed(false);
		}
	}

	protected virtual void OnPressed(bool pressed)
	{
	}

	protected virtual bool OnTap()
	{
		return false;
	}

	protected virtual void InputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			SetPressed(false);
			InvokeButtonTap();
			ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
			break;
		case POINTER_INFO.INPUT_EVENT.PRESS:
			SetPressed(true);
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			if (ptr.hitInfo.collider != InputCollider || Draggable)
			{
				SetPressed(false);
			}
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
			if (_pressed)
			{
				SetPressed(false);
				InvokeButtonTap();
			}
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			SetPressed(false);
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE:
			break;
		}
	}

	protected virtual void Awake()
	{
		_control = GetComponent<AutoSpriteControlBase>();
		_control.SetInputDelegate(InputDelegate);
		_buttonPush = GetComponent<UIButton>();
		_buttonState = GetComponent<UIStateToggleBtn>();
	}
}
