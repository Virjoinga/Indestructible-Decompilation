using System;

public class PlayerBoost
{
	public const long DefaultDuration = 864000000000L;

	public IAPShopItemBoost Item;

	public long Duration = 864000000000L;

	public bool BoostForever;

	public long StartTime;

	public float GetSeconds()
	{
		if (BoostForever)
		{
			return Duration;
		}
		long ticks = DateTime.UtcNow.Ticks;
		long num = StartTime + Duration - ticks;
		if (num < 0)
		{
			num = 0L;
		}
		return num / 10000000;
	}

	public void Buy(IAPShopItemBoost item)
	{
		Item = item;
		BoostForever = item.id == "iap_boost_2";
		StartTime = DateTime.UtcNow.Ticks;
	}
}
