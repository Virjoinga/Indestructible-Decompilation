using System;
using System.Collections;
using UnityEngine;

public class UIJustify : MonoBehaviour
{
	public enum Mode
	{
		Screen = 0,
		Parent = 1
	}

	[Serializable]
	public class Margin
	{
		public float m_left;

		public float m_top;

		public float m_right;

		public float m_bottom;
	}

	private Transform m_transform;

	public bool m_horizontal;

	public bool m_vertical;

	private SpriteRoot m_root;

	public Mode m_mode;

	public Margin m_margin;

	private int m_delayIndex;

	public int m_delay;

	private void Awake()
	{
		m_transform = GetComponent<Transform>();
		m_root = GetComponent<SpriteRoot>();
		if (base.enabled && m_delay <= 0)
		{
			UpdateJustification();
		}
		m_delayIndex++;
	}

	private IEnumerator Start()
	{
		if (m_delayIndex == m_delay)
		{
			UpdateJustification();
		}
		else if (m_delayIndex < m_delay)
		{
			while (m_delayIndex < m_delay)
			{
				m_delayIndex++;
				yield return null;
			}
			UpdateJustification();
		}
	}

	private UIRect GetParentRect()
	{
		float x = 0f;
		float y = 0f;
		float width = 0f;
		float height = 0f;
		Transform parent = m_transform.parent;
		if (parent != null)
		{
			SpriteRoot component = parent.GetComponent<SpriteRoot>();
			if (component != null)
			{
				Vector3 centerPoint = component.GetCenterPoint();
				x = centerPoint.x;
				y = centerPoint.y;
				width = component.width;
				height = component.height;
			}
		}
		return new UIRect(x, y, width, height);
	}

	public void UpdateJustification(UIRect rect)
	{
		float x = rect.m_x;
		float y = rect.m_y;
		float num = m_root.width;
		float num2 = m_root.height;
		if (m_horizontal)
		{
			num = rect.m_width - m_margin.m_left - m_margin.m_right;
			x += (m_margin.m_left - m_margin.m_right) / 2f;
		}
		else
		{
			x = m_transform.position.x;
		}
		if (m_vertical)
		{
			num2 = rect.m_height - m_margin.m_top - m_margin.m_bottom;
			y += (m_margin.m_bottom - m_margin.m_top) / 2f;
		}
		else
		{
			y = m_transform.position.y;
		}
		if (num <= 0f)
		{
			num = m_root.width;
		}
		if (num2 <= 0f)
		{
			num2 = m_root.height;
		}
		float z = m_transform.position.z;
		m_transform.position = new Vector3(x, y, z);
		m_root.SetSize(num, num2);
	}

	public void UpdateJustification()
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
		UpdateJustification(rect);
	}
}
