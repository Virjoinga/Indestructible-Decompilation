using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadUI : MonoBehaviour
{
	private void Awake()
	{
		SceneManager.LoadSceneAsync("GameplayGUIScene");
	}
}
