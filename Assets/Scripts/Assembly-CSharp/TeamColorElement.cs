using UnityEngine;

public class TeamColorElement : MonoBehaviour
{
	private void Start()
	{
		MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
		if (multiplayerGame != null)
		{
			GetComponent<SpriteRoot>().SetColor(MonoSingleton<Player>.Instance.GetTeamColor(multiplayerGame.localPlayer.teamID));
		}
	}
}
