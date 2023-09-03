using System;
using Glu;
using UnityEngine;

public class BaseActiveAbility : Glu.MonoBehaviour
{
	public float EnergyCost;

	public string BaseButtonText = string.Empty;

	public AudioClip ActivationSound;

	protected float _energyConsume;

	protected AudioSource _audioSource;

	protected AudioHelper _audioHelper;

	protected PhotonView _photonView;

	protected GameObject _rootObject;

	protected BuffSystem _buffSystem;

	protected Destructible _destructible;

	protected UIButton _abilityButton;

	protected float _effectScale = 1f;

	private Vehicle _vehicle;

	public float EffectScale
	{
		get
		{
			return _effectScale;
		}
		set
		{
			_effectScale = ((!(value > 0f)) ? 0f : value);
		}
	}

	public Vehicle vehicle
	{
		get
		{
			return _vehicle;
		}
	}

	public bool IsEnoughEnergy
	{
		get
		{
			return !(_buffSystem != null) || _vehicle.HasEnergy(GetEnergyConsume());
		}
	}

	public event Action<BaseActiveAbility> AbilityActivatedEvent;

	public float GetBaseEnergyConsume()
	{
		return EnergyCost;
	}

	public float GetEnergyConsume()
	{
		return _energyConsume;
	}

	public void SetEnergyConsume(float newEnergyConsume)
	{
		_energyConsume = newEnergyConsume;
		UpdateBtnEnergyText();
	}

	public virtual bool CanActivateAbility()
	{
		return _destructible != null && _destructible.isActive;
	}

	protected virtual void Start()
	{
		_rootObject = base.transform.root.gameObject;
		_vehicle = _rootObject.GetComponent<Vehicle>();
		_buffSystem = _rootObject.GetComponent<BuffSystem>();
		_destructible = _rootObject.GetComponent<Destructible>();
		_photonView = _vehicle.photonView;
		_photonView = ((!(_photonView != null) || !_photonView.isMine) ? null : _photonView);
		_energyConsume = EnergyCost;
		if (_vehicle.isMine && _vehicle is PlayerVehicle)
		{
			GameObject gameObject = GameObject.Find("BoostAbilityButton");
			_abilityButton = gameObject.GetComponent<UIButton>();
			AbilityActiveButton component = gameObject.GetComponent<AbilityActiveButton>();
			component.Label.Text = BaseButtonText + _energyConsume;
			component.OnTapDelegate = OnButtonPress;
		}
	}

	protected virtual void UpdateBtnEnergyText()
	{
		if ((bool)_abilityButton)
		{
			AbilityActiveButton component = _abilityButton.gameObject.GetComponent<AbilityActiveButton>();
			component.Label.Text = _energyConsume.ToString();
		}
	}

	public void OnButtonPress()
	{
		if (base.enabled)
		{
			ActivateAbility();
		}
	}

	public virtual void ActivateAbility()
	{
		if (CanActivateAbility())
		{
			OnAbilityStart();
		}
	}

	protected virtual void OnAbilityStart()
	{
		MonoSingleton<Player>.Instance.Statistics.Update(this);
	}

	protected virtual void AbilityActivated()
	{
		PlayActivationSound();
		if (this.AbilityActivatedEvent != null)
		{
			this.AbilityActivatedEvent(this);
		}
	}

	protected void PlayActivationSound()
	{
		if (ActivationSound != null)
		{
			if (_audioSource == null)
			{
				GameObject gameObject = new GameObject("AbilitySound");
				gameObject.transform.parent = base.gameObject.transform;
				_audioSource = gameObject.AddComponent<AudioSource>();
				_audioSource.playOnAwake = false;
				_audioHelper = new AudioHelper(_audioSource, false, false);
				_audioHelper.clip = ActivationSound;
			}
			_audioHelper.PlayIfEnabled();
		}
	}
}
