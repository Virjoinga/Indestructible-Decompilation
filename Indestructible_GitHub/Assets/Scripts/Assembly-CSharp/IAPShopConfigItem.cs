using System.Xml.Serialization;

[XmlType("IAPShopConfigItem")]
public abstract class IAPShopConfigItem : ShopConfigItem
{
	public enum State
	{
		Undefined = 0,
		CheckingAvailability = 1,
		AvailableForPurchase = 2,
		UnavailableForPurchase = 3,
		WaitingForRetrival = 4
	}

	[XmlAttribute("productId")]
	public string productId;

	[XmlAttribute("iTunesHash")]
	public string iTunesHash;

	[XmlAttribute("defaultPrice")]
	public decimal? defaultPrice;

	[XmlAttribute("defaultCurrencySymbol")]
	public string defaultCurrencySymbol;

	[XmlAttribute("defaultIsOnSale")]
	public bool? defaultIsOnSale;

	private State _state;

	public State state
	{
		get
		{
			if (_state == State.WaitingForRetrival && ICInAppPurchase.GetInstance().GetPurchaseTransactionStatus() == ICInAppPurchase.TRANSACTION_STATE.ACTIVE)
			{
				return State.WaitingForRetrival;
			}
			if (_state != State.AvailableForPurchase)
			{
				if (ICInAppPurchase.GetInstance().IsAvailable())
				{
					CInAppPurchaseProduct[] availableProducts = ICInAppPurchase.GetInstance().GetAvailableProducts();
					if (availableProducts != null)
					{
						CInAppPurchaseProduct[] array = availableProducts;
						foreach (CInAppPurchaseProduct cInAppPurchaseProduct in array)
						{
							if (cInAppPurchaseProduct.GetProductIdentifier() == productId)
							{
								_state = State.AvailableForPurchase;
								break;
							}
						}
						if (_state != State.AvailableForPurchase)
						{
							_state = State.UnavailableForPurchase;
						}
					}
					else
					{
						_state = State.CheckingAvailability;
					}
				}
				else
				{
					_state = State.UnavailableForPurchase;
				}
			}
			return _state;
		}
	}

	public override bool readyToShow
	{
		get
		{
			return state != 0 && state != State.CheckingAvailability && state != State.UnavailableForPurchase;
		}
	}

	public IAPShopConfigItem()
	{
		productId = string.Empty;
	}

	public override void Override(ShopConfigItem dest)
	{
		base.Override(dest);
		IAPShopConfigItem iAPShopConfigItem = dest as IAPShopConfigItem;
		if (!string.IsNullOrEmpty(productId))
		{
			iAPShopConfigItem.productId = string.Copy(productId);
		}
		if (!string.IsNullOrEmpty(iTunesHash))
		{
			iAPShopConfigItem.iTunesHash = string.Copy(iTunesHash);
		}
		if (defaultPrice.HasValue)
		{
			iAPShopConfigItem.defaultPrice = defaultPrice.Value;
		}
		if (!string.IsNullOrEmpty(defaultCurrencySymbol))
		{
			iAPShopConfigItem.defaultCurrencySymbol = string.Copy(defaultCurrencySymbol);
		}
		if (defaultIsOnSale.HasValue)
		{
			iAPShopConfigItem.defaultIsOnSale = defaultIsOnSale.Value;
		}
	}

	private CInAppPurchaseProduct FindInAppPurchaseProduct()
	{
		CInAppPurchaseProduct[] availableProducts = ICInAppPurchase.GetInstance().GetAvailableProducts();
		if (availableProducts != null)
		{
			CInAppPurchaseProduct[] array = availableProducts;
			foreach (CInAppPurchaseProduct cInAppPurchaseProduct in array)
			{
				if (cInAppPurchaseProduct.GetProductIdentifier() == productId)
				{
					return cInAppPurchaseProduct;
				}
			}
		}
		return null;
	}

	public string GetCurrencySymbol()
	{
		CInAppPurchaseProduct cInAppPurchaseProduct = FindInAppPurchaseProduct();
		if (cInAppPurchaseProduct != null && cInAppPurchaseProduct.GetPriceAsString() != null && cInAppPurchaseProduct.GetPriceAsString().Length > 0)
		{
			return cInAppPurchaseProduct.GetCurrencySymbol();
		}
		return defaultCurrencySymbol;
	}

	public decimal GetPrice()
	{
		CInAppPurchaseProduct cInAppPurchaseProduct = FindInAppPurchaseProduct();
		if (cInAppPurchaseProduct != null && cInAppPurchaseProduct.GetPriceAsString() != null && cInAppPurchaseProduct.GetPriceAsString().Length > 0)
		{
			return cInAppPurchaseProduct.GetPrice();
		}
		return (!defaultPrice.HasValue) ? 0m : defaultPrice.Value;
	}

	public bool IsOnSale()
	{
		CInAppPurchaseProduct cInAppPurchaseProduct = FindInAppPurchaseProduct();
		if (cInAppPurchaseProduct != null && cInAppPurchaseProduct.GetCurrencySymbol() != null && cInAppPurchaseProduct.GetCurrencySymbol().Length > 0 && defaultCurrencySymbol.Equals(cInAppPurchaseProduct.GetCurrencySymbol()))
		{
			decimal? num = defaultPrice;
			if (num.HasValue && cInAppPurchaseProduct.GetPrice() < num.Value)
			{
				return true;
			}
		}
		return defaultIsOnSale.HasValue && defaultIsOnSale.Value;
	}

	public string GetPriceAsString()
	{
		CInAppPurchaseProduct cInAppPurchaseProduct = FindInAppPurchaseProduct();
		if (cInAppPurchaseProduct != null && cInAppPurchaseProduct.GetPriceAsString() != null && cInAppPurchaseProduct.GetPriceAsString().Length > 0)
		{
			return cInAppPurchaseProduct.GetPriceAsString();
		}
		if (cInAppPurchaseProduct != null && cInAppPurchaseProduct.GetPrice() > 0m && cInAppPurchaseProduct.GetCurrencySymbol() != null)
		{
			return cInAppPurchaseProduct.GetCurrencySymbol() + cInAppPurchaseProduct.GetPrice();
		}
		return defaultCurrencySymbol + defaultPrice;
	}

	public virtual void Buy()
	{
		if (ICInAppPurchase.GetInstance().IsAvailable() && ICInAppPurchase.GetInstance().GetAvailableProducts() != null)
		{
			ICInAppPurchase.TRANSACTION_STATE purchaseTransactionStatus = ICInAppPurchase.GetInstance().GetPurchaseTransactionStatus();
			if (purchaseTransactionStatus != ICInAppPurchase.TRANSACTION_STATE.ACTIVE)
			{
				ICInAppPurchase.GetInstance().BuyProduct(productId);
				_state = State.WaitingForRetrival;
			}
		}
	}

	public abstract void Deliver();
}
