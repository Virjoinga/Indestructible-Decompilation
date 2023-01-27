using UnityEngine;

public class CurrencyButtonHard : MonoBehaviour
{
	private int _moneyHardCached = -1;

	public SpriteText HardCurrencyLabel;

	private void OnCurrencyButtonHardTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if ((int)MonoSingleton<Player>.Instance.Level > 1)
		{
			GamePlayHaven.Placement("bank_launch");
		}
		Dialogs.IAPShop("iaps_hard", true);
	}

	private void Update()
	{
		int num = MonoSingleton<Player>.Instance.MoneyHard;
		if (_moneyHardCached != num)
		{
			HardCurrencyLabel.Text = NumberFormat.Get(num);
			_moneyHardCached = num;
		}
	}
}
