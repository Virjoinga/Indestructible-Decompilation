using System;
using ExitGames.Client.Photon;
using UnityEngine;

public class NetworkManager : ScriptableObject, IPhotonPeerListener, IUpdatable
{
	public enum Result
	{
		Fail = 0,
		Cancel = 1,
		Success = 2
	}

	public enum State
	{
		Idle = 0,
		Disconnecting = 1,
		Connecting = 2,
		RoomCreating = 3,
		RoomJoining = 4
	}

	public enum OpCode
	{
		Disconnect = 1,
		Connect = 2,
		CreateRoom = 4,
		JoinRoom = 8
	}

	private const string _assetPath = "Assets/Bundles/Network/NetworkManager.asset";

	public string[] servers;

	public string appID;

	public string gameVersion;

	public int preferredServerIndex = -1;

	public int createAttemptCount = 2;

	public int joinAttemptCount = 3;

	private Action<Result> _finishDelegate;

	private State _state;

	private float _failTime;

	private OpCode _requestOpCode;

	private string _roomName;

	private int _maxPlayerCount;

	private int _bestPing;

	private int _bestServerIndex;

	private int _lastBestServerIndex;

	private int _currentServerIndex;

	private int _currentAttempt;

	private bool _isUpdating;

	private bool _autoCleanUpPlayerObjects;

	private bool _isOperationFailed;

	private static NetworkManager _instance;

