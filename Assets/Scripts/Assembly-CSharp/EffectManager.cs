using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
	private Dictionary<GameObject, ParticleEffect> _particleEffects;

	private static EffectManager _instance;

	public static EffectManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GameObject("EffectManager").AddComponent<EffectManager>();
			}
			return _instance;
		}
	}

	public ParticleEffect GetParticleEffect(GameObject prefab)
	{
		ParticleEffect value;
		if (!_particleEffects.TryGetValue(prefab, out value))
		{
			value = (Object.Instantiate(prefab) as GameObject).GetComponent<ParticleEffect>();
			_particleEffects.Add(prefab, value);
		}
		return value;
	}

	private void Awake()
	{
		_particleEffects = new Dictionary<GameObject, ParticleEffect>();
		_instance = this;
	}

	private void OnDestroy()
	{
		_instance = null;
	}
}
