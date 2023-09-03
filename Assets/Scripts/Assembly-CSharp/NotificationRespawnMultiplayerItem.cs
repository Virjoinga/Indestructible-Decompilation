using UnityEngine;

public class NotificationRespawnMultiplayerItem : MonoBehaviour
{
	public SpriteText Name;

	public SpriteText Points;

	public SpriteText Kills;

	public SpriteText Deaths;

	public PackedSprite LeagueIcon;

	public GamePlayer Actor;

	public void SetData(GamePlayer player, MultiplayerMatch match)
	{
		Actor = player;
		Name.Text = player.name;
		Kills.Text = player.killCount.ToString();
		Deaths.Text = player.deathCount.ToString();
		Points.Text = player.score.ToString();
		if (player == match.localPlayer)
		{
			SetColor(Color.yellow, false);
			LeagueIcon.SetColor(Color.white);
		}
		else if (player.isDisconnected)
		{
			SetColor(Color.gray, true);
		}
		else
		{
			SetColor(Color.white, true);
		}
		string leagueSpriteName = Player.GetLeagueSpriteName(Actor.league);
		LeagueIcon.PlayAnim(leagueSpriteName);
	}

	public void SetColor(Color color, bool icon)
	{
		Name.SetColor(color);
		Kills.SetColor(color);
		Deaths.SetColor(color);
		Points.SetColor(color);
		if (LeagueIcon != null && icon)
		{
			LeagueIcon.SetColor(color);
		}
	}
}
