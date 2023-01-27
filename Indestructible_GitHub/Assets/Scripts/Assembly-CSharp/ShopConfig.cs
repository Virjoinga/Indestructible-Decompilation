using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

public class ShopConfig
{
	private static List<Type> _shopConfigItemTypes;

	public List<ShopConfigItem> items;

	private List<ShopConfigGroup> _groups;

	public static List<Type> shopConfigItemTypes
	{
		get
		{
			if (_shopConfigItemTypes == null)
			{
				_shopConfigItemTypes = new List<Type>();
				Assembly assembly = Assembly.GetAssembly(typeof(ShopConfigItem));
				Type[] types = assembly.GetTypes();
				Type[] array = types;
				foreach (Type type in array)
				{
					if (type.IsSubclassOf(typeof(ShopConfigItem)))
					{
						_shopConfigItemTypes.Add(type);
					}
				}
			}
			return _shopConfigItemTypes;
		}
	}

	[XmlArray("groups")]
	[XmlArrayItem("group")]
	public List<ShopConfigGroup> groups
	{
		get
		{
			return (_groups != null) ? _groups.FindAll((ShopConfigGroup x) => x.visible != false) : null;
		}
		set
		{
			_groups = value;
		}
	}

	public List<ShopConfigGroup> rawGroups
	{
		get
		{
			return _groups;
		}
		set
		{
			_groups = value;
		}
	}

	public void SetDefaultGroupOrder()
	{
		int num = 0;
		foreach (ShopConfigGroup group in _groups)
		{
			if (!group.order.HasValue)
			{
				group.order = num;
			}
			group.SetDefaultOrder();
			num++;
		}
	}

	public ShopConfigItem FindItemById(string id)
	{
		return items.Find((ShopConfigItem x) => x.id.Equals(id));
	}

	public ShopConfigGroup FindGroup(string id)
	{
		return _groups.Find((ShopConfigGroup x) => x.id.Equals(id));
	}

	public void Print()
	{
		if (!LoggerSingleton<ShopConfigLogger>.IsEnabledFor(20))
		{
			return;
		}
		for (int i = 0; i < items.Count; i++)
		{
		}
		for (int j = 0; j < _groups.Count; j++)
		{
			if (_groups[j].rawItemRefs != null)
			{
				for (int k = 0; k < _groups[j].rawItemRefs.Count; k++)
				{
				}
			}
		}
	}

	public static ShopConfig Load(Stream stream)
	{
		XmlAttributes xmlAttributes = new XmlAttributes();
		foreach (Type shopConfigItemType in shopConfigItemTypes)
		{
			xmlAttributes.XmlArrayItems.Add(new XmlArrayItemAttribute(shopConfigItemType));
		}
		XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();
		xmlAttributeOverrides.Add(typeof(ShopConfig), "items", xmlAttributes);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(ShopConfig), xmlAttributeOverrides);
		ShopConfig shopConfig = (ShopConfig)xmlSerializer.Deserialize(stream);
		shopConfig.SetDefaultGroupOrder();
		SortGroups(shopConfig);
		ReferenceGroupItems(shopConfig);
		shopConfig.Print();
		return shopConfig;
	}

	public ShopConfig Clone()
	{
		ShopConfig res = new ShopConfig();
		res.items = new List<ShopConfigItem>();
		items.ForEach(delegate(ShopConfigItem x)
		{
			res.items.Add(x.Clone());
		});
		res._groups = new List<ShopConfigGroup>();
		_groups.ForEach(delegate(ShopConfigGroup x)
		{
			res._groups.Add(x.CloneUnbound());
		});
		ReferenceGroupItems(res);
		return res;
	}

	public ShopConfig Override(ShopOverride overs)
	{
		ShopConfig shopConfig = Clone();
		DateTime dateTime = DateTime.UtcNow.ToLocalTime();
		foreach (ShopOverride.Override @override in overs.overrides)
		{
			List<ShopOverride.Override.TimeRange> list = new List<ShopOverride.Override.TimeRange>();
			foreach (ShopOverride.Override.Timing timing in @override.timings)
			{
				ShopOverride.Override.TimeRange closeRange = timing.GetCloseRange(dateTime);
				if (closeRange != null)
				{
					list.Add(closeRange);
				}
			}
			list.Sort((ShopOverride.Override.TimeRange x, ShopOverride.Override.TimeRange y) => (x.start < y.start) ? (-1) : ((x.start > y.start) ? 1 : 0));
			ShopOverride.Override.TimeRange timeRange = new ShopOverride.Override.TimeRange(dateTime, TimeSpan.Zero);
			foreach (ShopOverride.Override.TimeRange item in list)
			{
				if (item.Intersects(timeRange))
				{
					timeRange.Expand(item);
				}
			}
			if (timeRange.span > TimeSpan.Zero)
			{
				ApplyOverride(shopConfig, @override.config, timeRange.start + timeRange.span);
			}
		}
		shopConfig.SetDefaultGroupOrder();
		SortGroups(shopConfig);
		ReferenceGroupItems(shopConfig);
		return shopConfig;
	}

	private void ApplyOverride(ShopConfig destConfig, ShopConfig sourceConfig, DateTime overrideEnd)
	{
		foreach (ShopConfigItem item in sourceConfig.items)
		{
			ShopConfigItem shopConfigItem = destConfig.FindItemById(item.id);
			if (shopConfigItem != null)
			{
				item.Override(shopConfigItem);
				shopConfigItem.overrideEnd = overrideEnd;
			}
			else
			{
				destConfig.items.Add(item.Clone());
			}
		}
		foreach (ShopConfigGroup group in sourceConfig._groups)
		{
			ShopConfigGroup shopConfigGroup = destConfig.FindGroup(group.id);
			if (shopConfigGroup != null)
			{
				group.Supplement(shopConfigGroup);
			}
			else
			{
				destConfig._groups.Add(group.CloneUnbound());
			}
		}
	}

	private static void SortGroups(ShopConfig config)
	{
		config._groups.Sort(delegate(ShopConfigGroup a, ShopConfigGroup b)
		{
			double? order = a.order;
			if (order.HasValue)
			{
				double? order2 = b.order;
				if (order2.HasValue && order.Value < order2.Value)
				{
					return -1;
				}
			}
			double? order3 = a.order;
			if (order3.HasValue)
			{
				double? order4 = b.order;
				if (order4.HasValue && order3.Value > order4.Value)
				{
					return 1;
				}
			}
			return 0;
		});
		foreach (ShopConfigGroup group in config._groups)
		{
			group.Sort();
		}
	}

	private static void ReferenceGroupItems(ShopConfig config)
	{
		foreach (ShopConfigGroup group in config._groups)
		{
			if (group.rawItemRefs == null)
			{
				continue;
			}
			ShopConfigGroup.Reference refer;
			foreach (ShopConfigGroup.Reference rawItemRef in group.rawItemRefs)
			{
				refer = rawItemRef;
				ShopConfigItem item = config.items.Find((ShopConfigItem x) => x.id.Equals(refer.itemId));
				refer.item = item;
				refer.group = group;
			}
		}
	}
}
