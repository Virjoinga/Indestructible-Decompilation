using System;
using Glu.Localization;
using UnityEngine;

public class GarageItemStandard : ButtonInputAdapter
{
	public int Index;

	public SpriteText ItemText;

	public UISingleSprite ItemSprite;

	public GarageVehicle Item;

	public PackedSprite ItemFuelIcon;

	public InclinedProgressBarSimple ItemFuelMeter;

	public InclinedProgressBar ItemGallonMeter;

	public SpriteText ItemGallonTimer;

	protected int _time;

	protected bool _frozen;

	protected bool _selected;

	protected GarageScrollList _scrollList;

	protected virtual void OnItemButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.VehicleSelected);
		_scrollList.Select(this, true);
	}

	protected override void Awake()
	{
		base.Awake();
		_buttonState.whenToInvoke = POINTER_INFO.INPUT_EVENT.TAP;
		_buttonState.scriptWithMethodToInvoke = this;
		_buttonState.methodToInvoke = "OnItemButtonTap";
	}

	public virtual bool IsSelected()
	{
		return _selected;
	}

	public virtual void SetSelected(bool selected)
	{
		if (!_selected && selected)
		{
			SetState("Selected");
			_selected = selected;
		}
		else if (_selected && !selected)
		{
			SetState("Normal");
			_selected = selected;
		}
	}

	protected virtual void Start()
	{
		ItemText.Text = Strings.GetString(Item.Body.nameId);
		SimpleSpriteUtils.ChangeTexture(ItemSprite, Item.Body.GarageSprite);
	}

	public virtual void SetData(GarageScrollList scrollList, GarageVehicle item, int index)
	{
		Item = item;
		Index = index;
		_scrollList = scrollList;
		_time = -1;
		Update();
	}

	private void UpdateFreeze()
	{
		bool flag = Item.Fuel.IsFrozen();
		if (flag && !_frozen)
		{
			_frozen = true;
			ItemGallonMeter.Play("Yellow");
			ItemFuelMeter.Play("Yellow");
			ItemFuelIcon.PlayAnim("Yellow");
		}
		else if (!flag && _frozen)
		{
			_frozen = false;
			ItemGallonMeter.Play("White");
			ItemFuelMeter.Play("White");
			ItemFuelIcon.PlayAnim("White");
		}
	}

	private void Update()
	{
		Item.Fuel.Update();
		float seconds = 0f;
		float levelPosition = Item.Fuel.GetLevelPosition(ref seconds);
		int num = Mathf.CeilToInt(seconds);
		if (_time == num)
		{
			return;
		}
		_time = num;
		UpdateFreeze();
		string text = string.Empty;
		if (Item.Fuel.FreezeForever)
		{
			text = Strings.GetString("IDS_REFUEL_VEHICLE_PERMANENT_LABEL");
		}
		else if (_time > 0)
		{
			TimeSpan timeSpan = new TimeSpan(0, 0, _time);
			int num2 = Mathf.FloorToInt((float)timeSpan.TotalHours);
			if (num2 > 0)
			{
				text = string.Format("{0:00}:", num2);
			}
			text += string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
		}
		ItemGallonTimer.Text = text;
		ItemGallonMeter.Position = levelPosition;
		ItemFuelMeter.Position = Item.Fuel.GetLevelRelative();
	}
}
