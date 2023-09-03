using Glu.Localization;
using UnityEngine;

public class CRTextNotifications : MonoBehaviour
{
	private bool _initialized;

	private CRTeamGame _CRTeamGame;

	private void SubscribeToGame()
	{
		_CRTeamGame = IDTGame.Instance as CRTeamGame;
		if (_CRTeamGame != null)
		{
			_CRTeamGame.chargeCapturedEvent += OnChargeCaptured;
			_CRTeamGame.chargeDissolvedEvent += OnChargeDissolvedEvent;
			_CRTeamGame.chargeSpawnedEvent += OnChargeSpawnedEvent;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private bool Initialize()
	{
		if (_initialized)
		{
			return true;
		}
		SubscribeToGame();
		return _initialized = true;
	}

	private void OnDestroy()
	{
		if (_CRTeamGame != null)
		{
			_CRTeamGame.chargeCapturedEvent -= OnChargeCaptured;
			_CRTeamGame.chargeDissolvedEvent -= OnChargeDissolvedEvent;
			_CRTeamGame.chargeSpawnedEvent -= OnChargeSpawnedEvent;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		Initialize();
	}

	private string GetTeamName(int teamId)
	{
		return MonoSingleton<Player>.Instance.GetTeamName(teamId);
	}

	private string GetOtherTeamName(int teamId)
	{
		teamId = ((teamId == 0) ? 1 : 0);
		return GetTeamName(teamId);
	}

	private void Notify(string text)
	{
		MonoSingleton<NotificationsQueue>.Instance.AddText(text);
	}

	private void OnChargeCaptured(MatchPlayer player)
	{
		string @string = Strings.GetString("IDS_CR_TEAM_CHARGE_CAPTURED");
		Notify(string.Format(@string, GetTeamName(player.teamID)));
	}

	private void OnChargeDissolvedEvent(MatchTeam team)
	{
		string @string = Strings.GetString("IDS_CR_TEAM_CHARGE_COLLECTED");
		Notify(string.Format(@string, GetTeamName(team.id)));
	}

	private void OnChargeSpawnedEvent()
	{
		string @string = Strings.GetString("IDS_CHARGE_SPAWNED_TEXT");
		Notify(@string);
	}
}
