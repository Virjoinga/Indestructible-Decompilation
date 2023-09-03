using UnityEngine;

[AddComponentMenu("Indestructible/Utils/UVScroller")]
public class UVScroller : MonoBehaviour
{
	public Vector2 uvScrollSpeed = new Vector2(0f, 0f);

	private Material _material;

	private void Start()
	{
		Renderer renderer = base.GetComponent<Renderer>();
		if (renderer != null)
		{
			_material = renderer.material;
		}
		if (_material == null)
		{
			base.enabled = false;
		}
	}

	private void OnBecameVisible()
	{
		if (_material != null)
		{
			base.enabled = true;
		}
	}

	private void OnBecameInvisible()
	{
		base.enabled = false;
	}

	private void Update()
	{
		Vector2 vector = uvScrollSpeed * Time.time;
		_material.mainTextureOffset = new Vector2(vector.x - Mathf.Floor(vector.x), vector.y - Mathf.Floor(vector.y));
	}
}
