using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using Glu.Localization;
using Glu.RuntimeDataProtection;
using UnityEngine;

public class Player : MonoSingleton<Player>
{
	private class LevelData
	{
		public int Experience;
	}

	public class RewardInfo
	{
		public int WinXP;

		public int WinSoftMoney;

		public int LoseXP;

		public int LoseSoftMoney;
	}

	private const int _bronzeLeagueMaxPoints = 250;

	private const int _silverLeagueMaxPoints = 500;

	private const string SAVE_FILE_NAME = "idt_save.dat";

	private const string SAVE_ELEMENT_ROOT = "save";

	private const string SAVE_ROOT_VERSION = "version";

	private const string SAVE_ELEMENT_INFO = "info";

	private const string SAVE_INFO_MONEY_HARD = "moneyHard";

	private const string SAVE_INFO_MONEY_SOFT = "moneySoft";

	private const string SAVE_INFO_EXPERIENCE = "experience";

	private const string SAVE_INFO_LEVEL = "level";

	private const string SAVE_INFO_TALENT_POINTS = "talentPoints";

	private const string SAVE_INFO_INFLUENCE_POINTS = "influencePoints";

	private const string SAVE_INFO_ELO_RATE = "eloRate";

	private const string SAVE_INFO_PLAYER_NAME = "name";

	private const string SAVE_INFO_LAST_WON_BOSS_FIGHT = "lastWonBossFight";

	private const string SAVE_INFO_CAMPAIGN_COMPLETED = "campaignCompleted";

	private const string SAVE_ELEMENT_ACHIEVEMENTS = "achievements";

	private const string SAVE_ELEMENT_STATISTICS = "statistics";

	private const string SAVE_ELEMENT_TUTORIAL = "tutorial";

	private const string SAVE_ELEMENT_TALENTS = "talents";

	private const string SAVE_ELEMENT_TALENT = "talent";

	private const string SAVE_TALENT_ID = "id";

	private const string SAVE_TALENT_LEVEL = "level";

	private const string SAVE_ELEMENT_BOOSTS = "boosts";

	private const string SAVE_ELEMENT_BOOST = "boost";

	private const string SAVE_BOOST_ID = "id";

	private const string SAVE_BOOST_DURATION = "duration";

	private const string SAVE_BOOST_START_TIME = "startTime";

	private const string SAVE_BOOST_BOOST_FOREVER = "boostForever";

	private const string SAVE_ELEMENT_BUNDLES = "bundles";

	private const string SAVE_ELEMENT_BUNDLE = "bundle";

	private const string SAVE_BUNDLE_ID = "id";

	private const string SAVE_BUNDLE_START_TIME = "startTime";

	private const string SAVE_BUNDLE_EXPIRED = "expired";

	private const string SAVE_ELEMENT_VEHICLES = "vehicles";

	private const string SAVE_VEHICLES_SELECTED = "selected";

	private const string SAVE_ELEMENT_ITEMS = "items";

	private const string SAVE_ELEMENT_ITEM = "item";

	private const string SAVE_ITEM_ITEM_ID = "id";

	private const string SAVE_ITEM_ITEM_COUNT = "count";

	private const string SAVE_ELEMENT_VEHICLE = "vehicle";

	private const string SAVE_VEHICLE_VEHICLE = "vehicle";

	private const string SAVE_VEHICLE_WEAPON = "weapon";

	private const string SAVE_VEHICLE_ARMOR = "armor";

	private const string SAVE_VEHICLE_BODY = "body";

	private const string SAVE_VEHICLE_AMMUNITION = "ammunition";

	private const string SAVE_ELEMENT_COMPONENTS = "components";

	private const string SAVE_ELEMENT_COMPONENT = "component";

	private const string SAVE_COMPONENT_ID = "id";

	private const string SAVE_ELEMENT_FUEL = "fuel";

	private const string SAVE_FUEL_LEVEL = "level";

	private const string SAVE_FUEL_LEVEL_TIME = "levelTime";

	private const string SAVE_FUEL_FREEZE_TIME = "freezeTime";

	private const string SAVE_FUEL_FREEZE_FOREVER = "freezeForever";

	private const string SAVE_ELEMENT_DAILY_CHALLENGES = "dailyChallenges";

	private const string LEVELS_ELEMENT_ROOT = "levels";

	private const string LEVELS_ELEMENT_LEVEL = "level";

	private const string LEVELS_LEVEL_EXPERIENCE = "experience";

	private const string REWARDS_ELEMENT_ROOT = "rewards";

	private const string REWARDS_ELEMENT_REWARD = "reward";

	private const string REWARDS_ELEMENT_WIN_EXPERIENCE = "win_experience";

	private const string REWARDS_ELEMENT_LOSE_EXPERIENCE = "lose_experience";

	private const string REWARDS_ELEMENT_WIN_SOFT_MONEY = "win_soft_money";

	private const string REWARDS_ELEMENT_LOSE_SOFT_MONEY = "lose_soft_money";

	public int EloRate;

	public Encrypted<int> InfluencePoints = 0;

	public int TalentPoints;

	public Encrypted<int> Experience = 0;

	public Encrypted<int> Level = 1;

	public string Name = "YOU";

	public Encrypted<int> MoneySoft = 0;

	private Encrypted<int> m_moneyHard = 0;

	public HashSet<ShopItem> BoughtItems = new HashSet<ShopItem>();

	public HashSet<GarageVehicle> BoughtVehicles = new HashSet<GarageVehicle>();

	public GarageVehicle SelectedVehicle;

	public List<PlayerTalent> BoughtTalents = new List<PlayerTalent>();

	public List<PlayerBoost> BoughtBoosts = new List<PlayerBoost>();

	public List<PlayerBundle> PlayerBundles = new List<PlayerBundle>();

	public PlayerAchievements Achievements = new PlayerAchievements();

	public PlayerStatistics Statistics = new PlayerStatistics();

	public PlayerTutorial Tutorial = new PlayerTutorial();

	public DailyChallenges Challenges = new DailyChallenges();

	public string LastPlayedType = string.Empty;

	public string LastPlayedMode = string.Empty;

	public string LastPlayedGame = string.Empty;

	public string LastPlayedMap = string.Empty;

	public int LastPlayedPlayers;

	public int LastWonBossFight = -1;

	public bool CampaignCompleted;

	private byte[] CRYPT_KEY = new byte[8] { 174, 143, 231, 229, 238, 12, 170, 169 };

	private byte[] CRYPT_IV = new byte[8] { 115, 17, 207, 87, 119, 152, 188, 204 };

	private List<LevelData> _levels = new List<LevelData>();

