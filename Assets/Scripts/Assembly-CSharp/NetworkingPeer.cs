using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;
using UnityEngine;

internal class NetworkingPeer : LoadbalancingPeer, IPhotonPeerListener
{
	private class InstantiatedPhotonViewSetup
	{
		public PhotonViewID[] viewIDs;

		public int group;

		public object[] instantiationData;
	}

	public string mAppVersion;

	private string mAppId;

	private byte nodeId;

	private string masterServerAddress;

	private string playername = string.Empty;

	private IPhotonPeerListener externalListener;

	private JoinType mLastJoinType;

	private bool mPlayernameHasToBeUpdated;

	public Dictionary<int, PhotonPlayer> mActors = new Dictionary<int, PhotonPlayer>();

	public PhotonPlayer[] mOtherPlayerListCopy = new PhotonPlayer[0];

	public PhotonPlayer[] mPlayerListCopy = new PhotonPlayer[0];

	public PhotonPlayer mMasterClient;

	public bool requestSecurity = true;

	private Dictionary<Type, List<MethodInfo>> monoRPCMethodsCache = new Dictionary<Type, List<MethodInfo>>();

	private ushort cacheInstantiationCount;

	public Dictionary<string, RoomInfo> mGameList = new Dictionary<string, RoomInfo>();

	public RoomInfo[] mGameListCopy = new RoomInfo[0];

	public bool insideLobby;

	public Dictionary<int, GameObject> instantiatedObjects = new Dictionary<int, GameObject>();

	private List<int> blockReceivingGroups = new List<int>();

	private List<int> blockSendingGroups = new List<int>();

	private Dictionary<int, PhotonView> photonViewList = new Dictionary<int, PhotonView>();

	private short currentLevelPrefix = -1;

	public Dictionary<int, PhotonViewID> allocatedIDs = new Dictionary<int, PhotonViewID>();

	private List<InstantiatedPhotonViewSetup> instantiatedPhotonViewSetupList = new List<InstantiatedPhotonViewSetup>();

	private Dictionary<Type, Dictionary<PhotonNetworkingMessage, MethodInfo>> cachedMethods = new Dictionary<Type, Dictionary<PhotonNetworkingMessage, MethodInfo>>();

	public string PlayerName
	{
		get
		{
			return playername;
		}
		set
		{
			if (!string.IsNullOrEmpty(value) && !value.Equals(playername))
			{
				if (mLocalActor != null)
				{
					mLocalActor.name = value;
				}
				playername = value;
				if (mCurrentGame != null)
				{
					SendPlayerName();
				}
			}
		}
	}

	public PeerState State { get; internal set; }

	public Room mCurrentGame
	{
		get
		{
			if (mRoomToGetInto != null && mRoomToGetInto.isLocalClientInside)
			{
				return mRoomToGetInto;
			}
			return null;
		}
	}

	internal Room mRoomToGetInto { get; set; }

	public PhotonPlayer mLocalActor { get; internal set; }

	public string mGameserver { get; internal set; }

	public int mQueuePosition { get; internal set; }

	public int mMasterCount { get; internal set; }

	public int mGameCount { get; internal set; }

	public int mPeerCount { get; internal set; }

	public NetworkingPeer(IPhotonPeerListener listener, string playername, ConnectionProtocol connectionProtocol)
		: base(listener, connectionProtocol)
	{
		base.Listener = this;
		externalListener = listener;
		PlayerName = playername;
		mLocalActor = new PhotonPlayer(true, -1, this.playername);
		AddNewPlayer(mLocalActor.ID, mLocalActor);
		State = global::PeerState.PeerCreated;
	}

	public override bool Connect(string serverAddress, string appID, byte nodeId)
	{
		if (PhotonNetwork.connectionStateDetailed == global::PeerState.Disconnecting)
		{
			Debug.LogError("ERROR: Cannot connect to Photon while Disconnecting. Connection failed.");
			return false;
		}
		if (string.IsNullOrEmpty(masterServerAddress))
		{
			masterServerAddress = serverAddress;
		}
		mAppId = appID;
		bool flag = base.Connect(serverAddress, string.Empty, nodeId);
		State = ((!flag) ? global::PeerState.Disconnected : global::PeerState.Connecting);
		return flag;
	}

	public override void Disconnect()
	{
		if (base.PeerState == PeerStateValue.Disconnected)
		{
			if ((int)base.DebugOut >= 2)
			{
				DebugReturn(DebugLevel.WARNING, string.Format("Can't execute Disconnect() while not connected. Nothing changed. State: {0}", State));
			}
		}
		else
		{
			base.Disconnect();
			State = global::PeerState.Disconnecting;
			LeftRoomCleanup();
			LeftLobbyCleanup();
		}
	}

	private void DisconnectFromMaster()
	{
		base.Disconnect();
		State = global::PeerState.DisconnectingFromMasterserver;
		LeftLobbyCleanup();
	}

	private void DisconnectFromGameServer()
	{
		base.Disconnect();
		State = global::PeerState.DisconnectingFromGameserver;
		LeftRoomCleanup();
	}

	private void LeftLobbyCleanup()
	{
		if (insideLobby)
		{
			SendMonoMessage(PhotonNetworkingMessage.OnLeftLobby);
			insideLobby = false;
		}
	}

	private void LeftRoomCleanup()
	{
		bool flag = mRoomToGetInto != null;
		bool flag2 = ((mRoomToGetInto == null) ? PhotonNetwork.autoCleanUpPlayerObjects : mRoomToGetInto.autoCleanUp);
		mRoomToGetInto = null;
		mActors = new Dictionary<int, PhotonPlayer>();
		mPlayerListCopy = new PhotonPlayer[0];
		mOtherPlayerListCopy = new PhotonPlayer[0];
		mMasterClient = null;
		blockReceivingGroups = new List<int>();
		blockSendingGroups = new List<int>();
		mGameList = new Dictionary<string, RoomInfo>();
		mGameListCopy = new RoomInfo[0];
		instantiatedPhotonViewSetupList = new List<InstantiatedPhotonViewSetup>();
		ChangeLocalID(-1);
		if (flag2)
		{
			List<GameObject> list = new List<GameObject>(instantiatedObjects.Values);
			foreach (PhotonView value in photonViewList.Values)
			{
				if (value != null && !value.isSceneView && value.gameObject != null)
				{
					list.Add(value.gameObject);
				}
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				GameObject gameObject = list[num];
				if (gameObject != null)
				{
					if ((int)base.DebugOut >= 5)
					{
						DebugReturn(DebugLevel.ALL, "Network destroy Instantiated GO: " + gameObject.name);
					}
					DestroyGO(gameObject);
				}
			}
			cacheInstantiationCount = 0;
			instantiatedObjects = new Dictionary<int, GameObject>();
			allocatedIDs = new Dictionary<int, PhotonViewID>();
		}
		if (flag)
		{
			SendMonoMessage(PhotonNetworkingMessage.OnLeftRoom);
		}
	}

	private void DestroyGO(GameObject go)
	{
		PhotonView[] componentsInChildren = go.GetComponentsInChildren<PhotonView>();
		PhotonView[] array = componentsInChildren;
		foreach (PhotonView photonView in array)
		{
			if (photonView != null)
			{
				RemovePhotonView(photonView, false);
			}
		}
		UnityEngine.Object.Destroy(go);
	}

	private void SwitchNode(byte masterNodeId)
	{
		nodeId = masterNodeId;
		DisconnectFromGameServer();
	}

	private void readoutStandardProperties(Hashtable gameProperties, Hashtable pActorProperties, int targetActorNr)
	{
		if (mCurrentGame != null && gameProperties != null)
		{
			mCurrentGame.CacheProperties(gameProperties);
		}
		if (pActorProperties == null || pActorProperties.Count <= 0)
		{
			return;
		}
		if (targetActorNr > 0)
		{
			PhotonPlayer playerWithID = GetPlayerWithID(targetActorNr);
			if (playerWithID != null)
			{
				playerWithID.InternalCacheProperties(GetActorPropertiesForActorNr(pActorProperties, targetActorNr));
			}
			return;
		}
		foreach (object key in pActorProperties.Keys)
		{
			int num = (int)key;
			Hashtable hashtable = (Hashtable)pActorProperties[key];
			string name = (string)hashtable[byte.MaxValue];
			PhotonPlayer photonPlayer = GetPlayerWithID(num);
			if (photonPlayer == null)
			{
				photonPlayer = new PhotonPlayer(false, num, name);
				AddNewPlayer(num, photonPlayer);
			}
			photonPlayer.InternalCacheProperties(hashtable);
		}
	}

	private void AddNewPlayer(int ID, PhotonPlayer player)
	{
		if (!mActors.ContainsKey(ID))
		{
			mActors[ID] = player;
			RebuildPlayerListCopies();
		}
		else
		{
			Debug.LogError("Adding player twice: " + ID);
		}
	}

