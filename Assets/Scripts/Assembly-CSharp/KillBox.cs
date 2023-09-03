using System.Collections;
using UnityEngine;

public class KillBox : MonoBehaviour
{
	public float DestroyDelay = 1f;

	private FollowingCameraFixedDirection _followCamera;

	private void Start()
	{
		_followCamera = Camera.mainCamera.GetComponent<FollowingCameraFixedDirection>();
	}

	private void OnTriggerEnter(Collider collider)
	{
		Vehicle playerVehicle = VehiclesManager.instance.playerVehicle;
		if (playerVehicle != null && playerVehicle.mainOwnerCollider == collider)
		{
			if ((bool)_followCamera)
			{
				_followCamera.target = null;
			}
			StartCoroutine(DelayedDestroy(playerVehicle.destructible));
		}
		else
		{
			KillObject(collider, false);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		KillObject(other, true);
	}

	private void KillObject(Collider objCollider, bool instant)
	{
		Destructible component = objCollider.gameObject.GetComponent<Destructible>();
		if (!(component == null) && component.isMine)
		{
			if (instant)
			{
				component.Die(DestructionReason.Killbox);
			}
			else
			{
				StartCoroutine(DelayedDestroy(component));
			}
		}
	}

	private void EnableOnMaster()
	{
		if (PhotonNetwork.isMasterClient)
		{
			base.collider.enabled = true;
		}
		else
		{
			base.collider.enabled = false;
		}
	}

	private IEnumerator DelayedDestroy(Destructible destructible)
	{
		if (!(destructible == null))
		{
			yield return new WaitForSeconds(DestroyDelay);
			destructible.Die(DestructionReason.Killbox);
		}
	}
}
