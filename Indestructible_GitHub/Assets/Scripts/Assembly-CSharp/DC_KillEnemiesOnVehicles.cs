public class DC_KillEnemiesOnVehicles : DC_StoreStringSet
{
	public DC_KillEnemiesOnVehicles(string id, int vehicles)
		: base(id)
	{
		_goal = vehicles;
	}

	public override void OnGameFinished(IDTGame game, ref IDTGame.Reward reward)
	{
		base.OnGameFinished(game, ref reward);
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (multiplayerGame != null && multiplayerGame.localPlayer.killCount > 0)
		{
			_items.Add(MonoSingleton<Player>.Instance.SelectedVehicle.Vehicle.id);
			_value = _items.Count;
		}
	}
}
