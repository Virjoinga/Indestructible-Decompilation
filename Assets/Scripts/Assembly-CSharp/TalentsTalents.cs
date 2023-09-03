using Glu.Localization;
using UnityEngine;

public class TalentsTalents : MonoBehaviour
{
	public TalentsTalentButton[] TalentsButtons;

	public TalentsTalentLine[] TalentsLines;

	public Transform[] Columns;

	public SpriteText TalentName;

	public SpriteText TalentText;

	public UIButton LearnButton;

	public UIButton ResetButton;

	public UIExpandSprite TalentDetailsBackground;

	public UIBorderSprite TalentNameBackground;

	public SpriteText TalentPoints;

	public GameObject TalentLevel;

	public GameObject TapSelectText;

	private TalentsTalentButton _selectedTalent;

	private void Awake()
	{
		CreateTalents();
		int num = Columns.Length;
		Vector2 screenSize = UITools.GetScreenSize();
		float num2 = screenSize.x / (float)(num + 1);
		float num3 = screenSize.x / 2f - num2;
		for (int i = 0; i < num; i++)
		{
			Transform transform = Columns[i];
			Vector3 position = transform.position;
			position.x = (float)i * num2 - num3;
			transform.position = position;
		}
		TalentDetailsBackground.SetWidth(screenSize.x - 6.71875f);
		TalentText.maxWidth = screenSize.x - 6.71875f - 85f;
		TalentsTalentLine[] talentsLines = TalentsLines;
		foreach (TalentsTalentLine talentsTalentLine in talentsLines)
		{
			talentsTalentLine.Initialize();
		}
		ShopItemPrice itemPrice = MonoSingleton<ShopController>.Instance.GetItemPrice("price_talents_reset");
		ResetButton.Text = itemPrice.GetPriceString(ShopItemCurrency.None);
	}

	public bool LearnTalent()
	{
		if (_selectedTalent != null && _selectedTalent.IsUnlocked() && MonoSingleton<Player>.Instance.Buy(_selectedTalent.Item))
		{
			UpdateTalents();
			return true;
		}
		return false;
	}

	public void UpdateTalentPoints()
	{
		string @string = Strings.GetString("IDS_TALENTS_AVAILABLE_POINTS");
		TalentPoints.Text = string.Format(@string, MonoSingleton<Player>.Instance.TalentPoints);
	}

	public void UpdateResetButton()
	{
		if (MonoSingleton<Player>.Instance.GetTalentPointsSpent() > 0)
		{
			if (ResetButton.controlState != 0)
			{
				ResetButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
			}
		}
		else if (ResetButton.controlState != UIButton.CONTROL_STATE.DISABLED)
		{
			ResetButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		}
	}

	private void UpdateLearnButton(TalentsTalentButton button)
	{
		if (button != null)
		{
			if (button.IsUnlocked())
			{
				if (MonoSingleton<Player>.Instance.GetTalentLevel(button.Item.id) != button.Item.LevelsCount)
				{
					if (MonoSingleton<Player>.Instance.TalentPoints > 0)
					{
						LearnButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
					}
					else
					{
						LearnButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
					}
					LearnButton.Text = Strings.GetString("IDS_TALENTS_LEARN_SKILL");
				}
				else
				{
					LearnButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
					LearnButton.Text = Strings.GetString("IDS_TALENTS_SKILL_MAXED");
				}
			}
			else
			{
				LearnButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
				LearnButton.Text = Strings.GetString("IDS_TALENTS_SKILL_LOCKED");
			}
		}
		else
		{
			LearnButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
			LearnButton.Text = Strings.GetString("IDS_TALENTS_LEARN_SKILL");
		}
	}

	private void UpdateTalentLevel(TalentsTalentButton button)
	{
		SpriteText component = TalentLevel.GetComponent<SpriteText>();
		UIAlign component2 = TalentLevel.GetComponent<UIAlign>();
		if (button != null)
		{
			int levelsCount = button.Item.GetLevelsCount();
			int talentLevel = MonoSingleton<Player>.Instance.GetTalentLevel(button.Item.id);
			component.Text = string.Format("{0}/{1}", talentLevel, levelsCount);
		}
		else
		{
			component.Text = string.Empty;
		}
		component2.UpdateAlignment();
	}

	public bool SelectTalent(TalentsTalentButton button)
	{
		if (_selectedTalent != button)
		{
			if (_selectedTalent != null)
			{
				TalentName.Text = string.Empty;
				TalentText.Text = string.Empty;
				_selectedTalent.SetSelected(false);
				_selectedTalent = null;
			}
			if (button != null)
			{
				_selectedTalent = button;
				_selectedTalent.SetSelected(true);
				string text = Strings.GetString(button.Item.DescriptionId);
				if (!button.IsUnlocked())
				{
					int num = text.IndexOf('\n');
					if (num != -1)
					{
						text = text.Substring(0, num);
					}
					int talentPointsSpent = MonoSingleton<Player>.Instance.GetTalentPointsSpent();
					int unlockPoints = button.Item.GetUnlockPoints();
					text += "\n[#D23500]";
					if (unlockPoints > talentPointsSpent)
					{
						string @string = Strings.GetString("IDS_TALENTS_MORE_POINTS_REQUIRED");
						text += string.Format(@string, unlockPoints - talentPointsSpent);
					}
					if (!string.IsNullOrEmpty(button.Item.ParentId))
					{
						int talentLevel = MonoSingleton<Player>.Instance.GetTalentLevel(button.Item.ParentId);
						if (talentLevel < 1)
						{
							text += Strings.GetString("IDS_TALENTS_LEARN_PARENT_SKILL");
						}
					}
				}
				TalentText.Text = text;
				TalentName.Text = Strings.GetString(button.Item.nameId);
				TalentNameBackground.SetInternalWidth(TalentName.TotalWidth + 25f);
				UpdateTalentLevel(_selectedTalent);
				UpdateLearnButton(_selectedTalent);
				if (TapSelectText != null)
				{
					Object.Destroy(TapSelectText);
					TapSelectText = null;
				}
			}
			return false;
		}
		return true;
	}

	public void OnTalentButtonTap(TalentsTalentButton button)
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.TalentSelected);
		SelectTalent(button);
	}

	public void SelectDefault()
	{
		TalentName.Text = Strings.GetString("IDS_TALENTS_NOTHING_SELECTED");
		TalentText.Text = string.Empty;
	}

	private void CreateTalents()
	{
		int num = TalentsButtons.Length;
		for (int i = 0; i < num; i++)
		{
			ShopItemTalent itemTalent = MonoSingleton<ShopController>.Instance.GetItemTalent(i);
			TalentsButtons[i].SetData(this, itemTalent);
		}
	}

	public void UpdateTalents()
	{
		if (_selectedTalent == null)
		{
			SelectDefault();
		}
		TalentsTalentButton[] talentsButtons = TalentsButtons;
		foreach (TalentsTalentButton talentsTalentButton in talentsButtons)
		{
			bool selected = _selectedTalent == talentsTalentButton;
			talentsTalentButton.UpdateData(selected);
		}
		TalentsTalentLine[] talentsLines = TalentsLines;
		foreach (TalentsTalentLine talentsTalentLine in talentsLines)
		{
			talentsTalentLine.UpdateData();
		}
		UpdateTalentPoints();
		UpdateTalentLevel(_selectedTalent);
		UpdateLearnButton(_selectedTalent);
		UpdateResetButton();
	}
}
