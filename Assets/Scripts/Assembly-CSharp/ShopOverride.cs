using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

public class ShopOverride
{
	public class Override
	{
		public class TimeRange
		{
			public DateTime start;

			public TimeSpan span;

			public TimeRange(DateTime start, TimeSpan span)
			{
				this.start = start;
				this.span = span;
			}

			public TimeRange(DateTime start, DateTime end)
			{
				this.start = ((!(start <= end)) ? end : start);
				span = ((!(start <= end)) ? (start - end) : (end - start));
			}

			public bool Contains(DateTime time)
			{
				if (start >= time && start + span <= time)
				{
					return true;
				}
				return false;
			}

			public bool Intersects(TimeRange range)
			{
				if (start + span >= range.start && start <= range.start + range.span)
				{
					return true;
				}
				return false;
			}

			public void Expand(TimeRange range)
			{
				DateTime dateTime = ((!(start + span > range.start + range.span)) ? (range.start + range.span) : (start + span));
				start = ((!(start < range.start)) ? range.start : start);
				span = dateTime - start;
			}
		}

		public class Timing
		{
			public enum Period
			{
				once = 0,
				daily = 1,
				weekly = 2,
				monthly = 3
			}

			private DateTimeFormatInfo _dateTimeFormatInfo;

			private string _strStart;

			private string _strEnd;

			[XmlAttribute("period")]
			public Period period;

			[XmlAttribute("every")]
			public string every;

			private static string[] _weekDays = new string[7] { "sunday", "monday", "tuesday", "wednesday", "thursday", "friday", "saturday" };

			private DateTime _start = DateTime.MinValue;

			private DateTime _end = DateTime.MinValue;

			[XmlAttribute("start")]
			public string strStart
			{
				get
				{
					return _strStart;
				}
				set
				{
					_strStart = value;
					if (!DateTime.TryParse(_strStart, _dateTimeFormatInfo, DateTimeStyles.None, out _start))
					{
						_start = DateTime.MinValue;
					}
				}
			}

			[XmlAttribute("end")]
			public string strEnd
			{
				get
				{
					return _strEnd;
				}
				set
				{
					_strEnd = value;
					if (!DateTime.TryParse(_strEnd, _dateTimeFormatInfo, DateTimeStyles.None, out _end))
					{
						_end = DateTime.MinValue;
					}
				}
			}

			public DateTime Start
			{
				get
				{
					return _start;
				}
			}

			public DateTime End
			{
				get
				{
					return _end;
				}
			}

			public Timing()
			{
				_dateTimeFormatInfo = new CultureInfo("en-US", false).DateTimeFormat;
				period = Period.once;
				every = string.Empty;
			}

			private static DayOfWeek GetDay(string day)
			{
				int num = 0;
				string[] weekDays = _weekDays;
				foreach (string text in weekDays)
				{
					if (text == day)
					{
						return (DayOfWeek)num;
					}
					num++;
				}
				return DayOfWeek.Saturday;
			}

			public TimeRange GetCloseRange(DateTime time)
			{
				DateTime dateTime = new DateTime(time.Year, time.Month, time.Day);
				TimeSpan timeSpan = ((!(Start != DateTime.MinValue)) ? new TimeSpan(0, 0, 0, 0) : new TimeSpan(0, Start.Hour, Start.Minute, Start.Second, Start.Millisecond));
				DateTime start = dateTime.Add(timeSpan);
				TimeSpan timeSpan2 = ((!(End != DateTime.MinValue)) ? new TimeSpan(1, 0, 0, 0) : new TimeSpan(0, End.Hour, End.Minute, End.Second, End.Millisecond));
				DateTime dateTime2 = dateTime.Add(timeSpan2);
				switch (period)
				{
				case Period.once:
				{
					DateTime dateTime3 = ((!(End != DateTime.MinValue)) ? (Start + new TimeSpan(1, 0, 0, 0)) : End);
					if (time <= dateTime3)
					{
						return new TimeRange(Start, dateTime3);
					}
					break;
				}
				case Period.daily:
					if (time <= dateTime2)
					{
						return new TimeRange(start, dateTime2);
					}
					break;
				case Period.weekly:
				{
					DayOfWeek day = GetDay(every);
					if (dateTime.DayOfWeek == day && time <= dateTime2)
					{
						return new TimeRange(start, dateTime2);
					}
					TimeSpan timeSpan4 = new TimeSpan(1, 0, 0, 0);
					do
					{
						dateTime += timeSpan4;
					}
					while (dateTime.DayOfWeek != day);
					return new TimeRange(dateTime + timeSpan, dateTime + timeSpan2);
				}
				case Period.monthly:
				{
					int num = int.Parse(every);
					if (num < 1)
					{
						num = 1;
					}
					if (num > 31)
					{
						num = 31;
					}
					if (dateTime.Day == num && time <= dateTime2)
					{
						return new TimeRange(start, dateTime2);
					}
					TimeSpan timeSpan3 = new TimeSpan(1, 0, 0, 0);
					do
					{
						dateTime += timeSpan3;
					}
					while (dateTime.Day != num);
					return new TimeRange(dateTime + timeSpan, dateTime + timeSpan2);
				}
				}
				return null;
			}
		}

		[XmlArray("timings")]
		[XmlArrayItem("timing")]
		public List<Timing> timings;

		[XmlElement("ShopConfig")]
		public ShopConfig config;
	}

	[XmlArrayItem("override")]
	[XmlArray("overrides")]
	public List<Override> overrides;

	private void Print()
	{
		if (!LoggerSingleton<ShopConfigLogger>.IsEnabledFor(20))
		{
			return;
		}
		for (int i = 0; i < overrides.Count; i++)
		{
			for (int j = 0; j < overrides[i].timings.Count; j++)
			{
			}
			overrides[i].config.Print();
		}
	}

	public static ShopOverride Load(Stream stream)
	{
		XmlAttributes xmlAttributes = new XmlAttributes();
		foreach (Type shopConfigItemType in ShopConfig.shopConfigItemTypes)
		{
			xmlAttributes.XmlArrayItems.Add(new XmlArrayItemAttribute(shopConfigItemType));
		}
		XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();
		xmlAttributeOverrides.Add(typeof(ShopConfig), "items", xmlAttributes);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(ShopOverride), xmlAttributeOverrides);
		return (ShopOverride)xmlSerializer.Deserialize(stream);
	}
}
