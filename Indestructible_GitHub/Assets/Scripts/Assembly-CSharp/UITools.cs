using UnityEngine;

public class UITools
{
	public static Vector2 GetScreenSize()
	{
		int num = ((Screen.width <= Screen.height) ? Screen.height : Screen.width);
		int num2 = ((Screen.width <= Screen.height) ? Screen.width : Screen.height);
		float num3 = (float)num / (float)num2;
		return new Vector2(200f * num3, 200f);
	}

	public static UIRect GetScreenRect()
	{
		Vector2 screenSize = GetScreenSize();
		return new UIRect(0f, 0f, screenSize.x, screenSize.y);
	}
}
