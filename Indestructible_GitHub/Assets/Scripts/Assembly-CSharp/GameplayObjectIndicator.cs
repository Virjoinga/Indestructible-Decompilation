using UnityEngine;

public class GameplayObjectIndicator : MonoBehaviour
{
	protected string _mainCameraName = "Main Camera";

	protected string _uiCameraName = "UICamera";

	protected Camera _mainCamera;

	protected Camera _uiCamera;

	protected Transform _transform;

	protected bool _initialized;

	protected virtual void Awake()
	{
		_transform = GetComponent<Transform>();
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void Update()
	{
		Initialize();
	}

	protected virtual void LateUpdate()
	{
	}

	protected virtual Vector3 MainToUICamera(Vector3 vector)
	{
		Vector3 vector2 = _mainCamera.WorldToViewportPoint(vector);
		if (vector2.z < 0f)
		{
			vector2 = -vector2;
		}
		return _uiCamera.ViewportToWorldPoint(vector2);
	}

	protected virtual Camera FindCamera(string name)
	{
		GameObject gameObject = GameObject.Find(name);
		if (gameObject != null)
		{
			return gameObject.GetComponent<Camera>();
		}
		return null;
	}

	protected virtual bool Initialize()
	{
		if (!_initialized)
		{
			_uiCamera = FindCamera(_uiCameraName);
			_mainCamera = FindCamera(_mainCameraName);
			_initialized = _mainCamera != null && _uiCamera != null;
		}
		return _initialized;
	}
}
