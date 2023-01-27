public class PanelAmmunition : PanelAtlasController
{
	public LoadoutInformation LoadInformation;

	public AmmunitionInformation AmmoInformation;

	public void OnGarageButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GarageManager garageManager = Owner as GarageManager;
		garageManager.ActivatePanel("PanelGarage");
	}

	public void OnBackButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Owner.ActivatePreviousPanel();
	}

	public void OnPlayButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		MonoSingleton<Player>.Instance.Tutorial.SetPlayButtonTap();
		GameAnalytics.EventPlayButtonTap(base.Name);
		GarageManager manager = Owner as GarageManager;
		PanelGarage.StartSelectScene(manager);
	}

	public void OnAmmunitionInformationTap()
	{
		OpenShopAmmunitions();
	}

	public void OnUnequippButtonTap()
	{
		MonoSingleton<Player>.Instance.SelectedVehicle.Ammunition = null;
		AmmoInformation.UpdateData();
	}

	public override void OnActivate()
	{
		base.OnActivate();
		LoadInformation.UpdateData();
		AmmoInformation.UpdateData();
	}

	public void OpenShopAmmunitions()
	{
		GarageManager garageManager = Owner as GarageManager;
		PanelManagerPanel panelManagerPanel = garageManager.ActivatePanel("PanelShop");
		PanelShop panelShop = panelManagerPanel as PanelShop;
		panelShop.ScrollList.FeedAmmunitions(string.Empty);
	}

	private void Update()
	{
		if (MonoSingleton<GameController>.Instance.BackKeyReleased() && MonoSingleton<DialogsQueue>.Instance.IsEmpty())
		{
			OnBackButtonTap();
		}
	}

	private void OnMoreGamesButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GamePlayHaven.Placement("more_games", true);
	}
}
