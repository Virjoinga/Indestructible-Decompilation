using System.Collections;
using UnityEngine;

public class PlayerBuffIndicator : CachedObject
{
	public SpriteText TimerText;

	public PackedSprite BuffIcon;

	private float _duration;

	private Buff _buff;

	private PlayerBuffsIndicators _indicators;

	private YieldInstruction _delay = new WaitForSeconds(1f);

	public Buff RepresentedBuff
	{
		get
		{
			return _buff;
		}
	}

	private IEnumerator UpdateRoutine()
	{
		do
		{
			UpdateText();
			float t2 = Time.time;
			yield return _delay;
			t2 = Time.time - t2;
			_duration -= t2;
			if (_buff != null)
			{
				_duration = _buff.startTime + _buff.duration;
				_duration -= Time.time;
			}
		}
		while (!(_duration <= 0f));
		_indicators.Deactivate(this);
	}

	public override void Deactivate()
	{
		_buff = null;
		StopAllCoroutines();
		base.Deactivate();
	}

	private void UpdateText()
	{
		int num = Mathf.CeilToInt(_duration);
		TimerText.Text = num.ToString();
	}

	public void SetData(PlayerBuffsIndicators indicators, string icon, float duration)
	{
		_indicators = indicators;
		_duration = duration;
		_buff = null;
		BuffIcon.PlayAnim(icon);
		if (_duration <= 0f)
		{
			TimerText.Text = "INF";
		}
		else
		{
			StartCoroutine(UpdateRoutine());
		}
	}

	public void SetData(PlayerBuffsIndicators indicators, Buff buff)
	{
		string icon = buff.GetType().ToString();
		_duration = buff.startTime + buff.duration;
		_duration -= Time.time;
		SetData(indicators, icon, _duration);
		_buff = buff;
	}
}
