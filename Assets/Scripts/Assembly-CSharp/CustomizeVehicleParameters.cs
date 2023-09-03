using UnityEngine;

public class CustomizeVehicleParameters : MonoBehaviour
{
	public SpriteText LabelDPS;

	public SpriteText LabelArmor;

	public SpriteText LabelSpeed;

	public SpriteText LabelEnergy;

	public Color BadColor = Color.red;

	public Color GoodColor = Color.green;

	public void UpdateData(BuffModifyInfo info)
	{
		LabelDPS.Text = NumberFormat.Get(info.Damage / info.FireInterval, 1);
		LabelArmor.Text = NumberFormat.Get(info.Health, 1);
		LabelSpeed.Text = NumberFormat.Get(info.Speed, 1);
		LabelEnergy.Text = NumberFormat.Get(info.Energy, 1);
	}

	public void UpdateData(BuffModifyInfo info, BuffModifyInfo comparedInfo)
	{
		float num = info.Damage / info.FireInterval;
		float num2 = comparedInfo.Damage / comparedInfo.FireInterval;
		LabelDPS.Text = NumberFormat.Get(num, 1);
		LabelDPS.Color = ((num == num2) ? Color.white : ((!(num < num2)) ? GoodColor : BadColor));
		LabelArmor.Text = NumberFormat.Get(info.Health, 1);
		LabelArmor.Color = ((info.Health == comparedInfo.Health) ? Color.white : ((!(info.Health < comparedInfo.Health)) ? GoodColor : BadColor));
		LabelSpeed.Text = NumberFormat.Get(info.Speed, 1);
		LabelSpeed.Color = ((info.Speed == comparedInfo.Speed) ? Color.white : ((!(info.Speed < comparedInfo.Speed)) ? GoodColor : BadColor));
		LabelEnergy.Text = NumberFormat.Get(info.Energy, 1);
		LabelEnergy.Color = ((info.Energy == comparedInfo.Energy) ? Color.white : ((!(info.Energy < comparedInfo.Energy)) ? GoodColor : BadColor));
	}
}
