using System.Collections.Generic;
using System.Xml;

public class PlayerStatistics
{
	private abstract class Item
	{
		public string Id = string.Empty;

		public bool Storable;

		public Item(string id, bool storable)
		{
			Id = id;
			Storable = storable;
		}

		public abstract void SetDefault();

		public abstract void LoadXml(XmlElement root);

		public abstract void SaveXml(XmlDocument document, XmlElement root);
	}

	private class ItemInt : Item
	{
		public int Value;

		public ItemInt(string id, bool storable)
			: base(id, storable)
		{
			SetDefault();
		}

		public override void SetDefault()
		{
			Value = 0;
		}

		public override void LoadXml(XmlElement root)
		{
			Value = XmlUtils.GetAttribute<int>(root, Id);
		}

		public override void SaveXml(XmlDocument document, XmlElement root)
		{
			XmlUtils.SetAttribute(root, Id, Value);
		}
	}

	private class ItemBool : Item
	{
		public bool Value;

		public ItemBool(string id, bool storable)
			: base(id, storable)
		{
			SetDefault();
		}

		public override void SetDefault()
		{
			Value = false;
		}

		public override void LoadXml(XmlElement root)
		{
			Value = XmlUtils.GetAttribute<bool>(root, Id);
		}

		public override void SaveXml(XmlDocument document, XmlElement root)
		{
			XmlUtils.SetAttribute(root, Id, Value);
		}
	}

	private class ItemFloat : Item
	{
		public float Value;

		public ItemFloat(string id, bool storable)
			: base(id, storable)
		{
			SetDefault();
		}

		public override void SetDefault()
		{
			Value = 0f;
		}

		public override void LoadXml(XmlElement root)
		{
			Value = XmlUtils.GetAttribute<float>(root, Id);
		}

		public override void SaveXml(XmlDocument document, XmlElement root)
		{
			XmlUtils.SetAttribute(root, Id, Value);
		}
	}

	private class ItemMultiplayerTotalKillsOnVehicle : Item
	{
		private class Element
		{
			public string VehicleId;

			public int Kills;

			public Element(string id, int kills)
			{
				VehicleId = id;
				Kills = kills;
			}
		}

		private const string SAVE_VEHICLE = "vehicle";

		private const string SAVE_VEHICLE_ID = "id";

		private const string SAVE_VEHICLE_KILLS = "kills";

		private List<Element> _elements = new List<Element>();

		public ItemMultiplayerTotalKillsOnVehicle(string id, bool storable)
			: base(id, storable)
		{
		}

		public override void SetDefault()
		{
			_elements.Clear();
		}

		public override void LoadXml(XmlElement root)
		{
			XmlElement xmlElement = root["multiplayerTotalKillsOnVehicle"];
			if (xmlElement == null)
			{
				return;
			}
			XmlNodeList xmlNodeList = xmlElement.SelectNodes("vehicle");
			foreach (XmlNode item in xmlNodeList)
			{
				string attribute = XmlUtils.GetAttribute<string>(item, "id");
				int attribute2 = XmlUtils.GetAttribute<int>(item, "kills");
				_elements.Add(new Element(attribute, attribute2));
			}
		}

		public override void SaveXml(XmlDocument document, XmlElement root)
		{
			XmlElement xmlElement = document.CreateElement("multiplayerTotalKillsOnVehicle");
			root.AppendChild(xmlElement);
			foreach (Element element in _elements)
			{
				XmlElement xmlElement2 = document.CreateElement("vehicle");
				xmlElement.AppendChild(xmlElement2);
				XmlUtils.SetAttribute(xmlElement2, "id", element.VehicleId);
				XmlUtils.SetAttribute(xmlElement2, "kills", element.Kills);
			}
		}

		private Element Find(string vehicleId)
		{
			Element element = _elements.Find((Element a) => a.VehicleId == vehicleId);
			if (element == null)
			{
				element = new Element(vehicleId, 0);
				_elements.Add(element);
			}
			return element;
		}

		public void AddKills(string vehicleId, int delta)
		{
			Element element = Find(vehicleId);
			element.Kills += delta;
		}

		public int Vehikill()
		{
			int num = 0;
			foreach (Element element in _elements)
			{
				if (element.Kills > 0)
				{
					num++;
				}
			}
			return num;
		}
	}

