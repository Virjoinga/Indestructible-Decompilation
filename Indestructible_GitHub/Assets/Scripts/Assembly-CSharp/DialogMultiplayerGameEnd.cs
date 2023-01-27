using System.Collections.Generic;
using Glu.Localization;
using UnityEngine;

public class DialogMultiplayerGameEnd : UIDialog
{
	public DialogMultiplayerGameEndItem[] PlayersList;

	public UITextField MessageEdit;

	public UIScrollList MessagesList;

	public SpriteText ChatMessages;

	public SpriteText GameValueColumn;

	private MultiplayerMatch _match;

	public void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		MonoSingleton<Player>.Instance.StartMatchLeftLevel("GarageScene");
		MonoSingleton<DialogsQueue>.Instance.Pause();
		Close();
	}

	public override void Activate()
	{
		base.Activate();
		UpdatePlayers();
	}

	public static string GetLastColumnLabelId()
	{
		string result = "IDS_MULTIPLAYER_END_COLUMN_POINTS";
		if (IDTGame.Instance is CRTeamGame)
		{
			result = "IDS_MULTIPLAYER_END_COLUMN_CHARGES";
		}
		else if (IDTGame.Instance is CTFGame)
		{
			result = "IDS_MULTIPLAYER_END_COLUMN_FLAGS";
		}
		else if (IDTGame.Instance is DeathmatchGame)
		{
			result = "IDS_MULTIPLAYER_END_COLUMN_FRAGS";
		}
		return result;
	}

	private void SetLastColumnLabel()
	{
		string lastColumnLabelId = GetLastColumnLabelId();
		GameValueColumn.Text = Strings.GetString(lastColumnLabelId);
	}

	public void Fill()
	{
		ChatMessages.Text = string.Empty;
		foreach (MatchPlayer player in _match.players)
		{
			if (!player.isDisconnected)
			{
				string @string = Strings.GetString("IDS_MULTIPLAYER_END_JOINED_CHAT");
				InsertChatMessage(string.Format(@string, player.name));
			}
		}
	}

	private void UpdatePlayers()
	{
		List<MatchPlayer> list = new List<MatchPlayer>(_match.players);
		list.Sort(delegate(MatchPlayer a, MatchPlayer b)
		{
			GamePlayer gamePlayer = a as GamePlayer;
			GamePlayer gamePlayer2 = b as GamePlayer;
			return gamePlayer2.score - gamePlayer.score;
		});
		int num = 0;
		DialogMultiplayerGameEndItem[] playersList = PlayersList;
		foreach (DialogMultiplayerGameEndItem dialogMultiplayerGameEndItem in playersList)
		{
			if (num < list.Count)
			{
				MonoUtils.SetActive(dialogMultiplayerGameEndItem, true);
				dialogMultiplayerGameEndItem.SetData(list[num] as GamePlayer, _match);
				Color color = new Color(0.16f, 0.16f, 0.16f);
				if (num % 2 == 1)
				{
					color = new Color(0.22f, 0.22f, 0.22f);
				}
				dialogMultiplayerGameEndItem.Background.Color = color;
			}
			else
			{
				MonoUtils.SetActive(dialogMultiplayerGameEndItem, false);
			}
			num++;
		}
		SetLastColumnLabel();
	}

	private void CommitDelegate(IKeyFocusable control)
	{
		string text = MessageEdit.Text;
		text = MonoSingleton<Player>.Instance.ValidateMessage(text);
		if (text.Length > 0)
		{
			_match.SendChatMessage(MultiplayerMatch.Targets.All, text);
		}
		MessageEdit.Text = string.Empty;
	}

	private string ValidateDelegate(UITextField field, string text, ref int insertionPoint)
	{
		if (text != null && text.Length > 70)
		{
			text = text.Substring(0, 70);
		}
		text = MonoSingleton<Player>.Instance.ValidateMessage(text);
		return text;
	}

	private void FocusDelegate(UITextField field)
	{
		field.Text = string.Empty;
		field.spriteText.Color = new Color(0.8f, 0.8f, 0.8f, 1f);
	}

	private void InsertChatMessage(string message)
	{
		ChatMessages.Text = message + "\n" + ChatMessages.Text;
	}

	private void OnChatMessageReceived(string message, MatchPlayer sender)
	{
		if (sender != null)
		{
			string @string = Strings.GetString("IDS_MULTIPLAYER_END_MESSAGE_FORMAT");
			string text = string.Format(@string, sender.name, message);
			InsertChatMessage(string.Concat(Color.white, text));
			MessagesList.RepositionItems();
			MessagesList.ScrollPosition = 0f;
		}
	}

	private void OnMatchPlayerDisconnected(MultiplayerMatch match, MatchPlayer actor)
	{
		if (actor != null)
		{
			string @string = Strings.GetString("IDS_MULTIPLAYER_END_LEFT_CHAT");
			InsertChatMessage(string.Format(@string, actor.name));
			UpdatePlayers();
		}
	}

	private void OnMatchCancelled(MultiplayerMatch game, MultiplayerMatch.CancelReason reason)
	{
		if (reason == MultiplayerMatch.CancelReason.Disconnection)
		{
			string @string = Strings.GetString("IDS_MULTIPLAYER_END_DISCONNECTED");
			InsertChatMessage(@string);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (IDTGame.Instance is MultiplayerGame)
		{
			_match = (IDTGame.Instance as MultiplayerGame).match;
			_match.chatMessageRecievedEvent += OnChatMessageReceived;
			_match.playerDisconnectedEvent += OnMatchPlayerDisconnected;
			_match.canceledEvent += OnMatchCancelled;
		}
	}

	private void OnDestroy()
	{
		if (_match != null)
		{
			_match.chatMessageRecievedEvent -= OnChatMessageReceived;
			_match.playerDisconnectedEvent -= OnMatchPlayerDisconnected;
			_match.canceledEvent -= OnMatchCancelled;
		}
	}

	protected override void Start()
	{
		base.Start();
		MessageEdit.AddFocusDelegate(FocusDelegate);
		MessageEdit.AddCommitDelegate(CommitDelegate);
		MessageEdit.AddValidationDelegate(ValidateDelegate);
		ISpriteMesh spriteMesh = MessageEdit.spriteMesh;
		if (spriteMesh != null)
		{
			spriteMesh.Hide(true);
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
