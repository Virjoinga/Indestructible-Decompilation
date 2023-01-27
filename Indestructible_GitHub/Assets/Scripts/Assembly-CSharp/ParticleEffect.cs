using UnityEngine;

[AddComponentMenu("Indestructible/Effects/Particle Effect")]
public class ParticleEffect : MonoBehaviour
{
	public Vector3[] qualityEmissionSizeLifetimeFactors;

	private ParticleSystem _emitter;

	private float _minSize;

	private float _sizeRange;

	private float _minLifetime;

	private float _lifetimeRange;

	private Vector3 _worldVelocity;

	private Vector3 _localVelocity;

	private float _emitterVelocityScale;

	private float _minAngularVelocity;

	private float _angularVelocityRange;

	private float _minEmissionRate;

	private float _emissionRateRange;

	private float _minEmissionInterval;

	private float _emissionIntervalRange;

	private bool _isRndRotation;

	public float rndEmissionRate
	{
		get
		{
			return GetRndEmissionRate(Random.value);
		}
	}

	public float rndEmissionInterval
	{
		get
		{
			return GetRndEmissionInterval(Random.value);
		}
	}

	public float rndSize
	{
		get
		{
			return GetRndSize(Random.value);
		}
	}

	public float rndLifetime
	{
		get
		{
			return GetRndLifetime(Random.value);
		}
	}

	public Vector3 worldVelocity
	{
		get
		{
			return _worldVelocity;
		}
	}

	public Vector3 localVelocity
	{
		get
		{
			return _localVelocity;
		}
	}

	public float emitterVelocityScale
	{
		get
		{
			return _emitterVelocityScale;
		}
	}

	protected ParticleSystem emitter
	{
		get
		{
			return _emitter;
		}
	}

    /*public void Emit(Vector3 pos, Vector3 velocity, float size, float lifetime)
	{
		_emitter.Emit(pos, velocity, size, lifetime, Color.white);
	}*/

    public void Emit(Vector3 pos, Vector3 velocity, float size, float lifetime)
    {
        var main = _emitter.main;
        main.startSize = size;
        main.startLifetime = lifetime;

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = pos;
        emitParams.velocity = velocity;
        emitParams.startSize = size;
        emitParams.startLifetime = lifetime;
        _emitter.Emit(emitParams, 1);

    }

    /*public void Emit(Vector3 pos, Vector3 velocity, float size, float lifetime, float rotation, float angularVelocity)
	{
		_emitter.Emit(pos, velocity, size, lifetime, Color.white, rotation, angularVelocity);
	}*/

    public void Emit(Vector3 pos, Vector3 velocity, float size, float lifetime, float rotation, float angularVelocity)
    {
        var particle = new ParticleSystem.EmitParams();
        particle.position = pos;
        particle.velocity = velocity;
        particle.startSize = size;
        particle.startLifetime = lifetime;
        particle.startColor = Color.white;
        particle.rotation3D = new Vector3(0f, 0f, rotation);
        particle.angularVelocity = angularVelocity;
        _emitter.Emit(particle, 1);
    }

    /*public void EmitRandomSizeLifetimeRotation(Vector3 pos, Vector3 velocity)
	{
		if (!_isRndRotation)
		{
			_emitter.Emit(pos, velocity, rndSize, rndLifetime, Color.white);
			return;
		}
		float value = Random.value;
		_emitter.Emit(pos, velocity, rndSize, rndLifetime, Color.white, value * 360f - 180f, _minAngularVelocity + _angularVelocityRange * value);
	}*/

    public void EmitRandomSizeLifetimeRotation(Vector3 pos, Vector3 velocity)
    {
        var particle = new ParticleSystem.EmitParams();
        particle.position = pos;
        particle.velocity = velocity;
        particle.startColor = Color.white;
        if (!_isRndRotation)
        {
            particle.startSize = rndSize;
            particle.startLifetime = rndLifetime;
            _emitter.Emit(particle, 1);
            return;
        }
        float value = Random.value;
        particle.startSize = rndSize;
        particle.startLifetime = rndLifetime;
        particle.rotation3D = new Vector3(0f, 0f, value * 360f - 180f);
        particle.angularVelocity = _minAngularVelocity + _angularVelocityRange * value;
        _emitter.Emit(particle, 1);
    }

