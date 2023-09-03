using UnityEngine;

public class StasisBuff : Buff
{
	public float dragBonus = 1f;

	private Rigidbody _rb;

	private float rbDrag
	{
		get
		{
			return (!(_rb != null)) ? 0f : _rb.drag;
		}
		set
		{
			if (_rb != null)
			{
				_rb.drag = Mathf.Max(0f, value);
			}
		}
	}

	public StasisBuff(StasisConf config)
		: base(config)
	{
		dragBonus = config.dragBonus;
	}

	public override void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		base.Init(targetGO, targetVehicle, instigator);
		_rb = _target.GetComponent<Rigidbody>();
	}

	private void Start()
	{
		GlobalConfig instance = GlobalConfig.Instance;
		if (instance != null)
		{
			dragBonus = instance.StasisDebuffValue;
			base.duration = instance.StasisDebuffPeriod;
		}
	}

	protected override void OnStart()
	{
		rbDrag += dragBonus * base.effectScale;
		Debug.Log("New drag for vehicle: " + rbDrag);
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		rbDrag -= dragBonus * base.effectScale;
		Debug.Log("Old drag for vehicle: " + rbDrag);
	}
}
