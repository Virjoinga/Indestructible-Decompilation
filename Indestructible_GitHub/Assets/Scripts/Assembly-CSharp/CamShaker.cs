using UnityEngine;

[AddComponentMenu("Indestructible/Camera/Cam Shaker")]
public class CamShaker : MonoBehaviour
{
	public float sharpness = 50f;

	public Vector3 shakeScale = new Vector3(1f, 1f, 0.5f);

	public float shakeRotationScale = 3f;

	public Transform epicenterReference;

	public float quadraticAttenuation = 0.0025f;

	public float motionBlurMagnitudeFactor = 1f;

	private float _phaseProgress;

	private float _progress;

	private float _stepFactor;

	private float _currentMagnitude;

	private float _magnitude;

	private Vector3 _phaseOffset;

	private Vector3 _returnOffset;

	private float _phaseAngle;

	private float _returnAngle;

	private Quaternion _phaseRotation;

	private Transform _cameraTransform;

	private MotionBlur _motionBlur;

	private float _motionBlurFactor;

	private bool _isShaking;

	private bool _motionBlurUsed;

	private static CamShaker _instance;

	public static CamShaker Instance
	{
		get
		{
			return _instance;
		}
	}

	public static void ShakeIfExist(float magnitude, float duration, Vector3 epicenter)
	{
		CamShaker instance = Instance;
		if (instance != null)
		{
			instance.Shake(magnitude, duration, epicenter);
		}
	}

	public static void ShakeIfExist(float magnitude, float duration)
	{
		CamShaker instance = Instance;
		if (instance != null)
		{
			instance.Shake(magnitude, duration);
		}
	}

	public void Shake(float magnitude, float duration, Vector3 epicenter)
	{
		float sqrMagnitude = (epicenter - ((!(epicenterReference != null)) ? _cameraTransform.localPosition : epicenterReference.position)).sqrMagnitude;
		Shake(magnitude / (1f + sqrMagnitude * quadraticAttenuation), duration);
	}

	public void Shake(float magnitude, float duration)
	{
		_magnitude = magnitude;
		if (_motionBlur != null)
		{
			_motionBlurFactor = magnitude * motionBlurMagnitudeFactor;
			if (0.5f < _motionBlurFactor)
			{
				if (1f < _motionBlurFactor)
				{
					_motionBlurFactor = 1f;
				}
				float num = _motionBlur.baseNormalAccumulateFactor * _motionBlurFactor;
				if (_motionBlur.usageCount <= 0 || _motionBlur.normalAccumulateFactor < num)
				{
					_motionBlur.normalAccumulateFactor = _motionBlur.baseNormalAccumulateFactor * _motionBlurFactor;
					_motionBlur.downscaledAccumulateFactor = _motionBlur.baseDownscaledAccumulateFactor * _motionBlurFactor;
				}
				if (!_motionBlurUsed)
				{
					_motionBlurUsed = true;
					_motionBlur.Use();
				}
			}
		}
		if (_isShaking)
		{
			float num2 = _progress / _stepFactor;
			if (duration < num2)
			{
				duration = num2;
			}
			_progress = 1f;
			_stepFactor = 1f / duration;
			_currentMagnitude *= magnitude / _magnitude;
		}
		else
		{
			_progress = 1f;
			_phaseProgress = 1f;
			_stepFactor = 1f / duration;
			_currentMagnitude = magnitude;
			_returnOffset = (_phaseOffset = Vector3.Scale(Random.insideUnitSphere, shakeScale) * _currentMagnitude);
			_returnAngle = (_phaseAngle = Random.Range(0f - shakeRotationScale, shakeRotationScale) * _currentMagnitude);
			_phaseRotation = Quaternion.AngleAxis(_phaseAngle, Vector3.forward);
			base.enabled = true;
			_isShaking = true;
		}
	}

	private void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("Cam shaker instance already present while creating another instance");
			Debug.Break();
		}
		_instance = this;
		_cameraTransform = base.transform;
		_motionBlur = GetComponent<MotionBlur>();
		base.enabled = false;
	}

	private void Update()
	{
		if (!_isShaking)
		{
			return;
		}
		if (_phaseProgress == 0f)
		{
			if (!(0f < _progress))
			{
				_cameraTransform.localPosition -= _returnOffset;
				_cameraTransform.localRotation *= Quaternion.AngleAxis(0f - _returnAngle, Vector3.forward);
				if (_motionBlurUsed)
				{
					_motionBlurUsed = false;
					_motionBlurFactor = 0f;
					_motionBlur.Unuse();
				}
				base.enabled = false;
				_isShaking = false;
				return;
			}
			_phaseProgress = 1f;
			_currentMagnitude = _magnitude * _progress * _progress;
			Vector3 vector = Vector3.Scale(Random.insideUnitSphere, shakeScale) * _currentMagnitude;
			_phaseOffset = vector - _returnOffset;
			_returnOffset = vector;
			float num = Random.Range(0f - shakeRotationScale, shakeRotationScale) * _currentMagnitude;
			_phaseAngle = num - _returnAngle;
			_returnAngle = num;
			_phaseRotation = Quaternion.AngleAxis(_phaseAngle, Vector3.forward);
		}
		float deltaTime = Time.deltaTime;
		float num2 = deltaTime * sharpness;
		if (_phaseProgress < num2)
		{
			num2 = _phaseProgress;
		}
		_cameraTransform.localPosition += _phaseOffset * num2;
		_cameraTransform.localRotation *= Quaternion.Lerp(Quaternion.identity, _phaseRotation, num2);
		if (0.5f < _motionBlurFactor)
		{
			float num3 = _motionBlurFactor * _progress;
			float num4 = _motionBlur.baseNormalAccumulateFactor * num3;
			if (_motionBlur.usageCount <= 1 || _motionBlur.normalAccumulateFactor < num4)
			{
				_motionBlur.normalAccumulateFactor = _motionBlur.baseNormalAccumulateFactor * num3;
				float num5 = _motionBlur.baseDownscaledAccumulateFactor * num3;
				if (num5 < 0.5f)
				{
					num5 = 0.5f;
				}
				_motionBlur.downscaledAccumulateFactor = num5;
			}
		}
		_phaseProgress -= num2;
		_progress -= deltaTime * _stepFactor;
		if (_progress < 0f)
		{
			_progress = 0f;
		}
	}
}