	private List<RewardInfo> _rewards = new List<RewardInfo>();

	public Encrypted<int> MoneyHard
	{
		get
		{
			return m_moneyHard;
		}
	}

	public int League
	{
		get
		{
			return GetLeague(InfluencePoints);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Load();
		LoadLevels();
		LoadMPRewards();
		Challenges.LoadSettings();
		Statistics.OpenCount++;
		Save();
		GameAnalytics.EventPlayerVehicle(SelectedVehicle);
		GameAnalytics.EventOpenCount(Statistics.OpenCount, Level);
	}

	public bool ResetTalents()
	{
		ShopItemPrice itemPrice = MonoSingleton<ShopController>.Instance.GetItemPrice("price_talents_reset");
		if (itemPrice == null)
		{
			return false;
		}
		if (!Buy(itemPrice))
		{
			return false;
		}
		TalentPoints += GetTalentPointsSpent();
		BoughtTalents.Clear();
		Save();
		GameAnalytics.EventResetTalents(itemPrice);
		GameAnalytics.EventResetTalents(itemPrice);
		return true;
	}

	public void LoadDefault()
	{
		LoadDefault("vehicle_punisher");
	}

	public void SelectDefaultVehicle(string vehicleId)
	{
		BoughtVehicles.Clear();
		BoughtItems.Clear();
		GarageVehicle selectedVehicle = LoadDefaultVehicle(vehicleId);
		SelectedVehicle = selectedVehicle;
	}

	private void LoadDefault(string vehicleId)
	{
		BoughtTalents.Clear();
		BoughtVehicles.Clear();
		BoughtItems.Clear();
		BoughtBoosts.Clear();
		PlayerBundles.Clear();
		Challenges.SetDefault();
		Achievements.SetDefault();
		Statistics.SetDefault();
		Tutorial.SetDefault();
		SelectDefaultVehicle(vehicleId);
		EloRate = 1200;
		InfluencePoints = 0;
		TalentPoints = 0;
		MoneySoft = 120;
		AddMoneyHard(7, "CREDIT_NEW_PLAYER_CURRENCY", "StartingBalance", "StartingBalance");
		Experience = 0;
		Level = 1;
		LastWonBossFight = -1;
		CampaignCompleted = false;
		Save();
	}

	private GarageVehicle LoadDefaultVehicle(string vehicleId)
	{
		return BuyItemVehicle(vehicleId);
	}

	private ShopItem LoadBoughtItem(string itemId)
	{
		ShopItem item = MonoSingleton<ShopController>.Instance.GetItem(itemId);
		AddBought(item);
		return item;
	}

	private bool LoadXmlItems(XmlElement root)
	{
		foreach (XmlElement item2 in root)
		{
			string attribute = XmlUtils.GetAttribute<string>(item2, "id");
			ShopItem item = MonoSingleton<ShopController>.Instance.GetItem(attribute);
			if (item == null)
			{
				return false;
			}
			item.Count = XmlUtils.GetAttribute<int>(item2, "count");
			AddBought(item);
		}
		return true;
	}

	private bool LoadXmlComponents(GarageVehicle vehicle, XmlElement root)
	{
		if (root != null)
		{
			int num = 0;
			foreach (XmlElement item in root)
			{
				if (num >= vehicle.Components.Length)
				{
					return false;
				}
				string attribute = XmlUtils.GetAttribute<string>(item, "id");
				if (attribute != string.Empty)
				{
					ShopItemComponent itemComponent = MonoSingleton<ShopController>.Instance.GetItemComponent(attribute);
					if (itemComponent == null)
					{
						return false;
					}
					vehicle.Components[num] = itemComponent;
				}
				num++;
			}
		}
		return true;
	}

	private bool LoadXmlFuel(GarageVehicle vehicle, XmlElement root)
	{
		int num = 5;
		vehicle.Fuel.Level = XmlUtils.GetAttribute(root, "level", num);
		vehicle.Fuel.LevelTime = XmlUtils.GetAttribute<long>(root, "levelTime");
		vehicle.Fuel.FreezeTime = XmlUtils.GetAttribute<long>(root, "freezeTime");
		vehicle.Fuel.FreezeForever = XmlUtils.GetAttribute<bool>(root, "freezeForever");
		if (vehicle.Fuel.Level > num)
		{
			vehicle.Fuel.Level = num;
		}
		return true;
	}

	private bool LoadXmlVehicle(XmlElement root)
	{
		string attribute = XmlUtils.GetAttribute<string>(root, "vehicle");
		string attribute2 = XmlUtils.GetAttribute<string>(root, "weapon");
		string attribute3 = XmlUtils.GetAttribute<string>(root, "armor");
		string attribute4 = XmlUtils.GetAttribute<string>(root, "body");
		string attribute5 = XmlUtils.GetAttribute<string>(root, "ammunition");
		if (attribute == string.Empty)
		{
			return false;
		}
		if (attribute2 == string.Empty)
		{
			return false;
		}
		if (attribute4 == string.Empty)
		{
			return false;
		}
		GarageVehicle garageVehicle = FindBoughtVehicle(attribute);
		if (garageVehicle != null)
		{
			return false;
		}
		GarageVehicle garageVehicle2 = BuyItemVehicle(attribute);
		garageVehicle2.Weapon = MonoSingleton<ShopController>.Instance.GetItemWeapon(attribute2);
		garageVehicle2.Armor = MonoSingleton<ShopController>.Instance.GetItemArmor(attribute3);
		garageVehicle2.Body = MonoSingleton<ShopController>.Instance.GetItemBody(attribute4);
		garageVehicle2.Ammunition = MonoSingleton<ShopController>.Instance.GetItemAmmunition(attribute5);
		AddBought(garageVehicle2.Weapon);
		AddBought(garageVehicle2.Armor);
		AddBought(garageVehicle2.Body);
		XmlElement root2 = root["components"];
		LoadXmlComponents(garageVehicle2, root2);
		XmlElement root3 = root["fuel"];
		LoadXmlFuel(garageVehicle2, root3);
		return true;
	}

	private bool LoadXmlVehicles(XmlElement root)
	{
		foreach (XmlElement item in root)
		{
			if (!LoadXmlVehicle(item))
			{
				return false;
			}
		}
		return true;
	}

	private bool LoadXmlTalent(XmlElement root)
	{
		int attribute = XmlUtils.GetAttribute<int>(root, "level");
		if (attribute < 1 || attribute > 3)
		{
			return false;
		}
		string attribute2 = XmlUtils.GetAttribute<string>(root, "id");
		ShopItemTalent itemTalent = MonoSingleton<ShopController>.Instance.GetItemTalent(attribute2);
		if (itemTalent == null)
		{
			return false;
		}
		PlayerTalent playerTalent = new PlayerTalent();
		playerTalent.Item = itemTalent;
		playerTalent.Level = attribute;
		BoughtTalents.Add(playerTalent);
		return true;
	}

	private bool LoadXmlTalents(XmlElement root)
	{
		if (root == null)
		{
			return false;
		}
		foreach (XmlElement item in root)
		{
			if (!LoadXmlTalent(item))
			{
				return false;
			}
		}
		return true;
	}

	private bool LoadXmlBoosts(XmlElement root)
	{
		if (root == null)
		{
			return true;
		}
		foreach (XmlElement item in root)
		{
			string attribute = XmlUtils.GetAttribute<string>(item, "id");
			IAPShopItemBoost itemBoost = MonoSingleton<ShopController>.Instance.GetItemBoost(attribute);
			if (itemBoost != null)
			{
				PlayerBoost playerBoost = new PlayerBoost();
				playerBoost.Duration = XmlUtils.GetAttribute<long>(item, "duration");
				playerBoost.StartTime = XmlUtils.GetAttribute<long>(item, "startTime");
				playerBoost.BoostForever = XmlUtils.GetAttribute<bool>(item, "boostForever");
				playerBoost.Item = itemBoost;
				BoughtBoosts.Add(playerBoost);
			}
		}
		return true;
	}

	private bool LoadXmlBundles(XmlElement root)
	{
		if (root == null)
		{
			return true;
		}
		foreach (XmlElement item in root)
		{
			string attribute = XmlUtils.GetAttribute<string>(item, "id");
			ShopItemBundle itemBundle = MonoSingleton<ShopController>.Instance.GetItemBundle(attribute);
			if (itemBundle != null)
			{
				PlayerBundle playerBundle = new PlayerBundle();
				playerBundle.StartTime = XmlUtils.GetAttribute<long>(item, "startTime");
				playerBundle.Expired = XmlUtils.GetAttribute<bool>(item, "expired");
				playerBundle.Item = itemBundle;
				PlayerBundles.Add(playerBundle);
			}
		}
		return true;
	}

	public GarageVehicle FindBoughtVehicle(string vehicleId)
	{
		foreach (GarageVehicle boughtVehicle in BoughtVehicles)
		{
			if (boughtVehicle.Vehicle.id == vehicleId)
			{
				return boughtVehicle;
			}
		}
		return null;
	}

	private bool LoadXml(XmlDocument document)
	{
		XmlElement xmlElement = document["save"];
		if (xmlElement == null)
		{
			return false;
		}
		XmlElement element = xmlElement["info"];
		m_moneyHard = XmlUtils.GetAttribute<int>(element, "moneyHard");
		if ((int)m_moneyHard > 0)
		{
			m_moneyHard = 0;
		}
		MoneySoft = XmlUtils.GetAttribute<int>(element, "moneySoft");
		Level = XmlUtils.GetAttribute<int>(element, "level");
		Experience = XmlUtils.GetAttribute<int>(element, "experience");
		TalentPoints = XmlUtils.GetAttribute<int>(element, "talentPoints");
		InfluencePoints = XmlUtils.GetAttribute<int>(element, "influencePoints");
		EloRate = XmlUtils.GetAttribute<int>(element, "eloRate");
		Name = XmlUtils.GetAttribute<string>(element, "name");
		LastWonBossFight = XmlUtils.GetAttribute(element, "lastWonBossFight", -1);
		CampaignCompleted = XmlUtils.GetAttribute<bool>(element, "campaignCompleted");
		XmlElement xmlElement2 = xmlElement["vehicles"];
		if (!LoadXmlVehicles(xmlElement2))
		{
			return false;
		}
		string attribute = XmlUtils.GetAttribute<string>(xmlElement2, "selected");
		SelectedVehicle = FindBoughtVehicle(attribute);
		if (SelectedVehicle == null)
		{
			return false;
		}
		XmlElement root = xmlElement["items"];
		if (!LoadXmlItems(root))
		{
			return false;
		}
		XmlElement root2 = xmlElement["talents"];
		if (!LoadXmlTalents(root2))
		{
			return false;
		}
		XmlElement root3 = xmlElement["boosts"];
		if (!LoadXmlBoosts(root3))
		{
			return false;
		}
		XmlElement root4 = xmlElement["bundles"];
		if (!LoadXmlBundles(root4))
		{
			return false;
		}
		XmlElement root5 = xmlElement["statistics"];
		if (!Statistics.LoadXml(root5))
		{
			return false;
		}
		XmlElement root6 = xmlElement["tutorial"];
		if (!Tutorial.LoadXml(root6))
		{
			return false;
		}
		XmlElement root7 = xmlElement["achievements"];
		if (!Achievements.LoadXml(root7))
		{
			return false;
		}
		XmlElement root8 = xmlElement["dailyChallenges"];
		if (!Challenges.LoadXml(root8))
		{
			return false;
		}
		if ((int)Level <= 0)
		{
			Level = 1;
		}
		return true;
	}

	public bool Load()
	{
		XmlDocument xmlDocument = new XmlDocument();
		byte[] array = null;
		bool flag = true;
		try
		{
			array = StorageManagerAndroid.Read("idt_save.dat");
		}
		catch (Exception)
		{
			flag = false;
		}
		if (flag)
		{
			flag = array != null;
		}
		if (flag)
		{
			flag = array.Length > 0;
		}
		if (flag)
		{
			MemoryStream memoryStream = new MemoryStream(array);
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			ICryptoTransform cryptoTransform = dESCryptoServiceProvider.CreateDecryptor(CRYPT_KEY, CRYPT_IV);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
			try
			{
				xmlDocument.Load(cryptoStream);
			}
			catch (Exception)
			{
				flag = false;
			}
			cryptoStream.Close();
			memoryStream.Close();
		}
		if (flag)
		{
			flag = LoadXml(xmlDocument);
			if (!flag)
			{
			}
		}
		if (!flag)
		{
			LoadDefault();
		}
		return flag;
	}

	private void SaveXmlItems(XmlDocument document, XmlElement root)
	{
		foreach (ShopItem boughtItem in BoughtItems)
		{
			XmlElement xmlElement = document.CreateElement("item");
			XmlUtils.SetAttribute(xmlElement, "id", boughtItem.id);
			XmlUtils.SetAttribute(xmlElement, "count", boughtItem.GetCount().ToString());
			root.AppendChild(xmlElement);
		}
	}

	private void SaveXmlComponents(GarageVehicle vehicle, XmlDocument document, XmlElement root)
	{
		ShopItemComponent[] components = vehicle.Components;
		foreach (ShopItemComponent shopItemComponent in components)
		{
			XmlElement xmlElement = document.CreateElement("component");
			string value = ((shopItemComponent == null) ? string.Empty : shopItemComponent.id);
			XmlUtils.SetAttribute(xmlElement, "id", value);
			root.AppendChild(xmlElement);
		}
	}

	private void SaveXmlFuel(GarageVehicle vehicle, XmlDocument document, XmlElement root)
	{
		XmlUtils.SetAttribute(root, "level", vehicle.Fuel.Level);
		XmlUtils.SetAttribute(root, "levelTime", vehicle.Fuel.LevelTime);
		XmlUtils.SetAttribute(root, "freezeTime", vehicle.Fuel.FreezeTime);
		XmlUtils.SetAttribute(root, "freezeForever", vehicle.Fuel.FreezeForever);
	}

	private void SaveXmlVehicles(XmlDocument document, XmlElement root)
	{
		foreach (GarageVehicle boughtVehicle in BoughtVehicles)
		{
			if (boughtVehicle.Vehicle != null && boughtVehicle.Weapon != null && boughtVehicle.Body != null)
			{
				XmlElement xmlElement = document.CreateElement("vehicle");
				XmlUtils.SetAttribute(xmlElement, "vehicle", boughtVehicle.Vehicle.id);
				XmlUtils.SetAttribute(xmlElement, "weapon", boughtVehicle.Weapon.id);
				XmlUtils.SetAttribute(xmlElement, "body", boughtVehicle.Body.id);
				if (boughtVehicle.Ammunition != null)
				{
					XmlUtils.SetAttribute(xmlElement, "ammunition", boughtVehicle.Ammunition.id);
				}
				root.AppendChild(xmlElement);
				if (boughtVehicle.Armor != null)
				{
					XmlUtils.SetAttribute(xmlElement, "armor", boughtVehicle.Armor.id);
				}
				XmlElement xmlElement2 = document.CreateElement("components");
				SaveXmlComponents(boughtVehicle, document, xmlElement2);
				xmlElement.AppendChild(xmlElement2);
				XmlElement xmlElement3 = document.CreateElement("fuel");
				SaveXmlFuel(boughtVehicle, document, xmlElement3);
				xmlElement.AppendChild(xmlElement3);
			}
		}
	}

	private void SaveXmlTalents(XmlDocument document, XmlElement root)
	{
		foreach (PlayerTalent boughtTalent in BoughtTalents)
		{
			if (boughtTalent.Level >= 1 && boughtTalent.Level <= 3 && boughtTalent.Item != null)
			{
				XmlElement xmlElement = document.CreateElement("talent");
				XmlUtils.SetAttribute(xmlElement, "id", boughtTalent.Item.id);
				XmlUtils.SetAttribute(xmlElement, "level", boughtTalent.Level);
				root.AppendChild(xmlElement);
			}
		}
	}

	private void SaveXmlBoosts(XmlDocument document, XmlElement root)
	{
		foreach (PlayerBoost boughtBoost in BoughtBoosts)
		{
			if (boughtBoost.Item != null)
			{
				XmlElement xmlElement = document.CreateElement("boost");
				XmlUtils.SetAttribute(xmlElement, "id", boughtBoost.Item.id);
				XmlUtils.SetAttribute(xmlElement, "duration", boughtBoost.Duration);
				XmlUtils.SetAttribute(xmlElement, "startTime", boughtBoost.StartTime);
				XmlUtils.SetAttribute(xmlElement, "boostForever", boughtBoost.BoostForever);
				root.AppendChild(xmlElement);
			}
		}
	}

	private void SaveXmlBundles(XmlDocument document, XmlElement root)
	{
		foreach (PlayerBundle playerBundle in PlayerBundles)
		{
			if (playerBundle.Item != null)
			{
				XmlElement xmlElement = document.CreateElement("bundle");
				XmlUtils.SetAttribute(xmlElement, "id", playerBundle.Item.id);
				XmlUtils.SetAttribute(xmlElement, "startTime", playerBundle.StartTime);
				XmlUtils.SetAttribute(xmlElement, "expired", playerBundle.Expired);
				root.AppendChild(xmlElement);
			}
		}
	}

	private bool SaveXml(XmlDocument document)
	{
		if (SelectedVehicle == null)
		{
			return false;
		}
		XmlElement xmlElement = document.CreateElement("save");
		xmlElement.SetAttribute("version", "3");
		document.AppendChild(xmlElement);
		XmlElement xmlElement2 = document.CreateElement("info");
		XmlUtils.SetAttribute(xmlElement2, "moneyHard", m_moneyHard);
		XmlUtils.SetAttribute(xmlElement2, "moneySoft", MoneySoft);
		XmlUtils.SetAttribute(xmlElement2, "experience", Experience);
		XmlUtils.SetAttribute(xmlElement2, "level", Level);
		XmlUtils.SetAttribute(xmlElement2, "talentPoints", TalentPoints);
		XmlUtils.SetAttribute(xmlElement2, "influencePoints", InfluencePoints);
		XmlUtils.SetAttribute(xmlElement2, "eloRate", EloRate);
		XmlUtils.SetAttribute(xmlElement2, "name", Name);
		XmlUtils.SetAttribute(xmlElement2, "lastWonBossFight", LastWonBossFight);
		XmlUtils.SetAttribute(xmlElement2, "campaignCompleted", CampaignCompleted);
		xmlElement.AppendChild(xmlElement2);
		XmlElement xmlElement3 = document.CreateElement("vehicles");
		XmlUtils.SetAttribute(xmlElement3, "selected", SelectedVehicle.Vehicle.id);
		SaveXmlVehicles(document, xmlElement3);
		xmlElement.AppendChild(xmlElement3);
		XmlElement xmlElement4 = document.CreateElement("items");
		SaveXmlItems(document, xmlElement4);
		xmlElement.AppendChild(xmlElement4);
		XmlElement xmlElement5 = document.CreateElement("talents");
		SaveXmlTalents(document, xmlElement5);
		xmlElement.AppendChild(xmlElement5);
		XmlElement xmlElement6 = document.CreateElement("boosts");
		SaveXmlBoosts(document, xmlElement6);
		xmlElement.AppendChild(xmlElement6);
		XmlElement xmlElement7 = document.CreateElement("bundles");
		SaveXmlBundles(document, xmlElement7);
		xmlElement.AppendChild(xmlElement7);
		XmlElement xmlElement8 = document.CreateElement("statistics");
		Statistics.SaveXml(document, xmlElement8);
		xmlElement.AppendChild(xmlElement8);
		XmlElement xmlElement9 = document.CreateElement("tutorial");
		Tutorial.SaveXml(document, xmlElement9);
		xmlElement.AppendChild(xmlElement9);
		XmlElement xmlElement10 = document.CreateElement("achievements");
		Achievements.SaveXml(document, xmlElement10);
		xmlElement.AppendChild(xmlElement10);
		XmlElement xmlElement11 = document.CreateElement("dailyChallenges");
		Challenges.SaveXml(document, xmlElement11);
		xmlElement.AppendChild(xmlElement11);
		return true;
	}

	public bool Save()
	{
		MemoryStream memoryStream = new MemoryStream();
		XmlDocument xmlDocument = new XmlDocument();
		bool flag = SaveXml(xmlDocument);
		if (flag)
		{
			DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
			ICryptoTransform cryptoTransform = dESCryptoServiceProvider.CreateEncryptor(CRYPT_KEY, CRYPT_IV);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
			try
			{
				xmlDocument.Save(cryptoStream);
			}
			catch (Exception)
			{
				flag = false;
			}
			cryptoStream.Close();
		}
		memoryStream.Close();
		if (flag)
		{
			byte[] array = memoryStream.ToArray();
			if (flag)
			{
				flag = array != null;
			}
			if (flag)
			{
				flag = array.Length > 0;
			}
			if (flag)
			{
				try
				{
					StorageManagerAndroid.Write("idt_save.dat", array);
				}
				catch (Exception)
				{
					flag = false;
				}
			}
		}
		return flag;
	}

	public void Buy(IAPShopItem item)
	{
		if (item is IAPShopItemSimple)
		{
			IAPShopItemSimple iAPShopItemSimple = item as IAPShopItemSimple;
			int priceHard = iAPShopItemSimple.GetPriceHard();
			int priceSoft = iAPShopItemSimple.GetPriceSoft();
			int? priceHard2 = iAPShopItemSimple.PriceHard;
			if (priceHard2.HasValue)
			{
				MonoSingleton<Player>.Instance.AddMoneyHard(priceHard, "CREDIT_GC_PURCHASE", iAPShopItemSimple.productId, "IAP");
			}
			else
			{
				int? priceSoft2 = iAPShopItemSimple.PriceSoft;
				if (priceSoft2.HasValue)
				{
					MonoSingleton<Player>.Instance.AddMoneySoft(priceSoft, "CREDIT_SC", iAPShopItemSimple.productId);
				}
			}
			MonoSingleton<Player>.Instance.Statistics.TotalBoughtIAPs++;
		}
		else if (item is IAPShopItemBoost)
		{
			IAPShopItemBoost item2 = item as IAPShopItemBoost;
			PlayerBoost playerBoost = new PlayerBoost();
			playerBoost.Buy(item2);
			bool flag = BoughtBoosts.Count > 0;
			if (flag)
			{
				playerBoost.Duration = BoughtBoosts[0].Duration;
				playerBoost.Duration += 864000000000L;
			}
			BoughtBoosts.Clear();
			BoughtBoosts.Add(playerBoost);
			UnityEngine.Object @object = UnityEngine.Object.FindObjectOfType(typeof(DialogGameRewards));
			DialogGameRewards dialogGameRewards = @object as DialogGameRewards;
			if (dialogGameRewards != null)
			{
				dialogGameRewards.OnBought(item2, flag);
			}
		}
		MonoSingleton<Player>.Instance.Save();
		if (GameConstants.BuildType == "google")
		{
			Dialogs.IAPPurchaseSuccessful(item);
		}
		MonoSingleton<GameController>.Instance.ConfirmPurchase();
		GameAnalytics.EventIAPItemPurchased(item);
	}

	public int GetTalentLevel(string id)
	{
		PlayerTalent playerTalent = FindBoughtTalent(id);
		return (playerTalent != null) ? playerTalent.Level : 0;
	}

	public PlayerTalent FindBoughtTalent(string id)
	{
		foreach (PlayerTalent boughtTalent in BoughtTalents)
		{
			if (boughtTalent.Item.id == id)
			{
				return boughtTalent;
			}
		}
		return null;
	}

	public int GetTalentPointsSpent()
	{
		int num = 0;
		foreach (PlayerTalent boughtTalent in BoughtTalents)
		{
			num += boughtTalent.Level;
		}
		return num;
	}

	public bool Buy(ShopItem item)
	{
		return Buy(item, ShopItemCurrency.None);
	}

	public bool Buy(ShopItem item, ShopItemCurrency currency)
	{
		if (item.ItemType == ShopItemType.Talent)
		{
			return BuyTalent(item as ShopItemTalent);
		}
		bool flag = BuyItem(item, currency);
		if (flag)
		{
			PostBuyItem(item, currency);
		}
		return flag;
	}

	private GarageVehicle BuyItemVehicle(string vehicleId)
	{
		ShopItemVehicle itemVehicle = MonoSingleton<ShopController>.Instance.GetItemVehicle(vehicleId);
		ShopItemWeapon itemWeapon = MonoSingleton<ShopController>.Instance.GetItemWeapon(itemVehicle.WeaponId);
		ShopItemArmor itemArmor = MonoSingleton<ShopController>.Instance.GetItemArmor(itemVehicle.ArmorId);
		ShopItemBody itemBody = MonoSingleton<ShopController>.Instance.GetItemBody(itemVehicle.BodyId);
		GarageVehicle garageVehicle = new GarageVehicle();
		garageVehicle.Body = itemBody;
		garageVehicle.Armor = itemArmor;
		garageVehicle.Weapon = itemWeapon;
		garageVehicle.Vehicle = itemVehicle;
		BoughtVehicles.Add(garageVehicle);
		AddBought(itemBody);
		AddBought(itemArmor);
		AddBought(itemWeapon);
		AddBought(itemVehicle);
		return garageVehicle;
	}

	private void PostBuyItem(ShopItem item, ShopItemCurrency currency)
	{
		if (item.ItemType == ShopItemType.Vehicle)
		{
			ShopItemVehicle shopItemVehicle = item as ShopItemVehicle;
			SelectedVehicle = BuyItemVehicle(shopItemVehicle.id);
			UnityEngine.Object @object = UnityEngine.Object.FindObjectOfType(typeof(GarageManager));
			GarageManager garageManager = @object as GarageManager;
			garageManager.ActivatePanel("PanelGarage");
		}
		else if (item.ItemType == ShopItemType.Bundle)
		{
			ShopItemBundle shopItemBundle = item as ShopItemBundle;
			GarageVehicle garageVehicle = (SelectedVehicle = BuyItemVehicle(shopItemBundle.VehicleId));
			garageVehicle.Weapon = MonoSingleton<ShopController>.Instance.GetItemWeapon(shopItemBundle.WeaponId);
			garageVehicle.Armor = MonoSingleton<ShopController>.Instance.GetItemArmor(shopItemBundle.ArmorId);
			garageVehicle.Body = MonoSingleton<ShopController>.Instance.GetItemBody(shopItemBundle.BodyId);
			AddBought(garageVehicle.Weapon);
			AddBought(garageVehicle.Armor);
			AddBought(garageVehicle.Body);
			for (int i = 0; i < shopItemBundle.Components.Length; i++)
			{
				ShopItemBundle.Component component = shopItemBundle.Components[i];
				if (component == null)
				{
					continue;
				}
				ShopItemComponent itemComponent = MonoSingleton<ShopController>.Instance.GetItemComponent(component.Id);
				if (itemComponent != null)
				{
					string id = string.Format("slot_component_{0}", i + 1);
					ShopItemSlot itemSlot = MonoSingleton<ShopController>.Instance.GetItemSlot(id);
					if (!itemSlot.IsLocked())
					{
						garageVehicle.Components[i] = itemComponent;
					}
					AddBought(itemComponent);
				}
			}
			UnityEngine.Object object2 = UnityEngine.Object.FindObjectOfType(typeof(GarageManager));
			GarageManager garageManager2 = object2 as GarageManager;
			garageManager2.ActivatePanel("PanelCustomization");
		}
		else if (item.ItemType == ShopItemType.Ammunition)
		{
			MonoSingleton<Player>.Instance.Statistics.TotalBoughtAmmunition++;
		}
	}

	private bool BuyItem(ShopItem item, ShopItemCurrency currency)
	{
		bool flag = item.HasCurrencyHard(currency);
		bool flag2 = item.HasCurrencySoft(currency);
		if (flag)
		{
			int num = item.GetPriceHard();
			if (num < 0)
			{
				num = 0;
			}
			GameAnalytics.EventTryToBuyDurableHard(item, num);
			if ((int)MoneyHard >= num)
			{
				SubMoneyHard(num, "DEBIT_IN_APP_PURCHASE", item.id);
				UpdateStoredItems(item, ShopItemCurrency.Hard, num);
				GameAnalytics.EventLoseHard(num);
				return true;
			}
			GamePlayHaven.Placement("bank_launch_insufficient_funds");
			Dialogs.IAPShop("iaps_hard", false);
		}
		else if (flag2)
		{
			int num2 = item.GetPriceSoft();
			if (num2 < 0)
			{
				num2 = 0;
			}
			GameAnalytics.EventTryToBuyDurableSoft(item, num2);
			if ((int)MoneySoft >= num2)
			{
				MoneySoft = (int)MoneySoft - num2;
				UpdateStoredItems(item, ShopItemCurrency.Soft, num2);
				GameAnalytics.EventLoseSoft(num2);
				return true;
			}
			GamePlayHaven.Placement("bank_launch_insufficient_funds");
			Dialogs.IAPShop("iaps_soft", false);
		}
		return false;
	}

	private bool UpdateStoredItems(ShopItem item, ShopItemCurrency currency, int price)
	{
		if (item.IsStorable())
		{
			bool flag = true;
			if (item.GetPackCount() > 0)
			{
				foreach (ShopItem boughtItem in BoughtItems)
				{
					if (boughtItem.id == item.id)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				BoughtItems.Add(item);
			}
			item.Count = item.GetCount() + ((item.GetPackCount() > 0) ? item.GetPackCount() : 0);
			switch (currency)
			{
			case ShopItemCurrency.Soft:
				GameAnalytics.EventPurchaseDurableSoft(item, price);
				break;
			case ShopItemCurrency.Hard:
				GameAnalytics.EventPurchaseDurableHard(item, price);
				break;
			}
			return true;
		}
		return false;
	}

	public int BoughtItemsCount(ShopItemType type)
	{
		int num = 0;
		foreach (ShopItem boughtItem in BoughtItems)
		{
			if (boughtItem.ItemType == type)
			{
				num++;
			}
		}
		return num;
	}

	private bool BuyTalent(ShopItemTalent item)
	{
		if (TalentPoints < 1)
		{
			return false;
		}
		if (item.ParentId != string.Empty)
		{
			PlayerTalent playerTalent = FindBoughtTalent(item.ParentId);
			if (playerTalent == null)
			{
				return false;
			}
		}
		PlayerTalent playerTalent2 = FindBoughtTalent(item.id);
		if (playerTalent2 == null)
		{
			playerTalent2 = new PlayerTalent();
			playerTalent2.Item = item;
			playerTalent2.Level = 1;
			BoughtTalents.Add(playerTalent2);
			TalentPoints--;
			Save();
			GameAnalytics.EventTalentBought(playerTalent2);
			return true;
		}
		int? levelsCount = item.LevelsCount;
		if (levelsCount.HasValue && playerTalent2.Level < levelsCount.Value)
		{
			playerTalent2.Level++;
			TalentPoints--;
			Save();
			GameAnalytics.EventTalentBought(playerTalent2);
			return true;
		}
		return false;
	}

	private ShopItem GetBoughtItemById(string id)
	{
		foreach (ShopItem boughtItem in BoughtItems)
		{
			if (boughtItem.id == id)
			{
				return boughtItem;
			}
		}
		return null;
	}

	public bool IsBought(string id)
	{
		ShopItem boughtItemById = GetBoughtItemById(id);
		if (boughtItemById != null && !boughtItemById.IsEmpty())
		{
			return true;
		}
		return false;
	}

	public bool IsBought(ShopItem item)
	{
		return IsBought(item.id);
	}

	public bool AddBought(ShopItem item)
	{
		if (item == null)
		{
			return false;
		}
		if (!IsBought(item))
		{
			BoughtItems.Add(item);
			return true;
		}
		return false;
	}

	public void ConsumeConsumable()
	{
		SelectedVehicle.Fuel.Spend();
		if (SelectedVehicle.Ammunition != null && SelectedVehicle.Ammunition.Consume() == 0)
		{
			SelectedVehicle.Ammunition = null;
		}
		BoughtItems.RemoveWhere((ShopItem item) => item.IsEmpty());
	}

	public bool IsEquipped(ShopItem item)
	{
		return SelectedVehicle.Contains(item);
	}

	public bool IsEnoughFuel()
	{
		return SelectedVehicle.Fuel.GetLevel() > 0;
	}

	public bool IsEnoughPower()
	{
		int power = SelectedVehicle.GetPower();
		int totalPower = SelectedVehicle.GetTotalPower();
		return power <= totalPower;
	}

	public bool CanMount(ShopItem item, int slot)
	{
		return SelectedVehicle.CanMount(item, slot);
	}

	public void StartLevel(string level)
	{
		StartLevel(level, LoadingScene.Content.None, false, null);
	}

	private void StartLevel(string level, LoadingScene.Content content, bool waitForMatchObjects, Action<string> finishDelegate)
	{
		LoadingScene.Level = level;
		LoadingScene.ContentType = content;
		LoadingScene.finishDelegate = finishDelegate;
		LoadingScene.WaitForMatchObjects = waitForMatchObjects;
		Application.LoadLevel("LoadingScene");
	}

	public void StartMatchLeftLevel(string level)
	{
		StartLevel(level, LoadingScene.Content.Game, false, null);
	}

	public void StartMatchLevel(string level, Action<string> finishDelegate)
	{
		StartLevel(level, LoadingScene.Content.Game, true, finishDelegate);
	}

	public string GetTeamName(int teamId)
	{
		switch (teamId)
		{
		case 0:
			return Strings.GetString("IDS_TEAM_NAME_BLUE");
		case 1:
			return Strings.GetString("IDS_TEAM_NAME_RED");
		default:
			return "ERROR";
		}
	}

	public Color GetTeamColor(int teamId)
	{
		switch (teamId)
		{
		case 0:
			return new Color(0.15f, 0.6f, 0.85f);
		case 1:
			return new Color(0.75f, 0f, 0f);
		default:
			return Color.white;
		}
	}

	public int GetTeamId(int layer)
	{
		if (layer == LayerMask.NameToLayer("PlayerTeam0"))
		{
			return 0;
		}
		if (layer == LayerMask.NameToLayer("PlayerTeam1"))
		{
			return 1;
		}
		if (layer == LayerMask.NameToLayer("AITeam0"))
		{
			return 0;
		}
		if (layer == LayerMask.NameToLayer("AITeam1"))
		{
			return 1;
		}
		return -1;
	}

	private void LoadMPRewards()
	{
		TextAsset textAsset = BundlesUtils.Load("Assets/Bundles/Configuration/idt_rewards.xml") as TextAsset;
		MemoryStream inStream = new MemoryStream(textAsset.bytes, false);
		bool flag = true;
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(inStream);
		}
		catch (Exception)
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		XmlElement xmlElement = xmlDocument["rewards"];
		XmlNodeList xmlNodeList = xmlElement.SelectNodes("reward");
		foreach (XmlNode item in xmlNodeList)
		{
			XmlElement element = item as XmlElement;
			RewardInfo rewardInfo = new RewardInfo();
			rewardInfo.WinXP = XmlUtils.GetAttribute<int>(element, "win_experience");
			rewardInfo.WinSoftMoney = XmlUtils.GetAttribute<int>(element, "win_soft_money");
			rewardInfo.LoseXP = XmlUtils.GetAttribute<int>(element, "lose_experience");
			rewardInfo.LoseSoftMoney = XmlUtils.GetAttribute<int>(element, "lose_soft_money");
			_rewards.Add(rewardInfo);
		}
	}

	public void SetMultiplayerReward(bool win, ref IDTGame.Reward reward)
	{
		int num = (int)Level - 1;
		if (num >= _rewards.Count)
		{
			num = _rewards.Count - 1;
		}
		RewardInfo rewardInfo = _rewards[num];
		reward.ExperiencePoints = ((!win) ? rewardInfo.LoseXP : rewardInfo.WinXP);
		reward.MoneySoft = ((!win) ? rewardInfo.LoseSoftMoney : rewardInfo.WinSoftMoney);
	}

	private void LoadLevels()
	{
		TextAsset textAsset = BundlesUtils.Load("Assets/Bundles/Configuration/idt_levels.xml") as TextAsset;
		MemoryStream inStream = new MemoryStream(textAsset.bytes, false);
		bool flag = true;
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(inStream);
		}
		catch (Exception)
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		XmlElement xmlElement = xmlDocument["levels"];
		XmlNodeList xmlNodeList = xmlElement.SelectNodes("level");
		foreach (XmlNode item in xmlNodeList)
		{
			XmlElement element = item as XmlElement;
			LevelData levelData = new LevelData();
			levelData.Experience = XmlUtils.GetAttribute<int>(element, "experience");
			_levels.Add(levelData);
		}
	}

