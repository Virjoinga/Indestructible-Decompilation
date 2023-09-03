using System;
using System.Collections;
using UnityEngine;

public class DialogSelectVehicle : UIDialog
{
	public GameObject ItemPrefab;

	public bool VehicleBought;

	private string _freeVehicleId = string.Empty;

	public void Buy(DialogSelectVehicleItem item)
	{
		bool flag = false;
		if (item.ZeroPrice || MonoSingleton<Player>.Instance.Buy(item.Item))
		{
			GameAnalytics.EventFirstVehicleSelected(item.Item.id, _freeVehicleId);
			MonoSingleton<Player>.Instance.SelectDefaultVehicle(item.Item.id);
			MonoSingleton<Player>.Instance.Tutorial.SetVehicleChoosen();
			MonoSingleton<Player>.Instance.Save();
			VehicleBought = true;
			PanelGarage.StartPractice();
			StartCoroutine(CloseNextFrame());
		}
	}

	private IEnumerator CloseNextFrame()
	{
		yield return null;
		Close();
	}

	private void CreateItem(string itemId, bool zeroPrice, float offset, int stars)
	{
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ItemPrefab);
		DialogSelectVehicleItem component = gameObject.GetComponent<DialogSelectVehicleItem>();
		ShopItem item = MonoSingleton<ShopController>.Instance.GetItem(itemId);
		component.SetData(this, item, zeroPrice);
		component.SetStartCount(stars);
		Transform component2 = gameObject.GetComponent<Transform>();
		component2.parent = _transform;
		component2.localPosition = new Vector3(offset, 0f, -2f);
	}

	protected override void Awake()
	{
		base.Awake();
		Vector2 screenSize = UITools.GetScreenSize();
		string[] array = new string[2] { "vehicle_punisher", "vehicle_mantis" };
		System.Random random = new System.Random();
		int num = random.Next(0, array.Length);
		_freeVehicleId = array[num];
		CreateItem(_freeVehicleId, true, (0f - screenSize.x) * 0.325f, 1);
		CreateItem("vehicle_striker", false, 0f, 2);
		CreateItem("vehicle_thermodon", false, screenSize.x * 0.325f, 3);
	}

	public override void Activate()
	{
		base.Activate();
		GamePlayHaven.Placement("tutorial_start");
	}
}
