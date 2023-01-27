using System.Collections.Generic;
using UnityEngine;

public class DialogsQueue : MonoSingleton<DialogsQueue>
{
	private bool _pause;

	private Transform _transform;

	private Queue<UIDialog> _visible = new Queue<UIDialog>();

	private Queue<UIDialog> _dialogs = new Queue<UIDialog>();

	private UIDialog _dialog;

	protected static float _layer = -10f;

	protected static float _layerDelta = -10f;

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Clear();
	}

	public static void CheckLayer(ref float layer)
	{
		if (layer < -100f)
		{
			layer = -100f;
		}
		else if (layer > -10f)
		{
			layer = -10f;
		}
	}

	public static float AcquireLayer()
	{
		_layer += _layerDelta;
		CheckLayer(ref _layer);
		return _layer;
	}

	public static void ReleaseLayer()
	{
		_layer -= _layerDelta;
		CheckLayer(ref _layer);
	}

	protected override void Awake()
	{
		base.Awake();
		_transform = GetComponent<Transform>();
	}

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (_pause)
		{
			return;
		}
		while (_visible.Count > 0)
		{
			UIDialog uIDialog = _visible.Dequeue();
			if (!uIDialog.Visible && !uIDialog.Closed)
			{
				uIDialog.Activate();
			}
		}
		bool flag = _dialog == null;
		if (!flag)
		{
			flag = _dialog.Closed;
		}
		if (!flag)
		{
			return;
		}
		while (_dialogs.Count > 0)
		{
			UIDialog uIDialog2 = _dialogs.Dequeue();
			if (uIDialog2 == null || uIDialog2.Closed)
			{
				continue;
			}
			if (!uIDialog2.Visible)
			{
				uIDialog2.Activate();
			}
			_dialog = uIDialog2;
			break;
		}
	}

	public void Clear()
	{
		foreach (Transform item in _transform)
		{
			Object.Destroy(item.gameObject);
		}
		_visible.Clear();
		_dialogs.Clear();
		_dialog = null;
		_layer = -10f;
	}

	public bool ContainDialog<T>() where T : MonoBehaviour
	{
		foreach (UIDialog dialog in _dialogs)
		{
			if (dialog is T)
			{
				return true;
			}
		}
		return false;
	}

	public void Add(UIDialog dialog)
	{
		Transform component = dialog.GetComponent<Transform>();
		component.parent = _transform;
		MonoUtils.SetActive(dialog, false);
		_dialogs.Enqueue(dialog);
	}

	public void Show(UIDialog dialog)
	{
		_visible.Enqueue(dialog);
		Add(dialog);
	}

	public void Pause()
	{
		_pause = true;
	}

	public void Resume()
	{
		_pause = false;
	}

	public bool IsEmpty()
	{
		return _dialog == null;
	}

	public int WaitingCount()
	{
		int num = 0;
		UIDialog[] array = _dialogs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] == null) && !array[i].Closed)
			{
				num++;
			}
		}
		return num;
	}
}