	public static NetworkManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = BundlesUtils.Load("Assets/Bundles/Network/NetworkManager.asset") as NetworkManager;
				_instance.Init();
			}
			return _instance;
		}
	}

	public static bool isExists
	{
		get
		{
			return _instance != null;
		}
	}

	public State state
	{
		get
		{
			return _state;
		}
	}

	public OpCode requestOpCode
	{
		get
		{
			return _requestOpCode;
		}
	}

	public int currentServerIndex
	{
		get
		{
			return _currentServerIndex;
		}
	}

	public void Disconnect(Action<Result> finishDelegate)
	{
		Debug.Log("NetworkManager.Disconnect");
		if (_state != 0 && _requestOpCode != OpCode.Disconnect)
		{
			Cancel();
		}
		if (PhotonNetwork.connectionState != 0 && (PhotonNetwork.connectionState != ConnectionState.Disconnecting || _requestOpCode != OpCode.Disconnect || _state == State.Disconnecting))
		{
			StartRequest(0, OpCode.Disconnect, null, 0, false, 0f, finishDelegate);
		}
		else if (finishDelegate != null)
		{
			Debug.Log("NetworkManager.Disconnect {finishDelegate(Success);}");
			finishDelegate(Result.Success);
		}
	}

	public void Connect(int serverIndex, float timeout, Action<Result> finishDelegate)
	{
		Debug.Log("NetworkManager.Connect");
		if (_state != 0)
		{
			Cancel();
		}
		if (PhotonNetwork.room != null || PhotonNetwork.connectionStateDetailed != PeerState.Authenticated)
		{
			StartRequest(serverIndex, OpCode.Connect, null, 0, false, timeout, finishDelegate);
		}
		else if (finishDelegate != null)
		{
			Debug.Log("NetworkManager.Connect {finishDelegate(Success);}");
			finishDelegate(Result.Success);
		}
	}

	public void CreateRoom(int serverIndex, string roomName, int maxPlayerCount, bool autoCleanUpPlayerObjects, float timeout, Action<Result> finishDelegate)
	{
		Debug.Log("NetworkManager.CreateRoom");
		if (_state != 0)
		{
			Cancel();
		}
		if (PhotonNetwork.connectionStateDetailed != PeerState.Joined || PhotonNetwork.room == null || PhotonNetwork.room.name != roomName)
		{
			StartRequest(serverIndex, OpCode.CreateRoom, roomName, maxPlayerCount, autoCleanUpPlayerObjects, timeout, finishDelegate);
		}
		else if (finishDelegate != null)
		{
			Debug.Log("NetworkManager.CreateRoom {finishDelegate(Success);}");
			finishDelegate(Result.Success);
		}
	}

	public void JoinRoom(int serverIndex, string roomName, bool autoCleanUpPlayerObjects, float timeout, Action<Result> finishDelegate)
	{
		Debug.Log("NetworkManager.JoinRoom");
		if (PhotonNetwork.connectionStateDetailed != PeerState.Joined || PhotonNetwork.room == null || PhotonNetwork.room.name != roomName)
		{
			if (_state != 0)
			{
				Cancel();
			}
			StartRequest(serverIndex, OpCode.JoinRoom, roomName, 0, autoCleanUpPlayerObjects, timeout, finishDelegate);
			return;
		}
		if (_state != 0 && _state != State.RoomJoining)
		{
			Cancel();
		}
		if (finishDelegate != null)
		{
			Debug.Log("NetworkManager.JoinRoom {finishDelegate(Success);}");
			finishDelegate(Result.Success);
		}
	}

	public void CreateOrJoinRoom(int serverIndex, string roomName, int maxPlayerCount, bool autoCleanUpPlayerObjects, float timeout, Action<Result> finishDelegate)
	{
		Debug.Log("NetworkManager.CreateOrJoinRoom");
		if (PhotonNetwork.connectionStateDetailed != PeerState.Joined || PhotonNetwork.room == null || PhotonNetwork.room.name != roomName)
		{
			if (_state != 0)
			{
				Cancel();
			}
			StartRequest(serverIndex, (OpCode)12, roomName, maxPlayerCount, autoCleanUpPlayerObjects, timeout, finishDelegate);
			return;
		}
		if (_state != 0 && _state != State.RoomCreating && _state != State.RoomJoining)
		{
			Cancel();
		}
		if (finishDelegate != null)
		{
			Debug.Log("NetworkManager.CreateOrJoinRoom {finishDelegate(Success);}");
			finishDelegate(Result.Success);
		}
	}

	public void Cancel()
	{
		if (_state != 0)
		{
			Debug.Log("NetworkManager.Cancel()");
			Finish(Result.Cancel);
		}
	}

	private void Init()
	{
		PhotonNetwork.autoJoinLobby = false;
		PhotonNetwork.photonMono.externalListener = this;
	}

	private void StartRequest(int serverIndex, OpCode request, string roomName, int maxPlayerCount, bool autoCleanUpPlayerObjects, float timeout, Action<Result> finishDelegate)
	{
		_finishDelegate = finishDelegate;
		_requestOpCode = request;
		_roomName = roomName;
		_maxPlayerCount = maxPlayerCount;
		_autoCleanUpPlayerObjects = autoCleanUpPlayerObjects;
		_failTime = Time.time + timeout;
		PhotonNetwork.offlineMode = false;
		PhotonNetwork.networkingPeer.RemoveAllInstantiatedObjects();
		_state = State.Disconnecting;
		if (0 <= serverIndex && serverIndex < servers.Length)
		{
			_currentServerIndex = serverIndex;
			_bestPing = -1;
		}
		else if (0 <= preferredServerIndex && preferredServerIndex < servers.Length)
		{
			_currentServerIndex = preferredServerIndex;
			_bestPing = -1;
		}
		else
		{
			_bestPing = int.MaxValue;
			_bestServerIndex = 0;
			_currentServerIndex = ((_lastBestServerIndex == 0 && 1 < servers.Length) ? 1 : 0);
		}
		if (PhotonNetwork.connectionState != 0)
		{
			PhotonNetwork.Disconnect();
		}
		if (!_isUpdating)
		{
			_isUpdating = true;
			MonoSingleton<UpdateAgent>.Instance.StartUpdate(this);
		}
	}

	private void Finish(Result result)
	{
		Debug.Log("NetworkManager.Finish:" + result);
		_state = State.Idle;
		if (_finishDelegate != null)
		{
			Debug.Log(string.Concat("NetworkManager.Finish {finishDelegate(", result, ");}"));
			Action<Result> finishDelegate = _finishDelegate;
			_finishDelegate = null;
			finishDelegate(result);
		}
		if (_state == State.Idle)
		{
			_isUpdating = false;
		}
	}

	public bool DoUpdate()
	{
		if (_state == State.Disconnecting)
		{
			if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
			{
				if (_requestOpCode == OpCode.Disconnect)
				{
					Finish(Result.Success);
				}
				else
				{
					_state = State.Connecting;
					Debug.Log("NetworkManager.Update: Start connecting to " + servers[_currentServerIndex]);
					PhotonNetwork.Connect(servers[_currentServerIndex], 5055, appID, gameVersion);
				}
			}
		}
		else if (_state != 0)
		{
			if (_failTime < Time.time)
			{
				Finish(Result.Fail);
			}
			else
			{
				switch (_state)
				{
				case State.Connecting:
					if (PhotonNetwork.connectionStateDetailed == PeerState.ConnectedToMaster || PhotonNetwork.connectionStateDetailed == PeerState.Authenticated)
					{
						if (0 <= _bestPing)
						{
							int ping = PhotonNetwork.GetPing();
							Debug.Log("NetworkManager.Update: ping=" + ping + ", bestPing=" + _bestPing);
							if (ping < _bestPing)
							{
								_bestPing = ping;
								_bestServerIndex = _currentServerIndex;
							}
							if (_currentServerIndex != _lastBestServerIndex)
							{
								_currentServerIndex++;
								if (servers.Length <= _currentServerIndex)
								{
									_currentServerIndex = _lastBestServerIndex;
								}
								else if (_currentServerIndex == _lastBestServerIndex && _currentServerIndex < servers.Length - 1)
								{
									_currentServerIndex++;
								}
								_state = State.Disconnecting;
								PhotonNetwork.Disconnect();
								break;
							}
							_bestPing = -1;
							_lastBestServerIndex = _bestServerIndex;
							if (_currentServerIndex != _bestServerIndex)
							{
								_currentServerIndex = _bestServerIndex;
								_state = State.Disconnecting;
								PhotonNetwork.Disconnect();
								break;
							}
						}
						PhotonNetwork.sendRate = 20;
						PhotonNetwork.sendRateOnSerialize = 20;
						if ((_requestOpCode & (OpCode)12) == 0)
						{
							Finish(Result.Success);
							break;
						}
						PhotonNetwork.autoCleanUpPlayerObjects = _autoCleanUpPlayerObjects;
						_currentAttempt = 1;
						_isOperationFailed = false;
						if ((_requestOpCode & OpCode.CreateRoom) != 0)
						{
							_state = State.RoomCreating;
							PhotonNetwork.CreateRoom(_roomName, false, true, _maxPlayerCount);
						}
						else
						{
							_state = State.RoomJoining;
							PhotonNetwork.JoinRoom(_roomName);
						}
					}
					else if (PhotonNetwork.connectionStateDetailed != PeerState.Connecting && PhotonNetwork.connectionStateDetailed != PeerState.Connected && PhotonNetwork.connectionStateDetailed != PeerState.ConnectingToMasterserver)
					{
						Finish(Result.Fail);
					}
					break;
				case State.RoomCreating:
					if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
					{
						Finish(Result.Success);
					}
					else if (_isOperationFailed)
					{
						if (_currentAttempt < createAttemptCount)
						{
							_currentAttempt++;
							_isOperationFailed = false;
							PhotonNetwork.CreateRoom(_roomName, false, true, _maxPlayerCount);
						}
						else if ((_requestOpCode & OpCode.JoinRoom) != 0)
						{
							_state = State.RoomJoining;
							_currentAttempt = 1;
							_isOperationFailed = false;
							PhotonNetwork.JoinRoom(_roomName);
						}
						else
						{
							Finish(Result.Fail);
						}
					}
					break;
				case State.RoomJoining:
					if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
					{
						Finish(Result.Success);
					}
					else if (_isOperationFailed)
					{
						if (_currentAttempt < joinAttemptCount)
						{
							_currentAttempt++;
							_isOperationFailed = false;
							PhotonNetwork.JoinRoom(_roomName);
						}
						else
						{
							Finish(Result.Fail);
						}
					}
					break;
				}
			}
		}
		return _isUpdating;
	}

	public void DebugReturn(DebugLevel level, string message)
	{
	}

	public void OnOperationResponse(OperationResponse operationResponse)
	{
		if (operationResponse.ReturnCode != 0 && ((operationResponse.OperationCode == 227 && _state == State.RoomCreating) || (operationResponse.OperationCode == 226 && _state == State.RoomJoining)))
		{
			_isOperationFailed = true;
		}
	}

	public void OnStatusChanged(StatusCode statusCode)
	{
	}

	public void OnEvent(EventData photonEvent)
	{
	}
}
