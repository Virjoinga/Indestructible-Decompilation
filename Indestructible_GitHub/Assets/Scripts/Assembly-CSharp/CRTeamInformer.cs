using System;
using UnityEngine;

public class CRTeamInformer : MonoBehaviour
{
	[Serializable]
	public class TeamGUIElements
	{
		public SpriteText TeamsScore;
	}

	public TeamGUIElements[] TeamsGUI = new TeamGUIElements[2];

	private TeamGame _game;

	private void SubscribeToGame()
	{
		_game = IDTGame.Instance as TeamGame;
		CRTeamGame cRTeamGame = _game as CRTeamGame;
		if (cRTeamGame != null)
		{
			cRTeamGame.teamScoreChangedEvent += OnTeamScoreChanged;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		SubscribeToGame();
	}

	private void OnTeamScoreChanged(MatchTeam team)
	{
		TeamGUIElements teamGUIElements = TeamsGUI[team.id];
		if ((bool)teamGUIElements.TeamsScore)
		{
			teamGUIElements.TeamsScore.Text = TeamGame.GetData(team).score.ToString();
		}
	}
}
