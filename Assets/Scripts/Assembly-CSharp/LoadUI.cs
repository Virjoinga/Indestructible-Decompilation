using UnityEngine;

public class LoadUI : MonoBehaviour
{
	private void Awake()
	{
		Application.LoadLevelAdditive("GameplayGUIScene");
	}
}
