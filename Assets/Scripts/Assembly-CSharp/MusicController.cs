using UnityEngine;

public class MusicController : MonoBehaviour
{
	public string MusicPath = string.Empty;

	private AudioHelper _audioHelper;

	private void Start()
	{
		if (string.IsNullOrEmpty(MusicPath))
		{
			return;
		}
		AudioClip audioClip = Resources.Load(MusicPath) as AudioClip;
		if (audioClip != null)
		{
			AudioSource audioSource = GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = base.gameObject.AddComponent<AudioSource>();
			}
			audioSource.clip = audioClip;
			_audioHelper = new AudioHelper(audioSource, true, true);
		}
	}

	private void OnDestroy()
	{
		_audioHelper.Dispose();
	}
}
