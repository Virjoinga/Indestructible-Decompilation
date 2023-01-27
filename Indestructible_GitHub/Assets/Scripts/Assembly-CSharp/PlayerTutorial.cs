using System.Collections.Generic;
using System.Xml;

public class PlayerTutorial
{
	private class Step
	{
		public string Name;

		public bool Passed;

		public Step(string name, bool passed)
		{
			Name = name;
			Passed = passed;
		}
	}

	private const string SAVE_TUTORIAL_VEHICLE_CHOOSEN = "vehicleChoosen";

	private const string SAVE_TUTORIAL_PLAY_BUTTON_TAP = "playButtonTap";

	private const string SAVE_TUTORIAL_FIRST_GAME_PLAYED = "firstGamePlayed";

	private const string SAVE_TUTORIAL_STICK_TAP_DRIVE = "stickTapDrive";

	private const string SAVE_TUTORIAL_STICK_TAP_FIRE = "stickTapFire";

	private const string SAVE_TUTORIAL_NICKNAME_SET = "nicknameSet";

	private const string SAVE_TUTORIAL_CUSTOMIZE_TAP = "customizeTap";

	private const string SAVE_TUTORIAL_ABILITY_BUTTON_TAP = "abilityButtonTap";

	private const string SAVE_TUTORIAL_NEW_VEHICLES_BUTTON_TAP = "newVehiclesButtonTap";

	private const string SAVE_TUTORIAL_LIMITED_BUNDLE_BUTTON_TAP = "limitedBundleButtonTap";

	private List<Step> _tutorial = new List<Step>
	{
		new Step("vehicleChoosen", false),
		new Step("playButtonTap", false),
		new Step("firstGamePlayed", false),
		new Step("stickTapDrive", false),
		new Step("stickTapFire", false),
		new Step("nicknameSet", false),
		new Step("customizeTap", false),
		new Step("abilityButtonTap", false),
		new Step("newVehiclesButtonTap", false),
		new Step("limitedBundleButtonTap", false)
	};

	public bool IsVehicleChoosen()
	{
		return Get("vehicleChoosen");
	}

	public void SetVehicleChoosen()
	{
		Set("vehicleChoosen", true);
	}

	public bool IsPlayButtonTap()
	{
		return Get("playButtonTap");
	}

	public void SetPlayButtonTap()
	{
		Set("playButtonTap", true);
	}

	public bool IsNewVehiclesButtonTap()
	{
		return Get("newVehiclesButtonTap");
	}

	public void SetNewVehiclesButtonTap()
	{
		Set("newVehiclesButtonTap", true);
	}

	public bool IsLimitedBundleButtonTap()
	{
		return Get("limitedBundleButtonTap");
	}

	public void SetLimitedBundleButtonTap()
	{
		Set("limitedBundleButtonTap", true);
	}

	public bool IsAbilityButtonTap()
	{
		return Get("abilityButtonTap");
	}

	public void SetAbilityButtonTap()
	{
		Set("abilityButtonTap", true);
	}

	public bool IsFirstGamePlayed()
	{
		return Get("firstGamePlayed");
	}

	public void SetFirstGamePlayed()
	{
		Set("firstGamePlayed", true);
	}

	public bool IsStickTapDrive()
	{
		return Get("stickTapDrive");
	}

	public void SetStickTapDrive()
	{
		Set("stickTapDrive", true);
	}

	public bool IsStickTapFire()
	{
		return Get("stickTapFire");
	}

	public void SetStickTapFire()
	{
		Set("stickTapFire", true);
	}

	public bool IsNicknameSet()
	{
		return Get("nicknameSet");
	}

	public void SetNicknameSet()
	{
		Set("nicknameSet", true);
	}

	public bool IsCustomizeTap()
	{
		return Get("customizeTap");
	}

	public void SetCustomizeTap()
	{
		Set("customizeTap", true);
	}

	private Step FindStep(string name)
	{
		return _tutorial.Find((Step a) => a.Name == name);
	}

	public void SetDefault()
	{
		SetAll(false);
	}

	public void SetPassed()
	{
		SetAll(true);
	}

	private void SetAll(bool passed)
	{
		foreach (Step item in _tutorial)
		{
			item.Passed = passed;
		}
	}

	public bool LoadXml(XmlElement root)
	{
		foreach (Step item in _tutorial)
		{
			item.Passed = XmlUtils.GetAttribute(root, item.Name, true);
		}
		return true;
	}

	public void SaveXml(XmlDocument document, XmlElement root)
	{
		foreach (Step item in _tutorial)
		{
			XmlUtils.SetAttribute(root, item.Name, item.Passed);
		}
	}

	public bool Get(string name)
	{
		Step step = FindStep(name);
		if (step != null)
		{
			return step.Passed;
		}
		return false;
	}

	public void Set(string name, bool passed)
	{
		Step step = FindStep(name);
		if (step != null)
		{
			step.Passed = passed;
		}
	}
}
