using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Network/GameCenter Matchmaker")]
public class GCMatchmaker : UnityEngine.MonoBehaviour
{
	public enum Result
	{
		Cancel = 0,
		ConnectionFail = 1,
		Success = 2
	}

	private class InternalPlayer : GamePlayer
	{
		public string gcID;

		public InternalPlayer(int id)
			: base(id)
		{
		}

		public InternalPlayer(int id, string name, int rating, int league, string gcID)
			: base(id, name, rating, league)
		{
			this.gcID = gcID;
		}

		public override int GetEncodedSize()
		{
			return base.GetEncodedSize() + MatchPlayer.GetEncodedSize(gcID);
		}

		public override int Encode(byte[] data, int pos)
		{
			pos = base.Encode(data, pos);
			return MatchPlayer.Encode(gcID, data, pos);
		}

		protected override int Decode(byte[] data, int pos)
		{
			pos = base.Decode(data, pos);
			return MatchPlayer.Decode(data, pos, out gcID);
		}
	}

	public delegate void FinishDelegate(Result result, MultiplayerMatch match);

	private const float ConnectionTimeout = 7f;

	private PhotonView _photonView;

	private MultiplayerMatch _match;

	private bool _isMatchmaking;

	private static GCMatchmaker _matchmakerInstance;

	public static void HostMatch(int minPlayerCount, int maxPlayerCount, int teamCount, string levelName, string config, FinishDelegate finishDelegate)
	{
		Debug.Log("GCMatchmaker.HostMatch:" + minPlayerCount + "; " + maxPlayerCount + "; " + teamCount + "; " + levelName + "; " + config);
		GameCenter.hostedMatch.Create(minPlayerCount, maxPlayerCount, 1u, 1u, delegate(bool succeed)
		{
			Debug.Log("GCMatchmaker.HostMatchmakingFinished:" + succeed);
			FinishDelegate finishDelegate3 = finishDelegate;
			finishDelegate = null;
			if (_matchmakerInstance != null)
			{
				if (succeed && _matchmakerInstance._isMatchmaking && _matchmakerInstance._match != null)
				{
					if (finishDelegate3 != null)
					{
						finishDelegate3(Result.Success, _matchmakerInstance._match);
					}
					_matchmakerInstance._match.StartMatch();
					return;
				}
				_matchmakerInstance.Cancel();
			}
			NetworkManager.instance.Disconnect(null);
			if (finishDelegate3 != null)
			{
				finishDelegate3(Result.Cancel, null);
			}
		});
		NetworkManager.instance.CreateOrJoinRoom(0, GetMatchID(GameCenter.localUser.id), maxPlayerCount, false, 7f, delegate(NetworkManager.Result result)
		{
			Debug.Log("GCMatchmaker.Join room finished:" + result);
			if (result == NetworkManager.Result.Success)
			{
				Debug.Log("GCMatchmaker.PhotonNetwork.Instantiate(GCMatchmaker)");
				GCMatchmaker component = PhotonNetwork.Instantiate("GCMatchmaker", Vector3.zero, Quaternion.identity, 0).GetComponent<GCMatchmaker>();
				component.CreateMatch(minPlayerCount, maxPlayerCount, teamCount, levelName, config);
			}
			else
			{
				FinishDelegate finishDelegate2 = finishDelegate;
				finishDelegate = null;
				GameCenter.hostedMatch.DismissMatchmakerUI(false);
				if (finishDelegate2 != null)
				{
					finishDelegate2(Result.ConnectionFail, null);
				}
			}
		});
	}

	public static void JoinMatch(string inviterId, FinishDelegate finishDelegate)
	{
		Debug.Log("GCMatchmaker.JoinMatch:" + inviterId);
		GameCenter.hostedMatch.ShowMatchmakerUI(delegate(bool succeed)
		{
			Debug.Log("GCMatchmaker.InviteeMatchmakingFinished:" + succeed);
			FinishDelegate finishDelegate3 = finishDelegate;
			finishDelegate = null;
			if (_matchmakerInstance != null)
			{
				if (succeed && _matchmakerInstance._isMatchmaking && _matchmakerInstance._match != null)
				{
					if (finishDelegate3 != null)
					{
						finishDelegate3(Result.Success, _matchmakerInstance._match);
					}
					return;
				}
				_matchmakerInstance.Cancel();
			}
			NetworkManager.instance.Disconnect(null);
			if (finishDelegate3 != null)
			{
				finishDelegate3(Result.Cancel, null);
			}
		});
		string matchID = GetMatchID(inviterId);
		NetworkManager.instance.JoinRoom(0, matchID, false, 7f, delegate(NetworkManager.Result result)
		{
			Debug.Log("GCMatchmaker.Join room finished:" + result);
			if (result != NetworkManager.Result.Success)
			{
				FinishDelegate finishDelegate2 = finishDelegate;
				finishDelegate = null;
				GameCenter.hostedMatch.DismissMatchmakerUI(false);
				if (finishDelegate2 != null)
				{
					finishDelegate2(Result.ConnectionFail, null);
				}
			}
		});
	}

