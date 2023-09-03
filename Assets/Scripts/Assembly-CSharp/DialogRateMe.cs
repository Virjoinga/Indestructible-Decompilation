using Glu.Localization;
using UnityEngine;

public class DialogRateMe : UIDialog
{
	private void OnLaterButtonTap()
	{
		MonoSingleton<Player>.Instance.Statistics.RateMeLastOfferLevel = MonoSingleton<Player>.Instance.Level;
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void OnNeverButtonTap()
	{
		MonoSingleton<Player>.Instance.Statistics.RateMeLastOfferLevel = -1;
		MonoSingleton<Player>.Instance.Save();
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void OnRateButtonTap()
	{
		MonoSingleton<Player>.Instance.Statistics.RateMeLastOfferLevel = -1;
		MonoSingleton<Player>.Instance.Save();
		Application.OpenURL(AJavaTools.Internet.GetGameURL());
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
	}

	protected override void Start()
	{
		base.Start();
		Debug.Log("*** Step 0 ***");
		if (!(GameConstants.BuildType == "google"))
		{
			return;
		}
		Debug.Log("*** Step 1 ***");
		GameObject gameObject = GameObject.Find("MessageText");
		if (gameObject != null)
		{
			Debug.Log("*** Step 2 ***");
			SpriteText spriteText = gameObject.GetComponent("SpriteText") as SpriteText;
			if (spriteText != null)
			{
				spriteText.Text = Strings.GetString("IDS_RATEME_MESSAGE_ANDROID");
				Debug.Log("*** " + spriteText.Text);
			}
		}
	}

	private void OnApplicationPause(bool paused)
	{
		Debug.Log("*** RateMe OnApplicationPause ***");
		if (!paused && MonoSingleton<Player>.Instance.Statistics.RateMeLastOfferLevel == -1)
		{
			if (GameConstants.BuildType == "google")
			{
				MonoSingleton<Player>.Instance.AddMoneyHard(5, "CREDIT_IN_GAME_AWARD", "Incentivized Rate Me", "RateTheGame");
				Dialogs.PostRateGameAward();
			}
			Close();
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			Close();
		}
	}
}