	public int GetLevelsCount()
	{
		return _levels.Count;
	}

	public void GetLevelExperience(ref int min, ref int max)
	{
		if ((int)Level < _levels.Count && (int)Level > 0)
		{
			min = _levels[(int)Level - 1].Experience;
			max = _levels[Level].Experience;
		}
	}

	public PlayerBundle GetPlayerBundle(string id)
	{
		foreach (PlayerBundle playerBundle in PlayerBundles)
		{
			if (playerBundle.Item.id == id)
			{
				return playerBundle;
			}
		}
		return null;
	}

	private void ActivateBundles(long ticks)
	{
		bool flag = false;
		ShopConfigGroup group = MonoSingleton<ShopController>.Instance.GetGroup("bundles");
		foreach (ShopConfigGroup.Reference itemRef in group.itemRefs)
		{
			ShopItemBundle shopItemBundle = itemRef.item as ShopItemBundle;
			PlayerBundle playerBundle = GetPlayerBundle(shopItemBundle.id);
			if (playerBundle == null && shopItemBundle.CanActivate())
			{
				PlayerBundle playerBundle2 = new PlayerBundle();
				PlayerBundles.Insert(0, playerBundle2);
				playerBundle2.Activate(shopItemBundle, ticks);
				flag = true;
			}
		}
		if (flag)
		{
			MonoSingleton<Player>.Instance.Save();
		}
	}

