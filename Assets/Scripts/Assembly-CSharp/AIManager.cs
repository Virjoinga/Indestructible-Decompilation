using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
	protected class ProtectionPoint
	{
		public Vector3 Center = Vector3.zero;

		public float Radius;

		public ProtectionPoint()
		{
		}

		public ProtectionPoint(Vector3 center, float radius)
		{
			Center = center;
			Radius = radius;
		}
	}

	protected class TeamInfo
	{
		public List<ProtectionPoint> ProtectionPoints = new List<ProtectionPoint>();

		public LinkedList<AIMoveAndFireController> AICarsList = new LinkedList<AIMoveAndFireController>();
	}

	protected const int _teamsCount = 3;

	protected int _aiGeneralLayer;

	protected int _playerGeneralLayer;

	protected TeamInfo[] _teamsInfo = new TeamInfo[3];

	public float TrespasserMaxAttackDist = 35f;

	public float ProtectionUpdatePeriod = 0.3f;

	public float ReturnToBasePeriod = 0.6f;

	private bool _returnCheckEnabled;

	private bool _protectionPointsCheckEnabled;

	protected int GetAITeamByLayer(int layer)
	{
		int num = layer - _aiGeneralLayer;
		if (num >= 0 && num < 3)
		{
			return num;
		}
		return -1;
	}

	protected int GetPlayerTeamByLayer(int layer)
	{
		int num = layer - _playerGeneralLayer;
		if (num >= 0 && num < 3)
		{
			return num;
		}
		return -1;
	}

	private void RegisterProtectionPoints()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("AIProtection");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			int aITeamByLayer = GetAITeamByLayer(gameObject.layer);
			if (aITeamByLayer >= 0)
			{
				AIProtectionPoint component = gameObject.GetComponent<AIProtectionPoint>();
				if ((bool)component)
				{
					_teamsInfo[aITeamByLayer].ProtectionPoints.Add(new ProtectionPoint(gameObject.transform.position, component.Radius));
				}
			}
			Object.Destroy(gameObject);
		}
	}

	protected void SubscribeToVehiclesManager()
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.vehicleDeactivatedEvent += OnVehicleDeactivated;
			instance.vehicleActivatedEvent += OnVehicleActivated;
		}
	}

	protected void UnsubscribeFromVehiclesManager()
	{
		VehiclesManager instance = VehiclesManager.instance;
		if (instance != null)
		{
			instance.vehicleDeactivatedEvent -= OnVehicleDeactivated;
			instance.vehicleActivatedEvent -= OnVehicleActivated;
		}
	}

	protected virtual void Awake()
	{
		_aiGeneralLayer = LayerMask.NameToLayer("AI");
		_playerGeneralLayer = LayerMask.NameToLayer("Player");
		for (int i = 0; i < 3; i++)
		{
			_teamsInfo[i] = new TeamInfo();
		}
	}

	private void Start()
	{
		IDTGame instance = IDTGame.Instance;
		if (instance != null)
		{
			SubscribeToVehiclesManager();
			StartCoroutine(DelayedInit(instance));
		}
	}

	private IEnumerator DelayedInit(IDTGame game)
	{
		do
		{
			yield return null;
		}
		while (!game.isGameStarted);
		InitManager();
		if (PhotonNetwork.isMasterClient)
		{
			StartManager();
		}
	}

	protected virtual void InitManager()
	{
		RegisterProtectionPoints();
	}

	protected virtual void StartManager()
	{
		EnableProtectionPoints(true);
		EnableCheckReturnToBase(true);
	}

	protected void EnableProtectionPoints(bool enable)
	{
		if (enable)
		{
			if (!_protectionPointsCheckEnabled)
			{
				StartCoroutine("CheckProtectionPoints");
			}
			_protectionPointsCheckEnabled = true;
		}
		else
		{
			if (_protectionPointsCheckEnabled)
			{
				StartCoroutine("CheckProtectionPoints");
			}
			_protectionPointsCheckEnabled = false;
		}
	}

	protected void EnableCheckReturnToBase(bool enable)
	{
		if (enable)
		{
			if (!_returnCheckEnabled)
			{
				StartCoroutine("CheckReturnToBase");
			}
			_returnCheckEnabled = true;
		}
		else
		{
			if (_returnCheckEnabled)
			{
				StartCoroutine("CheckReturnToBase");
			}
			_returnCheckEnabled = false;
		}
	}

	private void OnVehicleActivated(Vehicle vehicle)
	{
		VehicleActivated(vehicle);
	}

	private void OnVehicleDeactivated(Vehicle vehicle)
	{
		VehicleDeactivated(vehicle);
	}

	protected void OnVehiclePlayerChanged(Vehicle vehicle)
	{
		if (vehicle.player != null)
		{
			int teamIdx = vehicle.player.teamID + 1;
			AddvehicleToAIList(vehicle, teamIdx);
			vehicle.playerChangedEvent -= OnVehiclePlayerChanged;
		}
	}

	protected virtual void VehicleActivated(Vehicle vehicle)
	{
		int num = ((vehicle.player == null) ? GetAITeamByLayer(vehicle.gameObject.layer) : (vehicle.player.teamID + 1));
		if (num < 0)
		{
			vehicle.playerChangedEvent += OnVehiclePlayerChanged;
		}
		else
		{
			AddvehicleToAIList(vehicle, num);
		}
	}

	protected void AddvehicleToAIList(Vehicle vehicle, int teamIdx)
	{
		AIMoveAndFireController component = vehicle.GetComponent<AIMoveAndFireController>();
		if ((bool)component)
		{
			_teamsInfo[teamIdx].AICarsList.AddLast(component);
		}
	}

	protected virtual void VehicleDeactivated(Vehicle vehicle)
	{
		AIMoveAndFireController component = vehicle.GetComponent<AIMoveAndFireController>();
		TeamInfo[] teamsInfo = _teamsInfo;
		foreach (TeamInfo teamInfo in teamsInfo)
		{
			teamInfo.AICarsList.Remove(component);
		}
	}

	protected void UpdateAITeamsInfo(AIMoveAndFireController ai)
	{
		if ((bool)ai)
		{
			TeamInfo[] teamsInfo = _teamsInfo;
			foreach (TeamInfo teamInfo in teamsInfo)
			{
				teamInfo.AICarsList.Remove(ai);
			}
			int aITeamByLayer = GetAITeamByLayer(ai.gameObject.layer);
			if (aITeamByLayer >= 0)
			{
				_teamsInfo[aITeamByLayer].AICarsList.AddLast(ai);
			}
		}
	}

	protected IEnumerator CheckProtectionPoints()
	{
		yield return null;
		MultiplayerGame mpGame = IDTGame.Instance as MultiplayerGame;
		while (true)
		{
			if (mpGame != null)
			{
				foreach (GamePlayer player in mpGame.players)
				{
					Transform vehicleTransform = player.vehicle.transform;
					if (vehicleTransform == null)
					{
						continue;
					}
					Vector3 pos = vehicleTransform.position;
					int oppositeId = 1 - player.teamID;
					int idx = oppositeId + 1;
					if (idx >= 3)
					{
						continue;
					}
					List<ProtectionPoint> pointsList2 = _teamsInfo[idx].ProtectionPoints;
					LinkedList<AIMoveAndFireController> carsList2 = _teamsInfo[idx].AICarsList;
					foreach (ProtectionPoint pp2 in pointsList2)
					{
						if ((pos - pp2.Center).sqrMagnitude < pp2.Radius * pp2.Radius)
						{
							AttackTrespasser(vehicleTransform, carsList2);
						}
					}
				}
			}
			int[] enemyMasks = new int[3]
			{
				1 << LayerMask.NameToLayer("AI"),
				1 << LayerMask.NameToLayer("AITeam1"),
				1 << LayerMask.NameToLayer("AITeam0")
			};
			for (int i = 0; i < 3; i++)
			{
				List<ProtectionPoint> pointsList = _teamsInfo[i].ProtectionPoints;
				LinkedList<AIMoveAndFireController> carsList = _teamsInfo[i].AICarsList;
				int enemyMask = enemyMasks[i];
				foreach (ProtectionPoint pp in pointsList)
				{
					Collider[] targets = Physics.OverlapSphere(pp.Center, pp.Radius, enemyMask);
					for (int j = 0; j < targets.Length; j++)
					{
						AttackTrespasser(targets[j].gameObject.transform, carsList);
					}
				}
			}
			yield return new WaitForSeconds(ProtectionUpdatePeriod);
		}
	}

	protected IEnumerator CheckReturnToBase()
	{
		yield return null;
		while (true)
		{
			for (int i = 0; i < 3; i++)
			{
				LinkedList<AIMoveAndFireController> carsList = _teamsInfo[i].AICarsList;
				foreach (AIMoveAndFireController ai in carsList)
				{
					if (ai.gameObject.active && !ai.IsOnChase && !ai.IsMoveToTarget && !ai.IsFire)
					{
						ai.SetFireTarget(null);
						ai.SetMoveTarget(ai.HomePoint);
						ai.StartMovement();
					}
				}
			}
			yield return new WaitForSeconds(ReturnToBasePeriod);
		}
	}

	protected bool AttackTrespasser(Transform target, LinkedList<AIMoveAndFireController> carsList)
	{
		AIMoveAndFireController aIMoveAndFireController = null;
		float num = 10f;
		float num2 = 5f;
		float num3 = TrespasserMaxAttackDist * TrespasserMaxAttackDist / (num * num2);
		foreach (AIMoveAndFireController cars in carsList)
		{
			if ((bool)cars && cars.gameObject.active)
			{
				float sqrMagnitude = (target.position - cars.transform.position).sqrMagnitude;
				float num4 = sqrMagnitude / ((!cars.IsFire) ? 10f : 1f);
				num4 /= ((!cars.IsMoveToTarget) ? 5f : 1f);
				if (num4 < num3)
				{
					num3 = num4;
					aIMoveAndFireController = cars;
				}
			}
		}
		if (aIMoveAndFireController != null)
		{
			aIMoveAndFireController.SetMoveTarget(target.position);
			aIMoveAndFireController.SetFireTarget(target);
			aIMoveAndFireController.StartMovement();
		}
		return false;
	}
}
