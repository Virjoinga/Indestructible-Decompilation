using System;
using UnityEngine;

public class TeamGame : MultiplayerGame
{
	public class TeamData
	{
		public readonly int damageLayers;

		protected int _score;

		public int score
		{
			get
			{
				return _score;
			}
		}

		protected TeamData(int teamID)
		{
			int num = ((teamID != 0) ? 3 : 5);
			damageLayers = (num << LayerMask.NameToLayer("Player")) | (num << LayerMask.NameToLayer("AI"));
		}

		public virtual void Clear()
		{
			_score = 0;
		}
	}

	protected class InternalTeamData : TeamData
	{
		public new int score
		{
			get
			{
				return base.score;
			}
			set
			{
				_score = value;
			}
		}

		public InternalTeamData(int teamID)
			: base(teamID)
		{
		}
	}

	public event Action<MatchTeam> teamScoreChangedEvent;

	public static TeamData GetData(MatchTeam team)
	{
		return team.data as TeamData;
	}

	protected override void Awake()
	{
		base.Awake();
		foreach (MatchTeam team in base.match.teams)
		{
			team.data = new InternalTeamData(team.id);
		}
	}

	protected override void Clear()
	{
		base.Clear();
	}

	protected override void StartGame()
	{
		base.StartGame();
		foreach (MatchTeam team in base.match.teams)
		{
			if (team.connectedPlayerCount <= 0)
			{
				TeamDisconnected(team);
				break;
			}
		}
	}

	protected override Transform SelectPlayerSpawnPoint(MatchPlayer player, Transform[] spawnPoints)
	{
		int num = base.playerGeneralLayer + 1 + player.teamID;
		MatchTeam team = base.match.GetTeam(player.teamID);
		int num2 = 0;
		foreach (MatchPlayer player2 in team.players)
		{
			if (player2 == player)
			{
				Transform transform = null;
				int num3 = 0;
				int i = 0;
				for (int num4 = spawnPoints.Length; i != num4; i++)
				{
					Transform transform2 = spawnPoints[i];
					if (transform2.gameObject.layer == num)
					{
						if (num3 == num2)
						{
							return transform2;
						}
						transform = transform2;
						num3++;
					}
				}
				if (transform != null)
				{
					return transform;
				}
				break;
			}
			num2++;
		}
		return base.SelectPlayerSpawnPoint(player, spawnPoints);
	}

	protected override void SetupVehicle(Vehicle vehicle, GamePlayer player)
	{
		base.SetupVehicle(vehicle, player);
		vehicle.gameObject.layer = LayerMask.NameToLayer("Player") + 1 + player.teamID;
		vehicle.SetDamageLayers(GetData(base.match.GetTeam(player.teamID)).damageLayers);
	}

	protected override void PlayerDisconnected(MultiplayerMatch match, MatchPlayer player)
	{
		base.PlayerDisconnected(match, player);
		if (base.isGameStarted && 0 <= player.teamID)
		{
			MatchTeam team = match.GetTeam(player.teamID);
			if (team.connectedPlayerCount <= 0)
			{
				TeamDisconnected(team);
			}
		}
	}

	protected virtual void TeamDisconnected(MatchTeam team)
	{
		GameOver(((team.id + 1) & 1) == base.match.localTeam.id);
	}

	protected void TeamScoreChanged(MatchTeam team)
	{
		if (this.teamScoreChangedEvent != null)
		{
			this.teamScoreChangedEvent(team);
		}
		CheckGameOver(team);
	}

	protected virtual void CheckGameOver(MatchTeam team)
	{
		if (victoryScore <= GetData(team).score)
		{
			GameOver(team == base.match.localTeam);
		}
	}

	protected InternalTeamData GetInternalData(MatchTeam team)
	{
		return team.data as InternalTeamData;
	}
}
