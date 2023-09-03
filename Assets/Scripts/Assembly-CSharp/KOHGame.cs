using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KOHGame : TeamGame
{
	public delegate void PointOwnerChangedEventDelegate(int ownerTeamId);

	public delegate void PointProgressChangedEventDelegate(float progress);

	public delegate void TeamScoreChangedEventDelegate(int teamId, float score);

	public float baseCaptureTime = 6f;

	public float restoreTime = 15f;

	public float captureProgressPeriod = 0.3f;

	public float dominationTime = 90f;

	public float dominationProgressPeriod = 1f;

	private List<MatchPlayer>[] _teamsPresence;

	private float[] _teamsRemainDomination;

	private float _captureProgressStep = 0.1f;

	private float _restoreProgressStep = 0.1f;

	private float _dominationProgressStep = 0.1f;

	private YieldInstruction _updateCapturePeriodYI;

	private YieldInstruction _updateDominationPeriodYI;

	private int _pointOwnerTeamId = -1;

	private float _pointProgress;

	private bool _dominationStarted;

	private bool _isPlayerOnPoint;

	private float _playerOnPointTime;

	public float PlayerOnPointTime
	{
		get
		{
			return _playerOnPointTime;
		}
	}

	public event PointOwnerChangedEventDelegate pointOwnerChangedEvent;

	public event PointOwnerChangedEventDelegate pointStartCaptureEvent;

	public event PointOwnerChangedEventDelegate pointCapturedEvent;

	public event PointProgressChangedEventDelegate pointProgressChangedEvent;

	protected override void Awake()
	{
		base.Awake();
		_teamsPresence = new List<MatchPlayer>[base.match.teamCount];
		_teamsRemainDomination = new float[base.match.teamCount];
		for (int i = 0; i < base.match.teamCount; i++)
		{
			_teamsPresence[i] = new List<MatchPlayer>();
			_teamsRemainDomination[i] = dominationTime;
		}
		_captureProgressStep = captureProgressPeriod / baseCaptureTime;
		_restoreProgressStep = captureProgressPeriod / restoreTime;
		_dominationProgressStep = dominationProgressPeriod;
		_updateCapturePeriodYI = new WaitForSeconds(captureProgressPeriod);
		_updateDominationPeriodYI = new WaitForSeconds(dominationProgressPeriod);
	}

	protected override void StartGame()
	{
		for (int i = 0; i < base.match.teamCount; i++)
		{
			InformTeamProgress(_pointOwnerTeamId, _teamsRemainDomination[i]);
			base.photonView.RPC("InformTeamProgress", PhotonTargets.Others, i, _teamsRemainDomination[i]);
		}
		base.StartGame();
	}

	public void PlayerEnterPoint()
	{
		ActorEnterPoint(base.localPlayer.id);
		base.photonView.RPC("ActorEnterPoint", PhotonTargets.Others, base.localPlayer.id);
	}

	public void PlayerLeavePoint()
	{
		ActorLeavePoint(base.match.localPlayer.id);
		base.photonView.RPC("ActorLeavePoint", PhotonTargets.Others, base.localPlayer.id);
	}

	private void CheckPointState()
	{
		int num = -1;
		for (int i = 0; i < base.match.teamCount; i++)
		{
			if (_teamsPresence[i].Count > 0)
			{
				if (num != -1)
				{
					StopPointProgress();
					return;
				}
				num = i;
			}
		}
		StopPointProgress();
		if (num == -1)
		{
			StartCoroutine("RestorePoint");
			return;
		}
		StartCapture(num);
		base.photonView.RPC("StartCapture", PhotonTargets.Others, num);
		StartCoroutine("CapturePoint", num);
	}

	private void StopPointProgress()
	{
		StopCoroutine("CapturePoint");
		StopCoroutine("RestorePoint");
	}

	private void StopDominationProgress()
	{
		_dominationStarted = false;
		StopCoroutine("UpdateTeamScore");
	}

	private IEnumerator CapturePoint(int teamId)
	{
		if (_pointOwnerTeamId != teamId)
		{
			StopDominationProgress();
			while (true)
			{
				_pointProgress -= _captureProgressStep * (float)_teamsPresence[teamId].Count;
				InformPointProgress(_pointProgress);
				base.photonView.RPC("InformPointProgress", PhotonTargets.Others, _pointProgress);
				if (_pointProgress <= 0f)
				{
					break;
				}
				yield return _updateCapturePeriodYI;
			}
			ClearPoint();
			base.photonView.RPC("ClearPoint", PhotonTargets.Others);
		}
		ChangeOwnerTeam(teamId);
		base.photonView.RPC("ChangeOwnerTeam", PhotonTargets.Others, teamId);
		while (true)
		{
			_pointProgress += _captureProgressStep * (float)_teamsPresence[teamId].Count;
			InformPointProgress(_pointProgress);
			base.photonView.RPC("InformPointProgress", PhotonTargets.Others, _pointProgress);
			if (_pointProgress >= 1f)
			{
				break;
			}
			yield return _updateCapturePeriodYI;
		}
		FixPoint(teamId);
		base.photonView.RPC("FixPoint", PhotonTargets.Others, teamId);
	}

	private IEnumerator RestorePoint()
	{
		ChangeOwnerTeam(_pointOwnerTeamId);
		base.photonView.RPC("ChangeOwnerTeam", PhotonTargets.Others, _pointOwnerTeamId);
		StopDominationProgress();
		while (true)
		{
			_pointProgress -= _restoreProgressStep;
			InformPointProgress(_pointProgress);
			base.photonView.RPC("InformPointProgress", PhotonTargets.Others, _pointProgress);
			if (_pointProgress <= 0f)
			{
				break;
			}
			yield return _updateCapturePeriodYI;
		}
		ClearPoint();
		base.photonView.RPC("ClearPoint", PhotonTargets.Others);
	}

	private IEnumerator UpdateTeamScore()
	{
		if (_dominationStarted)
		{
			yield break;
		}
		_dominationStarted = true;
		while (true)
		{
			_teamsRemainDomination[_pointOwnerTeamId] -= _dominationProgressStep;
			InformTeamProgress(_pointOwnerTeamId, _teamsRemainDomination[_pointOwnerTeamId]);
			base.photonView.RPC("InformTeamProgress", PhotonTargets.Others, _pointOwnerTeamId, _teamsRemainDomination[_pointOwnerTeamId]);
			if (_isPlayerOnPoint)
			{
				_playerOnPointTime += _dominationProgressStep;
			}
			if (_teamsRemainDomination[_pointOwnerTeamId] <= 0f)
			{
				break;
			}
			yield return _updateDominationPeriodYI;
		}
		base.photonView.RPC("TeamWin", PhotonTargets.Others, _pointOwnerTeamId);
		StopPointProgress();
		StopDominationProgress();
		StartCoroutine(DelayedTeamWin(_pointOwnerTeamId));
		_dominationStarted = false;
	}

	private IEnumerator DelayedTeamWin(int teamId)
	{
		yield return new WaitForSeconds(1f);
		TeamWin(teamId);
	}

	private void OnMasterClientSwitched(PhotonPlayer player)
	{
		if (PhotonNetwork.isMasterClient)
		{
			CheckPointState();
		}
	}

	[RPC]
	protected override void PlayerKillEnemy(int playerID, int enemyID, int damageType)
	{
		base.PlayerKillEnemy(playerID, enemyID, damageType);
	}

	[RPC]
	protected override void PlayerDied(int playerID)
	{
		base.PlayerDied(playerID);
		ActorLeavePoint(playerID);
	}

	[RPC]
	public void ActorEnterPoint(int playerID)
	{
		if (base.isGameOver)
		{
			return;
		}
		MatchPlayer player;
		if (base.match.TryGetPlayer(playerID, out player))
		{
			if (!_teamsPresence[player.teamID].Contains(player))
			{
				_teamsPresence[player.teamID].Add(player);
			}
			if (player == base.localPlayer)
			{
				_isPlayerOnPoint = true;
			}
		}
		if (PhotonNetwork.isMasterClient)
		{
			CheckPointState();
		}
	}

	[RPC]
	public void ActorLeavePoint(int playerID)
	{
		if (base.isGameOver)
		{
			return;
		}
		MatchPlayer player;
		if (base.match.TryGetPlayer(playerID, out player))
		{
			_teamsPresence[player.teamID].Remove(player);
			if (player == base.localPlayer)
			{
				_isPlayerOnPoint = false;
			}
		}
		if (PhotonNetwork.isMasterClient)
		{
			CheckPointState();
		}
	}

	[RPC]
	private void ClearPoint()
	{
		if (!base.isGameOver)
		{
			_pointProgress = 0f;
			ChangeOwnerTeam(-1);
			StopDominationProgress();
		}
	}

	[RPC]
	private void FixPoint(int teamId)
	{
		if (!base.isGameOver)
		{
			_pointProgress = 1f;
			ChangeOwnerTeam(teamId);
			if (this.pointCapturedEvent != null)
			{
				this.pointCapturedEvent(teamId);
			}
			if (PhotonNetwork.isMasterClient)
			{
				StartCoroutine("UpdateTeamScore");
			}
		}
	}

	[RPC]
	private void ChangeOwnerTeam(int teamId)
	{
		if (!base.isGameOver)
		{
			if (this.pointOwnerChangedEvent != null && _pointOwnerTeamId != teamId)
			{
				this.pointOwnerChangedEvent(teamId);
			}
			_pointOwnerTeamId = teamId;
		}
	}

	[RPC]
	private void StartCapture(int teamId)
	{
		if (!base.isGameOver && this.pointStartCaptureEvent != null)
		{
			this.pointStartCaptureEvent(teamId);
		}
	}

	[RPC]
	private void InformPointProgress(float val)
	{
		_pointProgress = val;
		if (this.pointProgressChangedEvent != null)
		{
			this.pointProgressChangedEvent(val);
		}
	}

	[RPC]
	private void InformTeamProgress(int teamId, float val)
	{
		if (teamId > 0)
		{
			_teamsRemainDomination[teamId] = val;
		}
		TeamScoreChanged(base.match.GetTeam(teamId));
	}

	[RPC]
	private void TeamWin(int teamId)
	{
		GameOver(teamId == base.match.localTeam.id);
	}

	protected override void PlayerDisconnected(MultiplayerMatch match, MatchPlayer player)
	{
		if (base.isGameStarted)
		{
			ActorLeavePoint(player.id);
		}
		base.PlayerDisconnected(match, player);
	}
}
