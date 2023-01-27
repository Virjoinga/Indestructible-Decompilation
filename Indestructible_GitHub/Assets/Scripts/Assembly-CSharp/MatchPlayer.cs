using System;
using System.Text;
using Glu;

public class MatchPlayer
{
	public class MatchBase : MonoBehaviour
	{
		protected static void SetID(MatchPlayer player, int value)
		{
			player._id = value;
		}

		protected static void SetTeamID(MatchPlayer player, int value)
		{
			player._teamID = value;
		}

		protected static void SetIsReady(MatchPlayer player, bool value)
		{
			player._isReady = value;
		}

		protected static void SetIsDisconnected(MatchPlayer player, bool value)
		{
			player._isDisconnected = value;
		}
	}

	private int _id;

	protected string _name;

	protected int _rating;

	private int _teamID;

	private bool _isReady;

	private bool _isDisconnected;

	public int id
	{
		get
		{
			return _id;
		}
	}

	public string name
	{
		get
		{
			return _name;
		}
	}

	public int rating
	{
		get
		{
			return _rating;
		}
	}

	public int teamID
	{
		get
		{
			return _teamID;
		}
	}

	public bool isReady
	{
		get
		{
			return _isReady;
		}
	}

	public bool isDisconnected
	{
		get
		{
			return _isDisconnected;
		}
	}

	protected MatchPlayer(int id, string name, int rating)
	{
		_id = id;
		_name = name;
		_rating = rating;
		_teamID = -1;
		_isReady = false;
	}

	protected MatchPlayer(int id)
	{
		_id = id;
		_teamID = -1;
		_isReady = false;
	}

	public static MatchPlayer Make(int id, byte[] data, int pos)
	{
		Type value;
		pos = Decode(data, pos, out value);
		MatchPlayer matchPlayer = Activator.CreateInstance(value, id) as MatchPlayer;
		matchPlayer.Decode(data, pos);
		return matchPlayer;
	}

	public virtual int GetEncodedSize()
	{
		return GetEncodedSize(GetType()) + GetEncodedSize(_name) + 4;
	}

	public virtual int Encode(byte[] data, int pos)
	{
		pos = Encode(GetType().ToString(), data, pos);
		pos = Encode(_name, data, pos);
		return Encode(_rating, data, pos);
	}

	protected virtual int Decode(byte[] data, int pos)
	{
		pos = Decode(data, pos, out _name);
		return Decode(data, pos, out _rating);
	}

	protected static int GetEncodedSize(Type type)
	{
		return GetEncodedSize(type.ToString());
	}

	protected static int GetEncodedSize(string str)
	{
		return Encoding.ASCII.GetByteCount(str) + 1;
	}

	protected static int Encode(Type value, byte[] data, int pos)
	{
		return Encode(value.ToString(), data, pos);
	}

	protected static int Encode(string value, byte[] data, int pos)
	{
		int bytes = Encoding.ASCII.GetBytes(value, 0, value.Length, data, pos + 1);
		data[pos] = (byte)bytes;
		return pos + bytes + 1;
	}

	protected static int Encode(int value, byte[] data, int pos)
	{
		Buffer.BlockCopy(BitConverter.GetBytes(value), 0, data, pos, 4);
		return pos + 4;
	}

	protected static int Decode(byte[] data, int pos, out Type value)
	{
		string value2;
		pos = Decode(data, pos, out value2);
		value = Type.GetType(value2);
		return pos;
	}

	protected static int Decode(byte[] data, int pos, out string value)
	{
		int num = data[pos];
		value = Encoding.ASCII.GetString(data, ++pos, num);
		return pos + num;
	}

	protected static int Decode(byte[] data, int pos, out int value)
	{
		value = BitConverter.ToInt32(data, pos);
		return pos + 4;
	}
}
