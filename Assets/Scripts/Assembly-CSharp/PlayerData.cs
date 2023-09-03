using UnityEngine;

public class PlayerData : MonoBehaviour
{
	private static PlayerData m_instance;

	public static PlayerData Instance
	{
		get
		{
			if (m_instance == null)
			{
				GameObject gameObject = new GameObject("PlayerData");
				m_instance = gameObject.AddComponent<PlayerData>();
				Object.DontDestroyOnLoad(gameObject);
			}
			return m_instance;
		}
	}
}
