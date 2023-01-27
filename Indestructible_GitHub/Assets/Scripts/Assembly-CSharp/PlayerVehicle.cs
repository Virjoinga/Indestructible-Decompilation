using System.Collections.Generic;
using UnityEngine;

public class PlayerVehicle : Vehicle
{
	protected override void Awake()
	{
		base.Awake();
		MountPlayerParts();
	}

	protected override void Start()
	{
		base.Start();
		if (base.isMine)
		{
			CamShaker instance = CamShaker.Instance;
			if (instance != null)
			{
				instance.epicenterReference = base.transform;
			}
		}
	}

	//[RPC]
	protected override void RemoteMountParts(string bodyName, string armorName, string weaponName)
	{
		base.RemoteMountParts(bodyName, armorName, weaponName);
	}

	//[RPC]
	protected override void RemoteMountComponents(string components)
	{
		base.RemoteMountComponents(components);
	}

	private void MountPlayerParts()
	{
		if (!base.isMine)
		{
			return;
		}
		GarageVehicle selectedVehicle = MonoSingleton<Player>.Instance.SelectedVehicle;
		string bodyName = null;
		string armorName = null;
		string text = null;
		LinkedList<ComponentsMountInfo> linkedList = new LinkedList<ComponentsMountInfo>();
		if (selectedVehicle.Components != null && selectedVehicle.Components.Length > 0)
		{
			for (int i = 0; i < selectedVehicle.Components.Length; i++)
			{
				if (selectedVehicle.Components[i] != null)
				{
					linkedList.AddLast(new ComponentsMountInfo(selectedVehicle.Components[i].ComponentPrefab));
				}
			}
		}
		foreach (PlayerTalent boughtTalent in MonoSingleton<Player>.Instance.BoughtTalents)
		{
			linkedList.AddLast(new ComponentsMountInfo(boughtTalent.Item.ComponentPrefab, boughtTalent.Level));
		}
		if (selectedVehicle.Body != null)
		{
			bodyName = selectedVehicle.Body.prefab;
			linkedList.AddLast(new ComponentsMountInfo(selectedVehicle.Body.ComponentPrefab));
		}
		if (selectedVehicle.Armor != null)
		{
			armorName = selectedVehicle.Armor.prefab;
			linkedList.AddLast(new ComponentsMountInfo(selectedVehicle.Armor.ComponentPrefab));
		}
		if (selectedVehicle.Weapon != null)
		{
			if (selectedVehicle.Ammunition != null)
			{
				text = selectedVehicle.Ammunition.ConfigureWeapon(selectedVehicle.Weapon);
				if (Resources.Load(text) as GameObject == null)
				{
					text = selectedVehicle.Weapon.prefab;
				}
			}
			else
			{
				text = selectedVehicle.Weapon.prefab;
			}
			linkedList.AddLast(new ComponentsMountInfo(selectedVehicle.Weapon.ComponentPrefab));
		}
		MountParts(bodyName, armorName, text);
		MountComponents(linkedList);
	}
}
