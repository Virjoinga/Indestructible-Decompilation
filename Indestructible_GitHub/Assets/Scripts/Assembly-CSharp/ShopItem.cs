using System.Xml.Serialization;
using Glu.Localization;
using UnityEngine;

public abstract class ShopItem : ShopConfigItem
{
	protected ShopItemType _itemType;

	[XmlAttribute("priceSoft")]
	public int? PriceSoft;

	[XmlAttribute("priceHard")]
	public int? PriceHard;

	[XmlAttribute("itemSprite")]
	public string ItemSprite;

	[XmlAttribute("descriptionId")]
	public string DescriptionId;

	[XmlAttribute("componentPrefab")]
	public string ComponentPrefab;

	[XmlAttribute("gradeLevel")]
	public int? GradeLevel;

	[XmlAttribute("power")]
	public int? Power;

	[XmlAttribute("offense")]
	public int? Offense;

	[XmlAttribute("packCount")]
	public int? PackCount;

	[XmlAttribute("count")]
	public int? Count;

	[XmlElement("lock")]
	public ShopItemCondition Lock = new ShopItemCondition();

	[XmlAttribute("saleText")]
	public string SaleText;

	[XmlIgnore]
	public int? OldPriceSoft;

	[XmlIgnore]
	public int? OldPriceHard;

	[XmlIgnore]
	public string GroupId = string.Empty;

	public ShopItemType ItemType
	{
		get
		{
			return _itemType;
		}
	}

	public ShopItem()
	{
		_itemType = ShopItemType.None;
	}

	public int GetPower()
	{
		int? power = Power;
		return power.HasValue ? power.Value : 0;
	}

	public int GetOffense()
	{
		int? offense = Offense;
		return offense.HasValue ? offense.Value : 0;
	}

	public int GetPriceSoft()
	{
		int? priceSoft = PriceSoft;
		return priceSoft.HasValue ? priceSoft.Value : 0;
	}

	public int GetPriceHard()
	{
		int? priceHard = PriceHard;
		return priceHard.HasValue ? priceHard.Value : 0;
	}

	public int GetOldPriceSoft()
	{
		int? oldPriceSoft = OldPriceSoft;
		return oldPriceSoft.HasValue ? oldPriceSoft.Value : 0;
	}

	public int GetOldPriceHard()
	{
		int? oldPriceHard = OldPriceHard;
		return oldPriceHard.HasValue ? oldPriceHard.Value : 0;
	}

	public int GetGradeLevel()
	{
		int? gradeLevel = GradeLevel;
		return gradeLevel.HasValue ? gradeLevel.Value : 0;
	}

	public int GetPackCount()
	{
		int? packCount = PackCount;
		return (!packCount.HasValue) ? (-1) : packCount.Value;
	}

	public int GetCount()
	{
		int? count = Count;
		return count.HasValue ? count.Value : 0;
	}

	public string GetPackCountString()
	{
		if (GetPackCount() > 0)
		{
			return string.Format(" x{0}", GetPackCount());
		}
		return string.Empty;
	}

	public string GetCountString()
	{
		if (GetPackCount() > 0 && GetCount() > 0)
		{
			return string.Format("x {0}", GetCount());
		}
		return string.Empty;
	}

	public bool IsEmpty()
	{
		return GetPackCount() > 0 && GetCount() <= 0;
	}

	public virtual bool IsLocked()
	{
		if (Lock != null)
		{
			return Lock.Lock();
		}
		return false;
	}

	public virtual bool IsLocked(ShopItemCurrency currency)
	{
		bool flag = IsLocked();
		if (flag && IsDualPriced())
		{
			flag = currency == ShopItemCurrency.Soft;
		}
		return flag;
	}

	public virtual string LockText()
	{
		if (Lock != null)
		{
			return Lock.Text();
		}
		return string.Empty;
	}

	public virtual bool IsStorable()
	{
		return true;
	}

	public virtual bool IsRemovable()
	{
		return false;
	}

	public virtual bool IsDualPriced()
	{
		return HasPriceHard() && HasPriceSoft();
	}

	protected virtual bool HasPriceHard()
	{
		return PriceHard.HasValue && PriceHard.Value >= 0;
	}

	protected virtual bool HasPriceSoft()
	{
		return PriceSoft.HasValue && PriceSoft.Value >= 0;
	}

