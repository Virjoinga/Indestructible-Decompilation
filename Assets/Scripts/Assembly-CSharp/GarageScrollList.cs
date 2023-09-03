using UnityEngine;

public class GarageScrollList : MonoBehaviour
{
	public Camera UICamera;

	public PanelGarage ParentPanelGarage;

	public GameObject GarageItemSpacePrefab;

	public GameObject GarageItemStandardPrefab;

	private GarageItemStandard _selectedItem;

	private UIScrollList _scrollList;

	public void Select(string id, bool smooth)
	{
		int count = _scrollList.Count;
		for (int i = 0; i < count; i++)
		{
			UIListItemContainer uIListItemContainer = _scrollList.GetItem(i) as UIListItemContainer;
			GarageItemStandard componentInChildren = uIListItemContainer.GetComponentInChildren<GarageItemStandard>();
			if (!(componentInChildren == null) && componentInChildren.Item.Vehicle.id == id)
			{
				Select(componentInChildren, smooth);
				break;
			}
		}
	}

	public void Select(GarageItemStandard item, bool smooth)
	{
		if (_selectedItem == item)
		{
			Dialogs.ShopVehicleDetails(item);
		}
		else
		{
			if (_selectedItem != null)
			{
				_selectedItem.SetSelected(false);
				_selectedItem = null;
			}
			_selectedItem = item;
			_selectedItem.SetSelected(true);
			ParentPanelGarage.ChangeVehicle(item);
			MonoSingleton<Player>.Instance.Save();
		}
		float scrollTime = ((!smooth) ? 0f : 2f);
		_scrollList.ScrollToItem(item.Index, scrollTime);
	}

	private void Awake()
	{
		Vector2 screenSize = UITools.GetScreenSize();
		float num = 0f;
		float num2 = 42.25f;
		_scrollList = GetComponent<UIScrollList>();
		_scrollList.viewableArea.x = screenSize.x + num - num2;
		Transform component = GetComponent<Transform>();
		Vector3 localPosition = component.localPosition;
		localPosition.x -= (num + num2) / 2f;
		component.localPosition = localPosition;
		FeedItems();
	}

	private void AddItem(GarageVehicle item)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(GarageItemStandardPrefab);
		GarageItemStandard componentInChildren = gameObject.GetComponentInChildren<GarageItemStandard>();
		componentInChildren.SetData(this, item, _scrollList.Count);
		_scrollList.AddItem(gameObject);
	}

	private void AddSpace()
	{
		GameObject itemGO = (GameObject)Object.Instantiate(GarageItemSpacePrefab);
		_scrollList.AddItem(itemGO);
	}

	public void FeedItems()
	{
		_scrollList.ClearList(true);
		AddSpace();
		foreach (GarageVehicle boughtVehicle in MonoSingleton<Player>.Instance.BoughtVehicles)
		{
			AddItem(boughtVehicle);
		}
		AddSpace();
	}
}
