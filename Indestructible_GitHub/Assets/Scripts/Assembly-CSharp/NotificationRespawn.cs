using Glu.Localization;
using UnityEngine;

public class NotificationRespawn : UINotification
{
	public SpriteText RespawnText;

	private float _time;

	private int _timeCached;

	public void SetTime(float time)
	{
		_timeCached = 0;
		_time = time;
		SetTimeText();
	}

	private void SetTimeText()
	{
		int num = Mathf.CeilToInt(_time);
		if (num != _timeCached)
		{
			string @string = Strings.GetString("IDS_GAMEPLAY_RESPAWN_TIMER");
			RespawnText.Text = string.Format(@string, num);
			_timeCached = num;
		}
	}

	private void Update()
	{
		if (_time > 0f)
		{
			_time -= Time.deltaTime;
			SetTimeText();
			if (_time <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
