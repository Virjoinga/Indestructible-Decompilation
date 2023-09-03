public class PanelBundle : PanelAtlasController
{
	public DialogBundleContent Dialog;

	public void OnBackButtonTap()
	{
	}

	public override void OnActivate()
	{
		base.OnActivate();
		GarageManager garageManager = Owner as GarageManager;
		MonoUtils.SetActive(garageManager.GarageObjects, false);
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		GarageManager garageManager = Owner as GarageManager;
		MonoUtils.SetActive(garageManager.GarageObjects, true);
		garageManager.UpdateVehicle();
	}

	public void SetData(ShopItemBundle bundle, bool checkPlayer, bool bossBundle)
	{
		Dialog.SetData(bundle, checkPlayer, bossBundle);
	}
}
