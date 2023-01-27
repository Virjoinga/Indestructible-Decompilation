using Glu.Localization;
using UnityEngine;

public class SPInformer : MonoBehaviour
{
	public GameObject FragsCounter;

	public GameObject WavesCounter;

	private SingleGame _Game;

	private SpriteText _fragsCounterText;

	private int _maxFrags;

	private int _waveNum = 1;

	private string _fragsTextFormat = string.Empty;

	private void SubscribeToGame()
	{
		_Game = IDTGame.Instance as SingleGame;
		if (_Game != null && !_Game.IsBossFight)
		{
			_Game.playerKillEnemyEvent += OnPlayerKillEnemy;
			SurvivalGame survivalGame = _Game as SurvivalGame;
			if ((bool)survivalGame)
			{
				_fragsTextFormat = Strings.GetString("IDS_GAMEPLAY_INFORMER_SINGLE");
				survivalGame.waveCompleteEvent += OnWaveComplete;
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		_fragsCounterText = ((!(FragsCounter != null)) ? null : FragsCounter.GetComponentInChildren<SpriteText>());
		SubscribeToGame();
		UpdateText();
	}

	private void OnPlayerKillEnemy(SingleGame game)
	{
		UpdateMaxFrags();
		UpdateText();
	}

	private void OnWaveComplete(int waveIndex, int rewardSC, int rewardXP)
	{
		_waveNum = waveIndex + 2;
		UpdateText();
	}

	private void UpdateText()
	{
		if ((bool)_fragsCounterText)
		{
			_fragsCounterText.Text = string.Format(_fragsTextFormat, _Game.killCount, _maxFrags, _waveNum);
		}
	}

	private void UpdateMaxFrags()
	{
		_maxFrags = -999;
		if (_maxFrags < _Game.killCount)
		{
			_maxFrags = _Game.killCount;
		}
	}
}
