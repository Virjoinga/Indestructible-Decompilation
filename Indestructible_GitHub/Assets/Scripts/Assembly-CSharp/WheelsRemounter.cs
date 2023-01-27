using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/Wheels Remounter")]
public class WheelsRemounter : Glu.MonoBehaviour, IMountable
{
	public Transform[] wheels;

	public void Mounted(Vehicle vehicle)
	{
		GameObject[] wheelVisualPrefabs = vehicle.GetComponent<WheeledVehicleFX>().wheelVisualPrefabs;
		int i = 0;
		for (int num = wheels.Length; i != num; i++)
		{
			Transform component = wheelVisualPrefabs[i].GetComponent<Transform>();
			Transform transform = wheels[i];
			transform.parent = component.parent;
			transform.localPosition = component.localPosition;
			transform.localRotation = component.localRotation;
			component.parent = null;
			Object.Destroy(wheelVisualPrefabs[i]);
			wheelVisualPrefabs[i] = transform.gameObject;
		}
		Object.Destroy(this);
	}

	public void WillUnmount(Vehicle vehicle)
	{
	}
}
