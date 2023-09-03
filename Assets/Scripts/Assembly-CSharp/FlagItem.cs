using System.Collections;
using UnityEngine;

[AddComponentMenu("Indestructible/CTF/Placeable Flag (Base)")]
public class FlagItem : CollectableItem
{
	private enum State
	{
		Home = 0,
		Dropped = 1,
		Captured = 2
	}

	public int TeamID;

	public Transform BaseObject;

	public float AutoReturnTime = 15f;

	private static FlagItem[] Flags = new FlagItem[2];

	private State _state;

	private CTFGame _ctfGame;

	private YieldInstruction _autoReturnYI;

	protected Destructible _carrierObjectDestructible;

	protected ItemConsumer _carrierObjectConsumer;

	public bool IsOnBase
	{
		get
		{
			return _transform.parent == BaseObject;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.tag = "CTF_Flag";
		if (TeamID >= Flags.Length)
		{
			Debug.LogError("Team ID (" + TeamID + ") is out of range");
		}
		Flags[TeamID] = this;
	}

	protected override void Start()
	{
		base.Start();
		_ctfGame = IDTGame.Instance as CTFGame;
		if (_ctfGame != null)
		{
			base.enabled = false;
		}
		_active = true;
		_autoReturnYI = new WaitForSeconds(AutoReturnTime);
	}

	public static FlagItem GetFlagInstanceByTeam(int team)
	{
		if (team >= Flags.Length || team < 0)
		{
			return null;
		}
		return Flags[team];
	}

	private void DetachFromParent()
	{
		if ((bool)_transform)
		{
			if (_transform.parent != null)
			{
				_transform.parent = null;
			}
			_transform.rotation = Quaternion.identity;
			if ((bool)_carrierObjectConsumer)
			{
				_carrierObjectConsumer.SetCargoItem(null);
			}
			_carrierObjectConsumer = null;
		}
	}

	public void Drop()
	{
		if (_state != State.Dropped)
		{
			if ((bool)_carrierObjectDestructible)
			{
				_carrierObjectDestructible.destructedEvent -= OnCarrierDestructed;
			}
			_carrierObjectDestructible = null;
			DetachFromParent();
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(new Ray(base.transform.position, Vector3.down), out hitInfo, 300f, 1 << LayerMask.NameToLayer("Default")))
			{
				base.transform.position = hitInfo.point;
			}
			StartCoroutine("AutoReturn");
			EnableCollider(true);
			_active = true;
			_state = State.Dropped;
		}
	}

	private IEnumerator AutoReturn()
	{
		yield return _autoReturnYI;
		ReturnFlagRPC();
	}

	protected override void Consume(GameObject consumerGO)
	{
		Vehicle component = consumerGO.GetComponent<Vehicle>();
		if (!component)
		{
			return;
		}
		GamePlayer gamePlayer = component.player as GamePlayer;
		if (TeamID == gamePlayer.teamID)
		{
			if (IsOnBase)
			{
				FlagItem componentInChildren = consumerGO.GetComponentInChildren<FlagItem>();
				if (componentInChildren != null)
				{
					_ctfGame.FlagDelivered(gamePlayer);
					componentInChildren.ReturnFlagRPC();
				}
			}
			else
			{
				_ctfGame.FlagReturned(gamePlayer);
				ReturnFlagRPC();
			}
		}
		else
		{
			PickUpFlagGO(consumerGO);
			if ((bool)_photonView)
			{
				_photonView.RPC("PickUpFlag", PhotonTargets.Others, gamePlayer.id);
			}
			_ctfGame.FlagCaptured(gamePlayer);
		}
	}

	private void PickUpFlagGO(GameObject consumerGO)
	{
		if (_state != State.Captured)
		{
			if (_endingFXCache != null)
			{
				_endingFXCache.Activate(_transform.position, _transform.rotation);
			}
			DetachFromParent();
			_active = false;
			EnableCollider(false);
			_carrierObjectConsumer = consumerGO.GetComponent<ItemConsumer>();
			_transform.parent = consumerGO.transform;
			_transform.localPosition = ((!_carrierObjectConsumer) ? Vector3.zero : _carrierObjectConsumer.CarryItemOffset);
			_transform.localRotation = Quaternion.identity;
			if ((bool)_carrierObjectConsumer)
			{
				_carrierObjectConsumer.SetCargoItem(base.gameObject);
			}
			Destructible componentInChildren = consumerGO.GetComponentInChildren<Destructible>();
			if ((bool)componentInChildren)
			{
				componentInChildren.destructedEvent += OnCarrierDestructed;
				_carrierObjectDestructible = componentInChildren;
			}
			StopCoroutine("AutoReturn");
			_state = State.Captured;
		}
	}

	private void OnCarrierDestructed(Destructible destructed)
	{
		if (_carrierObjectDestructible == destructed)
		{
			Vehicle vehicle = _carrierObjectDestructible.vehicle;
			if (vehicle != null && vehicle.player != null)
			{
				_ctfGame.CourierKilled(vehicle.player as GamePlayer);
			}
		}
		Drop();
	}

	[RPC]
	private void PickUpFlag(int playerID)
	{
		GamePlayer player;
		if ((IDTGame.Instance as MultiplayerGame).TryGetPlayer(playerID, out player))
		{
			GameObject consumerGO = player.vehicle.gameObject;
			PickUpFlagGO(consumerGO);
		}
	}

	private void ReturnFlagRPC()
	{
		ReturnFlag();
		if ((bool)_photonView)
		{
			_photonView.RPC("ReturnFlag", PhotonTargets.Others);
		}
	}

	[RPC]
	private void ReturnFlag()
	{
		if (_state != 0)
		{
			_carrierObjectDestructible = null;
			DetachFromParent();
			_transform.parent = BaseObject;
			_transform.localPosition = Vector3.zero;
			_transform.rotation = Quaternion.identity;
			EnableCollider(false);
			EnableCollider(true);
			_active = true;
			_state = State.Home;
			_ctfGame.FlagAutoReturned(TeamID);
		}
	}

	[RPC]
	private new void Respawn()
	{
		Debug.LogWarning("Dont use Respawn() for flag");
	}

	[RPC]
	private void Consumed(int consumerViewId)
	{
		Debug.LogWarning("Dont use Consumed() for flag");
	}

	private void OnDestroy()
	{
	}
}
