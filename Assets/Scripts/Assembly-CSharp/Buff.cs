using UnityEngine;

public class Buff
{
	public int Stacks;

	protected float _lastTick;

	protected bool _isStarted;

	protected BuffSystem _buffSystem;

	protected GameObject _target;

	protected Vehicle _vehicle;

	protected bool _finished;

	private float _duration = 5f;

	private int _maxStack;

	private float _updateRate = 1f;

	private float _startTime;

	private float _effectScale = 1f;

	private Object _instigator;

	private bool _shouldUpdateFromStart = true;

	private bool _isVisible = true;

	private bool _isUpdatable;

	public float duration
	{
		get
		{
			return _duration;
		}
		set
		{
			_duration = value;
		}
	}

	public int maxStack
	{
		get
		{
			return _maxStack;
		}
		set
		{
			_maxStack = value;
		}
	}

	public float updateRate
	{
		get
		{
			return _updateRate;
		}
		set
		{
			_updateRate = value;
		}
	}

	public bool shouldUpdateFromStart
	{
		get
		{
			return _shouldUpdateFromStart;
		}
		protected set
		{
			_shouldUpdateFromStart = value;
		}
	}

	public bool isVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			_isVisible = value;
		}
	}

	public bool isUpdatable
	{
		get
		{
			return _isUpdatable;
		}
		set
		{
			if (_isUpdatable != value)
			{
				_isUpdatable = value;
				if (value)
				{
					_buffSystem.StartUpdate(this);
				}
			}
		}
	}

	public Object instigator
	{
		get
		{
			return _instigator;
		}
		protected set
		{
			_instigator = value;
		}
	}

	public float startTime
	{
		get
		{
			return _startTime;
		}
		protected set
		{
			_startTime = value;
		}
	}

	public float effectScale
	{
		get
		{
			return _effectScale;
		}
		set
		{
			_effectScale = value;
		}
	}

	public Buff()
	{
	}

	public Buff(BuffConf config)
	{
		_duration = config.Duration;
	}

	public void SetBuffSystem(BuffSystem buffSystem)
	{
		_buffSystem = buffSystem;
	}

	public virtual void Init(GameObject targetGO, Vehicle targetVehicle, Object instigator)
	{
		_target = targetGO;
		_vehicle = targetVehicle;
		_instigator = instigator;
	}

	public void IncreaseStack(int StackBonus)
	{
		int stacks = Stacks;
		Stacks = ((maxStack <= 0) ? (Stacks + StackBonus) : Mathf.Min(Stacks + StackBonus, maxStack));
		stacks = Stacks - stacks;
		OnNewStackCount(stacks);
	}

	public void DecreaseStack(int StackDebonus)
	{
		int stacks = Stacks;
		Stacks = Mathf.Max(Stacks - StackDebonus, 0);
		stacks = Stacks - stacks;
		OnNewStackCount(stacks);
	}

	public void ResetStacks()
	{
		int deltaStack = 1 - Stacks;
		Stacks = 1;
		OnNewStackCount(deltaStack);
	}

	public void IncreaseStack()
	{
		IncreaseStack(1);
	}

	public void DecreaseStack()
	{
		DecreaseStack(1);
	}

	public void StartBuff()
	{
		StartBuff(true);
	}

	public void StartBuff(bool resetStack)
	{
		if (IsRunning())
		{
			_startTime = Time.time;
			IncreaseStack();
			return;
		}
		_isStarted = true;
		_startTime = Time.time;
		if (duration > 0f)
		{
			isUpdatable = true;
		}
		if (resetStack)
		{
			ResetStacks();
		}
		OnStart();
		if ((bool)_buffSystem)
		{
			_buffSystem.InvokeBuffStarted(this);
		}
	}

	public virtual bool IsEnded()
	{
		return _finished || (_isStarted && duration > 0f && Time.time - _startTime > duration);
	}

	public virtual bool IsRunning()
	{
		return !_finished && _isStarted && (duration <= 0f || (duration > 0f && Time.time - _startTime < duration));
	}

	public virtual void Reset()
	{
		_finished = false;
		_isStarted = false;
		_startTime = 0f;
		Stacks = 0;
		_lastTick = 0f;
	}

	public void FinishBuff()
	{
		_finished = true;
		_isStarted = false;
		_startTime = 0f;
		isUpdatable = false;
		OnFinish();
		if ((bool)_buffSystem)
		{
			_buffSystem.InvokeBuffFinished(this);
		}
	}

	protected virtual void OnNewStackCount(int deltaStack)
	{
		if (shouldUpdateFromStart)
		{
			OnTick(1);
		}
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnFinish()
	{
	}

	protected virtual void OnTick(int tickCount)
	{
	}

	public virtual bool Update()
	{
		if (IsRunning() && isUpdatable && Time.time - _lastTick > 1f / _updateRate)
		{
			int tickCount = Mathf.FloorToInt((Time.time - _lastTick) / _updateRate);
			OnTick(tickCount);
			_lastTick = Time.time;
		}
		if (IsEnded())
		{
			_buffSystem.RemoveBuff(this, false);
		}
		return isUpdatable;
	}

	public virtual void Reactivate()
	{
		if (IsRunning())
		{
			OnNewStackCount(0);
		}
	}

	public virtual void GetModifyInfo(BuffModifyInfo baseInfo, ref BuffModifyInfo info)
	{
	}
}
