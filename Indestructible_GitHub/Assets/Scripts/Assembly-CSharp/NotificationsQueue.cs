using System.Collections.Generic;
using UnityEngine;

public class NotificationsQueue : MonoSingleton<NotificationsQueue>
{
	private GameObject _notificationPrefab;

	private Queue<GameObject> _notifications = new Queue<GameObject>();

	private GameObject _notification;

	private Transform _transform;

	protected override void Awake()
	{
		base.Awake();
		_transform = GetComponent<Transform>();
		_notificationPrefab = (GameObject)Resources.Load("Dialogs/NotificationText");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Clear();
	}

	private void Update()
	{
		bool flag = _notification == null;
		if (!flag)
		{
			flag = !_notification.active;
		}
		if (!flag || _notifications.Count <= 0)
		{
			return;
		}
		GameObject gameObject = _notifications.Dequeue();
		if (gameObject != null)
		{
			UINotification component = gameObject.GetComponent<UINotification>();
			if (component != null)
			{
				component.Activate();
			}
		}
		_notification = gameObject;
	}

	public void Clear()
	{
		foreach (GameObject notification in _notifications)
		{
			if (notification != null)
			{
				Object.Destroy(notification);
			}
		}
		_notifications.Clear();
		Object.Destroy(_notification);
		_notification = null;
		foreach (Transform item in _transform)
		{
			if (item != null)
			{
				Object.Destroy(item.gameObject);
			}
		}
	}

	public void Add(GameObject o)
	{
		o.transform.parent = _transform;
		o.SetActiveRecursively(false);
		_notifications.Enqueue(o);
	}

	public void Show(UINotification notification)
	{
		Transform component = notification.GetComponent<Transform>();
		component.parent = _transform;
		component.localPosition = Vector3.zero;
		notification.Activate();
	}

	public void AddText(string text)
	{
		CachedObject.Cache cache = ObjectCacheManager.Instance.GetCache(_notificationPrefab);
		CachedObject cachedObject = cache.Activate();
		AnimatedNotificationText component = cachedObject.GetComponent<AnimatedNotificationText>();
		component.Notification.Text = text;
		component.SetCache(cachedObject);
		Add(cachedObject.gameObject);
	}
}
