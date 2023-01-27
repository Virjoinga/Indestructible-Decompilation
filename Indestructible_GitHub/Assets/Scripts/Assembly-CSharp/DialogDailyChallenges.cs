using System;
using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class DialogDailyChallenges : UIDialog
{
	public SpriteText Rewards;

	public SpriteText Progress;

	public PackedSprite StatusIcon;

	public SpriteText TimerText;

	public DialogDailyChallenge[] Challenges;

	private int _time = -1;

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	public override void Activate()
	{
		base.Activate();
		MonoSingleton<Player>.Instance.Challenges.Update();
		string @string = Strings.GetString("IDS_DAILY_CHALLENGES_CHALLENGE_REWARDS");
		List<DailyChallenges.DailyChallenge> completed = MonoSingleton<Player>.Instance.Challenges.GetCompleted();
		if (completed.Count == 0)
		{
			completed = MonoSingleton<Player>.Instance.Challenges.GetActive();
		}
		int num = 0;
		int num2 = 0;
		DialogDailyChallenge[] challenges = Challenges;
		foreach (DialogDailyChallenge dialogDailyChallenge in challenges)
		{
			if (num < completed.Count)
			{
				DailyChallenges.DailyChallenge dailyChallenge = completed[num];
				bool flag = dailyChallenge.IsCompleted();
				if (flag)
				{
					num2++;
				}
				dialogDailyChallenge.Rewards.Text = string.Format(@string, dailyChallenge.Reward.GetString());
				dialogDailyChallenge.StatusIcon.PlayAnim((!flag) ? "NotDone" : "Done");
				dialogDailyChallenge.Progress.Text = dailyChallenge.GetProgressText();
				dialogDailyChallenge.Title.Text = dailyChallenge.GetTitle();
			}
			else
			{
				MonoUtils.SetActive(dialogDailyChallenge, false);
			}
			num++;
		}
		string string2 = Strings.GetString("IDS_DAILY_CHALLENGES_OVERALL_PROGRESS");
		Progress.Text = string.Format(string2, num2, completed.Count);
		string2 = Strings.GetString("IDS_DAILY_CHALLENGES_OVERALL_REWARDS");
		Rewards.Text = string.Format(string2, MonoSingleton<Player>.Instance.Challenges.Reward.GetString());
		bool flag2 = num2 == completed.Count;
		StatusIcon.PlayAnim((!flag2) ? "NotDone" : "Done");
		Update();
	}

	private void Update()
	{
		float seconds = MonoSingleton<Player>.Instance.Challenges.GetSeconds();
		int num = Mathf.CeilToInt(seconds);
		if (_time != num)
		{
			_time = num;
			TimeSpan timeSpan = new TimeSpan(0, 0, _time);
			int num2 = Mathf.FloorToInt((float)timeSpan.TotalHours);
			string @string = Strings.GetString("IDS_DAILY_CHALLENGES_TIMER");
			TimerText.Text = string.Format(@string, num2, timeSpan.Minutes, timeSpan.Seconds);
		}
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
