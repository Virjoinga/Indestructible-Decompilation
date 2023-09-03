using UnityEngine;

[AddComponentMenu("Indestructible/Shadows/Shadow Controller")]
public class ShadowController : MonoBehaviour
{
	public Vector2 shadowMapSize = new Vector2(512f, 512f);

	public float aspectCorrection = 0.75f;

	public Shader shader;

	public string[] qualityTags;

	public float groundHeight;

	public Vector4 offsets = new Vector4(0.1f, -0.125f, 0f, 0f);

	public Vector4 colors = new Vector4(0.78f, 0.6f, 1f, 1f);

	public ShadowCameraUpdateType cameraUpdateType = ShadowCameraUpdateType.MoveAndRotate;

	private Transform _viewCameraTransform;

	private Camera _viewCamera;

	private Transform _shadowCameraTransform;

	private Camera _shadowCamera;

	private Transform _groundLevelTransform;

	private Matrix4x4 _shadowMapScaledProjectionMatrix;

	private RenderTexture _targetTexture;

	private string _currentQualityTag;

	public Transform groundLevelTransform
	{
		get
		{
			return _groundLevelTransform;
		}
		set
		{
			_groundLevelTransform = value;
		}
	}

	private void Start()
	{
		_targetTexture = MakeTargetTexture();
		ClearShadowMap(_targetTexture);
		Shader.SetGlobalVector("_ShadowmapOffsets", offsets);
		Shader.SetGlobalColor("_ShadowmapColors", colors);
		Shader.SetGlobalTexture("_ShadowMap", _targetTexture);
		_viewCamera = Camera.main;
		_viewCameraTransform = _viewCamera.transform;
		_shadowCameraTransform = base.transform;
		_shadowCamera = base.GetComponent<Camera>();
		_shadowCamera.targetTexture = _targetTexture;
		float num = 1f / shadowMapSize.x;
		float num2 = 1f / shadowMapSize.y;
		Debug.Log("SMS1: " + num + "," + num2);
		if (SystemInfo.graphicsDeviceName.IndexOf("Adreno") != -1 && SystemInfo.processorCount == 1)
		{
			num = 0f;
			num2 = 0f;
			Debug.Log("SMS2: " + num + "," + num2);
		}
		Vector2 normalizedTexelSize = new Vector2(num, num2);
		Vector2 normalizedShadowMapSize = new Vector2(1f - 2f * normalizedTexelSize.x, 1f - 2f * normalizedTexelSize.y);
		_shadowCamera.rect = new Rect(normalizedTexelSize.x, normalizedTexelSize.y, normalizedShadowMapSize.x, normalizedShadowMapSize.y);
		_shadowCamera.aspect = aspectCorrection * shadowMapSize.x / shadowMapSize.y;
		CalculateScaleMatrix(normalizedShadowMapSize, normalizedTexelSize);
		UpdateShadowCameraOrientation();
		UpdateShadowCameraPosition();
		Shader.SetGlobalMatrix("_ShadowMapMatrix", _shadowMapScaledProjectionMatrix * _shadowCamera.worldToCameraMatrix);
		QualityManager instance = QualityManager.instance;
		instance.qualityLevelChangedEvent += QualityLevelChanged;
		QualityLevelChanged(0, instance.qualityLevel);
		SetupPlayerVehicleFolowing();
	}

	private void SetupPlayerVehicleFolowing()
	{
		VehiclesManager instance = VehiclesManager.instance;
		instance.playerVehicleActivatedEvent += PlayerVehicleActivated;
		if (instance.playerVehicle != null)
		{
			PlayerVehicleActivated(instance.playerVehicle);
		}
	}

	private void PlayerVehicleActivated(Vehicle vehicle)
	{
		groundLevelTransform = vehicle.transform;
		VehiclesManager.instance.playerVehicleActivatedEvent -= PlayerVehicleActivated;
	}

	private void OnDestroy()
	{
		if (QualityManager.isExists)
		{
			QualityManager.instance.qualityLevelChangedEvent -= QualityLevelChanged;
		}
	}

