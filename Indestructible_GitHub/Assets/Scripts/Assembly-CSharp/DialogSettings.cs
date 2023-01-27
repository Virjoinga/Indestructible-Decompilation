using Glu.Localization;
using UnityEngine;

public class DialogSettings : UIDialog
{
	public UIStateToggleBtn MusicButton;

	public UIStateToggleBtn SoundButton;

	public UIStateToggleBtn NotificationsButton;

	private bool _lowestDialogLevel = true;

	private void UpdateButtonState(UIStateToggleBtn button, bool on)
	{
		if (on)
		{
			button.Text = Strings.GetString("IDS_SETTINGS_DIALOG_BUTTON_ON");
			button.SetToggleState("On", true);
		}
		else
		{
			button.Text = Strings.GetString("IDS_SETTINGS_DIALOG_BUTTON_OFF");
			button.SetToggleState("Off", true);
		}
	}

	private void OnMusicButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		bool on = MonoSingleton<SettingsController>.Instance.ToggleMusic();
		UpdateButtonState(MusicButton, on);
	}

	private void OnSoundButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		bool on = MonoSingleton<SettingsController>.Instance.ToggleSound();
		UpdateButtonState(SoundButton, on);
	}

	private void OnNotificationsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		bool on = MonoSingleton<SettingsController>.Instance.ToggleNotifications();
		UpdateButtonState(NotificationsButton, on);
	}

	private void OnCreditsButtonTap()
	{
		_lowestDialogLevel = false;
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Dialogs.CreditsDialog();
	}

	private void OnAboutButtonTap()
	{
		_lowestDialogLevel = false;
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Dialogs.AboutDialog();
	}

	private void OnRestorePurchasesButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		AInAppPurchase.RequestPendingPurchases();
	}

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void OnServerSettingsButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Dialogs.SelectServer();
	}

	private void InvokeButtonTap(UIStateToggleBtn button)
	{
		if (button.scriptWithMethodToInvoke != null && button.methodToInvoke != null)
		{
			button.scriptWithMethodToInvoke.Invoke(button.methodToInvoke, button.delay);
		}
	}

	private void InputDelegate(ref POINTER_INFO ptr)
	{
		if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
		{
			UIStateToggleBtn component = ptr.targetObj.gameObject.GetComponent<UIStateToggleBtn>();
			if (component != null)
			{
				InvokeButtonTap(component);
			}
			ptr.evt = POINTER_INFO.INPUT_EVENT.NO_CHANGE;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		MusicButton.SetInputDelegate(InputDelegate);
		SoundButton.SetInputDelegate(InputDelegate);
		NotificationsButton.SetInputDelegate(InputDelegate);
	}

	public override void Activate()
	{
		base.Activate();
		UpdateButtonState(MusicButton, MonoSingleton<SettingsController>.Instance.MusicEnabled);
		UpdateButtonState(SoundButton, MonoSingleton<SettingsController>.Instance.SoundEnabled);
		UpdateButtonState(NotificationsButton, MonoSingleton<SettingsController>.Instance.NotificationsEnabled);
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			if (_lowestDialogLevel)
			{
				OnCloseButtonTap();
			}
			else
			{
				_lowestDialogLevel = true;
			}
		}
	}
}