	private const string SAVE_STATISTICS_OPEN_COUNT = "openCount";

	private const string SAVE_STATISTICS_MULTIPLAYER_GAMES_PLAYED = "multiplayerGamesPlayed";

	private const string SAVE_STATISTICS_MULTIPLAYER_GAMES_WON = "multiplayerGamesWon";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_KILLS = "multiplayerTotalKills";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_KILLS_ON_VEHICLE = "multiplayerTotalKillsOnVehicle";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_KILLS_COLLISION = "multiplayerTotalKillsCollision";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_KILLS_FLAG_CARRIERS = "multiplayerTotalKillsFlagCarriers";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_DEATHS = "multiplayerTotalDeaths";

	private const string SAVE_STATISTICS_MULTIPLAYER_POWERUPS_COLLECTED = "multiplayerPowerupsCollected";

	private const string SAVE_STATISTICS_MULTIPLAYER_POWERUPS_COLLECTED_REPAIR = "multiplayerPowerupsCollectedRepair";

	private const string SAVE_STATISTICS_MULTIPLAYER_POWERUPS_COLLECTED_DAMAGE = "multiplayerPowerupsCollectedDamage";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_FLAGS_CAPTURED = "multiplayerTotalFlagsCaptured";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_TIME_ON_POINT = "multiplayerTotalTimeOnPoint";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_CHARGES_CAPTURED = "multiplayerTotalChargesCaptured";

	private const string SAVE_STATISTICS_MULTIPLAYER_TOTAL_ABILITIES_USED = "multiplayerTotalAbilitiesUsed";

	private const string SAVE_STATISTICS_TOTAL_EXPERIENCE_EARNED = "totalExperienceEarned";

	private const string SAVE_STATISTICS_TOTAL_INFLUENCE_POINTS_EARNED = "totalInfluencePointsEarned";

	private const string SAVE_STATISTICS_TOTAL_MONEY_SOFT_EARNED = "totalMoneySoftEarned";

	private const string SAVE_STATISTICS_FACEBOOK_LOGIN_REWARDED = "facebookLoginRewarded";

	private const string SAVE_STATISTICS_TOTAL_BOUGHT_IAPS = "totalBoughtIAPs";

	private const string SAVE_STATISTICS_TOTAL_BOUGHT_AMMUNITION = "totalBoughtAmmunition";

	private const string SAVE_STATISTICS_PANEL_AMMUNITION_SHOWN_LEVEL = "panelAmmunitionShownLevel";

	private const string SAVE_STATISTICS_TOTAL_GAMES_PLAYED = "totalGamesPlayed";

	private const string SAVE_STATISTICS_RATE_ME_LAST_OFFER_LEVEL = "ratemeLastOfferLevel";

	private const string SAVE_STATISTICS_MULTIPLAYER_LAST_PLAYED_KILLS = "multiplayerLastPlayedKills";

	private const string SAVE_STATISTICS_MULTIPLAYER_LAST_PLAYED_FLAGS = "multiplayerLastPlayedFlags";

	private const string SAVE_STATISTICS_MULTIPLAYER_LAST_PLAYED_CHARGES = "multiplayerLastPlayedCharges";

	private const string SAVE_STATISTICS_MULTIPLAYER_LAST_PLAYED_POWERUPS = "multiplayerLastPlayedPowerups";

