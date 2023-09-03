using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
	private Destructible _targetDestructible;

	private UIProgressBar _progress;

	private Color _color = Color.white;

	public float StayTime = 2f;

	public float HideTime = 1f;

	private float _time;

	private float _totalTime;

	private void Awake()
	{
		_progress = GetComponentInChildren<UIProgressBar>();
	}

	public void Initialize(GameObject o)
	{
		_targetDestructible = o.GetComponent<Destructible>();
		_totalTime = StayTime + HideTime;
		Reset();
	}

	private void Reset()
	{
		_color.a = 0f;
		_time = _totalTime;
		_progress.Value = 1f;
		_progress.SetColor(_color);
	}

	private void UpdateHealth()
	{
		float maxHP = _targetDestructible.GetMaxHP();
		float hp = _targetDestructible.hp;
		float num = hp / maxHP;
		if (num != _progress.Value)
		{
			_progress.Value = num;
			_time = 0f;
			if (_color.a == 0f)
			{
			}
			_color.a = 1f;
			_progress.SetColor(_color);
		}
	}

	private void UpdateColor()
	{
		if (_time < _totalTime)
		{
			_time += Time.deltaTime;
			if (_time > _totalTime)
			{
				_time = _totalTime;
				_color.a = 0f;
			}
			if (_time > StayTime && _time < _totalTime)
			{
				float num = _time - StayTime;
				_color.a = 1f - num / HideTime;
				_progress.SetColor(_color);
			}
		}
	}

	private void LateUpdate()
	{
		UpdateHealth();
		UpdateColor();
	}
}
