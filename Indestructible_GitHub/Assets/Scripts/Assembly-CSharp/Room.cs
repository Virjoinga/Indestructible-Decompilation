using System.Collections;
using ExitGames.Client.Photon;
using UnityEngine;

public class Room : RoomInfo
{
	public new int playerCount
	{
		get
		{
			if (PhotonNetwork.playerList != null)
			{
				return PhotonNetwork.playerList.Length;
			}
			return 0;
		}
	}

	public new string name
	{
		get
		{
			return nameField;
		}
		internal set
		{
			nameField = value;
		}
	}

	public new int maxPlayers
	{
		get
		{
			return maxPlayersField;
		}
		set
		{
			if (!Equals(PhotonNetwork.room))
			{
				PhotonNetwork.networkingPeer.DebugReturn(DebugLevel.WARNING, "Can't set room properties when not in that room.");
			}
			if (value > 255)
			{
				Debug.LogError("Error: room.maxPlayers called with value " + value + ". This has been reverted to the max of 255 players, because internally a 'byte' is used.");
				value = 255;
			}
			if (value != maxPlayersField && !PhotonNetwork.offlineMode)
			{
				PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(new Hashtable { 
				{
					byte.MaxValue,
					(byte)value
				} }, true, 0);
			}
			maxPlayersField = (byte)value;
		}
	}

	public new bool open
	{
		get
		{
			return openField;
		}
		set
		{
			if (!Equals(PhotonNetwork.room))
			{
				PhotonNetwork.networkingPeer.DebugReturn(DebugLevel.WARNING, "Can't set room properties when not in that room.");
			}
			if (value != openField && !PhotonNetwork.offlineMode)
			{
				PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(new Hashtable { 
				{
					(byte)253,
					value
				} }, true, 0);
			}
			openField = value;
		}
	}

	public new bool visible
	{
		get
		{
			return visibleField;
		}
		set
		{
			if (!Equals(PhotonNetwork.room))
			{
				PhotonNetwork.networkingPeer.DebugReturn(DebugLevel.WARNING, "Can't set room properties when not in that room.");
			}
			if (value != visibleField && !PhotonNetwork.offlineMode)
			{
				PhotonNetwork.networkingPeer.OpSetPropertiesOfRoom(new Hashtable { 
				{
					(byte)254,
					value
				} }, true, 0);
			}
			visibleField = value;
		}
	}

	public string[] propertiesListedInLobby { get; private set; }

	public bool autoCleanUp
	{
		get
		{
			return autoCleanUpField;
		}
	}

	internal Room(string roomName, Hashtable properties)
		: base(roomName, properties)
	{
		propertiesListedInLobby = new string[0];
	}

	internal Room(string roomName, Hashtable properties, bool isVisible, bool isOpen, int maxPlayers, bool autoCleanUp, string[] propsListedInLobby)
		: base(roomName, properties)
	{
		visibleField = isVisible;
		openField = isOpen;
		autoCleanUpField = autoCleanUp;
		if (maxPlayers > 255)
		{
			Debug.LogError("Error: Room() called with " + maxPlayers + " maxplayers. This has been reverted to the max of 255 players, because internally a 'byte' is used.");
			maxPlayers = 255;
		}
		maxPlayersField = (byte)maxPlayers;
		if (propsListedInLobby != null)
		{
			propertiesListedInLobby = propsListedInLobby;
		}
		else
		{
			propertiesListedInLobby = new string[0];
		}
	}

	public void SetCustomProperties(Hashtable propertiesToSet)
	{
		if (propertiesToSet != null)
		{
			base.customProperties.MergeStringKeys(propertiesToSet);
			base.customProperties.StripKeysWithNullValues();
			Hashtable gameProperties = propertiesToSet.StripToStringKeys();
			PhotonNetwork.networkingPeer.OpSetCustomPropertiesOfRoom(gameProperties, true, 0);
		}
	}
}
