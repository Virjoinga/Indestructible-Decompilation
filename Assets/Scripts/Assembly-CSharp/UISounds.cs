using System.Collections;
using UnityEngine;

public class UISounds : MonoSingleton<UISounds>
{
	public enum Type
	{
		Click = 0,
		MatchStart = 1,
		Victory = 2,
		Defeat = 3,
		ItemPurchased = 4,
		Refuel = 5,
		ItemEquiped = 6,
		ItemRemoved = 7,
		VehicleSelected = 8,
		EquipmentSelected = 9,
		TalentSelected = 10,
		TalentLearned = 11,
		PaintjobEquiped = 12
	}

	private string[] _soundPaths = new string[13]
	{
		"UISound/click", "UISound/refuel_1", "Music/victory_stinger", "Music/defeat_stinger", "UISound/item_purchased_1", "UISound/refuel_1", "UISound/item_equiped_1", "UISound/item_removed_1", "UISound/vehicle_1", "UISound/vehicle_1",
		"UISound/talent_click", "UISound/talent_learn", "UISound/paintjobs_spray_1"
	};

	private AudioClip[] _audioClips;

	private int _soundMutes;

	public void Play(Type type)
	{
		PlaySound(type);
	}

	public void PlayWithMusicMute(Type type)
	{
		StartCoroutine(MuteMusicCoroutine(PlaySound(type)));
	}

	private IEnumerator MuteMusicCoroutine(float duration)
	{
		if (!(duration <= 0f))
		{
			if (_soundMutes <= 0)
			{
				MonoSingleton<SettingsController>.Instance.MuteMusic(true);
			}
			_soundMutes++;
			yield return new WaitForSeconds(duration);
			_soundMutes--;
			if (_soundMutes <= 0)
			{
				MonoSingleton<SettingsController>.Instance.MuteMusic(false);
			}
		}
	}

	private float PlaySound(Type type)
	{
		if (!MonoSingleton<SettingsController>.Instance.SoundEnabled)
		{
			return 0f;
		}
		if (!base.GetComponent<AudioSource>())
		{
			base.gameObject.AddComponent<AudioSource>();
		}
		if (_audioClips == null)
		{
			_audioClips = new AudioClip[_soundPaths.Length];
		}
		AudioClip audioClip = _audioClips[(int)type];
		if (audioClip == null)
		{
			_audioClips[(int)type] = (AudioClip)Resources.Load(_soundPaths[(int)type]);
			audioClip = _audioClips[(int)type];
			if (audioClip == null)
			{
				return 0f;
			}
		}
		base.GetComponent<AudioSource>().PlayOneShot(audioClip);
		return audioClip.length;
	}
}
