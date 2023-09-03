using System.Collections.Generic;
using UnityEngine;

public class AbilitiesFXDispatcher : WheeledVehicleFX
{
	public enum WeaponEffect
	{
		Rage = 0,
		ROFBoost = 1,
		DamageBoost = 2
	}

	public Color32[] weaponEffectColors = new Color32[3]
	{
		new Color32(byte.MaxValue, 100, 0, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue),
		new Color32(byte.MaxValue, 0, 0, byte.MaxValue)
	};

	private ParticleSystem _weaponEffectsParticleSystem;

	private int _weaponEffectsMask;

	private List<GameObject> _customFX = new List<GameObject>();

	private PhotonView _photonView;

	private Dictionary<string, CachedObject.Cache> _namedFXCache;

	private void Start()
	{
		GetComponent<Vehicle>().SubscribeToMountedEvent(InitWeaponEffects);
		if (PhotonNetwork.room != null)
		{
			_photonView = base.gameObject.GetComponent<PhotonView>();
		}
	}

	public void ActivateWeaponEffect(WeaponEffect effect, bool shouldRemoteActivate)
	{
		ActivateWeaponEffect((int)effect);
		if (shouldRemoteActivate && _photonView != null)
		{
			_photonView.RPC("ActivateWeaponEffect", PhotonTargets.Others, (int)effect);
		}
	}

	[RPC]
	private void ActivateWeaponEffect(int index)
	{
		Debug.Log("ActivateWeaponEffect:" + index);
		if (!(_weaponEffectsParticleSystem != null))
		{
			return;
		}
		int num = 1 << index;
		if ((_weaponEffectsMask & num) == 0)
		{
			_weaponEffectsMask |= num;
			if (base.isVisible)
			{
				_weaponEffectsParticleSystem.startColor = weaponEffectColors[index];
				_weaponEffectsParticleSystem.Play(true);
			}
		}
	}

	public void DeactivateWeaponEffect(WeaponEffect effect, bool shouldRemoteDeactivate)
	{
		DeactivateWeaponEffect((int)effect);
		if (shouldRemoteDeactivate && _photonView != null)
		{
			_photonView.RPC("DeactivateWeaponEffect", PhotonTargets.Others, (int)effect);
		}
	}

	[RPC]
	private void DeactivateWeaponEffect(int index)
	{
		int num = 1 << index;
		if ((_weaponEffectsMask & num) != 0)
		{
			_weaponEffectsMask &= ~num;
			if (base.isVisible)
			{
				CheckWeaponEffect();
			}
		}
	}

	private void CheckWeaponEffect()
	{
		if (!(_weaponEffectsParticleSystem != null))
		{
			return;
		}
		if (_weaponEffectsMask != 0)
		{
			int num = 0;
			int num2 = 1;
			while ((_weaponEffectsMask & num2) == 0)
			{
				num++;
				num2 = 1 << num;
			}
			_weaponEffectsParticleSystem.startColor = weaponEffectColors[num];
			_weaponEffectsParticleSystem.Play(true);
		}
		else
		{
			_weaponEffectsParticleSystem.Stop(true);
		}
	}

	private void InitWeaponEffects(Vehicle vehicle)
	{
		Transform transform = vehicle.weapon.transform.Find("WeaponFX");
		if (transform != null)
		{
			_weaponEffectsParticleSystem = transform.GetComponent<ParticleSystem>();
		}
	}

	public void ActivateBurnEffect(bool shouldRemoteActivate)
	{
		ActivateBurnEffect();
		if (shouldRemoteActivate && _photonView != null)
		{
			_photonView.RPC("ActivateBurnEffect", PhotonTargets.Others);
		}
	}

	[RPC]
	private void ActivateBurnEffect()
	{
		base.burningFactor = 1f;
		base.smokingFactor = 1f;
	}

	public void DeactivateBurnEffect(bool shouldRemoteDeactivate)
	{
		DeactivateBurnEffect();
		if (shouldRemoteDeactivate && _photonView != null)
		{
			_photonView.RPC("DeactivateBurnEffect", PhotonTargets.Others);
		}
	}

	[RPC]
	private void DeactivateBurnEffect()
	{
		base.burningFactor = 0f;
		base.smokingFactor = 0f;
	}

	public void ActivateBoostEffect(bool shouldRemoteActivate)
	{
		ActivateBoostEffect();
		if (shouldRemoteActivate && _photonView != null)
		{
			_photonView.RPC("ActivateBoostEffect", PhotonTargets.Others);
		}
	}

	[RPC]
	private void ActivateBoostEffect()
	{
		base.exhaustFactor = 0f;
		base.boostFactor = 1f;
	}

	public void DeactivateBoostEffect(bool shouldRemoteDeactivate)
	{
		DeactivateBoostEffect();
		if (shouldRemoteDeactivate && _photonView != null)
		{
			_photonView.RPC("DeactivateBoostEffect", PhotonTargets.Others);
		}
	}

	[RPC]
	private void DeactivateBoostEffect()
	{
		base.exhaustFactor = 1f;
		base.boostFactor = 0f;
	}

	public void AddCustomFX(GameObject fxObj)
	{
		if (fxObj != null)
		{
			GameObject gameObject = _customFX.Find((GameObject obj) => obj.name == fxObj.name);
			if (gameObject == null)
			{
				_customFX.Add(fxObj);
			}
		}
	}

	protected override void OnBecameVisible()
	{
		base.OnBecameVisible();
		CheckWeaponEffect();
	}

	protected override void OnBecameInvisible()
	{
		base.OnBecameInvisible();
		if (_weaponEffectsMask != 0 && _weaponEffectsParticleSystem != null)
		{
			_weaponEffectsParticleSystem.Stop(true);
		}
	}

	public void PlayCustomFX(GameObject fxGO)
	{
		if (!(fxGO == null))
		{
			AddCustomFX(fxGO);
			PlayCustomFX(fxGO.name);
		}
	}

	public void PlayCustomFX(string name)
	{
		OnNamedFX(name);
		if ((bool)_photonView)
		{
			_photonView.RPC("OnNamedFX", PhotonTargets.Others, name);
		}
	}

	[RPC]
	private void OnNamedFX(string name)
	{
		if (!base.isVisible)
		{
			return;
		}
		if (_namedFXCache == null)
		{
			_namedFXCache = new Dictionary<string, CachedObject.Cache>();
		}
		CachedObject.Cache value = null;
		if (!_namedFXCache.TryGetValue(name, out value))
		{
			GameObject gameObject = _customFX.Find((GameObject obj) => obj.name == name);
			if (gameObject == null)
			{
				return;
			}
			value = ObjectCacheManager.Instance.GetCache(gameObject);
			_namedFXCache.Add(name, value);
		}
		CachedObject cachedObject = value.Activate();
		cachedObject.transform.parent = base.gameObject.transform;
		cachedObject.transform.localPosition = Vector3.zero;
	}
}
