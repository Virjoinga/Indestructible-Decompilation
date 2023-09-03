using UnityEngine;

public class DialogSelectServer : UIDialog
{
	public UIStateButtonShade ButtonAutomatic;

	public UIStateButtonShade ButtonUnitedStates;

	public UIStateButtonShade ButtonEurope;

	public UIStateButtonShade ButtonAsia;

	private RegionServer.Kind _kind;

	private void OnCloseButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		MonoSingleton<SettingsController>.Instance.SetServerRegion(_kind);
		MonoSingleton<SettingsController>.Instance.Save();
		Close();
	}

	private void ResetButton(UIStateButtonShade button)
	{
		if (button.StateName != "Normal")
		{
			button.SetToggleState("Normal", true);
			button.spriteText.SetCharacterSize(7f);
		}
	}

	private void ToggleButton(UIStateButtonShade button)
	{
		ResetButton(ButtonAutomatic);
		ResetButton(ButtonUnitedStates);
		ResetButton(ButtonEurope);
		ResetButton(ButtonAsia);
		if (button.StateName == "Normal")
		{
			button.spriteText.SetCharacterSize(7.5f);
			button.SetToggleState("Disabled", true);
		}
	}

	private void OnAutomaticButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		_kind = RegionServer.Kind.Automatic;
		ToggleButton(ButtonAutomatic);
	}

	private void OnServerAsiaButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		_kind = RegionServer.Kind.Asia;
		ToggleButton(ButtonAsia);
	}

	private void OnServerEuropeButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		_kind = RegionServer.Kind.Europe;
		ToggleButton(ButtonEurope);
	}

	private void OnServerUnitedStatesButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		_kind = RegionServer.Kind.UnitedStates;
		ToggleButton(ButtonUnitedStates);
	}

	public override void Activate()
	{
		base.Activate();
		_kind = MonoSingleton<SettingsController>.Instance.ServerRegion;
		switch (_kind)
		{
		case RegionServer.Kind.Automatic:
			ToggleButton(ButtonAutomatic);
			break;
		case RegionServer.Kind.UnitedStates:
			ToggleButton(ButtonUnitedStates);
			break;
		case RegionServer.Kind.Europe:
			ToggleButton(ButtonEurope);
			break;
		case RegionServer.Kind.Asia:
			ToggleButton(ButtonAsia);
			break;
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			OnCloseButtonTap();
		}
	}
}
