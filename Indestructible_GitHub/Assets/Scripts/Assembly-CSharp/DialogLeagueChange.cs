using Glu.Localization;
using UnityEngine;

public class DialogLeagueChange : UIDialog
{
	public SpriteText Title;

	public UIBorderSprite TitleBackground;

	public SpriteText TextTop;

	public SpriteText TextBottom;

	public GarageLeagueIcon LeagueIcon;

	public UIButton CloseButton;

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	public void SetData(int leagueChange)
	{
		int league = MonoSingleton<Player>.Instance.League;
		string leagueName = Player.GetLeagueName(league);
		if (leagueChange > 0)
		{
			Title.Text = Strings.GetString("IDS_LEAGUE_CHANGE_UP_TITLE");
			TextTop.Text = Strings.GetString("IDS_LEAGUE_CHANGE_UP_TEXT_TOP");
			string @string = Strings.GetString("IDS_LEAGUE_CHANGE_UP_TEXT_BOTTOM");
			TextBottom.Text = string.Format(@string, leagueName);
		}
		else
		{
			Title.Text = Strings.GetString("IDS_LEAGUE_CHANGE_DOWN_TITLE");
			TextTop.Text = Strings.GetString("IDS_LEAGUE_CHANGE_DOWN_TEXT_TOP");
			string string2 = Strings.GetString("IDS_LEAGUE_CHANGE_DOWN_TEXT_BOTTOM");
			TextBottom.Text = string.Format(string2, leagueName);
		}
		TitleBackground.SetInternalWidth(Title.TotalWidth);
		CloseButton.Text = Strings.GetString("IDS_LEAGUE_CHANGE_BUTTON_OK");
	}

	public override void Activate()
	{
		base.Activate();
		LeagueIcon.UpdateIcon();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
