using UnityEngine;

public class NotificationSpawn : UINotification
{
	public SpriteText SpawnTimer;

	public UIAnimation SpawnAnimation;

	public UIAnimation GoAnimation;

	private float _time = 3f;

	private int _timeCached;

	private DualStickController _playerStickController;

	private BaseActiveAbility _playerActiveAbility;

	protected override void Start()
	{
		base.Start();
		SetTimerText();
		MonoUtils.SetActive(GoAnimation, false);
		if ((bool)VehiclesManager.instance.playerVehicle)
		{
			_playerStickController = VehiclesManager.instance.playerVehicle.GetComponent<DualStickController>();
			if ((bool)_playerStickController)
			{
				_playerStickController.enabled = false;
			}
			_playerActiveAbility = VehiclesManager.instance.playerVehicle.GetComponent<BaseActiveAbility>();
			if ((bool)_playerActiveAbility)
			{
				_playerActiveAbility.enabled = false;
			}
		}
	}

	private void SetTimerText()
	{
		int num = Mathf.CeilToInt(_time);
		if (num != _timeCached)
		{
			SpawnTimer.Text = num.ToString();
			SpawnAnimation.Rewind();
			SpawnAnimation.Play();
			_timeCached = num;
		}
	}

	private void Update()
	{
		if (_time > 0f)
		{
			_time -= Time.deltaTime;
			SetTimerText();
			if (_time <= 0f)
			{
				SpawnTimer.Text = string.Empty;
				MonoUtils.SetActive(GoAnimation, true);
				GoAnimation.Play();
				if ((bool)_playerStickController)
				{
					_playerStickController.enabled = true;
				}
				if ((bool)_playerActiveAbility)
				{
					_playerActiveAbility.enabled = true;
				}
			}
		}
		if (GoAnimation.Finished)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
