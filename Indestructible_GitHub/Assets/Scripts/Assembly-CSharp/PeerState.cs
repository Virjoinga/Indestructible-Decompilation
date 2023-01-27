public enum PeerState
{
	Uninitialized = 0,
	PeerCreated = 1,
	Connecting = 2,
	Connected = 3,
	Queued = 4,
	Authenticated = 5,
	JoinedLobby = 6,
	DisconnectingFromMasterserver = 7,
	ConnectingToGameserver = 8,
	ConnectedToGameserver = 9,
	Joining = 10,
	Joined = 11,
	Leaving = 12,
	DisconnectingFromGameserver = 13,
	ConnectingToMasterserver = 14,
	ConnectedComingFromGameserver = 15,
	QueuedComingFromGameserver = 16,
	Disconnecting = 17,
	Disconnected = 18,
	ConnectedToMaster = 19
}
