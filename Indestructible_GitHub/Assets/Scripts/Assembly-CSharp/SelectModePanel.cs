using UnityEngine;

public class SelectModePanel : PanelManagerPanel
{
	public GameObject ObjectSingleAndCustomGame;

	public GameObject ObjectSingleBigButtonGame;

	private bool isButtonReposition;

	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		MonoSingleton<Player>.Instance.StartLevel("GarageScene");
	}

	private void OnRankedMatchButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMode = "matchmaking";
		selectManager.SelectedType = "multiplayer";
		selectManager.ActivatePanel("SelectMapPanel");
	}

	private void OnCustomMatchButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMode = "custom";
		selectManager.SelectedType = "multiplayer";
		Owner.ActivatePanel("SelectMapPanelCustom");
	}

	private void OnSinglePlayerButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMode = "survival";
		selectManager.SelectedType = "single";
		selectManager.ActivatePanel("SelectMapPanelSingle");
	}

	private void OnBossFightButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMode = "boss";
		selectManager.SelectedType = "single";
		selectManager.ActivatePanel("CapaignLinePanel");
	}

	public override void OnActivate()
	{
		base.OnActivate();
		bool flag = MonoSingleton<GameController>.Instance.Configuration.CustomMatch.Enabled;
		MonoUtils.SetActive(ObjectSingleAndCustomGame, flag);
		MonoUtils.SetActive(ObjectSingleBigButtonGame, !flag);
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}
}
