using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Indestructible/Weapons/LaserGun")]
public class LaserGun : ThermalWeapon
{
	public GameObject hitEffectsPrefab;

	public Vector2 beamAnimSpeed = new Vector2(-10f, 0f);

	public Vector2 beamScale = new Vector2(0.1f, 1f);

	private LineRenderer _beamRenderer;

	private Material _beamMaterial;

	private Renderer _muzzleRenderer;

	private Transform _hitEffectsTransform;

	private Renderer _hitRenderer;

	private ParticleSystem[] _hitEmitters;

	private bool _isHitEffectsActive;

	private static Vector3 FarBeamEnd = new Vector3(0f, 0f, 100f);

	//[RPC]
	public override void SetFireInterval(float value)
	{
		base.SetFireInterval(value);
	}

	protected override void Awake()
	{
		base.Awake();
		_beamRenderer = GetComponent<LineRenderer>();
		_beamMaterial = _beamRenderer.material;
		Renderer[] components = GetComponents<Renderer>();
		Renderer[] array = components;
		foreach (Renderer renderer in array)
		{
			if (!(renderer is LineRenderer))
			{
				_muzzleRenderer = renderer;
				break;
			}
		}
		GameObject gameObject = Object.Instantiate(hitEffectsPrefab) as GameObject;
		_hitEffectsTransform = gameObject.transform;
		_hitEffectsTransform.parent = base.transform;
		_hitRenderer = gameObject.GetComponent<Renderer>();
		ParticleSystem particleEmitter = gameObject.GetComponent<ParticleSystem>();
		if ((bool)particleEmitter)
		{
			List<ParticleSystem> list = new List<ParticleSystem>();
			list.Add(particleEmitter);
			foreach (Transform item in _hitEffectsTransform)
			{
				particleEmitter = item.GetComponent<ParticleSystem>();
				if ((bool)particleEmitter)
				{
					list.Add(particleEmitter);
				}
			}
			_hitEmitters = list.ToArray();
		}
		else
		{
			_hitEmitters = new ParticleSystem[0];
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		DeactivateHitEffect();
	}

	protected override void MakeShot()
	{
		Vector3 origin = base.transform.position;
		Vector3 forward = base.gunTurret.forward;
		while (true)
		{
			if (BeamCast(origin, forward, out _hitInfo, FarBeamEnd.z, base.collisionLayers))
			{
				Collider collider = _hitInfo.collider;
				if (!IsFoe(collider))
				{
					origin = _hitInfo.point + forward * 0.25f;
					continue;
				}
				Destructible destructible;
				if (CheckFoeDamageAbilty(collider, out destructible))
				{
					Damage(destructible);
				}
				_hitInfo.distance -= 0.35f;
				_hitEffectsTransform.localPosition = new Vector3(0f, 0f, _hitInfo.distance);
				StartHitEffect();
				break;
			}
			StopHitEffect();
			_hitInfo.distance = FarBeamEnd.z;
			break;
		}
		_beamRenderer.SetPosition(1, new Vector3(0f, 0f, _hitInfo.distance));
		_beamMaterial.mainTextureScale = new Vector2(beamScale.x * _hitInfo.distance, beamScale.y);
		RegShot();
	}

	protected override void StartFire()
	{
		_beamRenderer.enabled = true;
		_muzzleRenderer.enabled = true;
		base.StartFire();
	}

	protected override void StopFire()
	{
		base.StopFire();
		_beamRenderer.enabled = false;
		_muzzleRenderer.enabled = false;
		StopHitEffect();
	}

	private void StartHitEffect()
	{
		if (!_isHitEffectsActive)
		{
			ActivateHitEffect();
		}
	}

	private void ActivateHitEffect()
	{
		_isHitEffectsActive = true;
		if (_hitRenderer != null)
		{
			_hitRenderer.enabled = true;
		}
		ParticleSystem[] hitEmitters = _hitEmitters;
		foreach (ParticleSystem particleEmitter in hitEmitters)
		{
			particleEmitter.Play();
		}
	}

	private void StopHitEffect()
	{
		if (_isHitEffectsActive)
		{
			DeactivateHitEffect();
		}
	}

	private void DeactivateHitEffect()
	{
		_isHitEffectsActive = false;
		if (_hitRenderer != null)
		{
			_hitRenderer.enabled = false;
		}
		ParticleSystem[] hitEmitters = _hitEmitters;
		foreach (ParticleSystem particleEmitter in hitEmitters)
		{
			if (particleEmitter != null)
			{
				particleEmitter.Stop();
			}
		}
	}

	protected virtual bool BeamCast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float distance, int collisionLayers)
	{
		return Physics.Raycast(origin, direction, out hitInfo, distance, collisionLayers);
	}
}
