using System.Collections.Generic;
using System.Xml;

public class DC_StoreStringSet : DailyChallenges.DailyChallenge
{
	private const string SAVE_XML_ITEMS = "items";

	private const string SAVE_XML_ITEM = "item";

	private const string SAVE_XML_ITEM_ID = "id";

	protected HashSet<string> _items = new HashSet<string>();

	public DC_StoreStringSet(string id)
		: base(id)
	{
	}

	public override void Reset()
	{
		base.Reset();
		_items.Clear();
	}

	public override void LoadXml(XmlElement root)
	{
		base.LoadXml(root);
		_items.Clear();
		XmlElement xmlElement = root["items"];
		if (xmlElement == null)
		{
			return;
		}
		foreach (XmlElement item in xmlElement)
		{
			if (item.Name == "item")
			{
				string attribute = XmlUtils.GetAttribute<string>(item, "id");
				_items.Add(attribute);
			}
		}
	}

	public override void SaveXml(XmlDocument document, XmlElement root)
	{
		base.SaveXml(document, root);
		XmlElement xmlElement = document.CreateElement("items");
		foreach (string item in _items)
		{
			XmlElement xmlElement2 = document.CreateElement("item");
			XmlUtils.SetAttribute(xmlElement2, "id", item);
			xmlElement.AppendChild(xmlElement2);
		}
		root.AppendChild(xmlElement);
	}
}
