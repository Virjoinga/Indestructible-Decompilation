using UnityEngine;

public class VehicleTeamIndicator : MonoBehaviour
{
	private Transform _transform;

	private void Start()
	{
		_transform = GetComponent<Transform>();
		Vehicle component = _transform.parent.GetComponent<Vehicle>();
		if (component != null)
		{
			MatchPlayer player = component.player;
			if (player != null && 0 <= player.teamID && player != (IDTGame.Instance as MultiplayerGame).localPlayer)
			{
				Color teamColor = MonoSingleton<Player>.Instance.GetTeamColor(player.teamID);
				teamColor.a = 0.5f;
				GetComponent<UISingleSprite>().SetColor(teamColor);
				return;
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		_transform.rotation = Quaternion.identity;
	}
}
