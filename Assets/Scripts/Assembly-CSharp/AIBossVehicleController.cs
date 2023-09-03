using System;
using System.Collections;
using System.Collections.Generic;
using Glu;
using UnityEngine;

public class AIBossVehicleController : Glu.MonoBehaviour
{
	[Serializable]
	public class FireParameters
	{
		public float Radius = 40f;

		public int AimTresholdAngle = 15;

		public float TargetUpdatePeriod = 0.5f;

		public float FireUpdatePeriod = 0.3f;
	}

	[Serializable]
	public class SkillParameters
	{
		public bool Use = true;

		public float UseUpdatePeriod = 0.5f;

		public float UseMinQuality = 0.5f;
	}

	[Serializable]
	public class MovementParameters
	{
		public enum FinalPointActionType
		{
			Stop = 0,
			Round = 1
		}

		private float _sensorMinCos = -1f;

		public float PathUpdatePeriod = 1f;

		public float PathMoveUpdatePeriod = 0.2f;

		public float PathPointRadius = 5f;

		public float FinalPointRadius = 15f;

		public FinalPointActionType FinalPointAction;

		public float SensorDistance = 5f;

		public float SensorNormalMaxAngle = 90f;

		public float SensorsYOffset = 0.5f;

		public float SensorsWidth = 3f;

		public bool UseChasmSensor = true;

		public float SensorsChasmYOffset = 0.5f;

		public float SensorChasmForward = 7f;

		public float SensorChasmDeep = 3f;

		public float SensorSteerAngle = 60f;

		public float StuckDiff = 0.3f;

		public float StuckReverseTime = 0.3f;

		public float RoundThrottle = 0.3f;

		public float RoundCorrectionAngle = 30f;

		public float SensorMinCos
		{
			get
			{
				if (SensorNormalMaxAngle > 90f)
				{
					return 0f;
				}
				if (_sensorMinCos < 0f)
				{
					_sensorMinCos = Mathf.Cos((float)Math.PI / 180f * SensorNormalMaxAngle);
				}
				return _sensorMinCos;
			}
		}
	}

	[Serializable]
	public class Requirement
	{
		public enum Type
		{
			Default = 0,
			HealthLess = 1,
			Random = 2
		}

		public Type CheckType;

		public int Value;
	}

	[Serializable]
	public class FindHPPowerupActionParams
	{
		public float TryPeriod = 5f;

		public float Timeout = 10f;

		public float ActivationHPLoss = 500f;

		public int ActivationPercent = 50;

		public float MaxTimeBeforeSpawn = 5f;
	}

	[Serializable]
	public class FindDDPowerupActionParams
	{
		public float TryPeriod = 5f;

		public float Timeout = 10f;

		public int ActivationPercent = 50;

		public float MaxTimeBeforeSpawn = 5f;
	}

	[Serializable]
	public class RandomMovementActionParams
	{
		public MovementParameters.FinalPointActionType FinalPointAction;

		public float FinalPointRadius = 2f;

		public int BlockedWayCounter = 2;

		public float Radius = 10f;

		public float TryPeriod = 1f;

		public float Timeout = 3f;

		public int ActivationPercent = 5;
	}

	[Serializable]
	public class AIActionsParams
	{
		public float UpdatePeriod = 0.5f;

		public bool AttackPlayer = true;

		public RandomMovementActionParams RandomMovement;

		public FindHPPowerupActionParams GetHPPowerup;

		public FindDDPowerupActionParams GetDDPowerup;
	}

	[Serializable]
	public class BehaviorParameters
	{
		public string Name = "Default";

		public Requirement ActivationRequirement;

		public MovementParameters Movement;

		public FireParameters Fire;

		public SkillParameters Skill;

		public AIActionsParams Actions;
	}

	private class StopableCoroutine : IEnumerator
	{
		private IEnumerator _coroutineEnumerator;

		private bool _stop;

		public bool IsStoped
		{
			get
			{
				return _stop;
			}
		}

		public object Current
		{
			get
			{
				return _coroutineEnumerator.Current;
			}
		}

		public StopableCoroutine(IEnumerator coroutineEnumerator)
		{
			_coroutineEnumerator = coroutineEnumerator;
		}

		public void Stop()
		{
			_stop = true;
		}

		public bool MoveNext()
		{
			_stop = !_coroutineEnumerator.MoveNext() || _stop;
			return !_stop;
		}

		public void Reset()
		{
			_coroutineEnumerator.Reset();
		}
	}

	private class BaseAction
	{
		protected bool _active;

		protected float _activationTime;

		protected AIBossVehicleController _parentController;

		protected int _activationProbability = 50;

		public float ActiveTime
		{
			get
			{
				return (!_active) ? 0f : (Time.time - _activationTime);
			}
		}

		public bool IsActive
		{
			get
			{
				return _active;
			}
		}

		public BaseAction(AIBossVehicleController parentController)
		{
			_parentController = parentController;
		}

		public virtual void Reset()
		{
			_active = false;
		}

		public virtual bool IsFinished()
		{
			return false;
		}

		public void Activate()
		{
			if (!_active)
			{
				_active = true;
				_activationTime = Time.time;
				OnActivate();
			}
		}

		public void Deactivate()
		{
			if (_active)
			{
				_active = false;
				OnDeactivate();
			}
		}

		public bool TryProbability()
		{
			if (_activationProbability >= 100)
			{
				return true;
			}
			if (_activationProbability <= 0)
			{
				return false;
			}
			int num = UnityEngine.Random.Range(1, 100);
			if (num <= _activationProbability)
			{
				return true;
			}
			return false;
		}

		public virtual bool CanBeActivated()
		{
			return true;
		}

		public virtual bool CanBeInterrupted()
		{
			return true;
		}

		protected virtual void OnActivate()
		{
		}

		protected virtual void OnDeactivate()
		{
		}
	}

	private class RandomMovementAction : BaseAction
	{
		private float _radius;

		private int _activationBlockedWayCounter = 5;

		private float _retryPeriod;

		private float _lastTryTime;

		private float _timeout;

		private float _finalPontRadius;

		private MovementParameters.FinalPointActionType _fpAction;

		private bool _finished;

		private Transform _playerTransform;

