using UnityEngine;

public class AnimatedNotification : UINotification
{
	protected UIAnimation _animation;

	protected override void Awake()
	{
		base.Awake();
		_animation = GetComponent<UIAnimation>();
	}

	protected virtual void OnFinish()
	{
		Object.Destroy(base.gameObject);
	}

	protected virtual void Restart()
	{
		_animation.Rewind();
		_animation.Play();
	}

	protected virtual void Update()
	{
		if (_animation.Finished)
		{
			OnFinish();
		}
	}
}
