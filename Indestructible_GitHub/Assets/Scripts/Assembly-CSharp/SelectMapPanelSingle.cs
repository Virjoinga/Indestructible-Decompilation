using Glu.Localization;
using UnityEngine;

public class SelectMapPanelSingle : PanelManagerPanel
{
	public SpriteText Text;

	public SpriteText IcebergText;

	public SpriteText AircrashText;

	public SpriteText RocketBaseText;

	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	private void OnCTFButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMap = "ctf_aircrash";
		selectManager.SelectedGame = "CTFConf";
		selectManager.StartGame();
	}

	private void OnCRSButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMap = "dtb_rocketbase";
		selectManager.SelectedGame = "CRConf";
		selectManager.StartGame();
	}

	private void OnKOHButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMap = "koh_iceberg";
		selectManager.SelectedGame = "DeathmatchConf";
		selectManager.StartGame();
	}

	private void SetMapsLabels()
	{
		IcebergText.Text = Strings.GetString("IDS_SELECT_LABEL_ICEBERG");
		AircrashText.Text = Strings.GetString("IDS_SELECT_LABEL_AIRCRASH");
		RocketBaseText.Text = Strings.GetString("IDS_SELECT_LABEL_ROCKETBASE");
	}

	private void SetModesLabels()
	{
		IcebergText.Text = Strings.GetString("IDS_SELECT_LABEL_KOH");
		AircrashText.Text = Strings.GetString("IDS_SELECT_LABEL_CTF");
		RocketBaseText.Text = Strings.GetString("IDS_SELECT_LABEL_CRS");
	}

	public override void OnActivate()
	{
		base.OnActivate();
		SelectManager selectManager = Owner as SelectManager;
		if (selectManager.SelectedMode == "survival")
		{
			SetMapsLabels();
		}
		else if (selectManager.SelectedMode == "practice")
		{
			SetModesLabels();
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}
}
