using System.Xml.Serialization;
using Glu.Localization;

[XmlType("iap")]
public class IAPShopItemSimple : IAPShopItem
{
	[XmlAttribute("priceSoft")]
	public int? PriceSoft;

	[XmlAttribute("priceHard")]
	public int? PriceHard;

	[XmlAttribute("discount")]
	public int? Discount;

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

	public int GetDiscount()
	{
		int? discount = Discount;
		return discount.HasValue ? discount.Value : 0;
	}

	public override void Override(ShopConfigItem other)
	{
		base.Override(other);
		IAPShopItemSimple iAPShopItemSimple = other as IAPShopItemSimple;
		ShopItem.OverrideValue(ref iAPShopItemSimple.PriceHard, PriceHard);
		ShopItem.OverrideValue(ref iAPShopItemSimple.PriceSoft, PriceSoft);
		ShopItem.OverrideValue(ref iAPShopItemSimple.Discount, Discount);
	}

	public override ShopConfigItem Clone()
	{
		IAPShopItemSimple iAPShopItemSimple = new IAPShopItemSimple();
		Override(iAPShopItemSimple);
		return iAPShopItemSimple;
	}

	public virtual int GetValue()
	{
		if (PriceHard.HasValue)
		{
			return PriceHard.Value;
		}
		if (PriceSoft.HasValue)
		{
			return PriceSoft.Value;
		}
		return 0;
	}

	public virtual string GetValueKind()
	{
		if (PriceHard.HasValue)
		{
			return "HARD";
		}
		if (PriceSoft.HasValue)
		{
			return "SOFT";
		}
		return "ERROR";
	}

	public virtual string GetValueString()
	{
		if (PriceHard.HasValue)
		{
			string arg = NumberFormat.Get(PriceHard.Value);
			string @string = Strings.GetString("IDS_CURRENCY_HARD");
			return string.Format("{0} {1}", arg, @string);
		}
		if (PriceSoft.HasValue)
		{
			string arg2 = NumberFormat.Get(PriceSoft.Value);
			string string2 = Strings.GetString("IDS_CURRENCY_SOFT");
			return string.Format("{0} {1}", arg2, string2);
		}
		return "ERROR";
	}

	public virtual string GetDiscountString()
	{
		if (Discount.HasValue && Discount.Value > 0)
		{
			string @string = Strings.GetString("IDS_IAP_SHOP_DISCOUNT");
			return string.Format(@string, NumberFormat.Get(Discount.Value));
		}
		return string.Empty;
	}
}
