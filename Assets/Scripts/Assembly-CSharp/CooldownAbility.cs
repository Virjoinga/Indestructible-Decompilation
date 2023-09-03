using Glu.Localization;
using UnityEngine;

public class CooldownAbility : BaseActiveAbility
{
	public float Cooldown = 1f;

	protected float _elapsedCooldown;

	private int _lastCooldownRound = -1;

	protected float _cooldown;

	private bool _prevEnergyNotEnough;

	public bool IsCooldownFinished
	{
		get
		{
			return _elapsedCooldown <= 0f;
		}
	}

	public float GetBaseCooldown()
	{
		return Cooldown;
	}

	public float GetCooldown()
	{
		return _cooldown;
	}

	public void SetCooldown(float newCooldown)
	{
		float a = Cooldown * 0.1f;
		_cooldown = Mathf.Max(a, newCooldown);
	}

	private void Awake()
	{
		_cooldown = Cooldown;
	}

	protected override void Start()
	{
		base.Start();
		SetButtonEnabled(true);
	}

	protected virtual void OnEnable()
	{
		_elapsedCooldown = 0f;
		SetButtonEnabled(true);
	}

	public override void ActivateAbility()
	{
		if (CanActivateAbility() && base.vehicle.TryConsumeEnergy(GetEnergyConsume()))
		{
			_elapsedCooldown = _cooldown;
			if (_elapsedCooldown > 0f)
			{
				SetButtonEnabled(false);
			}
			else
			{
				_elapsedCooldown = 0f;
			}
			OnAbilityStart();
			AbilityActivated();
			if ((bool)_photonView)
			{
				_photonView.RPC("AbilityActivated", PhotonTargets.Others);
			}
		}
	}

	public override bool CanActivateAbility()
	{
		return base.CanActivateAbility() && _elapsedCooldown == 0f && base.vehicle.HasEnergy(GetEnergyConsume());
	}

	protected virtual void OnCooldownFinished()
	{
		SetButtonEnabled(base.IsEnoughEnergy);
		UpdateBtnEnergyText();
	}

	protected virtual void OnAbilityReady()
	{
		SetButtonEnabled(true);
	}

	protected virtual void OnOutOfEnergy()
	{
		SetButtonEnabled(false);
		UpdateBtnEnergyText();
	}

	protected virtual void Update()
	{
		if (_elapsedCooldown > 0f)
		{
			_elapsedCooldown -= Time.deltaTime;
			if (_elapsedCooldown <= 0f)
			{
				_prevEnergyNotEnough = !base.IsEnoughEnergy;
				_elapsedCooldown = 0f;
				OnCooldownFinished();
			}
			UpdateButtonText();
		}
		else if (base.IsEnoughEnergy)
		{
			if (_prevEnergyNotEnough)
			{
				_prevEnergyNotEnough = false;
				OnAbilityReady();
			}
		}
		else if (!_prevEnergyNotEnough)
		{
			_prevEnergyNotEnough = true;
			OnOutOfEnergy();
		}
	}

	private void UpdateButtonText()
	{
		int num = Mathf.RoundToInt(_elapsedCooldown);
		if (_lastCooldownRound != num)
		{
			_lastCooldownRound = num;
			if (num != 0 && _abilityButton != null)
			{
				_abilityButton.Text = num.ToString();
			}
		}
	}

	protected override void UpdateBtnEnergyText()
	{
		if ((bool)_abilityButton)
		{
			string @string = Strings.GetString("IDS_GAMEPLAY_ABILITY_BUTTON");
			_abilityButton.Text = string.Format(@string, GetEnergyConsume());
		}
	}

	protected void SetButtonEnabled(bool enable)
	{
		if ((bool)_abilityButton)
		{
			if (enable)
			{
				_abilityButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
				UpdateBtnEnergyText();
			}
			else
			{
				_abilityButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
			}
		}
	}
}