	public virtual bool HasCurrencyHard(ShopItemCurrency currency)
	{
		bool flag = currency == ShopItemCurrency.Hard || currency == ShopItemCurrency.None;
		return HasPriceHard() && flag;
	}

	public virtual bool HasCurrencySoft(ShopItemCurrency currency)
	{
		bool flag = currency == ShopItemCurrency.Soft || currency == ShopItemCurrency.None;
		return HasPriceSoft() && flag;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		ShopItem shopItem = other as ShopItem;
		OverrideValue(ref shopItem.PriceHard, PriceHard);
		OverrideValue(ref shopItem.PriceSoft, PriceSoft);
		OverrideValue(ref shopItem.SaleText, SaleText);
		OverrideValue(ref shopItem.ComponentPrefab, ComponentPrefab);
		OverrideValue(ref shopItem.DescriptionId, DescriptionId);
		OverrideValue(ref shopItem.ItemSprite, ItemSprite);
		OverrideValue(ref shopItem.GradeLevel, GradeLevel);
		OverrideValue(ref shopItem.Power, Power);
		OverrideValue(ref shopItem.Offense, Offense);
		OverrideValue(ref shopItem.GroupId, GroupId);
		OverrideValue(ref shopItem.PackCount, PackCount);
		OverrideValue(ref shopItem.Count, Count);
		Lock.Override(shopItem.Lock);
	}

	public static void OverrideValue<T>(ref T? otherValue, T? thisValue) where T : struct
	{
		if (thisValue.HasValue)
		{
			otherValue = thisValue;
		}
	}

	public static void OverrideValue<T>(ref T otherValue, T thisValue) where T : class
	{
		if (thisValue != null)
		{
			otherValue = thisValue;
		}
	}

	public virtual int GetPrice(ShopItemCurrency currency)
	{
		if (HasCurrencyHard(currency))
		{
			return PriceHard.Value;
		}
		if (HasCurrencySoft(currency))
		{
			return PriceSoft.Value;
		}
		return 0;
	}

	public virtual string GetPriceKind()
	{
		bool flag = HasPriceHard();
		bool flag2 = HasPriceSoft();
		if (flag && flag2)
		{
			return "DUAL";
		}
		if (flag)
		{
			return "HARD";
		}
		if (flag2)
		{
			return "SOFT";
		}
		return "ERROR";
	}

	public virtual string GetParameters()
	{
		return string.Format("{0} {1} {2}", GetGradeLevel(), GetPower(), GetOffense());
	}

	public virtual string GetPriceString(ShopItemCurrency currency)
	{
		return GetPriceString(currency, true, false);
	}

	public virtual string GetPriceString(ShopItemCurrency currency, bool icon, bool name)
	{
		if (HasCurrencyHard(currency))
		{
			if (PriceHard.Value <= 0)
			{
				return Strings.GetString("IDS_SHOP_ITEM_FREE");
			}
			string text = NumberFormat.Get(PriceHard.Value);
			if (name)
			{
				text = text + " " + Strings.GetString("IDS_CURRENCY_HARD");
			}
			if (icon)
			{
				text = "\u001f " + text;
			}
			return text;
		}
		if (HasCurrencySoft(currency))
		{
			if (PriceSoft.Value <= 0)
			{
				return Strings.GetString("IDS_SHOP_ITEM_FREE");
			}
			string text2 = NumberFormat.Get(PriceSoft.Value);
			if (name)
			{
				text2 = text2 + " " + Strings.GetString("IDS_CURRENCY_SOFT");
			}
			if (icon)
			{
				text2 = "\u001e " + text2;
			}
			return text2;
		}
		return "ERROR";
	}

	public virtual string GetOldPriceString()
	{
		if (OldPriceHard.HasValue)
		{
			return "\u001f " + NumberFormat.Get(OldPriceHard.Value);
		}
		if (OldPriceSoft.HasValue)
		{
			return "\u001e " + NumberFormat.Get(OldPriceSoft.Value);
		}
		return "ERROR";
	}

	public virtual int Consume()
	{
		if (GetPackCount() < 0)
		{
			return -1;
		}
		int num = GetCount();
		if (num > 0)
		{
			num--;
		}
		Count = num;
		Debug.Log("ShopItem Consume count = " + num);
		return num;
	}

	public virtual bool CanBuyMore()
	{
		if (GetPackCount() < 0)
		{
			return false;
		}
		return true;
	}
}
