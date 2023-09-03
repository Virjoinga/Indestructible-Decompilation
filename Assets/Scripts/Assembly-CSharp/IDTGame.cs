using System;
using Glu;
using UnityEngine;

public abstract class IDTGame : Glu.MonoBehaviour
{
	public struct Reward
	{
		public int MoneySoft;

		public int ExperiencePoints;

		public int InfluencePoints;

		public int LeagueChange;

		public int LevelsGained;

		public bool Victory;
	}

	public delegate void OnGameOverDelegate(IDTGame game, ref Reward reward);

	private bool _isGameStarted;

	private bool _isGameOver;

	private static IDTGame _instance;

	protected bool _isTutorial;

	protected int _bossFightIdx = -1;

	public static IDTGame Instance
	{
		get
		{
			return _instance;
		}
	}

	public bool isGameStarted
	{
		get
		{
			return _isGameStarted;
		}
	}

	public bool isGameOver
	{
		get
		{
			return _isGameOver;
		}
	}

	public bool IsTutorial
	{
		get
		{
			return _isTutorial;
		}
	}

	public bool IsBossFight
	{
		get
		{
			return _bossFightIdx > -1;
		}
	}

	public int BossFightIdx
	{
		get
		{
			return _bossFightIdx;
		}
	}

	public event OnGameOverDelegate gameOverEvent;

	public void SetIsTutorial(bool tutor)
	{
		_isTutorial = tutor;
	}

	public void SetBossFightIdx(int bossFightIdx)
	{
		_bossFightIdx = bossFightIdx;
	}

	public virtual void Cancel()
	{
	}

	public virtual void PauseMenuActivated()
	{
	}

	public virtual void PauseMenuDeactivated()
	{
	}

	public virtual void Destructed(Destructible destructible, DestructionReason reason, INetworkWeapon weapon)
	{
	}

	public static IDTGame PrepareInstance(GameObject gameObject, Type gameModeType)
	{
		IDTGame component = gameObject.GetComponent<IDTGame>();
		if (component != null)
		{
			if (component.GetType() == gameModeType)
			{
				return component;
			}
			UnityEngine.Object.Destroy(component);
		}
		return gameObject.AddComponent(gameModeType) as IDTGame;
	}

	protected virtual void Awake()
	{
		_instance = this;
	}

	protected virtual void OnDestroy()
	{
		QualityManager.instance.StopUpdateQualityLevel();
		_instance = null;
	}

	protected virtual void Clear()
	{
		_isGameStarted = false;
		_isGameOver = false;
	}

	protected virtual void ClearEvents()
	{
		this.gameOverEvent = null;
	}

	protected virtual void StartGame()
	{
		_isGameStarted = true;
		VehiclesManager instance = VehiclesManager.instance;
		instance.SpawnLocalPlayerVehicle(SelectSpawnPoint(null, instance.spawnPoints));
		QualityManager.instance.StartUpdateQualityLevel();
	}

	protected virtual Transform SelectSpawnPoint(Vehicle vehicle, Transform[] spawnPoints)
	{
		return VehiclesManager.SelectSpawnPoint(vehicle, spawnPoints);
	}

	protected virtual void GameOver(bool hasWon)
	{
		if (!_isGameOver)
		{
			MonoSingleton<DialogsQueue>.Instance.Clear();
			_isGameOver = true;
			Reward reward = default(Reward);
			CalculateReward(hasWon, ref reward);
			MonoSingleton<Player>.Instance.BoostReward(ref reward);
			MonoSingleton<Player>.Instance.AddReward(ref reward, "CREDIT_SC", "Mission Complete");
			if (this.gameOverEvent != null)
			{
				this.gameOverEvent(this, ref reward);
			}
		}
	}

	protected abstract void CalculateReward(bool hasWon, ref Reward reward);

	protected void SetBossFightReward(bool win, ref Reward reward)
	{
		reward.MoneySoft = 0;
		reward.ExperiencePoints = 0;
		reward.InfluencePoints = 0;
		BossFightConfig bossFightConfig = BossFightConfiguration.Instance.BossFights[BossFightIdx];
		if (bossFightConfig != null)
		{
			if (win && bossFightConfig.Reward != null)
			{
				int? experience = bossFightConfig.Reward.Experience;
				reward.ExperiencePoints = ((!experience.HasValue) ? new int?(0) : bossFightConfig.Reward.Experience).Value;
				int? moneySoft = bossFightConfig.Reward.MoneySoft;
				reward.MoneySoft = ((!moneySoft.HasValue) ? new int?(0) : bossFightConfig.Reward.MoneySoft).Value;
			}
			else if (!win && bossFightConfig.RewardFail != null)
			{
				int? experience2 = bossFightConfig.RewardFail.Experience;
				reward.ExperiencePoints = ((!experience2.HasValue) ? new int?(0) : bossFightConfig.RewardFail.Experience).Value;
				int? moneySoft2 = bossFightConfig.RewardFail.MoneySoft;
				reward.MoneySoft = ((!moneySoft2.HasValue) ? new int?(0) : bossFightConfig.RewardFail.MoneySoft).Value;
			}
		}
	}
}
