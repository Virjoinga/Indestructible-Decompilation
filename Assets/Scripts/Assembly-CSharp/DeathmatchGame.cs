using UnityEngine;

public class DeathmatchGame : MultiplayerGame
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (MonoSingleton<GameController>.Exists())
		{
			MonoSingleton<GameController>.Instance.suspendEvent -= SingleGame.OpenPauseMenu;
		}
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

	protected override void StartGame()
	{
		base.StartGame();
		if (base.match.connectedPlayerCount <= 1)
		{
			GameOver(true);
		}
		if (!base.match.isOnline)
		{
			MonoSingleton<GameController>.Instance.suspendEvent += SingleGame.OpenPauseMenu;
		}
	}

	protected override void PlayerKillEnemy(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		base.PlayerKillEnemy(player, enemy, damageType);
		CheckWin(player);
	}

	protected override void AddScoreOnKill(GamePlayer player, GamePlayer enemy, DamageType damageType)
	{
		if (player == null)
		{
			return;
		}
		if (base.match.isOnline)
		{
			GamePlayer.MultiplayerGameBase.AddScore(player, (player != enemy) ? 1 : (-1));
		}
		else if (player != enemy)
		{
			int num = player.score + 1;
			if (player == base.match.localPlayer || num < victoryScore)
			{
				GamePlayer.MultiplayerGameBase.SetScore(player, num);
			}
		}
	}

	private void CheckWin(GamePlayer player)
	{
		if (victoryScore <= player.score)
		{
			GameOver(player == base.localPlayer);
		}
	}

	protected override void PlayerDisconnected(MultiplayerMatch match, MatchPlayer player)
	{
		base.PlayerDisconnected(match, player);
		if (base.isGameStarted && match.connectedPlayerCount <= 1)
		{
			GameOver(true);
		}
	}

	protected override Transform SelectSpawnPoint(Vehicle vehicle, Transform[] spawnPoints)
	{
		if (vehicle == null)
		{
			return spawnPoints[base.localPlayer.id % spawnPoints.Length];
		}
		return base.SelectSpawnPoint(vehicle, spawnPoints);
	}

	//[RPC]
	protected override void PlayerKillEnemy(int playerID, int enemyID, int damageType)
	{
		base.PlayerKillEnemy(playerID, enemyID, damageType);
	}

	//[RPC]
	protected override void PlayerDied(int playerID)
	{
		base.PlayerDied(playerID);
	}
}
