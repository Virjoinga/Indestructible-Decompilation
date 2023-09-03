using UnityEngine;

public class UIManagerThreshold : MonoBehaviour
{
	private float _baseDPI = 100f;

	private void Awake()
	{
		if (Screen.dpi > 0f)
		{
			float num = Screen.dpi / _baseDPI;
			UIManager component = GetComponent<UIManager>();
			component.dragThreshold *= num;
		}
	}
}
