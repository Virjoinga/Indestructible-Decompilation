using UnityEngine;

public class NetCachedObject : CachedObject
{
	public class NetCache : StackCache
	{
		public NetCache(GameObject prefab)
			: base(prefab)
		{
		}

		protected override CachedObject InstantiateCachedObject()
		{
			return PhotonNetwork.Instantiate(_prefab.name, Vector3.zero, Quaternion.identity, 2).GetComponent<CachedObject>();
		}
	}

	private PhotonView _photonView;

	public override Cache CreateCache(GameObject prefab)
	{
		return new NetCache(prefab);
	}

	protected override void Awake()
	{
		base.Awake();
		_photonView = GetComponent<PhotonView>();
	}

	public override void Activate(Vector3 pos, Quaternion rot)
	{
		Activate();
		RemoteSetTransform(pos, rot);
		_photonView.RPC("RemoteSetTransform", PhotonTargets.Others, pos, rot);
	}

	public override void Activate(Vector3 pos)
	{
		Activate();
		RemoteSetPos(pos);
		_photonView.RPC("RemoteSetPos", PhotonTargets.Others, pos);
	}

	public override void Activate()
	{
		RemoteActivate();
		_photonView.RPC("RemoteActivate", PhotonTargets.Others);
	}

	public override void Deactivate()
	{
		_photonView.RPC("RemoteDeactivate", PhotonTargets.Others);
		RemoteDeactivate();
		if (base.cache != null)
		{
			base.cache.Deactivated(this);
		}
	}

	//[RPC]
	public virtual void RemoteActivate()
	{
		base.gameObject.SetActiveRecursively(true);
	}

	//[RPC]
	public virtual void RemoteDeactivate()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	//[RPC]
	public virtual void RemoteSetTransform(Vector3 pos, Quaternion rotation)
	{
		base.transform.localPosition = pos;
		base.transform.localRotation = rotation;
	}

	//[RPC]
	public virtual void RemoteSetPos(Vector3 pos)
	{
		base.transform.localPosition = pos;
	}
}
