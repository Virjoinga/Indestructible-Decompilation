using System;

internal class GamePush
{
	public DateTime FireDate;

	public int BadgeNumber;

	public string Text;

	public GamePush(string text, DateTime fireDate)
	{
		FireDate = fireDate;
		Text = text;
	}
}
