using System;

public class GarageVehicleFuel
{
	private const long LevelDurationFirst = 9000000000L;

	private const long LevelDuration = 15000000000L;

	private const long FreezeDuration = 864000000000L;

	public const int LevelsCount = 5;

	public int Level;

	public long LevelTime;

	public long FreezeTime;

	public bool FreezeForever;

	private long _ticks;

	public GarageVehicleFuel()
	{
		LevelTime = DateTime.UtcNow.Ticks;
		Level = 5;
	}

	private long GetTicks()
	{
		return DateTime.UtcNow.Ticks;
	}

	public void Update()
	{
		_ticks = GetTicks();
		if (FreezeTime > 0)
		{
			if (FreezeForever)
			{
				return;
			}
			long num = _ticks - FreezeTime;
			if (num < 864000000000L)
			{
				return;
			}
			FreezeTime = 0L;
		}
		if (Level >= 5)
		{
			return;
		}
		long num2 = _ticks - LevelTime;
		if (Level == 0)
		{
			if (num2 < 9000000000L)
			{
				return;
			}
			LevelTime += 9000000000L;
			num2 -= 9000000000L;
			Level++;
		}
		while (num2 >= 15000000000L)
		{
			LevelTime += 15000000000L;
			num2 -= 15000000000L;
			Level++;
		}
		if (Level > 5)
		{
			Level = 5;
		}
	}

	public int GetLevel()
	{
		return Level;
	}

	public float GetLevelRelative()
	{
		return (float)Level / 5f;
	}

	public bool IsFull()
	{
		return Level == 5;
	}

	public float GetLevelPosition(ref float seconds)
	{
		if (FreezeTime > 0)
		{
			if (FreezeForever)
			{
				seconds = 8.64E+11f;
				return 1f;
			}
			long num = 864000000000L - (_ticks - FreezeTime);
			float num2 = (float)num / 8.64E+11f;
			seconds = 8.64E+11f * num2;
			seconds /= 10000000f;
			return num2;
		}
		long num3 = 9000000000L;
		if (Level > 0)
		{
			num3 = 15000000000L;
		}
		if (Level < 5)
		{
			long num4 = num3 - (_ticks - LevelTime);
			float num5 = (float)num4 / (float)num3;
			seconds = (float)num3 * num5;
			seconds /= 10000000f;
			return 1f - num5;
		}
		seconds = 0f;
		return 1f;
	}

	public float GetRefuelSeconds()
	{
		if (FreezeTime > 0)
		{
			return 0f;
		}
		if (Level < 5)
		{
			long num = 9000000000L;
			num += 60000000000L;
			if (Level > 0)
			{
				num -= 9000000000L;
			}
			if (Level > 1)
			{
				num -= (Level - 1) * 15000000000L;
			}
			num -= _ticks - LevelTime;
			return (float)num / 10000000f;
		}
		return 0f;
	}

	public bool IsFrozen()
	{
		return FreezeTime > 0;
	}

	public void Freeze()
	{
		FreezeTime = _ticks;
		Level = 5;
		LevelTime = 0L;
		GameConfiguration configuration = MonoSingleton<GameController>.Instance.Configuration;
		FreezeForever = configuration.Fueling.FreezeForever;
	}

	public void Drain()
	{
		LevelTime = _ticks;
		FreezeForever = false;
		FreezeTime = 0L;
		Level = 0;
	}

	public bool AddGallon()
	{
		if (Level < 5 && FreezeTime <= 0)
		{
			Level++;
			if (Level >= 5)
			{
				Level = 5;
				LevelTime = 0L;
			}
			return true;
		}
		return false;
	}

	public bool Spend()
	{
		if (Level > 0)
		{
			if (FreezeTime <= 0)
			{
				Level--;
				if (Level == 0 || Level == 4)
				{
					LevelTime = _ticks;
				}
			}
			return true;
		}
		return false;
	}
}
