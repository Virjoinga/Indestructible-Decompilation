using System;
using System.Collections;
using Glu.Localization;
using UnityEngine;

public class CollectableItem : MonoBehaviour
{
	public CollectableItemType ItemType;

	public float AffectValue;

	public BuffConf CarryBuff;

	public string NotificationId = string.Empty;

	public float RespawnTime = 7f;

	public GameObject StartingEffect;

	public GameObject EndingEffect;

	public AudioClip SpawnSound;

	public AudioClip PickupSound;

	protected Transform _transform;

	protected PhotonView _photonView;

	protected YieldInstruction _respawnDelayYI;

	protected MonoBehaviour[] _allComponents;

	protected Renderer[] _allRenderers;

	protected Collider _collider;

	protected bool _active;

	protected CachedObject.Cache _startingFXCache;

	protected CachedObject.Cache _endingFXCache;

	protected float _spawnTime;

	private AudioHelper _audioHelper;

	public float SpawnTime
	{
		get
		{
			return _spawnTime;
		}
	}

	public bool IsActive
	{
		get
		{
			return _active;
		}
	}

	public event Action OnConsumedEvent;

	protected virtual void Awake()
	{
		_audioHelper = new AudioHelper(GetComponent<AudioSource>(), false, false);
		if (SpawnSound == null)
		{
			SpawnSound = _audioHelper.clip;
		}
	}

	protected virtual void Start()
	{
		_transform = base.gameObject.transform;
		_photonView = ((PhotonNetwork.room == null) ? null : GetComponent<PhotonView>());
		_respawnDelayYI = new WaitForSeconds(RespawnTime);
		_collider = base.gameObject.GetComponentInChildren<Collider>();
		if ((bool)StartingEffect)
		{
			_startingFXCache = ObjectCacheManager.Instance.GetCache(StartingEffect);
		}
		if ((bool)EndingEffect)
		{
			_endingFXCache = ObjectCacheManager.Instance.GetCache(EndingEffect);
		}
		RestartItem();
	}

	private void OnDestroy()
	{
		_audioHelper.Dispose();
	}

	private void OnMasterClientSwitched(PhotonPlayer player)
	{
		EnableCollider(true);
	}

	private void OnActivate()
	{
		RestartItem();
	}

	private void RestartItem()
	{
		if (_startingFXCache != null)
		{
			_startingFXCache.Activate(_transform.position, _transform.rotation);
		}
		_audioHelper.clip = SpawnSound;
		_audioHelper.PlayIfEnabled();
		EnableCollider(true);
		_active = true;
		_spawnTime = Time.time;
	}

	protected virtual void Consume(GameObject colGO)
	{
		PhotonView componentInChildren = colGO.GetComponentInChildren<PhotonView>();
		int num = ((!componentInChildren) ? (-1) : componentInChildren.viewID.ID);
		Consumed(num);
		if ((bool)_photonView)
		{
			_photonView.RPC("Consumed", PhotonTargets.Others, num);
		}
	}

	private void OnTriggerEnter(Collider ColliderObj)
	{
		if (_active)
		{
			GameObject gameObject = ColliderObj.gameObject;
			ItemConsumer componentInChildren = gameObject.GetComponentInChildren<ItemConsumer>();
			if (componentInChildren != null && componentInChildren.enabled && componentInChildren.CanConsume(this))
			{
				Consume(gameObject);
			}
		}
	}

	protected void Enable(bool enable)
	{
		_allComponents = _allComponents ?? base.gameObject.GetComponentsInChildren<MonoBehaviour>();
		MonoBehaviour[] allComponents = _allComponents;
		foreach (MonoBehaviour monoBehaviour in allComponents)
		{
			if (monoBehaviour != this)
			{
				monoBehaviour.enabled = enable;
			}
		}
		_allRenderers = _allRenderers ?? base.gameObject.GetComponentsInChildren<Renderer>();
		Renderer[] allRenderers = _allRenderers;
		foreach (Renderer renderer in allRenderers)
		{
			renderer.enabled = enable;
		}
		_active = enable;
	}

	protected IEnumerator RespawnCoroutine()
	{
		Debug.Log("RespawnCoroutine");
		yield return _respawnDelayYI;
		SendRespawnRPC();
	}

	protected virtual void SendRespawnRPC()
	{
		if (PhotonNetwork.isMasterClient)
		{
			if ((bool)_photonView)
			{
				_photonView.RPC("Respawn", PhotonTargets.All);
			}
			else
			{
				Respawn();
			}
		}
	}

	protected virtual void InternalRespawn()
	{
		Enable(true);
		RestartItem();
	}

	protected virtual void InternalConsumed(int consumerViewId)
	{
		if (_endingFXCache != null)
		{
			_endingFXCache.Activate(_transform.position, _transform.rotation);
		}
		_audioHelper.clip = PickupSound;
		_audioHelper.PlayIfEnabled();
		PhotonView photonView = PhotonView.Find(consumerViewId);
		if ((bool)photonView)
		{
			Vehicle component = photonView.GetComponent<Vehicle>();
			MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
			bool isMine = VehiclesManager.instance.playerVehicle == component || (multiplayerGame != null && !multiplayerGame.match.isOnline);
			ItemConsumer componentInChildren = photonView.GetComponentInChildren<ItemConsumer>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Consume(this, isMine);
			}
			if (VehiclesManager.instance.playerVehicle == component)
			{
				if (NotificationId != string.Empty)
				{
					string @string = Strings.GetString(NotificationId);
					MonoSingleton<NotificationsQueue>.Instance.AddText(@string);
				}
				MonoSingleton<Player>.Instance.Statistics.Update(this);
			}
			if (this.OnConsumedEvent != null)
			{
				this.OnConsumedEvent();
			}
		}
		StartRespawn();
	}

	public virtual void StartRespawn()
	{
		EnableCollider(false);
		_spawnTime = Time.time + RespawnTime;
		if (_active)
		{
			StartCoroutine(RespawnCoroutine());
		}
		Enable(false);
	}

	[RPC]
	protected virtual void Respawn()
	{
		InternalRespawn();
	}

	[RPC]
	private void Consumed(int consumerViewId)
	{
		InternalConsumed(consumerViewId);
	}

	protected void EnableCollider(bool enable)
	{
		if ((bool)_collider)
		{
			_collider.enabled = enable && (PhotonNetwork.room == null || PhotonNetwork.isMasterClient);
		}
	}
}
