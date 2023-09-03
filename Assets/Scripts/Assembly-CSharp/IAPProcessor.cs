using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAPProcessor : MonoBehaviour
{
	private const float _UPDATE_PERIOD = 1f;

	private IShopConfigProvider _shopConfigProvider;

	public bool isProductRetrivalRunning { get; private set; }

	public void Init(IShopConfigProvider shopConfigProvider, string androidMarketKey)
	{
		_shopConfigProvider = shopConfigProvider;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (ShopConfigItem item in _shopConfigProvider.shopConfig.items)
		{
			IAPShopConfigItem iAPShopConfigItem = item as IAPShopConfigItem;
			if (iAPShopConfigItem != null)
			{
				dictionary.Add(iAPShopConfigItem.productId, (iAPShopConfigItem.iTunesHash == null) ? string.Empty : iAPShopConfigItem.iTunesHash);
			}
		}
		ICInAppPurchase.GetInstance().Init(dictionary, androidMarketKey);
	}

	private void Start()
	{
		base.enabled = false;
		base.useGUILayout = false;
		ResumeProductRetrival();
	}

	private void OnDestroy()
	{
		StopProductRetrival();
	}

	public void ResumeProductRetrival()
	{
		if (!isProductRetrivalRunning)
		{
			StartCoroutine("ProductRetrivalRoutine");
			isProductRetrivalRunning = true;
		}
	}

	public void StopProductRetrival()
	{
		StopCoroutine("ProductRetrivalRoutine");
		isProductRetrivalRunning = false;
	}

	private IEnumerator ProductRetrivalRoutine()
	{
		while (true)
		{
			string productId = ICInAppPurchase.GetInstance().RetrieveProduct();
			if (productId != null)
			{
				IAPShopConfigItem iapShopConfigItem = FindIAPShopConfigItem(_shopConfigProvider.shopConfig, productId);
				if (iapShopConfigItem != null)
				{
					DeliverItem(iapShopConfigItem);
				}
			}
			float t = Time.realtimeSinceStartup + 1f;
			while (t > Time.realtimeSinceStartup)
			{
				yield return new WaitForEndOfFrame();
			}
		}
	}

	private static IAPShopConfigItem FindIAPShopConfigItem(ShopConfig shopConfig, string productId)
	{
		foreach (ShopConfigItem item in shopConfig.items)
		{
			IAPShopConfigItem iAPShopConfigItem = item as IAPShopConfigItem;
			if (iAPShopConfigItem != null && iAPShopConfigItem.productId.Equals(productId))
			{
				return iAPShopConfigItem;
			}
		}
		return null;
	}

	protected virtual void DeliverItem(IAPShopConfigItem iapShopConfigItem)
	{
		iapShopConfigItem.Deliver();
	}
}
