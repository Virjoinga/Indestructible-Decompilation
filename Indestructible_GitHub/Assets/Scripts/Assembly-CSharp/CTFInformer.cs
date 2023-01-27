using UnityEngine;

public class CTFInformer : MonoBehaviour
{
	public GameObject[] TeamFlagCounters = new GameObject[2];

	private SpriteText[] _teamFlagCountersTexts;

	private void SubscribeToGame()
	{
		CTFGame cTFGame = IDTGame.Instance as CTFGame;
		if (cTFGame != null)
		{
			cTFGame.flagDeliveredEvent += PlayerDeliveredFlag;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		SubscribeToGame();
		_teamFlagCountersTexts = new SpriteText[TeamFlagCounters.Length];
		for (int i = 0; i < TeamFlagCounters.Length; i++)
		{
			_teamFlagCountersTexts[i] = TeamFlagCounters[i].GetComponentInChildren<SpriteText>();
		}
	}

	private void PlayerDeliveredFlag(MatchPlayer player)
	{
		UpdateScoreTexts();
	}

	private void UpdateScoreTexts()
	{
		TeamGame teamGame = IDTGame.Instance as TeamGame;
		if (!(teamGame != null))
		{
			return;
		}
		MultiplayerMatch match = teamGame.match;
		for (int i = 0; i < _teamFlagCountersTexts.Length; i++)
		{
			if ((bool)_teamFlagCountersTexts[i])
			{
				_teamFlagCountersTexts[i].Text = string.Empty + TeamGame.GetData(match.GetTeam(i)).score;
			}
		}
	}
}
