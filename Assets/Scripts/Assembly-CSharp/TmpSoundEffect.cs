using UnityEngine;

public class TmpSoundEffect : CachedTmpObject
{
	private AudioHelper _audioHelper;

	protected override void Awake()
	{
		base.Awake();
		_audioHelper = new AudioHelper(GetComponent<AudioSource>(), false, false);
	}

	private void OnDestroy()
	{
		_audioHelper.Dispose();
	}

	public override void Activate()
	{
		base.Activate();
		_audioHelper.PlayIfEnabled();
	}
}
