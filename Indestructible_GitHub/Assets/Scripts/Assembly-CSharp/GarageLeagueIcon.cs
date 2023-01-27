using UnityEngine;

public class GarageLeagueIcon : MonoBehaviour
{
	private PackedSprite _icon;

	private void Awake()
	{
		_icon = GetComponent<PackedSprite>();
	}

	public void UpdateIcon()
	{
		int league = MonoSingleton<Player>.Instance.League;
		string leagueSpriteName = Player.GetLeagueSpriteName(league);
		_icon.PlayAnim(leagueSpriteName);
	}
}
