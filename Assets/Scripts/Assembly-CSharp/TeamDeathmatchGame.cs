using UnityEngine;

public class TeamDeathmatchGame : TeamGame
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (MonoSingleton<GameController>.Exists())
		{
			MonoSingleton<GameController>.Instance.suspendEvent -= SingleGame.OpenPauseMenu;
		}
	}

	protected override void AddScoreOnKill(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (!base.IsTutorial || (player != null && player != enemy && (player == base.match.localPlayer || GetInternalData(base.match.GetTeam(player.teamID)).score < victoryScore - 1)))
		{
			MatchTeam team;
			if (player == null)
			{
				team = base.match.GetTeam(enemy.teamID);
			}
			else if (player != enemy)
			{
				GamePlayer.MultiplayerGameBase.AddScore(player, 1);
				team = base.match.GetTeam(player.teamID);
			}
			else
			{
				team = base.match.GetTeam((player.teamID == 0) ? 1 : 0);
			}
			GetInternalData(team).score++;
			TeamScoreChanged(team);
		}
	}

	protected override void StartGame()
	{
		base.StartGame();
		if (!base.match.isOnline)
		{
			MonoSingleton<GameController>.Instance.suspendEvent += SingleGame.OpenPauseMenu;
			if (base.IsTutorial)
			{
				GameAnalytics.EventTutorialMatchStarted();
			}
		}
	}

	[RPC]
	protected override void PlayerKillEnemy(int playerID, int enemyID, int damageType)
	{
		base.PlayerKillEnemy(playerID, enemyID, damageType);
	}

	[RPC]
	protected override void PlayerDied(int playerID)
	{
		base.PlayerDied(playerID);
	}

	public override void PauseMenuActivated()
	{
		if (!base.match.isOnline)
		{
			SingleGame.Pause();
		}
	}

	public override void PauseMenuDeactivated()
	{
		if (!base.match.isOnline)
		{
			SingleGame.Resume();
		}
	}
}
