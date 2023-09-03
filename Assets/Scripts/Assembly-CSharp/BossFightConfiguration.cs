using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[XmlType("bossCampaign")]
public class BossFightConfiguration
{
	[XmlArray("fights")]
	public BossFightConfig[] BossFights;

	private static BossFightConfiguration _instance;

	public static BossFightConfiguration Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Load();
			}
			return _instance;
		}
	}

	public int FightIdx(string id)
	{
		if (BossFights == null || BossFights.Length <= 0)
		{
			return -1;
		}
		for (int i = 0; i < BossFights.Length; i++)
		{
			BossFightConfig bossFightConfig = BossFights[i];
			if (id == bossFightConfig.Id)
			{
				return i;
			}
		}
		return -1;
	}

	public int BossesCount(int checkLength)
	{
		if (BossFights == null || BossFights.Length <= 0 || checkLength <= 0)
		{
			return 0;
		}
		int num = Mathf.Min(BossFights.Length, checkLength);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			BossFightConfig bossFightConfig = BossFights[i];
			if (bossFightConfig.Bosses != null && bossFightConfig.Bosses.Length > 0)
			{
				num2++;
			}
		}
		return num2;
	}

	public static BossFightConfiguration Load()
	{
		TextAsset textAsset = BundlesUtils.Load("Assets/Bundles/Configuration/idt_boss_campaign.xml") as TextAsset;
		if (textAsset != null)
		{
			MemoryStream stream = new MemoryStream(textAsset.bytes, false);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(BossFightConfiguration));
			try
			{
				object obj = xmlSerializer.Deserialize(stream);
				BossFightConfiguration bossFightConfiguration = obj as BossFightConfiguration;
				if (bossFightConfiguration != null)
				{
					return bossFightConfiguration;
				}
			}
			catch (Exception)
			{
			}
		}
		return null;
	}
}
