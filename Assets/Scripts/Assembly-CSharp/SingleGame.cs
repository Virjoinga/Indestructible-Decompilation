using System;
using System.Collections;
using UnityEngine;

public class SingleGame : IDTGame
{
	public static bool playOnEnable = true;

	private int _killCount;

	private int _deathCount;

	public int killCount
	{
		get
		{
			return _killCount;
		}
	}

	public int deathCount
	{
		get
		{
			return _deathCount;
		}
	}

	public event Action<SingleGame> playerKillEnemyEvent;

	public override void Cancel()
	{
		base.Cancel();
		Resume();
	}

	public override void PauseMenuActivated()
	{
		Pause();
	}

	public override void PauseMenuDeactivated()
	{
		Resume();
	}

	public static void OpenPauseMenu()
	{
		if (!DialogPause.exists)
		{
			Dialogs.PauseGame();
		}
	}

	public static void Pause()
	{
		Time.timeScale = 0f;
	}

	public static void Resume()
	{
		Time.timeScale = 1f;
	}

	protected override void Awake()
	{
		base.Awake();
		InitGame();
	}

	protected virtual void OnEnable()
	{
		PrepareGame();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (MonoSingleton<GameController>.Exists())
		{
			MonoSingleton<GameController>.Instance.suspendEvent -= OpenPauseMenu;
		}
		Resume();
	}

	public void DelayedActivateGame()
	{
		StartCoroutine(DelayedStart());
	}

	public void ActivateGame()
	{
		StartGame();
	}

	private IEnumerator DelayedStart()
	{
		yield return null;
		StartGame();
	}

	protected override void StartGame()
	{
		base.StartGame();
		MonoSingleton<GameController>.Instance.suspendEvent += OpenPauseMenu;
	}

	protected virtual void InitGame()
	{
		if (PhotonNetwork.connected)
		{
			PhotonNetwork.Disconnect();
		}
		PhotonNetwork.offlineMode = true;
		PhotonNetwork.networkingPeer.RemoveAllInstantiatedObjects();
	}

	protected virtual void PrepareGame()
	{
		GameModeConf.ConfigureScene("[single]");
	}

	private void Died()
	{
		if (!base.isGameOver && 0 < ++_deathCount)
		{
			GameOver(false);
		}
	}

	public override void Destructed(Destructible destructible, DestructionReason reason, INetworkWeapon weapon)
	{
		if (!base.isGameOver)
		{
			if (destructible.vehicle == VehiclesManager.instance.playerVehicle)
			{
				Died();
			}
			else if (destructible.vehicle != null && destructible.vehicle is AIVehicle)
			{
				_killCount++;
			}
			if (this.playerKillEnemyEvent != null)
			{
				this.playerKillEnemyEvent(this);
			}
		}
	}

	protected override void CalculateReward(bool win, ref Reward reward)
	{
		reward.MoneySoft = _killCount;
		reward.ExperiencePoints = (int)(2f + 0.1f * (float)_killCount);
		reward.Victory = win;
	}
}
