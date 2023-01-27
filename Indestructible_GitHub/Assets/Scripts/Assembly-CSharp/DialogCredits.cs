using UnityEngine;

public class DialogCredits : UIDialog
{
	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
