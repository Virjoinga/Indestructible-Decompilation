using System;
using System.Collections.Generic;
using System.Linq;

namespace Glu.Plugins.AStats
{
	public class AStats
	{
		internal static void LogEvent(string eventId, IList<StatArg> args)
		{
			if (eventId == null)
			{
				throw new ArgumentNullException("eventId");
			}
			IEnumerable<string> source = Enumerable.Empty<string>();
			if (args != null)
			{
				source = args.SelectMany((StatArg sa) => new string[2]
				{
					sa.Key,
					sa.GetString()
				});
			}
			global::AStats.Flurry.LogEvent(eventId, source.ToArray());
			List<string> list = new List<string>(10);
			if (args != null)
			{
				IEnumerable<string> enumerable = from sa in args.Where((StatArg sa) => sa.Type == typeof(string)).Take(3)
					select sa.GetString();
				int num = 1;
				foreach (string item2 in enumerable)
				{
					string item = string.Format("st{0}", num);
					list.Add(item);
					list.Add(item2);
					num++;
				}
				string text = (from sa in args
					where sa.Type == typeof(int)
					select sa.GetString()).FirstOrDefault();
				if (text != null)
				{
					list.Add("v");
					list.Add(text);
				}
			}
			global::AStats.Kontagent.LogEvent(eventId, list.ToArray());
		}

		internal static void RevenueTracking(string eventId, float price, IList<StatArg> args)
		{
			if (eventId == null)
			{
				throw new ArgumentNullException("eventId");
			}
			string currency = "USD";
			if (args != null)
			{
				string text = (from sa in args
					where sa.Key == "currency"
					select sa.GetString()).SingleOrDefault();
				if (text != null)
				{
					currency = text;
				}
			}
			global::AStats.MobileAppTracking.RevenueTracking(eventId, price, currency);
		}
	}
}
