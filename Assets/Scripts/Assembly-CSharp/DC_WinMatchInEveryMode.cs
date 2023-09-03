public class DC_WinMatchInEveryMode : DC_StoreStringSet
{
	public DC_WinMatchInEveryMode(string id)
		: base(id)
	{
		_goal = 4;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		if (reward.Victory)
		{
			_items.Add(MonoSingleton<Player>.Instance.LastPlayedGame);
			_value = _items.Count;
		}
	}
}
