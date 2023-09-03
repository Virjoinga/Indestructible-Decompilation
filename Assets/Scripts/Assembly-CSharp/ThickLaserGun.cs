using UnityEngine;

public class ThickLaserGun : LaserGun
{
	public float beamRadius = 0.5f;

	protected override bool BeamCast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float distance, int collisionLayers)
	{
		return Physics.SphereCast(origin, beamRadius, direction, out hitInfo, distance, collisionLayers);
	}
}
