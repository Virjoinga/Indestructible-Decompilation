using System;
using System.Collections;

public class PushNotifications_Android : PushNotifications
{
	private class LocalNotification_Android : ILocalNotification
	{
		public DateTime fireDate { get; set; }

		public string timeZone { get; set; }

		public string alertBody { get; set; }

		public string alertAction { get; set; }

		public bool hasAction { get; set; }

		public string alertLaunchImage { get; set; }

		public int applicationIconBadgeNumber { get; set; }

		public string soundName { get; set; }

		public IDictionary userInfo { get; set; }
	}

	private class RemoteNotification_Android : IRemoteNotification
	{
		public string alertBody
		{
			get
			{
				return null;
			}
		}

		public bool hasAction
		{
			get
			{
				return false;
			}
		}

		public int applicationIconBadgeNumber
		{
			get
			{
				return 0;
			}
		}

		public string soundName
		{
			get
			{
				return null;
			}
		}

		public IDictionary userInfo
		{
			get
			{
				return null;
			}
		}
	}

	public override ILocalNotification CreateLocalNotification()
	{
		return new LocalNotification_Android();
	}

	public override void SetUrbanAirshipCredentials(string uaKey, string uaSecret)
	{
	}

	public override void SetUrbanAirshipSilentTime(string start, string end)
	{
	}

	public override void RegisterForRemoteNotificationTypes(int notificationTypes)
	{
	}

	public override void UnregisterForRemoteNotifications()
	{
	}

	public override int GetEnabledRemoteNotificationTypes()
	{
		return 0;
	}

	public override string GetDeviceToken()
	{
		return null;
	}

	public override string GetRegistrationError()
	{
		return null;
	}

	public override int GetLocalNotificationCount()
	{
		return 0;
	}

	public override ILocalNotification GetLocalNotification(int index)
	{
		return null;
	}

	public override ILocalNotification[] GetScheduledLocalNotifications()
	{
		return null;
	}

	public override void ScheduleLocalNotification(ILocalNotification notification)
	{
	}

	public override void PresentLocalNotificationNow(ILocalNotification notification)
	{
	}

	public override void CancelLocalNotification(ILocalNotification notification)
	{
	}

	public override void CancelAllLocalNotifications()
	{
	}

	public override void ClearLocalNotifications()
	{
	}

	public override int GetRemoteNotificationCount()
	{
		return 0;
	}

	public override IRemoteNotification GetRemoteNotification(int index)
	{
		return null;
	}

	public override void ClearRemoteNotifications()
	{
	}

	public override int GetBadgeNumber()
	{
		return 0;
	}

	public override void SetBadgeNumber(int number)
	{
	}
}
