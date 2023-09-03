using UnityEngine;

public class AudioListenerController : MonoSingleton<AudioListenerController>
{
	protected override void Awake()
	{
		base.Awake();
		base.gameObject.AddComponent<AudioListener>();
	}

	private void OnLevelWasLoaded()
	{
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
	}
}
