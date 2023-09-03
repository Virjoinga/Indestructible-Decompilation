using UnityEngine;

public class KOHPoint : MonoBehaviour
{
	public GameObject[] CaptureFX;

	public GameObject[] OwnFX;

	private GameObject _activeFX;

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
		}
	}

	private void OnDestroy()
	{
		if (_KOHGame != null)
		{
			_KOHGame.pointOwnerChangedEvent -= OnPointOwnerChanged;
			_KOHGame.pointStartCaptureEvent -= OnPointStartCapture;
			_KOHGame.pointCapturedEvent -= OnPointCaptured;
			_KOHGame.pointProgressChangedEvent -= OnPointProgressChanged;
		}
	}

	private void Start()
	{
		SubscribeToGame();
		if (CaptureFX != null)
		{
			GameObject[] captureFX = CaptureFX;
			foreach (GameObject gameObject in captureFX)
			{
				ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
				if (componentsInChildren != null)
				{
					ParticleSystem[] array = componentsInChildren;
					foreach (ParticleSystem particleSystem in array)
					{
						particleSystem.Play();
					}
				}
				gameObject.SetActiveRecursively(false);
			}
		}
		if (OwnFX == null)
		{
			return;
		}
		GameObject[] ownFX = OwnFX;
		foreach (GameObject gameObject2 in ownFX)
		{
			ParticleSystem[] componentsInChildren2 = gameObject2.GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren2 != null)
			{
				ParticleSystem[] array2 = componentsInChildren2;
				foreach (ParticleSystem particleSystem2 in array2)
				{
					particleSystem2.Play();
				}
			}
			gameObject2.SetActiveRecursively(false);
		}
	}

	private void StopActiveFX()
	{
		if ((bool)_activeFX)
		{
			_activeFX.SetActiveRecursively(false);
		}
		_activeFX = null;
	}

	private void ResetActiveFX(GameObject newFX)
	{
		StopActiveFX();
		_activeFX = newFX;
		if ((bool)_activeFX)
		{
			_activeFX.SetActiveRecursively(true);
		}
	}

	private void OnPointOwnerChanged(int ownerTeamId)
	{
		ResetActiveFX((ownerTeamId < 0) ? null : CaptureFX[ownerTeamId]);
	}

	private void OnPointCaptured(int ownerTeamId)
	{
		ResetActiveFX((ownerTeamId < 0) ? null : OwnFX[ownerTeamId]);
	}

	private void OnPointStartCapture(int ownerTeamId)
	{
		ResetActiveFX((ownerTeamId < 0) ? null : CaptureFX[ownerTeamId]);
	}

	private void OnPointProgressChanged(float progress)
	{
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (_KOHGame != null)
		{
			VehiclesManager instance = VehiclesManager.instance;
			if (instance.playerVehicle != null && instance.playerVehicle.isActive && instance.playerVehicle.mainOwnerCollider == collider)
			{
				_KOHGame.PlayerEnterPoint();
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (_KOHGame != null)
		{
			VehiclesManager instance = VehiclesManager.instance;
			if (instance.playerVehicle != null && instance.playerVehicle.mainOwnerCollider == collider)
			{
				_KOHGame.PlayerLeavePoint();
			}
		}
	}
}
