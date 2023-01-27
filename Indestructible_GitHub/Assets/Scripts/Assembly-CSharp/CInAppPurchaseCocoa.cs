using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CInAppPurchaseCocoa : ICInAppPurchase
{
	public override void Init(string[] products, string marketPublicKey)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(products.GetLength(0));
		foreach (string key in products)
		{
			dictionary.Add(key, string.Empty);
		}
		Init(dictionary, marketPublicKey);
	}

	public override void Init(Dictionary<string, string> products, string marketPublicKey)
	{
		string[] array = new string[products.Keys.Count];
		string[] array2 = new string[products.Values.Count];
		products.Keys.CopyTo(array, 0);
		products.Values.CopyTo(array2, 0);
		InitializeIAP(array, array2);
	}

	public override bool IsTurnedOn()
	{
		return IsIAPEnabled();
	}

	public override bool IsAvailable()
	{
		return IsIAPInitialized();
	}

	public override CInAppPurchaseProduct[] GetAvailableProducts()
	{
		List<CInAppPurchaseProduct> list = new List<CInAppPurchaseProduct>();
		if (IsAvailable())
		{
			string text = Marshal.PtrToStringAnsi(GetValidatedProducts());
			if (text != null && text.Length > 0)
			{
				string[] array = text.Split('\n');
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					string[] array3 = text2.Split('\t');
					if (array3.Length == 7)
					{
						list.Add(new CInAppPurchaseProduct(array3[0], array3[1], array3[2], decimal.Parse(array3[3]), array3[4], array3[5], array3[6]));
					}
					else
					{
						Debug.Log("Incorrect product info parameters count: " + array3.Length);
					}
				}
			}
		}
		return (list.Count <= 0) ? null : list.ToArray();
	}

	public override void BuyProduct(string product)
	{
		if (IsAvailable())
		{
			BuyProductWithID(product);
		}
	}

	public override void RestoreCompletedTransactions()
	{
		if (IsAvailable())
		{
			RestoreTransactions();
		}
	}

	public override RESTORE_STATE GetRestoreStatus()
	{
		return (RESTORE_STATE)GetRestoreCompletedTransactionsStatus();
	}

	public override TRANSACTION_STATE GetPurchaseTransactionStatus()
	{
		return (TRANSACTION_STATE)GetLastKnownStatusOfLatestTransaction();
	}

	public override string RetrieveProduct()
	{
		string result = null;
		if (IsAvailable())
		{
			result = Marshal.PtrToStringAnsi(RetrievePurchasedProduct(0, false));
		}
		return result;
	}

	public override string GetRetrievalQueueItem(int index)
	{
		string result = null;
		if (IsAvailable())
		{
			result = Marshal.PtrToStringAnsi(RetrievePurchasedProduct(index, true));
		}
		return result;
	}

	public override int GetRetrievalQueueCount()
	{
		return GetPurchasedItemsCount();
	}

	public override void RetrievalQueueDispose(int numItems)
	{
		ClearPurchasedItems(numItems);
	}

	[DllImport("__Internal")]
	private static extern void InitializeIAP(string[] productIDs, string[] hashes);

	[DllImport("__Internal")]
	private static extern bool IsIAPInitialized();

	[DllImport("__Internal")]
	private static extern bool IsIAPEnabled();

	[DllImport("__Internal")]
	private static extern IntPtr GetValidatedProducts();

	[DllImport("__Internal")]
	private static extern void BuyProductWithID(string product);

	[DllImport("__Internal")]
	private static extern void RestoreTransactions();

	[DllImport("__Internal")]
	private static extern int GetRestoreCompletedTransactionsStatus();

	[DllImport("__Internal")]
	private static extern int GetLastKnownStatusOfLatestTransaction();

	[DllImport("__Internal")]
	private static extern IntPtr RetrievePurchasedProduct(int occurence, bool keepProduct);

	[DllImport("__Internal")]
	private static extern int GetPurchasedItemsCount();

	[DllImport("__Internal")]
	private static extern void ClearPurchasedItems(int count);
}
