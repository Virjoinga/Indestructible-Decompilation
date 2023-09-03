using System.Collections;
using UnityEngine;

public class UITile : MonoBehaviour
{
	public enum Mode
	{
		None = 0,
		Sprite = 1,
		Perfect = 2
	}

	private SpriteRoot m_root;

	private Vector2 m_size = Vector2.zero;

	public Mode m_mode;

	private int m_delayIndex;

	public int m_delay;

	private void Awake()
	{
		m_root = GetComponent<SpriteRoot>();
		m_size.x = m_root.width;
		m_size.y = m_root.height;
		if (base.enabled && m_delay <= 0)
		{
			UpdateTiling();
		}
		m_delayIndex++;
	}

	private IEnumerator Start()
	{
		if (m_delayIndex == m_delay)
		{
			UpdateTiling();
		}
		else if (m_delayIndex < m_delay)
		{
			while (m_delayIndex < m_delay)
			{
				m_delayIndex++;
				yield return null;
			}
			UpdateTiling();
		}
	}

	public void UpdateTiling()
	{
		Vector2 size = new Vector2(m_root.width, m_root.height);
		UpdateTiling(size);
	}

	public void UpdateTiling(Vector2 size)
	{
		if (m_mode != 0)
		{
			Renderer component = m_root.GetComponent<Renderer>();
			Material material = component.material;
			Texture mainTexture = material.mainTexture;
			mainTexture.wrapMode = TextureWrapMode.Repeat;
			Vector2 one = Vector2.one;
			switch (m_mode)
			{
			case Mode.Sprite:
				one.x = size.x / m_size.x;
				one.y = size.y / m_size.y;
				break;
			case Mode.Perfect:
			{
				one.x = size.x / (float)mainTexture.width;
				one.y = size.y / (float)mainTexture.height;
				Vector2 screenSize = UITools.GetScreenSize();
				float num = (float)Screen.height / screenSize.y;
				one.x *= num;
				one.y *= num;
				break;
			}
			}
			material.mainTextureScale = one;
		}
	}
}
