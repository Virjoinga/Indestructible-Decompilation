using System.Collections.Generic;

public class MatchTeam
{
	public readonly int id;

	protected List<MatchPlayer> _players;

	protected int _connectedPlayerCount;

	protected object _data;

	public virtual IEnumerable<MatchPlayer> players
	{
		get
		{
			return _players;
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

	public object data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
		}
	}

	protected MatchTeam(int id)
	{
		this.id = id;
		_players = new List<MatchPlayer>();
	}

	public virtual void Clear()
	{
		_players.Clear();
		_connectedPlayerCount = 0;
		_data = null;
	}
}
