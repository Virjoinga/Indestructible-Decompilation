using Glu;
using UnityEngine;

[AddComponentMenu("Indestructible/PostEffects/Speed Blur")]
public class SpeedBlur : Glu.MonoBehaviour
{
	public float activationSpeed = 27f;

	public float speedBlurFactor = 0.004f;

	private VehiclePhysics _vehiclePhysics;

	private float _baseSqrSpeed;

	private MotionBlur _motionBlur;

	private bool _isMotionBlurUsed;

	private void Awake()
	{
		_motionBlur = GetComponent<MotionBlur>();
		if (_motionBlur == null)
		{
			base.enabled = false;
			Object.Destroy(this);
		}
	}

	private void Start()
	{
		activationSpeed *= activationSpeed;
		_baseSqrSpeed = activationSpeed - 0.25f / speedBlurFactor;
		base.enabled = false;
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.playerVehicleActivatedEvent += PlayerVehicleActivated;
			instance.playerVehicleDeactivatedEvent += PlayerVehicleDeactivated;
			if (instance.playerVehicle != null && instance.playerVehicle.isActive)
			{
				PlayerVehicleActivated(instance.playerVehicle);
			}
		}
	}

	private void OnDestroy()
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.playerVehicleActivatedEvent -= PlayerVehicleActivated;
			instance.playerVehicleDeactivatedEvent -= PlayerVehicleDeactivated;
		}
	}

	private void Update()
	{
		float sqrSpeed = _vehiclePhysics.sqrSpeed;
		if (activationSpeed < sqrSpeed)
		{
			float num = (sqrSpeed - _baseSqrSpeed) * speedBlurFactor;
			if (1f < num)
			{
				num = 1f;
			}
			if (!_isMotionBlurUsed)
			{
				_isMotionBlurUsed = true;
				_motionBlur.Use();
			}
			float num2 = _motionBlur.baseNormalAccumulateFactor * num;
			if (_motionBlur.usageCount <= 1 || _motionBlur.normalAccumulateFactor < num2)
			{
				_motionBlur.normalAccumulateFactor = num2;
				float num3 = _motionBlur.baseDownscaledAccumulateFactor * num;
				if (num3 < 0.5f)
				{
					num3 = 0.5f;
				}
				_motionBlur.downscaledAccumulateFactor = num3;
			}
		}
		else if (_isMotionBlurUsed)
		{
			_isMotionBlurUsed = false;
			_motionBlur.Unuse();
		}
	}

	private void PlayerVehicleActivated(Vehicle vehicle)
	{
		_vehiclePhysics = vehicle.GetExistingComponentIface<VehiclePhysics>();
		base.enabled = true;
	}

	private void PlayerVehicleDeactivated(Vehicle vehicle)
	{
		base.enabled = false;
		if (_isMotionBlurUsed)
		{
			_isMotionBlurUsed = false;
			_motionBlur.Unuse();
		}
	}
}
