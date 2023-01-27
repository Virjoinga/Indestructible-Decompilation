using System;
using System.Collections.Generic;
using UnityEngine;

public class VehicleComponentsMounter : MonoBehaviour
{
	[Serializable]
	public class TalentInfo
	{
		public string IDName;

		public int Points = 1;
	}

	public string[] Components;

	public TalentInfo[] Talents;

	private void Start()
	{
		Vehicle component = GetComponent<Vehicle>();
		if (!component)
		{
			return;
		}
		LinkedList<Vehicle.ComponentsMountInfo> linkedList = new LinkedList<Vehicle.ComponentsMountInfo>();
		string[] components = Components;
		foreach (string id in components)
		{
			ShopItemComponent itemComponent = MonoSingleton<ShopController>.Instance.GetItemComponent(id);
			if (itemComponent != null)
			{
				linkedList.AddLast(new Vehicle.ComponentsMountInfo(itemComponent.ComponentPrefab, 1));
			}
		}
		TalentInfo[] talents = Talents;
		foreach (TalentInfo talentInfo in talents)
		{
			ShopItemTalent itemTalent = MonoSingleton<ShopController>.Instance.GetItemTalent(talentInfo.IDName);
			if (itemTalent != null)
			{
				linkedList.AddLast(new Vehicle.ComponentsMountInfo(itemTalent.ComponentPrefab, talentInfo.Points));
			}
		}
		component.MountComponents(linkedList);
	}
}
