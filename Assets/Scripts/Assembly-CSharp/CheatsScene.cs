using UnityEngine;

public class CheatsScene : MonoBehaviour
{
	public GameObject CheatsShadePrefab;

	public GameObject CheatButtonPrefab;

	public GameObject CheatsButtons;

	public GameObject CheatsButton;

	public SpriteText BuildTagLabel;

	private bool _cheatsButtonVisible;

	private Transform _buttons;

	private Vector2 _coords;

	private void OnCheatsButtonTap()
	{
		bool flag = CheatsButtons.active;
		CheatsButtons.SetActiveRecursively(!flag);
	}

	private void Awake()
	{
		_buttons = CheatsButtons.GetComponent<Transform>();
		BuildTagLabel.Text = BuildTag.Get();
		_cheatsButtonVisible = true;
	}

	private void Start()
	{
		AddShade();
		AddCheats();
		CheatsButtons.SetActiveRecursively(false);
	}

	private void AddShade()
	{
		GameObject gameObject = (GameObject)Object.Instantiate(CheatsShadePrefab);
		UIDialogShade component = gameObject.GetComponent<UIDialogShade>();
		component.SetColor(new Color(0f, 0f, 0f, 0.5f));
		Transform component2 = gameObject.GetComponent<Transform>();
		component2.parent = _buttons;
		component2.localPosition = new Vector3(0f, 0f, 1f);
	}

	private void AddCheat(string label, CheatButton.OnTapDelegateType action)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(CheatButtonPrefab);
		Transform component = gameObject.GetComponent<Transform>();
		component.parent = _buttons;
		UIButton component2 = gameObject.GetComponent<UIButton>();
		float num = component2.width + 2f;
		float num2 = component2.height + 2f;
		CheatButton component3 = gameObject.GetComponent<CheatButton>();
		component3.OnTapDelegate = action;
		component3.Label.Text = label;
		Vector2 screenSize = UITools.GetScreenSize();
		float num3 = screenSize.x - num * 2f;
		float num4 = screenSize.y - num2 * 3f;
		int num5 = (int)(num4 / num2);
		float num6 = _coords.x * num;
		float num7 = _coords.y * num2;
		num6 -= num3 / 2f;
		num7 -= num4 / 2f - num2 / 2f;
		component.localPosition = new Vector3(num6, 0f - num7, 0f);
		_coords.y += 1f;
		if (_coords.y > (float)num5)
		{
			_coords.x += 1f;
			_coords.y = 0f;
		}
	}

	private void EnableRendering(Transform root, bool enable)
	{
		foreach (Transform item in root)
		{
			EnableRendering(item, enable);
		}
		Renderer component = root.GetComponent<Renderer>();
		if (!(component == null))
		{
			component.enabled = enable;
		}
	}

	private void AddCheats()
	{
		AddCheat("Soft +5000", delegate
		{
			Player instance3 = MonoSingleton<Player>.Instance;
			instance3.MoneySoft = (int)instance3.MoneySoft + 5000;
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Soft +50000", delegate
		{
			Player instance2 = MonoSingleton<Player>.Instance;
			instance2.MoneySoft = (int)instance2.MoneySoft + 50000;
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Hard +500", delegate
		{
			MonoSingleton<Player>.Instance.AddMoneyHard(500, "CREDIT_GC_PURCHASE", "cheat", "cheat");
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Soft -50%", delegate
		{
			Player instance = MonoSingleton<Player>.Instance;
			instance.MoneySoft = (int)instance.MoneySoft / 2;
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Hard -50%", delegate
		{
			MonoSingleton<Player>.Instance.SubMoneyHard((int)MonoSingleton<Player>.Instance.MoneyHard / 2, "DEBIT_IN_APP_PURCHASE", "cheat");
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("XP +75%", delegate
		{
			int min = 0;
			int max = 0;
			MonoSingleton<Player>.Instance.GetLevelExperience(ref min, ref max);
			MonoSingleton<Player>.Instance.AddExperience((max - min) * 3 / 4);
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Skill Pts +5", delegate
		{
			MonoSingleton<Player>.Instance.TalentPoints += 5;
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Skill Pts\nZero", delegate
		{
			MonoSingleton<Player>.Instance.TalentPoints = 0;
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Influence Points +100", delegate
		{
			MonoSingleton<Player>.Instance.AddInfluencePoints(100);
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Influence Points -5", delegate
		{
			MonoSingleton<Player>.Instance.AddInfluencePoints(-5);
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Reset Influence Points", delegate
		{
			MonoSingleton<Player>.Instance.InfluencePoints = 0;
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat(string.Format("Elo Rate +200"), delegate
		{
			MonoSingleton<Player>.Instance.EloRate += 200;
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat(string.Format("Elo Rate Reset"), delegate
		{
			MonoSingleton<Player>.Instance.EloRate = 1200;
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat(string.Format("Spent Fuel"), delegate
		{
			MonoSingleton<Player>.Instance.SelectedVehicle.Fuel.Spend();
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat(string.Format("Freeze Fuel"), delegate
		{
			MonoSingleton<Player>.Instance.SelectedVehicle.Fuel.Freeze();
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat(string.Format("Drain Fuel"), delegate
		{
			MonoSingleton<Player>.Instance.SelectedVehicle.Fuel.Drain();
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Reset Save", delegate
		{
			Application.LoadLevel("MenuScene");
			MonoSingleton<DialogsQueue>.Instance.Clear();
			MonoSingleton<Player>.Instance.LoadDefault();
			MonoSingleton<Player>.DestroyInstance();
		});
		AddCheat("Reset Achieves", delegate
		{
		});
		AddCheat("Skip Tutorial", delegate
		{
			MonoSingleton<Player>.Instance.Tutorial.SetPassed();
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Level Up", delegate
		{
			Dialogs.LevelUpDialog(1);
			Dialogs.RateMe();
		});
		AddCheat("Reset Challenges", delegate
		{
			MonoSingleton<Player>.Instance.Challenges.SetDefault();
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Next Challenges", delegate
		{
			MonoSingleton<Player>.Instance.Challenges.StartChallenge();
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Rewind 1 Battle", delegate
		{
			MonoSingleton<Player>.Instance.LastWonBossFight = Mathf.Max(MonoSingleton<Player>.Instance.LastWonBossFight - 1, -1);
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Forward 1 Battle", delegate
		{
			MonoSingleton<Player>.Instance.LastWonBossFight = Mathf.Min(MonoSingleton<Player>.Instance.LastWonBossFight + 1, BossFightConfiguration.Instance.BossFights.Length - 1);
			MonoSingleton<Player>.Instance.Save();
		});
		AddCheat("Cheats Btn\n> Hide", delegate(CheatButton button)
		{
			Transform component = CheatsButton.GetComponent<Transform>();
			_cheatsButtonVisible = !_cheatsButtonVisible;
			EnableRendering(component, _cheatsButtonVisible);
			if (_cheatsButtonVisible)
			{
				button.Label.Text = "Cheats Btn\n> Hide";
			}
			else
			{
				button.Label.Text = "Cheats Btn\n> Show";
			}
		});
	}
}
