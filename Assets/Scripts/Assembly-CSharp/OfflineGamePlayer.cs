public class OfflineGamePlayer : GamePlayer
{
	protected string _vehicleName;

	public string vehicleName
	{
		get
		{
			return _vehicleName;
		}
	}

	public OfflineGamePlayer(int id)
		: base(id)
	{
	}

	public OfflineGamePlayer(int id, string name, int rating, int league, string vehicleName)
		: base(id, name, rating, league)
	{
		_vehicleName = vehicleName;
	}
}