	private List<Item> _items = new List<Item>
	{
		new ItemInt("openCount", true),
		new ItemInt("multiplayerGamesPlayed", true),
		new ItemInt("multiplayerGamesWon", true),
		new ItemInt("multiplayerTotalKills", true),
		new ItemMultiplayerTotalKillsOnVehicle("multiplayerTotalKillsOnVehicle", true),
		new ItemInt("multiplayerTotalKillsCollision", true),
		new ItemInt("multiplayerTotalKillsFlagCarriers", true),
		new ItemInt("multiplayerTotalDeaths", true),
		new ItemInt("multiplayerPowerupsCollected", true),
		new ItemInt("multiplayerPowerupsCollectedRepair", true),
		new ItemInt("multiplayerPowerupsCollectedDamage", true),
		new ItemInt("multiplayerTotalFlagsCaptured", true),
		new ItemFloat("multiplayerTotalTimeOnPoint", true),
		new ItemInt("multiplayerTotalChargesCaptured", true),
		new ItemInt("multiplayerTotalAbilitiesUsed", true),
		new ItemInt("totalExperienceEarned", true),
		new ItemInt("totalInfluencePointsEarned", true),
		new ItemInt("totalMoneySoftEarned", true),
		new ItemBool("facebookLoginRewarded", true),
		new ItemInt("totalBoughtIAPs", true),
		new ItemInt("totalBoughtAmmunition", true),
		new ItemInt("panelAmmunitionShownLevel", true),
		new ItemInt("totalGamesPlayed", true),
		new ItemInt("ratemeLastOfferLevel", true),
		new ItemInt("multiplayerLastPlayedKills", false),
		new ItemInt("multiplayerLastPlayedFlags", false),
		new ItemInt("multiplayerLastPlayedCharges", false),
		new ItemInt("multiplayerLastPlayedPowerups", false)
	};

	public int OpenCount
	{
		get
		{
			return GetInt("openCount");
		}
		set
		{
			SetInt("openCount", value);
		}
	}

	public int TotalGamesPlayed
	{
		get
		{
			return GetInt("totalGamesPlayed");
		}
		set
		{
			SetInt("totalGamesPlayed", value);
		}
	}

	public int MultiplayerGamesPlayed
	{
		get
		{
			return GetInt("multiplayerGamesPlayed");
		}
		set
		{
			SetInt("multiplayerGamesPlayed", value);
		}
	}

	public int MultiplayerGamesWon
	{
		get
		{
			return GetInt("multiplayerGamesWon");
		}
		set
		{
			SetInt("multiplayerGamesWon", value);
		}
	}

	public int MultiplayerTotalKills
	{
		get
		{
			return GetInt("multiplayerTotalKills");
		}
		set
		{
			SetInt("multiplayerTotalKills", value);
		}
	}

	public int MultiplayerTotalKillsCollision
	{
		get
		{
			return GetInt("multiplayerTotalKillsCollision");
		}
		set
		{
			SetInt("multiplayerTotalKillsCollision", value);
		}
	}

	public int MultiplayerTotalKillsFlagCarriers
	{
		get
		{
			return GetInt("multiplayerTotalKillsFlagCarriers");
		}
		set
		{
			SetInt("multiplayerTotalKillsFlagCarriers", value);
		}
	}

	public int MultiplayerTotalDeaths
	{
		get
		{
			return GetInt("multiplayerTotalDeaths");
		}
		set
		{
			SetInt("multiplayerTotalDeaths", value);
		}
	}

	public int MultiplayerPowerupsCollected
	{
		get
		{
			return GetInt("multiplayerPowerupsCollected");
		}
		set
		{
			SetInt("multiplayerPowerupsCollected", value);
		}
	}

	public int MultiplayerPowerupsCollectedRepair
	{
		get
		{
			return GetInt("multiplayerPowerupsCollectedRepair");
		}
		set
		{
			SetInt("multiplayerPowerupsCollectedRepair", value);
		}
	}

	public int MultiplayerPowerupsCollectedDamage
	{
		get
		{
			return GetInt("multiplayerPowerupsCollectedDamage");
		}
		set
		{
			SetInt("multiplayerPowerupsCollectedDamage", value);
		}
	}

	public int MultiplayerTotalFlagsCaptured
	{
		get
		{
			return GetInt("multiplayerTotalFlagsCaptured");
		}
		set
		{
			SetInt("multiplayerTotalFlagsCaptured", value);
		}
	}

	public float MultiplayerTotalTimeOnPoint
	{
		get
		{
			return GetFloat("multiplayerTotalTimeOnPoint");
		}
		set
		{
			SetFloat("multiplayerTotalTimeOnPoint", value);
		}
	}

	public int MultiplayerTotalChargesCaptured
	{
		get
		{
			return GetInt("multiplayerTotalChargesCaptured");
		}
		set
		{
			SetInt("multiplayerTotalChargesCaptured", value);
		}
	}

