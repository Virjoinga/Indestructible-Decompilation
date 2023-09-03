using System;
using System.Threading;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

internal class PhotonHandler : Photon.MonoBehaviour, IPhotonPeerListener
{
	public IPhotonPeerListener externalListener;

	public static PhotonHandler SP;

	public int updateInterval;

	public int updateIntervalOnSerialize;

	private int nextSendTickCount = Environment.TickCount;

	private int nextSendTickCountOnSerialize = Environment.TickCount;

	private void Awake()
	{
		if (SP != null && SP != this)
		{
			Debug.LogError("Error: we already have an PhotonMono around!");
			UnityEngine.Object.Destroy(base.gameObject);
		}
		UnityEngine.Object.DontDestroyOnLoad(this);
		SP = this;
		updateInterval = 1000 / PhotonNetwork.sendRate;
		updateIntervalOnSerialize = 1000 / PhotonNetwork.sendRateOnSerialize;
	}

	private void Update()
	{
		if (PhotonNetwork.networkingPeer == null)
		{
			Debug.LogError("NetworkPeer broke!");
		}
		else
		{
			if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated || PhotonNetwork.connectionStateDetailed == PeerState.Disconnected || !PhotonNetwork.isMessageQueueRunning)
			{
				return;
			}
			bool flag = true;
			while (PhotonNetwork.isMessageQueueRunning && flag)
			{
				flag = PhotonNetwork.networkingPeer.DispatchIncomingCommands();
			}
			if (PhotonNetwork.isMessageQueueRunning && Environment.TickCount > nextSendTickCountOnSerialize)
			{
				PhotonNetwork.networkingPeer.RunViewUpdate();
				nextSendTickCountOnSerialize = Environment.TickCount + updateIntervalOnSerialize;
			}
			if (Environment.TickCount > nextSendTickCount)
			{
				bool flag2 = true;
				while (PhotonNetwork.isMessageQueueRunning && flag2)
				{
					flag2 = PhotonNetwork.networkingPeer.SendOutgoingCommands();
				}
				nextSendTickCount = Environment.TickCount + updateInterval;
			}
		}
	}

	public void OnApplicationQuit()
	{
		PhotonNetwork.Disconnect();
	}

	public void OnLevelWasLoaded(int level)
	{
		PhotonNetwork.networkingPeer.NewSceneLoaded();
	}

	public static void StartThread()
	{
		Thread thread = new Thread(MyThread);
		thread.Start();
	}

	public static void MyThread()
	{
		while (PhotonNetwork.networkingPeer != null && PhotonNetwork.networkingPeer.IsSendingOnlyAcks)
		{
			while (PhotonNetwork.networkingPeer.SendOutgoingCommands())
			{
			}
			Thread.Sleep(200);
		}
	}

	public void DebugReturn(DebugLevel level, string message)
	{
		if (level == DebugLevel.ERROR)
		{
			Debug.LogError(message);
		}
		else if (level == DebugLevel.WARNING)
		{
			Debug.LogWarning(message);
		}
		else if (level == DebugLevel.INFO && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log(message);
		}
		else if (level == DebugLevel.ALL && PhotonNetwork.logLevel == PhotonLogLevel.Full)
		{
			Debug.Log(message);
		}
		if (externalListener != null)
		{
			externalListener.DebugReturn(level, message);
		}
	}

	public void OnOperationResponse(OperationResponse operationResponse)
	{
		if (externalListener != null)
		{
			externalListener.OnOperationResponse(operationResponse);
		}
	}

	public void OnStatusChanged(StatusCode statusCode)
	{
		if (externalListener != null)
		{
			externalListener.OnStatusChanged(statusCode);
		}
	}

	public void OnEvent(EventData photonEvent)
	{
		if (externalListener != null)
		{
			externalListener.OnEvent(photonEvent);
		}
	}
}
