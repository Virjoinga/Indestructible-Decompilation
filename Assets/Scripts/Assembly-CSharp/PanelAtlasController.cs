public class PanelAtlasController : PanelManagerPanel
{
	public string MaterialName;

	public override void OnActivate()
	{
		base.OnActivate();
		if (!string.IsNullOrEmpty(MaterialName))
		{
			SpriteAtlasUtils.LoadMaterial(MaterialName);
		}
	}

	public override void OnDeactivate()
	{
		base.OnActivate();
		GarageManager garageManager = Owner as GarageManager;
		bool flag = garageManager != null && garageManager.GarageLeft;
		if (!string.IsNullOrEmpty(MaterialName) && !flag)
		{
			SpriteAtlasUtils.UnloadMaterial(MaterialName);
		}
	}

	private void OnDestroy()
	{
		if (!string.IsNullOrEmpty(MaterialName))
		{
			SpriteAtlasUtils.UnloadMaterial(MaterialName);
		}
	}
}
