using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Effects/VisualFX Override")]
public class VisualFXOverride : Glu.MonoBehaviour, IMountable
{
	public VisualFX.EffectOptions smokeOptions;

	public VisualFX.EffectOptions burnOptions;

	public virtual void Mounted(Vehicle vehicle)
	{
		VisualFX component = vehicle.GetComponent<VisualFX>();
		if (component != null)
		{
			OverrideOptions(smokeOptions, component.smokeOptions);
			OverrideOptions(burnOptions, component.burnOptions);
		}
		Object.Destroy(this);
	}

	public void WillUnmount(Vehicle vehicle)
	{
	}

	protected void OverrideOptions(VisualFX.EffectOptions srcOptions, VisualFX.EffectOptions dstOptions)
	{
		if (srcOptions.effectPrefab != null)
		{
			dstOptions.effectPrefab = srcOptions.effectPrefab;
			dstOptions.emissionScale = srcOptions.emissionScale;
			dstOptions.emissionSpeed = srcOptions.emissionSpeed;
			dstOptions.lifetimeScale = srcOptions.lifetimeScale;
			dstOptions.sizeScale = srcOptions.sizeScale;
			dstOptions.velocityScale = srcOptions.velocityScale;
		}
		if (srcOptions.emissionPoints.Length != 0)
		{
			dstOptions.emissionPoints = srcOptions.emissionPoints;
			dstOptions.shouldDestroyEmissionPoints = srcOptions.shouldDestroyEmissionPoints;
		}
	}
}
