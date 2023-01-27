using System;
using UnityEngine;

public class QualityManager : ScriptableObject, IUpdatable
{
	[Serializable]
	public class QualityLevelOptions
	{
		public float minFrameTime;

		public float maxFrameTime;

		public int shaderLOD;
	}

	public delegate void QualityLevelChanged(int oldLevel, int newLevel);

	private const string _assetPath = "Assets/Bundles/Quality/QualityManager.asset";

	public QualityLevelOptions[] qualityLevels;

	private int _qualityLevel;

	private float _frameTimeAcc;

	private int _accCount;

	private float _minFrameTime;

	private float _maxFrameTime;

	private int _minQualityLevel;

	private static QualityManager _instance;

	public static QualityManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = BundlesUtils.Load("Assets/Bundles/Quality/QualityManager.asset") as QualityManager;
				_instance.Init();
			}
			return _instance;
		}
	}

	public static bool isExists
	{
		get
		{
			return _instance != null;
		}
	}

	public int qualityLevel
	{
		get
		{
			return _qualityLevel;
		}
		protected set
		{
			int oldLevel = _qualityLevel;
			_qualityLevel = value;
			QualityLevelOptions qualityLevelOptions = qualityLevels[value];
			_minFrameTime = qualityLevelOptions.minFrameTime;
			_maxFrameTime = qualityLevelOptions.maxFrameTime;
			Shader.globalMaximumLOD = qualityLevelOptions.shaderLOD;
			if (this.qualityLevelChangedEvent != null)
			{
				this.qualityLevelChangedEvent(oldLevel, value);
			}
		}
	}

	public event QualityLevelChanged qualityLevelChangedEvent;

	public void StartUpdateQualityLevel()
	{
		MonoSingleton<UpdateAgent>.Instance.StartUpdate(this);
	}

	public void StopUpdateQualityLevel()
	{
		if (MonoSingleton<UpdateAgent>.Exists())
		{
			MonoSingleton<UpdateAgent>.Instance.StopUpdate(this);
		}
		qualityLevel = _minQualityLevel;
	}

	private void Init()
	{
		Resolution currentResolution = Screen.currentResolution;
		qualityLevel = (_minQualityLevel = ((1024 >= currentResolution.width && 1024 >= currentResolution.height) ? 1 : 0));
	}

	public bool DoUpdate()
	{
		_frameTimeAcc += Time.deltaTime;
		if (++_accCount == 10)
		{
			float num = _frameTimeAcc * 0.1f;
			_frameTimeAcc = 0f;
			_accCount = 0;
			if (num < _minFrameTime)
			{
				if (_minQualityLevel < _qualityLevel)
				{
					qualityLevel = _qualityLevel - 1;
				}
			}
			else if (_maxFrameTime < num)
			{
				int num2 = _qualityLevel + 1;
				if (num2 < qualityLevels.Length)
				{
					qualityLevel = num2;
				}
			}
		}
		return true;
	}
}
