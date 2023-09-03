using UnityEngine;

public class DummyOpenBundleDialog : UIDialog
{
	private string _bundleName = string.Empty;

	private bool _bossBundle = true;

	public void SetData(string bundleName, bool bossBundle)
	{
		_bundleName = bundleName;
		_bossBundle = bossBundle;
	}

	public override void Activate()
	{
		base.Activate();
		GarageManager garageManager = (GarageManager)Object.FindObjectOfType(typeof(GarageManager));
		if (garageManager != null)
		{
			garageManager.ForceOpenBundleOffer(_bundleName, _bossBundle);
		}
		Close();
	}
}
