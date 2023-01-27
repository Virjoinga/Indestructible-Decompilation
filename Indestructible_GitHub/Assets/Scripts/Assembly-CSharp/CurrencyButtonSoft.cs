using UnityEngine;

public class CurrencyButtonSoft : MonoBehaviour
{
	private int _moneySoftCached = -1;

	public SpriteText SoftCurrencyLabel;

	private void OnCurrencyButtonSoftTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if ((int)MonoSingleton<Player>.Instance.Level > 1)
		{
			GamePlayHaven.Placement("bank_launch");
		}
		Dialogs.IAPShop("iaps_soft", true);
	}

	private void Update()
	{
		int num = MonoSingleton<Player>.Instance.MoneySoft;
		if (_moneySoftCached != num)
		{
			SoftCurrencyLabel.Text = NumberFormat.Get(num);
			_moneySoftCached = num;
		}
	}
}
