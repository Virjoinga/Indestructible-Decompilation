using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("EZ GUI/Controls/Page Scroll List")]
public class UIPageScrollList : MonoBehaviour, IEZDragDrop, IUIObject
{
	public enum ORIENTATION
	{
		HORIZONTAL = 0,
		VERTICAL = 1
	}

	public enum DIRECTION
	{
		TtoB_LtoR = 0,
		BtoT_RtoL = 1
	}

	public enum ALIGNMENT
	{
		LEFT_TOP = 0,
		CENTER = 1,
		RIGHT_BOTTOM = 2
	}

	protected delegate float ItemAlignmentDel(IUIListObject item);

	protected delegate bool SnapCoordProc(float val);

	public delegate void ItemSnappedDelegate(IUIListObject item);

	protected const float reboundSpeed = 1f;

	protected const float overscrollAllowance = 0.5f;

	protected const float lowPassKernelWidthInSeconds = 0.045f;

	protected const float backgroundColliderOffset = 0.01f;

	private const float scrollStopThreshold = 0.0001f;

	public bool touchScroll = true;

	public float scrollWheelFactor = 100f;

	public float scrollDecelCoef = 0.04f;

	public bool snap;

	public float minSnapDuration = 1f;

	public EZAnimation.EASING_TYPE snapEasing = EZAnimation.EASING_TYPE.ExponentialOut;

	public UISlider slider;

	public ORIENTATION orientation;

	public DIRECTION direction;

	public ALIGNMENT alignment = ALIGNMENT.CENTER;

	public Vector2 viewableArea;

	protected Vector2 viewableAreaActual;

	public bool unitsInPixels;

	public Camera renderCamera;

	protected Rect3D clientClippingRect;

	public float itemSpacing;

	protected float itemSpacingActual;

	public bool spacingAtEnds = true;

	public float extraEndSpacing;

	protected float extraEndSpacingActual;

	public bool activateWhenAdding = true;

	public bool clipContents = true;

	public bool clipWhenMoving;

	public bool positionItemsImmediately = true;

	public float dragThreshold = float.NaN;

	public GameObject[] sceneItems = new GameObject[0];

	public PrefabListItem[] prefabItems = new PrefabListItem[0];

	public MonoBehaviour scriptWithMethodToInvoke;

	public string methodToInvokeOnSelect;

	public SpriteManager manager;

	public bool detargetOnDisable;

	public EZAnimation.EASING_TYPE positionEasing = EZAnimation.EASING_TYPE.ExponentialOut;

	public float positionEaseDuration = 0.5f;

	public float positionEaseDelay;

	public bool blockInputWhileEasing = true;

	protected bool doItemEasing;

	protected bool doPosEasing;

	protected List<EZAnimation> itemEasers = new List<EZAnimation>();

	protected EZAnimation scrollPosAnim;

	[NonSerialized]
	[HideInInspector]
	public bool repositionOnEnable = true;

	protected float contentExtents;

	protected IUIListObject selectedItem;

	protected IUIObject lastClickedControl;

	protected float scrollPos;

	protected GameObject _mover;

	protected Transform _moverTransform;

	protected Transform _transform;

	protected List<IUIListObject> items = new List<IUIListObject>();

	protected bool m_controlIsEnabled = true;

	protected IUIContainer container;

	protected EZInputDelegate inputDelegate;

	protected EZValueChangedDelegate changeDelegate;

	protected ItemSnappedDelegate itemSnappedDel;

	protected Vector3 cachedPos;

	protected Quaternion cachedRot;

	protected Vector3 cachedScale;

	protected bool m_started;

	protected bool m_awake;

	protected List<IUIListObject> newItems = new List<IUIListObject>();

	protected bool itemsInserted;

	protected bool isScrolling;

	protected bool noTouch = true;

	protected float lowPassFilterFactor;

	protected float scrollInertia;

	protected float scrollMax;

	protected float scrollDelta;

	private float scrollStopThresholdLog = Mathf.Log10(0.0001f);

	protected float inertiaLerpInterval = 0.06f;

	protected float inertiaLerpTime;

	protected float amtOfPlay;

	protected float autoScrollDuration;

	protected float autoScrollStart;

	protected float autoScrollPos;

	protected float autoScrollDelta;

	protected float autoScrollTime;

	protected bool autoScrolling;

	private bool listMoved;

	protected EZAnimation.Interpolator autoScrollInterpolator;

	private IUIListObject snappedItem;

	private float localUnitsPerPixel;

	protected EZDragDropDelegate dragDropDelegate;

	public GameObject _itemPrefab;

	public float _visibleAreaEventOffset = 0.1f;

	public Action<IUIListObject, int> EventFillData;

	public Action<IUIListObject, int> EventFreeData;

	private int _totalItens = 10;

	private int _currentIndex;

	private int _itensAmountToFill = 3;

	private float _itemSize;

	private float _outArea;

	private int _indexToScroll;

	private float _scrollTime;

	private bool _scrollToItem;

	private bool _canScroll = true;

	private GameObject[] _cachedObjects;

	public float ScrollPosition
	{
		get
		{
			return scrollPos;
		}
		set
		{
			ScrollListTo(value);
		}
	}

	public IUIListObject SnappedItem
	{
		get
		{
			return snappedItem;
		}
	}

	public float ContentExtents
	{
		get
		{
			return contentExtents;
		}
	}

	public float UnviewableArea
	{
		get
		{
			return amtOfPlay;
		}
	}

	public IUIListObject SelectedItem
	{
		get
		{
			return selectedItem;
		}
		set
		{
			IUIListObject iUIListObject = selectedItem;
			if (selectedItem != null)
			{
				selectedItem.selected = false;
			}
			if (value == null)
			{
				selectedItem = null;
				return;
			}
			selectedItem = value;
			selectedItem.selected = true;
			if (iUIListObject != selectedItem && changeDelegate != null)
			{
				changeDelegate(this);
			}
		}
	}

	public IUIObject LastClickedControl
	{
		get
		{
			return lastClickedControl;
		}
	}

	public int Count
	{
		get
		{
			return items.Count;
		}
	}

	public bool controlIsEnabled
	{
		get
		{
			return m_controlIsEnabled;
		}
		set
		{
			m_controlIsEnabled = value;
			for (int i = 0; i < items.Count; i++)
			{
				items[i].controlIsEnabled = value;
			}
		}
	}

	public virtual bool DetargetOnDisable
	{
		get
		{
			return DetargetOnDisable;
		}
		set
		{
			DetargetOnDisable = value;
		}
	}

	public virtual IUIContainer Container
	{
		get
		{
			return container;
		}
		set
		{
			if (value != container)
			{
				if (container != null)
				{
					RemoveItemsFromContainer();
				}
				container = value;
				AddItemsToContainer();
			}
			else
			{
				container = value;
			}
		}
	}

	public object Data
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool IsDraggable
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public LayerMask DropMask
	{
		get
		{
			return -1;
		}
		set
		{
		}
	}

	public float DragOffset
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public EZAnimation.EASING_TYPE CancelDragEasing
	{
		get
		{
			return EZAnimation.EASING_TYPE.Default;
		}
		set
		{
		}
	}

	public float CancelDragDuration
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public bool IsDragging
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public GameObject DropTarget
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool DropHandled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool CanScroll
	{
		get
		{
			return _canScroll;
		}
		set
		{
			_canScroll = value;
		}
	}

