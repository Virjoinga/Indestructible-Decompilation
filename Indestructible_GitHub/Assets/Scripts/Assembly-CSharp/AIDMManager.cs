using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDMManager : AIManager
{
	private enum GameType
	{
		Unsupported = 0,
		DM = 1,
		TeamDM = 2
	}

	private MultiplayerGame _game;

	private GameType _gameType;

	protected override void Awake()
	{
		base.Awake();
		_game = IDTGame.Instance as MultiplayerGame;
		if (_game == null || _game.match.isOnline)
		{
			base.enabled = false;
			Object.Destroy(this);
		}
		else if (_game is DeathmatchGame)
		{
			_gameType = GameType.DM;
		}
		else if (_game is TeamDeathmatchGame)
		{
			_gameType = GameType.TeamDM;
		}
		else
		{
			base.enabled = false;
			Object.Destroy(this);
		}
	}

	private void OnDestroy()
	{
		UnsubscribeFromVehiclesManager();
	}

	protected override void StartManager()
	{
		EnableCheckReturnToBase(false);
		EnableProtectionPoints(false);
		StartCoroutine(AssignTargets());
	}

	private IEnumerator AssignTargets()
	{
		yield return new WaitForSeconds(5f);
		YieldInstruction delayYI = new WaitForSeconds(3f);
		while (true)
		{
			switch (_gameType)
			{
			case GameType.DM:
			{
				LinkedList<AIMoveAndFireController> carsList = _teamsInfo[0].AICarsList;
				IEnumerable<GamePlayer> teamPlayers = _game.players;
				AssignTargetsForCars(carsList, teamPlayers);
				break;
			}
			case GameType.TeamDM:
			{
				LinkedList<AIMoveAndFireController> team0CarsList = _teamsInfo[1].AICarsList;
				LinkedList<AIMoveAndFireController> team1CarsList = _teamsInfo[2].AICarsList;
				IEnumerable<MatchPlayer> team0Players = _game.match.GetTeam(0).players;
				IEnumerable<MatchPlayer> team1Players = _game.match.GetTeam(1).players;
				AssignTargetsForCars(team0CarsList, team1Players);
				AssignTargetsForCars(team1CarsList, team0Players);
				break;
			}
			}
			yield return delayYI;
		}
	}

	private void AssignTargetsForCars<T>(LinkedList<AIMoveAndFireController> carsList, IEnumerable<T> teamPlayers) where T : MatchPlayer
	{
		foreach (AIMoveAndFireController cars in carsList)
		{
			if (!cars.gameObject.active)
			{
				continue;
			}
			Vector3 position = cars.transform.position;
			float num = 999999f;
			Transform transform = null;
			foreach (T teamPlayer in teamPlayers)
			{
				GamePlayer gamePlayer = teamPlayer as GamePlayer;
				if (!(cars.transform == gamePlayer.vehicle.transform) && !(gamePlayer.vehicle == null) && gamePlayer.vehicle.isActive)
				{
					float sqrMagnitude = (position - gamePlayer.vehicle.transform.position).sqrMagnitude;
					if (num > sqrMagnitude)
					{
						num = sqrMagnitude;
						transform = gamePlayer.vehicle.transform;
					}
				}
			}
			if (transform != null)
			{
				cars.SetFireTarget(transform);
				cars.SetChaseTarget(transform);
			}
		}
	}
}