		public RandomMovementAction(AIBossVehicleController parentController, int blockedWayCounter, float radius, float retryPeriod, float timeout, int activationProbability, float finalPontRadius, MovementParameters.FinalPointActionType fpAction)
			: base(parentController)
		{
			_playerTransform = VehiclesManager.instance.playerVehicle.transform;
			_activationBlockedWayCounter = blockedWayCounter;
			_radius = radius;
			_retryPeriod = retryPeriod;
			_activationProbability = activationProbability;
			_timeout = timeout;
			_finalPontRadius = finalPontRadius;
			_fpAction = fpAction;
		}

		public override bool CanBeActivated()
		{
			if (_activationProbability >= 100 || (_activationBlockedWayCounter > 0 && _parentController._wayBlockedCounter >= _activationBlockedWayCounter))
			{
				return true;
			}
			float num = Time.time - _lastTryTime;
			if (num >= _retryPeriod)
			{
				_lastTryTime = Time.time;
				return TryProbability();
			}
			return false;
		}

		protected override void OnActivate()
		{
			_finished = false;
			int num = UnityEngine.Random.Range(0, 36) * 10;
			Quaternion quaternion = Quaternion.Euler(0f, num, 0f);
			float num2 = _radius / 10f;
			float x = num2 * (float)UnityEngine.Random.Range(1, 11);
			Vector3 vector = new Vector3(x, 0f, 0f);
			vector = _parentController.transform.position + quaternion * vector;
			_parentController._getFinalPointRadiusDelegate = () => _finalPontRadius;
			_parentController._getFinalPointActionDelegate = () => _fpAction;
			_parentController.SetMoveTarget(vector);
			_parentController.SetFireTarget((!_parentController._playersGhostAbilityActive) ? _playerTransform : null);
			_parentController.StartMovement();
		}

		protected override void OnDeactivate()
		{
			_parentController._getFinalPointRadiusDelegate = null;
			_parentController._getFinalPointActionDelegate = null;
			_parentController.StopMovement();
		}

		public override bool CanBeInterrupted()
		{
			return (_timeout > 0f && base.ActiveTime > _timeout) || _parentController.IsFinalPointReached();
		}
	}

	private class ChaseAndFirePlayerAction : BaseAction
	{
		private bool _finished;

		private Transform _playerTransform;

		public ChaseAndFirePlayerAction(AIBossVehicleController parentController)
			: base(parentController)
		{
			_playerTransform = VehiclesManager.instance.playerVehicle.transform;
		}

		public override bool IsFinished()
		{
			return _finished;
		}

		public override bool CanBeActivated()
		{
			return _playerTransform.gameObject.active && !_parentController._playersGhostAbilityActive;
		}

		protected override void OnActivate()
		{
			_finished = false;
			VehiclesManager.instance.playerVehicleDeactivatedEvent += OnPlayerVehicleDeactivated;
			_parentController.SetChaseTarget(_playerTransform);
			_parentController.SetFireTarget(_playerTransform);
			_parentController.StartMovement();
		}

		protected override void OnDeactivate()
		{
			if (VehiclesManager.instance != null)
			{
				VehiclesManager.instance.playerVehicleDeactivatedEvent -= OnPlayerVehicleDeactivated;
			}
			_parentController.StopMovement();
		}

		private void OnPlayerVehicleDeactivated(Vehicle vehicle)
		{
			_finished = true;
		}
	}

	private class PickUpPowerupAction : BaseAction
	{
		public delegate bool CustomActivationDelegate();

		private CustomActivationDelegate _customActivationDelegate;

		private bool _canInterrupt;

		private List<CollectableItem> _powerups = new List<CollectableItem>();

		private float _respawnTime = 5f;

		private float _retryPeriod;

		private float _lastTryTime;

		private float _timeout;

		private CollectableItem _selectedItem;

		public PickUpPowerupAction(AIBossVehicleController parentController, CollectableItemType type, Type buffType, float respawnTime, float retryPeriod, float timeout, int activationProbability)
			: base(parentController)
		{
			_respawnTime = respawnTime;
			_retryPeriod = retryPeriod;
			_activationProbability = activationProbability;
			_timeout = timeout;
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(CollectableItem));
			UnityEngine.Object[] array2 = array;
			foreach (UnityEngine.Object @object in array2)
			{
				CollectableItem collectableItem = @object as CollectableItem;
				if (collectableItem.ItemType == type || (collectableItem.CarryBuff != null && collectableItem.CarryBuff.GetType() == buffType))
				{
					_powerups.Add(collectableItem);
				}
			}
		}

		public PickUpPowerupAction SetCustomActivationCheckDelegate(CustomActivationDelegate deleg)
		{
			_customActivationDelegate = deleg;
			return this;
		}

		public override void Reset()
		{
			base.Reset();
			_canInterrupt = true;
		}

		protected override void OnActivate()
		{
			AIBossVehicleController parentController = _parentController;
			parentController._onFinalPointReached = (Action)Delegate.Combine(parentController._onFinalPointReached, new Action(OnFinalPointReached));
			_parentController._getFinalPointRadiusDelegate = () => 1f;
			_selectedItem = GetBestItem();
			if (_selectedItem != null)
			{
				_selectedItem.OnConsumedEvent += OnItemCollected;
				_parentController.SetMoveTarget(_selectedItem.transform.position);
				_parentController.StartMovement();
				_canInterrupt = false;
			}
		}

		protected override void OnDeactivate()
		{
			if (_selectedItem != null)
			{
				_selectedItem.OnConsumedEvent -= OnItemCollected;
			}
			_selectedItem = null;
			AIBossVehicleController parentController = _parentController;
			parentController._onFinalPointReached = (Action)Delegate.Remove(parentController._onFinalPointReached, new Action(OnFinalPointReached));
			_parentController._getFinalPointRadiusDelegate = null;
			_parentController.StopMovement();
		}

		public override bool CanBeActivated()
		{
			float num = Time.time - _lastTryTime;
			if (num >= _retryPeriod)
			{
				_lastTryTime = Time.time;
				if (TryProbability() && (_customActivationDelegate == null || _customActivationDelegate()))
				{
					return IsPotentialPowerupsPresent();
				}
			}
			return false;
		}

		public override bool CanBeInterrupted()
		{
			return _canInterrupt || (_timeout > 0f && base.ActiveTime > _timeout);
		}

		private bool IsPotentialPowerupsPresent()
		{
			foreach (CollectableItem powerup in _powerups)
			{
				if (powerup.IsActive || powerup.SpawnTime - Time.time <= _respawnTime)
				{
					return true;
				}
			}
			return false;
		}

