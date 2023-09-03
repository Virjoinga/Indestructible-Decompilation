using System;
using UnityEngine;

[AddComponentMenu("Indestructible/Effects/VisualFX")]
public class VisualFX : RendererAgent
{
	[Serializable]
	public class EffectOptions
	{
		public GameObject effectPrefab;

		public float emissionSpeed;

		public float emissionScale = 1f;

		public float velocityScale;

		public float sizeScale = 1f;

		public float lifetimeScale = 1f;

		public float initEmissionFactor;

		public Transform[] emissionPoints;

		public bool shouldDestroyEmissionPoints = true;
	}

	protected struct Effect
	{
		public float emissionFactor;

		public float emissionAccCount;

		public float velocityScale;

		public float sizeScale;

		public float lifetimeScale;

		public ParticleEffect effect;

		public EmissionPoint[] emissionPoints;

		public float emissionScale;

		public void Init(EffectOptions effectOptions, Transform rootTransform)
		{
			EffectManager instance = EffectManager.instance;
			int num = effectOptions.emissionPoints.Length;
			if (!(effectOptions.effectPrefab != null) || num == 0)
			{
				return;
			}
			velocityScale = effectOptions.velocityScale;
			sizeScale = effectOptions.sizeScale;
			lifetimeScale = effectOptions.lifetimeScale;
			emissionScale = effectOptions.emissionScale;
			emissionPoints = new EmissionPoint[num];
			emissionFactor = effectOptions.initEmissionFactor * emissionScale;
			effect = instance.GetParticleEffect(effectOptions.effectPrefab);
			for (int i = 0; i != num; i++)
			{
				Transform transform = effectOptions.emissionPoints[i];
				emissionPoints[i].localPosition = rootTransform.InverseTransformPoint(transform.position);
				emissionPoints[i].localVelocity = rootTransform.InverseTransformDirection(transform.forward) * effectOptions.emissionSpeed;
				if (effectOptions.shouldDestroyEmissionPoints)
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
		}

		public void Update(Transform rootTransform, Vector3 rootVelocity)
		{
			float deltaTime = Time.deltaTime;
			float num = (emissionAccCount += emissionFactor * effect.rndEmissionRate * deltaTime);
			if (1f <= num)
			{
				int num2 = (int)num;
				emissionAccCount = num - (float)num2;
				Quaternion localRotation = rootTransform.localRotation;
				Vector3 localPosition = rootTransform.localPosition;
				rootVelocity *= velocityScale;
				int i = 0;
				for (int num3 = emissionPoints.Length; i != num3; i++)
				{
					Vector3 vector = rootVelocity + localRotation * emissionPoints[i].localVelocity;
					effect.EmitRandomSizeLifetimeRotation(num2, localPosition + localRotation * emissionPoints[i].localPosition - vector * deltaTime, vector, sizeScale, lifetimeScale);
				}
			}
		}
	}

	protected struct EmissionPoint
	{
		public Vector3 localPosition;

		public Vector3 localVelocity;
	}

	public EffectOptions smokeOptions;

	public EffectOptions burnOptions;

	private Effect _smokeEffect;

	private Effect _burnEffect;

	private Transform _rootTransform;

	private Rigidbody _rigidbody;

	private Vector3 _rootVelocity;

	private bool _hasRigidbody;

	public float smokingFactor
	{
		get
		{
			return _smokeEffect.emissionFactor;
		}
		set
		{
			_smokeEffect.emissionFactor = value * _smokeEffect.emissionScale;
			base.enabled = shouldUpdate;
		}
	}

	public float burningFactor
	{
		get
		{
			return _burnEffect.emissionFactor;
		}
		set
		{
			_burnEffect.emissionFactor = value * _burnEffect.emissionScale;
			base.enabled = shouldUpdate;
		}
	}

	protected Transform rootTransform
	{
		get
		{
			return _rootTransform;
		}
	}

	protected new Rigidbody rigidbody
	{
		get
		{
			return _rigidbody;
		}
	}

	protected virtual bool shouldUpdate
	{
		get
		{
			return base.isVisible && (0f < _smokeEffect.emissionFactor || 0f < _burnEffect.emissionFactor);
		}
	}

	protected virtual void Awake()
	{
		_rootTransform = base.transform.root;
		_rigidbody = _rootTransform.GetComponentInChildren<Rigidbody>();
		_hasRigidbody = _rigidbody != null;
		InitEffectsOnAwake();
	}

	protected virtual void InitEffectsOnAwake()
	{
		InitEffects();
	}

	protected virtual void InitEffects()
	{
		_smokeEffect.Init(smokeOptions, _rootTransform);
		smokeOptions = null;
		_burnEffect.Init(burnOptions, _rootTransform);
		burnOptions = null;
		base.enabled = shouldUpdate;
	}

	protected override void OnBecameVisible()
	{
		base.OnBecameVisible();
		base.enabled = shouldUpdate;
	}

	protected override void OnBecameInvisible()
	{
		base.OnBecameInvisible();
		base.enabled = shouldUpdate;
	}

	protected virtual void Update()
	{
		UpdateEffects((!_hasRigidbody) ? Vector3.zero : _rigidbody.velocity);
	}

	protected virtual void UpdateEffects(Vector3 rootVelocity)
	{
		if (0f < _smokeEffect.emissionFactor)
		{
			_smokeEffect.Update(_rootTransform, rootVelocity);
		}
		if (0f < _burnEffect.emissionFactor)
		{
			_burnEffect.Update(_rootTransform, rootVelocity);
		}
	}
}
