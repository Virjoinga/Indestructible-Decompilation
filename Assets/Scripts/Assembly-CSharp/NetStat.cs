using UnityEngine;

public class NetStat : MonoBehaviour
{
	private Vehicle _observedVehicle;

	private int _ping;

	private float _latency;

	private SpriteText _spriteText;

	private MultiplayerMatch _match;

	private bool _shouldUpdateLatency;

	private void Start()
	{
		_spriteText = GetComponent<SpriteText>();
		MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
		if (multiplayerGame != null && multiplayerGame.match.isOnline)
		{
			_shouldUpdateLatency = true;
			_match = multiplayerGame.match;
		}
	}

	private void Update()
	{
		float num = _latency;
		if (_shouldUpdateLatency)
		{
			if (_observedVehicle == null || _observedVehicle.player == null)
			{
				foreach (MatchPlayer player in _match.players)
				{
					if (player != _match.localPlayer)
					{
						_observedVehicle = (player as GamePlayer).vehicle;
						if (_observedVehicle != null)
						{
							num = _observedVehicle.vehiclePhysics.serverMessageLatency;
							break;
						}
					}
				}
			}
			else
			{
				num = _observedVehicle.vehiclePhysics.serverMessageLatency;
			}
		}
		int ping = PhotonNetwork.GetPing();
		if (ping != _ping || 0.05f < Mathf.Abs(num - _latency))
		{
			_ping = ping;
			_latency = num;
			_spriteText.Text = string.Format("ping:{0}, latency:{1} master:{2}", ping, num, PhotonNetwork.isMasterClient);
		}
	}
}
