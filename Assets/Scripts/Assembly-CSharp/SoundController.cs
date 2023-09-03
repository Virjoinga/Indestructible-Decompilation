using UnityEngine;

public class SoundController : MonoBehaviour
{
	private AudioHelper _audioHelper;

	private void Start()
	{
		AudioSource component = GetComponent<AudioSource>();
		if (component != null)
		{
			_audioHelper = new AudioHelper(component, false, true);
		}
	}

	private void OnDestroy()
	{
		if (_audioHelper != null)
		{
			_audioHelper.Dispose();
		}
	}
}
