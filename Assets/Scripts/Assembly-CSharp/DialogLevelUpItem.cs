using UnityEngine;

public class DialogLevelUpItem : MonoBehaviour
{
	public ShopItem Item;

	public UISingleSprite ItemSprite;

	private DialogLevelUp _dialog;

	private void OnButtonTap()
	{
		_dialog.OnItemButtonTap(this);
	}

	public void SetData(DialogLevelUp dialog, ShopItem item)
	{
		Item = item;
		_dialog = dialog;
		if (item.ItemType == ShopItemType.Vehicle)
		{
			ShopItemVehicle shopItemVehicle = item as ShopItemVehicle;
			ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(shopItemVehicle.BodyId);
			SimpleSpriteUtils.ChangeTexture(ItemSprite, itemBody.ItemSprite);
		}
		else
		{
			SimpleSpriteUtils.ChangeTexture(ItemSprite, item.ItemSprite);
		}
	}

	public void Release()
	{
		SimpleSpriteUtils.UnloadTexture(ItemSprite);
	}
}
