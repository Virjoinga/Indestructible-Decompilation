using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[XmlType("configuration")]
public class GameConfiguration
{
	[XmlElement("fueling")]
	public VehicleFueling Fueling = new VehicleFueling();

	[XmlElement("boosting")]
	public RewardsBoosting Boosting = new RewardsBoosting();

	[XmlElement("bundles")]
	public LimitedTimeBundles Bundles = new LimitedTimeBundles();

	[XmlElement("customMatch")]
	public CustomMultiplayerMatch CustomMatch = new CustomMultiplayerMatch();

	[XmlElement("rankedMatchmaker")]
	public RankedMatchmakerConfig RankedMatchmaker = new RankedMatchmakerConfig();

	[XmlElement("vehicleGrade")]
	public VehicleGradeLevels VehicleGrade = new VehicleGradeLevels();

	public static GameConfiguration Load()
	{
		TextAsset textAsset = BundlesUtils.Load("Assets/Bundles/Configuration/idt_configuration.xml") as TextAsset;
		if (textAsset != null)
		{
			MemoryStream stream = new MemoryStream(textAsset.bytes, false);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(GameConfiguration));
			try
			{
				object obj = xmlSerializer.Deserialize(stream);
				GameConfiguration gameConfiguration = obj as GameConfiguration;
				if (gameConfiguration != null)
				{
					return gameConfiguration;
				}
			}
			catch (Exception)
			{
			}
		}
		return null;
	}
}
