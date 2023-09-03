using UnityEngine;

public class ItemConsumer : MonoBehaviour
{
	public enum CargoItemOperation
	{
		None = 0,
		Loaded = 1,
		Dropped = 2,
		Replaced = 3
	}

	public delegate void CargoItemChangedDelegate(GameObject cargo, CargoItemOperation operation);

	public Vector3 CarryItemOffset;

	private Vehicle _vehicle;

	private bool _isCarryingCargo;

	public bool IsCarryingCargo
	{
		get
		{
			return _isCarryingCargo;
		}
	}

	public event CargoItemChangedDelegate CargoItemChangedEvent;

	private void Start()
	{
		_vehicle = base.gameObject.GetComponent<Vehicle>();
	}

	public virtual bool CanConsume(CollectableItem Item)
	{
		if ((bool)_vehicle)
		{
			switch (Item.ItemType)
			{
			case CollectableItemType.Health:
				return _vehicle.destructible.hp < _vehicle.destructible.GetMaxHP();
			case CollectableItemType.Energy:
				return _vehicle.energy < _vehicle.GetMaxEnergy();
			}
		}
		return true;
	}

	public virtual void Consume(CollectableItem Item, bool isMine)
	{
		switch (Item.ItemType)
		{
		case CollectableItemType.Health:
			if (isMine)
			{
				_vehicle.destructible.Heal(Item.AffectValue);
			}
			break;
		case CollectableItemType.Flag:
			break;
		default:
		{
			Buff buff = Item.CarryBuff.CreateBuff();
			_vehicle.buffSystem.AddInstancedBuff(buff, base.gameObject, false);
			break;
		}
		case CollectableItemType.Energy:
			if (isMine)
			{
				_vehicle.OnEnergyPickedUp();
			}
			break;
		}
	}

	public void SetCargoItem(GameObject cargo)
	{
		CargoItemOperation cargoItemOperation = CargoItemOperation.None;
		if (_isCarryingCargo)
		{
			if (cargo == null)
			{
				_isCarryingCargo = false;
				cargoItemOperation = CargoItemOperation.Dropped;
			}
			else
			{
				cargoItemOperation = CargoItemOperation.Replaced;
			}
		}
		else if (cargo != null)
		{
			_isCarryingCargo = true;
			cargoItemOperation = CargoItemOperation.Loaded;
		}
		if (this.CargoItemChangedEvent != null && cargoItemOperation != 0)
		{
			this.CargoItemChangedEvent(cargo, cargoItemOperation);
		}
	}
}
