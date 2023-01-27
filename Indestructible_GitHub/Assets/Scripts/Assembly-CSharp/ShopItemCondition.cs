using System.Xml.Serialization;
using Glu.Localization;

public class ShopItemCondition
{
	[XmlAttribute("completeCampaign")]
	public int? CompleteCampaign;

	[XmlAttribute("level")]
	public int? Level;

	[XmlAttribute("influencePoints")]
	public int? InfluencePoints;

	[XmlAttribute("boughtIAPs")]
	public int? BoughtIAPs;

	[XmlAttribute("limitBoughtIAPs")]
	public int? LimitBoughtIAPs;

	public int GetLevel()
	{
		int? level = Level;
		return level.HasValue ? level.Value : 0;
	}

	public int GetInfluencePoints()
	{
		int? influencePoints = InfluencePoints;
		return influencePoints.HasValue ? influencePoints.Value : 0;
	}

	public void Override(ShopItemCondition other)
	{
		ShopItem.OverrideValue(ref other.Level, Level);
		ShopItem.OverrideValue(ref other.InfluencePoints, InfluencePoints);
		ShopItem.OverrideValue(ref other.CompleteCampaign, CompleteCampaign);
	}

	public bool Lock()
	{
		bool flag = false;
		if (!flag && CompleteCampaign.HasValue)
		{
			flag = !MonoSingleton<Player>.Instance.CampaignCompleted;
		}
		if (!flag && Level.HasValue)
		{
			flag = (int)MonoSingleton<Player>.Instance.Level < Level.Value;
		}
		if (!flag && InfluencePoints.HasValue)
		{
			flag = (int)MonoSingleton<Player>.Instance.InfluencePoints < InfluencePoints.Value;
		}
		if (!flag && BoughtIAPs.HasValue)
		{
			flag = MonoSingleton<Player>.Instance.Statistics.TotalBoughtIAPs < BoughtIAPs.Value;
		}
		if (!flag && LimitBoughtIAPs.HasValue)
		{
			flag = MonoSingleton<Player>.Instance.Statistics.TotalBoughtIAPs >= LimitBoughtIAPs.Value;
		}
		return flag;
	}

	public string Text()
	{
		if (CompleteCampaign.HasValue)
		{
			return Strings.GetString("IDS_SHOP_ITEM_LOCK_CAMPAIGN");
		}
		if (Level.HasValue)
		{
			string @string = Strings.GetString("IDS_SHOP_ITEM_LOCK_LEVEL");
			return string.Format(@string, Level);
		}
		if (InfluencePoints.HasValue)
		{
			string string2 = Strings.GetString("IDS_SHOP_ITEM_LOCK_INFLUENCE_POINTS");
			return string.Format(string2, "\u001d", InfluencePoints);
		}
		return string.Empty;
	}
}
