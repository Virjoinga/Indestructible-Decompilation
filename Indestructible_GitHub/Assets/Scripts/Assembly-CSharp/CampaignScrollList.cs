using UnityEngine;

public class CampaignScrollList : MonoBehaviour
{
	public PanelShop ParentPanelShop;

	public GameObject BossListItemPrefab;

	private UIPageScrollList _scrollList;

	private int _curBossIdx = -1;

	private void Awake()
	{
		Vector2 screenSize = UITools.GetScreenSize();
		_scrollList = GetComponent<UIPageScrollList>();
		_scrollList.viewableArea.x = screenSize.x;
	}

	public void Clear()
	{
		_scrollList.ScrollListTo(0f);
		_scrollList.FreeItems();
	}

	private void EventFillData(IUIObject container, int index)
	{
		if (index == _curBossIdx)
		{
			container.gameObject.transform.localScale = new Vector3(1.08f, 1.08f, 1f);
		}
		else
		{
			container.gameObject.transform.localScale = Vector3.one;
		}
		BossListItem component = container.gameObject.GetComponent<BossListItem>();
		BossFightConfig[] bossFights = BossFightConfiguration.Instance.BossFights;
		component.SetData(this, bossFights[index], index + 1, _curBossIdx == index, index < _curBossIdx);
	}

	private void EventFreeData(IUIObject container, int index)
	{
		BossListItem component = container.gameObject.GetComponent<BossListItem>();
		component.ReleaseData();
	}

	public void FeedBossFights(int lastBossIdx)
	{
		Clear();
		_curBossIdx = lastBossIdx + 1;
		BossFightConfig[] bossFights = BossFightConfiguration.Instance.BossFights;
		_scrollList.EventFillData = EventFillData;
		_scrollList.EventFreeData = EventFreeData;
		_scrollList.Init(bossFights.Length);
		if (_curBossIdx != -1)
		{
			_scrollList.ScrollToItemIndex(Mathf.Max(0, _curBossIdx - 1), 1.5f);
		}
	}
}
