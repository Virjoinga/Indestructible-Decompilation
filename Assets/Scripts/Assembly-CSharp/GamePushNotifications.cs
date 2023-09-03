using System;
using System.Collections.Generic;
using Glu.Localization;

public static class GamePushNotifications
{
	public static void Cancel()
	{
		ANotificationManager.ClearActiveNotifications();
		ANotificationManager.ClearScheduledNotifications();
	}

	private static void Schedule(GamePush push)
	{
		Schedule(push.Text, push.FireDate, push.BadgeNumber);
	}

	public static void Schedule(string text, DateTime fireDate, int badgeNumber)
	{
		int time = (int)(fireDate - DateTime.Now).TotalSeconds;
		ANotificationManager.ScheduleNotificationSecFromNow(time, text, string.Empty);
	}

	public static void Setup()
	{
		Cancel();
		if (!MonoSingleton<SettingsController>.Instance.NotificationsEnabled)
		{
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		List<GamePush> list = new List<GamePush>();
		foreach (GarageVehicle boughtVehicle in MonoSingleton<Player>.Instance.BoughtVehicles)
		{
			GarageVehicleFuel fuel = boughtVehicle.Fuel;
			fuel.Update();
			if (!fuel.IsFull())
			{
				float refuelSeconds = fuel.GetRefuelSeconds();
				DateTime fireDate = utcNow.AddSeconds(refuelSeconds);
				ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(boughtVehicle.Vehicle.BodyId);
				string @string = Strings.GetString("IDS_LOCAL_PUSH_FULL_TANK");
				@string = string.Format(@string, Strings.GetString(itemBody.nameId));
				list.Add(new GamePush(@string, fireDate));
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		list.Sort((GamePush a, GamePush b) => a.FireDate.CompareTo(b.FireDate));
		int num = 0;
		using (List<GamePush>.Enumerator enumerator2 = list.GetEnumerator())
		{
			if (enumerator2.MoveNext())
			{
				GamePush current2 = enumerator2.Current;
				num++;
				current2.BadgeNumber = num;
				Schedule(current2);
			}
		}
	}
}