	public bool AutoScrolling
	{
		get
		{
			return autoScrolling;
		}
	}

	protected void Awake()
	{
		if (!m_awake)
		{
			m_awake = true;
			_transform = GetComponent<Transform>();
			_mover = new GameObject();
			_mover.name = "Mover";
			_moverTransform = _mover.GetComponent<Transform>();
			_moverTransform.parent = _transform;
			_moverTransform.localPosition = Vector3.zero;
			_moverTransform.localRotation = Quaternion.identity;
			_moverTransform.localScale = Vector3.one;
			if (direction == DIRECTION.BtoT_RtoL)
			{
				scrollPos = 1f;
			}
			autoScrollInterpolator = EZAnimation.GetInterpolator(snapEasing);
			lowPassFilterFactor = inertiaLerpInterval / 0.045f;
		}
	}

	protected void Start()
	{
		if (m_started)
		{
			return;
		}
		m_started = true;
		SetupCameraAndSizes();
		cachedPos = _transform.position;
		cachedRot = _transform.rotation;
		cachedScale = _transform.lossyScale;
		CalcClippingRect();
		if (slider != null)
		{
			slider.AddValueChangedDelegate(SliderMoved);
			slider.AddInputDelegate(SliderInputDel);
		}
		if (base.GetComponent<Collider>() == null && touchScroll)
		{
			BoxCollider boxCollider = (BoxCollider)base.gameObject.AddComponent(typeof(BoxCollider));
			boxCollider.size = new Vector3(viewableAreaActual.x, viewableAreaActual.y, 0.001f);
			boxCollider.center = Vector3.forward * 0.01f;
			boxCollider.isTrigger = true;
		}
		for (int i = 0; i < sceneItems.Length; i++)
		{
			if (sceneItems[i] != null)
			{
				AddItem(sceneItems[i]);
			}
		}
		for (int j = 0; j < prefabItems.Length; j++)
		{
			if (prefabItems[j] == null)
			{
				continue;
			}
			if (prefabItems[j].item == null)
			{
				if (prefabItems[0].item != null)
				{
					CreateItem(prefabItems[0].item, (!(prefabItems[j].itemText == string.Empty)) ? prefabItems[j].itemText : null);
				}
			}
			else
			{
				CreateItem(prefabItems[j].item, (!(prefabItems[j].itemText == string.Empty)) ? prefabItems[j].itemText : null);
			}
		}
		if (float.IsNaN(dragThreshold))
		{
			dragThreshold = UIManager.instance.dragThreshold;
		}
	}

	public void UpdateCamera()
	{
		SetupCameraAndSizes();
		CalcClippingRect();
		RepositionItems();
	}

	public void SetupCameraAndSizes()
	{
		if (renderCamera == null)
		{
			if (UIManager.Exists() && UIManager.instance.uiCameras[0].camera != null)
			{
				renderCamera = UIManager.instance.uiCameras[0].camera;
			}
			else
			{
				renderCamera = Camera.main;
			}
		}
		if (unitsInPixels)
		{
			CalcScreenToWorldUnits();
			viewableAreaActual = new Vector2(viewableArea.x * localUnitsPerPixel, viewableArea.y * localUnitsPerPixel);
			itemSpacingActual = itemSpacing * localUnitsPerPixel;
			extraEndSpacingActual = extraEndSpacing * localUnitsPerPixel;
		}
		else
		{
			viewableAreaActual = viewableArea;
			itemSpacingActual = itemSpacing;
			extraEndSpacingActual = extraEndSpacing;
		}
		for (int i = 0; i < items.Count; i++)
		{
			items[i].UpdateCamera();
		}
	}

	protected void CalcScreenToWorldUnits()
	{
		float distanceToPoint = new Plane(renderCamera.transform.forward, renderCamera.transform.position).GetDistanceToPoint(_transform.position);
		localUnitsPerPixel = Vector3.Distance(renderCamera.ScreenToWorldPoint(new Vector3(0f, 1f, distanceToPoint)), renderCamera.ScreenToWorldPoint(new Vector3(0f, 0f, distanceToPoint)));
	}

