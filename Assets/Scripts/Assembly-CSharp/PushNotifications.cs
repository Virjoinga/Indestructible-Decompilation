using System;
using System.Collections;

public abstract class PushNotifications
{
	public interface ILocalNotification
	{
		DateTime fireDate { get; set; }

		string timeZone { get; set; }

		string alertBody { get; set; }

		string alertAction { get; set; }

		bool hasAction { get; set; }

		string alertLaunchImage { get; set; }

		int applicationIconBadgeNumber { get; set; }

		string soundName { get; set; }

		IDictionary userInfo { get; set; }
	}

	public interface IRemoteNotification
	{
		string alertBody { get; }

		bool hasAction { get; }

		int applicationIconBadgeNumber { get; }

		string soundName { get; }

		IDictionary userInfo { get; }
	}

	public const int NOTIFICATION_TYPE_NONE = 0;

	public const int NOTIFICATION_TYPE_ALERT = 1;

	public const int NOTIFICATION_TYPE_BADGE = 2;

	public const int NOTIFICATION_TYPE_SOUND = 4;

	private static PushNotifications m_instance;

	public static PushNotifications GetInstance()
	{
		if (m_instance == null)
		{
			m_instance = new PushNotifications_Android();
		}
		return m_instance;
	}

	public abstract ILocalNotification CreateLocalNotification();

	public abstract void SetUrbanAirshipCredentials(string uaKey, string uaSecret);

	public abstract void SetUrbanAirshipSilentTime(string start, string end);

	public abstract void RegisterForRemoteNotificationTypes(int notificationTypes);

	public abstract void UnregisterForRemoteNotifications();

	public abstract int GetEnabledRemoteNotificationTypes();

	public abstract string GetDeviceToken();

	public abstract string GetRegistrationError();

	public abstract int GetLocalNotificationCount();

	public abstract ILocalNotification GetLocalNotification(int index);

	public abstract ILocalNotification[] GetScheduledLocalNotifications();

	public abstract void ScheduleLocalNotification(ILocalNotification notification);

	public abstract void PresentLocalNotificationNow(ILocalNotification notification);

	public abstract void CancelLocalNotification(ILocalNotification notification);

	public abstract void CancelAllLocalNotifications();

	public abstract void ClearLocalNotifications();

	public abstract int GetRemoteNotificationCount();

	public abstract IRemoteNotification GetRemoteNotification(int index);

	public abstract void ClearRemoteNotifications();

	public abstract int GetBadgeNumber();

	public abstract void SetBadgeNumber(int number);
}
