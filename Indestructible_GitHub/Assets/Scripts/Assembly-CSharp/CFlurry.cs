using System.Collections.Generic;

public static class CFlurry
{
	public static void LogEvent(string eventTypeId, Dictionary<string, object> eventParams)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, object> eventParam in eventParams)
		{
			list.Add(eventParam.Key);
			list.Add(eventParam.Value.ToString());
		}
		AStats.Flurry.LogEvent(eventTypeId, list.ToString());
	}
}
