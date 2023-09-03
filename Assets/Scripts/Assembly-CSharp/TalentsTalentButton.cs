using UnityEngine;

public class TalentsTalentButton : MonoBehaviour
{
	public ShopItemTalent Item;

	public PackedSprite TalentIcon;

	public PackedSprite TalentLock;

	public InclinedProgressBarSimple MeterFull;

	private TalentsTalents _talents;

	private UIStateToggleBtn _button;

	protected virtual void ButtonInputDelegate(ref POINTER_INFO ptr)
	{
		POINTER_INFO.INPUT_EVENT evt = ptr.evt;
		if (evt == POINTER_INFO.INPUT_EVENT.TAP)
		{
			_talents.OnTalentButtonTap(this);
			ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
		}
	}

	public void SetData(TalentsTalents talents, ShopItemTalent item)
	{
		_talents = talents;
		Item = item;
		_button = GetComponent<UIStateToggleBtn>();
		_button.SetInputDelegate(ButtonInputDelegate);
	}

	public bool IsUnlocked()
	{
		int talentPointsSpent = MonoSingleton<Player>.Instance.GetTalentPointsSpent();
		PlayerTalent playerTalent = MonoSingleton<Player>.Instance.FindBoughtTalent(Item.id);
		int? unlockPoints = Item.UnlockPoints;
		bool flag = (unlockPoints.HasValue && talentPointsSpent >= unlockPoints.Value) || playerTalent != null;
		PlayerTalent playerTalent2 = MonoSingleton<Player>.Instance.FindBoughtTalent(Item.ParentId);
		bool flag2 = playerTalent2 != null || Item.ParentId == string.Empty;
		return flag && flag2;
	}

	public void UpdateData(bool selected)
	{
		bool flag = IsUnlocked();
		MonoUtils.SetActive(TalentLock, !flag);
		MonoUtils.SetActive(TalentIcon, flag);
		MonoUtils.SetActive(MeterFull, flag);
		int talentLevel = MonoSingleton<Player>.Instance.GetTalentLevel(Item.id);
		if (flag)
		{
			float position = (float)talentLevel / 3f;
			MeterFull.Position = position;
			if (talentLevel > 0)
			{
				TalentIcon.PlayAnim("Available");
				if (Item.LevelsCount == 1)
				{
					MeterFull.Position = 1f / 3f;
				}
			}
			else
			{
				TalentIcon.PlayAnim("Disabled");
				if (Item.LevelsCount == 1)
				{
					MeterFull.Position = 0f;
				}
			}
		}
		else
		{
			TalentIcon.PlayAnim("Disabled");
			if (Item.LevelsCount == 1)
			{
				MeterFull.Position = 0f;
			}
		}
		if (selected)
		{
			if (talentLevel == Item.LevelsCount)
			{
				_button.SetToggleState("All", true);
			}
			else
			{
				_button.SetToggleState("Selected", true);
			}
		}
		else if (talentLevel == Item.LevelsCount)
		{
			_button.SetToggleState("Complete", true);
		}
		else
		{
			_button.SetToggleState("Normal", true);
		}
	}

	public void SetSelected(bool selected)
	{
		bool flag = false;
		PlayerTalent playerTalent = MonoSingleton<Player>.Instance.FindBoughtTalent(Item.id);
		if (playerTalent != null)
		{
			flag = playerTalent.Level == Item.LevelsCount;
		}
		if (selected)
		{
			if (flag)
			{
				_button.SetToggleState("All", true);
			}
			else
			{
				_button.SetToggleState("Selected", true);
			}
		}
		else if (flag)
		{
			_button.SetToggleState("Complete", true);
		}
		else
		{
			_button.SetToggleState("Normal", true);
		}
	}
}
