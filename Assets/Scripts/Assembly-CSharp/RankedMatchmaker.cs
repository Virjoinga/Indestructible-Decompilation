using System;
using System.Collections;
using System.Collections.Generic;
using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Network/Ranked Matchmaker")]
public class RankedMatchmaker : Glu.MonoBehaviour
{
	public enum State
	{
		Connecting = 1,
		Searching = 2,
		WaitingRemotePlayersDisconnection = 3,
		Joining = 4,
		ActiveStateMask = 7,
		Succeeded = 0,
		Failed = 8,
		Canceled = 16
	}

	private class InternalPlayer : GamePlayer, IComparable<InternalPlayer>
	{
		public new string name
		{
			get
			{
				return base.name;
			}
			set
			{
				_name = value;
			}
		}

		public InternalPlayer(int id)
			: base(id)
		{
		}

		public InternalPlayer(int id, string name, int rating, int league)
			: base(id, name, rating, league)
		{
		}

		public int CompareTo(InternalPlayer otherPlayer)
		{
			int num = base.rating - otherPlayer.rating;
			if (num != 0)
			{
				return num;
			}
			return base.id - otherPlayer.id;
		}
	}

	public delegate void MatchmakingDelegate(RankedMatchmaker matchmaker, State state);

	public float connectionTimeout = 13f;

	public float forceMatchmakingTimeout = 17.5f;

	public float idleFailTimeout = 180f;

	public int maxLeagueCount = 3;

	public int maxPlayerCount = 64;

	public int lobbySize;

	private bool inRemoteJoinProcess;

	private State _state;

	private string _lobbyName;

	private int _maxRequiredPlayerCount;

	private int _teamCount;

	private string _levelName;

	private string _config;

	private InternalPlayer _localPlayer;

	private int _playerCount;

	private List<InternalPlayer>[] _playersByLeague;

	private Dictionary<int, InternalPlayer> _playersMap;

	private List<InternalPlayer> _selectedPlayers;

	private PhotonView _photonView;

	private MultiplayerMatch _match;

	private float _matchmakingStartTime;

	private float _idleFailTime;

	private MatchmakingDelegate _matchmakingDelegate;

	private bool _shouldForceFindMatch;

	private bool _isSending;

	public State state
	{
		get
		{
			return _state;
		}
	}

	public int playerCount
	{
		get
		{
			return _playerCount;
		}
	}

	public int maxRequiredPlayerCount
	{
		get
		{
			return _maxRequiredPlayerCount;
		}
	}

	public MultiplayerMatch match
	{
		get
		{
			return _match;
		}
	}

	private bool isMaster
	{
		get
		{
			return PhotonNetwork.isMasterClient;
		}
	}

	public static bool IsFinished(State state)
	{
		return (state & State.ActiveStateMask) == 0;
	}

	public bool StartMatchmaking(string lobbyName, int maxRequiredPlayerCount, int teamCount, string levelName, string config, MatchmakingDelegate matchmakingDelegate)
	{
		if (!IsFinished(state))
		{
			return false;
		}
		_lobbyName = lobbyName;
		_maxRequiredPlayerCount = ((maxRequiredPlayerCount >= 2) ? maxRequiredPlayerCount : 2);
		_teamCount = teamCount;
		_levelName = levelName;
		_config = config;
		_matchmakingDelegate = matchmakingDelegate;
		_matchmakingStartTime = Time.time;
		_shouldForceFindMatch = false;
		StartMatchmaking();
		return true;
	}

	public void Cancel()
	{
		if (state == State.Connecting)
		{
			NetworkManager.instance.Disconnect(null);
		}
		else if (!IsFinished(state))
		{
			_state = State.Canceled;
			if (_match != null && _match.state != 0)
			{
				_match.canceledEvent -= MatchCanceled;
				_match.Cancel();
			}
			else
			{
				NetworkManager.instance.Disconnect(null);
			}
			Finish();
		}
	}

	private void Awake()
	{
		_playersMap = new Dictionary<int, InternalPlayer>();
		_playersByLeague = new List<InternalPlayer>[maxLeagueCount];
		for (int i = 0; i != maxLeagueCount; i++)
		{
			_playersByLeague[i] = new List<InternalPlayer>();
		}
		_selectedPlayers = new List<InternalPlayer>();
		_photonView = GetComponent<PhotonView>();
		_match = GetComponent<MultiplayerMatch>();
		RankedMatchmakerConfig rankedMatchmaker = MonoSingleton<GameController>.Instance.Configuration.RankedMatchmaker;
		connectionTimeout = rankedMatchmaker.ConnectionTimeout;
		forceMatchmakingTimeout = rankedMatchmaker.ForceMatchmakingTimeout;
		idleFailTimeout = rankedMatchmaker.IdleFailTimeout;
		maxPlayerCount = rankedMatchmaker.MaxPlayerCount;
	}