	protected void CalcClippingRect()
	{
		clientClippingRect.FromPoints(new Vector3((0f - viewableAreaActual.x) * 0.5f, viewableAreaActual.y * 0.5f, 0f), new Vector3(viewableAreaActual.x * 0.5f, viewableAreaActual.y * 0.5f, 0f), new Vector3((0f - viewableAreaActual.x) * 0.5f, (0f - viewableAreaActual.y) * 0.5f, 0f));
		clientClippingRect.MultFast(_transform.localToWorldMatrix);
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].TextObj != null)
			{
				items[i].TextObj.ClippingRect = clientClippingRect;
			}
		}
	}

	public void SliderMoved(IUIObject slider)
	{
		ScrollListTo_Internal(((UISlider)slider).Value);
	}

	public void SliderInputDel(ref POINTER_INFO ptr)
	{
		if (snap && (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP || ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE || ptr.evt == POINTER_INFO.INPUT_EVENT.RELEASE_OFF))
		{
			CalcSnapItem();
		}
	}

	public void ScrollListTo(float pos)
	{
		scrollInertia = 0f;
		scrollDelta = 0f;
		isScrolling = false;
		autoScrolling = false;
		ScrollListTo_Internal(pos);
	}

	public void ScrollToItem(IUIListObject item, float scrollTime, EZAnimation.EASING_TYPE easing)
	{
		snappedItem = item;
		if (newItems.Count != 0)
		{
			if (itemsInserted || doItemEasing)
			{
				RepositionItems();
			}
			else
			{
				PositionNewItems();
			}
			itemsInserted = false;
			newItems.Clear();
		}
		if (orientation == ORIENTATION.HORIZONTAL)
		{
			if (direction == DIRECTION.TtoB_LtoR)
			{
				autoScrollPos = Mathf.Clamp01(item.transform.localPosition.x / amtOfPlay);
			}
			else
			{
				autoScrollPos = Mathf.Clamp01((0f - item.transform.localPosition.x) / amtOfPlay);
			}
		}
		else if (direction == DIRECTION.TtoB_LtoR)
		{
			autoScrollPos = Mathf.Clamp01((0f - item.transform.localPosition.y) / amtOfPlay);
		}
		else
		{
			autoScrollPos = Mathf.Clamp01(item.transform.localPosition.y / amtOfPlay);
		}
		autoScrollInterpolator = EZAnimation.GetInterpolator(easing);
		autoScrollStart = scrollPos;
		autoScrollDelta = autoScrollPos - scrollPos;
		autoScrollDuration = scrollTime;
		autoScrollTime = 0f;
		autoScrolling = true;
		scrollDelta = 0f;
		isScrolling = false;
		if (itemSnappedDel != null)
		{
			itemSnappedDel(snappedItem);
		}
	}

	public void ScrollToItem(int index, float scrollTime, EZAnimation.EASING_TYPE easing)
	{
		if (index >= 0 && index < items.Count)
		{
			ScrollToItem(items[index], scrollTime, easing);
		}
	}

	public void ScrollToItem(IUIListObject item, float scrollTime)
	{
		ScrollToItem(item, scrollTime, snapEasing);
	}

	public void ScrollToItem(int index, float scrollTime)
	{
		ScrollToItem(index, scrollTime, snapEasing);
	}

	public void SetViewableAreaPixelDimensions(Camera cam, int width, int height)
	{
		float distanceToPoint = new Plane(cam.transform.forward, cam.transform.position).GetDistanceToPoint(_transform.position);
		float num = Vector3.Distance(cam.ScreenToWorldPoint(new Vector3(0f, 1f, distanceToPoint)), cam.ScreenToWorldPoint(new Vector3(0f, 0f, distanceToPoint)));
		viewableAreaActual = new Vector2((float)width * num, (float)height * num);
		CalcClippingRect();
		RepositionItems();
	}

	public void InsertItem(IUIListObject item, int position)
	{
		InsertItem(item, position, null, false);
	}

	public void InsertItem(IUIListObject item, int position, bool doEasing)
	{
		InsertItem(item, position, null, doEasing);
	}

	public void InsertItem(IUIListObject item, int position, string text)
	{
		InsertItem(item, position, text, false);
	}

	protected void PositionNewItems()
	{
		IUIListObject iUIListObject = null;
		float num = 0f;
		for (int i = 0; i < newItems.Count; i++)
		{
			if (newItems[i] == null)
			{
				continue;
			}
			int index = newItems[i].Index;
			IUIListObject iUIListObject2 = items[index];
			iUIListObject2.FindOuterEdges();
			iUIListObject2.UpdateCollider();
			float x = 0f;
			float y = 0f;
			bool flag = false;
			if (orientation == ORIENTATION.HORIZONTAL)
			{
				if (index > 0)
				{
					flag = true;
					iUIListObject = items[index - 1];
					x = ((direction != 0) ? (iUIListObject.transform.localPosition.x - iUIListObject.BottomRightEdge.x - itemSpacingActual + iUIListObject2.TopLeftEdge.x) : (iUIListObject.transform.localPosition.x + iUIListObject.BottomRightEdge.x + itemSpacingActual - iUIListObject2.TopLeftEdge.x));
				}
				else
				{
					if (spacingAtEnds)
					{
						flag = true;
					}
					x = ((direction != 0) ? (viewableAreaActual.x * 0.5f - iUIListObject2.BottomRightEdge.x - ((!spacingAtEnds) ? 0f : itemSpacingActual) - extraEndSpacingActual) : (viewableAreaActual.x * -0.5f - iUIListObject2.TopLeftEdge.x + ((!spacingAtEnds) ? 0f : itemSpacingActual) + extraEndSpacingActual));
				}
				switch (alignment)
				{
				case ALIGNMENT.CENTER:
					y = 0f;
					break;
				case ALIGNMENT.LEFT_TOP:
					y = viewableAreaActual.y * 0.5f - iUIListObject2.TopLeftEdge.y;
					break;
				case ALIGNMENT.RIGHT_BOTTOM:
					y = viewableAreaActual.y * -0.5f - iUIListObject2.BottomRightEdge.y;
					break;
				}
				num += iUIListObject2.BottomRightEdge.x - iUIListObject2.TopLeftEdge.x + ((!flag || iUIListObject == null) ? 0f : itemSpacingActual);
			}
			else
			{
				if (index > 0)
				{
					flag = true;
					iUIListObject = items[index - 1];
					y = ((direction != 0) ? (iUIListObject.transform.localPosition.y - iUIListObject.BottomRightEdge.y + itemSpacingActual + iUIListObject2.TopLeftEdge.y) : (iUIListObject.transform.localPosition.y + iUIListObject.BottomRightEdge.y - itemSpacingActual - iUIListObject2.TopLeftEdge.y));
				}
				else
				{
					if (spacingAtEnds)
					{
						flag = true;
					}
					y = ((direction != 0) ? (viewableAreaActual.y * -0.5f - iUIListObject2.BottomRightEdge.y + ((!spacingAtEnds) ? 0f : itemSpacingActual) + extraEndSpacingActual) : (viewableAreaActual.y * 0.5f - iUIListObject2.TopLeftEdge.y - ((!spacingAtEnds) ? 0f : itemSpacingActual) - extraEndSpacingActual));
				}
				switch (alignment)
				{
				case ALIGNMENT.CENTER:
					x = 0f;
					break;
				case ALIGNMENT.LEFT_TOP:
					x = viewableAreaActual.x * -0.5f - iUIListObject2.TopLeftEdge.x;
					break;
				case ALIGNMENT.RIGHT_BOTTOM:
					x = viewableAreaActual.x * 0.5f - iUIListObject2.BottomRightEdge.x;
					break;
				}
				num += iUIListObject2.TopLeftEdge.y - iUIListObject2.BottomRightEdge.y + ((!flag || iUIListObject == null) ? 0f : itemSpacingActual);
			}
			iUIListObject2.transform.localPosition = new Vector3(x, y, 0f);
		}
		UpdateContentExtents(num);
		ClipItems();
		newItems.Clear();
	}

	public void AddItem(GameObject itemGO)
	{
		IUIListObject iUIListObject = (IUIListObject)itemGO.GetComponent(typeof(IUIListObject));
		if (iUIListObject == null)
		{
			Debug.LogWarning("GameObject \"" + itemGO.name + "\" does not contain any list item component suitable to be added to scroll list \"" + base.name + "\".");
		}
		else
		{
			AddItem(iUIListObject, null);
		}
	}

	public void AddItem(IUIListObject item)
	{
		AddItem(item, null);
	}

	public void AddItem(IUIListObject item, string text)
	{
		if (!m_awake)
		{
			Awake();
		}
		if (!m_started)
		{
			Start();
		}
		InsertItem(item, items.Count, text, false);
	}

	public IUIListObject CreateItem(GameObject prefab)
	{
		if (!m_awake)
		{
			Awake();
		}
		if (!m_started)
		{
			Start();
		}
		return CreateItem(prefab, items.Count, null);
	}

	public IUIListObject CreateItem(GameObject prefab, string text)
	{
		if (!m_awake)
		{
			Awake();
		}
		if (!m_started)
		{
			Start();
		}
		return CreateItem(prefab, items.Count, text);
	}

	public IUIListObject CreateItem(GameObject prefab, int position, bool doEasing)
	{
		return CreateItem(prefab, position, null, doEasing);
	}

	public IUIListObject CreateItem(GameObject prefab, int position)
	{
		return CreateItem(prefab, position, null, false);
	}

	public IUIListObject CreateItem(GameObject prefab, int position, string text)
	{
		return CreateItem(prefab, position, text, false);
	}

	public IUIListObject CreateItem(GameObject prefab, int position, string text, bool doEasing)
	{
		IUIListObject iUIListObject = (IUIListObject)prefab.GetComponent(typeof(IUIListObject));
		if (iUIListObject == null)
		{
			return null;
		}
		iUIListObject.RenderCamera = renderCamera;
		GameObject gameObject;
		if (manager != null)
		{
			if (iUIListObject.IsContainer())
			{
				gameObject = (GameObject)UnityEngine.Object.Instantiate(prefab);
				Component[] componentsInChildren = gameObject.GetComponentsInChildren(typeof(SpriteRoot));
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					manager.AddSprite((SpriteRoot)componentsInChildren[i]);
				}
			}
			else
			{
				SpriteRoot spriteRoot = manager.CreateSprite(prefab);
				if (spriteRoot == null)
				{
					return null;
				}
				gameObject = spriteRoot.gameObject;
			}
		}
		else
		{
			gameObject = (GameObject)UnityEngine.Object.Instantiate(prefab);
		}
		iUIListObject = (IUIListObject)gameObject.GetComponent(typeof(IUIListObject));
		if (iUIListObject == null)
		{
			return null;
		}
		InsertItem(iUIListObject, position, text, doEasing);
		return iUIListObject;
	}

	protected void UpdateContentExtents(float change)
	{
		float num = amtOfPlay;
		float num2 = ((!spacingAtEnds) ? 0f : (itemSpacingActual * 2f)) + extraEndSpacingActual * 2f;
		contentExtents += change;
		if (orientation == ORIENTATION.HORIZONTAL)
		{
			amtOfPlay = contentExtents + num2 - viewableAreaActual.x;
			scrollMax = viewableAreaActual.x / (contentExtents + num2 - viewableAreaActual.x) * 0.5f;
		}
		else
		{
			amtOfPlay = contentExtents + num2 - viewableAreaActual.y;
			scrollMax = viewableAreaActual.y / (contentExtents + num2 - viewableAreaActual.y) * 0.5f;
		}
		float num3 = num * scrollPos / amtOfPlay;
		if (doPosEasing && num3 > 1f)
		{
			scrollPosAnim = AnimatePosition.Do(base.gameObject, EZAnimation.ANIM_MODE.By, Vector3.zero, ScrollPosInterpolator, positionEaseDuration, positionEaseDelay, null, OnPosEasingDone);
			scrollPosAnim.Data = new Vector2(num3, 1f - num3);
			itemEasers.Add(scrollPosAnim);
		}
		else
		{
			ScrollListTo_Internal(Mathf.Clamp01(num3));
		}
		doPosEasing = false;
	}

	protected float ScrollPosInterpolator(float time, float start, float delta, float duration)
	{
		Vector2 vector = (Vector2)scrollPosAnim.Data;
		ScrollListTo_Internal(EZAnimation.GetInterpolator(positionEasing)(time, vector.x, vector.y, duration));
		if (time >= duration)
		{
			scrollPosAnim = null;
		}
		return start;
	}

	protected float GetYCentered(IUIListObject item)
	{
		return 0f;
	}

	protected float GetYAlignTop(IUIListObject item)
	{
		return viewableAreaActual.y * 0.5f - item.TopLeftEdge.y;
	}

	protected float GetYAlignBottom(IUIListObject item)
	{
		return viewableAreaActual.y * -0.5f - item.BottomRightEdge.y;
	}

	protected float GetXCentered(IUIListObject item)
	{
		return 0f;
	}

	protected float GetXAlignLeft(IUIListObject item)
	{
		return viewableAreaActual.x * -0.5f - item.TopLeftEdge.x;
	}

	protected float GetXAlignRight(IUIListObject item)
	{
		return viewableAreaActual.x * 0.5f - item.BottomRightEdge.x;
	}

	public void PositionItems()
	{
		if (itemEasers.Count > 0)
		{
			for (int i = 0; i < itemEasers.Count; i++)
			{
				itemEasers[i].CompletedDelegate = null;
				itemEasers[i].End();
			}
			itemEasers.Clear();
			if (blockInputWhileEasing)
			{
				UIManager.instance.UnlockInput();
			}
		}
		if (orientation == ORIENTATION.HORIZONTAL)
		{
			PositionHorizontally(false);
		}
		else
		{
			PositionVertically(false);
		}
		UpdateContentExtents(0f);
		ClipItems();
		if (itemEasers.Count > 0 && blockInputWhileEasing)
		{
			UIManager.instance.LockInput();
		}
		doItemEasing = false;
	}

	public void RepositionItems()
	{
		if (itemEasers.Count > 0)
		{
			for (int i = 0; i < itemEasers.Count; i++)
			{
				itemEasers[i].CompletedDelegate = null;
				itemEasers[i].End();
			}
			itemEasers.Clear();
			if (blockInputWhileEasing)
			{
				UIManager.instance.UnlockInput();
			}
		}
		if (orientation == ORIENTATION.HORIZONTAL)
		{
			PositionHorizontally(true);
		}
		else
		{
			PositionVertically(true);
		}
		UpdateContentExtents(0f);
		ClipItems();
		if (itemEasers.Count > 0 && blockInputWhileEasing)
		{
			UIManager.instance.LockInput();
		}
		doItemEasing = false;
	}

	protected void PositionHorizontally(bool updateExtents)
	{
		contentExtents = 0f;
		ItemAlignmentDel itemAlignmentDel;
		switch (alignment)
		{
		case ALIGNMENT.CENTER:
			itemAlignmentDel = GetYCentered;
			break;
		case ALIGNMENT.LEFT_TOP:
			itemAlignmentDel = GetYAlignTop;
			break;
		case ALIGNMENT.RIGHT_BOTTOM:
			itemAlignmentDel = GetYAlignBottom;
			break;
		default:
			itemAlignmentDel = GetYCentered;
			break;
		}
		Vector3 vector;
		float num;
		if (direction == DIRECTION.TtoB_LtoR)
		{
			num = viewableAreaActual.x * -0.5f + ((!spacingAtEnds) ? 0f : itemSpacingActual) + extraEndSpacingActual;
			for (int i = 0; i < items.Count; i++)
			{
				if (updateExtents)
				{
					items[i].FindOuterEdges();
					items[i].UpdateCollider();
				}
				vector = new Vector3(num - items[i].TopLeftEdge.x, itemAlignmentDel(items[i]), 0f);
				if (doItemEasing)
				{
					if (newItems.Contains(items[i]))
					{
						items[i].transform.localPosition = vector;
					}
					else
					{
						itemEasers.Add(AnimatePosition.Do(items[i].gameObject, EZAnimation.ANIM_MODE.To, vector, EZAnimation.GetInterpolator(positionEasing), positionEaseDuration, positionEaseDelay, null, OnPosEasingDone));
					}
				}
				else
				{
					items[i].transform.localPosition = vector;
				}
				float num2 = items[i].BottomRightEdge.x - items[i].TopLeftEdge.x + itemSpacingActual;
				contentExtents += num2;
				num += num2;
				items[i].Index = i;
			}
			if (!spacingAtEnds)
			{
				contentExtents -= itemSpacingActual;
			}
			return;
		}
		num = viewableAreaActual.x * 0.5f - ((!spacingAtEnds) ? 0f : itemSpacingActual) - extraEndSpacingActual;
		for (int j = 0; j < items.Count; j++)
		{
			if (updateExtents)
			{
				items[j].FindOuterEdges();
				items[j].UpdateCollider();
			}
			vector = new Vector3(num - items[j].BottomRightEdge.x, itemAlignmentDel(items[j]), 0f);
			if (doItemEasing)
			{
				if (newItems.Contains(items[j]))
				{
					items[j].transform.localPosition = vector;
				}
				else
				{
					itemEasers.Add(AnimatePosition.Do(items[j].gameObject, EZAnimation.ANIM_MODE.To, vector, EZAnimation.GetInterpolator(positionEasing), positionEaseDuration, positionEaseDelay, null, OnPosEasingDone));
				}
			}
			else
			{
				items[j].transform.localPosition = vector;
			}
			float num2 = items[j].BottomRightEdge.x - items[j].TopLeftEdge.x + itemSpacingActual;
			contentExtents += num2;
			num -= num2;
			items[j].Index = j;
		}
		if (!spacingAtEnds)
		{
			contentExtents -= itemSpacingActual;
		}
	}

	protected void PositionVertically(bool updateExtents)
	{
		contentExtents = 0f;
		ItemAlignmentDel itemAlignmentDel;
		switch (alignment)
		{
		case ALIGNMENT.CENTER:
			itemAlignmentDel = GetXCentered;
			break;
		case ALIGNMENT.LEFT_TOP:
			itemAlignmentDel = GetXAlignLeft;
			break;
		case ALIGNMENT.RIGHT_BOTTOM:
			itemAlignmentDel = GetXAlignRight;
			break;
		default:
			itemAlignmentDel = GetXCentered;
			break;
		}
		Vector3 vector;
		float num;
		if (direction == DIRECTION.TtoB_LtoR)
		{
			num = viewableAreaActual.y * 0.5f - ((!spacingAtEnds) ? 0f : itemSpacingActual) - extraEndSpacingActual;
			for (int i = 0; i < items.Count; i++)
			{
				if (updateExtents)
				{
					items[i].FindOuterEdges();
					items[i].UpdateCollider();
				}
				vector = new Vector3(itemAlignmentDel(items[i]), num - items[i].TopLeftEdge.y, 0f);
				if (doItemEasing)
				{
					if (newItems.Contains(items[i]))
					{
						items[i].transform.localPosition = vector;
					}
					else
					{
						itemEasers.Add(AnimatePosition.Do(items[i].gameObject, EZAnimation.ANIM_MODE.To, vector, EZAnimation.GetInterpolator(positionEasing), positionEaseDuration, positionEaseDelay, null, OnPosEasingDone));
					}
				}
				else
				{
					items[i].transform.localPosition = vector;
				}
				float num2 = items[i].TopLeftEdge.y - items[i].BottomRightEdge.y + itemSpacingActual;
				contentExtents += num2;
				num -= num2;
				items[i].Index = i;
			}
			if (!spacingAtEnds)
			{
				contentExtents -= itemSpacingActual;
			}
			return;
		}
		num = viewableAreaActual.y * -0.5f + ((!spacingAtEnds) ? 0f : itemSpacingActual) + extraEndSpacingActual;
		for (int j = 0; j < items.Count; j++)
		{
			if (updateExtents)
			{
				items[j].FindOuterEdges();
				items[j].UpdateCollider();
			}
			vector = new Vector3(itemAlignmentDel(items[j]), num - items[j].BottomRightEdge.y, 0f);
			if (doItemEasing)
			{
				if (newItems.Contains(items[j]))
				{
					items[j].transform.localPosition = vector;
				}
				else
				{
					itemEasers.Add(AnimatePosition.Do(items[j].gameObject, EZAnimation.ANIM_MODE.To, vector, EZAnimation.GetInterpolator(positionEasing), positionEaseDuration, positionEaseDelay, null, OnPosEasingDone));
				}
			}
			else
			{
				items[j].transform.localPosition = vector;
			}
			float num2 = items[j].TopLeftEdge.y - items[j].BottomRightEdge.y + itemSpacingActual;
			contentExtents += num2;
			num += num2;
			items[j].Index = j;
		}
		if (!spacingAtEnds)
		{
			contentExtents -= itemSpacingActual;
		}
	}

	protected void OnPosEasingDone(EZAnimation anim)
	{
		itemEasers.Remove(anim);
		if (itemEasers.Count == 0 && blockInputWhileEasing)
		{
			UIManager.instance.UnlockInput();
		}
	}

	protected void ClipItems()
	{
	}

	public void DidSelect(IUIListObject item)
	{
		if (selectedItem != null)
		{
			selectedItem.selected = false;
		}
		selectedItem = item;
		item.selected = true;
		DidClick(item);
	}

	public void DidClick(IUIObject item)
	{
		lastClickedControl = item;
		if (scriptWithMethodToInvoke != null)
		{
			scriptWithMethodToInvoke.Invoke(methodToInvokeOnSelect, 0f);
		}
		if (changeDelegate != null)
		{
			changeDelegate(this);
		}
	}

	public void ListDragged(POINTER_INFO ptr)
	{
		if (!touchScroll || !controlIsEnabled)
		{
			return;
		}
		autoScrolling = false;
		if (Mathf.Approximately(ptr.inputDelta.sqrMagnitude, 0f))
		{
			scrollDelta = 0f;
			return;
		}
		listMoved = true;
		Plane plane = default(Plane);
		plane.SetNormalAndPosition(_moverTransform.forward * -1f, _moverTransform.position);
		float enter;
		plane.Raycast(ptr.ray, out enter);
		Vector3 position = ptr.ray.origin + ptr.ray.direction * enter;
		plane.Raycast(ptr.prevRay, out enter);
		Vector3 position2 = ptr.prevRay.origin + ptr.prevRay.direction * enter;
		position = _transform.InverseTransformPoint(position);
		position2 = _transform.InverseTransformPoint(position2);
		Vector3 vector = position - position2;
		if (orientation == ORIENTATION.HORIZONTAL)
		{
			scrollDelta = (0f - vector.x) / amtOfPlay;
		}
		else
		{
			scrollDelta = vector.y / amtOfPlay;
		}
		float num = scrollPos + scrollDelta;
		if (num > 1f)
		{
			scrollDelta *= Mathf.Clamp01(1f - (num - 1f) / scrollMax);
		}
		else if (num < 0f)
		{
			scrollDelta *= Mathf.Clamp01(1f + num / scrollMax);
		}
		if (direction == DIRECTION.BtoT_RtoL)
		{
			scrollDelta *= -1f;
		}
		ScrollListTo_Internal(scrollPos + scrollDelta);
		noTouch = false;
		isScrolling = true;
	}

	public void ScrollWheel(float amt)
	{
		if (direction == DIRECTION.BtoT_RtoL)
		{
			amt *= -1f;
		}
		ScrollListTo(Mathf.Clamp01(scrollPos - amt * scrollWheelFactor / amtOfPlay));
	}

	public void PointerReleased()
	{
		noTouch = true;
		if (scrollInertia != 0f)
		{
			scrollDelta = scrollInertia;
		}
		scrollInertia = 0f;
		if (snap && listMoved)
		{
			CalcSnapItem();
		}
		listMoved = false;
	}

	public void SetSelectedItem(int index)
	{
		IUIListObject iUIListObject = selectedItem;
		if (index < 0 || index >= items.Count)
		{
			if (selectedItem != null)
			{
				selectedItem.selected = false;
			}
			selectedItem = null;
			if (iUIListObject != selectedItem && changeDelegate != null)
			{
				changeDelegate(this);
			}
			return;
		}
		IUIListObject iUIListObject2 = items[index];
		if (selectedItem != null)
		{
			selectedItem.selected = false;
		}
		selectedItem = iUIListObject2;
		iUIListObject2.selected = true;
		if (iUIListObject != selectedItem && changeDelegate != null)
		{
			changeDelegate(this);
		}
	}

	public IUIListObject GetItem(int index)
	{
		if (index < 0 || index >= items.Count)
		{
			return null;
		}
		return items[index];
	}

	public void RemoveItem(int index, bool destroy)
	{
		RemoveItem(index, destroy, false);
	}

	public void RemoveItem(int index, bool destroy, bool doEasing)
	{
		if (index >= 0 && index < items.Count)
		{
			if (index == items.Count - 1)
			{
				doItemEasing = false;
			}
			else
			{
				doItemEasing = doEasing;
			}
			doPosEasing = doEasing;
			if (container != null)
			{
				container.RemoveChild(items[index].gameObject);
			}
			if (selectedItem == items[index])
			{
				selectedItem = null;
				items[index].selected = false;
			}
			if (lastClickedControl != null && (lastClickedControl == items[index] || (lastClickedControl.Container != null && lastClickedControl.Container.Equals(items[index]))))
			{
				lastClickedControl = null;
			}
			if (destroy)
			{
				items[index].Delete();
				UnityEngine.Object.Destroy(items[index].gameObject);
			}
			else
			{
				items[index].transform.parent = null;
				items[index].gameObject.SetActiveRecursively(false);
			}
			items.RemoveAt(index);
			PositionItems();
		}
	}

	public void RemoveItem(IUIListObject item, bool destroy)
	{
		RemoveItem(item, destroy, false);
	}

	public void RemoveItem(IUIListObject item, bool destroy, bool doEasing)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] == item)
			{
				RemoveItem(i, destroy, doEasing);
				break;
			}
		}
	}

	public void ClearList(bool destroy)
	{
		RemoveItemsFromContainer();
		selectedItem = null;
		lastClickedControl = null;
		for (int i = 0; i < items.Count; i++)
		{
			NotifyItem(false, i, items[i]);
			items[i].transform.parent = null;
			if (destroy)
			{
				UnityEngine.Object.Destroy(items[i].gameObject);
			}
			else
			{
				items[i].gameObject.SetActiveRecursively(false);
			}
		}
		_currentIndex = 0;
		contentExtents = 0f;
		items.Clear();
	}

	public void FreeItems()
	{
		for (int i = 0; i < items.Count; i++)
		{
			NotifyItem(false, i, items[i]);
		}
		_currentIndex = 0;
		selectedItem = null;
		lastClickedControl = null;
		contentExtents = 0f;
		items.Clear();
		newItems.Clear();
	}

	public void OnInput(POINTER_INFO ptr)
	{
		if (!m_controlIsEnabled)
		{
			if (Container != null)
			{
				ptr.callerIsControl = true;
				Container.OnInput(ptr);
			}
			return;
		}
		if (Vector3.SqrMagnitude(ptr.origPos - ptr.devicePos) > dragThreshold * dragThreshold)
		{
			ptr.isTap = false;
			if (ptr.evt == POINTER_INFO.INPUT_EVENT.TAP)
			{
				ptr.evt = POINTER_INFO.INPUT_EVENT.RELEASE;
			}
		}
		else
		{
			ptr.isTap = true;
		}
		if (inputDelegate != null)
		{
			inputDelegate(ref ptr);
		}
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.NO_CHANGE:
			if (ptr.active)
			{
				ListDragged(ptr);
			}
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			if (!ptr.isTap)
			{
				ListDragged(ptr);
			}
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			PointerReleased();
			break;
		}
		if (scrollWheelFactor != 0f && ptr.inputDelta.z != 0f && ptr.type != POINTER_INFO.POINTER_TYPE.RAY)
		{
			ScrollWheel(ptr.inputDelta.z);
		}
		if (Container != null)
		{
			ptr.callerIsControl = true;
			Container.OnInput(ptr);
		}
	}

	protected void CalcSnapItem()
	{
		float num = 100000000f;
		IUIListObject iUIListObject = null;
		IUIListObject iUIListObject2 = null;
		int num2 = 1;
		if (items.Count < 1)
		{
			return;
		}
		float num3;
		float scrollTime;
		if (Mathf.Approximately(scrollDelta, 0f))
		{
			scrollTime = minSnapDuration;
			num3 = scrollPos;
		}
		else
		{
			num3 = scrollPos + scrollDelta / scrollDecelCoef;
			float num4 = Mathf.Abs(scrollDelta);
			scrollTime = Time.fixedDeltaTime * (scrollStopThresholdLog - Mathf.Log10(num4)) / Mathf.Log10((num4 - num4 * scrollDecelCoef) / num4);
			scrollTime = Mathf.Max(scrollTime, minSnapDuration);
		}
		if (num3 >= 1f || num3 <= 0f)
		{
			if (num3 <= 0f)
			{
				ScrollToItem(0, scrollTime);
			}
			else
			{
				ScrollToItem(items.Count - 1, scrollTime);
			}
			return;
		}
		int num5 = (int)Mathf.Clamp((float)(items.Count - 1) * num3, 0f, items.Count - 1);
		if (orientation == ORIENTATION.HORIZONTAL)
		{
			float num6 = ((direction != 0) ? 1f : (-1f));
			iUIListObject = items[num5];
			num = Mathf.Abs(num3 + num6 * iUIListObject.transform.localPosition.x / amtOfPlay);
			if (num5 + num2 < items.Count)
			{
				iUIListObject2 = items[num5 + num2];
				float num7 = Mathf.Abs(num3 + num6 * iUIListObject2.transform.localPosition.x / amtOfPlay);
				if (num7 < num)
				{
					num = num7;
					iUIListObject = iUIListObject2;
					num5 += num2;
				}
				else
				{
					num2 = -1;
				}
			}
			else
			{
				num2 = -1;
			}
			for (int i = num5 + num2; i > -1 && i < items.Count; i += num2)
			{
				float num7 = Mathf.Abs(num3 + num6 * items[i].transform.localPosition.x / amtOfPlay);
				if (num7 < num)
				{
					num = num7;
					iUIListObject = items[i];
					continue;
				}
				break;
			}
			ScrollToItem(iUIListObject, scrollTime);
			return;
		}
		float num8 = ((direction != 0) ? (-1f) : 1f);
		iUIListObject = items[num5];
		num = Mathf.Abs(num3 + num8 * iUIListObject.transform.localPosition.y / amtOfPlay);
		if (num5 + num2 < items.Count)
		{
			iUIListObject2 = items[num5 + num2];
			float num7 = Mathf.Abs(num3 + num8 * iUIListObject2.transform.localPosition.y / amtOfPlay);
			if (num7 < num)
			{
				num = num7;
				iUIListObject = iUIListObject2;
				num5 += num2;
			}
			else
			{
				num2 = -1;
			}
		}
		else
		{
			num2 = -1;
		}
		for (int j = num5 + num2; j > -1 && j < items.Count; j += num2)
		{
			float num7 = Mathf.Abs(num3 + num8 * items[j].transform.localPosition.y / amtOfPlay);
			if (num7 < num)
			{
				num = num7;
				iUIListObject = items[j];
				continue;
			}
			break;
		}
		ScrollToItem(iUIListObject, scrollTime);
	}

	protected void AddItemsToContainer()
	{
		if (container != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				container.AddChild(items[i].gameObject);
			}
		}
	}

	protected void RemoveItemsFromContainer()
	{
		if (container != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				container.RemoveChild(items[i].gameObject);
			}
		}
	}

	public IUIObject GetControl(ref POINTER_INFO ptr)
	{
		return this;
	}

	public bool RequestContainership(IUIContainer cont)
	{
		Transform parent = base.transform.parent;
		Transform transform = ((Component)cont).transform;
		while (parent != null)
		{
			if (parent == transform)
			{
				container = cont;
				return true;
			}
			if (parent.gameObject.GetComponent("IUIContainer") != null)
			{
				return false;
			}
			parent = parent.parent;
		}
		return false;
	}

	public bool GotFocus()
	{
		return false;
	}

	public void SetInputDelegate(EZInputDelegate del)
	{
		inputDelegate = del;
	}

	public void AddInputDelegate(EZInputDelegate del)
	{
		inputDelegate = (EZInputDelegate)Delegate.Combine(inputDelegate, del);
	}

	public void RemoveInputDelegate(EZInputDelegate del)
	{
		inputDelegate = (EZInputDelegate)Delegate.Remove(inputDelegate, del);
	}

	public void SetValueChangedDelegate(EZValueChangedDelegate del)
	{
		changeDelegate = del;
	}

	public void AddValueChangedDelegate(EZValueChangedDelegate del)
	{
		changeDelegate = (EZValueChangedDelegate)Delegate.Combine(changeDelegate, del);
	}

	public void RemoveValueChangedDelegate(EZValueChangedDelegate del)
	{
		changeDelegate = (EZValueChangedDelegate)Delegate.Remove(changeDelegate, del);
	}

	public void AddItemSnappedDelegate(ItemSnappedDelegate del)
	{
		itemSnappedDel = (ItemSnappedDelegate)Delegate.Combine(itemSnappedDel, del);
	}

	public void RemoveItemSnappedDelegate(ItemSnappedDelegate del)
	{
		itemSnappedDel = (ItemSnappedDelegate)Delegate.Remove(itemSnappedDel, del);
	}

	public void DragUpdatePosition(POINTER_INFO ptr)
	{
	}

	public void CancelDrag()
	{
	}

	public void OnEZDragDrop_Internal(EZDragDropParams parms)
	{
		if (dragDropDelegate != null)
		{
			dragDropDelegate(parms);
		}
	}

	public void AddDragDropDelegate(EZDragDropDelegate del)
	{
		dragDropDelegate = (EZDragDropDelegate)Delegate.Combine(dragDropDelegate, del);
	}

	public void RemoveDragDropDelegate(EZDragDropDelegate del)
	{
		dragDropDelegate = (EZDragDropDelegate)Delegate.Remove(dragDropDelegate, del);
	}

	public void SetDragDropDelegate(EZDragDropDelegate del)
	{
		dragDropDelegate = del;
	}

	public void SetDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public void AddDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public void RemoveDragDropInternalDelegate(EZDragDropHelper.DragDrop_InternalDelegate del)
	{
	}

	public EZDragDropHelper.DragDrop_InternalDelegate GetDragDropInternalDelegate()
	{
		return null;
	}

	private void OnDrawGizmosSelected()
	{
		SetupCameraAndSizes();
		Vector3 vector = base.transform.position - base.transform.TransformDirection(Vector3.right * viewableAreaActual.x * 0.5f * base.transform.lossyScale.x) + base.transform.TransformDirection(Vector3.up * viewableAreaActual.y * 0.5f * base.transform.lossyScale.y);
		Vector3 vector2 = base.transform.position - base.transform.TransformDirection(Vector3.right * viewableAreaActual.x * 0.5f * base.transform.lossyScale.x) - base.transform.TransformDirection(Vector3.up * viewableAreaActual.y * 0.5f * base.transform.lossyScale.y);
		Vector3 vector3 = base.transform.position + base.transform.TransformDirection(Vector3.right * viewableAreaActual.x * 0.5f * base.transform.lossyScale.x) - base.transform.TransformDirection(Vector3.up * viewableAreaActual.y * 0.5f * base.transform.lossyScale.y);
		Vector3 vector4 = base.transform.position + base.transform.TransformDirection(Vector3.right * viewableAreaActual.x * 0.5f * base.transform.lossyScale.x) + base.transform.TransformDirection(Vector3.up * viewableAreaActual.y * 0.5f * base.transform.lossyScale.y);
		Gizmos.color = new Color(1f, 0f, 0.5f, 1f);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector3, vector4);
		Gizmos.DrawLine(vector4, vector);
	}

	public static UIPageScrollList Create(string name, Vector3 pos)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.transform.position = pos;
		return (UIPageScrollList)gameObject.AddComponent(typeof(UIPageScrollList));
	}

	public static UIPageScrollList Create(string name, Vector3 pos, Quaternion rotation)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.transform.position = pos;
		gameObject.transform.rotation = rotation;
		return (UIPageScrollList)gameObject.AddComponent(typeof(UIPageScrollList));
	}

	public void DrawPreTransitionUI(int selState, IGUIScriptSelector gui)
	{
		scriptWithMethodToInvoke = gui.DrawScriptSelection(scriptWithMethodToInvoke, ref methodToInvokeOnSelect);
	}

	public void Init(int totalItens)
	{
		_totalItens = totalItens;
		if (!m_started)
		{
			Start();
		}
		if (_cachedObjects == null)
		{
			_outArea = viewableAreaActual.x;
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(_itemPrefab);
			IUIListObject iUIListObject = (IUIListObject)gameObject.GetComponent(typeof(IUIListObject));
			iUIListObject.FindOuterEdges();
			_itemSize = iUIListObject.BottomRightEdge.x - iUIListObject.TopLeftEdge.x;
			float num = _itemSize + itemSpacingActual;
			_itensAmountToFill = (int)(_outArea / num + 2f);
			_cachedObjects = new GameObject[_itensAmountToFill];
			_cachedObjects[0] = gameObject;
			for (int i = 0; i < _itensAmountToFill - 1; i++)
			{
				GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(_itemPrefab);
				_cachedObjects[i + 1] = gameObject2;
			}
		}
		for (int j = 0; j < _itensAmountToFill; j++)
		{
			GameObject gameObject3 = _cachedObjects[j];
			bool flag = j < _totalItens;
			MonoUtils.SetActive(gameObject3, flag);
			if (flag)
			{
				AddItem(gameObject3);
			}
		}
	}

	public void InsertItem(IUIListObject item, int position, string text, bool doEasing)
	{
		item.gameObject.layer = base.gameObject.layer;
		item.transform.parent = _moverTransform;
		item.transform.localPosition = Vector3.zero;
		newItems.Add(item);
	}

	protected void ScrollListTo_Internal(float pos)
	{
		if (_canScroll && !float.IsNaN(pos) && scrollPos != pos)
		{
			float num = ((direction != 0) ? 1f : (-1f));
			float num2 = num * Mathf.Clamp(amtOfPlay, 0f, amtOfPlay) * pos;
			_moverTransform.localPosition = Vector3.right * num2;
			scrollPos = pos;
		}
	}

	public void RefreshListItems()
	{
		if (items.Count > 0)
		{
			int num = _currentIndex;
			int num2 = 0;
			while (num < _currentIndex + _itensAmountToFill)
			{
				NotifyItem(true, num, items[num2]);
				num++;
				num2++;
			}
		}
	}

	public void OnEnable()
	{
	}

	protected void OnDisable()
	{
	}

	private void PositionPageItems()
	{
		IUIListObject iUIListObject = null;
		IUIListObject iUIListObject2 = null;
		float num = 0f;
		for (int i = 0; i < newItems.Count; i++)
		{
			iUIListObject = newItems[i];
			items.Add(iUIListObject);
			iUIListObject.FindOuterEdges();
			iUIListObject.UpdateCollider();
			float x = 0f;
			float y = 0f;
			if (orientation == ORIENTATION.HORIZONTAL)
			{
				if (i > 0)
				{
					iUIListObject2 = newItems[i - 1];
					x = ((direction != 0) ? (iUIListObject2.transform.localPosition.x - iUIListObject2.BottomRightEdge.x - itemSpacingActual + iUIListObject.TopLeftEdge.x) : (iUIListObject2.transform.localPosition.x + iUIListObject2.BottomRightEdge.x + itemSpacingActual - iUIListObject.TopLeftEdge.x));
				}
				else
				{
					x = ((direction != 0) ? (viewableAreaActual.x * 0.5f - iUIListObject.BottomRightEdge.x - ((!spacingAtEnds) ? 0f : itemSpacingActual) - extraEndSpacingActual) : (viewableAreaActual.x * -0.5f - iUIListObject.TopLeftEdge.x + ((!spacingAtEnds) ? 0f : itemSpacingActual) + extraEndSpacingActual));
				}
			}
			iUIListObject.transform.localPosition = new Vector3(x, y);
			NotifyItem(true, i, iUIListObject);
		}
		num = _itemSize * (float)_totalItens + itemSpacingActual * (float)(_totalItens - 1);
		UpdateContentExtents(num);
	}

	public void ScrollToItemIndex(int index, float scrollTime)
	{
		_indexToScroll = index;
		_scrollTime = scrollTime;
		_scrollToItem = true;
	}

	private void Scroll()
	{
		if (_canScroll && _scrollToItem)
		{
			float num = _itemSize * (float)_indexToScroll;
			num += (float)(_indexToScroll - 1) * itemSpacingActual;
			if (spacingAtEnds)
			{
				num += itemSpacingActual;
			}
			int num2 = (int)(viewableAreaActual.x / _itemSize);
			float num3 = (float)num2 * _itemSize;
			num3 += (float)(num2 - 1) * itemSpacingActual;
			float num4 = (viewableArea.x - num3) / 2f;
			num += itemSpacingActual - num4;
			if (direction == DIRECTION.TtoB_LtoR)
			{
				autoScrollPos = Mathf.Clamp01(num / amtOfPlay);
			}
			else
			{
				autoScrollPos = Mathf.Clamp01((0f - num) / amtOfPlay);
			}
			autoScrollStart = scrollPos;
			autoScrollDelta = autoScrollPos - scrollPos;
			autoScrollDuration = _scrollTime;
			autoScrollTime = 0f;
			autoScrolling = true;
			scrollDelta = 0f;
			isScrolling = false;
			_scrollToItem = false;
		}
	}

	private void NotifyItem(bool activate, int itemIndex, IUIListObject item)
	{
		if (activate)
		{
			if (EventFillData != null)
			{
				EventFillData(item, itemIndex);
			}
		}
		else if (EventFreeData != null)
		{
			EventFreeData(item, itemIndex);
		}
	}

	private IUIListObject TeleportItemToRight()
	{
		IUIListObject iUIListObject = items[0];
		int num = items.Count - 1;
		for (int i = 0; i < num; i++)
		{
			items[i] = items[i + 1];
		}
		items[num] = iUIListObject;
		IUIListObject iUIListObject2 = items[num - 1];
		float x = iUIListObject2.transform.localPosition.x + iUIListObject2.BottomRightEdge.x + itemSpacingActual - iUIListObject.TopLeftEdge.x;
		Transform component = iUIListObject.gameObject.GetComponent<Transform>();
		component.localPosition = new Vector3(x, component.localPosition.y);
		_currentIndex = Mathf.Clamp(++_currentIndex, 0, _totalItens - 1);
		return iUIListObject;
	}

	private IUIListObject TeleportItemToLeft()
	{
		IUIListObject iUIListObject = items[items.Count - 1];
		int num = 0;
		for (int num2 = items.Count - 1; num2 > num; num2--)
		{
			items[num2] = items[num2 - 1];
		}
		items[num] = iUIListObject;
		IUIListObject iUIListObject2 = items[num + 1];
		float x = iUIListObject2.transform.localPosition.x + iUIListObject2.TopLeftEdge.x - itemSpacingActual - iUIListObject.BottomRightEdge.x;
		Transform component = iUIListObject.gameObject.GetComponent<Transform>();
		component.localPosition = new Vector3(x, component.localPosition.y);
		_currentIndex = Mathf.Clamp(--_currentIndex, 0, _totalItens - 1);
		return iUIListObject;
	}

	private void TeleportItem()
	{
		if (items.Count <= 0)
		{
			return;
		}
		float num = _outArea * -0.5f + _transform.position.x;
		float num2 = _outArea * 0.5f + _transform.position.x;
		IUIListObject iUIListObject = items[0];
		IUIListObject iUIListObject2 = items[items.Count - 1];
		float num3 = iUIListObject.TopLeftEdge.x + iUIListObject.transform.position.x;
		float num4 = iUIListObject2.BottomRightEdge.x + iUIListObject2.transform.position.x;
		if (num4 < num2)
		{
			if (_currentIndex < _totalItens - _itensAmountToFill)
			{
				IUIListObject item = TeleportItemToRight();
				NotifyItem(false, _currentIndex - 1, item);
				NotifyItem(true, _currentIndex + _itensAmountToFill - 1, item);
			}
		}
		else if (num3 > num && _currentIndex > 0)
		{
			IUIListObject item2 = TeleportItemToLeft();
			NotifyItem(false, _currentIndex + _itensAmountToFill, item2);
			NotifyItem(true, _currentIndex, item2);
		}
	}

	public void LateUpdate()
	{
		if (newItems.Count > 0)
		{
			PositionPageItems();
			newItems.Clear();
		}
		float deltaTime = Time.deltaTime;
		inertiaLerpTime += deltaTime;
		if (!noTouch && inertiaLerpTime >= inertiaLerpInterval)
		{
			scrollInertia = Mathf.Lerp(scrollInertia, scrollDelta, lowPassFilterFactor);
			scrollDelta = 0f;
			inertiaLerpTime %= inertiaLerpInterval;
		}
		if (isScrolling && noTouch && !autoScrolling)
		{
			scrollDelta -= scrollDelta * scrollDecelCoef;
			float num = scrollPos;
			if (num < 0f)
			{
				num -= num * 1f * (deltaTime / 0.166f);
				scrollDelta *= Mathf.Clamp01(1f + num / scrollMax);
			}
			else if (scrollPos > 1f)
			{
				num -= (num - 1f) * 1f * (deltaTime / 0.166f);
				scrollDelta *= Mathf.Clamp01(1f - (num - 1f) / scrollMax);
			}
			if (Mathf.Abs(scrollDelta) < 0.0001f)
			{
				scrollDelta = 0f;
				if (num > -0.0001f && num < 0.0001f)
				{
					num = Mathf.Clamp01(num);
				}
			}
			ScrollListTo_Internal(num + scrollDelta);
			if (scrollPos >= 0f && scrollPos <= 1.001f && scrollDelta == 0f)
			{
				isScrolling = false;
			}
		}
		else if (autoScrolling)
		{
			autoScrollTime += deltaTime;
			float num2 = scrollPos;
			if (autoScrollTime >= autoScrollDuration)
			{
				autoScrolling = false;
				num2 = autoScrollPos;
			}
			else
			{
				num2 = autoScrollInterpolator(autoScrollTime, autoScrollStart, autoScrollDelta, autoScrollDuration);
			}
			ScrollListTo_Internal(num2);
		}
		if (autoScrolling)
		{
			if (items.Count > 0)
			{
				for (int i = 0; i < items.Count; i++)
				{
					TeleportItem();
				}
			}
		}
		else
		{
			TeleportItem();
		}
		Scroll();
	}

	/*virtual GameObject IUIObject.get_gameObject()
	{
		return base.gameObject;
	}

	virtual Transform IUIObject.get_transform()
	{
		return base.transform;
	}

	virtual string IUIObject.get_name()
	{
		return base.name;
	}*/
}
