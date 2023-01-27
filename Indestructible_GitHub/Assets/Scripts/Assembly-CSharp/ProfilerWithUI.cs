using System;
using UnityEngine;

[RequireComponent(typeof(SpriteText))]
public class ProfilerWithUI : MonoBehaviour
{
	private float _fps;

	private float _frames;

	private float _lastTime;

	public float UpdateInterval = 0.5f;

	private SpriteText _spriteScript;

	private void Awake()
	{
		_spriteScript = GetComponent<SpriteText>();
	}

	private void LateUpdate()
	{
		_frames += 1f;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = realtimeSinceStartup - _lastTime;
		if (num >= UpdateInterval)
		{
			_fps = _frames / num;
			_frames = 0f;
			_lastTime = realtimeSinceStartup;
			string text = Mathf.RoundToInt(_fps).ToString();
			long num2 = GC.GetTotalMemory(false) >> 20;
			int num3 = NativeUtils.GetCurrentMemoryBytes() >> 20;
			text += string.Format("\n{0:0}, {1:0}", num3, num2);
			_spriteScript.Text = text;
		}
	}
}
