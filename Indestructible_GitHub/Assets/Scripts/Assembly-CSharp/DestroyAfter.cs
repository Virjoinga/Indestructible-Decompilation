using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
	public float destroyAfter = 15f;

	private void Start()
	{
		Object.Destroy(base.gameObject, destroyAfter);
	}
}
