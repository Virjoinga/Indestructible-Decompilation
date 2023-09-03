using System.Collections.Generic;
using UnityEngine;

public class UIAnimation : MonoBehaviour
{
	private abstract class ElementColor
	{
		protected Color _color = Color.white;

		public abstract void SetColor(Color color);
	}

	private class ElementColorText : ElementColor
	{
		private SpriteText _text;

		public ElementColorText(SpriteText text)
		{
			_text = text;
			_color = text.Color;
		}

		public override void SetColor(Color color)
		{
			_text.SetColor(_color * color);
		}
	}

	private class ElementColorRoot : ElementColor
	{
		private SpriteRoot _root;

		public ElementColorRoot(SpriteRoot root)
		{
			_root = root;
			_color = root.Color;
		}

		public override void SetColor(Color color)
		{
			_root.SetColor(_color * color);
		}
	}

	private class ElementColorPackable : ElementColor
	{
		private UIPackable _packable;

		public ElementColorPackable(UIPackable packable)
		{
			_packable = packable;
			_color = _packable.Color;
		}

		public override void SetColor(Color color)
		{
			_packable.Color = _color * color;
		}
	}

	public float Duration = 1f;

	public bool PlayOnStart;

	public bool PlayForever;

	public bool AnimatePosition;

	public AnimationCurve PositionX;

	public AnimationCurve PositionY;

	public AnimationCurve PositionZ;

	public bool AnimateScale;

	public AnimationCurve ScaleX;

	public AnimationCurve ScaleY;

	public AnimationCurve ScaleZ;

	public bool AnimateRotation;

	public AnimationCurve RotationX;

	public AnimationCurve RotationY;

	public AnimationCurve RotationZ;

	public bool AnimateColor;

	public bool AnimateColorAlpha;

	public bool AnimateColorChildren;

	public AnimationCurve ColorR;

	public AnimationCurve ColorG;

	public AnimationCurve ColorB;

	public AnimationCurve ColorA;

	private List<ElementColor> Elements = new List<ElementColor>();

	private Transform _transform;

	private bool _finished;

	private bool _paused = true;

	private float _time;

	public bool Finished
	{
		get
		{
			return _finished;
		}
	}

	private void Awake()
	{
		_transform = GetComponent<Transform>();
		CollectElements();
	}

	private void Start()
	{
		SetRelativeTime(0f);
		if (PlayOnStart)
		{
			Play();
		}
	}

	private void CollectElements()
	{
		if (AnimateColor || AnimateColorAlpha)
		{
			SpriteText[] array;
			SpriteRoot[] array2;
			UIPackable[] array3;
			if (AnimateColorChildren)
			{
				array = GetComponentsInChildren<SpriteText>();
				array2 = GetComponentsInChildren<SpriteRoot>();
				array3 = GetComponentsInChildren<UIPackable>();
			}
			else
			{
				array = GetComponents<SpriteText>();
				array2 = GetComponents<SpriteRoot>();
				array3 = GetComponents<UIPackable>();
			}
			SpriteText[] array4 = array;
			foreach (SpriteText text in array4)
			{
				Elements.Add(new ElementColorText(text));
			}
			SpriteRoot[] array5 = array2;
			foreach (SpriteRoot root in array5)
			{
				Elements.Add(new ElementColorRoot(root));
			}
			UIPackable[] array6 = array3;
			foreach (UIPackable packable in array6)
			{
				Elements.Add(new ElementColorPackable(packable));
			}
		}
	}

	private void SetColor(Color color)
	{
		foreach (ElementColor element in Elements)
		{
			element.SetColor(color);
		}
	}

	public void SetRelativeTime(float time)
	{
		_time = time;
		SampleRelative(_time);
	}

	private void SampleRelative(float time)
	{
		if (AnimatePosition)
		{
			float x = PositionX.Evaluate(time);
			float y = PositionY.Evaluate(time);
			float z = PositionZ.Evaluate(time);
			_transform.localPosition = new Vector3(x, y, z);
		}
		if (AnimateScale)
		{
			float x2 = ScaleX.Evaluate(time);
			float y2 = ScaleY.Evaluate(time);
			float z2 = ScaleZ.Evaluate(time);
			_transform.localScale = new Vector3(x2, y2, z2);
		}
		if (AnimateRotation)
		{
			float x3 = RotationX.Evaluate(time);
			float y3 = RotationY.Evaluate(time);
			float z3 = RotationZ.Evaluate(time);
			_transform.localRotation = Quaternion.Euler(x3, y3, z3);
		}
		if (AnimateColor || AnimateColorAlpha)
		{
			Color white = Color.white;
			if (AnimateColor)
			{
				white.r = ColorR.Evaluate(time);
				white.g = ColorG.Evaluate(time);
				white.b = ColorB.Evaluate(time);
			}
			if (AnimateColorAlpha)
			{
				white.a = ColorA.Evaluate(time);
			}
			SetColor(white);
		}
	}

	public void Rewind()
	{
		_paused = true;
		_finished = false;
		SetRelativeTime(0f);
	}

	public void Play()
	{
		_paused = false;
	}

	public void Pause()
	{
		_paused = true;
	}

	private void Update()
	{
		if (_paused)
		{
			return;
		}
		if (PlayForever)
		{
			float deltaTime = Time.deltaTime;
			_time += deltaTime / Duration;
			SampleRelative(_time);
		}
		else
		{
			if (!(_time < 1f))
			{
				return;
			}
			float deltaTime2 = Time.deltaTime;
			deltaTime2 /= Duration;
			if (deltaTime2 > 0f)
			{
				_time += deltaTime2;
				if (_time >= 1f)
				{
					_finished = true;
					_time = 1f;
				}
				SampleRelative(_time);
			}
		}
	}
}
