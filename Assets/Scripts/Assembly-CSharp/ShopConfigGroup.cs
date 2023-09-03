using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public class ShopConfigGroup
{
	public class Reference
	{
		public ShopConfigGroup group;

		[XmlAttribute("id")]
		public string id;

		[XmlAttribute("itemId")]
		public string itemId;

		[XmlAttribute("order")]
		public double? order;

		[XmlAttribute("visible")]
		public bool? visible;

		public ShopConfigItem item;

		public Reference()
		{
			id = string.Empty;
			itemId = string.Empty;
			item = null;
		}

		public void Override(Reference dest)
		{
			if (!string.IsNullOrEmpty(id))
			{
				dest.id = string.Copy(id);
			}
			if (!string.IsNullOrEmpty(itemId))
			{
				dest.itemId = string.Copy(itemId);
			}
			if (order.HasValue)
			{
				dest.order = order.Value;
			}
			if (visible.HasValue)
			{
				dest.visible = visible.Value;
			}
		}

		public Reference CloneUnbound()
		{
			Reference reference = new Reference();
			Override(reference);
			return reference;
		}
	}

	private List<Reference> _itemRefs;

	[XmlAttribute("id")]
	public string id;

	[XmlAttribute("nameId")]
	public string nameId;

	[XmlAttribute("prefab")]
	public string prefab;

	[XmlAttribute("visible")]
	public bool? visible;

	[XmlAttribute("order")]
	public double? order;

	[XmlArray("itemRefs")]
	[XmlArrayItem("itemRef")]
	public List<Reference> itemRefs
	{
		get
		{
			return (_itemRefs != null) ? _itemRefs.FindAll((Reference x) => x.visible != false) : null;
		}
		set
		{
			_itemRefs = value;
		}
	}

	public List<Reference> rawItemRefs
	{
		get
		{
			return _itemRefs;
		}
		set
		{
			_itemRefs = value;
		}
	}

	public ShopConfigGroup()
	{
		id = string.Empty;
		nameId = string.Empty;
		prefab = string.Empty;
	}

	public void SetDefaultOrder()
	{
		if (_itemRefs == null)
		{
			return;
		}
		int num = 0;
		foreach (Reference itemRef in _itemRefs)
		{
			if (!itemRef.order.HasValue)
			{
				itemRef.order = num;
			}
			num++;
		}
	}

	public void Supplement(ShopConfigGroup dest)
	{
		if (!string.IsNullOrEmpty(id))
		{
			dest.id = string.Copy(id);
		}
		if (!string.IsNullOrEmpty(nameId))
		{
			dest.nameId = string.Copy(nameId);
		}
		if (visible.HasValue)
		{
			dest.visible = visible.Value;
		}
		if (order.HasValue)
		{
			dest.order = order.Value;
		}
		if (!string.IsNullOrEmpty(prefab))
		{
			dest.prefab = string.Copy(prefab);
		}
		if (_itemRefs == null)
		{
			return;
		}
		foreach (Reference itemRef in _itemRefs)
		{
			Reference reference = dest.FindReference(itemRef.id);
			if (reference != null)
			{
				itemRef.Override(reference);
				continue;
			}
			if (dest._itemRefs == null)
			{
				dest._itemRefs = new List<Reference>();
			}
			dest._itemRefs.Add(itemRef.CloneUnbound());
		}
	}

	public ShopConfigGroup CloneUnbound()
	{
		ShopConfigGroup shopConfigGroup = new ShopConfigGroup();
		Supplement(shopConfigGroup);
		return shopConfigGroup;
	}

	public void Sort()
	{
		if (_itemRefs == null)
		{
			return;
		}
		Comparison<Reference> comparison = delegate(Reference a, Reference b)
		{
			double? num = a.order;
			if (num.HasValue)
			{
				double? num2 = b.order;
				if (num2.HasValue && num.Value < num2.Value)
				{
					return -1;
				}
			}
			double? num3 = a.order;
			if (num3.HasValue)
			{
				double? num4 = b.order;
				if (num4.HasValue && num3.Value > num4.Value)
				{
					return 1;
				}
			}
			return 0;
		};
		_itemRefs.Sort(comparison);
	}

	public Reference FindReference(string id)
	{
		return (_itemRefs == null) ? null : _itemRefs.Find((Reference x) => x.id.Equals(id));
	}
}
