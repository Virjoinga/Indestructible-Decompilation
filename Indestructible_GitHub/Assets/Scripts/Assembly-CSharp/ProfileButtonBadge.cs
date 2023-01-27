using UnityEngine;

public class ProfileButtonBadge : MonoBehaviour
{
	public SpriteText BadgeText;

	private void OnEnable()
	{
		int talentPoints = MonoSingleton<Player>.Instance.TalentPoints;
		if (talentPoints <= 0)
		{
			MonoUtils.SetActive(this, false);
		}
		else
		{
			BadgeText.Text = talentPoints.ToString();
		}
	}
}