    /*public void EmitRandomSizeLifetimeRotation(Vector3 pos, Vector3 velocity, float sizeScale, float lifetimeScale)
	{
		if (!_isRndRotation)
		{
			_emitter.Emit(pos, velocity, rndSize * sizeScale, rndLifetime * lifetimeScale, Color.white);
			return;
		}
		float value = Random.value;
		_emitter.Emit(pos, velocity, rndSize * sizeScale, rndLifetime * lifetimeScale, Color.white, value * 360f - 180f, _minAngularVelocity + _angularVelocityRange * value);
	}*/

    public void EmitRandomSizeLifetimeRotation(Vector3 pos, Vector3 velocity, float sizeScale, float lifetimeScale)
    {
        var particle = new ParticleSystem.EmitParams();
        particle.position = pos;
        particle.velocity = velocity;
        particle.startColor = Color.white;
        if (!_isRndRotation)
        {
            particle.startSize = rndSize * sizeScale;
            particle.startLifetime = rndLifetime * lifetimeScale;
            _emitter.Emit(particle, 1);
            return;
        }
        float value = Random.value;
        particle.startSize = rndSize * sizeScale;
        particle.startLifetime = rndLifetime * lifetimeScale;
        particle.rotation3D = new Vector3(0f, 0f, value * 360f - 180f);
        particle.angularVelocity = _minAngularVelocity + _angularVelocityRange * value;
        _emitter.Emit(particle, 1);
    }

    /*public void EmitRandomSizeLifetimeRotation(int count, Vector3 pos, Vector3 velocity)
	{
		if (!_isRndRotation)
		{
			do
			{
				_emitter.Emit(pos, velocity, rndSize, rndLifetime, Color.white);
			}
			while (0 < --count);
			return;
		}
		do
		{
			float value = Random.value;
			_emitter.Emit(pos, velocity, rndSize, rndLifetime, Color.white, value * 360f - 180f, _minAngularVelocity + _angularVelocityRange * value);
		}
		while (0 < --count);
	}*/

    public void EmitRandomSizeLifetimeRotation(int count, Vector3 pos, Vector3 velocity)
    {
        var particle = new ParticleSystem.EmitParams();
        particle.position = pos;
        particle.velocity = velocity;
        particle.startColor = Color.white;
        if (!_isRndRotation)
        {
            particle.startSize = rndSize;
            particle.startLifetime = rndLifetime;
            while (count > 0)
            {
                _emitter.Emit(particle, 1);
                count--;
            }
            return;
        }
        while (count > 0)
        {
            float value = Random.value;
            particle.startSize = rndSize;
            particle.startLifetime = rndLifetime;
            particle.rotation3D = new Vector3(0f, 0f, value * 360f - 180f);
            particle.angularVelocity = _minAngularVelocity + _angularVelocityRange * value;
            _emitter.Emit(particle, 1);
            count--;
        }
    }

    /*public void EmitRandomSizeLifetimeRotation(int count, Vector3 pos, Vector3 velocity, float sizeScale, float lifetimeScale)
	{
		if (!_isRndRotation)
		{
			do
			{
				_emitter.Emit(pos, velocity, rndSize * sizeScale, rndLifetime * lifetimeScale, Color.white);
			}
			while (0 < --count);
			return;
		}
		do
		{
			float value = Random.value;
			_emitter.Emit(pos, velocity, rndSize * sizeScale, rndLifetime * lifetimeScale, Color.white, value * 360f - 180f, _minAngularVelocity + _angularVelocityRange * value);
		}
		while (0 < --count);
	}*/

    public void EmitRandomSizeLifetimeRotation(int count, Vector3 pos, Vector3 velocity, float sizeScale, float lifetimeScale)
    {
        var particle = new ParticleSystem.EmitParams();
        particle.position = pos;
        particle.velocity = velocity;
        particle.startColor = Color.white;
        if (!_isRndRotation)
        {
            particle.startSize = rndSize * sizeScale;
            particle.startLifetime = rndLifetime * lifetimeScale;
            while (count > 0)
            {
                _emitter.Emit(particle, 1);
                count--;
            }
            return;
        }
        while (count > 0)
        {
            float value = Random.value;
            particle.startSize = rndSize * sizeScale;
            particle.startLifetime = rndLifetime * lifetimeScale;
            particle.rotation3D = new Vector3(0f, 0f, value * 360f - 180f);
            particle.angularVelocity = _minAngularVelocity + _angularVelocityRange * value;
            _emitter.Emit(particle, 1);
            count--;
        }
    }

    public float GetRndEmissionRate(float rnd01)
	{
		return _minEmissionRate + _emissionRateRange * rnd01;
	}

	public float GetRndEmissionInterval(float rnd01)
	{
		return _minEmissionInterval + _emissionIntervalRange * rnd01;
	}

