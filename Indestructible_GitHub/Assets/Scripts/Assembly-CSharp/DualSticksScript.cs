using UnityEngine;

public class DualSticksScript : MonoBehaviour
{
	public Camera m_camera;

	public UIButton m_leftStickButton;

	public UIButton m_rightStickButton;

	private void Start()
	{
		Vector2 screenSize = UITools.GetScreenSize();
		m_leftStickButton.width = screenSize.x / 2f;
		m_rightStickButton.width = screenSize.x / 2f;
		m_leftStickButton.height = screenSize.y;
		m_rightStickButton.height = screenSize.y;
	}
}
