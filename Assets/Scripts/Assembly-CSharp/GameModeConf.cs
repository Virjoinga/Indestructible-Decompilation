using UnityEngine;

public abstract class GameModeConf : ScriptableObject
{
	public float respawnDelay = 10f;

	public int victoryScore = 3;

	public abstract void Configure(MultiplayerMatch match);

	public virtual void Configure(GameObject gameGO)
	{
	}

	protected virtual void Configure(IDTGame game)
	{
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		if (multiplayerGame != null)
		{
			multiplayerGame.respawnDelay = respawnDelay;
			multiplayerGame.victoryScore = victoryScore;
		}
	}

	public static void ConfigureScene(string gameType)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("GameSpec");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (!gameObject.name.ToLower().Contains(gameType))
			{
				gameObject.SetActiveRecursively(false);
				Object.Destroy(gameObject);
			}
		}
	}

	public static void ConfigureScene(string[] gameTypes)
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("GameSpec");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			string text = gameObject.name.ToLower();
			bool flag = true;
			foreach (string value in gameTypes)
			{
				if (text.Contains(value))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				gameObject.SetActiveRecursively(false);
				Object.Destroy(gameObject);
			}
		}
	}
}
