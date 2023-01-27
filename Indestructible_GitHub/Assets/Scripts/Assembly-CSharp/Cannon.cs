using System.Collections;
using UnityEngine;

public abstract class Cannon : MainWeapon
{
	protected const float RaycastStep = 0.1f;

	public float projectileSpeed = 50f;

	public float recoilForce;

	public Transform muzzleTransform;

	public MuzzleEffect muzzleEffect;

	public ParticleSystem shellEmitter;

	public GameObject[] hitEffects;

	public Transform[] barrels;

	public AnimationClip[] barrelAnimations;

	public ImpactWeaponImp impactImplementation = new ImpactWeaponImp();

	private float _nextShotTime;

	private int _barrelCount;

	private int _currentBarrelIndex;

	private Vector3[] _muzzleLocalPositions;

	private UpdateAgent _updateAgent;

	private Animation _animation;

	private Rigidbody _rigidbody;

	private CachedObject.Cache[] _hitEffectsCache;

	private RendererAgent _rendererAgent;

	private bool _isUpdating;

	private bool _hasMuzzleEffect;

	private bool _hasRigidbody;

	private bool _shouldEmitShells;

	public int barrelCount
	{
		get
		{
			return _barrelCount;
		}
	}

	public int currentBarrelIndex
	{
		get
		{
			return _currentBarrelIndex;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (barrelAnimations != null && barrelAnimations.Length != 0)
		{
			_animation = GetComponentInChildren<Animation>();
		}
		if (muzzleEffect != null)
		{
			muzzleEffect.Use();
			_hasMuzzleEffect = true;
		}
	}

	protected override void Start()
	{
		base.Start();
		_updateAgent = MonoSingleton<UpdateAgent>.Instance;
		impactImplementation.Init();
		_barrelCount = ((barrels != null) ? barrels.Length : 0);
		if (_barrelCount != 0)
		{
			if (1 < _barrelCount)
			{
				Transform parent = muzzleTransform.parent;
				_muzzleLocalPositions = new Vector3[_barrelCount];
				int num = 0;
				do
				{
					Transform transform = barrels[num];
					Vector3 position = transform.position;
					_muzzleLocalPositions[num] = parent.InverseTransformPoint(position);
					Object.Destroy(transform.gameObject);
				}
				while (++num != _barrelCount);
				muzzleTransform.localPosition = _muzzleLocalPositions[0];
			}
			else
			{
				Object.Destroy(barrels[0].gameObject);
			}
		}
		barrels = null;
		if ((bool)_animation)
		{
			int num2 = 0;
			foreach (AnimationState item in _animation)
			{
				item.layer = num2++;
			}
		}
		_rigidbody = base.mainOwnerCollider.attachedRigidbody;
		_hasRigidbody = _rigidbody != null;
		if (recoilForce != 0f)
		{
			if (_hasRigidbody)
			{
				recoilForce = 0f - recoilForce;
			}
			else
			{
				recoilForce = 0f;
			}
		}
		ObjectCacheManager instance = ObjectCacheManager.Instance;
		int num3 = hitEffects.Length;
		_hitEffectsCache = new CachedObject.Cache[num3];
		for (int i = 0; i != num3; i++)
		{
			_hitEffectsCache[i] = instance.GetCache(hitEffects[i]);
		}
		base.gunTurret.projectileSpeed = projectileSpeed;
		_shouldEmitShells = shellEmitter != null;
		_rendererAgent = base.transform.root.GetComponentInChildren<RendererAgent>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (_isUpdating)
		{
			_updateAgent.StopUpdate(this);
		}
	}

	protected override IEnumerator FireLoop()
	{
		do
		{
			float time = Time.time;
			if (_nextShotTime <= time && TryConsumeShotEnergy())
			{
				base.gunTurret.SetDirection();
				MakeShot();
				_nextShotTime = time + GetFireInterval();
				while (true)
				{
					yield return null;
					float dt = Time.deltaTime;
					time += dt;
					if (!shouldFire || _nextShotTime <= time)
					{
						break;
					}
					base.gunTurret.LerpDirection(dt);
				}
			}
			else
			{
				base.gunTurret.LerpDirection(Time.deltaTime);
				yield return null;
			}
		}
		while (shouldFire);
		FireLoopEnd();
	}

	protected override IEnumerator AvatarFireLoop()
	{
		do
		{
			base.gunTurret.SetAvatarDirection();
			ConsumeShotEnergy();
			MakeShot();
			yield return base.fireIntervalInstruction;
		}
		while (base.shouldAvatarFire);
		FireLoopEnd();
	}

	protected virtual void MakeShot()
	{
		RegShot();
		StartUpdate();
		if (1 < _barrelCount)
		{
			if (++_currentBarrelIndex == _barrelCount)
			{
				_currentBarrelIndex = 0;
			}
			muzzleTransform.localPosition = _muzzleLocalPositions[_currentBarrelIndex];
		}
		if (_hasMuzzleEffect)
		{
			if (_hasRigidbody)
			{
				muzzleEffect.Play(muzzleTransform.position, base.lastShotDirection, _rigidbody.velocity);
			}
			else
			{
				muzzleEffect.Play(muzzleTransform.position, base.lastShotDirection);
			}
		}
		if (_shouldEmitShells && _rendererAgent.isVisible)
		{
			shellEmitter.Emit(1);
		}
		base.audioHelper.PlayIfEnabled();
		if (_animation != null && barrelAnimations != null)
		{
			_animation.Play(barrelAnimations[_currentBarrelIndex].name);
		}
		if (recoilForce != 0f)
		{
			_rigidbody.AddForceAtPosition(base.lastShotDirection * recoilForce, muzzleTransform.position, ForceMode.Impulse);
		}
	}

	protected void StartUpdate()
	{
		if (!_isUpdating)
		{
			_isUpdating = true;
			_updateAgent.StartUpdate(this);
		}
	}

	protected void UpdateStopped()
	{
		_isUpdating = false;
	}

	protected CachedObject.Cache GetHitEffectCache(int surfaceType)
	{
		return _hitEffectsCache[(surfaceType < _hitEffectsCache.Length) ? surfaceType : 0];
	}

	protected void Damage(Destructible destructible, Vector3 position, Vector3 velocity)
	{
		Damage(destructible);
		impactImplementation.AddHitImpulse(destructible, position, velocity);
	}
}