	private void OnDestroy()
	{
		Cancel();
	}

	private void SetStateAndNotify(State state)
	{
		_state = state;
		if (_matchmakingDelegate != null)
		{
			_matchmakingDelegate(this, state);
		}
	}

	private void StartMatchmaking()
	{
		Debug.Log("RankedMatchmaker.StartMatchmaking, state=" + state);
		Screen.sleepTimeout = -1;
		SetStateAndNotify(State.Connecting);
		NetworkManager.instance.CreateOrJoinRoom(-1, _lobbyName, lobbySize, true, connectionTimeout, JoinRoomFinished);
	}

	private void JoinRoomFinished(NetworkManager.Result result)
	{
		NetworkManager instance = NetworkManager.instance;
		GameAnalytics.EventMatchmakerConnectionFinished(result, Time.time - _matchmakingStartTime, instance.preferredServerIndex, instance.currentServerIndex);
		if (result == NetworkManager.Result.Success)
		{
			StartCoroutine(Searching());
		}
		else
		{
			Finish((result != NetworkManager.Result.Cancel) ? State.Failed : State.Canceled);
		}
	}

	private IEnumerator Searching()
	{
		float time = Time.time;
		float forceFindMatchTime = time + forceMatchmakingTimeout;
		InitPlayer();
		SetStateAndNotify(State.Searching);
		CheckPlayers();
		ShareLocalPlayer(null);
		while (state == State.Searching && Time.time < forceFindMatchTime)
		{
			yield return null;
		}
		if (state == State.Searching)
		{
			_shouldForceFindMatch = true;
			_idleFailTime = Time.time + idleFailTimeout;
			if (isMaster && _maxRequiredPlayerCount <= playerCount)
			{
				FindMatch(_localPlayer);
			}
			while (state == State.Searching && Time.time < _idleFailTime)
			{
				yield return null;
			}
			if (state == State.Searching)
			{
				Finish(State.Failed);
			}
		}
	}

	private void InitPlayer()
	{
		Player instance = MonoSingleton<Player>.Instance;
		_localPlayer = new InternalPlayer(PhotonNetwork.player.ID, instance.Name, instance.EloRate, instance.League);
		InsertPlayer(_localPlayer);
	}

	private void ShareLocalPlayer(PhotonPlayer photonPlayer)
	{
		string methodName = "MatchmakerPlayerJoined";
		byte[] array = new byte[_localPlayer.GetEncodedSize()];
		_localPlayer.Encode(array, 0);
		if (photonPlayer != null)
		{
			_photonView.RPC(methodName, photonPlayer, array);
		}
		else
		{
			_photonView.RPC(methodName, PhotonTargets.Others, array);
		}
	}

	private void OnPhotonPlayerConnected(PhotonPlayer photonPlayer)
	{
		if (state == State.Searching)
		{
			ShareLocalPlayer(photonPlayer);
		}
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer)
	{
		Debug.Log(string.Concat("RankedMatchmaker.OnPhotonPlayerDisconnected:", photonPlayer.ID, "; state:", state, "; _selectedPlayers.Count:", _selectedPlayers.Count));
		if (state != State.Searching && state != State.WaitingRemotePlayersDisconnection)
		{
			return;
		}
		int iD = photonPlayer.ID;
		InternalPlayer value;
		if (_playersMap.TryGetValue(iD, out value))
		{
			RemovePlayer(value);
		}
		if (iD == _localPlayer.id)
		{
			return;
		}
		for (int i = 0; i != _selectedPlayers.Count; i++)
		{
			if (_selectedPlayers[i].id == iD)
			{
				_selectedPlayers.RemoveAt(i);
				break;
			}
		}
	}

	private void CheckPlayers()
	{
		if (!isMaster)
		{
			return;
		}
		_idleFailTime = Time.time + idleFailTimeout;
		if (_maxRequiredPlayerCount <= playerCount)
		{
			if (_shouldForceFindMatch)
			{
				FindMatch(_localPlayer);
			}
			else if (maxPlayerCount <= playerCount)
			{
				FindMatch();
			}
		}
	}

	private string GenMatchID()
	{
		return Guid.NewGuid().ToString();
	}

