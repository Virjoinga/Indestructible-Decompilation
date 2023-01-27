using System.Collections.Generic;

public class ModStackFloat
{
	private struct ModWithPriority
	{
		public int Priority;

		public ValueModifierDelegate Modifier;

		public ModWithPriority(int priority, ValueModifierDelegate modifier)
		{
			Priority = priority;
			Modifier = modifier;
		}
	}

	public delegate float ValueModifierDelegate(float prevValue, float baseValue);

	public delegate float GetBaseValueDelegate();

	public delegate void ValueModifiedDelegate(float calculatedValue);

	private GetBaseValueDelegate _getBaseValueDelegate;

	private List<ModWithPriority> _modifiers = new List<ModWithPriority>();

	public float BaseValue
	{
		get
		{
			return _getBaseValueDelegate();
		}
	}

	public float Value
	{
		get
		{
			return RecalculateValue();
		}
	}

	public event ValueModifiedDelegate ValueRecalculatedEvent;

	public ModStackFloat(GetBaseValueDelegate baseValueDelegate)
	{
		_getBaseValueDelegate = baseValueDelegate;
	}

	public ModStackFloat(GetBaseValueDelegate baseValueDelegate, ValueModifiedDelegate setModifiedvalDelegate)
	{
		_getBaseValueDelegate = baseValueDelegate;
		this.ValueRecalculatedEvent = setModifiedvalDelegate;
	}

	public void SetValue(float finalValue)
	{
		if (this.ValueRecalculatedEvent != null)
		{
			this.ValueRecalculatedEvent(finalValue);
		}
	}

	private int ModWithPriorityCompare(ModWithPriority left, ModWithPriority right)
	{
		return left.Priority - right.Priority;
	}

	public void AddValueModifier(ValueModifierDelegate mod, int priority)
	{
		_modifiers.Add(new ModWithPriority(priority, mod));
		_modifiers.Sort(ModWithPriorityCompare);
		Recalculate();
	}

	public void RemoveValueModifier(ValueModifierDelegate mod)
	{
		for (int i = 0; i < _modifiers.Count; i++)
		{
			if (_modifiers[i].Modifier == mod)
			{
				_modifiers.RemoveAt(i);
				break;
			}
		}
	}

	private float RecalculateValue()
	{
		float baseValue = BaseValue;
		float num = baseValue;
		for (int i = 0; i < _modifiers.Count; i++)
		{
			num = _modifiers[i].Modifier(num, baseValue);
		}
		return num;
	}

	public void Recalculate()
	{
		SetValue(RecalculateValue());
	}
}
