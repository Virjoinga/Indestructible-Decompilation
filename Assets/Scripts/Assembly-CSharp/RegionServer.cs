using System.Collections.Generic;

public static class RegionServer
{
	public enum Kind
	{
		UnitedStates = 0,
		Europe = 1,
		Asia = 2,
		Automatic = 3,
		Undefined = 4
	}

	public class Info
	{
		public Kind Kind;

		public string NameId;

		public string ShortNameId;

		public Info(Kind kind, string nameId, string shortNameId)
		{
			Kind = kind;
			NameId = nameId;
			ShortNameId = shortNameId;
		}
	}

	private static List<Info> Infos = new List<Info>
	{
		new Info(Kind.UnitedStates, "IDS_SERVER_REGION_UNITED_STATES", "IDS_SERVER_REGION_UNITED_STATES_SHORT"),
		new Info(Kind.Europe, "IDS_SERVER_REGION_EUROPE", "IDS_SERVER_REGION_EUROPE_SHORT"),
		new Info(Kind.Asia, "IDS_SERVER_REGION_ASIA", "IDS_SERVER_REGION_ASIA_SHORT"),
		new Info(Kind.Automatic, "IDS_SERVER_REGION_AUTOMATIC", "IDS_SERVER_REGION_AUTOMATIC_SHORT")
	};

	public static Kind ToKind(int addressIndex)
	{
		if (addressIndex >= 0 && addressIndex < 3)
		{
			return (Kind)addressIndex;
		}
		return Kind.Automatic;
	}

	public static int ToAddressIndex(Kind kind)
	{
		if (kind >= Kind.Automatic)
		{
			return -1;
		}
		return (int)kind;
	}

	public static Info GetServerInfo(Kind kind)
	{
		return Infos.Find((Info a) => a.Kind == kind);
	}
}
