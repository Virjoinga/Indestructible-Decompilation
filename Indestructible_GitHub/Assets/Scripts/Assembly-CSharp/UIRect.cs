public class UIRect
{
	public float m_x;

	public float m_y;

	public float m_width;

	public float m_height;

	public UIRect()
	{
		Set(0f, 0f, 0f, 0f);
	}

	public UIRect(float x, float y, float width, float height)
	{
		Set(x, y, width, height);
	}

	public void Set(float x, float y, float width, float height)
	{
		m_x = x;
		m_y = y;
		m_width = width;
		m_height = height;
	}

	public void SetPosition(float x, float y)
	{
		m_x = x;
		m_y = y;
	}

	public void SetSize(float width, float height)
	{
		m_width = width;
		m_height = height;
	}
}
