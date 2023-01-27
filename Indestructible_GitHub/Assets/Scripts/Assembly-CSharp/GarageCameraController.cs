using System;
using UnityEngine;

public class GarageCameraController : MonoBehaviour
{
	public UIButton ControlButton;

	public Transform VehicleTransform;

	private Transform _transform;

	private float _dragSpeed;

	private float _dragSpeedTarget;

	private bool _isDragging;

	public Vector2 DragMultiplier = new Vector2(0.5f, 0.5f);

	public Vector2 DragViewLimits = new Vector2(15f, 70f);

	public float DragSpeedLimit = 300f;

	public float DragSpeedMoveDamping = 0.4f;

	public float DragSpeedStayDamping = 25f;

	private float _rotateSpeed;

	public float AutoRotateSpeed = 5f;

	public float AutoRotateAcceleration = 2f;

	private float _distance;

	private float _targetDistance;

	private float _fromDistance;

	private float _distanceTime;

	private float _distanceDuration;

	public float DistanceDefault = 7f;

	public float DistanceTime = 2f;

	private float LimitDragSpeed(float speed)
	{
		float num = Mathf.Abs(speed);
		if (num > DragSpeedLimit)
		{
			float num2 = Mathf.Sign(speed);
			return num2 * DragSpeedLimit;
		}
		return speed;
	}

	private void SetDistance(float distance)
	{
		Vector3 vector = _transform.position - VehicleTransform.position;
		vector.Normalize();
		_transform.position = vector * distance;
		_transform.position += VehicleTransform.position;
	}

	private void OnInput(ref POINTER_INFO ptr)
	{
		switch (ptr.evt)
		{
		case POINTER_INFO.INPUT_EVENT.DRAG:
		{
			float num = ptr.inputDelta.x * DragMultiplier.x;
			float y = ptr.inputDelta.y * DragMultiplier.y;
			_dragSpeedTarget = num / Time.deltaTime;
			_dragSpeedTarget = LimitDragSpeed(_dragSpeedTarget);
			_dragSpeed += (_dragSpeedTarget - _dragSpeed) * 0.25f;
			_dragSpeed = LimitDragSpeed(_dragSpeed);
			num = _dragSpeed * Time.deltaTime;
			Rotate(num, y);
			break;
		}
		case POINTER_INFO.INPUT_EVENT.NO_CHANGE:
			if (ptr.active)
			{
				_dragSpeed -= _dragSpeed * Time.deltaTime * DragSpeedStayDamping;
			}
			break;
		case POINTER_INFO.INPUT_EVENT.PRESS:
			_isDragging = true;
			_rotateSpeed = 0f;
			_dragSpeed = 0f;
			break;
		case POINTER_INFO.INPUT_EVENT.RELEASE:
		case POINTER_INFO.INPUT_EVENT.TAP:
		case POINTER_INFO.INPUT_EVENT.RELEASE_OFF:
			if (_isDragging)
			{
				_isDragging = false;
				_rotateSpeed = 0f;
			}
			break;
		case POINTER_INFO.INPUT_EVENT.MOVE:
		case POINTER_INFO.INPUT_EVENT.MOVE_OFF:
			break;
		}
	}

	private void Awake()
	{
		_transform = GetComponent<Transform>();
		ControlButton.AddInputDelegate(OnInput);
		_targetDistance = DistanceDefault;
		_distance = DistanceDefault;
		SetDistance(DistanceDefault);
	}

	private void Start()
	{
	}

	private void Rotate(float x, float y)
	{
		if (x != 0f)
		{
			_transform.RotateAround(VehicleTransform.position, Vector3.up, x);
		}
		if (y != 0f)
		{
			Vector3 eulerAngles = _transform.rotation.eulerAngles;
			if (eulerAngles.x - y > DragViewLimits.y)
			{
				y = eulerAngles.x - DragViewLimits.y;
			}
			else if (eulerAngles.x - y < DragViewLimits.x)
			{
				y = eulerAngles.x - DragViewLimits.x;
			}
			if (y != 0f)
			{
				_transform.RotateAround(VehicleTransform.position, _transform.right, 0f - y);
			}
		}
	}

	private void Update()
	{
		if (_isDragging)
		{
			return;
		}
		if (_dragSpeed != 0f)
		{
			float dragSpeed = _dragSpeed;
			float num = Mathf.Abs(_dragSpeed);
			_dragSpeed -= _dragSpeed * DragSpeedMoveDamping * Time.deltaTime;
			if (dragSpeed * _dragSpeed <= 0f || num < 0.2f)
			{
				_dragSpeed = 0f;
			}
			else
			{
				float x = _dragSpeed * Time.deltaTime;
				Rotate(x, 0f);
			}
		}
		else
		{
			_rotateSpeed += AutoRotateAcceleration * Time.deltaTime;
			_rotateSpeed = Mathf.Clamp(_rotateSpeed, 0f, AutoRotateSpeed);
			float num2 = _rotateSpeed * Time.deltaTime;
			Rotate(0f - num2, 0f);
		}
		if (_targetDistance != _distance)
		{
			_distanceTime += Time.deltaTime;
			if (_distanceTime > _distanceDuration)
			{
				_distanceTime = _distanceDuration;
				_distance = _targetDistance;
			}
			else
			{
				float num3 = _distanceTime / _distanceDuration;
				float num4 = _targetDistance - _fromDistance;
				float num5 = (1f - Mathf.Cos((float)Math.PI * num3)) / 2f;
				_distance = num4 * num5 + _fromDistance;
			}
			SetDistance(_distance);
		}
	}

	public void SetTargetDistance(float distance)
	{
		_fromDistance = _distance;
		_targetDistance = distance;
		_distanceDuration = DistanceTime;
		_distanceTime = 0f;
	}
}
