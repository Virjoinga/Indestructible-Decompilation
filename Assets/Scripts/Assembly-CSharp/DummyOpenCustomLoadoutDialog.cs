using UnityEngine;

public class DummyOpenCustomLoadoutDialog : UIDialog
{
	public override void Activate()
	{
		base.Activate();
		GarageManager garageManager = (GarageManager)Object.FindObjectOfType(typeof(GarageManager));
		if (garageManager != null)
		{
			PanelManagerPanel panelManagerPanel = garageManager.ActivatePanel("PanelCustomization");
		}
		Close();
	}
}