	public int MultiplayerTotalAbilitiesUsed
	{
		get
		{
			return GetInt("multiplayerTotalAbilitiesUsed");
		}
		set
		{
			SetInt("multiplayerTotalAbilitiesUsed", value);
		}
	}

	public int TotalExperienceEarned
	{
		get
		{
			return GetInt("totalExperienceEarned");
		}
		set
		{
			SetInt("totalExperienceEarned", value);
		}
	}

	public int TotalInfluencePointsEarned
	{
		get
		{
			return GetInt("totalInfluencePointsEarned");
		}
		set
		{
			SetInt("totalInfluencePointsEarned", value);
		}
	}

	public int TotalMoneySoftEarned
	{
		get
		{
			return GetInt("totalMoneySoftEarned");
		}
		set
		{
			SetInt("totalMoneySoftEarned", value);
		}
	}

	public bool FacebookLoginRewarded
	{
		get
		{
			return GetBool("facebookLoginRewarded");
		}
		set
		{
			SetBool("facebookLoginRewarded", value);
		}
	}

	public int RateMeLastOfferLevel
	{
		get
		{
			return GetInt("ratemeLastOfferLevel");
		}
		set
		{
			SetInt("ratemeLastOfferLevel", value);
		}
	}

	public int TotalBoughtIAPs
	{
		get
		{
			return GetInt("totalBoughtIAPs");
		}
		set
		{
			SetInt("totalBoughtIAPs", value);
		}
	}

	public int TotalBoughtAmmunition
	{
		get
		{
			return GetInt("totalBoughtAmmunition");
		}
		set
		{
			SetInt("totalBoughtAmmunition", value);
		}
	}

	public int PanelAmmunitionShownLevel
	{
		get
		{
			return GetInt("panelAmmunitionShownLevel");
		}
		set
		{
			SetInt("panelAmmunitionShownLevel", value);
		}
	}

	public int MultiplayerLastPlayedKills
	{
		get
		{
			return GetInt("multiplayerLastPlayedKills");
		}
		set
		{
			SetInt("multiplayerLastPlayedKills", value);
		}
	}

	public int MultiplayerLastPlayedFlags
	{
		get
		{
			return GetInt("multiplayerLastPlayedFlags");
		}
		set
		{
			SetInt("multiplayerLastPlayedFlags", value);
		}
	}

	public int MultiplayerLastPlayedCharges
	{
		get
		{
			return GetInt("multiplayerLastPlayedCharges");
		}
		set
		{
			SetInt("multiplayerLastPlayedCharges", value);
		}
	}

	public int MultiplayerLastPlayedPowerups
	{
		get
		{
			return GetInt("multiplayerLastPlayedPowerups");
		}
		set
		{
			SetInt("multiplayerLastPlayedPowerups", value);
		}
	}

	public void AddMultiplayerTotalKillsOnVehicle(string vehicleId, int delta)
	{
		ItemMultiplayerTotalKillsOnVehicle itemMultiplayerTotalKillsOnVehicle = Get<ItemMultiplayerTotalKillsOnVehicle>("multiplayerTotalKillsOnVehicle");
		itemMultiplayerTotalKillsOnVehicle.AddKills(vehicleId, delta);
	}

	public int Vehikill()
	{
		ItemMultiplayerTotalKillsOnVehicle itemMultiplayerTotalKillsOnVehicle = Get<ItemMultiplayerTotalKillsOnVehicle>("multiplayerTotalKillsOnVehicle");
		return itemMultiplayerTotalKillsOnVehicle.Vehikill();
	}

	private T Get<T>(string id) where T : Item
	{
		T val = _items.Find((Item a) => a.Id == id) as T;
		if (val == null)
		{
			return (T)null;
		}
		return val;
	}

	private int GetInt(string id)
	{
		ItemInt itemInt = Get<ItemInt>(id);
		if (itemInt != null)
		{
			return itemInt.Value;
		}
		return 0;
	}

	private void SetInt(string id, int v)
	{
		ItemInt itemInt = Get<ItemInt>(id);
		if (itemInt != null)
		{
			itemInt.Value = v;
		}
	}

	private float GetFloat(string id)
	{
		ItemFloat itemFloat = Get<ItemFloat>(id);
		if (itemFloat != null)
		{
			return itemFloat.Value;
		}
		return 0f;
	}

