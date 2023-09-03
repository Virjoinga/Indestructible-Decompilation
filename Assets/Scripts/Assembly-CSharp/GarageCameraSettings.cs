using UnityEngine;

public class GarageCameraSettings : MonoBehaviour
{
	public float Distance = 10f;

	private void Start()
	{
		Camera main = Camera.main;
		GarageCameraController component = main.GetComponent<GarageCameraController>();
		component.SetTargetDistance(Distance);
	}
}
