using UnityEngine;

public class CustomizeSlot : ButtonInputAdapter
{
	public ShopItem Item;

	public string SlotId;

	public UIBorderSprite Background;

	public SpriteText ItemLockText;

	public SpriteText ItemEquipText;

	public UISingleSprite ItemSprite;

	public SpriteText ItemCountText;

	public GameObject ObjectLockText;

	public ShopItemType SlotType;

	public int SlotIndex;

	protected bool _locked;

	protected bool _selected;

	protected PanelCustomization _panel;

	public void Select()
	{
		if (!_locked)
		{
			_panel.Select(this);
		}
	}

	protected virtual void OnItemButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.EquipmentSelected);
		Select();
	}

	protected override void Awake()
	{
		base.Awake();
		_buttonPush.whenToInvoke = POINTER_INFO.INPUT_EVENT.TAP;
		_buttonPush.scriptWithMethodToInvoke = this;
		_buttonPush.methodToInvoke = "OnItemButtonTap";
		Draggable = true;
	}

	public virtual bool IsSelected()
	{
		return _selected;
	}

	public virtual void SetSelected(bool selected)
	{
		if (!_selected && selected)
		{
			_selected = selected;
			Background.Play("Selected");
		}
		else if (_selected && !selected)
		{
			_selected = selected;
			Background.Play("Normal");
		}
	}

	protected virtual void Start()
	{
	}

	public virtual void SetData(PanelCustomization panel, ShopItem item)
	{
		Item = item;
		_panel = panel;
		if (item != null)
		{
			_locked = false;
			MonoUtils.SetActive(ItemSprite, true);
			MonoUtils.SetActive(ItemEquipText, false);
			MonoUtils.SetActive(ObjectLockText, false);
			if ((bool)ItemCountText && item.GetCount() > 0)
			{
				MonoUtils.SetActive(ItemCountText, true);
				ItemCountText.Text = item.GetCountString();
			}
			SimpleSpriteUtils.ChangeTexture(ItemSprite, item.ItemSprite);
			return;
		}
		ShopItemSlot itemSlot = MonoSingleton<ShopController>.Instance.GetItemSlot(SlotId);
		_locked = itemSlot.IsLocked();
		MonoUtils.SetActive(ObjectLockText, _locked);
		if (_locked)
		{
			ItemLockText.Text = itemSlot.LockText();
		}
		if ((bool)ItemCountText)
		{
			MonoUtils.SetActive(ItemCountText, false);
		}
		MonoUtils.SetActive(ItemSprite, false);
		MonoUtils.SetActive(ItemEquipText, !_locked);
	}

	public void SetWidth(float width)
	{
		float height = _buttonPush.height;
		_buttonPush.SetSize(width, height);
		Background.SetWidth(width);
	}
}
