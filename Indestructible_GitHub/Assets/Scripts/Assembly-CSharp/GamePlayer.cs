public class GamePlayer : MatchPlayer
{
	public abstract class MultiplayerGameBase : IDTGame
	{
		protected static void SetScore(GamePlayer player, int value)
		{
			player._score = value;
		}

		protected static void AddScore(GamePlayer player, int value)
		{
			player._score += value;
		}

		protected static void SetKillCount(GamePlayer player, int value)
		{
			player._killCount = value;
		}

		protected static void AddKillCount(GamePlayer player, int value)
		{
			player._killCount += value;
		}

		protected static void SetSelfKillCount(GamePlayer player, int value)
		{
			player._selfKillCount = value;
		}

		protected static void AddSelfKillCount(GamePlayer player, int value)
		{
			player._selfKillCount += value;
		}

		protected static void SetCollisionKillCount(GamePlayer player, int value)
		{
			player._collisionKillCount = value;
		}

		protected static void AddCollisionKillCount(GamePlayer player, int value)
		{
			player._collisionKillCount += value;
		}

		protected static void SetDeathCount(GamePlayer player, int value)
		{
			player._deathCount = value;
		}

		protected static void AddDeathCount(GamePlayer player, int value)
		{
			player._deathCount += value;
		}

		protected static void SetVehicle(GamePlayer player, Vehicle value)
		{
			player._vehicle = value;
		}
	}

	protected int _league;

	protected int _score;

	protected int _killCount;

	protected int _selfKillCount;

	protected int _collisionKillCount;

	protected int _deathCount;

	protected Vehicle _vehicle;

	public int league
	{
		get
		{
			return _league;
		}
	}

	public Vehicle vehicle
	{
		get
		{
			return _vehicle;
		}
	}

	public int score
	{
		get
		{
			return _score;
		}
	}

	public int killCount
	{
		get
		{
			return _killCount;
		}
	}

	public int selfKillCount
	{
		get
		{
			return _selfKillCount;
		}
	}

	public int deathCount
	{
		get
		{
			return _deathCount;
		}
	}

	public int collisionKillCount
	{
		get
		{
			return _collisionKillCount;
		}
	}

	public GamePlayer(int id)
		: base(id)
	{
	}

	public GamePlayer(int id, string name, int rating, int league)
		: base(id, name, rating)
	{
		_league = league;
	}

	public override int GetEncodedSize()
	{
		return base.GetEncodedSize() + 4;
	}

	public override int Encode(byte[] data, int pos)
	{
		pos = base.Encode(data, pos);
		pos = MatchPlayer.Encode(_league, data, pos);
		return pos;
	}

	protected override int Decode(byte[] data, int pos)
	{
		pos = base.Decode(data, pos);
		return MatchPlayer.Decode(data, pos, out _league);
	}
}
