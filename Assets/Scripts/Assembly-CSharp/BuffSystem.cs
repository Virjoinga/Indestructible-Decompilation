using System;
using System.Collections;
using System.Collections.Generic;
using Glu;
using UnityEngine;

public class BuffSystem : Glu.MonoBehaviour
{
	public class WeaponModsParams
	{
		public ModStackFloat DamageMod;

		public ModStackFloat FireIntervalMod;
	}

	public delegate void BuffChangedDelegate(Buff buff);

	private Dictionary<ModStackFloat.GetBaseValueDelegate, ModStackFloat> _modStackMap = new Dictionary<ModStackFloat.GetBaseValueDelegate, ModStackFloat>();

	private List<Buff> _permanentBuffs = new List<Buff>();

	private List<Buff> _temporaryBuffs = new List<Buff>();

	private List<ModStackFloat> _modsForUpdate = new List<ModStackFloat>();

	private Vehicle _targetVehicle;

	private int _updatableBuffCount;

	private Buff[] _updatableBuffs = new Buff[0];

	public event BuffChangedDelegate BuffAddedEvent;

	public event BuffChangedDelegate BuffRemovedEvent;

	public event BuffChangedDelegate BuffStartedEvent;

	public event BuffChangedDelegate BuffEndedEvent;

	private void Awake()
	{
		_targetVehicle = GetComponent<Vehicle>();
	}

	private void Start()
	{
		StartCoroutine(StartSuspensedBuffs());
		_targetVehicle.destructible.activatedEvent += Activated;
	}

	private void OnDestroy()
	{
		if (_targetVehicle != null && _targetVehicle.destructible != null)
		{
			_targetVehicle.destructible.activatedEvent -= Activated;
		}
	}

	private void Activated(Destructible destructible)
	{
		_modsForUpdate.Clear();
		RemoveAllBuffs(false);
		ReactivateAllBuffs();
	}

	public void StartUpdate(Buff buff)
	{
		if (_updatableBuffCount == _updatableBuffs.Length)
		{
			Array.Resize(ref _updatableBuffs, _updatableBuffCount + 4);
		}
		_updatableBuffs[_updatableBuffCount] = buff;
		if (_updatableBuffCount == 0)
		{
			base.enabled = true;
		}
		_updatableBuffCount++;
	}

	private void Update()
	{
		int num = 0;
		while (num != _updatableBuffCount)
		{
			if (!_updatableBuffs[num].Update())
			{
				_updatableBuffCount--;
				_updatableBuffs[num] = _updatableBuffs[_updatableBuffCount];
				_updatableBuffs[_updatableBuffCount] = null;
			}
			else
			{
				num++;
			}
		}
		if (_updatableBuffCount == 0)
		{
			base.enabled = false;
		}
	}

	private IEnumerator StartSuspensedBuffs()
	{
		yield return new WaitForEndOfFrame();
		foreach (Buff buff2 in _temporaryBuffs)
		{
			if (!buff2.IsRunning())
			{
				buff2.StartBuff(false);
			}
		}
		foreach (Buff buff in _permanentBuffs)
		{
			if (!buff.IsRunning())
			{
				buff.StartBuff(false);
			}
		}
	}

	public ModStackFloat SetupModStack(ModStackFloat.GetBaseValueDelegate baseValDelegate, ModStackFloat.ValueModifiedDelegate setModifiedvalDelegate, ModStackFloat.ValueModifierDelegate modifier, int priority)
	{
		if (baseValDelegate == null)
		{
			return null;
		}
		ModStackFloat modStackFloat = null;
		if (!_modStackMap.ContainsKey(baseValDelegate))
		{
			modStackFloat = new ModStackFloat(baseValDelegate, setModifiedvalDelegate);
			_modStackMap.Add(baseValDelegate, modStackFloat);
			modStackFloat.AddValueModifier(modifier, priority);
		}
		else
		{
			modStackFloat = _modStackMap[baseValDelegate];
			modStackFloat.AddValueModifier(modifier, priority);
		}
		return modStackFloat;
	}

	protected int FindBuffIndex<T>() where T : Buff
	{
		for (int i = 0; i < _temporaryBuffs.Count; i++)
		{
			if (_temporaryBuffs[i].GetType() == typeof(T))
			{
				return i;
			}
		}
		return -1;
	}

	protected int FindBuffIndex<T>(UnityEngine.Object instigator) where T : Buff
	{
		for (int i = 0; i < _temporaryBuffs.Count; i++)
		{
			if (_temporaryBuffs[i].instigator == instigator && _temporaryBuffs[i].GetType() == typeof(T))
			{
				return i;
			}
		}
		return -1;
	}

	protected int FindBuffIndex(RuntimeTypeHandle rtthBuff, UnityEngine.Object instigator)
	{
		for (int i = 0; i < _temporaryBuffs.Count; i++)
		{
			if (_temporaryBuffs[i].instigator == instigator && Type.GetTypeHandle(_temporaryBuffs[i]).Equals(rtthBuff))
			{
				return i;
			}
		}
		return -1;
	}

