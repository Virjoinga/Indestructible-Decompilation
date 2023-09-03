using System.Collections;
using UnityEngine;

public class TrainController : MonoBehaviour, IUpdatable
{
	public float startDelay = 30f;

	public float startInterval = 60f;

	public float stopInterval = 10f;

	public float speed = 20f;

	public Animation startAnimation;

	private YieldInstruction _stopIntervalInstruction;

	private NetworkingPeer _networkingPeer;

	private uint _serverTimeMsToStart;

	private float _timeToStart;

	private Vector3 _startPosition;

	private Vector3 _velocity;

	private Transform _trainTransform;

	private Rigidbody _trainRigidbody;

	private void Start()
	{
		_trainTransform = base.transform;
		_startPosition = _trainTransform.position;
		_velocity = _trainTransform.forward * speed;
		_trainRigidbody = base.GetComponent<Rigidbody>();
		_stopIntervalInstruction = new WaitForSeconds(stopInterval);
		base.gameObject.SetActiveRecursively(false);
		if (PhotonNetwork.room != null)
		{
			MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
			if (multiplayerGame != null && multiplayerGame.match.isOnline)
			{
				_networkingPeer = PhotonNetwork.networkingPeer;
				if ((multiplayerGame.match.state & MultiplayerMatch.State.Started) == MultiplayerMatch.State.Started)
				{
					MatchStarted(multiplayerGame.match);
				}
				else
				{
					multiplayerGame.match.matchStartedEvent += MatchStarted;
				}
				return;
			}
		}
		_timeToStart = Time.time + startDelay;
		MonoSingleton<UpdateAgent>.Instance.StartUpdate(this);
	}

	private void OnDestroy()
	{
		if (MonoSingleton<UpdateAgent>.Exists())
		{
			MonoSingleton<UpdateAgent>.Instance.StopUpdate(this);
		}
	}

	private void MatchStarted(MultiplayerMatch match)
	{
		_serverTimeMsToStart = match.serverStartTimeInMilliSeconds + (uint)(startDelay * 1000f);
		MonoSingleton<UpdateAgent>.Instance.StartUpdate(this);
	}

	public bool DoUpdate()
	{
		if (_networkingPeer == null)
		{
			if (Time.time < _timeToStart)
			{
				return true;
			}
		}
		else if ((uint)_networkingPeer.ServerTimeInMilliSeconds < _serverTimeMsToStart)
		{
			return true;
		}
		StartTrain();
		return true;
	}

	private void StartTrain()
	{
		if (_networkingPeer == null)
		{
			_timeToStart += startInterval;
		}
		else
		{
			_serverTimeMsToStart += (uint)(startInterval * 1000f);
		}
		base.gameObject.SetActiveRecursively(true);
		_trainTransform.position = _startPosition;
		_trainRigidbody.velocity = _velocity;
		if (startAnimation != null)
		{
			startAnimation.Play();
		}
		StartCoroutine(StopTrain());
	}

	private IEnumerator StopTrain()
	{
		yield return _stopIntervalInstruction;
		base.gameObject.SetActiveRecursively(false);
	}
}
