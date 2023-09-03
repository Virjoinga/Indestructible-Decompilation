public class DC_MatchOnEveryMap : DC_StoreStringSet
{
	private bool _checkVictory;

	public DC_MatchOnEveryMap(string id, bool checkVictory)
		: base(id)
	{
		_checkVictory = checkVictory;
		_goal = 4;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		if (!_checkVictory || (_checkVictory && reward.Victory))
		{
			_items.Add(MonoSingleton<Player>.Instance.LastPlayedMap);
			_value = _items.Count;
		}
	}
}