	private void FindMatch()
	{
		int num = int.MaxValue;
		List<InternalPlayer> list = null;
		int num2 = 0;
		int num3 = _maxRequiredPlayerCount - 1;
		for (int i = 0; i < maxLeagueCount; i++)
		{
			List<InternalPlayer> list2 = _playersByLeague[i];
			int j = num3;
			for (int count = list2.Count; j < count; j++)
			{
				int num4 = j - num3;
				int num5 = list2[j].rating - list2[num4].rating;
				if (num5 < num)
				{
					num = num5;
					list = list2;
					num2 = num4;
				}
			}
		}
		Debug.Log("RankedMatchmaker.FindMatch: found in league:" + list[num2].league + ", from:" + num2 + " to:" + num2 + num3);
		GameAnalytics.EventMatchmakerFoundMatch(Time.time - _matchmakingStartTime, _maxRequiredPlayerCount, playerCount, 0, num);
		string text = GenMatchID();
		do
		{
			InternalPlayer internalPlayer = list[num2];
			internalPlayer.name = text;
			_selectedPlayers.Add(internalPlayer);
			RemovePlayer(internalPlayer);
			if (internalPlayer.id == _localPlayer.id)
			{
				SetStateAndNotify(State.WaitingRemotePlayersDisconnection);
			}
		}
		while (--num3 >= 0);
		if (!_isSending)
		{
			StartCoroutine(Sending());
		}
	}

	private void FindMatch(InternalPlayer player)
	{
		Debug.Log("FindMatch(local player:" + player.id + ")");
		int num = player.league;
		int num2 = num;
		int num3 = num;
		int count = _selectedPlayers.Count;
		int sentPlayerCount = 0;
		AddNearestToMatch(player, _playersByLeague[num], ref sentPlayerCount);
		if (sentPlayerCount < _maxRequiredPlayerCount)
		{
			int num4 = 1;
			while (true)
			{
				num -= num4;
				if (num < 0)
				{
					for (num += ++num4; num < _playersByLeague.Length; num++)
					{
						if (AddLeagueHeadToMatch(_playersByLeague[num], ref sentPlayerCount))
						{
							num2 = num;
							if (_maxRequiredPlayerCount <= sentPlayerCount)
							{
								break;
							}
						}
					}
					break;
				}
				if (AddLeagueTailToMatch(_playersByLeague[num], ref sentPlayerCount))
				{
					num2 = num;
					if (_maxRequiredPlayerCount <= sentPlayerCount)
					{
						break;
					}
				}
				num += ++num4;
				if (_playersByLeague.Length <= num)
				{
					num -= ++num4;
					while (0 <= num)
					{
						if (AddLeagueTailToMatch(_playersByLeague[num], ref sentPlayerCount))
						{
							num3 = num;
							if (_maxRequiredPlayerCount <= sentPlayerCount)
							{
								break;
							}
						}
						num--;
					}
					break;
				}
				if (AddLeagueHeadToMatch(_playersByLeague[num], ref sentPlayerCount))
				{
					num3 = num;
					if (_maxRequiredPlayerCount <= sentPlayerCount)
					{
						break;
					}
				}
				num4++;
			}
		}
		if (sentPlayerCount < _maxRequiredPlayerCount)
		{
			int num5 = _selectedPlayers.Count - 1;
			while (count <= num5)
			{
				_selectedPlayers.RemoveAt(num5);
				num5--;
			}
			return;
		}
		string text = GenMatchID();
		int selectionCount = playerCount;
		player = _selectedPlayers[count];
		player.name = text;
		int rating = player.rating;
		int rating2 = player.rating;
		RemovePlayer(player);
		if (player.id == _localPlayer.id)
		{
			SetStateAndNotify(State.WaitingRemotePlayersDisconnection);
		}
		for (count++; count < _selectedPlayers.Count; count++)
		{
			player = _selectedPlayers[count];
			player.name = text;
			if (player.rating < rating)
			{
				rating = player.rating;
			}
			else if (rating2 < player.rating)
			{
				rating2 = player.rating;
			}
			RemovePlayer(player);
			if (player.id == _localPlayer.id)
			{
				SetStateAndNotify(State.WaitingRemotePlayersDisconnection);
			}
		}
		GameAnalytics.EventMatchmakerFoundMatch(Time.time - _matchmakingStartTime, _maxRequiredPlayerCount, selectionCount, num3 - num2, rating2 - rating);
		if (!_isSending)
		{
			StartCoroutine(Sending());
		}
	}

