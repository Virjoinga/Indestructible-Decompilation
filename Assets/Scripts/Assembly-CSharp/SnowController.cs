using UnityEngine;

public class SnowController : MonoBehaviour
{
	public int requiredQualityLevel;

	private ParticleSystem _particleSystem;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		if (_particleSystem != null)
		{
			QualityManager instance = QualityManager.instance;
			instance.qualityLevelChangedEvent += QualityLevelChanged;
			QualityLevelChanged(0, instance.qualityLevel);
		}
	}

	private void OnDestroy()
	{
		if (QualityManager.isExists)
		{
			QualityManager.instance.qualityLevelChangedEvent -= QualityLevelChanged;
		}
	}

	private void QualityLevelChanged(int oldLevel, int newLevel)
	{
		if (_particleSystem != null)
		{
			_particleSystem.Stop();
		}
	}
}
