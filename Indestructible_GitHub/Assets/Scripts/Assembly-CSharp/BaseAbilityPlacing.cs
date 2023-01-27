using System.Collections;
using UnityEngine;

public class BaseAbilityPlacing : Weapon
{
	public AudioClip StartSound;

	public AudioClip EndSound;

	protected bool _setuped;

	protected int _teamID = -1;

	protected PhotonView _photonView;

	protected NetCachedObject _netCached;

	private int _ownerActorId = -1;

	protected float _effectScale = 1f;

	private GameObject _ownerObject;

	private AudioSource _audioSource;

	private bool _shouldPlayStartSound;

	private bool _shouldPlayEndSound;

	public override float GetBaseDamage()
	{
		return baseDamage * _effectScale;
	}

	protected override void Awake()
	{
		base.Awake();
		_audioSource = GetComponent<AudioSource>();
		if (_audioSource != null && (StartSound != null || EndSound != null))
		{
			SettingsController instance = MonoSingleton<SettingsController>.Instance;
			instance.soundSettingsChangedEvent += SoundSettingsChanged;
			SoundSettingsChanged(instance, instance.SoundEnabled);
		}
	}

	private void OnDestroy()
	{
		if (MonoSingleton<SettingsController>.Exists())
		{
			MonoSingleton<SettingsController>.Instance.soundSettingsChangedEvent -= SoundSettingsChanged;
		}
	}

	public void SetOwnerObject(GameObject ownerObject)
	{
		if (!(_ownerObject != ownerObject))
		{
			return;
		}
		_ownerObject = ownerObject;
		Collider[] componentsInChildren = ownerObject.GetComponentsInChildren<Collider>();
		int i = 0;
		for (int num = componentsInChildren.Length; i != num; i++)
		{
			Collider collider = componentsInChildren[i];
			if (!collider.isTrigger)
			{
				SetMainOwnerCollider(collider);
				break;
			}
		}
		OnOwnerObjectChanged(ownerObject);
	}

	public void SetTeamWithRPC(int teamID)
	{
		InternalSetTeam(teamID);
		base.gameObject.GetComponent<PhotonView>().RPC("SetTeam", PhotonTargets.Others, teamID);
	}

	protected void InternalSetupPhotonView()
	{
		if (PhotonNetwork.room != null)
		{
			_photonView = base.gameObject.GetComponent<PhotonView>();
		}
	}

	protected void InternalSetTeam(int teamID)
	{
		if (!_setuped)
		{
			_setuped = true;
			SetDamageLayers(damageLayerMask.value);
			_teamID = teamID;
			int num = 0;
			switch (_teamID)
			{
			case 0:
				num = (1 << LayerMask.NameToLayer("PlayerTeam0")) | (1 << LayerMask.NameToLayer("AITeam0"));
				OnSetTeam(_teamID);
				break;
			case 1:
				num = (1 << LayerMask.NameToLayer("PlayerTeam1")) | (1 << LayerMask.NameToLayer("AITeam1"));
				OnSetTeam(_teamID);
				break;
			default:
				OnSetTeam(0);
				break;
			}
			int num2 = base.damageLayers;
			num2 &= ~num;
			SetDamageLayers(num2);
		}
	}

	protected virtual void OnSetTeam(int teamId)
	{
	}

	protected void DeactivateMe()
	{
		if (_shouldPlayEndSound)
		{
			_audioSource.PlayOneShot(EndSound);
			StartCoroutine(DeactivateOnSoundEnd(EndSound));
		}
		else
		{
			Deactivate();
		}
	}

	private IEnumerator DeactivateOnSoundEnd(AudioClip soundClip)
	{
		yield return new WaitForSeconds(soundClip.length);
		Deactivate();
	}

	private void Deactivate()
	{
		if ((bool)_netCached)
		{
			_netCached.Deactivate();
		}
		else
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	protected virtual void OnEnable()
	{
		InternalSetupPhotonView();
		_setuped = false;
		UpdateOwnerInfo();
		if (!_netCached)
		{
			_netCached = GetComponent<NetCachedObject>();
		}
		if (_shouldPlayStartSound)
		{
			_audioSource.PlayOneShot(StartSound);
		}
	}

	protected void InternalSetOwner(int destructibleId, int actorId)
	{
		SetPlayer(null);
		_ownerActorId = actorId;
		UpdateOwnerInfo();
	}

	public void SetEffectScaleWithRPC(float scale)
	{
		InternalSetEffectScale(scale);
		base.gameObject.GetComponent<PhotonView>().RPC("SetEffectScale", PhotonTargets.Others, scale);
	}

	protected void InternalSetEffectScale(float scale)
	{
		_effectScale = scale;
	}

	private void UpdateOwnerInfo()
	{
		if (player == null || _ownerActorId >= 0)
		{
			MultiplayerGame multiplayerGame = IDTGame.Instance as MultiplayerGame;
			MatchPlayer matchPlayer;
			if (multiplayerGame != null && multiplayerGame.match.TryGetPlayer(_ownerActorId, out matchPlayer))
			{
				SetPlayer(matchPlayer);
			}
		}
	}

	protected virtual void OnOwnerObjectChanged(GameObject ownerObject)
	{
		if (ownerObject != null)
		{
			Vehicle component = ownerObject.GetComponent<Vehicle>();
			if ((bool)component)
			{
				SetPlayer(component.player);
				Destructible destructible = component.destructible;
				if ((bool)_photonView)
				{
					_photonView.RPC("SetOwner", PhotonTargets.OthersBuffered, (!(destructible != null)) ? (-1) : destructible.id, (player == null) ? (-1) : player.id);
				}
				return;
			}
		}
		SetPlayer(null);
	}

	private void SoundSettingsChanged(SettingsController settingsController, bool state)
	{
		if (state && _audioSource != null)
		{
			_shouldPlayStartSound = StartSound != null;
			_shouldPlayEndSound = EndSound != null;
		}
		else
		{
			_shouldPlayStartSound = false;
			_shouldPlayEndSound = false;
		}
	}
}
