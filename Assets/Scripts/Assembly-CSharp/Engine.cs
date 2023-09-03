using System.Collections;
using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/Vehicle/Engine")]
public class Engine : Glu.MonoBehaviour, IVehicleActivationObserver
{
	public float torqueIncreaseSpeed = 750f;

	public float maximalTorque = 500f;

	public float baseMaxSpeed = 25f;

	public AudioClip startSound;

	public float startSoundCrossfadeDuration = 1f;

	public AudioSource audioSource;

	public float minSoundPitch = 0.7f;

	public float soundPitchRPMFactor = 0.0001f;

	public float soundPitchLerpFactor = 0.25f;

	private float _throttle;

	private float _torque;

	private float _maxTorque;

	private float _maxSqrSpeed;

	private float _maxSpeed;

	private float _rpm;

	private float _soundPitch;

	private float _soundVolume;

	private VehiclePhysics _vehiclePhysics;

	private bool _isSoundEnabled;

	public float throttle
	{
		get
		{
			return _throttle;
		}
		set
		{
			_throttle = value;
		}
	}

	public float torqueRiseSpeed
	{
		get
		{
			return torqueIncreaseSpeed;
		}
		set
		{
			torqueIncreaseSpeed = value;
		}
	}

	public float torque
	{
		get
		{
			return _torque;
		}
		set
		{
			_torque = value;
		}
	}

	public float rpm
	{
		get
		{
			return _rpm;
		}
	}

	public float GetBaseMaxTorque()
	{
		return maximalTorque;
	}

	public float GetMaxTorque()
	{
		return _maxTorque;
	}

	public void SetMaxTorque(float value)
	{
		_maxTorque = value;
	}

	public float GetBaseMaxSpeed()
	{
		return baseMaxSpeed;
	}

	public void SetMaxSpeed(float value)
	{
		_maxSpeed = value;
		_maxSqrSpeed = value * value;
	}

	public float GetMaxSpeed()
	{
		return _maxSpeed;
	}

	public void UpdateRpm(float wheelsRpm, float gearRatio)
	{
		_rpm = Mathf.Abs(wheelsRpm * gearRatio);
	}

	private void Awake()
	{
		_maxTorque = maximalTorque;
		_maxSqrSpeed = baseMaxSpeed * baseMaxSpeed;
		_vehiclePhysics = GetComponent<VehiclePhysics>();
		if (GetComponent<PlayerVehicle>() == null)
		{
			Object.Destroy(audioSource);
		}
		else if (audioSource != null && audioSource.clip != null)
		{
			_soundVolume = audioSource.volume;
			SettingsController instance = MonoSingleton<SettingsController>.Instance;
			instance.soundSettingsChangedEvent += SoundSettingsChanged;
			_isSoundEnabled = instance.SoundEnabled;
		}
	}

	private void Start()
	{
		GetComponent<Vehicle>().AddActivationObserver(this);
		GetComponent<VehicleFX>().SubscriveToAvatarVisibilityChange(AvatarBecameVisible, AvatarBecameInvisible);
	}

	public void VehicleActivated(Vehicle vehicle)
	{
		if (_isSoundEnabled)
		{
			_soundPitch = minSoundPitch;
			if (startSound != null && 0f < startSoundCrossfadeDuration)
			{
				StartCoroutine(StartSound());
				return;
			}
			audioSource.Play();
			audioSource.pitch = _soundPitch;
		}
	}

	private void OnDestroy()
	{
		VehicleFX component = GetComponent<VehicleFX>();
		if (component != null)
		{
			component.UnsubscriveFromAvatarVisibilityChange(AvatarBecameVisible, AvatarBecameInvisible);
		}
	}

	private IEnumerator StartSound()
	{
		AudioSource.PlayClipAtPoint(startSound, Vector3.zero);
		yield return new WaitForSeconds(startSound.length - startSoundCrossfadeDuration);
		audioSource.Play();
		float t = Time.deltaTime;
		float volTimeFactor = _soundVolume / startSoundCrossfadeDuration;
		for (; t < startSoundCrossfadeDuration; t += Time.deltaTime)
		{
			audioSource.volume = t * volTimeFactor;
			yield return null;
		}
		audioSource.volume = _soundVolume;
	}

	private void FixedUpdate()
	{
		float num = _maxTorque * _throttle * _throttle;
		float num2 = _vehiclePhysics.sqrSpeed - _maxSqrSpeed;
		if (0f < num2)
		{
			if (5f < num2)
			{
				num2 = 5f;
			}
			num -= num2 * num * 0.19f;
		}
		float num3 = _torque + torqueIncreaseSpeed * _throttle * Time.deltaTime;
		if (num < num3)
		{
			_torque = num;
		}
		else
		{
			_torque = num3;
		}
		if (_isSoundEnabled)
		{
			_soundPitch = Mathf.Lerp(_soundPitch, minSoundPitch + _rpm * soundPitchRPMFactor, soundPitchLerpFactor);
			audioSource.pitch = _soundPitch;
		}
	}

	private void SoundSettingsChanged(SettingsController settingsController, bool state)
	{
		if (state)
		{
			if (audioSource != null && audioSource.clip != null)
			{
				_isSoundEnabled = true;
				audioSource.Play();
				return;
			}
		}
		else if (audioSource != null)
		{
			audioSource.Stop();
		}
		_isSoundEnabled = false;
	}

	private void AvatarBecameInvisible()
	{
		base.enabled = false;
	}

	private void AvatarBecameVisible()
	{
		base.enabled = true;
	}
}
