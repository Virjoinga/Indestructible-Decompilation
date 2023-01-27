using UnityEngine;

public class DamageHistory : MonoBehaviour
{
	private SpriteText _text;

	private Destructible _destructible;

	private float _lastHealth;

	private int index;

	private void Start()
	{
		_text = GetComponent<SpriteText>();
		VehiclesManager instance = VehiclesManager.instance;
		instance.playerVehicleActivatedEvent += PlayerVehicleActivated;
		instance.playerVehicleDeactivatedEvent += PlayerVehicleDeactivated;
		if (instance.playerVehicle != null && instance.playerVehicle.isActive)
		{
			PlayerVehicleActivated(instance.playerVehicle);
		}
	}

	private void PlayerVehicleActivated(Vehicle vehicle)
	{
		_destructible = vehicle.destructible;
		_lastHealth = _destructible.hp;
	}

	private void PlayerVehicleDeactivated(Vehicle vehicle)
	{
		_destructible = null;
	}

	private void Update()
	{
		if (!(_destructible != null))
		{
			return;
		}
		float hp = _destructible.hp;
		if (hp != _lastHealth)
		{
			float num = hp - _lastHealth;
			_lastHealth = hp;
			string text = _text.Text;
			while (text.Length > 100)
			{
				int startIndex = text.LastIndexOf('\n');
				text = text.Remove(startIndex);
			}
			_text.Text = string.Format("\n{0:00} {1:+0.##;-0.##}", index, num) + text;
			index++;
			if (index > 99)
			{
				index = 0;
			}
		}
	}
}
