using UnityEngine;

public class ForcedUpdateExample : MonoBehaviour
{
	private void Start()
	{
		ForcedUpdate.Init("http://mskbarchives.glu.com/moscow-upload/testUnityTemplate/fu_data.xml", "1.0.0");
		ForcedUpdate.CheckUpdateStatus();
	}

	private void FixedUpdate()
	{
		if (ForcedUpdate.NeedToQuit())
		{
			Debug.Log("Must update from URL " + ForcedUpdate.GetUpdateURL());
			Application.Quit();
		}
		if (ForcedUpdate.NeedToUpdate())
		{
			Debug.Log("Should update from URL " + ForcedUpdate.GetUpdateURL());
		}
	}
}
