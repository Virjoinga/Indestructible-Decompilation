using Glu.Localization;
using UnityEngine;

public class DialogShopWeaponDetails : UIDialog
{
	public UISingleSprite WeaponSprite;

	public UIBorderSprite WeaponNameBackground;

	public SpriteText WeaponName;

	public SpriteText WeaponText;

	public ShopItemBuyButton BuyButton;

	public DialogShopWeaponParameters Parameters;

	private ShopItemWeapon _weapon;

	private ShopItemStandard _item;

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void OnBuyButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (_item.IsBought())
		{
			_item.Equip();
			Close();
		}
		else if (_item.Buy(BuyButton.Currency))
		{
			Close();
		}
	}

	private void UpdateData()
	{
		ShopItemWeapon shopItemWeapon = null;
		if (_item != null)
		{
			shopItemWeapon = _item.Item as ShopItemWeapon;
		}
		else if (_weapon != null)
		{
			shopItemWeapon = _weapon;
		}
		SimpleSpriteUtils.ChangeTexture(WeaponSprite, shopItemWeapon.ItemSprite);
		string @string = Strings.GetString(shopItemWeapon.nameId);
		WeaponName.Text = @string.Replace('\n', ' ');
		WeaponNameBackground.SetInternalWidth(WeaponName.TotalWidth);
		WeaponText.Text = Strings.GetString(shopItemWeapon.DescriptionId);
		Parameters.SetData(shopItemWeapon);
		BuyButton.UpdateState(shopItemWeapon, ShopItemBuyButton.ForceUpdate.None);
		if (_item == null)
		{
			MonoUtils.DetachAndDestroy(BuyButton);
		}
	}

	public void SetData(ShopItemWeapon weapon, ShopItemStandard item)
	{
		_weapon = weapon;
		_item = item;
	}

	public override void Activate()
	{
		base.Activate();
		UpdateData();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
