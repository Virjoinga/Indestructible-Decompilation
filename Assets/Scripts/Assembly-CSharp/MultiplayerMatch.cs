using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Indestructible/Network/Multiplayer Match")]
public class MultiplayerMatch : MatchPlayer.MatchBase
{
	public enum CancelReason
	{
		General = 0,
		JoinFailed = 1,
		Disconnection = 2
	}

	public enum State
	{
		Inactive = 0,
		Connecting = 1,
		Joining = 2,
		Joined = 4,
		LevelReady = 12,
		Started = 28
	}

	public enum Targets
	{
		All = 0,
		Others = 1
	}

	private class InternalTeam : MatchTeam
	{
		public InternalTeam(int id)
			: base(id)
		{
		}

		public void AddPlayer(MatchPlayer player)
		{
			_players.Add(player);
			if (!player.isDisconnected)
			{
				_connectedPlayerCount++;
			}
		}

		public void RemovePlayer(MatchPlayer player)
		{
			if (!player.isDisconnected)
			{
				_connectedPlayerCount--;
			}
			_players.Remove(player);
		}

		public void PlayerDisconnected(MatchPlayer player)
		{
			_connectedPlayerCount--;
		}
	}

	public delegate void PlayerDelegate(MultiplayerMatch match, MatchPlayer actor);

	public delegate void CaneledDelegate(MultiplayerMatch game, CancelReason reason);

	public delegate void ChatMessageRecievedDelegate(string message, MatchPlayer sender);

	public float joinTimeout = 20f;

	public float afterLoadTimeout = 2f;

	public float maxStartTimeout = 15f;

	private PhotonView _photonView;

	private MatchPlayer _localPlayer;

	private InternalTeam _localTeam;

	private Dictionary<int, MatchPlayer> _players;

	private InternalTeam[] _teams;

	private State _state;

	private uint _serverStartTimeInMilliSeconds;

	private string _levelName;

	private string _config;

	private int _minPlayerCount;

	private int _teamCount;

	private int _connectedPlayerCount;

	private int _readyPlayerCount;

	private float _joinFailTime;

	private float _lastMatchReadyTimestamp;

	private int _readyMatchCount;

	private bool _isOnline;

	private bool _forceDestroyOnLoad;

	public State state
	{
		get
		{
			return _state;
		}
	}

	public bool isOnline
	{
		get
		{
			return _isOnline;
		}
	}

	public uint serverStartTimeInMilliSeconds
	{
		get
		{
			return _serverStartTimeInMilliSeconds;
		}
	}

	public MatchPlayer localPlayer
	{
		get
		{
			return _localPlayer;
		}
	}

	public IEnumerable<MatchPlayer> players
	{
		get
		{
			return _players.Values;
		}
	}

	public int playerCount
	{
		get
		{
			return _players.Count;
		}
	}

	public int connectedPlayerCount
	{
		get
		{
			return _connectedPlayerCount;
		}
	}

	public MatchTeam localTeam
	{
		get
		{
			return _localTeam;
		}
	}

	public IEnumerable<MatchTeam> teams
	{
		get
		{
			return _teams;
		}
	}

	public int readyPlayerCount
	{
		get
		{
			return _readyPlayerCount;
		}
	}

	public int minPlayerCount
	{
		get
		{
			return _minPlayerCount;
		}
	}

	public int teamCount
	{
		get
		{
			return _teamCount;
		}
	}

	public PhotonView photonView
	{
		get
		{
			return _photonView;
		}
	}

	public string levelName
	{
		get
		{
			return _levelName;
		}
	}

	public string config
	{
		get
		{
			return _config;
		}
	}

	public event PlayerDelegate playerConnectedEvent;

	public event PlayerDelegate playerReadyEvent;

	public event PlayerDelegate playerDisconnectedEvent;

	public event Action<MultiplayerMatch> matchReadyEvent;

	public event Action<MultiplayerMatch> matchStartedEvent;

	public event CaneledDelegate canceledEvent;

	public event ChatMessageRecievedDelegate chatMessageRecievedEvent;

	public bool TryGetPlayer(int id, out MatchPlayer player)
	{
		return _players.TryGetValue(id, out player);
	}

	public MatchTeam GetTeam(int id)
	{
		return _teams[id];
	}

