public class SelectCustomMatchPanel : PanelManagerPanel
{
	private void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	private void OnTwoPlayersButtonTap()
	{
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedCustomMatchPlayersMin = 2;
		selectManager.SelectedCustomMatchPlayersMax = 2;
		selectManager.StartGame();
	}

	private void OnFourPlayersButtonTap()
	{
		SelectManager selectManager = Owner as SelectManager;
		selectManager.SelectedCustomMatchPlayersMin = 4;
		selectManager.SelectedCustomMatchPlayersMax = 4;
		selectManager.StartGame();
	}
}
