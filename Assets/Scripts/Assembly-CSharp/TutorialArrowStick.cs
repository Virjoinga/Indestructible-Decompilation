using UnityEngine;

public class TutorialArrowStick : MonoBehaviour
{
	public enum Kind
	{
		Drive = 0,
		Fire = 1
	}

	public StickScript Stick;

	public Kind StickKind;

	private void Awake()
	{
		if (StickKind == Kind.Fire)
		{
			if (MonoSingleton<Player>.Instance.Tutorial.IsStickTapFire())
			{
				Done();
			}
		}
		else if (StickKind == Kind.Drive && MonoSingleton<Player>.Instance.Tutorial.IsStickTapDrive())
		{
			Done();
		}
	}

	private void Done()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (Stick.GetVector() != Vector2.zero)
		{
			if (StickKind == Kind.Fire)
			{
				MonoSingleton<Player>.Instance.Tutorial.SetStickTapFire();
			}
			else if (StickKind == Kind.Drive)
			{
				MonoSingleton<Player>.Instance.Tutorial.SetStickTapDrive();
			}
			MonoSingleton<Player>.Instance.Save();
			Done();
		}
	}
}