		private CollectableItem GetBestItem()
		{
			CollectableItem collectableItem = null;
			float num = 9999f;
			foreach (CollectableItem powerup in _powerups)
			{
				float num2 = Mathf.Max(powerup.SpawnTime - Time.time, 0f);
				if (!powerup.IsActive && !(num2 <= _respawnTime))
				{
					continue;
				}
				if (collectableItem == null)
				{
					collectableItem = powerup;
					continue;
				}
				float sqrMagnitude = (powerup.transform.position - _parentController.transform.position).sqrMagnitude;
				float num3 = _parentController._carEngine.GetMaxSpeed() * 2f * _respawnTime;
				sqrMagnitude += num3;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					collectableItem = powerup;
				}
			}
			return collectableItem;
		}

		private void OnFinalPointReached()
		{
			_canInterrupt = true;
		}

		private void OnItemCollected()
		{
			_canInterrupt = true;
		}
	}

	private delegate float PointRadiusDelegate();

	private delegate MovementParameters.FinalPointActionType PointActionDelegate();

	public float BehaviorSwitchCheckPeriod = 1f;

	public BehaviorParameters[] Behaviors = new BehaviorParameters[1];

	public float SpeedDownRadius = 2f;

	public float FinalPointMaxSpeed = 20f;

	private GameObject _pawn;

	protected int _pathNextPoint;

	protected NavMeshPath _movementPath;

	protected Vector3 _prevCheckPosition = Vector3.zero;

	protected int _stuckCount;

	protected bool _isFinalPoint = true;

	protected bool _movementStarted;

	protected bool _finalPointPatternStarted;

	protected Vector3 _moveTarget = Vector3.zero;

	protected Transform _attackTarget;

	protected Transform _chaseTarget;

	protected Vector3 _pathCornerPosition = Vector3.zero;

	private BehaviorParameters _activeBehavior;

	protected int _wayBlockedCounter;

	protected Transform _transform;

	protected ISteeringControl _carSteeringControl;

	protected IBrakes _carBrakes;

	protected MainWeapon _weapon;

	protected GunTurret _gunTurret;

	protected Engine _carEngine;

	protected VehiclePhysics _carPhysics;

	protected ITransmission _carTransmission;

	protected Vehicle _vehicle;

	protected Destructible _destructible;

	protected AIBaseAbilityUseHelper.UsageQualityContext _abilityUseContext;

	protected AIBaseAbilityUseHelper _abilityUseHelper;

	protected BaseActiveAbility _ability;

	protected bool _playersGhostAbilityActive;

	private StopableCoroutine _updateMovePathSC;

	private StopableCoroutine _updateFireSC;

	private StopableCoroutine _updateMovementSC;

	private StopableCoroutine _updateMoveToPointSC;

	private StopableCoroutine _breakTillStopSC;

	private StopableCoroutine _moveReverseDirectionSC;

	private StopableCoroutine _moveToPointSC;

	private StopableCoroutine _finishMovementPatternSC;

	private StopableCoroutine _updateActionsSC;

	private StopableCoroutine _updateAbilitySC;

	private PointRadiusDelegate _getFinalPointRadiusDelegate;

	private PointActionDelegate _getFinalPointActionDelegate;

	private Action _onFinalPointReached;

	protected Vector3 _homePoint = Vector3.zero;

	protected int _sensorsLayersMask = -1;

	protected int _frendlyLayersMask;

	private BaseAction[] _aiActions;

	public Vehicle AIVehicle
	{
		get
		{
			return _vehicle;
		}
	}

	public Vector3 HomePoint
	{
		get
		{
			return _homePoint;
		}
	}

	public bool IsMoveToTarget
	{
		get
		{
			return _movementStarted;
		}
	}

	public bool IsHaveAttackTarget
	{
		get
		{
			return _attackTarget != null;
		}
	}

	public bool IsFire
	{
		get
		{
			return _weapon != null && _weapon.shouldFire;
		}
	}

	public bool IsOnChase
	{
		get
		{
			return _chaseTarget != null;
		}
	}

	private float FinalPointRadius
	{
		get
		{
			if (_getFinalPointRadiusDelegate != null)
			{
				return _getFinalPointRadiusDelegate();
			}
			return _activeBehavior.Movement.FinalPointRadius;
		}
	}

	private MovementParameters.FinalPointActionType FinalPointAction
	{
		get
		{
			if (_getFinalPointActionDelegate != null)
			{
				return _getFinalPointActionDelegate();
			}
			return _activeBehavior.Movement.FinalPointAction;
		}
	}

	private Coroutine RestartStopableCoroutine(ref StopableCoroutine outCoroutine, IEnumerator coroutine)
	{
		StopStopableCoroutine(ref outCoroutine);
		if (!base.gameObject.active)
		{
			return null;
		}
		outCoroutine = new StopableCoroutine(coroutine);
		return StartCoroutine(outCoroutine);
	}

	private StopableCoroutine StartStopableCoroutine(IEnumerator coroutine)
	{
		StopableCoroutine stopableCoroutine = new StopableCoroutine(coroutine);
		StartCoroutine(stopableCoroutine);
		return stopableCoroutine;
	}

	private StopableCoroutine StartStopableCoroutine(IEnumerator coroutine, out Coroutine startedCoroutineYI)
	{
		StopableCoroutine stopableCoroutine = new StopableCoroutine(coroutine);
		startedCoroutineYI = StartCoroutine(stopableCoroutine);
		return stopableCoroutine;
	}

	private void StopStopableCoroutine(ref StopableCoroutine coroutine)
	{
		if (coroutine != null)
		{
			coroutine.Stop();
		}
		coroutine = null;
	}

	private void InitAIActions()
	{
		AIActionsParams actions = _activeBehavior.Actions;
		_aiActions = new BaseAction[4];
		_aiActions[0] = new PickUpPowerupAction(this, CollectableItemType.Health, null, actions.GetHPPowerup.MaxTimeBeforeSpawn, actions.GetHPPowerup.TryPeriod, actions.GetHPPowerup.Timeout, actions.GetHPPowerup.ActivationPercent).SetCustomActivationCheckDelegate(() => _destructible.GetMaxHP() - _destructible.hp >= _activeBehavior.Actions.GetHPPowerup.ActivationHPLoss);
		_aiActions[1] = new PickUpPowerupAction(this, CollectableItemType.DPS, null, actions.GetDDPowerup.MaxTimeBeforeSpawn, actions.GetDDPowerup.TryPeriod, actions.GetDDPowerup.Timeout, actions.GetDDPowerup.ActivationPercent).SetCustomActivationCheckDelegate(() => true);
		_aiActions[2] = new RandomMovementAction(this, actions.RandomMovement.BlockedWayCounter, actions.RandomMovement.Radius, actions.RandomMovement.TryPeriod, actions.RandomMovement.Timeout, actions.RandomMovement.ActivationPercent, actions.RandomMovement.FinalPointRadius, actions.RandomMovement.FinalPointAction);
		_aiActions[3] = new ChaseAndFirePlayerAction(this);
	}

	private void DeactivateActions()
	{
		if (_aiActions == null)
		{
			return;
		}
		BaseAction[] aiActions = _aiActions;
		foreach (BaseAction baseAction in aiActions)
		{
			if (baseAction.IsActive)
			{
				baseAction.Deactivate();
			}
		}
	}

	private IEnumerator UpdateActions()
	{
		yield return null;
		BaseAction[] aiActions = _aiActions;
		foreach (BaseAction act in aiActions)
		{
			act.Reset();
		}
		BaseAction _activeAction = null;
		YieldInstruction periodYI = new WaitForSeconds(_activeBehavior.Actions.UpdatePeriod);
		while (true)
		{
			if (_activeAction != null && _activeAction.IsFinished())
			{
				_activeAction.Deactivate();
				_activeAction = null;
			}
			if (_activeAction == null || _activeAction.CanBeInterrupted())
			{
				for (int i = 0; i < _aiActions.Length; i++)
				{
					BaseAction act2 = _aiActions[i];
					if (act2.CanBeActivated() && (_activeAction != act2 || !(act2 is ChaseAndFirePlayerAction)))
					{
						if (_activeAction != null)
						{
							_activeAction.Deactivate();
						}
						_activeAction = null;
						if (_activeBehavior.Actions.AttackPlayer || !(act2 is ChaseAndFirePlayerAction))
						{
							_activeAction = act2;
							_activeAction.Activate();
						}
						break;
					}
				}
			}
			yield return periodYI;
		}
	}

	private IEnumerator UpdateBehaviors()
	{
		yield return null;
		YieldInstruction periodYI = new WaitForSeconds(BehaviorSwitchCheckPeriod);
		while (true)
		{
			BehaviorParameters bestBehavior = null;
			int _minActivationHP = 9999;
			BehaviorParameters[] behaviors = Behaviors;
			foreach (BehaviorParameters bp in behaviors)
			{
				if (bp.ActivationRequirement.CheckType == Requirement.Type.HealthLess && _destructible.hp <= (float)bp.ActivationRequirement.Value && bp.ActivationRequirement.Value < _minActivationHP)
				{
					bestBehavior = bp;
					_minActivationHP = bp.ActivationRequirement.Value;
				}
			}
			if (bestBehavior != null)
			{
				SwitchBehavior(bestBehavior, false);
			}
			else
			{
				BehaviorParameters[] behaviors2 = Behaviors;
				foreach (BehaviorParameters bp3 in behaviors2)
				{
					if (bp3.ActivationRequirement.CheckType == Requirement.Type.Random)
					{
						int dice = UnityEngine.Random.Range(1, 100);
						if (dice <= bp3.ActivationRequirement.Value)
						{
							bestBehavior = bp3;
							break;
						}
					}
				}
				if (bestBehavior != null)
				{
					SwitchBehavior(bestBehavior, false);
				}
				else
				{
					BehaviorParameters[] behaviors3 = Behaviors;
					foreach (BehaviorParameters bp2 in behaviors3)
					{
						if (bp2.ActivationRequirement.CheckType == Requirement.Type.Default)
						{
							SwitchBehavior(bp2, false);
							break;
						}
					}
				}
			}
			yield return periodYI;
		}
	}

	private void SwitchBehavior(BehaviorParameters newBehavior, bool force)
	{
		if (newBehavior != null && (force || newBehavior != _activeBehavior))
		{
			_chaseTarget = null;
			StopMovementCoroutines();
			DeactivateActions();
			_activeBehavior = newBehavior;
			InitAIActions();
			RestartStopableCoroutine(ref _updateMovePathSC, UpdateMovePath());
			RestartStopableCoroutine(ref _updateFireSC, UpdateFire());
			RestartStopableCoroutine(ref _updateActionsSC, UpdateActions());
			RestartStopableCoroutine(ref _updateAbilitySC, UpdateAbility());
		}
	}

	private void SubscribeToGhostAbility()
	{
		GhostAbility component = VehiclesManager.instance.playerVehicle.GetComponent<GhostAbility>();
		if ((bool)component)
		{
			component.AbilityActivatedEvent += OnGhostAbilityActivated;
			component.ghostAbilityDeactivatedEvent += OnGhostAbilityDeactivated;
		}
	}

	private void UnSubscribeFromGhostAbility()
	{
		if (!(VehiclesManager.instance == null) && !(VehiclesManager.instance.playerVehicle == null))
		{
			GhostAbility component = VehiclesManager.instance.playerVehicle.GetComponent<GhostAbility>();
			if ((bool)component)
			{
				component.AbilityActivatedEvent -= OnGhostAbilityActivated;
				component.ghostAbilityDeactivatedEvent -= OnGhostAbilityDeactivated;
			}
		}
	}

	private void OnGhostAbilityActivated(BaseActiveAbility ability)
	{
		_playersGhostAbilityActive = true;
		_attackTarget = null;
		SetChaseTarget(null);
		RestartStopableCoroutine(ref _updateActionsSC, UpdateActions());
	}

	private void OnGhostAbilityDeactivated(GhostAbility ability)
	{
		_playersGhostAbilityActive = false;
		RestartStopableCoroutine(ref _updateActionsSC, UpdateActions());
	}

	private void InitController()
	{
		_transform = base.transform;
		_pawn = base.gameObject;
		_vehicle = GetComponent<Vehicle>();
		_homePoint = base.transform.position;
		_destructible = _vehicle.destructible;
		_carSteeringControl = Glu.MonoBehaviour.GetExistingComponentIface<ISteeringControl>(_pawn);
		_gunTurret = _pawn.GetComponentInChildren<GunTurret>();
		_weapon = _gunTurret.GetComponentInChildren<MainWeapon>();
		_gunTurret.weapon = _weapon;
		_carBrakes = Glu.MonoBehaviour.GetComponentIface<IBrakes>(_pawn);
		_carEngine = _pawn.GetComponentInChildren<Engine>();
		_carPhysics = GetComponent<VehiclePhysics>();
		_carTransmission = GetComponentIface<ITransmission>();
		_abilityUseHelper = GetComponent<AIBaseAbilityUseHelper>();
		_ability = GetComponent<BaseActiveAbility>();
		_activeBehavior = Behaviors[0];
		_wayBlockedCounter = 0;
		SubscribeToGhostAbility();
		InitAIActions();
	}

	private void Start()
	{
		_sensorsLayersMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("PlayerTeam0")) | (1 << LayerMask.NameToLayer("PlayerTeam1")) | (1 << LayerMask.NameToLayer("AI")) | (1 << LayerMask.NameToLayer("AITeam0")) | (1 << LayerMask.NameToLayer("AITeam1"));
		_frendlyLayersMask = 1 << base.gameObject.layer;
		InitController();
	}

	private void OnEnable()
	{
		_wayBlockedCounter = 0;
		BehaviorParameters[] behaviors = Behaviors;
		foreach (BehaviorParameters behaviorParameters in behaviors)
		{
			if (behaviorParameters.ActivationRequirement.CheckType == Requirement.Type.Default)
			{
				SwitchBehavior(behaviorParameters, true);
				break;
			}
		}
		StartCoroutine(UpdateBehaviors());
		StartCoroutine(ClearWayBlockedCounter());
		RestartStopableCoroutine(ref _updateMovePathSC, UpdateMovePath());
		RestartStopableCoroutine(ref _updateFireSC, UpdateFire());
		RestartStopableCoroutine(ref _updateActionsSC, UpdateActions());
		RestartStopableCoroutine(ref _updateAbilitySC, UpdateAbility());
	}

	private void OnDisable()
	{
		_chaseTarget = null;
		StopMovementCoroutines();
		DeactivateActions();
	}

	private void OnDestroy()
	{
		UnSubscribeFromGhostAbility();
	}

	public void SetMoveTarget(Vector3 target)
	{
		_chaseTarget = null;
		_moveTarget = target;
		SearchPathToPoint(_moveTarget);
	}

	public void SetChaseTarget(Transform target)
	{
		_chaseTarget = target;
		if (_chaseTarget != null)
		{
			SearchPathToPoint(_chaseTarget.position);
		}
	}

	private IEnumerator UpdateMovePath()
	{
		yield return null;
		YieldInstruction _updateDelay = new WaitForSeconds(_activeBehavior.Movement.PathUpdatePeriod);
		while (true)
		{
			if (_movementStarted || _chaseTarget != null)
			{
				if (_chaseTarget != null)
				{
					if (!_chaseTarget.gameObject.active)
					{
						_chaseTarget = null;
						StopMovement();
					}
					else if ((_transform.position - _chaseTarget.position).magnitude > FinalPointRadius)
					{
						if (SearchPathToPoint(_chaseTarget.position))
						{
							StartMovementCoroutine();
						}
						else if (IsWayBlocked(_activeBehavior.Movement.SensorDistance, _activeBehavior.Movement.SensorChasmForward))
						{
							yield return RestartStopableCoroutine(ref _breakTillStopSC, BreakTillStop());
							yield return StartCoroutine(MoveReverseUntillWayBlocked());
						}
					}
				}
				else if ((_transform.position - _moveTarget).magnitude > FinalPointRadius)
				{
					if (SearchPathToPoint(_moveTarget) && !IsFinalPointReached())
					{
						StartMovementCoroutine();
					}
					else
					{
						StopMovement();
					}
				}
				else
				{
					if (_onFinalPointReached != null)
					{
						_onFinalPointReached();
					}
					StartFinalPointMovementPattern(_moveTarget);
				}
			}
			else
			{
				if (IsWayBlocked(_activeBehavior.Movement.SensorDistance, _activeBehavior.Movement.SensorChasmForward))
				{
					yield return RestartStopableCoroutine(ref _breakTillStopSC, BreakTillStop());
					yield return StartCoroutine(MoveReverseUntillWayBlocked());
				}
				StopMovement();
			}
			yield return _updateDelay;
		}
	}

	private bool SearchPathToPoint(Vector3 destination)
	{
		_movementPath = null;
		_pathNextPoint = 0;
		_isFinalPoint = false;
		float maxDistance = 30f;
		int num = -1;
		NavMeshHit hit = default(NavMeshHit);
		if (NavMesh.SamplePosition(_transform.position, out hit, maxDistance, num))
		{
			NavMeshHit hit2 = default(NavMeshHit);
			if (NavMesh.SamplePosition(destination, out hit2, maxDistance, num))
			{
				_movementPath = new NavMeshPath();
				if (NavMesh.CalculatePath(hit.position, hit2.position, num, _movementPath))
				{
					if (_movementPath.corners.Length > 1)
					{
						_pathNextPoint = 1;
						_pathCornerPosition = _movementPath.corners[_pathNextPoint];
					}
					else
					{
						_pathNextPoint = 0;
						_pathCornerPosition = _movementPath.corners[_pathNextPoint];
					}
					CheckFinalPoint();
					return true;
				}
				_movementPath = null;
			}
		}
		return false;
	}

	private IEnumerator UpdateMovement()
	{
		yield return null;
		while (true)
		{
			float startTime = Time.time;
			if (_carTransmission != null && _carTransmission.gear == 0)
			{
				_carTransmission.gear = 1;
			}
			yield return RestartStopableCoroutine(ref _updateMoveToPointSC, UpdateMoveToPoint());
			SetNextPathPoint();
			float dt = Time.time - startTime;
			if (dt >= _activeBehavior.Movement.PathMoveUpdatePeriod)
			{
				yield return null;
			}
			else
			{
				yield return new WaitForSeconds(_activeBehavior.Movement.PathMoveUpdatePeriod - dt);
			}
		}
	}

	private void SetNextPathPoint()
	{
		if (_movementPath != null && _pathNextPoint < _movementPath.corners.Length)
		{
			_pathCornerPosition = _movementPath.corners[_pathNextPoint];
			_pathNextPoint++;
		}
		CheckFinalPoint();
	}

	private IEnumerator UpdateMoveToPoint()
	{
		if (IsWrongDirection())
		{
			yield return RestartStopableCoroutine(ref _breakTillStopSC, BreakTillStop());
		}
		yield return RestartStopableCoroutine(ref _moveToPointSC, MoveToPoint());
	}

	private bool CheckSensor(Vector3 start, Vector3 dir, float dist, int layerMask, out Vector3 hitNormal, Color debugColor, Color debugColorHit, float debugTime)
	{
		RaycastHit[] array = Physics.RaycastAll(start, dir, dist, layerMask);
		hitNormal = Vector3.zero;
		if (array == null || array.Length == 0 || (array.Length == 1 && array[0].transform == base.transform))
		{
			return false;
		}
		hitNormal = array[array.Length - 1].normal;
		return true;
	}

	private bool CheckSensor(Vector3 start, Vector3 dir, float dist, out Vector3 hitNormal, Color debugColor, Color debugColorHit, float debugTime)
	{
		return CheckSensor(start, dir, dist, _sensorsLayersMask, out hitNormal, debugColor, debugColorHit, debugTime);
	}

	private bool IsWayBlocked(float wallDist, float chasmDist)
	{
		Vector3 position = base.transform.position;
		position.y += _activeBehavior.Movement.SensorsChasmYOffset;
		Vector3 start = position;
		start += chasmDist * base.transform.forward;
		Vector3 hitNormal = Vector3.zero;
		Vector3 hitNormal2 = Vector3.zero;
		bool flag = CheckSensor(position, base.transform.forward, wallDist, out hitNormal, Color.green, Color.red, 0.3f) && _activeBehavior.Movement.SensorMinCos <= 0f - Vector3.Dot(hitNormal, base.transform.forward);
		bool flag2 = _activeBehavior.Movement.UseChasmSensor && !CheckSensor(start, Vector3.down, _activeBehavior.Movement.SensorChasmDeep, out hitNormal2, Color.green, Color.red, 0.3f);
		return flag || flag2;
	}

	private bool UpdateDirectionBySideSensors(ref Vector2 dir, float dist)
	{
		bool result = false;
		Vector3 hitNormal = Vector3.zero;
		Vector3 hitNormal2 = Vector3.zero;
		Vector3 velocity = _carPhysics.velocity;
		Vector3 position = base.transform.position;
		position.y += _activeBehavior.Movement.SensorsYOffset;
		float num = _activeBehavior.Movement.SensorsWidth / (2f * dist);
		Vector3 normalized = (base.transform.forward - num * base.transform.right).normalized;
		Vector3 normalized2 = (base.transform.forward + num * base.transform.right).normalized;
		bool flag = CheckSensor(position, normalized, dist, out hitNormal, Color.yellow, Color.magenta, 0.3f);
		bool flag2 = CheckSensor(position, normalized2, dist, out hitNormal2, Color.yellow, Color.magenta, 0.3f);
		Vector3 vector = dir;
		Vector3 forward = base.transform.forward;
		Vector2 vector2 = new Vector2(forward.x, forward.z);
		float sensorSteerAngle = _activeBehavior.Movement.SensorSteerAngle;
		bool flag3 = Vector3.Dot(velocity, forward) > 0f;
		if (flag && hitNormal.y < 0.3f)
		{
			float num2 = 0f - Vector3.Dot(hitNormal, forward);
			float num3 = sensorSteerAngle * num2;
			dir = Quaternion.Euler(0f, 0f, (!flag3) ? num3 : (0f - num3)) * vector;
			result = true;
		}
		else if (flag2 && hitNormal2.y < 0.3f)
		{
			float num4 = 0f - Vector3.Dot(hitNormal2, forward);
			float num5 = sensorSteerAngle * num4;
			dir = Quaternion.Euler(0f, 0f, (!flag3) ? (0f - num5) : num5) * vector;
			result = true;
		}
		return result;
	}

	private bool CheckStuck()
	{
		if (_carPhysics == null)
		{
			return false;
		}
		float num = 0.25f;
		float stuckDiff = _activeBehavior.Movement.StuckDiff;
		if (_carPhysics.sqrSpeed <= num && (_prevCheckPosition - _transform.position).sqrMagnitude < stuckDiff * stuckDiff)
		{
			_stuckCount++;
			if (_stuckCount > 5)
			{
				_stuckCount = 0;
				return true;
			}
		}
		else
		{
			_prevCheckPosition = _transform.position;
			_stuckCount = 0;
		}
		return false;
	}

	private float GetDirectionDiff()
	{
		float num = Vector3.Dot(_transform.forward, (_pathCornerPosition - _transform.position).normalized);
		return (0f - (num - 1f)) / 2f;
	}

	private bool IsWrongDirection()
	{
		float num = Vector3.Dot(_transform.forward, _pathCornerPosition - _transform.position);
		return num < 0f;
	}

	private IEnumerator BreakTillStop()
	{
		float epsilon2 = 0.25f;
		if (_carPhysics.sqrSpeed > epsilon2)
		{
			PressBreak();
			yield return new WaitForSeconds(0.5f);
		}
		_carBrakes.brakeFactor = 0f;
	}

	private IEnumerator BreakTillLowSpeed(float speed)
	{
		float epsilon2 = speed * speed;
		if (_carPhysics.sqrSpeed > epsilon2)
		{
			PressBreak(0.7f);
			yield return new WaitForSeconds(0.2f);
		}
		if (_carEngine != null)
		{
			_carEngine.throttle = 0.3f;
		}
		_carBrakes.brakeFactor = 0f;
	}

	private IEnumerator MoveReverseDirection(float time)
	{
		Vector3 carDir = _transform.forward;
		_carSteeringControl.direction = new Vector2(carDir.x, carDir.z);
		_carEngine.throttle = 1f;
		_carBrakes.brakeFactor = 0f;
		_carTransmission.gear = 0;
		yield return new WaitForSeconds(time);
		_carTransmission.gear = 1;
	}

	private IEnumerator MoveReverseUntillWayBlocked()
	{
		_wayBlockedCounter++;
		yield return RestartStopableCoroutine(ref _breakTillStopSC, BreakTillStop());
		while (IsWayBlocked(_activeBehavior.Movement.SensorDistance, _activeBehavior.Movement.SensorChasmForward))
		{
			float speed = _carPhysics.velocity.magnitude;
			float t = ((speed == 0f) ? _activeBehavior.Movement.StuckReverseTime : (1f / speed));
			yield return RestartStopableCoroutine(coroutine: MoveReverseDirection(Math.Min(t, _activeBehavior.Movement.StuckReverseTime)), outCoroutine: ref _moveReverseDirectionSC);
		}
	}

	private IEnumerator ClearWayBlockedCounter()
	{
		YieldInstruction periodYI = new WaitForSeconds(15f);
		while (true)
		{
			_wayBlockedCounter = 0;
			yield return periodYI;
		}
	}

	private IEnumerator MoveToPoint()
	{
		float pointRadius = ((!_isFinalPoint) ? _activeBehavior.Movement.PathPointRadius : FinalPointRadius);
		float epsilon2 = pointRadius * pointRadius;
		float speedDownEpsilon2 = (pointRadius + SpeedDownRadius) * (pointRadius + SpeedDownRadius);
		while (true)
		{
			float velMagnitude = base.rigidbody.velocity.magnitude;
			float delay = 0.3f;
			Vector3 diff3d2 = _pathCornerPosition - _transform.position;
			float dist3 = new Vector2(diff3d2.x, diff3d2.z).sqrMagnitude;
			if (velMagnitude > 0f)
			{
				delay = Mathf.Min(2f / velMagnitude, 0.3f);
			}
			if (dist3 > epsilon2)
			{
				if (IsWayBlocked(Mathf.Max(velMagnitude, _activeBehavior.Movement.SensorDistance), Mathf.Max(velMagnitude, _activeBehavior.Movement.SensorChasmForward)))
				{
					yield return RestartStopableCoroutine(ref _breakTillStopSC, BreakTillStop());
					yield return StartCoroutine(MoveReverseUntillWayBlocked());
				}
				if (CheckStuck())
				{
					yield return RestartStopableCoroutine(ref _moveReverseDirectionSC, MoveReverseDirection(_activeBehavior.Movement.StuckReverseTime));
				}
				diff3d2 = _pathCornerPosition - _transform.position;
				Vector2 diff2d = new Vector2(diff3d2.x, diff3d2.z);
				dist3 = diff2d.sqrMagnitude;
				if (!(dist3 > epsilon2))
				{
					break;
				}
				diff2d.Normalize();
				bool steerObstacle = UpdateDirectionBySideSensors(ref diff2d, Mathf.Max(velMagnitude, _activeBehavior.Movement.SensorDistance * 1.3f));
				_carSteeringControl.direction = diff2d;
				_carEngine.throttle = ((!steerObstacle) ? 1f : 0.3f);
				_carBrakes.brakeFactor = 0f;
				if (_carTransmission.gear == 0)
				{
					_carTransmission.gear = 1;
				}
				if (_isFinalPoint && dist3 < speedDownEpsilon2)
				{
					yield return StartCoroutine(BreakTillLowSpeed(FinalPointMaxSpeed));
				}
				yield return new WaitForSeconds(delay);
				continue;
			}
			if (_isFinalPoint)
			{
				if (_onFinalPointReached != null)
				{
					_onFinalPointReached();
				}
				StartFinalPointMovementPattern(_pathCornerPosition);
			}
			else
			{
				PressBreak();
			}
			yield break;
		}
		if (_isFinalPoint)
		{
			if (_onFinalPointReached != null)
			{
				_onFinalPointReached();
			}
			StartFinalPointMovementPattern(_pathCornerPosition);
		}
		else
		{
			PressBreak();
		}
	}

	private void PressBreak()
	{
		PressBreak(1f);
	}

	private void PressBreak(float power)
	{
		if (_carEngine != null)
		{
			_carEngine.throttle = 0f;
		}
		if (_carBrakes != null)
		{
			_carBrakes.brakeFactor = power;
		}
	}

	private bool CheckFinalPoint()
	{
		_isFinalPoint = _movementPath == null || _pathNextPoint >= _movementPath.corners.Length - 1;
		return _isFinalPoint;
	}

	private bool IsFinalPointReached()
	{
		if (!_isFinalPoint)
		{
			return false;
		}
		float finalPointRadius = FinalPointRadius;
		float num = finalPointRadius * finalPointRadius;
		Vector3 vector = _pathCornerPosition - _transform.position;
		vector.y = 0f;
		if (vector.sqrMagnitude <= num)
		{
			return true;
		}
		return false;
	}

	public void StopMovement()
	{
		PressBreak(1f);
		StopMovementCoroutines();
	}

	public void StartMovement()
	{
		StopFinalPointMovementPattern();
		PressBreak(0f);
		StartMovementCoroutine();
	}

	private void StartMovementCoroutine()
	{
		StopFinalPointMovementPattern();
		if (!_movementStarted)
		{
			RestartStopableCoroutine(ref _updateMovementSC, UpdateMovement());
		}
		_movementStarted = true;
	}

	private void StopMovementCoroutines()
	{
		if (_movementStarted || _finalPointPatternStarted)
		{
			_movementStarted = false;
			_finalPointPatternStarted = false;
			StopStopableCoroutine(ref _updateMovementSC);
			StopStopableCoroutine(ref _updateMoveToPointSC);
			StopStopableCoroutine(ref _breakTillStopSC);
			StopStopableCoroutine(ref _moveReverseDirectionSC);
			StopStopableCoroutine(ref _moveToPointSC);
			StopFinalPointMovementPattern();
		}
	}

	private void StopFinalPointMovementPattern()
	{
		_finalPointPatternStarted = false;
		StopStopableCoroutine(ref _finishMovementPatternSC);
	}

	private void StartFinalPointMovementPattern(Vector3 finalPointPos)
	{
		if (!_finalPointPatternStarted)
		{
			StopMovement();
			MovementParameters.FinalPointActionType finalPointAction = FinalPointAction;
			if (finalPointAction != 0 && finalPointAction == MovementParameters.FinalPointActionType.Round)
			{
				_finalPointPatternStarted = true;
				RestartStopableCoroutine(ref _finishMovementPatternSC, MoveAroundPointPattern(finalPointPos));
			}
		}
	}

	private IEnumerator MoveAroundPointPattern(Vector3 point)
	{
		yield return null;
		float coorectionAngle = _activeBehavior.Movement.RoundCorrectionAngle;
		YieldInstruction delayYI = new WaitForSeconds(0.1f);
		Quaternion cwCorectAng = Quaternion.Euler(0f, coorectionAngle, 0f);
		Quaternion ccwCorectAng = Quaternion.Euler(0f, 0f - coorectionAngle, 0f);
		while (true)
		{
			float velMagnitude = base.rigidbody.velocity.magnitude;
			Vector3 dir3d = _transform.forward;
			dir3d.y = 0f;
			dir3d.Normalize();
			Vector3 toVehDir = _transform.position - point;
			toVehDir.y = 0f;
			toVehDir.Normalize();
			Vector3 moveDir3d3 = new Vector3(toVehDir.z, 0f, 0f - toVehDir.x);
			if (Vector3.Dot(dir3d, moveDir3d3) < Vector2.Dot(dir3d, -moveDir3d3))
			{
				moveDir3d3 = -moveDir3d3;
				moveDir3d3 = ccwCorectAng * moveDir3d3;
			}
			else
			{
				moveDir3d3 = cwCorectAng * moveDir3d3;
			}
			Vector2 moveDir = new Vector2(moveDir3d3.x, moveDir3d3.z);
			if (IsWayBlocked(Mathf.Max(velMagnitude, _activeBehavior.Movement.SensorDistance), Mathf.Max(velMagnitude, _activeBehavior.Movement.SensorChasmForward)))
			{
				yield return StartCoroutine(MoveReverseUntillWayBlocked());
			}
			if (CheckStuck())
			{
				yield return RestartStopableCoroutine(ref _moveReverseDirectionSC, MoveReverseDirection(_activeBehavior.Movement.StuckReverseTime / 3f));
			}
			_carSteeringControl.direction = moveDir;
			_carEngine.throttle = _activeBehavior.Movement.RoundThrottle;
			_carBrakes.brakeFactor = 0f;
			if (_carTransmission.gear == 0)
			{
				_carTransmission.gear = 1;
			}
			yield return delayYI;
		}
	}

	public void SetFireTarget(Transform target)
	{
		_attackTarget = target;
		if (_weapon != null)
		{
			_weapon.shouldFire = false;
		}
	}

	private IEnumerator UpdateFire()
	{
		yield return null;
		YieldInstruction periodYI = new WaitForSeconds(_activeBehavior.Fire.FireUpdatePeriod);
		while (true)
		{
			if ((bool)_attackTarget && _attackTarget.gameObject.active)
			{
				Vector3 start = _transform.position;
				start.y += _activeBehavior.Movement.SensorsYOffset;
				Vector3 FireDirection2 = _attackTarget.transform.position + new Vector3(0f, -0.5f, 0f) - start;
				float distance = FireDirection2.magnitude;
				if (distance > 0f && distance <= _activeBehavior.Fire.Radius)
				{
					FireDirection2.Normalize();
					float halfTreshold = (float)_activeBehavior.Fire.AimTresholdAngle / 2f;
					Quaternion rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f - halfTreshold, halfTreshold), 0f);
					FireDirection2 = rot * FireDirection2;
					if (_gunTurret != null)
					{
						_gunTurret.SetTargetDirection(new Vector2(FireDirection2.x, FireDirection2.z));
					}
					_weapon.shouldFire = true;
				}
				else
				{
					_weapon.shouldFire = false;
				}
			}
			else
			{
				_weapon.shouldFire = false;
			}
			yield return periodYI;
		}
	}

	private IEnumerator UpdateAbility()
	{
		yield return null;
		if (_ability == null || _abilityUseHelper == null)
		{
			yield break;
		}
		YieldInstruction periodYI = new WaitForSeconds(_activeBehavior.Skill.UseUpdatePeriod);
		while (true)
		{
			if (_activeBehavior.Skill.Use && (bool)_attackTarget && _attackTarget.gameObject.active && _ability.CanActivateAbility() && _abilityUseHelper.GetAbilityUsageQuality(_abilityUseContext, _attackTarget) >= _activeBehavior.Skill.UseMinQuality)
			{
				_ability.ActivateAbility();
			}
			yield return periodYI;
		}
	}

	private void OnDrawGizmos()
	{
		BehaviorParameters behaviorParameters = Behaviors[0];
		if (_movementPath != null && _movementPath.corners.Length > 1)
		{
			Gizmos.color = Color.blue;
			for (int i = 0; i < _movementPath.corners.Length - 1; i++)
			{
				Vector3 from = _movementPath.corners[i];
				Vector3 to = _movementPath.corners[i + 1];
				Gizmos.DrawLine(from, to);
			}
			Gizmos.DrawLine(_pathCornerPosition, _transform.position);
			if (_isFinalPoint)
			{
				Gizmos.DrawWireSphere(_pathCornerPosition, FinalPointRadius);
			}
			else
			{
				Gizmos.DrawWireSphere(_pathCornerPosition, behaviorParameters.Movement.PathPointRadius);
			}
		}
		if (_chaseTarget != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawLine(_chaseTarget.position, _transform.position);
		}
		Gizmos.color = Color.red;
		if (_attackTarget != null)
		{
			Gizmos.DrawLine(_attackTarget.position, _transform.position);
		}
		Gizmos.DrawWireSphere(base.transform.position, behaviorParameters.Fire.Radius);
		Vector3 position = base.transform.position;
		position.y += behaviorParameters.Movement.SensorsYOffset;
		float num = behaviorParameters.Movement.SensorsWidth / (2f * behaviorParameters.Movement.SensorDistance * 1.3f);
		Gizmos.color = Color.yellow;
		Vector3 normalized = (base.transform.forward - num * base.transform.right).normalized;
		Vector3 normalized2 = (base.transform.forward + num * base.transform.right).normalized;
		float num2 = ((!(_carPhysics != null)) ? behaviorParameters.Movement.SensorDistance : Mathf.Max(_carPhysics.velocity.magnitude, behaviorParameters.Movement.SensorDistance));
		Gizmos.DrawLine(position, position + base.transform.forward * num2);
		Gizmos.DrawLine(position, position + normalized * num2 * 1.3f);
		Gizmos.DrawLine(position, position + normalized2 * num2 * 1.3f);
		float num3 = ((!(_carPhysics != null)) ? behaviorParameters.Movement.SensorChasmForward : Mathf.Max(_carPhysics.velocity.magnitude, behaviorParameters.Movement.SensorChasmForward));
		Vector3 position2 = base.transform.position;
		position2.y += behaviorParameters.Movement.SensorsChasmYOffset;
		position2 += num3 * base.transform.forward;
		Gizmos.DrawLine(position2, position2 + Vector3.down * behaviorParameters.Movement.SensorChasmDeep);
	}
}
