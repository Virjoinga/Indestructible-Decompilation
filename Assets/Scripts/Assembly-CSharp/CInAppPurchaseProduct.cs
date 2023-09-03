using System;

public class CInAppPurchaseProduct
{
	private string m_Param;

	private string m_Description;

	private string m_Title;

	private string m_ProductIdentifier;

	private decimal m_fPrice;

	private string m_szPrice;

	private string m_CurrencySymbol;

	private string m_LocaleIdentifier;

	public CInAppPurchaseProduct(string param)
	{
		if (CStringUtils.IsIAPXmlFormat(param))
		{
			ExtractData(param);
		}
		else
		{
			m_ProductIdentifier = param;
		}
	}

	public CInAppPurchaseProduct(string title, string description, string identifier, decimal price, string priceString, string currency, string locale)
	{
		m_Title = title;
		m_Description = description;
		m_ProductIdentifier = identifier;
		m_fPrice = price;
		m_szPrice = priceString;
		m_CurrencySymbol = currency;
		m_LocaleIdentifier = locale;
	}

	private CInAppPurchaseProduct()
	{
	}

	public string GetDescription()
	{
		return m_Description;
	}

	public string GetTitle()
	{
		return m_Title;
	}

	public string GetProductIdentifier()
	{
		return m_ProductIdentifier;
	}

	public decimal GetPrice()
	{
		return m_fPrice;
	}

	public string GetPriceAsString()
	{
		return m_szPrice;
	}

	public string GetCurrencySymbol()
	{
		return m_CurrencySymbol;
	}

	public string GetLocaleIdentifier()
	{
		return m_LocaleIdentifier;
	}

	private void ExtractData(string param)
	{
		m_Param = param;
		m_Description = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "Description");
		m_Title = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "Title");
		m_ProductIdentifier = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "ProductIdentifier");
		string text = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "Price");
		m_fPrice = 0m;
		if (text != string.Empty)
		{
			m_fPrice = Convert.ToDecimal(text);
		}
		m_szPrice = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "PriceFormatted");
		m_CurrencySymbol = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "CurrencySymbol");
		m_LocaleIdentifier = CStringUtils.ExtractFirstValueFromStringForKey(m_Param, "LocaleIdentifier");
	}
}
