using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class AnimatedNotificationAchievement : AnimatedNotification
{
	public SpriteText Title;

	public PackedSprite Image;

	public SpriteText Description;

	private bool _finished = true;

	private Queue<string> _achievements = new Queue<string>();

	private static AnimatedNotificationAchievement _instance;

	public static AnimatedNotificationAchievement Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject original = (GameObject)Resources.Load("Dialogs/NotificationAchievement");
				GameObject gameObject = (GameObject)Object.Instantiate(original);
				Transform component = gameObject.GetComponent<Transform>();
				component.position = Vector3.zero;
				_instance = gameObject.GetComponent<AnimatedNotificationAchievement>();
			}
			return _instance;
		}
	}

	public void StartNext()
	{
		if (_achievements.Count == 0)
		{
			_finished = true;
			Deactivate();
			return;
		}
		string id = _achievements.Dequeue();
		PlayerAchievements achievements = MonoSingleton<Player>.Instance.Achievements;
		PlayerAchievements.Achievement achievement = achievements.Find(id);
		if (achievement != null)
		{
			Description.Text = Strings.GetString(achievement.DescriptionId);
			Title.Text = Strings.GetString(achievement.TitleId);
			Image.PlayAnim(achievement.Id);
			achievement.SetShown();
			MonoSingleton<Player>.Instance.Save();
			Restart();
		}
		else
		{
			StartNext();
		}
	}

	protected override void OnFinish()
	{
		StartNext();
	}

	private void Deactivate()
	{
		Object.Destroy(base.gameObject);
		_instance = null;
	}

	public void Add(string id)
	{
		_achievements.Enqueue(id);
		if (_finished)
		{
			_finished = false;
			StartNext();
		}
	}

	private void OnDestroy()
	{
		_instance = null;
	}
}
