using UnityEngine;

public class SelectModePanelSingle : PanelManagerPanel
{
	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	private void ActivateSelectMap(string mode)
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMode = mode;
		selectManager.ActivatePanel("SelectMapPanelSingle");
	}

	private void OnPracticeButtonTap()
	{
		ActivateSelectMap("practice");
	}

	private void OnStuntButtonTap()
	{
	}

	private void OnSurvivalButtonTap()
	{
		ActivateSelectMap("survival");
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}
}
