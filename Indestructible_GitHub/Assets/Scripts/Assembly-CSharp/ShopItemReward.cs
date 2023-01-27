using System.Xml.Serialization;

public class ShopItemReward
{
	[XmlAttribute("moneySoft")]
	public int? MoneySoft;

	[XmlAttribute("moneyHard")]
	public int? MoneyHard;

	[XmlAttribute("experience")]
	public int? Experience;

	[XmlAttribute("influencePoints")]
	public int? InfluencePoints;

	public void Override(ShopItemReward other)
	{
		ShopItem.OverrideValue(ref other.MoneySoft, MoneySoft);
		ShopItem.OverrideValue(ref other.MoneyHard, MoneyHard);
		ShopItem.OverrideValue(ref other.Experience, Experience);
		ShopItem.OverrideValue(ref other.InfluencePoints, InfluencePoints);
	}

	public string GetString()
	{
		string text = string.Empty;
		if (MoneyHard.HasValue && MoneyHard.Value > 0)
		{
			text = text + "\u001f " + MoneyHard.Value;
		}
		if (MoneySoft.HasValue && MoneySoft.Value > 0)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text = text + "\u001e " + MoneySoft.Value;
		}
		if (Experience.HasValue && Experience.Value > 0)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text = text + "\u001c " + Experience.Value;
		}
		if (InfluencePoints.HasValue && InfluencePoints.Value > 0)
		{
			if (text.Length > 0)
			{
				text += ", ";
			}
			text = text + "\u001d " + InfluencePoints.Value;
		}
		return text;
	}
}