	private void Update()
	{
		if (_groundLevelTransform != null)
		{
			groundHeight = _groundLevelTransform.localPosition.y;
		}
		if ((cameraUpdateType & ShadowCameraUpdateType.Rotate) != 0)
		{
			UpdateShadowCameraOrientation();
		}
		if ((cameraUpdateType & ShadowCameraUpdateType.Move) != 0)
		{
			UpdateShadowCameraPosition();
		}
		Shader.SetGlobalMatrix("_ShadowMapMatrix", _shadowMapScaledProjectionMatrix * _shadowCamera.worldToCameraMatrix);
	}

	private void UpdateShadowCameraOrientation()
	{
		Vector3 forward = _viewCameraTransform.forward;
		forward.y = 0f;
		Vector3 toDirection = _shadowCameraTransform.InverseTransformDirection(forward);
		toDirection.z = 0f;
		_shadowCameraTransform.localRotation *= Quaternion.FromToRotation(Vector3.up, toDirection);
	}

	private void UpdateShadowCameraPosition()
	{
		Vector2 vector = GetGroundIntersectionPoint(_viewCamera, 0f) - GetGroundIntersectionPoint(_shadowCamera, 0.05f);
		Vector3 position = _shadowCameraTransform.position;
		position.x += vector.x;
		position.z += vector.y;
		_shadowCameraTransform.position = position;
	}

	private Vector2 GetGroundIntersectionPoint(Camera camera, float bottom)
	{
		Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, bottom));
		float num = (groundHeight - ray.origin.y) / ray.direction.y;
		return new Vector2(ray.origin.x + ray.direction.x * num, ray.origin.z + ray.direction.z * num);
	}

	private RenderTexture MakeTargetTexture()
	{
		RenderTexture renderTexture = new RenderTexture((int)shadowMapSize.x, (int)shadowMapSize.y, 16, RenderTextureFormat.RGB565);
		renderTexture.filterMode = FilterMode.Bilinear;
		renderTexture.hideFlags = HideFlags.HideAndDontSave;
		return renderTexture;
	}

	private void CalculateScaleMatrix(Vector2 normalizedShadowMapSize, Vector2 normalizedTexelSize)
	{
		Vector2 vector = normalizedShadowMapSize * 0.5f;
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x[0, 0] = vector.x;
		matrix4x[1, 1] = vector.y;
		matrix4x[2, 2] = 0.5f;
		matrix4x.SetColumn(3, new Vector4(vector.x + normalizedTexelSize.x, vector.y + normalizedTexelSize.y, 0.5f, 1f));
		_shadowMapScaledProjectionMatrix = matrix4x * _shadowCamera.projectionMatrix;
	}

	private void ClearShadowMap(RenderTexture texture)
	{
		RenderTexture.active = texture;
		GL.Viewport(new Rect(0f, 0f, shadowMapSize.x, shadowMapSize.y));
		GL.Clear(true, true, new Color(1f, 1f, 1f, 0f));
		RenderTexture.active = null;
	}

	private void QualityLevelChanged(int oldLevel, int newLevel)
	{
		int num = qualityTags.Length;
		if (num <= newLevel)
		{
			if (num == 0)
			{
				Disable();
				return;
			}
			newLevel = num - 1;
		}
		string text = qualityTags[newLevel];
		if (!(text != _currentQualityTag))
		{
			return;
		}
		if (string.IsNullOrEmpty(text))
		{
			Disable();
			return;
		}
		_shadowCamera.SetReplacementShader(shader, text);
		if (_currentQualityTag == null)
		{
			_shadowCamera.enabled = true;
			if (cameraUpdateType != 0)
			{
				base.enabled = true;
			}
		}
		_currentQualityTag = text;
	}

	private void Disable()
	{
		_shadowCamera.enabled = false;
		base.enabled = false;
		_currentQualityTag = null;
	}

	private void OnDrawGizmos()
	{
		if (_targetTexture == null)
		{
			_targetTexture = MakeTargetTexture();
			ClearShadowMap(_targetTexture);
			Shader.SetGlobalTexture("_ShadowMap", _targetTexture);
		}
	}
}
