using UnityEngine;

[AddComponentMenu("Indestructible/CR/Placeable ChargeItem (Base)")]
public class ChargeItem : MonoBehaviour
{
	private Transform _transform;

	private Collider[] _killBoxesColliders;

	private Collider _collider;

	private CRTeamGame _crGame;

	protected ItemConsumer _carrierObjectConsumer;

	protected void Start()
	{
		_transform = base.transform;
		_collider = base.collider;
		_crGame = IDTGame.Instance as CRTeamGame;
		Object[] array = Object.FindObjectsOfType(typeof(KillBox));
		if (array.Length > 0)
		{
			_killBoxesColliders = new Collider[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				_killBoxesColliders[i] = ((KillBox)array[i]).collider;
			}
		}
	}

	private void DetachFromParent()
	{
		if ((bool)_transform)
		{
			_transform.parent = null;
			_transform.rotation = Quaternion.identity;
			if ((bool)_carrierObjectConsumer)
			{
				_carrierObjectConsumer.SetCargoItem(null);
			}
			_carrierObjectConsumer = null;
		}
	}

	public bool CheckDropPosition(out Vector3 pos)
	{
		float num = 1f;
		pos = Vector3.zero;
		RaycastHit hitInfo = default(RaycastHit);
		Ray ray = new Ray(base.transform.position + num * Vector3.up, Vector3.down);
		float distance = 300f;
		if (Physics.Raycast(ray, out hitInfo, distance, 1 << LayerMask.NameToLayer("Default")))
		{
			if (_killBoxesColliders != null)
			{
				Collider[] killBoxesColliders = _killBoxesColliders;
				foreach (Collider collider in killBoxesColliders)
				{
					if (collider != null && (collider.bounds.Contains(hitInfo.point) || collider.bounds.Contains(base.transform.position)))
					{
						return false;
					}
				}
			}
			pos = hitInfo.point;
			return true;
		}
		return false;
	}

	public void HideTillRespawn()
	{
		DetachFromParent();
		base.gameObject.SetActiveRecursively(false);
	}

	public void SetOnPos(Vector3 pos)
	{
		base.gameObject.SetActiveRecursively(true);
		DetachFromParent();
		_collider.enabled = true;
		_transform.localPosition = pos;
		_transform.localRotation = Quaternion.identity;
	}

	public void AttachToVehicle(Vehicle vehicle)
	{
		DetachFromParent();
		_collider.enabled = false;
		_carrierObjectConsumer = vehicle.gameObject.GetComponent<ItemConsumer>();
		_transform.parent = vehicle.gameObject.transform;
		_transform.localPosition = ((!_carrierObjectConsumer) ? Vector3.zero : _carrierObjectConsumer.CarryItemOffset);
		_transform.localRotation = Quaternion.identity;
		if ((bool)_carrierObjectConsumer)
		{
			_carrierObjectConsumer.SetCargoItem(base.gameObject);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (_crGame != null)
		{
			PlayerVehicle component = collider.GetComponent<PlayerVehicle>();
			if ((bool)component)
			{
				_crGame.PlayerLeaveCharge(component);
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (_crGame != null)
		{
			PlayerVehicle component = collider.GetComponent<PlayerVehicle>();
			if ((bool)component)
			{
				_crGame.PlayerStartLoadingCharge(component);
			}
		}
	}
}
