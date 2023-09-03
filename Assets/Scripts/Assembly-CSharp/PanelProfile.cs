using System.Collections;
using UnityEngine;

public class PanelProfile : PanelAtlasController
{
	public ProfilePlayerInformation Information;

	public GameObject ObjectMenuButton;

	private void Awake()
	{
		Object.Destroy(ObjectMenuButton);
	}

	public void OnGarageButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GarageManager garageManager = Owner as GarageManager;
		garageManager.ActivatePanel("PanelGarage");
	}

	public void OnMenuButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GarageManager garageManager = Owner as GarageManager;
		garageManager.LoadLevel("MenuScene");
	}

	private void OnSettingsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Dialogs.SettingsDialog();
	}

	private void OnAchievementsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
	}

	private void OnLeaderboardsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
	}

	private void OnSkillsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePanel("PanelTalents");
	}

	private void OnHelpButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Dialogs.HelpDialog();
	}

	private void OnTermsButtonTap()
	{
		Application.OpenURL("http://www.glu.com/legal");
	}

	private void OnMoreGamesButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GamePlayHaven.Placement("more_games", true);
	}

	private void OnFacebookButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (ASocial.Facebook.IsLoggedIn())
		{
			GameAnalytics.EventFacebookLoggedOut();
			ASocial.Facebook.Logout();
			return;
		}
		string[] permissions = new string[1] { string.Empty };
		GameAnalytics.EventFacebookButtonTap("PanelProfile");
		ASocial.Facebook.Login(permissions);
	}

	private IEnumerator FacebookButton()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			Information.UpdateFacebookButton(false);
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		Information.UpdateData();
		StartCoroutine(FacebookButton());
		GameObject gameObject = GameObject.Find("AchievementsButton");
		if ((bool)gameObject)
		{
			gameObject.SetActiveRecursively(false);
		}
		GameObject gameObject2 = GameObject.Find("LeaderboardsButton");
		if ((bool)gameObject2)
		{
			gameObject2.SetActiveRecursively(false);
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnGarageButtonTap();
		}
	}
}