	public Buff GetBuff(int index)
	{
		if (index < 0 || index >= _temporaryBuffs.Count)
		{
			return null;
		}
		return _temporaryBuffs[index];
	}

	public T FindBuff<T>() where T : Buff
	{
		int index = FindBuffIndex<T>();
		return (T)GetBuff(index);
	}

	public T FindBuff<T>(UnityEngine.Object instigator) where T : Buff
	{
		int index = FindBuffIndex<T>(instigator);
		return (T)GetBuff(index);
	}

	public Buff FindBuff(RuntimeTypeHandle rtthBuff, UnityEngine.Object instigator)
	{
		int index = FindBuffIndex(rtthBuff, instigator);
		return GetBuff(index);
	}

	public int GetActiveBuffsCount()
	{
		return _temporaryBuffs.Count;
	}

	public T AddBuffSuspended<T>(UnityEngine.Object instigator) where T : Buff, new()
	{
		T val = FindBuff<T>(instigator);
		if (val == null)
		{
			val = new T();
			if (val == null)
			{
				return (T)null;
			}
			_temporaryBuffs.Add(val);
			_targetVehicle = _targetVehicle ?? GetComponent<Vehicle>();
			val.SetBuffSystem(this);
			val.Init(base.gameObject, _targetVehicle, instigator);
		}
		if (this.BuffAddedEvent != null)
		{
			this.BuffAddedEvent(val);
		}
		return val;
	}

	public T AddBuff<T>(UnityEngine.Object instigator) where T : Buff, new()
	{
		T val = AddBuffSuspended<T>(instigator);
		if (val != null)
		{
			val.StartBuff();
		}
		return val;
	}

	public Buff AddInstancedBuffSuspended(Buff buff, UnityEngine.Object instigator, bool permanent)
	{
		if (buff == null)
		{
			return null;
		}
		Buff buff2 = null;
		List<Buff> list = _permanentBuffs;
		if (!permanent)
		{
			buff2 = FindBuff(Type.GetTypeHandle(buff), instigator);
			list = _temporaryBuffs;
		}
		if (buff2 == null)
		{
			list.Add(buff);
			_targetVehicle = _targetVehicle ?? GetComponent<Vehicle>();
			buff.SetBuffSystem(this);
			buff.Init(base.gameObject, _targetVehicle, instigator);
			if (this.BuffAddedEvent != null)
			{
				this.BuffAddedEvent(buff);
			}
			return buff;
		}
		return buff2;
	}

	public Buff AddInstancedBuff(Buff buff, UnityEngine.Object instigator, bool permanent)
	{
		Buff buff2 = AddInstancedBuffSuspended(buff, instigator, permanent);
		if (buff2 != null)
		{
			buff2.StartBuff();
		}
		return buff2;
	}

	public void RemoveBuff(Buff buffInstance, bool permanent)
	{
		buffInstance.FinishBuff();
		((!permanent) ? _temporaryBuffs : _permanentBuffs).Remove(buffInstance);
	}

	public void RemoveBuff<T>(UnityEngine.Object instigator) where T : Buff
	{
		int num = FindBuffIndex<T>(instigator);
		if (num < 0 || num >= _temporaryBuffs.Count)
		{
			Debug.LogWarning("Trying to remove non existent buff");
			return;
		}
		_temporaryBuffs[num].FinishBuff();
		if (this.BuffRemovedEvent != null)
		{
			this.BuffRemovedEvent(_temporaryBuffs[num]);
		}
		_temporaryBuffs.RemoveAt(num);
	}

	public void RemoveAllBuffs(bool withPermanents)
	{
		foreach (Buff temporaryBuff in _temporaryBuffs)
		{
			temporaryBuff.FinishBuff();
		}
		_temporaryBuffs.Clear();
		if (!withPermanents)
		{
			return;
		}
		foreach (Buff permanentBuff in _permanentBuffs)
		{
			permanentBuff.FinishBuff();
		}
		_permanentBuffs.Clear();
	}

	public void ReactivateAllBuffs()
	{
		for (int i = 0; i < _temporaryBuffs.Count; i++)
		{
			_temporaryBuffs[i].Reactivate();
		}
		for (int j = 0; j < _permanentBuffs.Count; j++)
		{
			_permanentBuffs[j].Reactivate();
		}
	}

	private IEnumerator UpdateMods()
	{
		yield return new WaitForEndOfFrame();
		foreach (ModStackFloat mod in _modsForUpdate)
		{
			mod.Recalculate();
		}
		_modsForUpdate.Clear();
	}

	public void RecalculateValue(ModStackFloat modValue)
	{
		if (modValue != null)
		{
			if (!_modsForUpdate.Contains(modValue))
			{
				_modsForUpdate.Add(modValue);
			}
			if (_modsForUpdate.Count == 1)
			{
				StartCoroutine(UpdateMods());
			}
		}
	}

	public void InvokeBuffStarted(Buff buff)
	{
		if (this.BuffStartedEvent != null)
		{
			this.BuffStartedEvent(buff);
		}
	}

	public void InvokeBuffFinished(Buff buff)
	{
		if (this.BuffEndedEvent != null)
		{
			this.BuffEndedEvent(buff);
		}
	}
}
