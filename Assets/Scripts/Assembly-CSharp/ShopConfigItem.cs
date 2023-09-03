using System;
using System.Xml.Serialization;

public abstract class ShopConfigItem
{
	[XmlAttribute("id")]
	public string id;

	[XmlAttribute("nameId")]
	public string nameId;

	[XmlAttribute("prefab")]
	public string prefab;

	public DateTime? overrideEnd;

	public TimeSpan overrideTimeLeft
	{
		get
		{
			DateTime? dateTime = overrideEnd;
			if (dateTime.HasValue && overrideEnd.HasValue)
			{
				return overrideEnd.Value - DateTime.UtcNow.ToLocalTime();
			}
			return TimeSpan.Zero;
		}
	}

	public virtual bool readyToShow
	{
		get
		{
			return true;
		}
	}

	public ShopConfigItem()
	{
		id = string.Empty;
		nameId = string.Empty;
		prefab = string.Empty;
	}

	public virtual void Override(ShopConfigItem dest)
	{
		if (!string.IsNullOrEmpty(id))
		{
			dest.id = string.Copy(id);
		}
		if (!string.IsNullOrEmpty(nameId))
		{
			dest.nameId = string.Copy(nameId);
		}
		if (!string.IsNullOrEmpty(prefab))
		{
			dest.prefab = string.Copy(prefab);
		}
	}

	public abstract ShopConfigItem Clone();

	public virtual string GetDescription()
	{
		string text = GetType().ToString() + " ";
		text = text + "id = " + id;
		text = text + " nameId = " + nameId;
		return text + " prefab = " + prefab;
	}
}