	private void Awake()
	{
		Debug.Log("GCMatchmaker.Awake");
		_matchmakerInstance = this;
		_photonView = GetComponent<PhotonView>();
		_match = GetComponent<MultiplayerMatch>();
		Object.DontDestroyOnLoad(this);
	}

	private void OnDestroy()
	{
		Debug.Log("GCMatchmaker.OnDestroy");
		_matchmakerInstance = null;
	}

	private void CreateMatch(int minPlayerCount, int maxPlayerCount, int teamCount, string levelName, string config)
	{
		Debug.Log("GCMatchmaker.CreateMatch!");
		_isMatchmaking = true;
		_photonView.RPC("RemoteJoinMatch", PhotonTargets.OthersBuffered, minPlayerCount, maxPlayerCount, teamCount, levelName, config);
		JoinMatch(minPlayerCount, maxPlayerCount, teamCount, levelName, config);
	}

	[RPC]
	private void RemoteJoinMatch(int minPlayerCount, int maxPlayerCount, int teamCount, string levelName, string config, PhotonMessageInfo msgInfo)
	{
		Debug.Log("GCMatchmaker.RemoteJoinMatch:" + levelName + "; " + config + "; " + minPlayerCount + "; " + maxPlayerCount + "; " + teamCount + "; " + msgInfo.sender.ID);
		_isMatchmaking = true;
		JoinMatch(minPlayerCount, maxPlayerCount, teamCount, levelName, config);
	}

	private void JoinMatch(int minPlayerCount, int maxPlayerCount, int teamCount, string levelName, string config)
	{
		Debug.Log("GCMatchmaker.JoinMatch:" + _isMatchmaking);
		_match.canceledEvent += MatchCanceled;
		_match.playerDisconnectedEvent += PlayerDisconnected;
		_match.playerReadyEvent += PlayerReady;
		_match.matchReadyEvent += MatchReady;
		_match.joinTimeout = 10000000f;
		Player instance = MonoSingleton<Player>.Instance;
		_match.JoinMatch(0, PhotonNetwork.room.name, minPlayerCount, maxPlayerCount, teamCount, levelName, config, new InternalPlayer(-1, instance.Name, instance.EloRate, instance.League, GameCenter.localUser.id));
	}

	private void PlayerDisconnected(MultiplayerMatch match, MatchPlayer player)
	{
		Debug.Log("GCMatchmaker.PlayerDisconnected, SetPlayerStatus(" + (player as InternalPlayer).gcID + ", false)");
		GameCenter.hostedMatch.SetPlayerStatus((player as InternalPlayer).gcID, false);
	}

	private void PlayerReady(MultiplayerMatch match, MatchPlayer player)
	{
		Debug.Log("GCMatchmaker.PlayerReady, SetPlayerStatus(" + (player as InternalPlayer).gcID + ", true)");
		GameCenter.hostedMatch.SetPlayerStatus((player as InternalPlayer).gcID, true);
	}

	private void MatchReady(MultiplayerMatch match)
	{
		Debug.Log("GCMatchmaker.MatchReady!");
		GameCenter.hostedMatch.DismissMatchmakerUI(true);
		Clear();
	}

	private void Fail()
	{
		Debug.Log("GCMatchmaker.Fail()!");
		GameCenter.hostedMatch.DismissMatchmakerUI(false);
	}

	private void Cancel()
	{
		Debug.Log("GCMatchmaker.Cancel");
		if (_isMatchmaking)
		{
			Clear();
			if (_match != null)
			{
				_match.Cancel();
			}
		}
	}

	private void Clear()
	{
		Debug.Log("GCMatchmaker.Clear");
		_isMatchmaking = false;
		_match.canceledEvent -= MatchCanceled;
	}

	private void MatchCanceled(MultiplayerMatch match, MultiplayerMatch.CancelReason reason)
	{
		Debug.Log("GCMatchmaker.MatchCanceled!");
		if (_isMatchmaking)
		{
			Fail();
		}
	}

	private void OnDisconnectedFromPhoton()
	{
		Debug.Log("GCMatchmaker.OnDisconnectedFromPhoton!");
		if (_isMatchmaking)
		{
			Fail();
		}
	}

	private void OnLeftRoom()
	{
		Debug.Log("GCMatchmaker.OnLeftRoom!");
		if (_isMatchmaking)
		{
			Fail();
		}
	}

	private void OnMasterClientSwitched(PhotonPlayer player)
	{
		Debug.Log("GCMatchmaker.OnMasterClientSwitched!");
		if (_isMatchmaking)
		{
			Fail();
		}
	}

	private static string GetMatchID(string gcID)
	{
		return "PrivateMatch" + gcID;
	}
}
