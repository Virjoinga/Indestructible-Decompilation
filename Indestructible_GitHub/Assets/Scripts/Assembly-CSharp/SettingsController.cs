using System;
using System.IO;
using System.Xml;

public class SettingsController : MonoSingleton<SettingsController>
{
	public delegate void SettingsChangedDelegate(SettingsController settingsController, bool state);

	private const string SETTINGS_FILE_NAME = "idt_settings.dat";

	private const string SETTINGS_ELEMENT_ROOT = "settings";

	private const string SETTINGS_ROOT_MUSIC = "music";

	private const string SETTINGS_ROOT_SOUND = "sound";

	private const string SETTINGS_ROOT_NOTIFICATIONS = "notifications";

	private const string SETTINGS_ROOT_SERVER_REGION = "serverRegion";

	private const string SETTINGS_ROOT_NOTIFICATIONS_PROMPT = "notificationsPrompt";

	private bool _musicEnabled = true;

	private bool _soundEnabled = true;

	private bool _isSoundUnmuted = true;

	private bool _isMusicUnmuted = true;

	private bool _notificationsEnabled = true;

	private RegionServer.Kind _serverRegion = RegionServer.Kind.Undefined;

	private bool m_notificaitonsPrompt;

	private string _settingsFilePath;

	public bool MusicEnabled
	{
		get
		{
			return _musicEnabled;
		}
	}

	public bool SoundEnabled
	{
		get
		{
			return _soundEnabled;
		}
	}

	public bool NotificationsEnabled
	{
		get
		{
			return _notificationsEnabled;
		}
	}

	public RegionServer.Kind ServerRegion
	{
		get
		{
			return _serverRegion;
		}
	}

	public bool NotificationsPrompt
	{
		get
		{
			return m_notificaitonsPrompt;
		}
		set
		{
			m_notificaitonsPrompt = value;
		}
	}

	public event SettingsChangedDelegate musicSettingsChangedEvent;

	public event SettingsChangedDelegate soundSettingsChangedEvent;

	public void SetServerRegion(RegionServer.Kind kind)
	{
		_serverRegion = kind;
		int preferredServerIndex = RegionServer.ToAddressIndex(kind);
		NetworkManager.instance.preferredServerIndex = preferredServerIndex;
	}

	public bool ToggleMusic()
	{
		EnableMusic(!_musicEnabled);
		Save();
		return _musicEnabled;
	}

	public bool ToggleSound()
	{
		EnableSound(!_soundEnabled);
		Save();
		return _soundEnabled;
	}

	public bool ToggleNotifications()
	{
		EnableNotifications(!_notificationsEnabled);
		Save();
		return _notificationsEnabled;
	}

	private void EnableMusic(bool enable)
	{
		_musicEnabled = enable;
		if (this.musicSettingsChangedEvent != null)
		{
			this.musicSettingsChangedEvent(this, enable);
		}
	}

	private void EnableSound(bool enable)
	{
		_soundEnabled = enable;
		if (this.soundSettingsChangedEvent != null)
		{
			this.soundSettingsChangedEvent(this, _isSoundUnmuted && _soundEnabled);
		}
	}

	public void MuteSound(bool shouldMuted)
	{
		_isSoundUnmuted = !shouldMuted;
		if (this.soundSettingsChangedEvent != null)
		{
			this.soundSettingsChangedEvent(this, _isSoundUnmuted && _soundEnabled);
		}
	}

	public void MuteMusic(bool shouldMuted)
	{
		_isMusicUnmuted = !shouldMuted;
		if (this.musicSettingsChangedEvent != null)
		{
			this.musicSettingsChangedEvent(this, _isMusicUnmuted && _musicEnabled);
		}
	}

	private void EnableNotifications(bool enable)
	{
		_notificationsEnabled = enable;
		if (enable != ANotificationManager.IsEnabled())
		{
			ANotificationManager.SetEnabled(enable);
			AStats.Flurry.LogEvent("NOTIFICATION_SETTING_CHANGED_TO", enable.ToString());
		}
	}

	private bool LoadXml(XmlDocument document)
	{
		XmlElement xmlElement = document["settings"];
		if (xmlElement == null)
		{
			return false;
		}
		_musicEnabled = XmlUtils.GetAttribute<bool>(xmlElement, "music");
		_soundEnabled = XmlUtils.GetAttribute<bool>(xmlElement, "sound");
		_notificationsEnabled = XmlUtils.GetAttribute<bool>(xmlElement, "notifications");
		_serverRegion = XmlUtils.GetAttribute(xmlElement, "serverRegion", RegionServer.Kind.Undefined);
		m_notificaitonsPrompt = XmlUtils.GetAttribute<bool>(xmlElement, "notificationsPrompt");
		return true;
	}

	private bool Load()
	{
		XmlDocument xmlDocument = new XmlDocument();
		bool flag = true;
		try
		{
			FileStream fileStream = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read);
			if (fileStream.Length > 0)
			{
				xmlDocument.Load(fileStream);
			}
			fileStream.Close();
		}
		catch (Exception)
		{
			flag = false;
		}
		if (flag)
		{
			flag = LoadXml(xmlDocument);
		}
		return flag;
	}

	private bool SaveXml(XmlDocument document)
	{
		XmlElement xmlElement = document.CreateElement("settings");
		XmlUtils.SetAttribute(xmlElement, "music", _musicEnabled);
		XmlUtils.SetAttribute(xmlElement, "sound", _soundEnabled);
		XmlUtils.SetAttribute(xmlElement, "notifications", _notificationsEnabled);
		XmlUtils.SetAttribute(xmlElement, "serverRegion", _serverRegion);
		XmlUtils.SetAttribute(xmlElement, "notificationsPrompt", m_notificaitonsPrompt);
		document.AppendChild(xmlElement);
		return true;
	}

	public bool Save()
	{
		XmlDocument xmlDocument = new XmlDocument();
		bool flag = SaveXml(xmlDocument);
		if (flag)
		{
			FileStream fileStream = new FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write);
			xmlDocument.Save(fileStream);
			fileStream.Close();
			AJavaTools.Backup.DataChanged();
		}
		return flag;
	}

	protected override void Awake()
	{
		base.Awake();
		_settingsFilePath = GameConstants.AndroidFilePath;
		_settingsFilePath += "/idt_settings.dat";
		Load();
		if (_serverRegion == RegionServer.Kind.Undefined)
		{
			_serverRegion = RegionServer.ToKind(NetworkManager.instance.preferredServerIndex);
		}
		Apply();
		Save();
	}

	public void Apply()
	{
		EnableMusic(_musicEnabled);
		EnableSound(_soundEnabled);
		EnableNotifications(_notificationsEnabled);
		SetServerRegion(_serverRegion);
	}
}
