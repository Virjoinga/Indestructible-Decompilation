using UnityEngine;

public class UIDialog : MonoBehaviour
{
	public GameObject ShadePrefab;

	public Color ShadeColor = new Color(0f, 0f, 0f, 0.7f);

	protected Transform _transform;

	protected Transform _shadeTransform;

	protected bool _visible;

	protected bool _closed;

	public bool Visible
	{
		get
		{
			return _visible;
		}
	}

	public bool Closed
	{
		get
		{
			return _closed;
		}
	}

	protected virtual void Awake()
	{
		_transform = GetComponent<Transform>();
		GameObject gameObject = (GameObject)Object.Instantiate(ShadePrefab);
		UIDialogShade component = gameObject.GetComponent<UIDialogShade>();
		_shadeTransform = gameObject.GetComponent<Transform>();
		component.SetColor(ShadeColor);
		_shadeTransform.parent = _transform;
		_shadeTransform.localPosition = new Vector3(0f, 0f, 1f);
	}

	protected virtual void Start()
	{
	}

	public virtual void Activate()
	{
		if (!_visible)
		{
			float z = DialogsQueue.AcquireLayer();
			_transform.position = new Vector3(0f, 0f, z);
			MonoUtils.SetActive(this, true);
			_visible = true;
		}
	}

	public virtual void Close()
	{
		if (!_closed)
		{
			_closed = true;
			_visible = false;
			DialogsQueue.ReleaseLayer();
			MonoUtils.SetActive(this, false);
			Object.Destroy(base.gameObject);
		}
	}
}