	public List<PlayerBundle> GetActiveBundles()
	{
		List<PlayerBundle> list = new List<PlayerBundle>();
		foreach (PlayerBundle playerBundle in PlayerBundles)
		{
			if (playerBundle.IsActive())
			{
				list.Add(playerBundle);
			}
		}
		return list;
	}

	public void UpdateBundles()
	{
		long ticks = DateTime.UtcNow.Ticks;
		foreach (PlayerBundle playerBundle in PlayerBundles)
		{
			playerBundle.Update(ticks);
		}
		ActivateBundles(ticks);
	}

	public void UpdateBoosts()
	{
		int count = BoughtBoosts.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			PlayerBoost playerBoost = BoughtBoosts[num];
			float seconds = playerBoost.GetSeconds();
			if (seconds <= 0f)
			{
				BoughtBoosts.RemoveAt(num);
			}
		}
	}

	public void UpdateEloRate(TeamGame game, bool win)
	{
		double num = 0.0;
		double num2 = 0.0;
		int teamID = game.localPlayer.teamID;
		foreach (MatchPlayer player in game.match.players)
		{
			double num3 = Math.Pow(10.0, (double)player.rating / 400.0);
			if (player.teamID == teamID)
			{
				num += num3;
			}
			num2 += num3;
		}
		double num4 = ((!win) ? 0.0 : 1.0);
		double num5 = num / num2;
		double num6 = 10.0;
		if (Statistics.MultiplayerGamesPlayed < 30)
		{
			num6 = 30.0;
		}
		else if (EloRate < 2400)
		{
			num6 = 15.0;
		}
		int eloRate = EloRate;
		double a = num6 * (num4 - num5);
		AddEloRate((int)Math.Round(a));
		Save();
	}

	public void BoostReward(ref IDTGame.Reward reward)
	{
		UpdateBoosts();
		if (BoughtBoosts.Count > 0)
		{
			reward.ExperiencePoints *= 2;
			reward.MoneySoft *= 2;
		}
	}

	public void AddReward(ref IDTGame.Reward reward, string type, string desc)
	{
		reward.LevelsGained = AddExperience(reward.ExperiencePoints);
		if (reward.LevelsGained > 0)
		{
			TalentPoints += reward.LevelsGained;
		}
		reward.LeagueChange = AddInfluencePoints(reward.InfluencePoints);
		AddMoneySoft(reward.MoneySoft, type, desc);
		Save();
	}

	public int AddReward(ShopItemReward reward, string source)
	{
		int num = 0;
		if (reward.MoneyHard.HasValue)
		{
			AddMoneyHard(reward.MoneyHard.Value, "CREDIT_IN_GAME_AWARD", reward.GetString(), source);
		}
		if (reward.MoneySoft.HasValue)
		{
			AddMoneySoft(reward.MoneySoft.Value, "CREDIT_IN_GAME_AWARD", reward.GetString());
		}
		if (reward.Experience.HasValue)
		{
			num = AddExperience(reward.Experience.Value);
			if (num > 0)
			{
				TalentPoints += num;
			}
		}
		if (reward.InfluencePoints.HasValue)
		{
			AddInfluencePoints(reward.InfluencePoints.Value);
		}
		Save();
		return num;
	}

	public int AddExperience(int experience)
	{
		if (experience <= 0)
		{
			return 0;
		}
		Statistics.TotalExperienceEarned += experience;
		Experience = (int)Experience + experience;
		int count = _levels.Count;
		int num = Level;
		while ((int)Level < count)
		{
			LevelData levelData = _levels[Level];
			if ((int)Experience >= levelData.Experience)
			{
				Level = (int)Level + 1;
				GameAnalytics.EventLevelUp();
				if (GameConstants.BuildType == "amazon")
				{
					ASocial.Amazon.Sync("Reached Level " + Level, string.Empty);
				}
				continue;
			}
			break;
		}
		if ((int)Level >= count)
		{
			Level = count;
			Experience = _levels[(int)Level - 1].Experience;
		}
		return (int)Level - num;
	}

	public void AddMoneySoft(int money, string type, string desc)
	{
		if (money > 0)
		{
			Statistics.TotalMoneySoftEarned += money;
			GameAnalytics.EventEarnedSoft(money);
			MoneySoft = (int)MoneySoft + money;
		}
	}

	public void AddMoneyHard(int money, string type, string desc, string source)
	{
		if (money > 0)
		{
			GameAnalytics.EventEarnedHard(money, source);
		}
	}

	public void SubMoneyHard(int money, string type, string desc)
	{

	}

	public int AddInfluencePoints(int points)
	{
		int league = League;
		if (points != 0)
		{
			Statistics.TotalInfluencePointsEarned += points;
			GameAnalytics.EventEarnedInfluencePoints(points);
			InfluencePoints = (int)InfluencePoints + points;
			if ((int)InfluencePoints < 0)
			{
				InfluencePoints = 0;
			}
		}
		return League - league;
	}

	public void AddEloRate(int delta)
	{
		if (delta != 0)
		{
			GameAnalytics.EventEarnedEloRate(delta);
			EloRate += delta;
			if (EloRate < 0)
			{
				EloRate = 0;
			}
		}
	}

	public int GetLeague(int influencePoints)
	{
		return (influencePoints >= 250) ? ((influencePoints < 500) ? 1 : 2) : 0;
	}

	public int GetLeagueRange(int league)
	{
		switch (league)
		{
		case 0:
			return 250;
		case 1:
			return 250;
		default:
			return 0;
		}
	}

	public int GetLeagueMin(int league)
	{
		switch (league)
		{
		case 0:
			return 0;
		case 1:
			return 250;
		case 2:
			return 500;
		default:
			return 0;
		}
	}

	public static string GetLeagueName(int league)
	{
		switch (league)
		{
		case 0:
			return Strings.GetString("IDS_LEAGUE_BRONZE");
		case 1:
			return Strings.GetString("IDS_LEAGUE_SILVER");
		default:
			return Strings.GetString("IDS_LEAGUE_GOLD");
		}
	}

	public static string GetLeagueSpriteName(int league)
	{
		switch (league)
		{
		case 0:
			return "Bronze";
		case 1:
			return "Silver";
		default:
			return "Gold";
		}
	}

	public string ValidateMessage(string text)
	{
		string text2 = string.Empty;
		if (text != null)
		{
			foreach (char c in text)
			{
				if (IsEnglishAlpha(c) || IsNumeric(c) || IsAdditionalAlpha(c))
				{
					text2 += c;
				}
			}
		}
		return text2;
	}

	public bool IsAdditionalAlpha(char c)
	{
		return " ~!?@#$%^&*()_+-=\\{}[]:;\"'<>.,/".IndexOf(c) != -1;
	}

	public bool IsEnglishAlpha(char c)
	{
		return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
	}

	public bool IsNumeric(char c)
	{
		return c >= '0' && c <= '9';
	}

	public bool ValidateName(string name, ref string s)
	{
		s = string.Empty;
		if (name == null)
		{
			return false;
		}
		foreach (char c in name)
		{
			bool flag = IsEnglishAlpha(c) || IsNumeric(c);
			if (!flag && Strings.Locale == "ru")
			{
				flag = c >= 'А' && c <= 'я';
			}
			if (flag)
			{
				s += c;
			}
		}
		if (s.Length < 3)
		{
			return false;
		}
		if (s.Length > 10)
		{
			return false;
		}
		return true;
	}
}
