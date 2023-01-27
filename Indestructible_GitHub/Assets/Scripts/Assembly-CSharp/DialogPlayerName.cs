using System.Collections;
using Glu.Localization;
using UnityEngine;

public class DialogPlayerName : UIDialog
{
	public UITextField NameEdit;

	public UIButton RightButton;

	public SpriteText FacebookText;

	public UIButton FacebookButton;

	private bool _focusSet;

	protected override void Awake()
	{
		base.Awake();
		NameEdit.AddFocusDelegate(OnFocusDelegate);
		NameEdit.AddValidationDelegate(OnValidateName);
		string @string = Strings.GetString("IDS_PLAYER_NAME_DIALOG_FACEBOOK_TEXT");
		FacebookText.Text = string.Format(@string, "\u001f");
	}

	public override void Activate()
	{
		base.Activate();
		NameEdit.Text = string.Empty;
		NameEdit.spriteText.SetCharacterSize(6f);
		NameEdit.spriteText.color = new Color(1f, 1f, 1f, 0.5f);
		NameEdit.spriteText.Text = Strings.GetString("IDS_PLAYER_NAME_DIALOG_TAP_TO_TYPE");
		RightButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		float num = 15f;
		Vector3 localPosition = _transform.localPosition;
		localPosition.y = num;
		_transform.localPosition = localPosition;
		localPosition = _shadeTransform.localPosition;
		localPosition.y = 0f - num;
		_shadeTransform.localPosition = localPosition;
		GameAnalytics.EventNameEnterPrompted();
		if (ASocial.Facebook.IsLoggedIn())
		{
			OnFacebookLoggedIn();
		}
		ISpriteMesh spriteMesh = NameEdit.spriteMesh;
		if (spriteMesh != null)
		{
			spriteMesh.Hide(true);
		}
	}

	private void OnFocusDelegate(UITextField field)
	{
		if (!_focusSet)
		{
			_focusSet = true;
			field.spriteText.Text = string.Empty;
			field.spriteText.Color = Color.white;
			field.spriteText.SetCharacterSize(10f);
		}
	}

	private string OnValidateName(UITextField field, string text, ref int insertionPoint)
	{
		if (text != null && text.Length > 10)
		{
			text = text.Substring(0, 10);
		}
		string s = string.Empty;
		if (MonoSingleton<Player>.Instance.ValidateName(text, ref s))
		{
			RightButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
		}
		else
		{
			RightButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		}
		return s;
	}

	private void OnRightButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		if (!string.IsNullOrEmpty(NameEdit.Text))
		{
			MonoSingleton<Player>.Instance.Tutorial.SetNicknameSet();
			MonoSingleton<Player>.Instance.Name = NameEdit.Text;
			MonoSingleton<Player>.Instance.Save();
			GameAnalytics.EventNameEntered(MonoSingleton<Player>.Instance.Name);
		}
		Object @object = Object.FindObjectOfType(typeof(GarageManager));
		GarageManager manager = @object as GarageManager;
		PanelGarage.StartSelectScene(manager);
		Close();
	}

	private void OnFacebookButtonTap()
	{
		MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
		GameAnalytics.EventFacebookButtonTap("DialogPlayerName");
		StopAllCoroutines();
		string[] permissions = new string[1] { string.Empty };
		ASocial.Facebook.Login(permissions);
		StartCoroutine(WaitLogin());
	}

	private string ValidateFacebookName(string firstName)
	{
		string s = string.Empty;
		MonoSingleton<Player>.Instance.ValidateName(firstName, ref s);
		if (s.Length > 10)
		{
			s = s.Substring(0, 10);
		}
		else if (s.Length < 3)
		{
			s = string.Empty;
		}
		return s;
	}

	private void OnFacebookUserName(string fullName, string firstName)
	{
		string text = ValidateFacebookName(firstName);
		if (string.IsNullOrEmpty(text))
		{
			Dialogs.FacebookNameIncorrect();
			ASocial.Facebook.Logout();
			FacebookButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
		}
		else
		{
			NameEdit.Text = text;
			NameEdit.spriteText.Color = Color.white;
			NameEdit.spriteText.SetCharacterSize(10f);
			RightButton.SetControlState(UIButton.CONTROL_STATE.NORMAL, true);
		}
	}

	private void OnFacebookLoggedIn()
	{
		FacebookButton.SetControlState(UIButton.CONTROL_STATE.DISABLED, true);
		bool facebookLoginRewarded = MonoSingleton<Player>.Instance.Statistics.FacebookLoginRewarded;
		GameAnalytics.EventFacebookLoggedIn("DialogPlayerName", facebookLoginRewarded);
		StartCoroutine(GetFBUserInfo());
		if (!facebookLoginRewarded)
		{
			MonoSingleton<Player>.Instance.Statistics.FacebookLoginRewarded = true;
			MonoSingleton<Player>.Instance.AddMoneyHard(10, "CREDIT_IN_GAME_AWARD", "Facebook Login", "FACEBOOK");
			MonoSingleton<Player>.Instance.Save();
		}
	}

	private IEnumerator WaitLogin()
	{
		do
		{
			yield return new WaitForSeconds(1f);
		}
		while (!ASocial.Facebook.IsLoggedIn());
		OnFacebookLoggedIn();
	}

	private IEnumerator GetFBUserInfo()
	{
		yield return new WaitForSeconds(1f);
		OnFacebookUserName(ASocial.Facebook.GetUserInfo("name"), ASocial.Facebook.GetUserInfo("first_name"));
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			Close();
		}
	}
}
