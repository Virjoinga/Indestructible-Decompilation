using System.Collections;
using UnityEngine;

public class UIAlign : MonoBehaviour
{
	public enum Horizontal
	{
		None = 0,
		Left = 1,
		Right = 2,
		Center = 3
	}

	public enum Vertical
	{
		None = 0,
		Top = 1,
		Bottom = 2,
		Center = 3
	}

	public enum Mode
	{
		Screen = 0,
		Parent = 1
	}

	private Transform m_transform;

	public Horizontal m_horizontal;

	public Vertical m_vertical;

	public Mode m_mode;

	public Vector2 m_margin = Vector2.zero;

	private int m_delayIndex;

	public int m_delay;

	private void Awake()
	{
		m_transform = GetComponent<Transform>();
		if (base.enabled && m_delay <= 0)
		{
			UpdateAlignment();
		}
		m_delayIndex++;
	}

	private IEnumerator Start()
	{
		if (m_delayIndex == m_delay)
		{
			UpdateAlignment();
		}
		else if (m_delayIndex < m_delay)
		{
			while (m_delayIndex < m_delay)
			{
				m_delayIndex++;
				yield return null;
			}
			UpdateAlignment();
		}
	}

	private UIRect GetParentRect()
	{
		float x = m_transform.position.x;
		float y = m_transform.position.y;
		float width = 0f;
		float height = 0f;
		Transform parent = m_transform.parent;
		if (parent != null)
		{
			x = parent.position.x;
			y = parent.position.y;
			SpriteRoot component = parent.GetComponent<SpriteRoot>();
			if (component != null)
			{
				Vector3 centerPoint = component.GetCenterPoint();
				x = parent.position.x + centerPoint.x;
				y = parent.position.y + centerPoint.y;
				width = component.width;
				height = component.height;
			}
			else
			{
				UIPackable component2 = parent.GetComponent<UIPackable>();
				if (component2 != null)
				{
					Vector3 center = component2.Center;
					x = parent.position.x + center.x;
					y = parent.position.y + center.y;
					width = component2.Width;
					height = component2.Height;
				}
			}
		}
		return new UIRect(x, y, width, height);
	}

	private void UpdateAlignment(UIRect rect)
	{
		float num = rect.m_x;
		float num2 = rect.m_y;
		float width = rect.m_width;
		float height = rect.m_height;
		Vector2 margin = m_margin;
		margin.x -= width / 2f;
		margin.y -= height / 2f;
		switch (m_horizontal)
		{
		case Horizontal.Left:
			num += margin.x;
			break;
		case Horizontal.Right:
			num += width + margin.x;
			break;
		case Horizontal.Center:
			num += width / 2f + margin.x;
			break;
		case Horizontal.None:
			num = m_transform.position.x;
			break;
		}
		switch (m_vertical)
		{
		case Vertical.Bottom:
			num2 += margin.y;
			break;
		case Vertical.Top:
			num2 += height + margin.y;
			break;
		case Vertical.Center:
			num2 += height / 2f + margin.y;
			break;
		case Vertical.None:
			num2 = m_transform.position.y;
			break;
		}
		float z = m_transform.position.z;
		m_transform.position = new Vector3(num, num2, z);
	}

	public void UpdateAlignment()
	{
		UIRect rect = new UIRect();
		switch (m_mode)
		{
		case Mode.Screen:
			rect = UITools.GetScreenRect();
			break;
		case Mode.Parent:
			rect = GetParentRect();
			break;
		}
		UpdateAlignment(rect);
	}
}
