using UnityEngine;

public class TalentsTalentLine : MonoBehaviour
{
	public Transform Left;

	public Transform Right;

	public bool Special;

	private UIBorderSprite _line;

	private void Awake()
	{
		_line = GetComponent<UIBorderSprite>();
	}

	public void Initialize()
	{
		_line = GetComponent<UIBorderSprite>();
		Transform component = _line.GetComponent<Transform>();
		if (Special)
		{
			_line.SetWidth(Right.position.y - Left.position.y);
			Vector3 position = component.position;
			position.y = (Right.position.y + Left.position.y) / 2f;
			position.x = (Right.position.x + Left.position.x) / 2f - 10.78125f;
			component.position = position;
		}
		else
		{
			_line.SetWidth(Right.position.x - Left.position.x - 21.5625f);
			Vector3 position2 = component.position;
			position2.x = (Right.position.x + Left.position.x) / 2f;
			position2.y = (Right.position.y + Left.position.y) / 2f;
			component.position = position2;
		}
	}

	public void UpdateData()
	{
		TalentsTalentButton component = Left.GetComponent<TalentsTalentButton>();
		TalentsTalentButton component2 = Right.GetComponent<TalentsTalentButton>();
		if (component.IsUnlocked() && component2.IsUnlocked())
		{
			_line.Color = new Color(0.24f, 1f, 1f);
		}
		else
		{
			_line.Color = Color.white;
		}
	}
}
