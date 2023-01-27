using UnityEngine;

public class UIDialogShade : MonoBehaviour
{
	private SpriteRoot _root;

	protected virtual void Awake()
	{
		_root = GetComponent<SpriteRoot>();
	}

	protected virtual void Start()
	{
		Vector2 screenSize = UITools.GetScreenSize();
		_root.SetSize(screenSize.x, screenSize.y);
		BoxCollider component = GetComponent<BoxCollider>();
		component.size = new Vector3(screenSize.x, screenSize.y, 0f);
	}

	public void SetColor(Color color)
	{
		_root.SetColor(color);
	}
}
