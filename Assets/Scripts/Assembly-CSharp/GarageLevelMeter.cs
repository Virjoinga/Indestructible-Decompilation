using Glu.Localization;
using UnityEngine;

public class GarageLevelMeter : MonoBehaviour
{
	public SpriteText LevelValue;

	public InclinedProgressBar LevelProgress;

	private Transform _glowTransform;

	private int _cachedExperience = -1;

	private bool _cachedBoost;

	private void Update()
	{
		MonoSingleton<Player>.Instance.UpdateBoosts();
		bool flag = MonoSingleton<Player>.Instance.BoughtBoosts.Count > 0;
		if (_cachedBoost != flag)
		{
			_cachedBoost = flag;
			if (_cachedBoost)
			{
				LevelProgress.Play("Boost");
			}
			else
			{
				LevelProgress.Play("Normal");
			}
		}
		if (_cachedExperience != (int)MonoSingleton<Player>.Instance.Experience)
		{
			_cachedExperience = MonoSingleton<Player>.Instance.Experience;
			if ((int)MonoSingleton<Player>.Instance.Level < MonoSingleton<Player>.Instance.GetLevelsCount())
			{
				int min = 0;
				int max = 0;
				MonoSingleton<Player>.Instance.GetLevelExperience(ref min, ref max);
				float num = _cachedExperience - min;
				float num2 = max - min;
				float position = num / num2;
				LevelProgress.Position = position;
				string @string = Strings.GetString("IDS_GARAGE_LEVEL_METER");
				LevelValue.Text = string.Format(@string, MonoSingleton<Player>.Instance.Level);
			}
			else
			{
				LevelProgress.Position = 0f;
				string string2 = Strings.GetString("IDS_GARAGE_LEVEL_METER_MAX");
				LevelValue.Text = string.Format(string2, MonoSingleton<Player>.Instance.Level);
			}
		}
	}
}
