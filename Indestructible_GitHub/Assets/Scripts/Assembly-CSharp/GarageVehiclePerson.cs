using Glu.Localization;
using UnityEngine;

public class GarageVehiclePerson : MonoBehaviour
{
	public UIBorderSprite PersonBackground;

	public UISingleSprite PersonSprite;

	public UISingleSprite PersonIcon;

	public SpriteText PersonName;

	private string _cachedName;

	private void Start()
	{
		_cachedName = string.Empty;
		UpdateData();
	}

	private void Update()
	{
		UpdateData();
	}

	private void UpdateData()
	{
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		if (selectedVehicle == null)
		{
			return;
		}
		ShopItemBody body = selectedVehicle.Body;
		if (body.nameId != _cachedName)
		{
			_cachedName = body.nameId;
			PersonName.Text = Strings.GetString(_cachedName);
			if (PersonBackground != null)
			{
				PersonBackground.SetInternalWidth(PersonName.TotalWidth);
			}
			SimpleSpriteUtils.UnloadTexture(PersonSprite);
			SimpleSpriteUtils.UnloadTexture(PersonIcon);
			SimpleSpriteUtils.ChangeTexture(PersonSprite, body.PersonSprite);
			SimpleSpriteUtils.ChangeTexture(PersonIcon, body.PersonIcon);
		}
	}
}