	private void AddNearestToMatch(InternalPlayer player, List<InternalPlayer> leaguePlayers, ref int sentPlayerCount)
	{
		Debug.Log("RankedMatchmaker.AddNearestToMatch: playerID=" + player.id + ", sentPlayerCount=" + sentPlayerCount);
		int num = leaguePlayers.BinarySearch(player);
		if (num < 0)
		{
			return;
		}
		_selectedPlayers.Add(player);
		if (++sentPlayerCount >= _maxRequiredPlayerCount)
		{
			return;
		}
		int num2 = 1;
		while (true)
		{
			num -= num2;
			if (num < 0)
			{
				for (num += ++num2; num < leaguePlayers.Count; num++)
				{
					_selectedPlayers.Add(leaguePlayers[num]);
					if (_maxRequiredPlayerCount <= ++sentPlayerCount)
					{
						break;
					}
				}
				break;
			}
			_selectedPlayers.Add(leaguePlayers[num]);
			if (_maxRequiredPlayerCount <= ++sentPlayerCount)
			{
				break;
			}
			num += ++num2;
			if (leaguePlayers.Count <= num)
			{
				num -= ++num2;
				while (0 <= num)
				{
					_selectedPlayers.Add(leaguePlayers[num]);
					if (_maxRequiredPlayerCount <= ++sentPlayerCount)
					{
						break;
					}
					num--;
				}
				break;
			}
			_selectedPlayers.Add(leaguePlayers[num]);
			if (_maxRequiredPlayerCount <= ++sentPlayerCount)
			{
				break;
			}
			num2++;
		}
	}

	private bool AddLeagueTailToMatch(List<InternalPlayer> leaguePlayers, ref int sentPlayerCount)
	{
		if (leaguePlayers.Count != 0)
		{
			int num = leaguePlayers.Count - 1;
			do
			{
				_selectedPlayers.Add(leaguePlayers[num]);
			}
			while (++sentPlayerCount < _maxRequiredPlayerCount && 0 <= --num);
			return true;
		}
		return false;
	}

	private bool AddLeagueHeadToMatch(List<InternalPlayer> leaguePlayers, ref int sentPlayerCount)
	{
		if (leaguePlayers.Count != 0)
		{
			int num = 0;
			do
			{
				_selectedPlayers.Add(leaguePlayers[num]);
			}
			while (++sentPlayerCount < _maxRequiredPlayerCount && ++num < leaguePlayers.Count);
			return true;
		}
		return false;
	}

	private IEnumerator Sending()
	{
		float waitLimitTime = -1f;
		float interval = 1.25f / (float)PhotonNetwork.sendRate;
		YieldInstruction yieldInst = new WaitForSeconds(interval);
		int i = 0;
		while (true)
		{
			_isSending = true;
			yield return yieldInst;
			Debug.Log(_selectedPlayers.Count);
			_isSending = false;
			if (_selectedPlayers.Count == 0)
			{
				break;
			}
			if (_selectedPlayers.Count <= i)
			{
				i = 0;
			}
			InternalPlayer player = _selectedPlayers[i];
			int playerID = player.id;
			if (playerID == _localPlayer.id)
			{
				if (waitLimitTime < 0f)
				{
					waitLimitTime = Time.time + 1.5f;
				}
				if (_selectedPlayers.Count == 1 || waitLimitTime < Time.time)
				{
					_selectedPlayers.Clear();
					JoinMatch(player.name, _maxRequiredPlayerCount, _teamCount, _levelName, _config);
				}
			}
			else
			{
				PhotonPlayer photonPlayer = PhotonPlayer.Find(player.id);
				if (photonPlayer != null)
				{
					Debug.Log("Send RPC RemoteJoinMatch to: " + photonPlayer.ID + "; my id: " + PhotonNetwork.player.ID);
					_photonView.RPC("RemoteJoinMatch", photonPlayer, player.name, _maxRequiredPlayerCount, _teamCount, _levelName, _config);
				}
			}
			i++;
		}
	}

	private void JoinMatch(string matchID, int requiredPlayerCount, int teamCount, string levelName, string config)
	{
		Debug.Log("RankedMatchmaker.JoinMatch");
		InternalPlayer localPlayer = _localPlayer;
		localPlayer.name = MonoSingleton<Player>.Instance.Name;
		ClearSearching();
		SetStateAndNotify(State.Joining);
		_match.canceledEvent += MatchCanceled;
		_match.matchReadyEvent += MatchStarted;
		_match.playerConnectedEvent += MatchPlayerConnected;
		_match.playerDisconnectedEvent += MatchPlayerDisconnected;
		_match.playerReadyEvent += MatchPlayerReady;
		_match.JoinMatch(NetworkManager.instance.currentServerIndex, matchID, requiredPlayerCount, requiredPlayerCount, teamCount, levelName, config, localPlayer);
	}

	private void MatchPlayerConnected(MultiplayerMatch match, MatchPlayer player)
	{
		if (_matchmakingDelegate != null)
		{
			_matchmakingDelegate(this, _state);
		}
	}

