using UnityEngine;

public class CheatButton : MonoBehaviour
{
	public delegate void OnTapDelegateType(CheatButton button);

	public SpriteText Label;

	public OnTapDelegateType OnTapDelegate;

	private void OnCheatButtonTap()
	{
		if (OnTapDelegate != null)
		{
			OnTapDelegate(this);
		}
	}

	private void Start()
	{
	}
}