	private void RemovePlayer(int ID, PhotonPlayer player)
	{
		mActors.Remove(ID);
		if (!player.isLocal)
		{
			RebuildPlayerListCopies();
		}
	}

	private void RebuildPlayerListCopies()
	{
		mPlayerListCopy = new PhotonPlayer[mActors.Count];
		mActors.Values.CopyTo(mPlayerListCopy, 0);
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		PhotonPlayer[] array = mPlayerListCopy;
		foreach (PhotonPlayer photonPlayer in array)
		{
			if (!photonPlayer.isLocal)
			{
				list.Add(photonPlayer);
			}
		}
		mOtherPlayerListCopy = list.ToArray();
	}

	private void ResetPhotonViewsOnSerialize()
	{
		foreach (PhotonView value in photonViewList.Values)
		{
			value.lastOnSerializeDataSent = null;
		}
	}

	private void HandleEventLeave(int actorID)
	{
		if ((int)base.DebugOut >= 3)
		{
			DebugReturn(DebugLevel.INFO, "HandleEventLeave actorNr: " + actorID);
		}
		if (actorID < 0 || !mActors.ContainsKey(actorID))
		{
			if ((int)base.DebugOut >= 1)
			{
				DebugReturn(DebugLevel.ERROR, string.Format("Received event Leave for unknown actorNumber: {0}", actorID));
			}
			return;
		}
		PhotonPlayer playerWithID = GetPlayerWithID(actorID);
		if (playerWithID == null)
		{
			Debug.LogError("Error: HandleEventLeave for actorID=" + actorID + " has no PhotonPlayer!");
		}
		if (mMasterClient != null && mMasterClient.ID == actorID)
		{
			mMasterClient = null;
		}
		CheckMasterClient(actorID);
		if (mCurrentGame != null && mCurrentGame.autoCleanUp)
		{
			DestroyPlayerObjects(playerWithID, true);
		}
		RemovePlayer(actorID, playerWithID);
		SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerDisconnected, playerWithID);
	}

	private void CheckMasterClient(int ignoreActorID)
	{
		int num = int.MaxValue;
		if (mMasterClient != null && mActors.ContainsKey(mMasterClient.ID))
		{
			return;
		}
		foreach (int key in mActors.Keys)
		{
			if ((ignoreActorID == -1 || ignoreActorID != key) && key < num)
			{
				num = key;
			}
		}
		if (mMasterClient == null || mMasterClient.ID != num)
		{
			mMasterClient = mActors[num];
			SendMonoMessage(PhotonNetworkingMessage.OnMasterClientSwitched, mMasterClient);
		}
	}

	private Hashtable GetActorPropertiesForActorNr(Hashtable actorProperties, int actorNr)
	{
		if (actorProperties.ContainsKey(actorNr))
		{
			return (Hashtable)actorProperties[actorNr];
		}
		return actorProperties;
	}

	private PhotonPlayer GetPlayerWithID(int number)
	{
		if (mActors != null && mActors.ContainsKey(number))
		{
			return mActors[number];
		}
		return null;
	}

	private void SendPlayerName()
	{
		if (State == global::PeerState.Joining)
		{
			mPlayernameHasToBeUpdated = true;
		}
		else if (mLocalActor != null)
		{
			mLocalActor.name = PlayerName;
			Hashtable hashtable = new Hashtable();
			hashtable[byte.MaxValue] = PlayerName;
			OpSetPropertiesOfActor(mLocalActor.ID, hashtable, true, 0);
			mPlayernameHasToBeUpdated = false;
		}
	}

	private void GameEnteredOnGameServer(OperationResponse operationResponse)
	{
		if (operationResponse.ReturnCode != 0)
		{
			switch (operationResponse.OperationCode)
			{
			case 227:
				DebugReturn(DebugLevel.ERROR, "Create failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonCreateRoomFailed);
				break;
			case 226:
				DebugReturn(DebugLevel.WARNING, "Join failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
				if (operationResponse.ReturnCode == 32758)
				{
					Debug.Log("Most likely the game became empty during the switch to GameServer.");
				}
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonJoinRoomFailed);
				break;
			case 225:
				DebugReturn(DebugLevel.WARNING, "Join failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
				if (operationResponse.ReturnCode == 32758)
				{
					Debug.Log("Most likely the game became empty during the switch to GameServer.");
				}
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonRandomJoinFailed);
				break;
			}
			DisconnectFromGameServer();
		}
		else
		{
			State = global::PeerState.Joined;
			mRoomToGetInto.isLocalClientInside = true;
			Hashtable pActorProperties = (Hashtable)operationResponse[249];
			Hashtable gameProperties = (Hashtable)operationResponse[248];
			readoutStandardProperties(gameProperties, pActorProperties, 0);
			int newID = (int)operationResponse[254];
			ChangeLocalID(newID);
			CheckMasterClient(-1);
			if (mPlayernameHasToBeUpdated)
			{
				SendPlayerName();
			}
			switch (operationResponse.OperationCode)
			{
			case 227:
				SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom);
				break;
			case 225:
			case 226:
				break;
			}
		}
	}

	private Hashtable GetLocalActorProperties()
	{
		if (PhotonNetwork.player != null)
		{
			return PhotonNetwork.player.allProperties;
		}
		Hashtable hashtable = new Hashtable();
		hashtable[byte.MaxValue] = PlayerName;
		return hashtable;
	}

	public void ChangeLocalID(int newID)
	{
		if (mLocalActor == null)
		{
			Debug.LogWarning(string.Format("Local actor is null or not in mActors! mLocalActor: {0} mActors==null: {1} newID: {2}", mLocalActor, mActors == null, newID));
		}
		if (mActors.ContainsKey(mLocalActor.ID))
		{
			mActors.Remove(mLocalActor.ID);
		}
		mLocalActor.InternalChangeLocalID(newID);
		mActors[mLocalActor.ID] = mLocalActor;
		RebuildPlayerListCopies();
	}

	public bool OpCreateGame(string gameID, bool isVisible, bool isOpen, byte maxPlayers, bool autoCleanUp, Hashtable customGameProperties, string[] propsListedInLobby)
	{
		mRoomToGetInto = new Room(gameID, customGameProperties, isVisible, isOpen, maxPlayers, autoCleanUp, propsListedInLobby);
		return base.OpCreateRoom(gameID, isVisible, isOpen, maxPlayers, autoCleanUp, customGameProperties, GetLocalActorProperties(), propsListedInLobby);
	}

	public bool OpJoin(string gameID)
	{
		mRoomToGetInto = new Room(gameID, null);
		return OpJoinRoom(gameID, GetLocalActorProperties());
	}

	public override bool OpJoinRandomRoom(Hashtable expectedGameProperties)
	{
		mRoomToGetInto = new Room(null, expectedGameProperties);
		return base.OpJoinRandomRoom(expectedGameProperties);
	}

	public virtual bool OpLeave()
	{
		if (State != global::PeerState.Joined)
		{
			DebugReturn(DebugLevel.ERROR, "NetworkingPeer::leaveGame() - ERROR: no game is currently joined");
			return false;
		}
		return OpCustom(254, null, true, 0);
	}

	public override bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId, int[] targetActors, EventCaching cache)
	{
		if (PhotonNetwork.offlineMode)
		{
			return false;
		}
		return base.OpRaiseEvent(eventCode, evData, sendReliable, channelId, targetActors, cache);
	}

	public override bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId, EventCaching cache, ReceiverGroup receivers)
	{
		if (PhotonNetwork.offlineMode)
		{
			return false;
		}
		return base.OpRaiseEvent(eventCode, evData, sendReliable, channelId, cache, receivers);
	}

	public void DebugReturn(DebugLevel level, string message)
	{
		externalListener.DebugReturn(level, message);
	}

	public void OnOperationResponse(OperationResponse operationResponse)
	{
		if (PhotonNetwork.networkingPeer.State == global::PeerState.Disconnecting)
		{
			if ((int)base.DebugOut >= 3)
			{
				DebugReturn(DebugLevel.INFO, "OperationResponse ignored while disconnecting: " + operationResponse.OperationCode);
			}
			return;
		}
		if (operationResponse.ReturnCode == 0)
		{
			if ((int)base.DebugOut >= 3)
			{
				DebugReturn(DebugLevel.INFO, operationResponse.ToString());
			}
		}
		else if ((int)base.DebugOut >= 2)
		{
			if (operationResponse.ReturnCode == -3)
			{
				DebugReturn(DebugLevel.WARNING, "Operation could not be executed yet. Wait for state JoinedLobby or ConnectedToMaster and their respective callbacks before calling OPs. Client must be authorized.");
			}
			DebugReturn(DebugLevel.WARNING, operationResponse.ToStringFull());
		}
		switch (operationResponse.OperationCode)
		{
		case 230:
			if (operationResponse.ReturnCode != 0)
			{
				if ((int)base.DebugOut >= 1)
				{
					DebugReturn(DebugLevel.ERROR, string.Format("Authentication failed: '{0}' Code: {1}", operationResponse.DebugMessage, operationResponse.ReturnCode));
				}
				if (operationResponse.ReturnCode == -2)
				{
					DebugReturn(DebugLevel.ERROR, string.Format("If you host Photon yourself, make sure to start the 'Instance LoadBalancing'"));
				}
				if (operationResponse.ReturnCode == short.MaxValue)
				{
					DebugReturn(DebugLevel.ERROR, string.Format("The appId this client sent is unknown on the server (Cloud). Check settings. If using the Cloud, check account."));
				}
				Disconnect();
				State = global::PeerState.Disconnecting;
				if (operationResponse.ReturnCode == 32757)
				{
					DebugReturn(DebugLevel.ERROR, string.Format("Currently, the limit of users is reached for this title. Try again later. Disconnecting"));
					SendMonoMessage(PhotonNetworkingMessage.OnPhotonMaxCccuReached);
				}
			}
			else if (State == global::PeerState.Connected || State == global::PeerState.ConnectedComingFromGameserver)
			{
				if (operationResponse.Parameters.ContainsKey(223))
				{
					mQueuePosition = (int)operationResponse[223];
					if (mQueuePosition > 0)
					{
						if (State == global::PeerState.ConnectedComingFromGameserver)
						{
							State = global::PeerState.QueuedComingFromGameserver;
						}
						else
						{
							State = global::PeerState.Queued;
						}
						break;
					}
				}
				if (PhotonNetwork.autoJoinLobby)
				{
					OpJoinLobby();
					State = global::PeerState.Authenticated;
				}
				else
				{
					State = global::PeerState.ConnectedToMaster;
					SendMonoMessage(PhotonNetworkingMessage.OnConnectedToMaster);
				}
			}
			else if (State == global::PeerState.ConnectedToGameserver)
			{
				State = global::PeerState.Joining;
				if (mLastJoinType == JoinType.JoinGame || mLastJoinType == JoinType.JoinRandomGame)
				{
					OpJoin(mRoomToGetInto.name);
				}
				else if (mLastJoinType == JoinType.CreateGame)
				{
					OpCreateGame(mRoomToGetInto.name, mRoomToGetInto.visible, mRoomToGetInto.open, (byte)mRoomToGetInto.maxPlayers, mRoomToGetInto.autoCleanUp, mRoomToGetInto.customProperties, mRoomToGetInto.propertiesListedInLobby);
				}
			}
			break;
		case 227:
			if (State != global::PeerState.Joining)
			{
				if (operationResponse.ReturnCode != 0)
				{
					if ((int)base.DebugOut >= 1)
					{
						DebugReturn(DebugLevel.ERROR, string.Format("createGame failed, client stays on masterserver: {0}.", operationResponse.ToStringFull()));
					}
					SendMonoMessage(PhotonNetworkingMessage.OnPhotonCreateRoomFailed);
					break;
				}
				string text = (string)operationResponse[byte.MaxValue];
				if (!string.IsNullOrEmpty(text))
				{
					mRoomToGetInto.name = text;
				}
				mGameserver = (string)operationResponse[230];
				DisconnectFromMaster();
				mLastJoinType = JoinType.CreateGame;
			}
			else
			{
				GameEnteredOnGameServer(operationResponse);
			}
			break;
		case 226:
			if (State != global::PeerState.Joining)
			{
				if (operationResponse.ReturnCode != 0)
				{
					SendMonoMessage(PhotonNetworkingMessage.OnPhotonJoinRoomFailed);
					if ((int)base.DebugOut >= 1)
					{
						DebugReturn(DebugLevel.ERROR, string.Format("joinGame failed, client stays on masterserver: {0}. State: {1}", operationResponse.ToStringFull(), State));
					}
				}
				else
				{
					mGameserver = (string)operationResponse[230];
					DisconnectFromMaster();
					mLastJoinType = JoinType.JoinGame;
				}
			}
			else
			{
				GameEnteredOnGameServer(operationResponse);
			}
			break;
		case 225:
			if (operationResponse.ReturnCode != 0)
			{
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonRandomJoinFailed);
				if ((int)base.DebugOut >= 1)
				{
					DebugReturn(DebugLevel.ERROR, string.Format("joinrandom failed, client stays on masterserver: {0}.", operationResponse.ToStringFull()));
				}
			}
			else
			{
				string name = (string)operationResponse[byte.MaxValue];
				mRoomToGetInto.name = name;
				mGameserver = (string)operationResponse[230];
				DisconnectFromMaster();
				mLastJoinType = JoinType.JoinRandomGame;
			}
			break;
		case 229:
			State = global::PeerState.JoinedLobby;
			insideLobby = true;
			SendMonoMessage(PhotonNetworkingMessage.OnJoinedLobby);
			break;
		case 228:
			State = global::PeerState.Authenticated;
			LeftLobbyCleanup();
			break;
		case 254:
			DisconnectFromGameServer();
			break;
		case 251:
		{
			Hashtable pActorProperties = (Hashtable)operationResponse[249];
			Hashtable gameProperties = (Hashtable)operationResponse[248];
			readoutStandardProperties(gameProperties, pActorProperties, 0);
			break;
		}
		default:
			if ((int)base.DebugOut >= 1)
			{
				DebugReturn(DebugLevel.ERROR, string.Format("operationResponse unhandled: {0}", operationResponse.ToString()));
			}
			break;
		case 252:
		case 253:
			break;
		}
		externalListener.OnOperationResponse(operationResponse);
	}

	public void OnStatusChanged(StatusCode statusCode)
	{
		if ((int)base.DebugOut >= 3)
		{
			DebugReturn(DebugLevel.INFO, string.Format("OnStatusChanged: {0}", statusCode.ToString()));
		}
		switch (statusCode)
		{
		case StatusCode.Connect:
			if (State == global::PeerState.ConnectingToGameserver)
			{
				if ((int)base.DebugOut >= 5)
				{
					DebugReturn(DebugLevel.ALL, "Connected to gameserver.");
				}
				State = global::PeerState.ConnectedToGameserver;
			}
			else
			{
				if ((int)base.DebugOut >= 5)
				{
					DebugReturn(DebugLevel.ALL, "Connected to masterserver.");
				}
				if (State == global::PeerState.Connecting)
				{
					SendMonoMessage(PhotonNetworkingMessage.OnConnectedToPhoton);
					State = global::PeerState.Connected;
				}
				else
				{
					State = global::PeerState.ConnectedComingFromGameserver;
				}
			}
			if (requestSecurity)
			{
				EstablishEncryption();
			}
			else if (!OpAuthenticate(mAppId, mAppVersion))
			{
				externalListener.DebugReturn(DebugLevel.ERROR, "Error Authenticating! Did not work.");
			}
			break;
		case StatusCode.Disconnect:
			if (State == global::PeerState.DisconnectingFromMasterserver)
			{
				if (nodeId != 0)
				{
					Debug.Log("connecting to game on node " + nodeId);
				}
				Connect(mGameserver, mAppId, nodeId);
				State = global::PeerState.ConnectingToGameserver;
			}
			else if (State == global::PeerState.DisconnectingFromGameserver)
			{
				nodeId = 0;
				Connect(masterServerAddress, mAppId, 0);
				State = global::PeerState.ConnectingToMasterserver;
			}
			else
			{
				LeftRoomCleanup();
				State = global::PeerState.PeerCreated;
				SendMonoMessage(PhotonNetworkingMessage.OnDisconnectedFromPhoton);
			}
			break;
		case StatusCode.ExceptionOnConnect:
		{
			State = global::PeerState.PeerCreated;
			DisconnectCause disconnectCause = (DisconnectCause)statusCode;
			SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, disconnectCause);
			break;
		}
		case StatusCode.Exception:
			if (State == global::PeerState.Connecting)
			{
				DebugReturn(DebugLevel.WARNING, "Exception while connecting to: " + base.ServerAddress + ". Check if the server is available.");
				if (base.ServerAddress == null || base.ServerAddress.StartsWith("127.0.0.1"))
				{
					DebugReturn(DebugLevel.WARNING, "The server address is 127.0.0.1 (localhost): Make sure the server is running on this machine. Android and iOS emulators have their own localhost.");
					if (base.ServerAddress == mGameserver)
					{
						DebugReturn(DebugLevel.WARNING, "This might be a misconfiguration in the game server config. You need to edit it to a (public) address.");
					}
				}
				State = global::PeerState.PeerCreated;
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, disconnectCause);
			}
			else
			{
				State = global::PeerState.PeerCreated;
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, disconnectCause);
			}
			Disconnect();
			break;
		case StatusCode.InternalReceiveException:
		case StatusCode.TimeoutDisconnect:
		case StatusCode.DisconnectByServer:
		case StatusCode.DisconnectByServerUserLimit:
		case StatusCode.DisconnectByServerLogic:
			if (State == global::PeerState.Connecting)
			{
				DebugReturn(DebugLevel.WARNING, string.Concat(statusCode, " while connecting to: ", base.ServerAddress, ". Check if the server is available."));
				State = global::PeerState.PeerCreated;
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, disconnectCause);
			}
			else
			{
				State = global::PeerState.PeerCreated;
				DisconnectCause disconnectCause = (DisconnectCause)statusCode;
				SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, disconnectCause);
			}
			Disconnect();
			break;
		case StatusCode.EncryptionEstablished:
			if (!OpAuthenticate(mAppId, mAppVersion))
			{
				externalListener.DebugReturn(DebugLevel.ERROR, "Error Authenticating! Did not work.");
			}
			break;
		case StatusCode.EncryptionFailedToEstablish:
			externalListener.DebugReturn(DebugLevel.ERROR, string.Concat("Encryption wasn't established: ", statusCode, ". Going to authenticate anyways."));
			if (!OpAuthenticate(mAppId, mAppVersion))
			{
				externalListener.DebugReturn(DebugLevel.ERROR, "Error Authenticating! Did not work.");
			}
			break;
		default:
			DebugReturn(DebugLevel.ERROR, "Received unknown status code: " + statusCode);
			break;
		case StatusCode.QueueOutgoingReliableWarning:
		case StatusCode.QueueOutgoingUnreliableWarning:
		case StatusCode.SendError:
		case StatusCode.QueueOutgoingAcksWarning:
		case StatusCode.QueueSentWarning:
			break;
		}
		externalListener.OnStatusChanged(statusCode);
	}

	public void OnEvent(EventData photonEvent)
	{
		if ((int)base.DebugOut >= 3)
		{
			DebugReturn(DebugLevel.INFO, string.Format("OnEvent: {0}", photonEvent.ToString()));
		}
		int num = -1;
		PhotonPlayer photonPlayer = null;
		if (photonEvent.Parameters.ContainsKey(254))
		{
			num = (int)photonEvent[254];
			if (mActors.ContainsKey(num))
			{
				photonPlayer = mActors[num];
			}
		}
		switch (photonEvent.Code)
		{
		case 210:
		{
			byte b = (byte)photonEvent[209];
			byte b2 = (byte)photonEvent[208];
			if (b != b2)
			{
				SwitchNode(b2);
			}
			else
			{
				nodeId = b;
			}
			break;
		}
		case 230:
		{
			mGameList = new Dictionary<string, RoomInfo>();
			Hashtable hashtable2 = (Hashtable)photonEvent[222];
			foreach (DictionaryEntry item in hashtable2)
			{
				string text = (string)item.Key;
				mGameList[text] = new RoomInfo(text, (Hashtable)item.Value);
			}
			mGameListCopy = new RoomInfo[mGameList.Count];
			mGameList.Values.CopyTo(mGameListCopy, 0);
			SendMonoMessage(PhotonNetworkingMessage.OnReceivedRoomList);
			break;
		}
		case 229:
		{
			Hashtable hashtable3 = (Hashtable)photonEvent[222];
			foreach (DictionaryEntry item2 in hashtable3)
			{
				string text2 = (string)item2.Key;
				Room room = new Room(text2, (Hashtable)item2.Value);
				if (room.removedFromList)
				{
					mGameList.Remove(text2);
				}
				else
				{
					mGameList[text2] = room;
				}
			}
			mGameListCopy = new RoomInfo[mGameList.Count];
			mGameList.Values.CopyTo(mGameListCopy, 0);
			SendMonoMessage(PhotonNetworkingMessage.OnReceivedRoomListUpdate);
			break;
		}
		case 228:
			if (photonEvent.Parameters.ContainsKey(223))
			{
				mQueuePosition = (int)photonEvent[223];
			}
			else
			{
				DebugReturn(DebugLevel.ERROR, "Event QueueState must contain position!");
			}
			if (mQueuePosition == 0)
			{
				if (PhotonNetwork.autoJoinLobby)
				{
					OpJoinLobby();
					State = global::PeerState.Authenticated;
				}
				else
				{
					State = global::PeerState.ConnectedToMaster;
					SendMonoMessage(PhotonNetworkingMessage.OnConnectedToMaster);
				}
			}
			break;
		case 226:
			mPeerCount = (int)photonEvent[229];
			mGameCount = (int)photonEvent[228];
			mMasterCount = (int)photonEvent[227];
			break;
		case byte.MaxValue:
		{
			Hashtable properties = (Hashtable)photonEvent[249];
			if (photonPlayer == null)
			{
				bool isLocal = mLocalActor.ID == num;
				AddNewPlayer(num, new PhotonPlayer(isLocal, num, properties));
				ResetPhotonViewsOnSerialize();
			}
			if (mActors[num] == mLocalActor)
			{
				int[] array = (int[])photonEvent[252];
				int[] array2 = array;
				foreach (int num6 in array2)
				{
					if (mLocalActor.ID != num6 && !mActors.ContainsKey(num6))
					{
						Debug.Log("creating player");
						AddNewPlayer(num6, new PhotonPlayer(false, num6, string.Empty));
					}
				}
				SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom);
			}
			else
			{
				SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerConnected, mActors[num]);
			}
			break;
		}
		case 254:
			HandleEventLeave(num);
			break;
		case 253:
		{
			int num3 = (int)photonEvent[253];
			Hashtable gameProperties = null;
			Hashtable pActorProperties = null;
			if (num3 == 0)
			{
				gameProperties = (Hashtable)photonEvent[251];
			}
			else
			{
				pActorProperties = (Hashtable)photonEvent[251];
			}
			readoutStandardProperties(gameProperties, pActorProperties, num3);
			break;
		}
		case 200:
			ExecuteRPC(photonEvent[245] as Hashtable, photonPlayer);
			break;
		case 201:
		case 206:
		{
			Hashtable hashtable4 = (Hashtable)photonEvent[245];
			int networkTime = (int)hashtable4[(byte)0];
			short correctPrefix = -1;
			short num4 = 1;
			if (hashtable4.ContainsKey((byte)1))
			{
				correctPrefix = (short)hashtable4[(byte)1];
				num4 = 2;
			}
			for (short num5 = num4; num5 < hashtable4.Count; num5++)
			{
				OnSerializeRead(hashtable4[num5] as Hashtable, photonPlayer, networkTime, correctPrefix);
			}
			break;
		}
		case 202:
			DoInstantiate((Hashtable)photonEvent[245], photonPlayer, null);
			break;
		case 203:
			if (photonPlayer == null || !photonPlayer.isMasterClient)
			{
				Debug.LogError(string.Concat("Error: Someone else(", photonPlayer, ") then the masterserver requests a disconnect!"));
			}
			else
			{
				PhotonNetwork.LeaveRoom();
			}
			break;
		case 204:
		{
			Hashtable hashtable = (Hashtable)photonEvent[245];
			int num2 = (int)hashtable[(byte)0];
			PhotonView photonView = GetPhotonView(num2);
			if (photonView == null || photonPlayer == null)
			{
				Debug.LogError(string.Concat("ERROR: Illegal destroy request on view ID=", num2, " from player/actorNr: ", num, " view=", photonView, "  orgPlayer=", photonPlayer));
			}
			else
			{
				DestroyPhotonView(photonView, true);
			}
			break;
		}
		default:
			Debug.LogError("Error. Unhandled event: " + photonEvent);
			break;
		}
		externalListener.OnEvent(photonEvent);
	}

	public static void SendMonoMessage(PhotonNetworkingMessage methodString, params object[] parameters)
	{
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		MonoBehaviour[] array = (MonoBehaviour[])UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour));
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (!hashSet.Contains(monoBehaviour.gameObject))
			{
				hashSet.Add(monoBehaviour.gameObject);
				if (parameters != null && parameters.Length == 1)
				{
					monoBehaviour.SendMessage(methodString.ToString(), parameters[0], SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					monoBehaviour.SendMessage(methodString.ToString(), parameters, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	public void ExecuteRPC(Hashtable rpcData, PhotonPlayer sender)
	{
		if (rpcData == null || !rpcData.ContainsKey((byte)0))
		{
			DebugReturn(DebugLevel.ERROR, "Malformed RPC; this should never occur.");
			return;
		}
		int num = (int)rpcData[(byte)0];
		int num2 = -1;
		if (rpcData.ContainsKey((byte)1))
		{
			num2 = (short)rpcData[(byte)1];
		}
		string text = (string)rpcData[(byte)3];
		object[] array = (object[])rpcData[(byte)4];
		if (array == null)
		{
			array = new object[0];
		}
		PhotonView photonView = GetPhotonView(num);
		if (photonView == null)
		{
			Debug.LogError("Received RPC \"" + text + "\" for viewID " + num + " but this PhotonView does not exist!");
			return;
		}
		if (photonView.prefix != num2)
		{
			Debug.LogError("Received RPC \"" + text + "\" on viewID " + num + " with a prefix of " + num2 + ", our prefix is " + photonView.prefix + ". The RPC has been ignored.");
			return;
		}
		if (text == string.Empty)
		{
			DebugReturn(DebugLevel.ERROR, "Malformed RPC; this should never occur.");
			return;
		}
		if ((int)base.DebugOut >= 5)
		{
			DebugReturn(DebugLevel.ALL, "Received RPC; " + text);
		}
		if (blockReceivingGroups.Contains(photonView.group))
		{
			return;
		}
		Type[] array2 = Type.EmptyTypes;
		if (array.Length > 0)
		{
			array2 = new Type[array.Length];
			int num3 = 0;
			foreach (object obj in array)
			{
				if (obj == null)
				{
					array2[num3] = null;
				}
				else
				{
					array2[num3] = obj.GetType();
				}
				num3++;
			}
		}
		int num4 = 0;
		int num5 = 0;
		MonoBehaviour[] components = photonView.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in components)
		{
			Type type = monoBehaviour.GetType();
			List<MethodInfo> list = null;
			if (monoRPCMethodsCache.ContainsKey(type))
			{
				list = monoRPCMethodsCache[type];
			}
			if (list == null)
			{
				List<MethodInfo> list2 = new List<MethodInfo>();
				MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				for (int k = 0; k < methods.Length; k++)
				{
					if (methods[k].IsDefined(typeof(RPC), false))
					{
						list2.Add(methods[k]);
					}
				}
				List<MethodInfo> list3 = list2;
				monoRPCMethodsCache[type] = list3;
				list = list3;
			}
			if (list == null)
			{
				continue;
			}
			for (int l = 0; l < list.Count; l++)
			{
				MethodInfo methodInfo = list[l];
				if (!(methodInfo.Name == text))
				{
					continue;
				}
				num5++;
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == array2.Length)
				{
					if (CheckTypeMatch(parameters, array2))
					{
						num4++;
						object obj2 = methodInfo.Invoke(monoBehaviour, array);
						if (methodInfo.ReturnType == typeof(IEnumerator))
						{
							PhotonHandler.SP.StartCoroutine((IEnumerator)obj2);
						}
					}
				}
				else if (parameters.Length - 1 == array2.Length)
				{
					if (CheckTypeMatch(parameters, array2) && parameters[parameters.Length - 1].ParameterType == typeof(PhotonMessageInfo))
					{
						num4++;
						int timestamp = (int)rpcData[(byte)2];
						object[] array3 = new object[array.Length + 1];
						array.CopyTo(array3, 0);
						array3[array3.Length - 1] = new PhotonMessageInfo(sender, timestamp, photonView);
						object obj3 = methodInfo.Invoke(monoBehaviour, array3);
						if (methodInfo.ReturnType == typeof(IEnumerator))
						{
							PhotonHandler.SP.StartCoroutine((IEnumerator)obj3);
						}
					}
				}
				else if (parameters.Length == 1 && parameters[0].ParameterType.IsArray)
				{
					num4++;
					object obj4 = methodInfo.Invoke(monoBehaviour, new object[1] { array });
					if (methodInfo.ReturnType == typeof(IEnumerator))
					{
						PhotonHandler.SP.StartCoroutine((IEnumerator)obj4);
					}
				}
			}
		}
		if (num4 == 1)
		{
			return;
		}
		string text2 = string.Empty;
		foreach (Type type2 in array2)
		{
			if (text2 != string.Empty)
			{
				text2 += ", ";
			}
			text2 = ((type2 != null) ? (text2 + type2.Name) : (text2 + "null"));
		}
		if (num4 == 0)
		{
			if (num5 == 0)
			{
				DebugReturn(DebugLevel.ERROR, "PhotonView with ID " + num + " has no method \"" + text + "\" marked with the [RPC](C#) or @RPC(JS) property!");
			}
			else
			{
				DebugReturn(DebugLevel.ERROR, "PhotonView with ID " + num + " has no method \"" + text + "\" that takes " + array2.Length + " argument(s): " + text2);
			}
		}
		else
		{
			DebugReturn(DebugLevel.ERROR, "PhotonView with ID " + num + " has " + num4 + " methods \"" + text + "\" that takes " + array2.Length + " argument(s): " + text2 + ". Should be just one?");
		}
	}

	private bool CheckTypeMatch(ParameterInfo[] parameters, Type[] types)
	{
		if (parameters.Length < types.Length)
		{
			return false;
		}
		int num = 0;
		foreach (Type type in types)
		{
			if (type != null && parameters[num].ParameterType != type)
			{
				return false;
			}
			num++;
		}
		return true;
	}

	private int AllocateInstantiationId()
	{
		int result = ++cacheInstantiationCount + (mLocalActor.ID << 16);
		if (cacheInstantiationCount == ushort.MaxValue)
		{
			Debug.LogError("Next Instantiation will create a overflow.");
		}
		return result;
	}

	internal Hashtable SendInstantiate(string prefabName, Vector3 position, Quaternion rotation, int group, PhotonViewID[] viewIDs, object[] data, bool isGlobalObject)
	{
		int num = AllocateInstantiationId();
		Hashtable hashtable = new Hashtable();
		hashtable[(byte)0] = prefabName;
		if (position != Vector3.zero)
		{
			hashtable[(byte)1] = position;
		}
		hashtable[(byte)2] = rotation;
		if (group != 0)
		{
			hashtable[(byte)3] = group;
		}
		if (viewIDs != null && viewIDs.Length > 0)
		{
			hashtable[(byte)4] = viewIDs;
		}
		if (data != null)
		{
			hashtable[(byte)5] = data;
		}
		hashtable[(byte)6] = base.ServerTimeInMilliSeconds;
		hashtable[(byte)7] = num;
		EventCaching cache = EventCaching.AddToRoomCache;
		if (isGlobalObject)
		{
			cache = EventCaching.AddToRoomCacheGlobal;
		}
		OpRaiseEvent(202, hashtable, true, 0, cache, ReceiverGroup.Others);
		return hashtable;
	}

	internal GameObject DoInstantiate(Hashtable evData, PhotonPlayer photonPlayer, GameObject resourceGameObject)
	{
		string text = (string)evData[(byte)0];
		Vector3 position = ((!evData.ContainsKey((byte)1)) ? Vector3.zero : ((Vector3)evData[(byte)1]));
		Quaternion rotation = (Quaternion)evData[(byte)2];
		int num = 0;
		if (evData.ContainsKey((byte)3))
		{
			num = (int)evData[(byte)3];
		}
		PhotonViewID[] viewIDs = ((!evData.ContainsKey((byte)4)) ? new PhotonViewID[0] : ((PhotonViewID[])evData[(byte)4]));
		object[] instantiationData = ((!evData.ContainsKey((byte)5)) ? new object[0] : ((object[])evData[(byte)5]));
		int timestamp = (int)evData[(byte)6];
		int key = (int)evData[(byte)7];
		if (blockReceivingGroups.Contains(num))
		{
			return null;
		}
		if (resourceGameObject == null)
		{
			resourceGameObject = (GameObject)Resources.Load(text, typeof(GameObject));
			if (resourceGameObject == null)
			{
				Debug.LogError("PhotonNetwork error: Could not Instantiate the prefab [" + text + "]. Please verify you have this gameobject in a Resources folder.");
				return null;
			}
		}
		InstantiatedPhotonViewSetup instantiatedPhotonViewSetup = new InstantiatedPhotonViewSetup();
		instantiatedPhotonViewSetup.viewIDs = viewIDs;
		instantiatedPhotonViewSetup.group = num;
		instantiatedPhotonViewSetup.instantiationData = instantiationData;
		instantiatedPhotonViewSetupList.Add(instantiatedPhotonViewSetup);
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(resourceGameObject, position, rotation);
		instantiatedObjects.Add(key, gameObject);
		SetupInstantiatedGO(gameObject, instantiatedPhotonViewSetup);
		object[] parameters = new object[1]
		{
			new PhotonMessageInfo(photonPlayer, timestamp, null)
		};
		MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			MethodInfo cachedMethod = GetCachedMethod(monoBehaviour, PhotonNetworkingMessage.OnPhotonInstantiate);
			if (cachedMethod != null)
			{
				object obj = cachedMethod.Invoke(monoBehaviour, parameters);
				if (cachedMethod.ReturnType == typeof(IEnumerator))
				{
					PhotonHandler.SP.StartCoroutine((IEnumerator)obj);
				}
			}
		}
		return gameObject;
	}

	public bool PhotonViewSetup_FindMatchingRoot(GameObject start)
	{
		Transform parent = start.transform.parent;
		for (int i = 0; i < instantiatedPhotonViewSetupList.Count; i++)
		{
			InstantiatedPhotonViewSetup instantiatedPhotonViewSetup = instantiatedPhotonViewSetupList[i];
			int num = start.GetComponentsInChildren<PhotonView>().Length;
			if (num == instantiatedPhotonViewSetup.viewIDs.Length)
			{
				SetupInstantiatedGO(start, instantiatedPhotonViewSetup);
				return true;
			}
			if (parent != null && PhotonViewSetup_FindMatchingRoot(parent.gameObject))
			{
				return true;
			}
		}
		return false;
	}

	private void SetupInstantiatedGO(GameObject goRoot, InstantiatedPhotonViewSetup setupInfo)
	{
		if (instantiatedPhotonViewSetupList.Contains(setupInfo))
		{
			PhotonView[] componentsInChildren = goRoot.GetComponentsInChildren<PhotonView>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				PhotonView photonView = componentsInChildren[i];
				photonView.viewID = setupInfo.viewIDs[i];
				photonView.group = setupInfo.group;
				photonView.instantiationData = setupInfo.instantiationData;
			}
			instantiatedPhotonViewSetupList.Remove(setupInfo);
		}
	}

	public void RemoveAllInstantiatedObjects()
	{
		GameObject[] array = new GameObject[instantiatedObjects.Count];
		instantiatedObjects.Values.CopyTo(array, 0);
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null))
			{
				RemoveInstantiatedGO(gameObject, false);
			}
		}
		if (instantiatedObjects.Count > 0)
		{
			Debug.LogError("RemoveAllInstantiatedObjects() this.instantiatedObjects.Count should be 0 by now.");
		}
		cacheInstantiationCount = 0;
		instantiatedObjects = new Dictionary<int, GameObject>();
	}

	public void RemoveAllInstantiatedObjectsByPlayer(PhotonPlayer player, bool localOnly)
	{
		GameObject[] array = new GameObject[instantiatedObjects.Count];
		instantiatedObjects.Values.CopyTo(array, 0);
		foreach (GameObject gameObject in array)
		{
			if (gameObject == null)
			{
				continue;
			}
			PhotonView[] componentsInChildren = gameObject.GetComponentsInChildren<PhotonView>();
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				PhotonView photonView = componentsInChildren[num];
				if (photonView.owner == player)
				{
					RemoveInstantiatedGO(gameObject, localOnly);
					break;
				}
			}
		}
	}

	public void RemoveInstantiatedGO(GameObject go, bool localOnly)
	{
		if (go == null)
		{
			if (base.DebugOut == DebugLevel.ERROR)
			{
				DebugReturn(DebugLevel.ERROR, "Can't remove instantiated GO if it's null.");
			}
			return;
		}
		int instantiatedObjectsId = GetInstantiatedObjectsId(go);
		if (instantiatedObjectsId == -1)
		{
			if (base.DebugOut == DebugLevel.ERROR)
			{
				DebugReturn(DebugLevel.ERROR, "Can't find GO in instantiation list. Object: " + go);
			}
			return;
		}
		instantiatedObjects.Remove(instantiatedObjectsId);
		PhotonView[] componentsInChildren = go.GetComponentsInChildren<PhotonView>();
		bool flag = false;
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			PhotonView photonView = componentsInChildren[num];
			if (!(photonView == null))
			{
				if (!flag)
				{
					int actorNr = 0;
					if (photonView.owner != null)
					{
						actorNr = photonView.owner.ID;
					}
					RemoveFromServerInstantiationCache(instantiatedObjectsId, actorNr);
					flag = true;
				}
				if (photonView.owner == mLocalActor)
				{
					PhotonNetwork.UnAllocateViewID(photonView.viewID);
				}
				DestroyPhotonView(photonView, localOnly);
			}
		}
		if ((int)base.DebugOut >= 5)
		{
			DebugReturn(DebugLevel.ALL, "Network destroy Instantiated GO: " + go.name);
		}
		DestroyGO(go);
	}

	public int GetInstantiatedObjectsId(GameObject go)
	{
		int num = -1;
		if (go == null)
		{
			DebugReturn(DebugLevel.ERROR, "GetInstantiatedObjectsId() for GO == null.");
			return num;
		}
		foreach (KeyValuePair<int, GameObject> instantiatedObject in instantiatedObjects)
		{
			if (go == instantiatedObject.Value)
			{
				num = instantiatedObject.Key;
				break;
			}
		}
		if (num == -1 && base.DebugOut == DebugLevel.ALL)
		{
			DebugReturn(DebugLevel.ALL, "instantiatedObjects does not contain: " + go);
		}
		return num;
	}

	private void RemoveFromServerInstantiationCache(int instantiateId, int actorNr)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[(byte)7] = instantiateId;
		OpRaiseEvent(202, hashtable, true, 0, new int[1] { actorNr }, EventCaching.RemoveFromRoomCache);
	}

	private void RemoveFromServerInstantiationsOfPlayer(int actorNr)
	{
		OpRaiseEvent(202, null, true, 0, new int[1] { actorNr }, EventCaching.RemoveFromRoomCache);
	}

	public void DestroyPlayerObjects(PhotonPlayer player, bool localOnly)
	{
		RemoveAllInstantiatedObjectsByPlayer(player, localOnly);
		PhotonView[] array = (PhotonView[])UnityEngine.Object.FindObjectsOfType(typeof(PhotonView));
		for (int num = array.Length - 1; num >= 0; num--)
		{
			PhotonView photonView = array[num];
			if (photonView.owner == player)
			{
				DestroyPhotonView(photonView, localOnly);
			}
		}
	}

	public void DestroyPhotonView(PhotonView view, bool localOnly)
	{
		if (!localOnly && (view.isMine || mMasterClient == mLocalActor))
		{
			Hashtable hashtable = new Hashtable();
			hashtable[(byte)0] = view.viewID.ID;
			OpRaiseEvent(204, hashtable, true, 0, EventCaching.DoNotCache, ReceiverGroup.Others);
		}
		if (view.isMine || mMasterClient == mLocalActor)
		{
			RemoveRPCs(view);
			if (view.owner == mLocalActor)
			{
				PhotonNetwork.UnAllocateViewID(view.viewID);
			}
		}
		int instantiatedObjectsId = GetInstantiatedObjectsId(view.gameObject);
		if (instantiatedObjectsId != -1)
		{
			instantiatedObjects.Remove(instantiatedObjectsId);
		}
		if ((int)base.DebugOut >= 5)
		{
			DebugReturn(DebugLevel.ALL, "Network destroy PhotonView GO: " + view.gameObject.name);
		}
		DestroyGO(view.gameObject);
	}

	public PhotonView GetPhotonView(int viewID)
	{
		PhotonView value = null;
		photonViewList.TryGetValue(viewID, out value);
		return value;
	}

	public void RegisterPhotonView(PhotonView netView)
	{
		if (!Application.isPlaying)
		{
			photonViewList = new Dictionary<int, PhotonView>();
			return;
		}
		netView.prefix = currentLevelPrefix;
		if (netView.owner != null)
		{
			int num = netView.viewID.ID / PhotonNetwork.MAX_VIEW_IDS;
			if (netView.owner.ID != num)
			{
				Debug.LogError(string.Concat("RegisterPhotonView: registered view ID ", netView.viewID, " with owner ", netView.owner.ID, " but it should be ", num));
			}
		}
		if (!photonViewList.ContainsKey(netView.viewID.ID))
		{
			photonViewList.Add(netView.viewID.ID, netView);
			if ((int)base.DebugOut >= 5)
			{
				DebugReturn(DebugLevel.ALL, "Registered PhotonView: " + netView.viewID);
			}
		}
	}

	public void RemovePhotonView(PhotonView netView, bool mayFail)
	{
		if (!Application.isPlaying)
		{
			photonViewList = new Dictionary<int, PhotonView>();
		}
		else
		{
			if (!photonViewList.ContainsKey(netView.viewID.ID))
			{
				return;
			}
			if (photonViewList[netView.viewID.ID] != netView)
			{
				if (!mayFail)
				{
					Debug.LogError("PHOTON ERROR: This should never be possible: Two PhotonViews with same ID registered? ID=" + netView.viewID.ID + " " + netView.name + "  and " + photonViewList[netView.viewID.ID].name);
				}
			}
			else
			{
				photonViewList.Remove(netView.viewID.ID);
				if ((int)base.DebugOut >= 5)
				{
					DebugReturn(DebugLevel.ALL, "Removed PhotonView: " + netView.viewID);
				}
			}
		}
	}

	public void RemoveRPCs(int actorNumber)
	{
		OpRaiseEvent(200, null, true, 0, new int[1] { actorNumber }, EventCaching.RemoveFromRoomCache);
	}

	public void RemoveCompleteCacheOfPlayer(int actorNumber)
	{
		OpRaiseEvent(0, null, true, 0, new int[1] { actorNumber }, EventCaching.RemoveFromRoomCache);
	}

	private void RemoveCacheOfLeftPlayers()
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary[244] = (byte)0;
		dictionary[247] = (byte)7;
		OpCustom(253, dictionary, true, 0);
	}

	public void RemoveRPCs(PhotonView view)
	{
		if (!mLocalActor.isMasterClient && view.owner != mLocalActor)
		{
			Debug.LogError(string.Concat("Error, cannot remove cached RPCs on a PhotonView thats not ours! ", view.owner, " scene: ", view.isSceneView));
		}
		else
		{
			Hashtable hashtable = new Hashtable();
			hashtable[(byte)0] = view.viewID.ID;
			OpRaiseEvent(200, hashtable, true, 0, EventCaching.RemoveFromRoomCache, ReceiverGroup.Others);
		}
	}

	public void RemoveRPCsInGroup(int group)
	{
		foreach (KeyValuePair<int, PhotonView> photonView in photonViewList)
		{
			PhotonView value = photonView.Value;
			if (value.group == group)
			{
				RemoveRPCs(value);
			}
		}
	}

	public void SetLevelPrefix(short prefix)
	{
		currentLevelPrefix = prefix;
		foreach (PhotonView value in photonViewList.Values)
		{
			value.prefix = prefix;
		}
	}

	public void RPC(PhotonView view, string methodName, PhotonPlayer player, params object[] parameters)
	{
		if (!blockSendingGroups.Contains(view.group))
		{
			if (view.viewID.ID < 1)
			{
				Debug.LogError(string.Concat("Illegal view ID:", view.viewID, " method: ", methodName, " GO:", view.gameObject.name));
			}
			if ((int)base.DebugOut >= 3)
			{
				DebugReturn(DebugLevel.INFO, string.Concat("Sending RPC \"", methodName, "\" to player[", player, "]"));
			}
			Hashtable hashtable = new Hashtable();
			hashtable[(byte)0] = view.viewID.ID;
			if (view.prefix > 0)
			{
				hashtable[(byte)1] = view.prefix;
			}
			hashtable[(byte)2] = base.ServerTimeInMilliSeconds;
			hashtable[(byte)3] = methodName;
			hashtable[(byte)4] = parameters;
			if (mLocalActor == player)
			{
				ExecuteRPC(hashtable, player);
				return;
			}
			int[] targetActors = new int[1] { player.ID };
			OpRaiseEvent(200, hashtable, true, 0, targetActors);
		}
	}

	public void RPC(PhotonView view, string methodName, PhotonTargets target, params object[] parameters)
	{
		if (blockSendingGroups.Contains(view.group))
		{
			return;
		}
		if (view.viewID.ID < 1)
		{
			Debug.LogError(string.Concat("Illegal view ID:", view.viewID, " method: ", methodName, " GO:", view.gameObject.name));
		}
		if ((int)base.DebugOut >= 3)
		{
			DebugReturn(DebugLevel.INFO, "Sending RPC \"" + methodName + "\" to " + target);
		}
		Hashtable hashtable = new Hashtable();
		hashtable[(byte)0] = view.viewID.ID;
		if (view.prefix > 0)
		{
			hashtable[(byte)1] = view.prefix;
		}
		hashtable[(byte)2] = base.ServerTimeInMilliSeconds;
		hashtable[(byte)3] = methodName;
		hashtable[(byte)4] = parameters;
		switch (target)
		{
		case PhotonTargets.All:
			OpRaiseEvent(200, hashtable, true, 0);
			ExecuteRPC(hashtable, mLocalActor);
			break;
		case PhotonTargets.Others:
			OpRaiseEvent(200, hashtable, true, 0);
			break;
		case PhotonTargets.AllBuffered:
			OpRaiseEvent(200, hashtable, true, 0, EventCaching.AddToRoomCache, ReceiverGroup.Others);
			ExecuteRPC(hashtable, mLocalActor);
			break;
		case PhotonTargets.OthersBuffered:
			OpRaiseEvent(200, hashtable, true, 0, EventCaching.AddToRoomCache, ReceiverGroup.Others);
			break;
		case PhotonTargets.MasterClient:
			if (mMasterClient == mLocalActor)
			{
				ExecuteRPC(hashtable, mLocalActor);
			}
			else
			{
				OpRaiseEvent(200, hashtable, true, 0, EventCaching.DoNotCache, ReceiverGroup.MasterClient);
			}
			break;
		default:
			Debug.LogError("Unsupported target enum: " + target);
			break;
		}
	}

	public void SetReceivingEnabled(int group, bool enabled)
	{
		if (!enabled)
		{
			if (!blockReceivingGroups.Contains(group))
			{
				blockReceivingGroups.Add(group);
			}
		}
		else
		{
			blockReceivingGroups.Remove(group);
		}
	}

	public void SetSendingEnabled(int group, bool enabled)
	{
		if (!enabled)
		{
			if (!blockSendingGroups.Contains(group))
			{
				blockSendingGroups.Add(group);
			}
		}
		else
		{
			blockSendingGroups.Remove(group);
		}
	}

	public void NewSceneLoaded()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PhotonView> photonView in photonViewList)
		{
			PhotonView value = photonView.Value;
			if (value == null)
			{
				list.Add(photonView.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			int key = list[i];
			photonViewList.Remove(key);
		}
		if (list.Count > 0 && (int)base.DebugOut >= 3)
		{
			DebugReturn(DebugLevel.INFO, "Removed " + list.Count + " scene view IDs from last scene.");
		}
	}

	public void RunViewUpdate()
	{
		if (!PhotonNetwork.connected || PhotonNetwork.offlineMode || mActors == null || mActors.Count <= 1)
		{
			return;
		}
		Hashtable hashtable = new Hashtable();
		hashtable[(byte)0] = base.ServerTimeInMilliSeconds;
		Hashtable hashtable2 = new Hashtable();
		hashtable2[(byte)0] = base.ServerTimeInMilliSeconds;
		int num = 1;
		if (currentLevelPrefix >= 0)
		{
			hashtable[(byte)1] = currentLevelPrefix;
			hashtable2[(byte)1] = currentLevelPrefix;
			num = 2;
		}
		foreach (KeyValuePair<int, PhotonView> photonView in photonViewList)
		{
			PhotonView value = photonView.Value;
			if (!(value.observed != null) || value.synchronization == ViewSynchronization.Off || (value.owner != mLocalActor && (!value.isSceneView || mMasterClient != mLocalActor)) || !value.gameObject.active || blockSendingGroups.Contains(value.group))
			{
				continue;
			}
			Hashtable hashtable3 = OnSerializeWrite(value);
			if (hashtable3 == null)
			{
				continue;
			}
			if (value.synchronization == ViewSynchronization.ReliableDeltaCompressed)
			{
				if (hashtable3.ContainsKey((byte)1) || hashtable3.ContainsKey((byte)2))
				{
					hashtable.Add((short)hashtable.Count, hashtable3);
				}
			}
			else
			{
				hashtable2.Add((short)hashtable2.Count, hashtable3);
			}
		}
		if (hashtable.Count > num)
		{
			OpRaiseEvent(206, hashtable, true, 0);
		}
		if (hashtable2.Count > num)
		{
			OpRaiseEvent(201, hashtable2, false, 1);
		}
	}

	private void ExecuteOnSerialize(MonoBehaviour monob, PhotonStream pStream, PhotonMessageInfo info)
	{
		object[] parameters = new object[2] { pStream, info };
		MethodInfo cachedMethod = GetCachedMethod(monob, PhotonNetworkingMessage.OnPhotonSerializeView);
		if (cachedMethod != null)
		{
			object obj = cachedMethod.Invoke(monob, parameters);
			if (cachedMethod.ReturnType == typeof(IEnumerator))
			{
				PhotonHandler.SP.StartCoroutine((IEnumerator)obj);
			}
		}
		else
		{
			Debug.LogError(string.Concat("Tried to run ", PhotonNetworkingMessage.OnPhotonSerializeView, ", but this method was missing on: ", monob));
		}
	}

	private Hashtable OnSerializeWrite(PhotonView view)
	{
		List<object> list = new List<object>();
		if (view.observed is MonoBehaviour)
		{
			MonoBehaviour monob = (MonoBehaviour)view.observed;
			PhotonStream photonStream = new PhotonStream(true, null);
			PhotonMessageInfo info = new PhotonMessageInfo(mLocalActor, base.ServerTimeInMilliSeconds, view);
			ExecuteOnSerialize(monob, photonStream, info);
			if (photonStream.Count == 0)
			{
				return null;
			}
			list = photonStream.data;
		}
		else if (view.observed is Transform)
		{
			Transform transform = (Transform)view.observed;
			if (view.onSerializeTransformOption == OnSerializeTransform.OnlyPosition || view.onSerializeTransformOption == OnSerializeTransform.PositionAndRotation || view.onSerializeTransformOption == OnSerializeTransform.All)
			{
				list.Add(transform.localPosition);
			}
			else
			{
				list.Add(null);
			}
			if (view.onSerializeTransformOption == OnSerializeTransform.OnlyRotation || view.onSerializeTransformOption == OnSerializeTransform.PositionAndRotation || view.onSerializeTransformOption == OnSerializeTransform.All)
			{
				list.Add(transform.localRotation);
			}
			else
			{
				list.Add(null);
			}
			if (view.onSerializeTransformOption == OnSerializeTransform.OnlyScale || view.onSerializeTransformOption == OnSerializeTransform.All)
			{
				list.Add(transform.localScale);
			}
		}
		else
		{
			if (!(view.observed is Rigidbody))
			{
				Debug.LogError("Observed type is not serializable: " + view.observed.GetType());
				return null;
			}
			Rigidbody rigidbody = (Rigidbody)view.observed;
			if (view.onSerializeRigidBodyOption != OnSerializeRigidBody.OnlyAngularVelocity)
			{
				list.Add(rigidbody.velocity);
			}
			else
			{
				list.Add(null);
			}
			if (view.onSerializeRigidBodyOption != 0)
			{
				list.Add(rigidbody.angularVelocity);
			}
		}
		object[] array = list.ToArray();
		Hashtable hashtable = new Hashtable();
		hashtable[(byte)0] = view.viewID.ID;
		hashtable[(byte)1] = array;
		if (view.synchronization == ViewSynchronization.ReliableDeltaCompressed)
		{
			bool flag = DeltaCompressionWrite(view, hashtable);
			view.lastOnSerializeDataSent = array;
			if (!flag)
			{
				return null;
			}
		}
		return hashtable;
	}

	private void OnSerializeRead(Hashtable data, PhotonPlayer sender, int networkTime, short correctPrefix)
	{
		int num = (int)data[(byte)0];
		PhotonView photonView = GetPhotonView(num);
		if (photonView == null)
		{
			Debug.LogWarning("Received OnSerialization for view ID " + num + ". We have no such PhotonView! Ignored this if you're leaving a room. State: " + State);
		}
		else if (photonView.prefix > 0 && correctPrefix != photonView.prefix)
		{
			Debug.LogError("Received OnSerialization for view ID " + num + " with prefix " + correctPrefix + ". Our prefix is " + photonView.prefix);
		}
		else
		{
			if (blockReceivingGroups.Contains(photonView.group))
			{
				return;
			}
			if (photonView.synchronization == ViewSynchronization.ReliableDeltaCompressed)
			{
				if (!DeltaCompressionRead(photonView, data))
				{
					DebugReturn(DebugLevel.INFO, string.Concat("Skipping packet for ", photonView.name, " [", photonView.viewID, "] as we haven't received a full packet for delta compression yet. This is OK if it happens for the first few frames after joining a game."));
					return;
				}
				photonView.lastOnSerializeDataReceived = data[(byte)1] as object[];
			}
			if (photonView.observed is MonoBehaviour)
			{
				object[] incomingData = data[(byte)1] as object[];
				MonoBehaviour monob = (MonoBehaviour)photonView.observed;
				PhotonStream pStream = new PhotonStream(false, incomingData);
				PhotonMessageInfo info = new PhotonMessageInfo(sender, networkTime, photonView);
				ExecuteOnSerialize(monob, pStream, info);
			}
			else if (photonView.observed is Transform)
			{
				object[] array = data[(byte)1] as object[];
				Transform transform = (Transform)photonView.observed;
				if (array.Length >= 1 && array[0] != null)
				{
					transform.localPosition = (Vector3)array[0];
				}
				if (array.Length >= 2 && array[1] != null)
				{
					transform.localRotation = (Quaternion)array[1];
				}
				if (array.Length >= 3 && array[2] != null)
				{
					transform.localScale = (Vector3)array[2];
				}
			}
			else if (photonView.observed is Rigidbody)
			{
				object[] array2 = data[(byte)1] as object[];
				Rigidbody rigidbody = (Rigidbody)photonView.observed;
				if (array2.Length >= 1 && array2[0] != null)
				{
					rigidbody.velocity = (Vector3)array2[0];
				}
				if (array2.Length >= 2 && array2[1] != null)
				{
					rigidbody.angularVelocity = (Vector3)array2[1];
				}
			}
			else
			{
				Debug.LogError("Type of observed is unknown when receiving.");
			}
		}
	}

	private bool DeltaCompressionWrite(PhotonView view, Hashtable data)
	{
		if (view.lastOnSerializeDataSent == null)
		{
			return true;
		}
		object[] lastOnSerializeDataSent = view.lastOnSerializeDataSent;
		object[] array = data[(byte)1] as object[];
		if (array == null)
		{
			return false;
		}
		if (lastOnSerializeDataSent.Length != array.Length)
		{
			return true;
		}
		object[] array2 = new object[array.Length];
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < array2.Length; i++)
		{
			object obj = array[i];
			object two = lastOnSerializeDataSent[i];
			if (ObjectIsSameWithInprecision(obj, two))
			{
				num++;
				continue;
			}
			array2[i] = array[i];
			if (obj == null)
			{
				list.Add(i);
			}
		}
		if (num > 0)
		{
			data.Remove((byte)1);
			if (num == array.Length)
			{
				return false;
			}
			data[(byte)2] = array2;
			if (list.Count > 0)
			{
				data[(byte)3] = list.ToArray();
			}
		}
		return true;
	}

	private bool DeltaCompressionRead(PhotonView view, Hashtable data)
	{
		if (data.ContainsKey((byte)1))
		{
			return true;
		}
		if (view.lastOnSerializeDataReceived == null)
		{
			return false;
		}
		object[] array = data[(byte)2] as object[];
		if (array == null)
		{
			return false;
		}
		int[] array2 = data[(byte)3] as int[];
		if (array2 == null)
		{
			array2 = new int[0];
		}
		object[] lastOnSerializeDataReceived = view.lastOnSerializeDataReceived;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null && !array2.Contains(i))
			{
				object obj = lastOnSerializeDataReceived[i];
				array[i] = obj;
			}
		}
		data[(byte)1] = array;
		return true;
	}

	private bool ObjectIsSameWithInprecision(object one, object two)
	{
		if (one == null || two == null)
		{
			return one == null && two == null;
		}
		if (!one.Equals(two))
		{
			if (one is Vector3)
			{
				Vector3 target = (Vector3)one;
				Vector3 second = (Vector3)two;
				if (target.AlmostEquals(second, PhotonNetwork.precisionForVectorSynchronization))
				{
					return true;
				}
			}
			else if (one is Vector2)
			{
				Vector2 target2 = (Vector2)one;
				Vector2 second2 = (Vector2)two;
				if (target2.AlmostEquals(second2, PhotonNetwork.precisionForVectorSynchronization))
				{
					return true;
				}
			}
			else if (one is Quaternion)
			{
				Quaternion target3 = (Quaternion)one;
				Quaternion second3 = (Quaternion)two;
				if (target3.AlmostEquals(second3, PhotonNetwork.precisionForQuaternionSynchronization))
				{
					return true;
				}
			}
			else if (one is float)
			{
				float target4 = (float)one;
				float second4 = (float)two;
				if (target4.AlmostEquals(second4, PhotonNetwork.precisionForFloatSynchronization))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private MethodInfo GetCachedMethod(MonoBehaviour monob, PhotonNetworkingMessage methodType)
	{
		Type type = monob.GetType();
		if (!cachedMethods.ContainsKey(type))
		{
			Dictionary<PhotonNetworkingMessage, MethodInfo> value = new Dictionary<PhotonNetworkingMessage, MethodInfo>();
			cachedMethods.Add(type, value);
		}
		Dictionary<PhotonNetworkingMessage, MethodInfo> dictionary = cachedMethods[type];
		if (!dictionary.ContainsKey(methodType))
		{
			Type[] types;
			switch (methodType)
			{
			case PhotonNetworkingMessage.OnPhotonSerializeView:
				types = new Type[2]
				{
					typeof(PhotonStream),
					typeof(PhotonMessageInfo)
				};
				break;
			case PhotonNetworkingMessage.OnPhotonInstantiate:
				types = new Type[1] { typeof(PhotonMessageInfo) };
				break;
			default:
				Debug.LogError("Invalid PhotonNetworkingMessage!");
				return null;
			}
			MethodInfo method = monob.GetType().GetMethod(string.Concat(methodType, string.Empty), BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
			if (method != null)
			{
				dictionary.Add(methodType, method);
			}
		}
		if (dictionary.ContainsKey(methodType))
		{
			return dictionary[methodType];
		}
		return null;
	}
}
