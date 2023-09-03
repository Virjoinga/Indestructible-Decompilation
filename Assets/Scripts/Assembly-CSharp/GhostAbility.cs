using System;
using System.Collections;
using UnityEngine;

public class GhostAbility : CooldownAbility
{
	public enum State
	{
		Inactive = 0,
		Active = 1,
		FadeIn = 2
	}

	public GameObject transitionEffectPrefab;

	public float transitionEffectSize = 45f;

	public float transitionEffectLifetime = 0.33f;

	public Material[] mineInvisibleMaterials;

	public float maxAbilityDuration = 10f;

	private State _state;

	private float _maxAbilityFinishTime;

	private int _refShotsCount;

	private ItemConsumer _itemConsumer;

	private Renderer _renderer;

	private SkelController _skelController;

	private EnemyInfoIndicatorController _infoIndicatorController;

	private Renderer _teamIndicatorRenderer;

	private Material[] _origMaterials;

	private PlayerBuffIndicator _buffIndicator;

	private ParticleEffect _transitionEffect;

	public State state
	{
		get
		{
			return _state;
		}
	}

	public event Action<GhostAbility> ghostAbilityDeactivatedEvent;

	protected override void Start()
	{
		base.Start();
		_renderer = base.GetComponent<Renderer>();
		_skelController = GetComponent<SkelController>();
		_origMaterials = _renderer.materials;
		_infoIndicatorController = GetComponent<EnemyInfoIndicatorController>();
		VehicleTeamIndicator componentInChildren = GetComponentInChildren<VehicleTeamIndicator>();
		if (transitionEffectPrefab != null)
		{
			_transitionEffect = EffectManager.instance.GetParticleEffect(transitionEffectPrefab);
		}
		if (componentInChildren != null)
		{
			_teamIndicatorRenderer = componentInChildren.GetComponent<Renderer>();
		}
		_itemConsumer = GetComponent<ItemConsumer>();
		if (_itemConsumer != null)
		{
			_itemConsumer.CargoItemChangedEvent += CargoChanged;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (state == State.Active)
		{
			Deactivate();
		}
	}

	private void OnDestroy()
	{
		if (_itemConsumer != null)
		{
			_itemConsumer.CargoItemChangedEvent -= CargoChanged;
		}
	}

	public override bool CanActivateAbility()
	{
		return base.CanActivateAbility() && !_itemConsumer.IsCarryingCargo;
	}

	[RPC]
	protected override void AbilityActivated()
	{
		base.AbilityActivated();
		Activate();
	}

	private void Activate()
	{
		if (_renderer.isVisible && _transitionEffect != null)
		{
			_transitionEffect.Emit(base.vehicle.transform.localPosition, Vector3.zero, transitionEffectSize, transitionEffectLifetime);
		}
		if (VehiclesManager.instance.playerVehicle == base.vehicle)
		{
			_renderer.materials = mineInvisibleMaterials;
			_skelController.materials = mineInvisibleMaterials;
			_buffIndicator = PlayerBuffsIndicators.Instance.AddIndicator("sm_invisibility", maxAbilityDuration);
		}
		else
		{
			base.GetComponent<Renderer>().enabled = false;
			if (_infoIndicatorController != null)
			{
				_infoIndicatorController.indicatorEnabled = false;
			}
			if (_teamIndicatorRenderer != null)
			{
				_teamIndicatorRenderer.enabled = false;
			}
		}
		_refShotsCount = base.vehicle.weapon.shotsCount;
		_maxAbilityFinishTime = Time.time + maxAbilityDuration;
		_state = State.Active;
	}

	private IEnumerator FadeIn()
	{
		_state = State.FadeIn;
		if (_transitionEffect != null)
		{
			_transitionEffect.Emit(base.vehicle.transform.localPosition, Vector3.zero, transitionEffectSize, transitionEffectLifetime);
		}
		yield return null;
		Deactivate();
	}

	private void Deactivate()
	{
		_renderer.materials = _origMaterials;
		_renderer.enabled = true;
		_skelController.materials = _origMaterials;
		if (_infoIndicatorController != null)
		{
			_infoIndicatorController.indicatorEnabled = true;
		}
		if (_teamIndicatorRenderer != null)
		{
			_teamIndicatorRenderer.enabled = true;
		}
		if (_buffIndicator != null)
		{
			PlayerBuffsIndicators.Instance.Deactivate(_buffIndicator);
			_buffIndicator = null;
		}
		_state = State.Inactive;
		if (this.ghostAbilityDeactivatedEvent != null)
		{
			this.ghostAbilityDeactivatedEvent(this);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (_state == State.Active && (_maxAbilityFinishTime < Time.time || _refShotsCount != base.vehicle.weapon.shotsCount))
		{
			StartCoroutine(FadeIn());
		}
	}

	private void CargoChanged(GameObject cargo, ItemConsumer.CargoItemOperation operation)
	{
		if (_state == State.Active)
		{
			StartCoroutine(FadeIn());
		}
	}
}