	public float GetRndSize(float rnd01)
	{
		return _minSize + _sizeRange * rnd01;
	}

	public float GetRndLifetime(float rnd01)
	{
		return _minLifetime + _lifetimeRange * rnd01;
	}

    /*protected virtual void Awake()
	{
		_emitter = base.particleEmitter;
		//_emitter.emit = false;
        _emitter.Stop();
        _minSize = _emitter.minSize;
		_sizeRange = _emitter.maxSize - _minSize;
		_minLifetime = _emitter.minEnergy;
		_lifetimeRange = _emitter.maxEnergy - _minLifetime;
		_worldVelocity = emitter.worldVelocity;
		_localVelocity = emitter.localVelocity;
		_emitterVelocityScale = emitter.emitterVelocityScale;
		_minEmissionRate = _emitter.minEmission;
		float num = 1f / _minEmissionRate;
		float maxEmission = _emitter.maxEmission;
		_minEmissionInterval = 1f / maxEmission;
		_emissionRateRange = maxEmission - _minEmissionRate;
		_emissionIntervalRange = num - _minEmissionInterval;
		if (emitter.rndRotation)
		{
			_isRndRotation = true;
			float rndAngularVelocity = emitter.rndAngularVelocity;
			_minAngularVelocity = emitter.angularVelocity - rndAngularVelocity;
			_angularVelocityRange = rndAngularVelocity * 2f;
		}
		QualityManager instance = QualityManager.instance;
		instance.qualityLevelChangedEvent += QualityLevelChanged;
		QualityLevelChanged(0, instance.qualityLevel);
	}*/

    protected virtual void Awake()
    {
        _emitter = GetComponent<ParticleSystem>();
        _emitter.Stop();
        _minSize = _emitter.main.startSize.constantMin;
        _sizeRange = _emitter.main.startSize.constantMax - _minSize;
        _minLifetime = _emitter.main.startLifetime.constantMin;
        _lifetimeRange = _emitter.main.startLifetime.constantMax - _minLifetime;
        ParticleSystemSimulationSpace _worldVelocity = _emitter.velocityOverLifetime.space;
        float _localVelocity = _emitter.velocityOverLifetime.x.constant;
        _emitterVelocityScale = _emitter.velocityOverLifetime.xMultiplier;
        _minEmissionRate = _emitter.emission.rate.constantMin;
        float num = 1f / _minEmissionRate;
        float maxEmission = _emitter.emission.rate.constantMax;
        _minEmissionInterval = 1f / maxEmission;
        _emissionRateRange = maxEmission - _minEmissionRate;
        _emissionIntervalRange = num - _minEmissionInterval;
        if (_emitter.rotationOverLifetime.enabled)
        {
            _isRndRotation = true;
            float rndAngularVelocity = _emitter.rotationOverLifetime.z.constant;
            _minAngularVelocity = rndAngularVelocity;
            _angularVelocityRange = rndAngularVelocity * 2f;
        }
        QualityManager instance = QualityManager.instance;
        instance.qualityLevelChangedEvent += QualityLevelChanged;
        QualityLevelChanged(0, instance.qualityLevel);
    }

    protected virtual void OnDestroy()
	{
		if (QualityManager.isExists)
		{
			QualityManager.instance.qualityLevelChangedEvent -= QualityLevelChanged;
		}
	}

	protected virtual void QualityLevelChanged(int oldLevel, int newLevel)
	{
		if (qualityEmissionSizeLifetimeFactors.Length <= --oldLevel)
		{
			oldLevel = qualityEmissionSizeLifetimeFactors.Length - 1;
		}
		if (qualityEmissionSizeLifetimeFactors.Length <= --newLevel)
		{
			newLevel = qualityEmissionSizeLifetimeFactors.Length - 1;
		}
		if (oldLevel != newLevel)
		{
			Vector3 vector = ((0 > newLevel) ? new Vector3(1f, 1f, 1f) : qualityEmissionSizeLifetimeFactors[newLevel]);
			if (0 <= oldLevel)
			{
				Vector3 vector2 = qualityEmissionSizeLifetimeFactors[oldLevel];
				vector.x /= vector2.x;
				vector.y /= vector2.y;
				vector.z /= vector2.z;
			}
			_minEmissionRate *= vector.x;
			_emissionRateRange *= vector.x;
			_emissionIntervalRange *= vector.x;
			_minEmissionInterval /= vector.x;
			_minSize *= vector.y;
			_sizeRange *= vector.y;
			_minLifetime *= vector.z;
			_lifetimeRange *= vector.z;
		}
	}
}
