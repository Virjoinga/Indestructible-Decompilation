using UnityEngine;

public class StickScriptButton : MonoBehaviour
{
	private bool m_pointerActive;

	private bool m_isPressed;

	private Vector3 m_targetBasePosition;

	private Transform m_targetTransform;

	public GameObject m_targetObject;

	public bool m_dynamicCenter;

	public bool IsPressed()
	{
		return m_isPressed;
	}

	private void OnPointerActive(ref POINTER_INFO pointer)
	{
		Vector3 vector = default(Vector3);
		vector.x = pointer.ray.origin.x;
		vector.y = pointer.ray.origin.y;
		vector.z = m_targetBasePosition.z;
	}

	private void SetFireButtonPressed(bool pressed)
	{
		UIButton component = m_targetObject.GetComponent<UIButton>();
		if (pressed)
		{
			component.SetControlState(UIButton.CONTROL_STATE.ACTIVE);
		}
		else
		{
			component.SetControlState(UIButton.CONTROL_STATE.NORMAL);
		}
	}

	private void InputDelegate(ref POINTER_INFO pointer)
	{
		switch (pointer.evt)
		{
		case POINTER_INFO.INPUT_EVENT.PRESS:
			if (m_dynamicCenter)
			{
				Vector3 position = m_targetTransform.position;
				position.x = pointer.ray.origin.x;
				position.y = pointer.ray.origin.y;
				m_targetBasePosition = position;
				m_targetTransform.position = position;
				float z = m_targetTransform.position.z;
				m_targetTransform.position = new Vector3(position.x, position.y, z);
				m_targetTransform.gameObject.SetActiveRecursively(true);
			}
			OnPointerActive(ref pointer);
			SetFireButtonPressed(true);
			m_pointerActive = true;
			m_isPressed = true;
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			OnPointerActive(ref pointer);
			m_pointerActive = true;
			m_isPressed = true;
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			m_targetTransform.position = m_targetBasePosition;
			if (m_dynamicCenter)
			{
				m_targetTransform.gameObject.SetActiveRecursively(false);
			}
			SetFireButtonPressed(false);
			m_pointerActive = false;
			m_isPressed = false;
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE:
			break;
		}
	}

	private void Awake()
	{
		UIButton component = GetComponent<UIButton>();
		component.SetInputDelegate(InputDelegate);
		m_targetTransform = m_targetObject.GetComponent<Transform>();
		m_targetBasePosition = m_targetTransform.position;
		m_dynamicCenter = false;
		if (m_dynamicCenter)
		{
			m_targetTransform.gameObject.SetActiveRecursively(false);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		bool flag = false;
		if (Input.GetKey(KeyCode.RightControl))
		{
			SetFireButtonPressed(true);
			m_isPressed = true;
			flag = true;
		}
		if (!flag && !m_pointerActive)
		{
			SetFireButtonPressed(false);
			m_isPressed = false;
		}
		if (m_dynamicCenter)
		{
			m_targetTransform.gameObject.SetActiveRecursively(m_isPressed);
		}
	}
}