	public void JoinMatch(int serverIndex, string matchID, int minPlayerCount, int maxPlayerCount, int teamCount, string levelName, string config, MatchPlayer player)
	{
		Debug.Log(string.Format("MultiplayerMatch.JoinMatch({0}, {1}, {2}, {3}, {4}, {5}, {6})", serverIndex, matchID, minPlayerCount, maxPlayerCount, teamCount, levelName, config));
		if (state != 0)
		{
			return;
		}
		Clear();
		PrepareMatch(minPlayerCount, teamCount, levelName, config);
		_joinFailTime = Time.time + joinTimeout;
		_state = State.Connecting;
		_isOnline = true;
		NetworkManager.instance.CreateOrJoinRoom(serverIndex, matchID, maxPlayerCount, false, joinTimeout, delegate(NetworkManager.Result result)
		{
			Debug.Log("MultiplayerMatch.JoinRoomFinished: " + result);
			if (result == NetworkManager.Result.Success)
			{
				_state = State.Joining;
				JoinLocalPlayer(player);
				StartCoroutine(JoiningProcess());
			}
			else
			{
				Cancel(CancelReason.JoinFailed);
			}
		});
	}

	public void CreateOfflineMatch(int minPlayerCount, int maxPlayerCount, int teamCount, string levelName, string config, MatchPlayer player)
	{
		Debug.Log(string.Format("MultiplayerMatch.CreateOfflineMatch({0}, {1}, {2}, {3}, {4})", minPlayerCount, maxPlayerCount, teamCount, levelName, config));
		if (state == State.Inactive)
		{
			Clear();
			NetworkManager.instance.Disconnect(null);
			PhotonNetwork.offlineMode = true;
			PrepareMatch(minPlayerCount, teamCount, levelName, config);
			_state = State.Joining;
			JoinLocalPlayer(player);
		}
	}

	public void JoinPlayer(MatchPlayer player, int teamID, bool isReady)
	{
		Debug.Log("MultiplayerMatch.JoinPlayer:" + player.id + "; " + teamID + "; " + isReady + "; " + player.name);
		if ((state == State.Connecting || state == State.Joining) && !_players.ContainsKey(player.id))
		{
			AddPlayer(player, teamID, isReady);
		}
	}

	private void Awake()
	{
		_players = new Dictionary<int, MatchPlayer>();
		_photonView = GetComponent<PhotonView>();
	}

	private void Clear()
	{
		Debug.Log("MultiplayerMatch.Clear");
		_isOnline = false;
		_localTeam = null;
		_localPlayer = null;
		_players.Clear();
		_connectedPlayerCount = 0;
		_readyPlayerCount = 0;
		_lastMatchReadyTimestamp = 0f;
		_readyMatchCount = 0;
		_levelName = null;
		_config = null;
		if (_teams != null)
		{
			int i = 0;
			for (int num = _teams.Length; i != num; i++)
			{
				_teams[i].Clear();
			}
		}
	}

	private void ClearEvents()
	{
		Debug.Log("MultiplayerMatch.ClearEvents");
		this.playerReadyEvent = null;
		this.matchReadyEvent = null;
		this.canceledEvent = null;
		this.chatMessageRecievedEvent = null;
	}

	private void OnDestroy()
	{
		Debug.Log(string.Format("MultiplayerMatch.OnDestroy: status={0}, isOnline={1}", _state, isOnline));
		_state = State.Inactive;
		if (isOnline)
		{
			NetworkManager.instance.Disconnect(null);
		}
	}

	private void PrepareMatch(int minPlayerCount, int teamCount, string levelName, string config)
	{
		_minPlayerCount = minPlayerCount;
		_teamCount = teamCount;
		_levelName = levelName;
		_config = config;
		PrepareTeams();
	}

	private void PrepareTeams()
	{
		int num = ((_teams != null) ? _teams.Length : 0);
		Array.Resize(ref _teams, _teamCount);
		for (int i = num; i < _teamCount; i++)
		{
			_teams[i] = new InternalTeam(i);
		}
	}

	private IEnumerator JoiningProcess()
	{
		while (state == State.Joining && Time.time < _joinFailTime)
		{
			yield return null;
		}
		if (state == State.Joining)
		{
			Cancel(CancelReason.JoinFailed);
		}
	}

	private void JoinLocalPlayer(MatchPlayer player)
	{
		Debug.Log("MultiplayerMatch.JoinLocalPlayer");
		if (isOnline)
		{
			MatchPlayer.MatchBase.SetID(player, PhotonNetwork.player.ID);
		}
		_localPlayer = player;
		AddPlayer(player, player.teamID, player.isReady);
		if (isOnline)
		{
			ShareLocalPlayer(null);
		}
	}

