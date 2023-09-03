using Glu.Localization;
using Glu.UnityBuildSystem;
using UnityEngine;

public class DialogAbout : UIDialog
{
	public SpriteText AboutText;

	private int _secret1;

	private int _secret2;

	protected override void Awake()
	{
		base.Awake();
		string @string = Strings.GetString("IDS_ABOUT_DIALOG_TEXT");
		AboutText.Text = string.Format(@string, BuildInfo.bundleVersion, Application.unityVersion);
	}

	private void OnSecretButton1Tap()
	{
		_secret1++;
		if (_secret1 * _secret2 == 2)
		{
			Dialogs.SecretDialog();
		}
	}

	private void OnSecretButton2Tap()
	{
		_secret2++;
	}

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
