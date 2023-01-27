using UnityEngine;

public class SelectMapPanelCustom : PanelManagerPanel
{
	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	public override void OnActivate()
	{
		base.OnActivate();
	}

	private void OnCTFButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMap = "ctf_aircrash";
		selectManager.SelectedGame = "CTFConf";
		Owner.ActivatePanel("SelectCustomMatchPanel");
	}

	private void OnCRSButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMap = "dtb_rocketbase";
		selectManager.SelectedGame = "CRConf";
		Owner.ActivatePanel("SelectCustomMatchPanel");
	}

	private void OnKOHButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMap = "koh_iceberg";
		selectManager.SelectedGame = "DeathmatchConf";
		Owner.ActivatePanel("SelectCustomMatchPanel");
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}

	private void OnTDMButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMap = "koh_iceberg";
		selectManager.SelectedGame = "TeamDeathmatchConf";
		Owner.ActivatePanel("SelectCustomMatchPanel");
	}
}
