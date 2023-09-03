using UnityEngine;

public class AIProtectionPoint : MonoBehaviour
{
	public float Radius = 10f;

	private void Awake()
	{
		base.gameObject.tag = "AIProtection";
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(base.transform.position, Radius);
	}
}