	private void MatchPlayerReady(MultiplayerMatch match, MatchPlayer player)
	{
		if (_matchmakingDelegate != null)
		{
			_matchmakingDelegate(this, _state);
		}
		if (match.minPlayerCount <= match.readyPlayerCount)
		{
			match.StartMatch();
		}
	}

	private void MatchPlayerDisconnected(MultiplayerMatch match, MatchPlayer player)
	{
		if (_matchmakingDelegate != null)
		{
			_matchmakingDelegate(this, _state);
		}
	}

	private void MatchStarted(MultiplayerMatch match)
	{
		if (state == State.Joining)
		{
			match.canceledEvent -= MatchCanceled;
			Finish(State.Succeeded);
		}
	}

	private void MatchCanceled(MultiplayerMatch match, MultiplayerMatch.CancelReason reason)
	{
		_state = State.Failed;
		StartMatchmaking();
	}

	private void OnMasterClientSwitched(PhotonPlayer player)
	{
		if (state == State.Searching)
		{
			CheckPlayers();
		}
	}

	private void OnLeftRoom()
	{
		if (state == State.Searching || state == State.WaitingRemotePlayersDisconnection)
		{
			Finish(State.Failed);
		}
	}

	private void OnDisconnectedFromPhoton()
	{
		if (state == State.Searching || state == State.WaitingRemotePlayersDisconnection)
		{
			Finish(State.Failed);
		}
	}

	private void ClearSearching()
	{
		_localPlayer = null;
		_playerCount = 0;
		int i = 0;
		for (int num = _playersByLeague.Length; i != num; i++)
		{
			_playersByLeague[i].Clear();
		}
		_playersMap.Clear();
		_selectedPlayers.Clear();
	}

	private void Finish(State state)
	{
		_state = state;
		Finish();
	}

	private void Finish()
	{
		ClearSearching();
		_levelName = null;
		_config = null;
		if (_state != 0)
		{
			NetworkManager.instance.Disconnect(null);
		}
		if (_matchmakingDelegate != null)
		{
			MatchmakingDelegate matchmakingDelegate = _matchmakingDelegate;
			_matchmakingDelegate = null;
			matchmakingDelegate(this, _state);
		}
	}

	private void InsertPlayer(InternalPlayer player)
	{
		List<InternalPlayer> list = _playersByLeague[player.league];
		int num = list.BinarySearch(player);
		if (0 <= num)
		{
			list[num] = player;
		}
		else
		{
			InsertPlayer(player, ~num);
		}
	}

	private void InsertPlayer(InternalPlayer player, int index)
	{
		_playerCount++;
		int id = player.id;
		if (_playersMap.ContainsKey(id))
		{
			_playersMap.Remove(id);
		}
		_playersMap.Add(id, player);
		int league = player.league;
		_playersByLeague[league].Insert(index, player);
		if (_matchmakingDelegate != null)
		{
			_matchmakingDelegate(this, _state);
		}
	}

	private void RemovePlayer(InternalPlayer player)
	{
		int num = _playersByLeague[player.league].BinarySearch(player);
		if (0 <= num)
		{
			_playerCount--;
			_playersByLeague[player.league].RemoveAt(num);
			_playersMap.Remove(player.id);
			if (_matchmakingDelegate != null)
			{
				_matchmakingDelegate(this, _state);
			}
		}
	}

	[RPC]
	private void MatchmakerPlayerJoined(byte[] data, PhotonMessageInfo msgInfo)
	{
		if (state == State.Connecting || state == State.Searching)
		{
			InsertPlayer(MatchPlayer.Make(msgInfo.sender.ID, data, 0) as InternalPlayer);
			if (state == State.Searching)
			{
				CheckPlayers();
			}
		}
	}

	[RPC]
	private void RemoteJoinMatch(string matchID, int requiredPlayerCount, int teamCount, string levelName, string config)
	{
		Debug.Log("RankedMatchmaker.RemoteJoinMatch:" + levelName);
		if (!inRemoteJoinProcess)
		{
			StartCoroutine(RemoteJoinMatchProcess(matchID, requiredPlayerCount, teamCount, levelName, config));
		}
	}

	private IEnumerator RemoteJoinMatchProcess(string matchID, int requiredPlayerCount, int teamCount, string levelName, string config)
	{
		inRemoteJoinProcess = true;
		yield return new WaitForSeconds(3f);
		if (state == State.Searching)
		{
			JoinMatch(matchID, requiredPlayerCount, teamCount, levelName, config);
		}
		inRemoteJoinProcess = false;
	}
}
