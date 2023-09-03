using UnityEngine;

public class EnergyBoostBuff : Buff
{
	public bool EnegryGainAbsoluteValue = true;

	public float EnegryGainBoost;

	public bool MaxEnegryAbsoluteValue = true;

	public float MaxEnegryBoost;

	public bool RestoreToMax = true;

	private ModStackFloat _maxEnergyMod;

	private ModStackFloat _energyGainMod;

	public EnergyBoostBuff(EnergyBoostConf config)
		: base(config)
	{
		EnegryGainAbsoluteValue = config.EnegryGainAbsoluteValue;
		EnegryGainBoost = config.EnegryGainBoost;
		MaxEnegryAbsoluteValue = config.MaxEnegryAbsoluteValue;
		MaxEnegryBoost = config.MaxEnegryBoost;
		RestoreToMax = config.RestoreToMax;
	}

	protected override void OnStart()
	{
		if (_buffSystem != null)
		{
			_maxEnergyMod = _buffSystem.SetupModStack(_vehicle.GetBaseMaxEnergy, _vehicle.SetMaxEnergy, ModMaxEnergy, 0);
			_energyGainMod = _buffSystem.SetupModStack(_vehicle.GetBaseEnergyGainRate, _vehicle.SetEnergyGainRate, ModEnergyGain, 0);
			_maxEnergyMod.Recalculate();
			_energyGainMod.Recalculate();
			if (RestoreToMax)
			{
				_vehicle.AddEnergy(_maxEnergyMod.Value);
			}
		}
	}

	protected override void OnFinish()
	{
		if (_buffSystem != null)
		{
			_maxEnergyMod.RemoveValueModifier(ModMaxEnergy);
			_energyGainMod.RemoveValueModifier(ModEnergyGain);
		}
		base.OnFinish();
	}

	protected override void OnNewStackCount(int deltaStack)
	{
		base.OnNewStackCount(deltaStack);
		if (_maxEnergyMod != null)
		{
			_maxEnergyMod.Recalculate();
		}
		if (_energyGainMod != null)
		{
			_energyGainMod.Recalculate();
		}
		if (RestoreToMax && _maxEnergyMod != null)
		{
			Debug.Log("ADD ENERGY " + _maxEnergyMod.Value);
			_vehicle.AddEnergy(_maxEnergyMod.Value);
		}
	}

	public float ModMaxEnergy(float modifiedMaxEnergy, float baseMaxEnergy)
	{
		float num = ((!MaxEnegryAbsoluteValue) ? (baseMaxEnergy * MaxEnegryBoost) : MaxEnegryBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(0f, modifiedMaxEnergy + num);
	}

	public float ModEnergyGain(float modifiedEnergyGain, float baseEnergyGain)
	{
		float num = ((!EnegryGainAbsoluteValue) ? (baseEnergyGain * EnegryGainBoost) : EnegryGainBoost) * base.effectScale * (float)Stacks;
		return Mathf.Max(0f, modifiedEnergyGain + num);
	}

	public override void GetModifyInfo(BuffModifyInfo baseInfo, ref BuffModifyInfo info)
	{
		info.Energy = ModMaxEnergy(info.Energy, baseInfo.Energy);
	}
}
