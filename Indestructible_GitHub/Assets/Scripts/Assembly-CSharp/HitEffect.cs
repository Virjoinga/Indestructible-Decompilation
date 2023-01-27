using UnityEngine;

public class HitEffect : PooledObject
{
	public HitParticleEffect particleEffect;

	private AudioHelper _audioHelper;

	public override void Activate(Vector3 pos)
	{
		ActivateInvisible(pos);
		particleEffect.Play(pos, Vector3.up);
	}

	public void Activate(Vector3 pos, Vector3 normal)
	{
		ActivateInvisible(pos);
		particleEffect.Play(pos, normal);
	}

	public void ActivateInvisible(Vector3 pos)
	{
		base.transform.localPosition = pos;
		_audioHelper.PlayIfEnabled();
	}

	public static void Activate(CachedObject effectObj, Vector3 pos, bool isVisible)
	{
		if (!isVisible)
		{
			HitEffect hitEffect = effectObj as HitEffect;
			if (hitEffect != null)
			{
				hitEffect.ActivateInvisible(pos);
				return;
			}
		}
		effectObj.Activate(pos);
	}

	protected override void Awake()
	{
		base.Awake();
		particleEffect.Use();
		_audioHelper = new AudioHelper(GetComponent<AudioSource>(), false, false);
	}

	private void OnDestroy()
	{
		_audioHelper.Dispose();
	}
}
