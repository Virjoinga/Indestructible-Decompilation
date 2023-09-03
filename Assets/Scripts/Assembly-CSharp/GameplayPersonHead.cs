using UnityEngine;

public class GameplayPersonHead : MonoBehaviour
{
	public UISingleSprite PersonHead;

	private void Awake()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		string personHead = selectedVehicle.Body.PersonHead;
		SimpleSpriteUtils.ChangeTexture(PersonHead, personHead);
	}
}
