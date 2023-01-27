using UnityEngine;

[AddComponentMenu("Indestructible/PostEffects/Motion Blur")]
public class MotionBlur : MonoBehaviour
{
	private class BlurFrame
	{
		public RenderTexture texture;

		public Material material;
	}

	public Shader accumulateShader;

	public float baseNormalAccumulateFactor = 0.65f;

	public float baseDownscaledAccumulateFactor = 0.8f;

	public int downscale = 2;

	public Shader blitCopyShader;

	public int requiredQualityLevel;

	private Camera _camera;

	private Rect _pixelRect;

	private BlurFrame _backFrame;

	private BlurFrame _currentFrame;

	private BlurFrame _downscaledFrame;

	private BlurFrame _normalFrame;

	private RenderTexture _accumTexture;

	private Material _blitCopyMaterial;

	private Color _clearColor = Color.black;

	private float _normalAccumulateFactor;

	private float _downscaledAccumulateFactor;

	private int _usageCount;

	private CameraClearFlags _origClearFlags;

	private bool _isAccumInitialized;

	private bool _activated;

	public float normalAccumulateFactor
	{
		get
		{
			return _normalAccumulateFactor;
		}
		set
		{
			_normalAccumulateFactor = value;
			_normalFrame.material.SetFloat("_AccumFactor", value);
		}
	}

	public float downscaledAccumulateFactor
	{
		get
		{
			return _downscaledAccumulateFactor;
		}
		set
		{
			_downscaledAccumulateFactor = value;
			if (_downscaledFrame != _normalFrame)
			{
				_downscaledFrame.material.SetFloat("_AccumFactor", value);
			}
		}
	}

	public int usageCount
	{
		get
		{
			return _usageCount;
		}
	}

	public void Use()
	{
		_usageCount++;
		if (QualityManager.instance.qualityLevel <= requiredQualityLevel)
		{
			base.enabled = true;
		}
	}

	public void Unuse()
	{
		if (--_usageCount <= 0)
		{
			base.enabled = false;
		}
	}

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		if (_camera != null)
		{
			_normalFrame = new BlurFrame();
			_normalFrame.material = new Material(accumulateShader);
			_normalFrame.material.hideFlags = HideFlags.HideAndDontSave;
			normalAccumulateFactor = baseNormalAccumulateFactor;
			if (downscale < 2)
			{
				_downscaledFrame = _normalFrame;
			}
			else
			{
				_downscaledFrame = new BlurFrame();
				_downscaledFrame.material = new Material(accumulateShader);
				_downscaledFrame.material.hideFlags = HideFlags.HideAndDontSave;
				downscaledAccumulateFactor = baseDownscaledAccumulateFactor;
			}
			_blitCopyMaterial = new Material(blitCopyShader);
		}
		else
		{
			Object.Destroy(this);
		}
		QualityManager.instance.qualityLevelChangedEvent += QualityLevelChanged;
	}

	private void OnDisable()
	{
		_isAccumInitialized = false;
		_activated = false;
		_usageCount = 0;
		if (_camera != null)
		{
			_camera.targetTexture = null;
			_camera.clearFlags = _origClearFlags;
		}
	}

	private void OnDestroy()
	{
		if (QualityManager.isExists)
		{
			QualityManager.instance.qualityLevelChangedEvent -= QualityLevelChanged;
		}
		Object.DestroyImmediate(_accumTexture);
		Object.DestroyImmediate(_downscaledFrame.texture);
		Object.DestroyImmediate(_downscaledFrame.material);
		Object.DestroyImmediate(_normalFrame.texture);
		Object.DestroyImmediate(_normalFrame.material);
		Object.DestroyImmediate(_blitCopyMaterial);
	}

	private void QualityLevelChanged(int oldLevel, int newLevel)
	{
		if (0 < _usageCount)
		{
			base.enabled = newLevel <= requiredQualityLevel;
		}
	}

	private void Activate()
	{
		_pixelRect = _camera.pixelRect;
		int num = (int)_pixelRect.width;
		int num2 = (int)_pixelRect.height;
		if (_accumTexture == null || _downscaledFrame.texture == null || _normalFrame.texture == null || _accumTexture.width != num || _accumTexture.height != num2)
		{
			Object.DestroyImmediate(_accumTexture);
			Object.DestroyImmediate(_downscaledFrame.texture);
			Object.DestroyImmediate(_normalFrame.texture);
			_accumTexture = new RenderTexture(num, num2, 0, RenderTextureFormat.RGB565);
			_accumTexture.filterMode = FilterMode.Point;
			_accumTexture.hideFlags = HideFlags.HideAndDontSave;
			_normalFrame.texture = new RenderTexture(num, num2, 24, RenderTextureFormat.RGB565);
			_normalFrame.texture.filterMode = FilterMode.Point;
			_normalFrame.texture.hideFlags = HideFlags.HideAndDontSave;
			if (1 < downscale)
			{
				_downscaledFrame.texture = new RenderTexture(num / downscale, num2 / downscale, 24, RenderTextureFormat.RGB565);
				_downscaledFrame.texture.filterMode = FilterMode.Trilinear;
				_downscaledFrame.texture.hideFlags = HideFlags.HideAndDontSave;
			}
			_blitCopyMaterial.SetTexture("_MainTex", _accumTexture);
		}
		_currentFrame = _normalFrame;
		_backFrame = _downscaledFrame;
		_camera.targetTexture = _currentFrame.texture;
		_origClearFlags = _camera.clearFlags;
		_camera.clearFlags = CameraClearFlags.Depth;
		_activated = true;
	}

	private void OnPostRender()
	{
		if (!_isAccumInitialized)
		{
			if (!_activated)
			{
				Activate();
				return;
			}
			_isAccumInitialized = true;
			Graphics.Blit(_currentFrame.texture, _accumTexture);
		}
		else
		{
			Graphics.Blit(_currentFrame.texture, _accumTexture, _currentFrame.material);
		}
		_camera.targetTexture = _backFrame.texture;
		SwapBlurFrames();
		BlitAccumulationTexture();
	}

	private void SwapBlurFrames()
	{
		BlurFrame backFrame = _backFrame;
		_backFrame = _currentFrame;
		_currentFrame = backFrame;
	}

	private void BlitAccumulationTexture()
	{
		RenderTexture.active = null;
		GL.Viewport(_pixelRect);
		GL.Clear(true, true, _clearColor);
		_blitCopyMaterial.SetPass(0);
		GL.LoadOrtho();
		GL.LoadIdentity();
		GL.Begin(5);
		GL.Vertex3(0f, 0f, 0f);
		GL.Vertex3(1f, 0f, 0f);
		GL.Vertex3(0f, 1f, 0f);
		GL.Vertex3(1f, 1f, 0f);
		GL.End();
	}
}
