using System;
using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/VehicleFX")]
public abstract class VehicleFX : VisualFX
{
	public delegate void AvatarVisibilityChanged();

	public EffectOptions exhaustOptions;

	public float exhaustThrottleEmissionFactor;

	public EffectOptions boostOptions;

	public float boostThrottleEmissionFactor;

	private float _exhaustFactor;

	private Effect _exhaustEffect;

	private float _boostFactor;

	private Effect _boostEffect;

	private Vehicle _vehicle;

	private Engine _engine;

	public float exhaustFactor
	{
		get
		{
			return _exhaustFactor;
		}
		set
		{
			if (0f < _exhaustEffect.emissionScale + exhaustThrottleEmissionFactor)
			{
				_exhaustFactor = value;
				base.enabled = shouldUpdate;
			}
		}
	}

	public float boostFactor
	{
		get
		{
			return _boostFactor;
		}
		set
		{
			if (0f < _boostEffect.emissionScale + boostThrottleEmissionFactor)
			{
				_boostFactor = value;
				base.enabled = shouldUpdate;
			}
		}
	}

	public Vehicle vehicle
	{
		get
		{
			return _vehicle;
		}
	}

	protected override bool shouldUpdate
	{
		get
		{
			return base.isVisible && (base.shouldUpdate || 0f < _exhaustFactor + _boostFactor);
		}
	}

	private event AvatarVisibilityChanged _avatarBecameVisibleEvent;

	private event AvatarVisibilityChanged _avatarBecameInvisibleEvent;

	public void SubscriveToAvatarVisibilityChange(AvatarVisibilityChanged becameVisibleDelegate, AvatarVisibilityChanged becameInvisibleDelegate)
	{
		if (PhotonNetwork.room == null)
		{
			return;
		}
		this._avatarBecameVisibleEvent = (AvatarVisibilityChanged)Delegate.Combine(this._avatarBecameVisibleEvent, becameVisibleDelegate);
		this._avatarBecameInvisibleEvent = (AvatarVisibilityChanged)Delegate.Combine(this._avatarBecameInvisibleEvent, becameInvisibleDelegate);
		if (_vehicle != null && !_vehicle.isMine)
		{
			if (base.isVisible)
			{
				becameVisibleDelegate();
			}
			else
			{
				becameInvisibleDelegate();
			}
		}
	}

	public void UnsubscriveFromAvatarVisibilityChange(AvatarVisibilityChanged becameVisibleDelegate, AvatarVisibilityChanged becameInvisibleDelegate)
	{
		this._avatarBecameVisibleEvent = (AvatarVisibilityChanged)Delegate.Remove(this._avatarBecameVisibleEvent, becameVisibleDelegate);
		this._avatarBecameInvisibleEvent = (AvatarVisibilityChanged)Delegate.Remove(this._avatarBecameInvisibleEvent, becameInvisibleDelegate);
	}

	protected override void Awake()
	{
		base.Awake();
		_vehicle = base.rootTransform.GetComponent<Vehicle>();
		_engine = _vehicle.GetComponent<Engine>();
		_vehicle.SubscribeToMountedEvent(PartsMounted);
	}

	protected override void InitEffectsOnAwake()
	{
	}

	protected override void InitEffects()
	{
		_exhaustEffect.Init(exhaustOptions, base.rootTransform);
		exhaustOptions = null;
		if (_exhaustEffect.effect == null)
		{
			exhaustThrottleEmissionFactor = 0f;
		}
		exhaustFactor = 1f;
		_boostEffect.Init(boostOptions, base.rootTransform);
		boostOptions = null;
		base.InitEffects();
	}

	protected override void UpdateEffects(Vector3 rootVelocity)
	{
		base.UpdateEffects(rootVelocity);
		if (0f < _exhaustFactor)
		{
			_exhaustEffect.emissionFactor = (_exhaustEffect.emissionScale + _engine.throttle * exhaustThrottleEmissionFactor) * _exhaustFactor;
			_exhaustEffect.Update(base.rootTransform, rootVelocity);
		}
		if (0f < _boostFactor)
		{
			_boostEffect.emissionFactor = (_boostEffect.emissionScale + _engine.throttle * boostThrottleEmissionFactor) * _boostFactor;
			_boostEffect.Update(base.rootTransform, rootVelocity);
		}
	}

	protected override void OnBecameVisible()
	{
		bool flag = base.isVisible;
		base.OnBecameVisible();
		if (!flag && !_vehicle.isMine && this._avatarBecameVisibleEvent != null)
		{
			this._avatarBecameVisibleEvent();
		}
	}

	protected override void OnBecameInvisible()
	{
		if (!_vehicle.isMine && this._avatarBecameInvisibleEvent != null)
		{
			this._avatarBecameInvisibleEvent();
		}
		base.OnBecameInvisible();
	}

	private void OnMasterClientSwitched(PhotonPlayer player)
	{
		if (!base.isVisible && PhotonNetwork.isMasterClient && this._avatarBecameVisibleEvent != null)
		{
			this._avatarBecameVisibleEvent();
		}
	}

	private void PartsMounted(Vehicle vehicle)
	{
		InitEffects();
	}
}
