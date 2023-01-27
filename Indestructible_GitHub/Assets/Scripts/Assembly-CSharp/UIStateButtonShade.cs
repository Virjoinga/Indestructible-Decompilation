using UnityEngine;

public class UIStateButtonShade : UIStateToggleBtn
{
	public delegate void OnPressedDelegate(UIStateButtonShade button, bool pressed);

	public delegate void OnTapDelegate(UIStateButtonShade button);

	public Color Normal = Color.white;

	public Color Pressed = new Color(0.5f, 0.5f, 0.5f);

	public Color Disabled = new Color(0.5f, 0.5f, 0.5f);

	public SpriteText[] Texts;

	public SpriteRoot[] Sprites;

	private bool _pressed;

	private Collider _collider;

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

	public event OnPressedDelegate OnPressedEvent;

	public event OnTapDelegate OnTapEvent;

	private void SetElementsColor(Color color)
	{
		SpriteText[] texts = Texts;
		foreach (SpriteText spriteText in texts)
		{
			spriteText.SetColor(color);
		}
		SpriteRoot[] sprites = Sprites;
		foreach (SpriteRoot spriteRoot in sprites)
		{
			spriteRoot.SetColor(color);
		}
	}

	public override void SetToggleState(int s, bool suppressTransition)
	{
		if (s == states.Length - 1)
		{
			m_controlIsEnabled = false;
			curStateIndex = s;
			DisableMe();
		}
		else
		{
			m_controlIsEnabled = true;
			base.SetToggleState(s, suppressTransition);
		}
		string stateName = base.StateName;
		Color elementsColor = Normal;
		if (stateName.EndsWith("Pressed"))
		{
			elementsColor = Pressed;
		}
		else if (stateName.EndsWith("Disabled"))
		{
			elementsColor = Disabled;
		}
		SetElementsColor(elementsColor);
		SetColor(elementsColor);
	}

	private void SetPressed(bool pressed)
	{
		if (_pressed == pressed)
		{
			return;
		}
		string stateName = base.StateName;
		if (stateName.EndsWith("Disabled"))
		{
			return;
		}
		bool flag = stateName.EndsWith("Pressed");
		if (pressed)
		{
			if (!flag)
			{
				SetToggleState(stateName + "Pressed", true);
			}
		}
		else if (flag)
		{
			int length = stateName.Length;
			stateName = stateName.Remove(length - 7);
			SetToggleState(stateName, true);
		}
		_pressed = pressed;
		if (this.OnPressedEvent != null)
		{
			this.OnPressedEvent(this, _pressed);
		}
	}

	private void InvokeButtonTap()
	{
		if (scriptWithMethodToInvoke != null)
		{
			scriptWithMethodToInvoke.Invoke(methodToInvoke, delay);
		}
		if (this.OnTapEvent != null)
		{
			this.OnTapEvent(this);
		}
	}

	private void InputDelegate(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.TAP:
			SetPressed(false);
			InvokeButtonTap();
			break;
		case POINTER_INFO.INPUT_EVENT.PRESS:
			SetPressed(true);
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
		{
			bool pressed = ptr.hitInfo.collider == InputCollider;
			SetPressed(pressed);
			break;
		}
		case POINTER_INFO.INPUT_EVENT.RELEASE:
			if (_pressed)
			{
				InvokeButtonTap();
				SetPressed(false);
			}
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			SetPressed(false);
			break;
		}
		ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
	}

	protected override void Awake()
	{
		base.Awake();
		SetInputDelegate(InputDelegate);
	}
}
