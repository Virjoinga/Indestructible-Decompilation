public class AnimatedNotificationText : AnimatedNotification
{
	public SpriteText Notification;

	private CachedObject _cachedObject;

	public void SetCache(CachedObject cachedObject)
	{
		_cachedObject = cachedObject;
	}

	public override void Activate()
	{
		base.Activate();
		Restart();
	}

	protected override void OnFinish()
	{
		if ((bool)_cachedObject)
		{
			_cachedObject.Deactivate();
		}
	}
}
