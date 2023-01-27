using UnityEngine;

public class GameMode_CTF : IGameMode
{
	private GameObject[] _teamFlags;

	private GameObject[] _teamVehicles;

	private int _numberOfTeams;

	public void Init(int maxNumberOfTeams)
	{
		_numberOfTeams = maxNumberOfTeams;
		_teamVehicles = new GameObject[maxNumberOfTeams];
		_teamFlags = new GameObject[maxNumberOfTeams];
		GetFlagsFromMap();
	}

	private void GetFlagsFromMap()
	{
		GameObject[] mapFlags = GameObject.FindGameObjectsWithTag("CTF_Flag");
		for (int i = 0; i < _numberOfTeams; i++)
		{
			_teamFlags[i] = SearchTeamFlag(mapFlags, i);
		}
	}

	private GameObject SearchTeamFlag(GameObject[] mapFlags, int teamID)
	{
		if (mapFlags.Length > teamID)
		{
			return mapFlags[teamID];
		}
		return null;
	}

	public void Init()
	{
		Init(2);
	}

	public void OnPlayerVehicleCreated(GameObject playerVehicle, int teamID)
	{
		if (teamID < 0 || teamID >= _numberOfTeams)
		{
			Debug.LogError("Team ID (" + teamID + ") is out of range [0, " + _numberOfTeams + "].");
		}
		else
		{
			if (_teamVehicles[teamID] != null)
			{
				Debug.LogWarning("Vehicle was already initialized for team " + teamID);
			}
			_teamVehicles[teamID] = playerVehicle;
		}
	}
}
