using UnityEngine;

public class ShopItemGradeLevel : MonoBehaviour
{
	public UIBorderSprite Background;

	public SpriteText GradeLabel;

	public SpriteText GradeLevelText;

	public SpriteText GradeLevelChangeText;

	public PackedSprite GradeLevelChangeIcon;

	public Transform GradeGroup;

	private void Start()
	{
		float totalWidth = GradeLabel.TotalWidth;
		Background.SetInternalWidth(totalWidth);
	}

	public void SetData(ShopItem item)
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		int gradeLevel = selectedVehicle.Vehicle.GetGradeLevel();
		int gradeLevel2 = item.GetGradeLevel();
		GradeLevelText.Text = gradeLevel2.ToString();
		int num = gradeLevel2 - gradeLevel;
		if (num >= 0)
		{
			GradeLevelChangeText.Color = Color.green;
			GradeLevelChangeIcon.PlayAnim("ChangeUp");
		}
		else
		{
			GradeLevelChangeText.Color = Color.red;
			GradeLevelChangeIcon.PlayAnim("ChangeDown");
		}
		num = Mathf.Abs(num);
		GradeLevelChangeText.Text = num.ToString();
		float num2 = GradeLevelChangeText.TotalWidth + 3.75f;
		GradeGroup.localPosition = new Vector3(0f - num2, 0f, 0f);
	}
}
