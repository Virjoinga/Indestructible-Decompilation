using UnityEngine;

public class AudioHelper
{
	private AudioSource _audioSource;

	private bool _isEnabled;

	private bool _shouldPlayOnEnable;

	private bool _isMusic;

	public AudioClip clip
	{
		get
		{
			return (!(_audioSource != null)) ? null : _audioSource.clip;
		}
		set
		{
			if (_audioSource != null)
			{
				_audioSource.clip = value;
				SettingsController instance = MonoSingleton<SettingsController>.Instance;
				CheckState(instance, (!_isMusic) ? instance.SoundEnabled : instance.MusicEnabled);
			}
		}
	}

	public bool isEnabled
	{
		get
		{
			return _isEnabled;
		}
	}

	public AudioHelper(AudioSource audioSource, bool isMusic, bool shouldPlayOnEnable)
	{
		_audioSource = audioSource;
		_isMusic = isMusic;
		_shouldPlayOnEnable = shouldPlayOnEnable;
		SubscribeToSettingsChanging();
	}

	public void Dispose()
	{
		UnsubscribeToSettingsChanging();
	}

	public void PlayIfEnabled()
	{
		if (_isEnabled && (bool)_audioSource)
		{
			_audioSource.Play();
		}
	}

	public void StopIfEnabled()
	{
		if (_isEnabled && (bool)_audioSource)
		{
			_audioSource.Stop();
		}
	}

	public void CheckState(SettingsController settingsController, bool state)
	{
		_isEnabled = state && _audioSource != null && _audioSource.clip != null;
		if (!_isEnabled)
		{
			if (_audioSource != null)
			{
				_audioSource.Stop();
			}
		}
		else if (_shouldPlayOnEnable)
		{
			_audioSource.Play();
		}
	}

	private void SubscribeToSettingsChanging()
	{
		SettingsController instance = MonoSingleton<SettingsController>.Instance;
		if (_isMusic)
		{
			instance.musicSettingsChangedEvent += CheckState;
			CheckState(instance, instance.MusicEnabled);
		}
		else
		{
			instance.soundSettingsChangedEvent += CheckState;
			CheckState(instance, instance.SoundEnabled);
		}
	}

	private void UnsubscribeToSettingsChanging()
	{
		if (MonoSingleton<SettingsController>.Exists())
		{
			if (_isMusic)
			{
				MonoSingleton<SettingsController>.Instance.musicSettingsChangedEvent -= CheckState;
			}
			else
			{
				MonoSingleton<SettingsController>.Instance.soundSettingsChangedEvent -= CheckState;
			}
		}
	}
}
