using Glu.Localization;
using UnityEngine;

public class CustomizeSlotInformation : MonoBehaviour
{
	public CustomizeSlot Slot;

	public SpriteText ItemName;

	public SpriteText ItemDescription;

	public SpriteText ItemCount;

	public UISingleSprite ItemSprite;

	public CustomizeSlotParameters ItemParameters;

	public Animation OpenCloseAnimation;

	private AnimationState _animation;

	private AnimationState _animationOpen;

	private AnimationState _animationClose;

	private bool _animating;

	private bool _opened = true;

	private void Awake()
	{
		_animationClose = OpenCloseAnimation["CustomizeSlotInformationClose"];
		_animationOpen = OpenCloseAnimation["CustomizeSlotInformationOpen"];
	}

	public void SetData(CustomizeSlot slot, BuffModifyInfo baseInfo, BuffModifyInfo info)
	{
		Slot = slot;
		if (slot != null && slot.Item != null)
		{
			ItemName.Text = Strings.GetString(slot.Item.nameId);
			SimpleSpriteUtils.ChangeTexture(ItemSprite, slot.Item.ItemSprite);
			ItemParameters.SetData(slot.Item, baseInfo, info);
			if (slot.Item.ItemType == ShopItemType.Armor)
			{
				ShopItemArmor shopItemArmor = slot.Item as ShopItemArmor;
				string @string = Strings.GetString(slot.Item.DescriptionId);
				ItemDescription.Text = string.Format(@string, shopItemArmor.Damage);
				MonoUtils.SetActive(ItemDescription, true);
			}
			else if (slot.Item.ItemType == ShopItemType.Component || slot.Item.ItemType == ShopItemType.Ammunition)
			{
				ItemDescription.Text = Strings.GetString(slot.Item.DescriptionId);
				MonoUtils.SetActive(ItemDescription, true);
			}
			else
			{
				MonoUtils.SetActive(ItemDescription, false);
			}
			if ((bool)ItemCount)
			{
				if (slot.Item.GetCount() > 0 && slot.Item.GetPackCount() > 0)
				{
					MonoUtils.SetActive(ItemCount, true);
					ItemCount.Text = slot.Item.GetCountString();
				}
				else
				{
					MonoUtils.SetActive(ItemCount, false);
				}
			}
		}
		FinishAnimation();
	}

	private void FinishAnimation()
	{
		if (_animation != null)
		{
			_animation.enabled = true;
			_animation.normalizedTime = 1f;
			OpenCloseAnimation.Sample();
			_animation.enabled = false;
			_animation = null;
		}
	}

	private void OnOpenButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		_animation = _animationOpen;
		if (_opened)
		{
			_animation = _animationClose;
		}
		OpenCloseAnimation.Play(_animation.clip.name);
		_opened = !_opened;
	}

	private void Update()
	{
		_animating = false;
		if (_animation != null)
		{
			_animating = _animation.enabled;
			if (!_animating)
			{
				FinishAnimation();
			}
		}
	}

	private void OnSlotInformationTap()
	{
		if (!_animating)
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			Slot.Select();
		}
	}
}
