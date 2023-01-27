public class DialogTemplate : UIDialog
{
	public delegate void OnButtonTap(DialogTemplate parent);

	public SpriteText Title;

	public SpriteText Body;

	public UIButton LeftButton;

	public UIButton RightButton;

	public UIButton CloseButton;

	public OnButtonTap OnLeftButtonDelegate;

	public OnButtonTap OnRightButtonDelegate;

	public OnButtonTap OnCloseButtonDelegate;

	public OnButtonTap OnBackKeyDelegate;

	private bool _closeButton;

	protected override void Awake()
	{
		base.Awake();
		LeftButton.Text = string.Empty;
		RightButton.Text = string.Empty;
	}

	public void ShowCloseButton()
	{
		_closeButton = true;
	}

	public override void Activate()
	{
		base.Activate();
		bool flag = LeftButton.Text != string.Empty;
		bool flag2 = RightButton.Text != string.Empty;
		MonoUtils.SetActive(LeftButton, flag);
		MonoUtils.SetActive(RightButton, flag2);
		MonoUtils.SetActive(CloseButton, _closeButton);
	}

	private void OnLeftButtonTap()
	{
		if (OnLeftButtonDelegate != null)
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			OnLeftButtonDelegate(this);
		}
	}

	private void OnRightButtonTap()
	{
		if (OnRightButtonDelegate != null)
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			OnRightButtonDelegate(this);
		}
	}

	private void OnCloseButtonTap()
	{
		if (OnCloseButtonDelegate != null)
		{
			MonoSingleton<UISounds>.Instance.Play(UISounds.Type.Click);
			OnCloseButtonDelegate(this);
		}
	}

	private void Update()
	{
		if (MonoSingleton<GameController>.Instance.BackKeyReleased())
		{
			if (OnBackKeyDelegate != null)
			{
				OnBackKeyDelegate(this);
			}
			else if (OnCloseButtonDelegate != null)
			{
				OnCloseButtonDelegate(this);
			}
			else
			{
				Close();
			}
		}
	}
}
