using UnityEngine;

public class StickScript : MonoBehaviour
{
	public enum KeyboardType
	{
		None = 0,
		WASD = 1,
		Arrows = 2
	}

	private Vector2 m_vector = Vector2.zero;

	private Vector2 m_currentVector = Vector2.zero;

	private bool m_pointerActive;

	private Vector3 m_targetBasePosition;

	private Transform m_targetTransform;

	private Transform m_backgroundTransform;

	public GameObject m_targetObject;

	public GameObject m_background;

	public float m_moveRadius = 50f;

	public bool m_dynamicCenter;

	public bool m_rotateBackground;

	public KeyboardType m_keyboardType;

	public Vector2 GetVector()
	{
		return m_currentVector;
	}

	private void OnPointerActive(ref POINTER_INFO pointer)
	{
		Vector3 vector = default(Vector3);
		vector.x = pointer.ray.origin.x;
		vector.y = pointer.ray.origin.y;
		vector.z = m_targetBasePosition.z;
		m_vector.x = vector.x - m_targetBasePosition.x;
		m_vector.y = vector.y - m_targetBasePosition.y;
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
				Transform backgroundTransform = m_backgroundTransform;
				backgroundTransform.position = new Vector3(position.x, position.y, backgroundTransform.position.z);
				base.gameObject.SetActiveRecursively(true);
			}
			OnPointerActive(ref pointer);
			m_pointerActive = true;
			break;
		case POINTER_INFO.INPUT_EVENT.DRAG:
			OnPointerActive(ref pointer);
			m_pointerActive = true;
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			m_targetTransform.position = m_targetBasePosition;
			m_vector = Vector2.zero;
			if (m_dynamicCenter)
			{
				base.gameObject.SetActiveRecursively(false);
				base.gameObject.active = true;
			}
			if (m_rotateBackground)
			{
				m_backgroundTransform.localRotation = Quaternion.identity;
			}
			m_pointerActive = false;
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE:
			break;
		}
	}

	protected virtual void Awake()
	{
		UIButton component = GetComponent<UIButton>();
		component.SetInputDelegate(InputDelegate);
		m_backgroundTransform = m_background.GetComponent<Transform>();
		m_targetTransform = m_targetObject.GetComponent<Transform>();
		m_dynamicCenter = false;
		if (m_dynamicCenter)
		{
			base.gameObject.SetActiveRecursively(false);
			base.gameObject.active = true;
		}
	}

	protected virtual void Start()
	{
		m_targetBasePosition = m_targetTransform.position;
	}

	private void Update()
	{
		bool flag = false;
		if (m_keyboardType == KeyboardType.WASD)
		{
			if (Input.GetKey(KeyCode.A))
			{
				m_vector.x += 0f - m_moveRadius;
				flag = true;
			}
			if (Input.GetKey(KeyCode.D))
			{
				m_vector.x += m_moveRadius;
				flag = true;
			}
			if (Input.GetKey(KeyCode.W))
			{
				m_vector.y += m_moveRadius;
				flag = true;
			}
			if (Input.GetKey(KeyCode.S))
			{
				m_vector.y += 0f - m_moveRadius;
				flag = true;
			}
		}
		else if (m_keyboardType == KeyboardType.Arrows)
		{
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				m_vector.x += 0f - m_moveRadius;
				flag = true;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				m_vector.x += m_moveRadius;
				flag = true;
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				m_vector.y += m_moveRadius;
				flag = true;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				m_vector.y += 0f - m_moveRadius;
				flag = true;
			}
		}
		if (!flag && !m_pointerActive)
		{
			m_vector = Vector2.zero;
			if (m_rotateBackground)
			{
				m_backgroundTransform.localRotation = Quaternion.identity;
			}
		}
		if (!(m_vector != m_currentVector))
		{
			return;
		}
		float magnitude = m_vector.magnitude;
		m_vector.Normalize();
		if (magnitude > m_moveRadius)
		{
			m_currentVector = m_vector;
			float num = 1f;
		}
		else
		{
			float num = magnitude / m_moveRadius;
			m_currentVector.x = m_vector.x * num;
			m_currentVector.y = m_vector.y * num;
		}
		m_vector = m_currentVector;
		if (m_dynamicCenter)
		{
			bool activeRecursively = m_vector.x != 0f || m_vector.y != 0f;
			base.gameObject.SetActiveRecursively(activeRecursively);
			base.gameObject.active = true;
		}
		if (m_rotateBackground)
		{
			if (m_vector == Vector2.zero)
			{
				m_backgroundTransform.localRotation = Quaternion.identity;
			}
			else
			{
				float z = Mathf.Atan2(m_vector.y, m_vector.x) * 57.29578f - 75f;
				m_backgroundTransform.localRotation = Quaternion.Euler(0f, 0f, z);
			}
		}
		Vector3 position = default(Vector3);
		position.x = m_targetBasePosition.x + m_vector.x * m_moveRadius;
		position.y = m_targetBasePosition.y + m_vector.y * m_moveRadius;
		position.z = m_targetBasePosition.z;
		m_targetTransform.position = position;
	}
}
