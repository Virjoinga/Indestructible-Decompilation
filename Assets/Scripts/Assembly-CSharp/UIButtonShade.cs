using UnityEngine;

public class UIButtonShade : UIButton
{
	public Color Normal = Color.white;

	public Color Pressed = new Color(0.5f, 0.5f, 0.5f);

	public Color Disabled = new Color(0.5f, 0.5f, 0.5f);

	public SpriteText[] Texts;

	public SpriteRoot[] Sprites;

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

	public override void SetControlState(CONTROL_STATE s, bool suppressTransitions)
	{
		base.SetControlState(s, suppressTransitions);
		Color elementsColor = Normal;
		switch (s)
		{
		case CONTROL_STATE.ACTIVE:
			elementsColor = Pressed;
			break;
		case CONTROL_STATE.DISABLED:
			elementsColor = Disabled;
			break;
		}
		SetElementsColor(elementsColor);
		SetColor(elementsColor);
	}
}
