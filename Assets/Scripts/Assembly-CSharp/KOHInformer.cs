using UnityEngine;

public class KOHInformer : MonoBehaviour
{
	public SpriteText[] TeamTimers;

	public InclinedProgressBar CaptureMeter;

	public float PointProgressSmoothSpeed = 0.3f;

	private float _pointProgress;

	private float _pointSmoothProgress;

	private KOHGame _KOHGame;

	private void SubscribeToGame()
	{
		_KOHGame = IDTGame.Instance as KOHGame;
		if (_KOHGame != null)
		{
			_KOHGame.pointOwnerChangedEvent += OnPointOwnerChanged;
			_KOHGame.pointStartCaptureEvent += OnPointStartCapture;
			_KOHGame.pointCapturedEvent += OnPointCaptured;
			_KOHGame.pointProgressChangedEvent += OnPointProgressChanged;
			_KOHGame.teamScoreChangedEvent += OnTeamScoreChanged;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		SubscribeToGame();
		CaptureMeter.Position = 0f;
	}

	private void ActivateProgressBar(int teamId)
	{
		CaptureMeter.Color = MonoSingleton<Player>.Instance.GetTeamColor(teamId);
		CaptureMeter.Position = 0f;
	}

	private void SetProgress(float v)
	{
		CaptureMeter.Position = v;
	}

	private void OnPointOwnerChanged(int ownerTeamId)
	{
		ActivateProgressBar(ownerTeamId);
	}

	private void OnPointCaptured(int ownerTeamId)
	{
		_pointSmoothProgress = 1f;
		SetProgress(1f);
	}

	private void OnPointStartCapture(int ownerTeamId)
	{
	}

	private void OnPointProgressChanged(float progress)
	{
		_pointProgress = progress;
	}

	private void OnTeamScoreChanged(MatchTeam team)
	{
		string text = TeamTimers[team.id].Text;
		string text2 = TeamGame.GetData(team).score.ToString("0");
		if (text2 != text)
		{
			TeamTimers[team.id].Text = text2;
		}
	}

	private void UpdateSmoothProgress()
	{
		if (_pointSmoothProgress == _pointProgress)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		float num = PointProgressSmoothSpeed * deltaTime;
		if (_pointSmoothProgress > _pointProgress)
		{
			_pointSmoothProgress -= num;
			if (_pointSmoothProgress < _pointProgress)
			{
				_pointSmoothProgress = _pointProgress;
			}
		}
		else
		{
			_pointSmoothProgress += num;
			if (_pointSmoothProgress > _pointProgress)
			{
				_pointSmoothProgress = _pointProgress;
			}
		}
		SetProgress(_pointSmoothProgress);
	}

	private void Update()
	{
		UpdateSmoothProgress();
	}
}
