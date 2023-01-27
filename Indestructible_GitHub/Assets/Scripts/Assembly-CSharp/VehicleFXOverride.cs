using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/VehicleFX Override")]
public class VehicleFXOverride : VisualFXOverride
{
	public VisualFX.EffectOptions exhaustOptions;

	public float exhaustThrottleEmissionFactor = -1f;

	public VisualFX.EffectOptions boostOptions;

	public float boostThrottleEmissionFactor = -1f;

	public override void Mounted(Vehicle vehicle)
	{
		VehicleFX component = vehicle.GetComponent<VehicleFX>();
		if (component != null)
		{
			OverrideOptions(exhaustOptions, component.exhaustOptions);
			if (0f <= exhaustThrottleEmissionFactor)
			{
				component.exhaustThrottleEmissionFactor = exhaustThrottleEmissionFactor;
			}
			OverrideOptions(boostOptions, component.boostOptions);
			if (0f <= boostThrottleEmissionFactor)
			{
				component.boostThrottleEmissionFactor = boostThrottleEmissionFactor;
			}
		}
		base.Mounted(vehicle);
	}
}
