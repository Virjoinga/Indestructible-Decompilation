using Glu.Localization;
using UnityEngine;

public class TeamGameUIController : MonoBehaviour
{
	public SpriteText[] teamLabels;

	public SpriteText[] teamScores;

	public string[] teamLabelsStrings;

	public string[] teamCampaignStrings;

	private void Start()
	{
		SubscribeToEvents();
		TeamGame teamGame = IDTGame.Instance as TeamGame;
		if (teamGame.IsBossFight)
		{
			for (int i = 0; i < teamLabels.Length; i++)
			{
				teamLabels[i].Text = Strings.GetString(teamCampaignStrings[i]);
			}
		}
		else
		{
			for (int j = 0; j < teamLabels.Length; j++)
			{
				teamLabels[j].Text = Strings.GetString(teamLabelsStrings[j]);
			}
		}
	}

	protected virtual void SubscribeToEvents()
	{
		TeamGame teamGame = IDTGame.Instance as TeamGame;
		if (teamGame != null)
		{
			teamGame.teamScoreChangedEvent += TeamScoreChanged;
		}
	}

	protected virtual void TeamScoreChanged(MatchTeam team)
	{
		Debug.Log("TeamScoreChanged:" + TeamGame.GetData(team).score);
		if (team.id < teamScores.Length)
		{
			UpdateTeamScore(teamScores[team.id], TeamGame.GetData(team).score);
		}
	}

	protected virtual void UpdateTeamScore(SpriteText text, int score)
	{
		text.Text = score.ToString();
	}
}
