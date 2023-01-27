using UnityEngine;

public interface IGameMode
{
	void Init();

	void OnPlayerVehicleCreated(GameObject playerVehicle, int teamID);
}
