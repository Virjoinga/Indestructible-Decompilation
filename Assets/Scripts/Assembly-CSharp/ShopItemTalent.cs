using System.Xml.Serialization;

[XmlType("talent")]
public class ShopItemTalent : ShopItem
{
	[XmlAttribute("parentId")]
	public string ParentId;

	[XmlAttribute("unlockPoints")]
	public int? UnlockPoints;

	[XmlAttribute("levelsCount")]
	public int? LevelsCount;

	public ShopItemTalent()
	{
		_itemType = ShopItemType.Talent;
	}

	public int GetUnlockPoints()
	{
		int? unlockPoints = UnlockPoints;
		return unlockPoints.HasValue ? unlockPoints.Value : 0;
	}

	public int GetLevelsCount()
	{
		int? levelsCount = LevelsCount;
		return levelsCount.HasValue ? levelsCount.Value : 0;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ShopItemTalent shopItemTalent = other as ShopItemTalent;
		ShopItem.OverrideValue(ref shopItemTalent.ParentId, ParentId);
		ShopItem.OverrideValue(ref shopItemTalent.UnlockPoints, UnlockPoints);
		ShopItem.OverrideValue(ref shopItemTalent.LevelsCount, LevelsCount);
	}

	public override ShopConfigItem Clone()
	{
		ShopItemTalent shopItemTalent = new ShopItemTalent();
		Override(shopItemTalent);
		return shopItemTalent;
	}
}
