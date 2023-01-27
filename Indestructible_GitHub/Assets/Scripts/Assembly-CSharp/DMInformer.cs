using System.Collections;
using Glu.Localization;
using UnityEngine;

public class DMInformer : MonoBehaviour
{
	public SpriteText FragsCounter;

	private DeathmatchGame _game;

	private int _maxFrags = -1;

	private int _frags = -1;

	private string _fragsTextFormat;

	private void Start()
	{
		_game = IDTGame.Instance as DeathmatchGame;
		_fragsTextFormat = Strings.GetString("IDS_GAMEPLAY_INFORMER_DEATHMATCH");
		StartCoroutine(UpdateText());
	}

	private int GetMaxFrags()
	{
		int num = int.MinValue;
		foreach (GamePlayer player in _game.players)
		{
			if (num < player.score)
			{
				num = player.score;
			}
		}
		return num;
	}

	private IEnumerator UpdateText()
	{
		while (true)
		{
			bool changed = false;
			int maxFrags = GetMaxFrags();
			if (_maxFrags != maxFrags)
			{
				_maxFrags = maxFrags;
				changed = true;
			}
			if (_frags != _game.localPlayer.score)
			{
				_frags = _game.localPlayer.score;
				changed = true;
			}
			if (changed)
			{
				FragsCounter.Text = string.Format(_fragsTextFormat, _game.localPlayer.score, _maxFrags);
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
}
