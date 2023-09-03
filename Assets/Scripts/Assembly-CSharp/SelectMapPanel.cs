using Glu.Localization;
using UnityEngine;

public class SelectMapPanel : PanelManagerPanel
{
	public GameObject ObjectCTFLock;

	public GameObject ObjectCRSLock;

	public GameObject ObjectKOHLock;

	public UIButton ButtonCTF;

	public UIButton ButtonCRS;

	public UIButton ButtonKOH;

	public SpriteText ServerRegion;

	private RegionServer.Kind _kindCached = RegionServer.Kind.Undefined;

	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	private void OnServerButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Dialogs.SelectServer();
	}

	public void UpdateServerButton()
	{
		RegionServer.Kind serverRegion = MonoSingleton<SettingsController>.Instance.ServerRegion;
		if (serverRegion != _kindCached)
		{
			_kindCached = serverRegion;
			RegionServer.Info serverInfo = RegionServer.GetServerInfo(serverRegion);
			if (serverInfo != null)
			{
				string @string = Strings.GetString("IDS_SELECT_SERVER_REGION_BUTTON_1");
				ServerRegion.Text = string.Format(@string, Strings.GetString(serverInfo.ShortNameId));
			}
		}
	}

	private void Update()
	{
		UpdateServerButton();
		if (Input.GetKeyUp(KeyCode.Escape) && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		bool flag = (int)MonoSingleton<Player>.Instance.Level < 3;
		bool flag2 = (int)MonoSingleton<Player>.Instance.Level < 6;
		bool flag3 = (int)MonoSingleton<Player>.Instance.Level < 9;
		if (flag)
		{
			ButtonCTF.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		}
		if (flag2)
		{
			ButtonCRS.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		}
		if (flag3)
		{
			ButtonKOH.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		}
		MonoUtils.SetActive(ObjectCTFLock, flag);
		MonoUtils.SetActive(ObjectCRSLock, flag2);
		MonoUtils.SetActive(ObjectKOHLock, flag3);
		UpdateServerButton();
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

	private void OnTDMButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedMap = "koh_iceberg";
		selectManager.SelectedGame = "TeamDeathmatchConf";
		selectManager.StartGame();
	}
}