	private void OnPhotonPlayerConnected(PhotonPlayer photonPlayer)
	{
		if (state == State.Joining && isOnline && _localPlayer != null)
		{
			Debug.Log("MultiplayerMatch.OnPhotonPlayerConnected");
			ShareLocalPlayer(photonPlayer);
		}
	}

	private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer)
	{
		if (state == State.Inactive || !isOnline)
		{
			return;
		}
		Debug.Log("MultiplayerMatch.OnPhotonPlayerDisconnected");
		MatchPlayer player;
		if (TryGetPlayer(photonPlayer.ID, out player))
		{
			if ((state & State.Started) == State.Started)
			{
				DisconnectPlayer(player);
			}
			else
			{
				RemovePlayer(player);
			}
			if (this.playerDisconnectedEvent != null)
			{
				this.playerDisconnectedEvent(this, player);
			}
		}
	}

	private void DisconnectPlayer(MatchPlayer player)
	{
		Debug.Log("MultiplayerMatch.DisconnectPlayer: " + player.id);
		_connectedPlayerCount--;
		MatchPlayer.MatchBase.SetIsDisconnected(player, true);
		if (0 <= player.teamID)
		{
			_teams[player.teamID].PlayerDisconnected(player);
		}
	}

	private void RemovePlayer(MatchPlayer player)
	{
		Debug.Log("MultiplayerMatch.RemovePlayer: " + player.id);
		if (player.isReady)
		{
			_readyPlayerCount--;
		}
		if (!player.isDisconnected)
		{
			_connectedPlayerCount--;
		}
		_players.Remove(player.id);
		if (0 <= player.teamID)
		{
			_teams[player.teamID].RemovePlayer(player);
		}
	}

	private void ShareLocalPlayer(PhotonPlayer photonPlayer)
	{
		string methodName = "RemotePlayerJoined";
		byte[] array = new byte[_localPlayer.GetEncodedSize()];
		_localPlayer.Encode(array, 0);
		if (photonPlayer != null)
		{
			_photonView.RPC(methodName, photonPlayer, array, _localPlayer.teamID, _localPlayer.isReady);
		}
		else
		{
			_photonView.RPC(methodName, PhotonTargets.Others, array, _localPlayer.teamID, _localPlayer.isReady);
		}
	}

	//[RPC]
	private void RemotePlayerJoined(byte[] data, int teamID, bool isReady, PhotonMessageInfo msgInfo)
	{
		Debug.Log("MultiplayerMatch.RemotePlayerJoined:" + msgInfo.sender.ID + "; " + teamID + "; " + isReady);
		if (isOnline && (state == State.Connecting || state == State.Joining))
		{
			JoinPlayer(MatchPlayer.Make(msgInfo.sender.ID, data, 0), teamID, isReady);
		}
	}

	private void AddPlayer(MatchPlayer player, int teamID, bool isReady)
	{
		Debug.Log("MultiplayerMatch.AddPlayer:" + player.id + "; " + player.name);
		_players.Add(player.id, player);
		_connectedPlayerCount++;
		if (this.playerConnectedEvent != null)
		{
			this.playerConnectedEvent(this, player);
		}
		if (0 <= teamID)
		{
			AssignTeam(player, teamID);
		}
		if (isReady && !player.isReady)
		{
			Ready(player);
		}
		if (minPlayerCount > playerCount)
		{
			return;
		}
		if (0 < teamCount)
		{
			if (PhotonNetwork.isMasterClient)
			{
				AssignTeams();
			}
		}
		else
		{
			Ready();
		}
	}

	private void AssignTeams()
	{
		int[] array = new int[teamCount];
		foreach (MatchPlayer player in players)
		{
			if (0 <= player.teamID)
			{
				array[player.teamID]++;
			}
		}
		foreach (MatchPlayer player2 in players)
		{
			if (player2.teamID >= 0)
			{
				continue;
			}
			int num = int.MaxValue;
			int num2 = 0;
			for (int i = 0; i < teamCount; i++)
			{
				if (array[i] < num)
				{
					num = array[i];
					num2 = i;
				}
			}
			array[num2]++;
			AssignTeam(player2, num2);
			if (isOnline)
			{
				_photonView.RPC("RemoteAssignTeam", PhotonTargets.Others, player2.id, num2);
			}
		}
	}

	private void AssignTeam(MatchPlayer player, int teamID)
	{
		Debug.Log("MultiplayerMatch.AssignTeam: pid:" + player.id + "; tid:" + teamID);
		MatchPlayer.MatchBase.SetTeamID(player, teamID);
		InternalTeam internalTeam = _teams[teamID];
		internalTeam.AddPlayer(player);
		if (player == _localPlayer)
		{
			_localTeam = internalTeam;
		}
		if (_localPlayer == null || _localPlayer.isReady)
		{
			return;
		}
		int num = 0;
		foreach (MatchPlayer player2 in players)
		{
			if (player2.teamID < 0)
			{
				return;
			}
			num++;
		}
		if (minPlayerCount <= num)
		{
			Ready();
		}
	}

	//[RPC]
	private void RemoteAssignTeam(int playerID, int teamID)
	{
		Debug.Log("MultiplayerMatch.RemoteAssignTeam: pid:" + playerID + "; tid:" + teamID);
		MatchPlayer player;
		if (state == State.Joining && isOnline && TryGetPlayer(playerID, out player))
		{
			AssignTeam(player, teamID);
		}
	}

	private void Ready()
	{
		Debug.Log("MultiplayerMatch.Ready");
		if (isOnline)
		{
			_photonView.RPC("RemotePlayerReady", PhotonTargets.Others);
			Ready(_localPlayer);
			return;
		}
		foreach (MatchPlayer player in players)
		{
			Ready(player);
		}
	}

	private void Ready(MatchPlayer player)
	{
		Debug.Log("MultiplayerMatch.Ready:" + player.id);
		MatchPlayer.MatchBase.SetIsReady(player, true);
		_readyPlayerCount++;
		if (this.playerReadyEvent != null)
		{
			this.playerReadyEvent(this, player);
		}
	}

	//[RPC]
	private void RemotePlayerReady(PhotonMessageInfo msgInfo)
	{
		Debug.Log("MultiplayerMatch.RemotePlayerReady:" + msgInfo.sender.ID);
		MatchPlayer player;
		if (state == State.Joining && isOnline && TryGetPlayer(msgInfo.sender.ID, out player) && !player.isReady)
		{
			Ready(player);
		}
	}

	public void StartMatch()
	{
		Debug.Log("MultiplayerMatch.StartMatch");
		if (state != State.Joining)
		{
			return;
		}
		_state = State.Joined;
		if (isOnline)
		{
			_photonView.RPC("RemoteStartMatch", PhotonTargets.Others);
			if (PhotonNetwork.isMasterClient)
			{
				GameAnalytics.EventRoomCreated();
			}
			else
			{
				GameAnalytics.EventRoomJoined();
			}
		}
		this.playerReadyEvent = null;
		if (this.matchReadyEvent != null)
		{
			this.matchReadyEvent(this);
			this.matchReadyEvent = null;
		}
		StartCoroutine(LoadMatchLevel());
	}

	//[RPC]
	private void RemoteStartMatch()
	{
		Debug.Log("MultiplayerMatch.RemoteStartMatch");
		if (state == State.Joining && isOnline)
		{
			StartMatch();
		}
	}

	private IEnumerator LoadMatchLevel()
	{
		if (isOnline)
		{
			yield return new WaitForSeconds(2f / (float)PhotonNetwork.sendRate);
			PhotonNetwork.isMessageQueueRunning = false;
		}
		_forceDestroyOnLoad = false;
		UnityEngine.Object.DontDestroyOnLoad(this);
		Debug.Log("MultiplayerMatch.LoadMatchLevel:" + _levelName);
		MonoSingleton<Player>.Instance.StartMatchLevel(_levelName, MatchLevelLoaded);
	}

	private void OnLevelWasLoaded(int level)
	{
		Debug.Log(string.Format("MultiplayerMatch.OnLevelWasLoaded: level={0}, _forceDestroyOnLoad={1}, status={2}, loadedLevelName={3}, matchLevelName={4}, _config={5}", level, _forceDestroyOnLoad, state, Application.loadedLevelName, _levelName, _config));
		if (_forceDestroyOnLoad)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void MatchLevelLoaded(string loadedLevelName)
	{
		Debug.Log(string.Format("MultiplayerMatch.MatchLevelLoaded: _forceDestroyOnLoad={0}, status={1}, loadedLevelName={2}, matchLevelName={3}, _config={4}", _forceDestroyOnLoad, state, loadedLevelName, _levelName, _config));
		if (state == State.Joined && loadedLevelName == _levelName)
		{
			StartCoroutine(MatchLevelInitializing());
		}
	}

	private IEnumerator MatchLevelInitializing()
	{
		Debug.Log("MultiplayerMatch.MatchLevelLoaded");
		_forceDestroyOnLoad = true;
		Configure();
		yield return new WaitForEndOfFrame();
		_state = State.LevelReady;
		if (isOnline)
		{
			PhotonNetwork.isMessageQueueRunning = true;
			_photonView.RPC("MatchReady", PhotonTargets.All);
			StartCoroutine(StartLoadedMatch(maxStartTimeout));
		}
		else
		{
			StartCoroutine(StartLoadedMatch(0f));
		}
	}

	private void Configure()
	{
		Debug.Log("MultiplayerMatch.Configure:" + _config);
		if (!string.IsNullOrEmpty(_config))
		{
			GameModeConf gameModeConf = BundlesUtils.Load("Assets/Bundles/" + _config + ".asset") as GameModeConf;
			if (gameModeConf != null)
			{
				gameModeConf.Configure(this);
			}
		}
	}

	//[RPC]
	private void MatchReady(PhotonMessageInfo msgInfo)
	{
		Debug.Log("MultiplayerMatch.MatchReady: " + msgInfo.sender.ID + ", timestamp=" + msgInfo.timestamp);
		if (msgInfo.sender.isMasterClient)
		{
			_serverStartTimeInMilliSeconds = (uint)(msgInfo.timestamp * 1000.0);
		}
		if ((state & State.Joined) != 0 && isOnline)
		{
			float num = (float)msgInfo.timestamp;
			if (_lastMatchReadyTimestamp < num)
			{
				_lastMatchReadyTimestamp = num;
			}
			if (playerCount <= ++_readyMatchCount)
			{
				Debug.Log("MultiplayerMatch.MatchReady: lastTimestamp=" + _lastMatchReadyTimestamp);
				StartCoroutine(StartLoadedMatch(_lastMatchReadyTimestamp + afterLoadTimeout - (float)PhotonNetwork.time));
			}
		}
	}

	private IEnumerator StartLoadedMatch(float delay)
	{
		Debug.Log("MultiplayerMatch.StartLoadedMatch(" + delay + ")");
		yield return new WaitForSeconds(delay);
		if (state == State.LevelReady)
		{
			StartLoadedMatch();
		}
	}

	private void StartLoadedMatch()
	{
		Debug.Log("MultiplayerMatch.StartLoadedMatch");
		_state = State.Started;
		if (this.matchStartedEvent != null)
		{
			this.matchStartedEvent(this);
			this.matchStartedEvent = null;
		}
	}

	public void Cancel()
	{
		Cancel(CancelReason.General);
	}

	private void Cancel(CancelReason reason)
	{
		Debug.Log("MultiplayerMatch.Cancel: " + reason);
		CaneledDelegate caneledDelegate = this.canceledEvent;
		bool flag = isOnline;
		Clear();
		ClearEvents();
		if (state != 0)
		{
			if (reason == CancelReason.Disconnection && (state & State.Joined) != 0)
			{
				GameAnalytics.EventMultiplayerDisconnect();
			}
			_state = State.Inactive;
			if (flag && reason != CancelReason.Disconnection)
			{
				NetworkManager.instance.Disconnect(null);
			}
			if (caneledDelegate != null)
			{
				caneledDelegate(this, reason);
			}
		}
	}

	private void OnLeftRoom()
	{
		if (isOnline && state != 0 && state != State.Connecting)
		{
			Debug.Log("MultiplayerMatch.OnLeftRoom");
			Cancel(CancelReason.Disconnection);
		}
	}

	private void OnDisconnectedFromPhoton()
	{
		Debug.Log("MultiplayerMatch.OnDisconnectedFromPhoton");
		OnLeftRoom();
	}

	public void SendChatMessage(Targets targets, string message)
	{
		_photonView.RPC("ChatMessage", (targets == Targets.Others) ? PhotonTargets.Others : PhotonTargets.All, message);
	}

	public bool SendChatMessage(MatchPlayer player, string message)
	{
		PhotonPlayer photonPlayer = PhotonPlayer.Find(player.id);
		if (photonPlayer != null)
		{
			_photonView.RPC("ChatMessage", photonPlayer, message);
			return true;
		}
		return false;
	}

	//[RPC]
	private void ChatMessage(string message, PhotonMessageInfo msgInfo)
	{
		if (this.chatMessageRecievedEvent != null)
		{
			MatchPlayer player;
			TryGetPlayer(msgInfo.sender.ID, out player);
			this.chatMessageRecievedEvent(message, player);
		}
	}
}
