using UnityEngine;

public class TmpAnimation : CachedTmpObject
{
	private Animation _animation;

	protected override void Awake()
	{
		_animation = GetComponentInChildren<Animation>();
		base.Awake();
	}

	public override void Activate()
	{
		base.Activate();
		_animation.Play();
	}
}
