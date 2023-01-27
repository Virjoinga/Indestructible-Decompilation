using System.Xml.Serialization;

public class RankedMatchmakerConfig
{
	public float? _connectionTimeout;

	public float? _forceMatchmakingTimeout;

	public float? _idleFailTimeout;

	public int? _maxPlayerCount;

	[XmlAttribute("connectionTimeout")]
	public float ConnectionTimeout
	{
		get
		{
			float? connectionTimeout = _connectionTimeout;
			return (!connectionTimeout.HasValue) ? 13f : connectionTimeout.Value;
		}
		private set
		{
			_connectionTimeout = value;
		}
	}

	[XmlAttribute("forceMatchmakingTimeout")]
	public float ForceMatchmakingTimeout
	{
		get
		{
			float? forceMatchmakingTimeout = _forceMatchmakingTimeout;
			return (!forceMatchmakingTimeout.HasValue) ? 17.5f : forceMatchmakingTimeout.Value;
		}
		private set
		{
			_forceMatchmakingTimeout = value;
		}
	}

	[XmlAttribute("idleFailTimeout")]
	public float IdleFailTimeout
	{
		get
		{
			float? idleFailTimeout = _idleFailTimeout;
			return (!idleFailTimeout.HasValue) ? 180f : idleFailTimeout.Value;
		}
		private set
		{
			_idleFailTimeout = value;
		}
	}

	[XmlAttribute("maxPlayerCount")]
	public int MaxPlayerCount
	{
		get
		{
			int? maxPlayerCount = _maxPlayerCount;
			return (!maxPlayerCount.HasValue) ? 64 : maxPlayerCount.Value;
		}
		private set
		{
			_maxPlayerCount = value;
		}
	}
}
