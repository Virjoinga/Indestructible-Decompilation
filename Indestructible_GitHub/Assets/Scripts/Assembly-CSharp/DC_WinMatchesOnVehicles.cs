public class DC_WinMatchesOnVehicles : DC_StoreStringSet
{
	public DC_WinMatchesOnVehicles(string id, int vehicles)
		: base(id)
	{
		_goal = vehicles;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		if (reward.Victory)
		{
			_items.Add(MonoSingleton<Player>.Instance.SelectedVehicle.Vehicle.id);
			_value = _items.Count;
		}
	}
}
