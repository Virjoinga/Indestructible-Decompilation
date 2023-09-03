using Glu.Localization;
using UnityEngine;

public class DialogPause : UIDialog
{
	public UIStateToggleBtn MusicButton;

	public UIStateToggleBtn SoundButton;

	private static int _instanceCount;

	public static bool exists
	{
		get
		{
			return 0 < _instanceCount;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_instanceCount++;
		MusicButton.SetInputDelegate(InputDelegate);
		SoundButton.SetInputDelegate(InputDelegate);
	}

	private void OnDestroy()
	{
		_instanceCount--;
	}

	private void UpdateButtonState(UIStateToggleBtn button, bool on)
	{
		if (on)
		{
			button.Text = Strings.GetString("IDS_PAUSE_DIALOG_BUTTON_ON");
			button.SetToggleState("On", true);
		}
		else
		{
			button.Text = Strings.GetString("IDS_PAUSE_DIALOG_BUTTON_OFF");
			button.SetToggleState("Off", true);
		}
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

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		Close();
	}

	private void OnLeaveButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (!MonoSingleton<Player>.Instance.Tutorial.IsFirstGamePlayed())
		{
			AStats.MobileAppTracking.TrackAction("tutorial_complete");
		}
		MonoSingleton<Player>.Instance.Tutorial.SetFirstGamePlayed();
		IDTGame instance = IDTGame.Instance;
		GameAnalytics.EventMultiplayerGameLeft(instance);
		GameAnalytics.EventTutorialMatchFinished("Left");
		if (instance is SingleGame && !instance.IsBossFight)
		{
			VehiclesManager.instance.playerVehicle.destructible.Die(DestructionReason.Disconnect);
		}
		else
		{
			MonoSingleton<Player>.Instance.Statistics.TotalGamesPlayed++;
			MonoSingleton<Player>.Instance.ConsumeConsumable();
			MonoSingleton<Player>.Instance.Save();
			MonoSingleton<Player>.Instance.StartMatchLeftLevel("GarageScene");
		}
		Close();
	}

	public override void Activate()
	{
		base.Activate();
		SettingsController instance = MonoSingleton<SettingsController>.Instance;
		instance.MuteSound(true);
		UpdateButtonState(MusicButton, instance.MusicEnabled);
		UpdateButtonState(SoundButton, instance.SoundEnabled);
		IDTGame instance2 = IDTGame.Instance;
		if (instance2 != null)
		{
			instance2.PauseMenuActivated();
		}
	}

	public override void Close()
	{
		base.Close();
		MonoSingleton<SettingsController>.Instance.MuteSound(false);
		IDTGame instance = IDTGame.Instance;
		if (instance != null)
		{
			instance.PauseMenuDeactivated();
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Close();
		}
	}
}
