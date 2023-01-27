using UnityEngine;

public class CustomizePowerMeter : MonoBehaviour
{
	public InclinedProgressBar Meter;

	public SpriteText MeterText;

	public void UpdateValue()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		float num = selectedVehicle.GetTotalPower();
		float num2 = selectedVehicle.GetPower();
		if (num == 0f)
		{
			num = 1f;
			num2 = 1f;
		}
		MeterText.Text = string.Format("{0:0}/{1:0}", num2, num);
		if (Meter != null)
		{
			Meter.Position = num2 / num;
		}
	}
}
