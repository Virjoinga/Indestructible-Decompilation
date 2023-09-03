using System;
using UnityEngine;

public class CTFGame : TeamGame
{
	public int capturedFlagScore;

	public int returnedFlagScore;

	public int deliveredFlagScore = 1;

	public int killCourierAddScore;

	private GamePlayer[] _courierPlayers = new GamePlayer[2];

	private int _deliveredFlags;

	private int _flagCourierKills;

	public int deliveredFlags
	{
		get
		{
			return _deliveredFlags;
		}
	}

	public int flagCourierKills
	{
		get
		{
			return _flagCourierKills;
		}
	}

	public event Action<GamePlayer> flagCapturedEvent;

	public event Action<GamePlayer> flagReturnedEvent;

	public event Action<GamePlayer> flagDeliveredEvent;

	public event Action<GamePlayer> courierKilledEvent;

	public event Action<int> flagAutoReturnedEvent;

	protected override void StartGame()
	{
		base.StartGame();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Destructed(Destructible destructed, DestructionReason reason, INetworkWeapon weapon)
	{
		base.Destructed(destructed, reason, weapon);
		if (weapon != null && weapon.player != null && destructed.vehicle != null && (destructed.vehicle.player == _courierPlayers[0] || destructed.vehicle.player == _courierPlayers[1]))
		{
			CourierKilledByPlayer(weapon.player.id);
			base.photonView.RPC("CourierKilledByPlayer", PhotonTargets.Others, weapon.player.id);
		}
	}

	[RPC]
	private void CourierKilledByPlayer(int killerId)
	{
		if (base.match.localPlayer.id == killerId)
		{
			_flagCourierKills++;
		}
	}

	public void FlagCaptured(GamePlayer player)
	{
		PlayerCaptureFlag(player);
		if (base.match.isOnline)
		{
			base.photonView.RPC("PlayerCaptureFlag", PhotonTargets.Others, player.id);
		}
	}

	public void FlagReturned(GamePlayer player)
	{
		PlayerReturnFlag(player);
		if (base.match.isOnline)
		{
			base.photonView.RPC("PlayerReturnFlag", PhotonTargets.Others, player.id);
		}
	}

	public void FlagAutoReturned(int teamID)
	{
		if (this.flagAutoReturnedEvent != null)
		{
			this.flagAutoReturnedEvent(teamID);
		}
	}

	public void FlagDelivered(GamePlayer player)
	{
		PlayerDeliverFlag(player);
		if (base.match.isOnline)
		{
			base.photonView.RPC("PlayerDeliverFlag", PhotonTargets.Others, player.id);
		}
		if (_courierPlayers[player.teamID] == player)
		{
			_courierPlayers[player.teamID] = null;
		}
	}

	public void CourierKilled(GamePlayer player)
	{
		if (this.courierKilledEvent != null)
		{
			this.courierKilledEvent(player);
		}
	}

	private void PlayerCaptureFlag(GamePlayer player)
	{
		GamePlayer.MultiplayerGameBase.AddScore(player, capturedFlagScore);
		if (this.flagCapturedEvent != null)
		{
			this.flagCapturedEvent(player);
		}
		_courierPlayers[player.teamID] = player;
	}

	private void PlayerReturnFlag(GamePlayer player)
	{
		GamePlayer.MultiplayerGameBase.AddScore(player, returnedFlagScore);
		if (this.flagReturnedEvent != null)
		{
			this.flagReturnedEvent(player);
		}
	}

	private void PlayerDeliverFlag(GamePlayer player)
	{
		GamePlayer.MultiplayerGameBase.AddScore(player, deliveredFlagScore);
		MatchTeam team = base.match.GetTeam(player.teamID);
		GetInternalData(team).score++;
		if (base.match.localPlayer == player)
		{
			_deliveredFlags++;
		}
		if (this.flagDeliveredEvent != null)
		{
			this.flagDeliveredEvent(player);
		}
		if (_courierPlayers[player.teamID] == player)
		{
			_courierPlayers[player.teamID] = null;
		}
		TeamScoreChanged(team);
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

	[RPC]
	private void PlayerCaptureFlag(int playerID)
	{
		GamePlayer player;
		if (TryGetPlayer(playerID, out player))
		{
			PlayerCaptureFlag(player);
		}
	}

	[RPC]
	private void PlayerReturnFlag(int playerID)
	{
		GamePlayer player;
		if (TryGetPlayer(playerID, out player))
		{
			PlayerReturnFlag(player);
		}
	}

	[RPC]
	private void PlayerDeliverFlag(int playerID)
	{
		GamePlayer player;
		if (TryGetPlayer(playerID, out player))
		{
			PlayerDeliverFlag(player);
		}
	}
}
