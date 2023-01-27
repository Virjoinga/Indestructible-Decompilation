using System.Collections;
using UnityEngine;

public class DeactivateAfter : MonoBehaviour
{
	public float DeactivateTime = 15f;

	public void OnEnable()
	{
		StartCoroutine(DelayedDeactivate());
	}

	private IEnumerator DelayedDeactivate()
	{
		yield return new WaitForSeconds(DeactivateTime);
		base.gameObject.transform.parent = null;
		base.gameObject.SetActiveRecursively(false);
	}
}
