using Glu;
using UnityEngine;

public class RPGSystem : Glu.MonoBehaviour
{
	public float DefaultHealValue = 100f;

	public float DefaultEnergyValue = 100f;

	public float DefaultMaxEnergy = 100f;

	public float DefaultEnergyGainRate = 2.5f;

	private Destructible _destructible;

	private float _energy;

	protected float _maxEnergy;

	protected float _energyGainRate;

	public float MaxEnergy
	{
		get
		{
			return _maxEnergy;
		}
	}

	public float EnergyGainRate
	{
		get
		{
			return _energyGainRate;
		}
	}

	public float Energy
	{
		get
		{
			return _energy;
		}
	}

	public bool IsFullEnergy
	{
		get
		{
			return _energy >= MaxEnergy;
		}
	}

	public bool IsFullHealth
	{
		get
		{
			if ((bool)_destructible)
			{
				return _destructible.hp >= _destructible.GetMaxHP();
			}
			return true;
		}
	}

	public float GetBaseMaxEnergy()
	{
		return DefaultEnergyValue;
	}

	public float GetMaxEnergy()
	{
		return _maxEnergy;
	}

	public void SetMaxEnergy(float newMaxEnergy)
	{
		_maxEnergy = newMaxEnergy;
	}

	public float GetBaseEnergyGainRate()
	{
		return DefaultEnergyGainRate;
	}

	public float GetEnergyGainRate()
	{
		return _energyGainRate;
	}

	public void SetEnergyGainRate(float newEnergyGainRate)
	{
		_energyGainRate = newEnergyGainRate;
	}

	private void Awake()
	{
		_energyGainRate = DefaultEnergyGainRate;
		_maxEnergy = DefaultMaxEnergy;
		_energy = DefaultMaxEnergy;
	}

	private void OnEnable()
	{
		_energy = _maxEnergy;
	}

	private void Start()
	{
		_destructible = base.gameObject.GetComponent<Destructible>();
	}

	public void OnHealthPickedUp(float healValue)
	{
		if (!(_destructible == null))
		{
			_destructible.Heal(healValue);
		}
	}

	public bool HasSufficientEnergy(float Value)
	{
		return Value <= _energy;
	}

	public bool TryConsumeEnergy(float value)
	{
		float num = _energy - value;
		if (num < 0f)
		{
			return false;
		}
		_energy = num;
		return true;
	}

	public bool TryConsumeEnergy(float value, float minReserve)
	{
		if (_energy < minReserve)
		{
			return false;
		}
		_energy -= value;
		return true;
	}

	public void ConsumeEnergy(float Value)
	{
		_energy -= Value;
	}

	public void OnEnergyPickedUp()
	{
		_energy = Mathf.Clamp(_energy + DefaultEnergyValue, 0f, _maxEnergy);
	}

	private void RegenerateEnergy()
	{
		_energy = Mathf.Clamp(_energy + _energyGainRate * Time.deltaTime, 0f, _maxEnergy);
	}

	public void AddEnergy(float addValue)
	{
		_energy = Mathf.Clamp(_energy + addValue, 0f, _maxEnergy);
	}

	private void Update()
	{
		RegenerateEnergy();
	}
}
