using Glu.Localization;
using UnityEngine;

public class CTFFlagTextNotifications : MonoBehaviour
{
	private CTFGame _game;

	private bool _initialized;

	private bool Initialize()
	{
		if (_initialized)
		{
			return true;
		}
		_game = IDTGame.Instance as CTFGame;
		if (_game != null)
		{
			_game.flagCapturedEvent += OnPlayerCaptureFlag;
			_game.flagDeliveredEvent += OnPlayerDeliverFlag;
			_game.courierKilledEvent += OnPlayerKillCourier;
			_game.flagReturnedEvent += OnPlayerReturnFlag;
			_game.flagAutoReturnedEvent += OnReturnFlag;
			_initialized = true;
		}
		return _initialized;
	}

	private void OnDestroy()
	{
		if (_game != null)
		{
			_game.flagCapturedEvent -= OnPlayerCaptureFlag;
			_game.flagDeliveredEvent -= OnPlayerDeliverFlag;
			_game.courierKilledEvent -= OnPlayerKillCourier;
			_game.flagReturnedEvent -= OnPlayerReturnFlag;
			_game.flagAutoReturnedEvent -= OnReturnFlag;
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

	private void OnPlayerCaptureFlag(MatchPlayer player)
	{
		string @string = Strings.GetString("IDS_CTF_FLAG_TAKEN");
		Notify(string.Format(@string, GetOtherTeamName(player.teamID)));
	}

	private void OnPlayerDeliverFlag(MatchPlayer player)
	{
		string @string = Strings.GetString("IDS_CTF_FLAG_CAPTURED");
		Notify(string.Format(@string, player.name, GetOtherTeamName(player.teamID)));
	}

	private void OnPlayerReturnFlag(MatchPlayer player)
	{
		string @string = Strings.GetString("IDS_CTF_FLAG_RETURNED");
		Notify(string.Format(@string, GetTeamName(player.teamID)));
	}

	private void OnPlayerKillCourier(MatchPlayer player)
	{
		string @string = Strings.GetString("IDS_CTF_FLAG_DROPPED");
		Notify(string.Format(@string, GetOtherTeamName(player.teamID)));
	}

	private void OnReturnFlag(int teamId)
	{
		string @string = Strings.GetString("IDS_CTF_FLAG_RETURNED");
		Notify(string.Format(@string, GetTeamName(teamId)));
	}
}
