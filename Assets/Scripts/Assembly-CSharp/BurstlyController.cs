using UnityEngine;

public class BurstlyController : MonoBehaviour
{
	private static bool m_displayed;

	public static void HideAd()
	{
		GWalletHelper.HideAd();
		m_displayed = false;
	}

	private void ShowAd()
	{
	}

	private void OnEnable()
	{
		m_displayed = false;
	}

	private void OnDisable()
	{
		HideAd();
	}

	private void LateUpdate()
	{
		if ((int)MonoSingleton<Player>.Instance.Level > 1 && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			ShowAd();
		}
	}
}