	private void SetFloat(string id, float v)
	{
		ItemFloat itemFloat = Get<ItemFloat>(id);
		if (itemFloat != null)
		{
			itemFloat.Value = v;
		}
	}

	private bool GetBool(string id)
	{
		ItemBool itemBool = Get<ItemBool>(id);
		if (itemBool != null)
		{
			return itemBool.Value;
		}
		return false;
	}

	private void SetBool(string id, bool v)
	{
		ItemBool itemBool = Get<ItemBool>(id);
		if (itemBool != null)
		{
			itemBool.Value = v;
		}
	}

	public void SetDefault()
	{
		foreach (Item item in _items)
		{
			item.SetDefault();
		}
	}

	public void Reset()
	{
		MultiplayerLastPlayedKills = 0;
		MultiplayerLastPlayedCharges = 0;
		MultiplayerLastPlayedFlags = 0;
		MultiplayerLastPlayedPowerups = 0;
	}

	public void Update(IDTGame game, ref IDTGame.Reward reward)
	{
		if (IsOnlineGame(game))
		{
			MultiplayerGame multiplayerGame = game as MultiplayerGame;
			int num = (reward.Victory ? 1 : 0);
			GamePlayer localPlayer = multiplayerGame.localPlayer;
			MultiplayerLastPlayedKills = localPlayer.killCount;
			MultiplayerGamesPlayed++;
			MultiplayerGamesWon += num;
			MultiplayerTotalKills += localPlayer.killCount;
			MultiplayerTotalKillsCollision += localPlayer.collisionKillCount;
			MultiplayerTotalDeaths += localPlayer.deathCount;
			MultiplayerPowerupsCollected += MultiplayerLastPlayedPowerups;
			string id = MonoSingleton<Player>.Instance.SelectedVehicle.Vehicle.id;
			AddMultiplayerTotalKillsOnVehicle(id, localPlayer.killCount);
			if (game is CTFGame)
			{
				CTFGame cTFGame = game as CTFGame;
				MultiplayerLastPlayedFlags = cTFGame.deliveredFlags;
				MultiplayerTotalFlagsCaptured += cTFGame.deliveredFlags;
				MultiplayerTotalKillsFlagCarriers += cTFGame.flagCourierKills;
			}
			else if (game is KOHGame)
			{
				MultiplayerTotalTimeOnPoint += (game as KOHGame).PlayerOnPointTime;
			}
			else if (game is CRTeamGame)
			{
				CRTeamGame cRTeamGame = game as CRTeamGame;
				MultiplayerLastPlayedCharges = cRTeamGame.collectedChargesCount;
				MultiplayerTotalChargesCaptured += cRTeamGame.collectedChargesCount;
			}
		}
		else if (!(game is SingleGame))
		{
		}
	}

	public void Update(CollectableItem item)
	{
		IDTGame instance = IDTGame.Instance;
		if (!(instance == null) && IsOnlineGame(instance))
		{
			MultiplayerLastPlayedPowerups++;
			if (item.ItemType == CollectableItemType.Health)
			{
				MultiplayerPowerupsCollectedRepair++;
			}
			else if (item.ItemType != CollectableItemType.Flag && item.CarryBuff != null && item.CarryBuff is DamageBoostConf)
			{
				MultiplayerPowerupsCollectedDamage++;
			}
		}
	}

	public void Update(BaseActiveAbility activeAbility)
	{
		IDTGame instance = IDTGame.Instance;
		if (!(instance == null) && IsOnlineGame(instance))
		{
			MultiplayerTotalAbilitiesUsed++;
		}
	}

	public bool LoadXml(XmlElement root)
	{
		if (root == null)
		{
			return true;
		}
		foreach (Item item in _items)
		{
			if (item.Storable)
			{
				item.LoadXml(root);
			}
		}
		return true;
	}

	public void SaveXml(XmlDocument document, XmlElement root)
	{
		foreach (Item item in _items)
		{
			if (item.Storable)
			{
				item.SaveXml(document, root);
			}
		}
	}

	private bool IsOnlineGame(IDTGame game)
	{
		MultiplayerGame multiplayerGame = game as MultiplayerGame;
		return multiplayerGame != null && multiplayerGame.match.isOnline;
	}
}
