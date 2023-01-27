using UnityEngine;

public class PlayerHealthBarScript : MonoBehaviour
{
	public SpriteText Text;

	private InclinedProgressBar _progress;

	private Destructible _destructible;

	private float _cachedPercent = -1f;

	private float _cachedMaximum = 1f;

	private void Start()
	{
		_progress = GetComponent<InclinedProgressBar>();
		VehiclesManager instance = VehiclesManager.instance;
		instance.playerVehicleActivatedEvent += PlayerVehicleActivated;
		instance.playerVehicleDeactivatedEvent += PlayerVehicleDeactivated;
		if (instance.playerVehicle != null && instance.playerVehicle.isActive)
		{
			PlayerVehicleActivated(instance.playerVehicle);
		}
	}

	private void OnDestroy()
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.playerVehicleActivatedEvent -= PlayerVehicleActivated;
			instance.playerVehicleDeactivatedEvent -= PlayerVehicleDeactivated;
		}
	}

	private void PlayerVehicleActivated(Vehicle vehicle)
	{
		_destructible = vehicle.destructible;
		SetValue(_destructible.hp, _destructible.GetBaseMaxHP());
	}

	private void PlayerVehicleDeactivated(Vehicle vehicle)
	{
		SetValue(0f, _cachedMaximum);
		_destructible = null;
	}

	private void SetValue(float current, float maximum)
	{
		current = Mathf.Clamp(current, 0f, maximum);
		float num = current / maximum;
		if (num != _cachedPercent || _cachedMaximum != maximum)
		{
			current = Mathf.Ceil(current);
			maximum = Mathf.Ceil(maximum);
			Text.Text = string.Format("{0:0} / {1:0}", current, maximum);
			_progress.Position = num;
			_cachedPercent = num;
		}
		_cachedMaximum = maximum;
	}

	private void LateUpdate()
	{
		if (_destructible != null)
		{
			SetValue(_destructible.hp, _destructible.GetMaxHP());
		}
	}
}
