using System.Collections;
using Glu;
using UnityEngine;

public class AutoGunController : Glu.MonoBehaviour
{
	public GunTurret gunTurret;

	private PhotonView _photonView;

	private bool _isMine;

	private void Awake()
	{
		if (PhotonNetwork.room != null)
		{
			_photonView = GetComponent<PhotonView>();
		}
	}

	private void OnEnable()
	{
		CheckOwnership();
		StartCoroutine(SubscribeToGunTurret());
	}

	private void OnDisable()
	{
		gunTurret.aimTargetChangedEvent -= AimTargetChanged;
		gunTurret.StopDetectAimTarget();
	}

	private IEnumerator SubscribeToGunTurret()
	{
		yield return null;
		gunTurret.aimTargetChangedEvent += AimTargetChanged;
		if (_isMine)
		{
			gunTurret.StartDetectAimTarget();
		}
	}

	private void AimTargetChanged(Collider target, GunTurret turret)
	{
		turret.weapon.shouldFire = target != null;
		if (!_isMine || !(_photonView != null))
		{
			return;
		}
		int num = -1;
		if (target != null)
		{
			Destructible component = target.transform.root.GetComponent<Destructible>();
			if (component != null)
			{
				num = component.id;
			}
		}
		_photonView.RPC("ChangeAimTarget", PhotonTargets.Others, num);
	}

	[RPC]
	private void ChangeAimTarget(int destructibleID)
	{
		Collider aimTarget = null;
		if (0 <= destructibleID)
		{
			aimTarget = Destructible.Find(destructibleID).mainDamageCollider;
		}
		gunTurret.aimTarget = aimTarget;
	}

	private void CheckOwnership()
	{
		_isMine = _photonView == null || _photonView.isMine;
	}

	private void OnMasterClientSwitched(PhotonPlayer player)
	{
		CheckOwnership();
		if (_isMine && base.enabled && base.gameObject.active)
		{
			gunTurret.StartDetectAimTarget();
		}
	}
}
